using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;
using System.Collections.Specialized;
using CryptSharp;

namespace ParkPlaces.IO
{
    public class Sql
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }
        public string Port { get; set; }

        public EventHandler<UpdateProcessChangedArgs> OnUpdateChangedEventHandler;

        public Sql()
        {
            LoadDbCredientals();
        }

        public Sql(string server="", string database="", string user="", string password="", string port="3306")
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
            Port = port;
        }

        private void LoadDbCredientals()
        {
            var dbSect = ConfigurationManager.GetSection("DBConnection") as NameValueCollection;
            if (dbSect == null) return;
            Server = dbSect["server"].ToString();
            Database = dbSect["database"].ToString();
            User = dbSect["user"].ToString();
            Password = dbSect["password"].ToString();
            Port = dbSect["port"];
        }


        public MySqlConnection GetConnection()
        {
            var mySqlConnection = new MySqlConnection(
                $@"SERVER={Server};PORT={Port};DATABASE={Database};UID={User};PASSWORD={Password}");

            try
            {
                mySqlConnection.Open();
            }
            catch (MySqlException e)
            {
                Debug.Write("Mysql error: " + e.Message);
                return null;
            }
            return mySqlConnection;
        }

        public bool AuthenticateUser(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
                return false;

            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE username = @username ")
            { Connection = GetConnection() })
            {
                cmd.Parameters.Add("@username", MySqlDbType.String);
                cmd.Parameters[0].Value = username;

                using(var reader = cmd.ExecuteReader())
                {
                    reader.Read();

                    if (reader.HasRows)
                        return Crypter.CheckPassword(password, reader["password"].ToString());
                    else
                        return false;
                }
            }
        }

        public void ExportToMySql(Dto2Object dto)
        {
            // Flush table
            SimpleExecQuery("DELETE FROM geometry");
            SimpleExecQuery("DELETE FROM zones");
            SimpleExecQuery("DELETE FROM cities");
            SimpleExecQuery("ALTER TABLE cities AUTO_INCREMENT = 0");
            SimpleExecQuery("ALTER TABLE zones AUTO_INCREMENT = 0");
            SimpleExecQuery("ALTER TABLE geometry AUTO_INCREMENT = 0");
               
     

            // Insert data
            Dictionary<string, int> cityIds = new Dictionary<string, int>();

            var cities = dto.Zones.DistinctBy(zone => zone.Telepules);

            using (var cmd = new MySqlCommand("INSERT INTO cities (city) VALUES (@city)") { Connection = GetConnection() })
            {
                var cityId = 1;

                cmd.Parameters.Add("@city", MySqlDbType.String);

                foreach (var city in cities)
                {
                    cmd.Parameters[0].Value = city.Telepules;
                    cmd.ExecuteNonQuery();
                        
                    cityIds.Add(city.Telepules, cityId);
                    cityId++;
                }
            }

            using (var cmd = new MySqlCommand(
                "INSERT INTO zones (cityid, color, fee, service_na, description, timetable) VALUES" +
                "(@cityid, @color, @fee, @service_na, @description, @timetable)") { Connection = GetConnection()})
            {

                var zoneId = 1;
                cmd.Parameters.AddRange(new[]
                {
                    new MySqlParameter("@cityid", MySqlDbType.String), new MySqlParameter("@color", MySqlDbType.String),
                    new MySqlParameter("@fee", MySqlDbType.String), new MySqlParameter("@service_na", MySqlDbType.String),
                    new MySqlParameter("@description", MySqlDbType.String), new MySqlParameter("@timetable", MySqlDbType.String)
                });


                foreach (var zone in dto.Zones)
                {
                    cmd.Parameters[0].Value = cityIds[zone.Telepules];
                    cmd.Parameters[1].Value = zone.Color;
                    cmd.Parameters[2].Value = zone.Fee;
                    cmd.Parameters[3].Value = zone.ServiceNa;
                    cmd.Parameters[4].Value = zone.Description;
                    cmd.Parameters[5].Value = zone.Timetable;

                    cmd.ExecuteNonQuery();

                    foreach (var geometry in zone.Geometry)
                    {
                        using (var cmd1 =
                            new MySqlCommand(
                                "INSERT INTO geometry (zoneid, lat, lng) VALUES (@zoneid, @lat, @lng)") { Connection = GetConnection() })
                        {
                            cmd1.Parameters.AddRange(new[]
                            {
                                new MySqlParameter("@zoneid", MySqlDbType.String),
                                new MySqlParameter("@lat", MySqlDbType.Double),
                                new MySqlParameter("@lng", MySqlDbType.Double)
                            });

                            cmd1.Parameters[0].Value = zoneId;
                            cmd1.Parameters[1].Value = geometry.Lat;
                            cmd1.Parameters[2].Value = geometry.Lng;
                            cmd1.ExecuteNonQuery();
                            cmd1.Connection.Close();
                        }
                    }
                    zoneId++;
                }
            }
        }

        /// <summary>
        /// Executes a query 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>-1 if the execution fails. Otherwise the number of affected rows </returns>
        public int SimpleExecQuery(string query)
        {
            try
            {
                return new MySqlCommand(query) { Connection =  GetConnection()}
                    .ExecuteNonQuery();
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }

        public async Task<Dto2Object> LoadFromDb()
        {
            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };

            
            var zoneCount = await GetZoneCount();
            var cProcess = new UpdateProcessChangedArgs(zoneCount);
            var actualCount = 0;

            using (var cmd = new MySqlCommand("SELECT * FROM zones")
            { Connection = GetConnection() })
            {
                var geometryConnection = GetConnection();

                var rd = await cmd.ExecuteReaderAsync();
                while(await rd.ReadAsync())
                {
                    var zone = new PolyZone()
                    {
                        Geometry = new List<Geometry>(),
                        Id = rd["id"].ToString(),
                        Color = rd["color"].ToString(),
                        Description = rd["description"].ToString(),
                        ServiceNa = rd["service_na"].ToString(),
                        Timetable = rd["timetable"].ToString(),
                        Fee = long.Parse(rd["fee"].ToString()),
                        Zoneid = "TODO: Fix this"
                    };

                    using (MySqlCommand cmd1 = new MySqlCommand($"SELECT * FROM geometry WHERE zoneid = {rd["id"]}") { Connection = geometryConnection })
                    {
                        var rd1 = await cmd1.ExecuteReaderAsync();
                        while(await rd1.ReadAsync())
                        {
                            zone.Geometry.Add(new Geometry() { Lat = (double)rd1["lat"], Lng = (double)rd1["lng"] });
                        }
                        rd1.Close();

                        actualCount++;

                        cProcess.UpdateChunks(zoneCount - actualCount);
                        OnUpdateChangedEventHandler?.Invoke(this, cProcess);
                    }

                    dto.Zones.Add(zone);
                }
                geometryConnection.Close();
                rd.Close();
            }
            return dto;
        }

        public async Task<int> GetZoneCount()
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM zones") { Connection = GetConnection() })
            {
                var count = await cmd.ExecuteScalarAsync();
                return int.Parse(count.ToString());
            }
        }
    }
}
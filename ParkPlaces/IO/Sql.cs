using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using GMap.NET;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;

namespace ParkPlaces.IO
{
    public class Sql
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }

        public Sql()
        {

        }

        public Sql(string server, string database, string user, string password)
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
        }

        public MySqlConnection GetConnection()
        {
            var mySqlConnection = new MySqlConnection(
                $@"SERVER={Server};DATABASE={Database};UID={User};PASSWORD={Password}");

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


        public void ExportToMySql(Dto2Object dto)
        {
            // Flush table
            simpleExecQuery("DELETE FROM geometry");
            simpleExecQuery("DELETE FROM zones");
            simpleExecQuery("DELETE FROM cities");
            simpleExecQuery("ALTER TABLE cities AUTO_INCREMENT = 0");
            simpleExecQuery("ALTER TABLE zones AUTO_INCREMENT = 0");
            simpleExecQuery("ALTER TABLE geometry AUTO_INCREMENT = 0");
               
            //return;

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
        public int simpleExecQuery(string query)
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

            using (var cmd = new MySqlCommand("SELECT * FROM zones")
            { Connection = GetConnection() })
            {
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

                    using (MySqlCommand cmd1 = new MySqlCommand($"SELECT * FROM geometry WHERE zoneid = {rd["id"]}") { Connection = GetConnection() })
                    {
                        
                        var rd1 = await cmd1.ExecuteReaderAsync();
                        while(await rd1.ReadAsync())
                        {
                            zone.Geometry.Add(new Geometry() { Lat = (double)rd1["lat"], Lng = (double)rd1["lng"] });
                        }
                        rd1.Close();
                    }

                    dto.Zones.Add(zone);
                }
                rd.Close();
            }
            return dto;
        }
    }
}
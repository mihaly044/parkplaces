using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;

namespace ParkPlaces.IO
{
    public class Sql
    {
        private static MySqlConnection _mysqlConnection;

        public static string Server { get; set; }
        public static string Database { get; set; }
        public static string User { get; set; }
        public static string Password { get; set; }

        public static bool Connect()
        {
            if (_mysqlConnection?.State == System.Data.ConnectionState.Open)
            {
                return true;
            }

            _mysqlConnection = new MySqlConnection(
                $@"SERVER={Server};DATABASE={Database};UID={User};PASSWORD={Password}");

            try
            {
                _mysqlConnection.Open();
            }
            catch (MySqlException e)
            {
                Debug.Write("Mysql error: " + e.Message);
                return false;
            }
            return true;
        }

        public static void Close()
        {
            _mysqlConnection?.Close();
        }

        public static void ExportToMySql(Dto2Object dto)
        {
            if (!Connect()) return;

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

            using (var cmd = new MySqlCommand("INSERT INTO cities (city) VALUES (@city)") { Connection = _mysqlConnection })
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
                "(@cityid, @color, @fee, @service_na, @description, @timetable)") { Connection = _mysqlConnection})
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
                                "INSERT INTO geometry (zoneid, lat, lng) VALUES (@zoneid, @lat, @lng)") { Connection = _mysqlConnection })
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

            Close();
        }

        /// <summary>
        /// Executes a query 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>-1 if the execution fails. Otherwise the number of affected rows </returns>
        public static int simpleExecQuery(string query)
        {
            if (_mysqlConnection is null)
                return -1;

            try
            {
                return new MySqlCommand(query) { Connection = _mysqlConnection }
                    .ExecuteNonQuery();
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
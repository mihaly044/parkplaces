using System;
using System.Collections.Generic;
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
        private static MySqlConnection mysqlConnection;

        public static string Server { get; set; }
        public static string Database { get; set; }
        public static string User { get; set; }
        public static string Password { private get; set; }

        private static string _connectionString;

        public static bool Connect()
        {
            if(mysqlConnection?.State == System.Data.ConnectionState.Open)
            {
                return true;
            }

            mysqlConnection = new MySqlConnection(string.Format(@"SERVER={0};DATABASE={1};
UID={2};PASSWORD={3}", Server, Database, User, Password));

            try
            {
                mysqlConnection.Open();
            } catch (MySqlException e)
            {
                Debug.Write("Mysql error: " + e.Message);
                return false;
            }
            return true;
        }

        public static void ExportToMySql(Dto2Object dto)
        {
            if (Connect())
            {
                var cities = dto.Zones.DistinctBy( zone => zone.Telepules);

                var cmd = new MySqlCommand("INSERT INTO cities (city) VALUES (@city)");
                cmd.Parameters.Add("@city", MySqlDbType.String);
                cmd.Connection = mysqlConnection;

                foreach(var city in cities)
                {
                    cmd.Parameters[0].Value = city.Telepules;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}

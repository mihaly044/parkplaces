using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PPNetLib.Contracts;
using PPNetLib.Prototypes;
using PPServer.LocalPrototypes;

namespace PPServer.Database
{
    public partial class Sql
    {
        /// <summary>
        /// Calculate the count of zones
        /// </summary>
        /// <returns>The count of rows in the zones table</returns>
        public int GetZoneCount()
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM zones") { Connection = GetConnection() })
            {
                var count = cmd.ExecuteScalar();
                return int.Parse(count.ToString());
            }
        }

        /// <summary>
        /// Load polygon data from the database
        /// </summary>
        /// <returns>Data transfer object that holds the data</returns>
        public void LoadZones(Func<PolyZone, bool> Callback)
        {
            var strCmd = "SELECT * FROM zones INNER JOIN cities ON cities.id = zones.cityid";

            using (var cmd = new MySqlCommand(strCmd)
            { Connection = GetConnection() })
            {
                var geometryConnection = GetConnection();

                var rd = cmd.ExecuteReader();
                while (rd.Read())
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
                        Zoneid = rd["common_name"].ToString(),
                        Telepules = rd["city"].ToString()
                    };

                    using (MySqlCommand cmd1 = new MySqlCommand($"SELECT * FROM geometry WHERE zoneid = {rd["id"]}") { Connection = geometryConnection })
                    {
                        var rd1 = cmd1.ExecuteReader();
                        while (rd1.Read())
                        {
                            zone.Geometry.Add(new Geometry((int)rd1["id"]) { Lat = (double)rd1["lat"], Lng = (double)rd1["lng"], IsModified = false });
                        }
                        rd1.Close();
                    }

                    Callback(zone);
                }
                geometryConnection.Close();
                rd.Close();
            }
        }

    }
}

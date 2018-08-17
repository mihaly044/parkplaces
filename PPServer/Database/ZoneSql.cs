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
        public void LoadZones(Func<PolyZone, bool> callback)
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

                    callback(zone);
                }
                geometryConnection.Close();
                rd.Close();
            }
        }

        /// <summary>
        /// Insert a zone into the database
        /// </summary>
        /// <param name="zone">The zone to be inserted</param>
        public async Task<int> InsertZone(PolyZone zone)
        {
            using (var cmd = new MySqlCommand(
                    "INSERT INTO zones (cityid, color, fee, service_na, description, timetable, common_name) VALUES" +
                    "(@cityid, @color, @fee, @service_na, @description, @timetable, @common_name)")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddRange(new[]
                {
                    new MySqlParameter("@cityid", MySqlDbType.String), new MySqlParameter("@color", MySqlDbType.String),
                    new MySqlParameter("@fee", MySqlDbType.String), new MySqlParameter("@service_na", MySqlDbType.String),
                    new MySqlParameter("@description", MySqlDbType.String), new MySqlParameter("@timetable", MySqlDbType.String),
                    new MySqlParameter("@common_name", MySqlDbType.String)
                });

                var city = City.FromString(zone.Telepules);
                cmd.Parameters[0].Value = !IsDuplicateCity(city) ? InsertCity(city) : GetCityId(city);
                cmd.Parameters[1].Value = zone.Color;
                cmd.Parameters[2].Value = zone.Fee;
                cmd.Parameters[3].Value = zone.ServiceNa;
                cmd.Parameters[4].Value = zone.Description;
                cmd.Parameters[5].Value = zone.Timetable;
                cmd.Parameters[6].Value = zone.Zoneid;

                await cmd.ExecuteNonQueryAsync();
                var zoneId = cmd.LastInsertedId;

                foreach (var geometry in zone.Geometry)
                {
                    using (var cmd1 =
                        new MySqlCommand(
                                "INSERT INTO geometry (zoneid, lat, lng) VALUES (@zoneid, @lat, @lng)")
                        { Connection = GetConnection() })
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
                        await cmd1.ExecuteNonQueryAsync();
                        geometry.Id = (int)cmd1.LastInsertedId;
                        cmd1.Connection.Close();
                    }
                }

                return (int)zoneId;
            }
        }
    }
}

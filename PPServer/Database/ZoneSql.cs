using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
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
            const string strCmd = "SELECT * FROM zones INNER JOIN cities ON cities.id = zones.cityid";

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

                    using (var cmd1 = new MySqlCommand($"SELECT * FROM geometry WHERE zoneid = {rd["id"]} ORDER BY polyindex") { Connection = geometryConnection })
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
        /// <param name="geometryCallback">Used to return new point Ids</param>
        public async Task<int> InsertZone(PolyZone zone, Func<int, bool> geometryCallback)
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

                var polyIndex = 0;
                var lastId = 0L;
                foreach (var geometry in zone.Geometry)
                {
                    if (lastId != zoneId)
                    {
                        polyIndex = 0;
                        lastId = zoneId;
                    }

                    using (var cmd1 =
                        new MySqlCommand(
                                "INSERT INTO geometry (zoneid, lat, lng, polyindex) VALUES (@zoneid, @lat, @lng, @polyIndex)")
                        { Connection = GetConnection() })
                    {
                        cmd1.Parameters.AddRange(new[]
                        {
                            new MySqlParameter("@zoneid", MySqlDbType.String),
                            new MySqlParameter("@lat", MySqlDbType.Double),
                            new MySqlParameter("@lng", MySqlDbType.Double),
                            new MySqlParameter("@polyIndex", polyIndex)
                        });

                        cmd1.Parameters[0].Value = zoneId;
                        cmd1.Parameters[1].Value = geometry.Lat;
                        cmd1.Parameters[2].Value = geometry.Lng;
                        await cmd1.ExecuteNonQueryAsync();

                        // Send back the inserted points' IDs
                        geometryCallback((int) cmd1.LastInsertedId);

                        cmd1.Connection.Close();
                    }
                    polyIndex++;
                }
                cmd.Connection.Close();

                return (int)zoneId;
            }
        }

        /// <summary>
        /// Remove a zone from the database
        /// </summary>
        /// <param name="zoneId">The zone to be removed</param>
        public async void RemoveZone(int zoneId)
        {
            using (var cmd = new MySqlCommand("DELETE FROM zones WHERE id = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", zoneId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }

            using (var cmd = new MySqlCommand("DELETE FROM geometry WHERE zoneid = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", zoneId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }
        }

        /// <summary>
        ///  Update a zone in the database
        /// </summary>
        /// <param name="zone"></param>
        public async void UpdateZoneInfo(PolyZone zone)
        {
            using (var cmd = new MySqlCommand(
                    @"UPDATE zones SET cityid = @cityid, color = @color, fee = @fee, timetable = @timetable, common_name = @common_name, service_na = @service_na WHERE id = @id")
                { Connection = GetConnection() })
            {

                cmd.Parameters.AddRange(new[]
                {
                    new MySqlParameter("@cityid", MySqlDbType.String),
                    new MySqlParameter("@color", MySqlDbType.String),
                    new MySqlParameter("@fee", MySqlDbType.String),
                    new MySqlParameter("@service_na", MySqlDbType.String),
                    new MySqlParameter("@description", MySqlDbType.String),
                    new MySqlParameter("@timetable", MySqlDbType.String),
                    new MySqlParameter("@common_name", MySqlDbType.String),
                    new MySqlParameter("@id", MySqlDbType.String)
                });


                var city = City.FromString(zone.Telepules);
                cmd.Parameters[0].Value = !IsDuplicateCity(city) ? InsertCity(city) : GetCityId(city);

                cmd.Parameters[1].Value = zone.Color;
                cmd.Parameters[2].Value = zone.Fee;
                cmd.Parameters[3].Value = zone.ServiceNa;
                cmd.Parameters[4].Value = zone.Description;
                cmd.Parameters[5].Value = zone.Timetable;
                cmd.Parameters[6].Value = zone.Zoneid;
                cmd.Parameters[7].Value = zone.Id;

                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Delete a point from the geometry table
        /// </summary>
        /// <param name="pointId">The point to be deleted</param>
        /// <param name="polyIndex">The index of the point to be deleted</param>
        /// <param name="zoneId">Id of the zone that the point is in</param>
        public async void RemovePoint(int pointId, int polyIndex, int zoneId)
        {
            using (var cmd =
                new MySqlCommand("UPDATE geometry SET polyindex = polyindex - 1 WHERE polyindex > @polyIndex AND zoneid = @zoneId")
                    { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@polyindex", polyIndex);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }

            using (var cmd = new MySqlCommand("DELETE FROM geometry WHERE id = @id AND zoneid = @zoneId")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", pointId);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }
        }

        /// <summary>
        /// Insert a point to the database
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="polyIndex"></param>
        /// <returns></returns>
        public async Task<int> InsertPointAsync(int zoneId, double lat, double lng, int polyIndex)
        {
            using (var cmd = 
                new MySqlCommand("UPDATE geometry SET polyindex = polyindex + 1 WHERE polyindex >= @polyIndex AND zoneid = @zoneId")
                { Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@polyindex", polyIndex);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }

            using (var cmd =
                new MySqlCommand(
                        "INSERT INTO geometry (zoneid, lat, lng, polyIndex) VALUES (@zoneId, @lat, @lng, @polyIndex)")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lng", lng);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                cmd.Parameters.AddWithValue("@polyIndex", polyIndex);

                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();

                return (int)cmd.LastInsertedId;
            }
        }

        /// <summary>
        /// Update a point of a zone in the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public async void UpdatePoint(int id, double lat, double lng)
        {
            using (var cmd = new MySqlCommand("UPDATE geometry SET lat = @lat, lng = @lng WHERE id = @id")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@lat", lat);
                cmd.Parameters.AddWithValue("@lng", lng);
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
            }
        }
    }
}

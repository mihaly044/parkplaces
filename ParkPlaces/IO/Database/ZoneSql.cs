using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;
using ParkPlaces.Map_shapes;
using PPNetLib.Prototypes;

namespace ParkPlaces.IO.Database
{
    public partial class Sql
    {
        /// <summary>
        /// Contains the IDs of points that are waiting to be deleted from the database
        /// (delete queue)
        /// </summary>
        private Queue<int> _pointsToBeDeleted;

        /// <summary>
        /// Creates the _pointsToBeDeleted queue if needed
        /// </summary>
        /// <returns>True if the list had to be created</returns>
        private bool CreatePointsQueue()
        {
            if (_pointsToBeDeleted == null)
            {
                _pointsToBeDeleted = new Queue<int>();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a point to the delete queue
        /// </summary>
        /// <param name="g"></param>
        public void AddPointToBeDeleted(Geometry g)
        {
            CreatePointsQueue();
            _pointsToBeDeleted.Enqueue(g.Id);
        }

        /// <summary>
        /// Return the next point to be deleted
        /// </summary>
        /// <returns>The next item to be deleted. -1 if there are none</returns>
        private int FetchPointToBeDeleted()
        {
            if (CreatePointsQueue())
            {
                return -1;
            }

            if(_pointsToBeDeleted.Count > 0)
            {
                return _pointsToBeDeleted.Dequeue();
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Load polygon data from the database
        /// </summary>
        /// <returns>Data transfer object that holds the data</returns>
        public Dto2Object LoadZones(IProgress<int> progress)
        {
            var dto = new Dto2Object
            {
                Type = "ZoneCollection",
                Zones = new List<PolyZone>()
            };


            var zoneCount = GetZoneCount();
            int doneSoFar = 0;
            int lastReportedProgress = -1;
            progress.Report(0);

            var strCmd = "SELECT * FROM zones INNER JOIN cities ON cities.id = zones.cityid";
            if (LimitCity != string.Empty)
                strCmd += " WHERE cities.city LIKE @city";

            using (var cmd = new MySqlCommand(strCmd)
                { Connection = GetConnection() })
            {
                var geometryConnection = GetConnection();

                if (LimitCity != string.Empty)
                    cmd.Parameters.AddWithValue("@city", '%' + LimitCity +'%');

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

                    dto.Zones.Add(zone);

                    doneSoFar++;
                    var progressPercentage = (int)((double)doneSoFar / zoneCount * 100);
                    if (progressPercentage != lastReportedProgress)
                    {
                        progress.Report(progressPercentage);

                        lastReportedProgress = progressPercentage;
                    }
                }
                geometryConnection.Close();
                rd.Close();
            }
            return dto;
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

        /// <summary>
        /// Remove a zone from the database
        /// </summary>
        /// <param name="polygon">The zone to be removed</param>
        public async void RemoveZone(Polygon polygon)
        {
            var zone = polygon.GetZoneInfo();

            using (var cmd = new MySqlCommand("DELETE FROM zones WHERE id = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", zone.Id);
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new MySqlCommand("DELETE FROM geometry WHERE zoneid = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", zone.Id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

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
            }
        }

        /// <summary>
        /// Update all points of a polygon
        /// Insert points into the database if necessary
        /// </summary>
        /// <param name="polygon"></param>
        public async void UpdatePoints(Polygon polygon)
        {
            var zone = polygon.GetZoneInfo();

            if(!IsZoneExist(zone))
            {
                // Zone not in database. There is nothing to save.
                return;
            }

            var i = 0;
            foreach (var geometry in zone.Geometry.ToList())
            {
                if (geometry.Id == 0 && zone.Geometry[i].Id == 0)
                {
                    Debug.WriteLine($"Insert pt {geometry} of {zone.Id}");

                    // New point. Insert into database
                    using (var cmd =
                        new MySqlCommand(
                                "INSERT INTO geometry (zoneid, lat, lng) VALUES (@zoneid, @lat, @lng)")
                            { Connection = GetConnection() })
                    {
                        cmd.Parameters.AddWithValue("@lat", geometry.Lat);
                        cmd.Parameters.AddWithValue("@lng", geometry.Lng);
                        cmd.Parameters.AddWithValue("@zoneid", zone.Id);

                        await cmd.ExecuteNonQueryAsync();

                        zone.Geometry.ElementAt(i++).Id = (int)cmd.LastInsertedId;

                        cmd.Connection.Close();
                    }
                }
                else if (geometry.IsModified)
                {
                    // Found a point that needs to be updated in
                    // the database 

                    Debug.WriteLine($"Update pt {geometry.Id} of {zone.Id}");

                    using (var cmd = new MySqlCommand("UPDATE geometry SET lat = @lat, lng = @lng WHERE id = @id")
                        { Connection = GetConnection() })
                    {
                        cmd.Parameters.AddWithValue("@lat", geometry.Lat);
                        cmd.Parameters.AddWithValue("@lng", geometry.Lng);
                        cmd.Parameters.AddWithValue("@id", geometry.Id);
                        await cmd.ExecuteNonQueryAsync();
                        cmd.Connection.Close();
                    }

                    zone.Geometry[i].IsModified = false;
                }
            }

            // Process points to be deleted
            int id;
            while((id = FetchPointToBeDeleted()) > -1 )
            {
                using (var cmd = new MySqlCommand("DELETE FROM geometry WHERE id = @id")
                { Connection = GetConnection() })
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    cmd.Connection.Close();
                }
            }
        }

        /// <summary>
        /// Update a point of a zone in the database
        /// </summary>
        /// <param name="g">The point to be updated</param>
        public async void UpdatePoint(Geometry g)
        {
            using (var cmd = new MySqlCommand("UPDATE geometry SET lat = @lat, lng = @lng WHERE id = @id")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@lat", g.Lat);
                cmd.Parameters.AddWithValue("@lng", g.Lng);
                cmd.Parameters.AddWithValue("@id", g.Id);
                await cmd.ExecuteNonQueryAsync();
                cmd.Connection.Close();
                g.IsModified = false;
            }
        }

        /// <summary>
        /// Check if a given point of a zone exists in the database
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True if the point exists in the geometry table</returns>
        public bool IsPointExist(Geometry point)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM geometry WHERE id = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", point.Id);
                var count = cmd.ExecuteScalar();
                cmd.Connection.Close();
                return int.Parse(count.ToString()) > 0;
            }
        }

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
        /// Check if a given zone exists in the database
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True if the zone exists in the database</returns>
        public bool IsZoneExist(PolyZone zone)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM zones WHERE id = @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", zone.Id);
                var count = cmd.ExecuteScalar();
                cmd.Connection.Close();
                return int.Parse(count.ToString()) > 0;
            }
        }

        /// <summary>
        /// Export all the available data in the passed dto object to
        /// the database
        /// </summary>
        /// <param name="dto">The data transfer object that holds polygon data</param>
        public void ExportToMySql(Dto2Object dto, IProgress<int> progress)
        {
            // Flush table
            Execute("DELETE FROM geometry");
            Execute("DELETE FROM zones");
            Execute("DELETE FROM cities");
            Execute("ALTER TABLE cities AUTO_INCREMENT = 0");
            Execute("ALTER TABLE zones AUTO_INCREMENT = 0");
            Execute("ALTER TABLE geometry AUTO_INCREMENT = 0");

            int doneSoFar = 0;
            int lastReportedProgress = -1;
            progress.Report(0);

            using (var cmd = new MySqlCommand(
                    "INSERT INTO zones (cityid, color, fee, service_na, description, timetable, common_name) VALUES" +
                    "(@cityid, @color, @fee, @service_na, @description, @timetable, @common_name)")
                { Connection = GetConnection() })
            {

                var zoneId = 1;
                cmd.Parameters.AddRange(new[]
                {
                    new MySqlParameter("@cityid", MySqlDbType.String), new MySqlParameter("@color", MySqlDbType.String),
                    new MySqlParameter("@fee", MySqlDbType.String), new MySqlParameter("@service_na", MySqlDbType.String),
                    new MySqlParameter("@description", MySqlDbType.String), new MySqlParameter("@timetable", MySqlDbType.String),
                    new MySqlParameter("@common_name", MySqlDbType.String)
                });

                foreach (var zone in dto.Zones)
                {
                    var city = City.FromString(zone.Telepules);

                    cmd.Parameters[0].Value = !IsDuplicateCity(city) ? InsertCity(city) : GetCityId(city);
                    cmd.Parameters[1].Value = zone.Color;
                    cmd.Parameters[2].Value = zone.Fee;
                    cmd.Parameters[3].Value = zone.ServiceNa ?? "Nincs adat";
                    cmd.Parameters[4].Value = zone.Description ?? "Nincs adat";
                    cmd.Parameters[5].Value = zone.Timetable ?? "";
                    cmd.Parameters[6].Value = zone.Zoneid;

                    cmd.ExecuteNonQuery();
                    zoneId = (int)cmd.LastInsertedId;

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
                            cmd1.ExecuteNonQuery();
                            cmd1.Connection.Close();
                        }
                    }

                    doneSoFar++;
                    var progressPercentage = (int)((double)doneSoFar / dto.Zones.Count * 100);
                    if (progressPercentage != lastReportedProgress)
                    {
                        progress.Report(progressPercentage);

                        lastReportedProgress = progressPercentage;
                    }
                }
            }

            progress.Report(100);
        }
    }
}

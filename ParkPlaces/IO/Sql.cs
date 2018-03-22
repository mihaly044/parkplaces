using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;
using System.Collections.Specialized;

#if !DEBUG
    using System.Windows.Forms;
#endif

using CryptSharp;
using ParkPlaces.Misc;

namespace ParkPlaces.IO
{
    public class Sql
    {
        public static Sql Instance => _instance ?? (_instance = new Sql());
        private static Sql _instance;
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }
        public string Port { get; set; }

        /// <summary>
        /// Used to send notifications about loading progress
        /// </summary>
        public EventHandler<UpdateProcessChangedArgs> OnUpdateChangedEventHandler;

        /// <summary>
        /// Re-read configuration values
        /// </summary>
        public static void ResetInstance()
        {
            Instance.LoadDbCredientals();
        }

        public Sql()
        {
            LoadDbCredientals();
            SetupDb();
        }

        public Sql(string server = "", string database = "", string user = "", string password = "", string port = "3306")
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
            Port = port;
            SetupDb();
        }

        /// <summary>
        /// Check if a database with a given name exist on the SQL server
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        private bool IsDatabaseExists(string dbName)
        {
            using (var cmd =
                new MySqlCommand("SELECT COUNT(*) FROM information_schema.schemata WHERE SCHEMA_NAME = @dbName")
                { Connection = GetConnection(true) })
            {
                cmd.Parameters.AddWithValue("@dbName", dbName);
                return int.Parse(cmd.ExecuteScalar().ToString()) >= 1;
            }
        }

        /// <summary>
        /// Check if a table with a given name exists on the SQL server
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private bool IsTableExists(string dbName, string tableName)
        {
            using (var cmd =
                new MySqlCommand("SELECT count(*) FROM information_schema.TABLES WHERE(TABLE_SCHEMA = @dbName) AND(TABLE_NAME = @tableName)")
                { Connection = GetConnection(true) })
            {
                cmd.Parameters.AddWithValue("@dbName", dbName);
                cmd.Parameters.AddWithValue("@tableName", tableName);

                return int.Parse(cmd.ExecuteScalar().ToString()) >= 1;
            }
        }

        /// <summary>
        /// Check if the required database schema exists.
        /// Create it if not
        /// </summary>
        private void SetupDb()
        {
            if (!IsDatabaseExists(Database) || !IsTableExists(Database, "cities"))
            {
                using (var cmd1 = new MySqlCommand(Properties.Resources.parkplaces)
                { Connection = GetConnection(true) })
                {
                    cmd1.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Load database credientals from App.config
        /// </summary>
        private void LoadDbCredientals()
        {
            // Switch between alt and main db connections based on App.config
            var configSection = "";
            var connectionMode = ConfigurationManager.AppSettings["DBConnection"];
            switch (connectionMode)
            {
                case "alt":
                    configSection = "AltDBConnection";
                    break;
                case "main":
                    configSection = "DBConnection";
                    break;
                default:
                    throw new Exception("Invalid configuration value fouund in DBConnectiion");
            }

            var dbSect = ConfigurationManager.GetSection(configSection) as NameValueCollection;
            if (dbSect == null) return;
            Server = dbSect["server"];
            Database = dbSect["database"];
            User = dbSect["user"];
            Password = dbSect["password"];
            Port = dbSect["port"];
        }

        /// <summary>
        /// Produces a new MySqlConnection object that is used to
        /// execute queries and retrieve data from the MySQL database
        /// </summary>
        /// <param name="noDatabase">True if there is no database specified in the connection string</param>
        /// <returns></returns>
        public MySqlConnection GetConnection(bool noDatabase = false)
        {
            MySqlConnection mySqlConnection;

            if (noDatabase)
            {
                mySqlConnection = new MySqlConnection(
                    $@"SERVER={Server};PORT={Port};UID={User};PASSWORD={Password}");
            }
            else
            {
                mySqlConnection = new MySqlConnection(
                    $@"SERVER={Server};PORT={Port};DATABASE={Database};UID={User};PASSWORD={Password}");
            }


            try
            {
                mySqlConnection.Open();
            }
            catch (MySqlException e)
            {
                var error = e.GetExceptionNumber();
#if DEBUG
                Debug.WriteLine($"Database error {error}");
                Debugger.Break();
#else
                MessageBox.Show($"MySQL error {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
#endif
            }
            return mySqlConnection;
        }

        /// <summary>
        /// Authenticate an user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>The logged in user object or null if the username
        /// or the password was wrong</returns>
        public User AuthenticateUser(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
                return null;

            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE username = @username ")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader.HasRows)
                    {
                        if (Crypter.CheckPassword(password, reader["password"].ToString()))
                        {
                            var groupRole = (GroupRole) Enum.Parse(typeof(GroupRole), reader["groupid"].ToString());
                            return new User(reader["UserName"].ToString(), (int)reader["id"])
                            {
                                GroupRole = groupRole,
                                IsAuthenticated = groupRole > GroupRole.Guest,
                                CreatorId = (int)reader["creatorid"]
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Export all the available data in the passed dto object to
        /// the database
        /// </summary>
        /// <param name="dto">The data transfer object that holds polygon data</param>
        public async Task<bool> ExportToMySql(Dto2Object dto)
        {
            // Flush table
            Execute("DELETE FROM geometry");
            Execute("DELETE FROM zones");
            Execute("DELETE FROM cities");
            Execute("ALTER TABLE cities AUTO_INCREMENT = 0");
            Execute("ALTER TABLE zones AUTO_INCREMENT = 0");
            Execute("ALTER TABLE geometry AUTO_INCREMENT = 0");

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
                    await cmd.ExecuteNonQueryAsync();

                    cityIds.Add(city.Telepules, cityId);
                    cityId++;
                }
            }

            using (var cmd = new MySqlCommand(
                "INSERT INTO zones (cityid, color, fee, service_na, description, timetable, common_name) VALUES" +
                "(@cityid, @color, @fee, @service_na, @description, @timetable, @common_name)") { Connection = GetConnection() })
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
                    cmd.Parameters[0].Value = cityIds[zone.Telepules];
                    cmd.Parameters[1].Value = zone.Color;
                    cmd.Parameters[2].Value = zone.Fee;
                    cmd.Parameters[3].Value = zone.ServiceNa;
                    cmd.Parameters[4].Value = zone.Description;
                    cmd.Parameters[5].Value = zone.Timetable;
                    cmd.Parameters[6].Value = zone.Zoneid;

                    await cmd.ExecuteNonQueryAsync();

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
                            await cmd1.ExecuteNonQueryAsync();
                            cmd1.Connection.Close();
                        }
                    }
                    zoneId++;
                }
            }

            return true;
        }

        /// <summary>
        /// Executes a query 
        /// </summary>
        /// <param name="query"></param>
        /// <returns>-1 if the execution fails. Otherwise the number of affected rows </returns>
        public int Execute(string query)
        {
            try
            {
                return new MySqlCommand(query) { Connection = GetConnection() }
                    .ExecuteNonQuery();
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }

        /// <summary>
        /// Load polygon data from the database
        /// </summary>
        /// <returns>Data transfer object that holds the data</returns>
        public async Task<Dto2Object> LoadZones()
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
                while (await rd.ReadAsync())
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
                        Zoneid = rd["common_name"].ToString()
                    };

                    using (MySqlCommand cmd1 = new MySqlCommand($"SELECT * FROM geometry WHERE zoneid = {rd["id"]}") { Connection = geometryConnection })
                    {
                        var rd1 = await cmd1.ExecuteReaderAsync();
                        while (await rd1.ReadAsync())
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

        /// <summary>
        /// Insert a zone into the database
        /// </summary>
        /// <param name="zone">The zone to be inserted</param>
        public async void InsertZone(PolyZone zone)
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

                cmd.Parameters[0].Value = 1;
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
                        cmd1.Connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the count of zones
        /// </summary>
        /// <returns>The count of rows in the zones table</returns>
        public async Task<int> GetZoneCount()
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM zones") { Connection = GetConnection() })
            {
                var count = await cmd.ExecuteScalarAsync();
                return int.Parse(count.ToString());
            }
        }

        /// <summary>
        /// Load users from the database
        /// </summary>
        /// <returns>A list consisting of User type objects</returns>
        public async Task<List<User>> LoadUsers()
        {
            var usersList = new List<User>();

            using (var cmd = new MySqlCommand("SELECT * FROM users")
            { Connection = GetConnection() })
            {
                var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    usersList.Add(new User(rd["username"].ToString(), (int)rd["id"])
                    {
                        UserName = rd["username"].ToString(),
                        GroupRole = (GroupRole)Enum.Parse(typeof(GroupRole), rd["groupid"].ToString())
                    });
                }
            }

            return usersList;
        }

        /// <summary>
        /// Remove an user from the database
        /// </summary>
        /// <param name="u">The user to be removed</param>
        public async void RemoveUser(User u)
        {
            if (u == null) return;

            using (var cmd = new MySqlCommand("DELETE FROM users WHERE id = @id")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", u.Id);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        
        /// <summary>
        /// Checks if an username already exists in the database
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public bool IsDuplicateUser(User u)
        {
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE username = @username AND id <> @id") { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", u.UserName);
                cmd.Parameters.AddWithValue("@id", u.Id);
                return int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
        }

        /// <summary>
        /// Update a user's data in the database
        /// </summary>
        /// <param name="user"></param>
        public void UpdateUser(User user)
        {
            using (var cmd = new MySqlCommand("UPDATE users SET username = @username, groupid = @groupid, creatorid = @creatorid WHERE id = @id")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", user.UserName);
                cmd.Parameters.AddWithValue("@id", user.Id);
                cmd.Parameters.AddWithValue("@groupid", user.GroupRole);
                cmd.Parameters.AddWithValue("@creatorid", user.CreatorId);
                
                cmd.ExecuteNonQuery();
            }

            if(user.Password != null)
            {
                using (var cmd = new MySqlCommand("UPDATE users SET password = @password WHERE id = @id")
                { Connection = GetConnection() })
                {
                    cmd.Parameters.AddWithValue("@password", Crypter.Blowfish.Crypt(user.Password));
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Get user data from the database based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetUserData(int id)
        {
            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE id = @id")
                {Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader.HasRows)
                    {
                        return new User(reader["username"].ToString(), (int)reader["id"])
                        {
                            GroupRole = (GroupRole)Enum.Parse(typeof(GroupRole), reader["groupid"].ToString()),
                            CreatorId = (int)reader["creatorid"]
                        };
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Get updated user data from the database
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User GetUserData(User user)
        {
            if (user == null)
            {
                return null;
            }

            return GetUserData(user.Id);
        }

        /// <summary>
        /// Insert a new user into the database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="creatorUser"></param>
        public void InsertUser(User user, User creatorUser = null)
        {
            using (var cmd = new MySqlCommand("INSERT INTO users (username, password, groupid) VALUES (@username, @password, @groupid)")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", user.UserName);
                cmd.Parameters.AddWithValue("@password", Crypter.Blowfish.Crypt(user.Password));
                cmd.Parameters.AddWithValue("@groupid", user.GroupRole);
                cmd.ExecuteNonQuery();
            }

            if(creatorUser != null)
            {
                var newUser = GetUserData(user);
                newUser.CreatorId = creatorUser.Id;
                UpdateUser(newUser);
            }
        }
    }
}
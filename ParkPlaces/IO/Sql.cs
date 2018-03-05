using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Windows.Forms;
using CryptSharp;
using ParkPlaces.Misc;

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
            SetupDb();
        }

        public Sql(string server="", string database="", string user="", string password="", string port="3306")
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
            Port = port;
            SetupDb();
        }

        private void SetupDb()
        {
            using (var cmd =
                new MySqlCommand("SELECT COUNT(*) FROM information_schema.schemata WHERE SCHEMA_NAME = @dbName")
                    {Connection = GetConnection(true)})
            {
                cmd.Parameters.AddWithValue("@dbName", Database);
                var count = cmd.ExecuteScalar();

                if (int.Parse(count.ToString()) >= 1) return;
                // Database does not exist. Create it here
                using (var cmd1 = new MySqlCommand(@"-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1
-- Létrehozás ideje: 2018. Már 05. 20:22
-- Kiszolgáló verziója: 10.1.30-MariaDB
-- PHP verzió: 7.2.1

SET SQL_MODE = ""NO_AUTO_VALUE_ON_ZERO"";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = ""+00:00"";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `parkplaces`
--
CREATE DATABASE IF NOT EXISTS `parkplaces` DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci;
USE `parkplaces`;

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `cities`
--

CREATE TABLE `cities` (
  `id` int(11) NOT NULL,
  `city` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `geometry`
--

CREATE TABLE `geometry` (
  `zoneid` int(11) NOT NULL,
  `id` int(11) NOT NULL,
  `lat` double NOT NULL,
  `lng` double NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `username` varchar(20) NOT NULL,
  `password` varchar(255) NOT NULL,
  `groupid` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- A tábla adatainak kiíratása `users`
--

INSERT INTO `users` (`id`, `username`, `password`, `groupid`) VALUES
(1, 'admin', '$2y$10$naypQWIa7gb7H8QLIUWa9.I8K3J3fh0SIp3AdOmnYpApBpnrC/KjG', 4);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `zones`
--

CREATE TABLE `zones` (
  `id` int(11) NOT NULL,
  `cityid` int(11) NOT NULL,
  `color` varchar(255) NOT NULL,
  `fee` int(11) NOT NULL,
  `service_na` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `timetable` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Indexek a kiírt táblákhoz
--

--
-- A tábla indexei `cities`
--
ALTER TABLE `cities`
  ADD PRIMARY KEY (`id`),
  ADD KEY `id` (`id`);

--
-- A tábla indexei `geometry`
--
ALTER TABLE `geometry`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `zones`
--
ALTER TABLE `zones`
  ADD PRIMARY KEY (`id`),
  ADD KEY `cityid` (`cityid`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `cities`
--
ALTER TABLE `cities`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=79;

--
-- AUTO_INCREMENT a táblához `geometry`
--
ALTER TABLE `geometry`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=25945;

--
-- AUTO_INCREMENT a táblához `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT a táblához `zones`
--
ALTER TABLE `zones`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=1129;

--
-- Megkötések a kiírt táblákhoz
--

--
-- Megkötések a táblához `zones`
--
ALTER TABLE `zones`
  ADD CONSTRAINT `zones_ibfk_1` FOREIGN KEY (`cityid`) REFERENCES `cities` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
")
                    { Connection = GetConnection(true)})
                {
                    cmd1.ExecuteNonQuery();
                }
            }
        }

        private void LoadDbCredientals()
        {
            var dbSect = ConfigurationManager.GetSection("DBConnection") as NameValueCollection;
            if (dbSect == null) return;
            Server = dbSect["server"];
            Database = dbSect["database"];
            User = dbSect["user"];
            Password = dbSect["password"];
            Port = dbSect["port"];
        }


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
#if DEBUG
                Debug.WriteLine("Mysql error: " + e.Message);
                Debugger.Break();
#else
                MessageBox.Show("Fatal database error. The application is going to close.", "Error" , MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
#endif
            }
            return mySqlConnection;
        }

        public GroupRole AuthenticateUser(string username, string password)
        {
            if (username == string.Empty || password == string.Empty)
                return GroupRole.None;

            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE username = @username ")
            { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", username);

                using(var reader = cmd.ExecuteReader())
                {
                    if(reader.Read() && reader.HasRows)
                        if (Crypter.CheckPassword(password, reader["password"].ToString()))
                            return (GroupRole)Enum.Parse(typeof(GroupRole), reader["groupid"].ToString());
                }
            }

            return GroupRole.None;
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
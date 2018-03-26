using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using ParkPlaces.Misc;

namespace ParkPlaces.IO.Database
{
    public partial class Sql
    {
        /// <summary>
        /// Load cities from the database
        /// </summary>
        /// <returns>A list consisting of City type objects</returns>
        public async Task<List<City>> LoadCities()
        {
            var cityList = new List<City>();

            using (var cmd = new MySqlCommand("SELECT * FROM cities")
                {Connection = GetConnection()})
            {
                var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    cityList.Add(new City((int) rd["id"])
                    {
                        Name = rd["city"].ToString()
                    });
                }
            }

            return cityList;
        }

        /// <summary>
        /// Checks if a zone already exists in the database
        /// </summary>
        /// <param name="city"></param>
        /// <returns></returns>
        public bool IsDuplicateCity(City city)
        {
            using (var cmd =
                new MySqlCommand("SELECT COUNT(*) FROM cities WHERE city = @city") {Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@city", city);
                return int.Parse(cmd.ExecuteScalar().ToString()) > 0;
            }
        }

        /// <summary>
        /// Insert a new city into the database
        /// </summary>
        /// <param name="city"></param>
        /// <returns>The id of the newly inserted city</returns>
        public int InsertCity(City city)
        {
            using (var cmd = new MySqlCommand("INSERT INTO cities (city) VALUES (@city)")
                {Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@city", city.Name);
                cmd.ExecuteNonQuery();
                return (int) cmd.LastInsertedId;
            }
        }

        /// <summary>
        /// Remove an city from the database
        /// </summary>i
        /// <param name="city">The user to be removed</param>
        public async void RemoveCity(City city)
        {
            if (city == null) return;

            using (var cmd = new MySqlCommand("DELETE FROM cities WHERE id = @id")
                {Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@id", city.Id);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        /// <summary>
        /// Update a city's data in the database
        /// </summary>
        /// <param name="city"></param>
        public void UpdateCity(City city)
        {
            using (var cmd =
                new MySqlCommand(
                    "UPDATE city SET city = @city WHERE id = @id")
                {
                    Connection = GetConnection()
                })
            {
                cmd.Parameters.AddWithValue("@city", city);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Get city data from the database based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public City GetCityData(int id)
        {
            using (var cmd = new MySqlCommand("SELECT * FROM city WHERE id = @id")
                {Connection = GetConnection()})
            {
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader.HasRows)
                    {
                        return City.FromString(reader["city"].ToString(), id);
                    }
                }
            }
            return null;
        }

        public int GetCityId(City city)
        {
            using (var cmd = new MySqlCommand("SELECT * FROM cities WHERE city = @city")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@city", city);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && reader.HasRows)
                    {
                        return (int) reader["id"];
                    }
                }
            }
            return 0;
        }
    }
}

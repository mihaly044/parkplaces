using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptSharp;
using MySql.Data.MySqlClient;
using PPNetLib.Prototypes;

namespace PPServer.Database
{
    public partial class Sql
    {
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
                            var groupRole = (GroupRole)Enum.Parse(typeof(GroupRole), reader["groupid"].ToString());
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
        /// <param name="userId">The user to be removed</param>
        public async void RemoveUser(int userId)
        {
            using (var cmd = new MySqlCommand("DELETE FROM users WHERE id = @id")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@id", userId);
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

            if (user.Password != null)
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
        private User GetUserData(int id)
        {
            using (var cmd = new MySqlCommand("SELECT * FROM users WHERE id = @id")
                { Connection = GetConnection() })
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
        /// <param name="creatorId"></param>
        public void InsertUser(User user, int creatorId)
        {
            using (var cmd = new MySqlCommand("INSERT INTO users (username, password, groupid, creatorid) VALUES (@username, @password, @groupid, @creatorid)")
                { Connection = GetConnection() })
            {
                cmd.Parameters.AddWithValue("@username", user.UserName);
                cmd.Parameters.AddWithValue("@password", Crypter.Blowfish.Crypt(user.Password));
                cmd.Parameters.AddWithValue("@groupid", user.GroupRole);
                cmd.Parameters.AddWithValue("@creatorid", creatorId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}

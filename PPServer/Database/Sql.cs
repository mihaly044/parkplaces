using MySql.Data.MySqlClient;
using PPServer.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPServer.Database
{
    public partial class Sql
    {
        public static Sql Instance => _instance ?? (_instance = new Sql());
        private static Sql _instance;
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }
        public string Port { get; set; }

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
        /// Check if the required database schema exists.
        /// Create it if not
        /// </summary>
        private void SetupDb()
        {
            // TODO: Rewrite SetupDb
            /*if (!IsDatabaseExists(Database) || !IsTableExists(Database, "cities"))
            {
                using (var cmd1 = new MySqlCommand(Properties.Resources.parkplaces)
                { Connection = GetConnection(true) })
                {
                    cmd1.ExecuteNonQuery();
                }
            }*/
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
                    $@"SERVER={Server};PORT={Port};UID={User};PASSWORD={Password};SslMode=none;Pooling=false");
            }
            else
            {
                mySqlConnection = new MySqlConnection(
                    $@"SERVER={Server};PORT={Port};DATABASE={Database};UID={User};PASSWORD={Password};SslMode=none;Pooling=false");
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
                Application.Exit();
#endif
            }
            return mySqlConnection;
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
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }
    }
}


using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using MySql.Data.MySqlClient;
using ParkPlaces.Extensions;
#if !DEBUG
    using System.Windows.Forms;
#endif

namespace ParkPlaces.IO.Database
{
    [Obsolete("Client side SQL operations are obsolete. Use server side instead", false)]
    public partial class Sql
    {
        public static Sql Instance => _instance ?? (_instance = new Sql());
        private static Sql _instance;
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { private get; set; }
        public string Port { get; set; }
        public string LimitCity { get; set; }

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
                    $@"SERVER={Server};PORT={Port};UID={User};PASSWORD={Password};SslMode=none");
            }
            else
            {
                mySqlConnection = new MySqlConnection(
                    $@"SERVER={Server};PORT={Port};DATABASE={Database};UID={User};PASSWORD={Password};SslMode=none");
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
            } catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }
    }
}
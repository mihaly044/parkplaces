using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using PPNetLib.Prototypes;

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

    }
}

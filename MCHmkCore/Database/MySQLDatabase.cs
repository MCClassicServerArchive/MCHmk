/*
    Copyright 2016 Jjp137

    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at

    http://www.opensource.org/licenses/ecl2.php
    http://www.gnu.org/licenses/gpl-3.0.html

    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
*/

using System;

using MySql.Data.MySqlClient;

namespace MCHmk.SQL {
    /// <summary>
    /// The MySQLDatabase class facilitates access to the server's MySQL-powered database.
    /// </summary>
    public class MySQLDatabase : Database {
        /// <summary>
        /// Gets the SQLEngine value representing the SQL engine powering the database.
        /// </summary>
        public override SQLEngine Engine {
            get {
                return SQLEngine.MySQL;
            }
        }

        /// <summary>
        /// Constructs a new MySQLDatabase object.
        /// </summary>
        /// <remarks>
        /// The constructor tests whether the given database exists on the MySQL server and creates it if necessary,
        /// and an exception is thrown if a valid connection to the MySQL server cannot be established.
        /// </remarks>
        /// <param name="host"> The IP address of the MySQL server. </param>
        /// <param name="port"> The port used to connect to the MySQL server.</param>
        /// <param name="user"> The username to use when accessing the MySQL server. </param>
        /// <param name="pass"> The password to use when accessing the MySQL server. </param>
        /// <param name="database"> The name of the database to use. </param>
        /// <param name="pooling"> Whether database pooling should be used. </param>
        /// <exception cref="MySqlException"> Thrown if an error occurs while checking whether creating
        /// the database is necessary. </exception>
        public MySQLDatabase(string host, string port, string user, string pass, string database, bool pooling) {
            // Create the factory object that can be used to obtain objects specific to MySQL.
            _factory = new MySqlClientFactory();

            // Create the initial connection string.
            _connString = "Data Source=" + host + ";" +
                          "Port=" + port + ";" +
                          "User ID=" + user + ";" +
                          "Password=" + pass + ";" +
                          "Pooling=" + pooling.ToString() + ";";

            // Attempt to create the database if it doesn't exist, and if the database is created or is
            // already there, append to the connection string and use that string from now on.
            try {
                using (MySqlConnection conn = new MySqlConnection(_connString)) {
                    conn.Open();

                    string cmdText = "CREATE DATABASE IF NOT EXISTS `" + database + "`";
                    using (MySqlCommand cmd = new MySqlCommand(cmdText, conn)) {
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
                _connString += "Database=" + database + ";";
            }
            catch (MySqlException) {
                throw;
            }
        }
    }
}

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

using System.Data.SQLite;

namespace MCHmk.SQL {
    /// <summary>
    /// The SQLiteDatabase class facilitates access to the server's SQLite-powered database.
    /// </summary>
    public class SQLiteDatabase : Database {
        /// <summary>
        /// Gets the SQLEngine value representing the SQL engine powering the database.
        /// </summary>
        public override SQLEngine Engine {
            get {
                return SQLEngine.SQLite;
            }
        }

        /// <summary>
        /// Constructs a new SQLiteDatabase object.
        /// </summary>
        /// <param name="dbPath"> The file path to the SQLite database. </param>
        /// <param name="pooling"> Whether pooling should be used. </param>
        public SQLiteDatabase(string dbPath, bool pooling) {
            // Create the factory object that can be used to obtain objects specific to SQLite.
            _factory = new SQLiteFactory();

            // Create the connection string that will be used for every operation.
            _connString = "Data Source=" + dbPath + ";" +
                          "Version=3;" +
                          "Pooling=" + pooling.ToString() + ";" +
                          "Max Pool Size=1000;";
        }
    }
    
}

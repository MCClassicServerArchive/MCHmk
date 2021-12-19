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
using System.Data;
using System.Data.Common;

namespace MCHmk.SQL {
    /// <summary>
    /// The abstract Database class facilitates access to the server's database.
    /// </summary>
    public abstract class Database {
        /// <summary>
        /// The factory that provides objects specific to the SQL engine being used.
        /// </summary>
        protected DbProviderFactory _factory;
        /// <summary>
        /// The connection string that is used to connect to the database.
        /// </summary>
        protected string _connString;

        /// <summary>
        /// Gets the SQLEngine value representing the SQL engine powering the database.
        /// </summary>
        public abstract SQLEngine Engine {
            get;
        }

        /// <summary>
        /// Gets the factory that can be used to create new objects specific to the SQL engine being used.
        /// </summary>
        /// <remarks>
        /// This property is provided in case the caller needs to perform a database operation that does
        /// not have its own method.
        /// </remarks>
        public DbProviderFactory Factory {
            get {
                return _factory;
            }
        }

        /// <summary>
        /// Opens a connection to the database and returns it.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for ensuring that the DbConnection object is properly disposed.
        /// It is best to use a 'using' block when working with DbConnection objects returned from this method.
        /// </remarks>
        /// <returns> A DbConnection object representing an open connection that is specific to the SQL engine
        /// being used. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while opening the connection. </exception>
        public DbConnection OpenConnection() {
            DbConnection conn = null;

            try {
                conn = _factory.CreateConnection();
                conn.ConnectionString = _connString;
                conn.Open();

                return conn;
            }
            catch (DbException) {
                if (conn != null) {
                    conn.Dispose();
                    conn = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL statement that returns a single integer.
        /// </summary>
        /// <remarks>
        /// This method is best for "SELECT COUNT(*) FROM table" statements.
        /// </remarks>
        /// <param name="statement"> The SQL statement to execute. </param>
        /// <returns> The integer representing the result. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement. </exception>
        public Int64 ExecuteCount(string statement) {
            Int64 count = 0;

            try {
                using (DbConnection conn = OpenConnection()) {
                    using (DbCommand cmd = conn.CreateCommand()) {
                        cmd.CommandText = statement;
                        count = (Int64)cmd.ExecuteScalar();
                    }
                    conn.Close();
                }
                return count;
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL statement.
        /// </summary>
        /// <remarks>
        /// This method is best for statements that modify a table, such as INSERT or UPDATE statements,
        /// or the database itself.
        /// </remarks>
        /// <param name="statement"> The SQL statement to execute. </param>
        /// <returns> The integer representing the result. It depends on the statement. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement. </exception>
        public int ExecuteStatement(string statement) {
            int count;

            try {
                using (DbConnection conn = OpenConnection()) {
                    count = ExecuteStatement(conn, statement);
                    conn.Close();
                }
                return count;
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Given an active connection, executes a SQL statement.
        /// </summary>
        /// <remarks>
        /// This method is best for statements that modify a table, such as INSERT or UPDATE statements, or the
        /// database itself, and this overload is useful when the caller is managing its own DbConnection object.
        /// </remarks>
        /// <param name="conn"> The DbConnection object to use. </param>
        /// <param name="statement"> The SQL statement to execute. </param>
        /// <returns> The integer representing the number of rows affected. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement.
        /// In particular, an exception may be thrown if the given connection is not valid and open. </exception>
        public int ExecuteStatement(DbConnection conn, string statement) {
            int count;

            try {
                using (DbCommand cmd = conn.CreateCommand()) {
                    cmd.CommandText = statement;
                    count = cmd.ExecuteNonQuery();
                }
                return count;
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL statement and obtains the result as a DataTable object.
        /// </summary>
        /// <remarks>
        /// This method is best for SELECT statements. 
        /// 
        /// The caller is responsible for disposing the DataTable object. It is best to use a 'using' block when
        /// working with DataTable objects returned from this method.
        /// </remarks>
        /// <param name="statement"> The SQL statement to execute. </param>
        /// <returns> The DataTable object containing the data. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement. </exception>
        public DataTable ObtainData(string statement) {
            DataTable table = null;

            try {
                table = new DataTable();

                using (DbConnection conn = OpenConnection()) {
                    using (DbCommand cmd = conn.CreateCommand()) {
                        cmd.CommandText = statement;

                        using (DbDataAdapter data = _factory.CreateDataAdapter()) {
                            data.SelectCommand = cmd;
                            data.Fill(table);
                        }
                    }
                    conn.Close();
                }
                return table;
            }
            catch (DbException) {
                if (table != null) {
                    table.Dispose();
                    table = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Create and return a new PreparedStatement object using the given statement.
        /// </summary>
        /// <remarks>
        /// The caller is responsible for disposing the PreparedStatement object. It is best to use a 'using' block
        /// when working with PreparedStatement objects returned from this method.
        /// </remarks>
        /// <param name="statement"> The SQL statement to execute that contains placeholders. </param>
        /// <returns> A new PreparedStatement object that uses the given statement. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while constructing the PreparedStatement object. </exception>
        public PreparedStatement MakePreparedStatement(string statement) {
            try {
                return new PreparedStatement(this, statement);
            }
            catch (DbException) {
                throw;
            }
        }
    }
}

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
    /// The PreparedStatement class represents a prepared statement that will be executed on the server's database.
    /// </summary>
    /// <remarks>
    /// Use prepared statements instead of building SQL statements using string concatenation in order to avoid
    /// introducing SQL injection vulnerabilities. Note that table names cannot be parameters, so another method must
    /// be used if a table name is variable.
    /// </remarks>
    public class PreparedStatement : IDisposable {
        /// <summary>
        /// The connection that will be used when executing the statement.
        /// </summary>
        private DbConnection _conn = null;
        /// <summary>
        /// The command that will be used when executing the statement.
        /// </summary>
        private DbCommand _cmd = null;
        /// <summary>
        /// The factory object used to create new database-related objects as needed.
        /// </summary>
        private DbProviderFactory _factory;

        /// <summary>
        /// Whether the statement has been prepared yet.
        /// </summary>
        private bool _prepared = false;
        /// <summary>
        /// Whether this object has been disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Gets or sets a parameter's value.
        /// </summary>
        /// <param name="key"> The parameter to set the value of. </param>
        public object this[string key] {
            get {
                return _cmd.Parameters[key].Value;
            }
            set {
                _cmd.Parameters[key].Value = value;
            }
        }

        /// <summary>
        /// Constructs a new PreparedStatement object.
        /// </summary>
        /// <remarks>
        /// The constructor opens a connection with the database, and it throws an exception if a connection
        /// cannot be established.
        /// </remarks>
        /// <param name="d"> The database to use. </param>
        /// <param name="statement"> The statement with placeholders to execute. </param>
        /// <exception cref="DbException"> Thrown if an error occurs while communicating with the database. </exception>
        public PreparedStatement(Database d, string statement) {
            // Keep an instance of the factory so that objects appropriate for the SQL engine being used
            // can be created when needed.
            _factory = d.Factory;

            // Open a connection and create a command.
            try {
                _conn = d.OpenConnection();
                _cmd = _conn.CreateCommand();
                _cmd.CommandText = statement;
            }
            catch (DbException) {
                if (_cmd != null) {
                    _cmd.Dispose();
                    _cmd = null;
                }
                if (_conn != null) {
                    _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }

                throw;
            }
        }

        /// <summary>
        /// Adds a parameter with its value set to null.
        /// </summary>
        /// <param name="name"> The name of the parameter. </param>
        public void AddParam(string name) {
            AddParam(name, null);
        }

        /// <summary>
        /// Adds a parameter with the given value.
        /// </summary>
        /// <param name="name"> The name of the parameter.</param>
        /// <param name="value"> The value of the parameter. </param>
        public void AddParam(string name, object value) {
            DbParameter param = _cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;

            _cmd.Parameters.Add(param);
        }

        /// <summary>
        /// Adds multiple parameters with their values set to null.
        /// </summary>
        /// <param name="names"> The names of the parameters to add. </param>
        public void AddMultipleParams(params string[] names) {
            foreach (string name in names) {
                AddParam(name, null);
            }
        }

        /// <summary>
        /// Executes the prepared statement.
        /// </summary>
        /// <remarks>
        /// This method is best for statements that modify a table, such as INSERT or UPDATE statements,
        /// or the database itself.
        /// </remarks>
        /// <returns> The integer representing the result. It depends on the statement. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement. </exception>
        public int Execute() {
            try {
                // Prepare the statement if it hasn't been prepared yet.
                if (!_prepared) {
                    _cmd.Prepare();
                    _prepared = true;
                }

                // Execute the statement afterwards.
                return _cmd.ExecuteNonQuery();
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Executes the prepared statement and obtains the result as a DataTable object.
        /// </summary>
        /// <remarks>
        /// This method is best for SELECT statements.
        ///
        /// The caller is responsible for disposing the DataTable object. It is best to use a 'using' block when
        /// working with DataTable objects returned from this method.
        /// </remarks>
        /// <returns> The DataTable object containing the data. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while attempting to execute the statement. </exception>
        public DataTable ObtainData() {
            DataTable table = null;

            try {
                // Prepare the statement if it hasn't been prepared yet.
                if (!_prepared) {
                    _cmd.Prepare();
                    _prepared = true;
                }

                // Execute the statement afterwards.
                table = new DataTable();

                using (DbDataAdapter data = _factory.CreateDataAdapter()) {
                    data.SelectCommand = _cmd;
                    data.Fill(table);
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
        /// Obtains a new TransactionHelper object associated with the same database connection that this
        /// PreparedStatement object is using, and begins the transaction.
        /// </summary>
        /// <returns> An TransactionHelper object representing a started transaction that will use the same database
        /// connection as this PreparedStatement object. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs while beginning a transaction. </exception>
        public TransactionHelper BeginTransaction() {
            try {
                return new TransactionHelper(_conn);
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Releases all resources that this PreparedStatement object is using.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources that this PreparedStatement object is using.
        /// </summary>
        /// <param name="disposing"> Whether this method was called from Dispose(). </param>
        protected virtual void Dispose(bool disposing) {
            // Implementators of IDisposable should ignore additional calls to Dispose() after the first one.
            if (_disposed) {
                return;
            }

            // Release all managed resources.
            if (disposing) {
                if (_cmd != null) {
                    _cmd.Dispose();
                    _cmd = null;
                }

                if (_conn != null) {
                    _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }
            }

            // Mark that this object has been disposed.
            _disposed = true;
        }
    }
}

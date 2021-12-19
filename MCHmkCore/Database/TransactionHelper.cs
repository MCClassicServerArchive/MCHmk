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
    /// The TransactionHelper class wraps around an ongoing transaction on the SQL server.
    /// </summary>
    /// <remarks>
    /// Use transactions when executing multiple SQL statements to the database at once. It is often faster,
    /// and it lets the SQL engine treat many SQL statements as a single unit, which has many benefits. Do not
    /// use transactions for single SQL statements, as an individual SQL statement is already its own transaction.
    /// </remarks>
    public class TransactionHelper : IDisposable {
        /// <summary>
        /// The transaction this object is representing.
        /// </summary>
        DbTransaction _trans = null;

        /// <summary>
        /// Whether this object has been disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Constructs a TransactionHelper object and begins the transaction.
        /// </summary>
        /// <param name="conn"> The database connection to use. </param>
        /// <exception cref="DbException"> Thrown if an error occurs while beginning a transaction. </exception>
        public TransactionHelper(DbConnection conn) {
            try {
                _trans = conn.BeginTransaction();
            }
            catch (DbException) {
                if (_trans != null) {
                    _trans.Dispose();
                    _trans = null;
                }
                throw;
            }
        }

        /// <summary>
        /// Commits any changes made during this transaction.
        /// </summary>
        /// <remarks>
        /// This method is for callers that require fine-grained control over when changes should be committed
        /// to the database.
        /// </remarks>
        /// <exception cref="DbException"> Thrown if an error occurs while committing changes. </exception>
        public void Commit() {
            try {
                _trans.Commit();
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Rolls back to a previous state, undoing any changes made after the last commit. If no commit has
        /// been made, it rolls back to the start of the transaction.
        /// </summary>
        /// <remarks>
        /// This method is for callers that require fine-grained control over when rollbacks should occur.
        /// </remarks>
        /// <exception cref="DbException"> Thrown if an error occurs while rolling back. </exception>
        public void Rollback() {
            try {
                _trans.Rollback();
            }
            catch (DbException) {
                throw;
            }
        }

        /// <summary>
        /// Commits any changes, and automatically rolls back if an error occurs while committing changes.
        /// </summary>
        /// <returns> Whether committing changes was successful. The return value is false if a rollback occurred. </returns>
        /// <exception cref="DbException"> Thrown if an error occurs during the rollback. </exception>
        public bool CommitOrRollback() {
            try {
                _trans.Commit();
                return true;
            }
            catch (DbException) {
                try {
                    _trans.Rollback();
                    return false;
                }
                catch (DbException) {
                    throw;
                }
            }
        }

        /// <summary>
        /// Releases all resources that this TransactionHelper object is using.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources that this TransactionHelper object is using.
        /// </summary>
        /// <param name="disposing"> Whether this method was called from Dispose(). </param>
        protected virtual void Dispose(bool disposing) {
            // Implementators of IDisposable should ignore additional calls to Dispose() after the first one.
            if (_disposed) {
                return;
            }

            // Release all managed resources.
            if (disposing) {
                if (_trans != null) {
                    _trans.Dispose();
                    _trans = null;
                }
            }

            // Mark that this object has been disposed.
            _disposed = true;
        }
    }
}

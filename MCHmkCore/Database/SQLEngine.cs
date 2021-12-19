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

namespace MCHmk.SQL {
    /// <summary>
    /// The SQLEngine enumeration represents the list of database engines that MCHmk supports.
    /// </summary>
    public enum SQLEngine {
        /// <summary>
        /// No database engine. An invalid value.
        /// </summary>
        None = 0,
        /// <summary>
        /// The MySQL database engine.
        /// </summary>
        MySQL = 1,
        /// <summary>
        /// The SQLite database engine.
        /// </summary>
        SQLite = 2
    }
}

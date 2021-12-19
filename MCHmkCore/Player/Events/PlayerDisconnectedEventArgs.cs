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

namespace MCHmk {
    /// <summary>
    /// Provides data for the PlayerDisconnected event, which occurs when a player disconnects from the server.
    /// </summary>
    public class PlayerDisconnectedEventArgs : EventArgs {
        /// <summary>
        /// The reason for the player disconnecting. This is often used to provide a reason for kicking a player.
        /// </summary>
        private string _reason;

        /// <summary>
        /// Gets or sets the reason for the player disconnecting.
        /// </summary>
        public string Reason {
            get {
                return _reason;
            }
            set {
                _reason = value;
            }
        }

        /// <summary>
        /// Constructs a new PlayerDisconnectedEventArgs instance.
        /// </summary>
        /// <param name="reason"> The reason for the player disconnecting. </param>
        public PlayerDisconnectedEventArgs(string reason) {
            _reason = reason;
        }
    }
}

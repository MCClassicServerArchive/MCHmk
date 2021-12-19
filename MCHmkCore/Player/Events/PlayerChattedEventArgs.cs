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
    /// Provides data for the PlayerChatted event, which occurs when a player sends a message to chat.
    /// </summary>
    public class PlayerChattedEventArgs : EventArgs {
        /// <summary>
        /// The message that was sent to chat.
        /// </summary>
        private string _message;

        /// <summary>
        /// Gets or sets the message that was sent to chat.
        /// </summary>
        public string Message {
            get {
                return _message;
            }
            set {
                _message = value;
            }
        }

        /// <summary>
        /// Constructs an instance of the PlayerChattedEventArgs class.
        /// </summary>
        /// <param name="message"> The message that was sent to chat. </param>
        public PlayerChattedEventArgs(string message) {
            _message = message;
        }
    }
}

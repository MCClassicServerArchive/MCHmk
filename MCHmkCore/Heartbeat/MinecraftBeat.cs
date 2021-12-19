/*
    Copyright 2016 Jjp137

    This file has been changed from the original source code by MCForge.

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

/*
    Copyright 2012 MCForge

    Dual-licensed under the	Educational Community License, Version 2.0 and
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
using System.IO;
using System.Security;
using System.Web;

namespace MCHmk {
    /// <summary>
    /// The MinecraftBeat class contains information necessary to send a heartbeat to the minecraft.net server.
    /// </summary>
    public sealed class MinecraftBeat : IBeat {
        /// <summary>
        /// Whether a URL for the server was received already.
        /// </summary>
        public bool UrlSaid = false;

        /// <summary>
        /// Gets minecraft.net's heartbeat URL.
        /// </summary>
        public string URL {
            get {
                return "https://minecraft.net/heartbeat.jsp";
            }
        }

        /// <summary>
        /// Gets whether the minecraft.net heartbeat needs to be resent periodically.
        /// </summary>
        public bool Persistance {
            get {
                return true;
            }
        }

        /// <summary>
        /// A reference to the server associated with this heartbeat.
        /// </summary>
        private Server _s;

        /// <summary>
        /// Constructs a MinecraftBeat object.
        /// </summary>
        /// <param name="s"> The server associated with this heartbeat. </param>
        internal MinecraftBeat(Server s) { 
            _s = s;
        }

        /// <summary>
        /// Obtains a string of parameters to send to the minecraft.net server.
        /// </summary>
        /// <returns> The parameters to send as part of the heartbeat. </returns>
        public string Prepare() {
            return "&port=" + _s.props.port +
                   "&max=" + _s.props.maxPlayers +
                   "&name=" + HttpUtility.UrlEncode(_s.props.name) +
                   "&public=" + _s.props.pub +
                   "&version=7" +
                   "&salt=" + _s.props.minecraftSalt +
                   "&users=" + _s.players.Count;
        }

        /// <summary>
        /// Called when a response from the minecraft.net server is received.
        /// </summary>
        /// <param name="response"> The server's response. </param>
        public void OnResponse(string response) {
            string newUrl = response.Trim();

            // Do not do anything if the response is empty.
            if (String.IsNullOrEmpty(newUrl)) {
                return;
            }

            // If we don't have a URL yet or if it changed, that means that the minecraft.net URL
            // has to be updated on the server's end.
            if (String.IsNullOrEmpty(_s.URL) || !newUrl.Equals(_s.URL)) {
                _s.URL = response;

                // Write the new URL to a text file and tell the server owner what the URL is.
                try {
                    File.WriteAllText(Path.Combine("text", "externalurl.txt"), _s.URL);
                }
                catch (Exception e) {
                    if (e is IOException || e is UnauthorizedAccessException || e is PathTooLongException || 
                        e is DirectoryNotFoundException || e is SecurityException) {
                        ;  // Swallow the exception since it is a minor file.
                    }
                    else {
                        throw;
                    }
                }

                if (UrlSaid == false) {
                    _s.logger.Log("Minecraft.net URL found: " + _s.URL);
                    UrlSaid = true;
                }
            }
        }
    }
}

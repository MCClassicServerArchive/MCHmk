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

namespace MCHmk {
    /// <summary>
    /// The IBeat interface provides a common interface for objects that work with specific types
    /// of heartbeats.
    /// </summary>
    public interface IBeat {
        /// <summary>
        /// Gets or sets the URL that the heartbeat will be sent to.
        /// </summary>
        string URL {
            get;
        }

        /// <summary>
        /// Obtains a string of parameters to send to the server.
        /// </summary>
        /// <returns> The parameters to send as part of the heartbeat. </returns>
        string Prepare();

        /// <summary>
        /// Gets whether the heartbeat needs to be resent periodically.
        /// </summary>
        bool Persistance {
            get;
        }

        /// <summary>
        /// Called when a response is recieved.
        /// </summary>
        /// <param name="response"> The server's response. </param>
        void OnResponse(string response);
    }
}

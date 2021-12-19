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
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;

namespace MCHmk {
    /// <summary>
    /// The Heart class provides methods for working with server heartbeats.
    /// </summary>
    public class HeartbeatPumper {
        /// <summary>
        /// The maximum number of tries before sending a heartbeat is canceled.
        /// </summary>
        private const int MAX_RETRIES = 3;

        /// <summary>
        /// Gets or sets whether heartbeats are allowed to be sent.
        /// </summary>
        public bool CanBeat {
            get;
            set;
        }

        /// <summary>
        /// The Timer object that periodically sends out heartbeats.
        /// </summary>
        private Timer beatTimer;

        /// <summary>
        /// An array of heartbeats that MCHmk sends periodically.
        /// </summary>
        private readonly IBeat[] beats;

        /// <summary>
        /// A reference to the server.
        /// </summary>
        private Server _s;

        /// <summary>
        /// The static constructor for the Heart class. Responsible for starting a timer that
        /// periodically sends all known heartbeats to their respective servers.
        /// </summary>
        // TODO: move into Init()
        internal HeartbeatPumper(Server s) {
            // Obtain a reference to the server.
            _s = s;

            // Constructor the heartbeats.
            beats = new IBeat[] {/*new MinecraftBeat(s),*/ new ClassiCubeBeat(s)};

            // Start the timer.
            // TODO: dispose this timer later
            beatTimer = new Timer(OnBeat, null, 45000, 45000);
        }

        /// <summary>
        /// Sends each heartbeat to its respective servers.
        /// </summary>
        /// <param name="state"> Not used. </param>
        private void OnBeat(object state) {
            for (int i = 0; i < beats.Length; i++) {
                // Only send heartbeats that are meant to be sent periodically.
                if (beats[i].Persistance) {
                    Pump(beats[i]);
                }
            }
        }

        /// <summary>
        /// Sends the initial heartbeats.
        /// </summary>
        public void Init() {
            // Allow heartbeats to be sent.
            CanBeat = true;

            // Send each heartbeat.
            for (int i = 0; i < beats.Length; i++) {
                Pump(beats[i]);
            }
        }

        /// <summary>
        /// Send a heartbeat to the server.
        /// </summary>
        /// <param name="beat"> Information about the heartbeat to send. <seealso cref="IBeat"/></param>
        public void Pump(IBeat beat) {
            // Do not do anything if the server has not really started yet.
            if (!CanBeat) {
                return;
            }

            // Obtain the parameters to send as part of the request.
            byte[] data = Encoding.ASCII.GetBytes(beat.Prepare());

            for (int i = 0; i < MAX_RETRIES; i++) {
                try {
                    // Create a request using the URL provided in the heartbeat object, and
                    // modify some parameters in the header.
                    var request = WebRequest.Create(beat.URL) as HttpWebRequest;
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    request.Timeout = 15000;
                    request.ContentLength = data.Length;

                    // Send the request.
                    using (var writer = request.GetRequestStream()) {
                        writer.Write(data, 0, data.Length);
                    }

                    // Obtain a response from the server and let the heartbeat object interpret the result.
                    using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                        string read = reader.ReadToEnd().Trim();
                        beat.OnResponse(read);
                    }

                    // And we're done.
                    return;
                }
                catch (WebException) {
                    continue;  // Sometimes, network hiccups occur.
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                    continue;
                }
            }
        }
    }
}

/*
    Copyright 2016 Jjp137

    This file contains code from MCForge-Redux.

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
using System.Net;

namespace MCHmk {
    public static class NetworkUtil {
        /// <summary>
        /// Gets the external IP address of the server.
        /// </summary>
        /// <returns> The external IP address. </returns>
        public static string GetExternalIP() {
            try {
                using (WebClient w = new WebClient()) {
                    return w.DownloadString("http://checkip.amazonaws.com/").Trim();
                }
            }
            catch (WebException) {
                throw;
            }
        }

        /// <summary>
        /// Checks if the IP address connecting to the server is in a private range.
        /// </summary>
        /// <param name="ip"> The IP address to check. </param>
        /// <returns> Whether the IP address is a private one. </returns>
        public static bool IPInPrivateRange(string ip) {
            // These are private:
            // - 127.0.0.1/8
            // - 10.0.0.0/8 (10.0.0.0 to 10.255.255.255)
            // - 172.16.0.0/12 (172.16.0.0 to 172.31.255.255)
            // - 192.168.0.0/16 (192.168.0.0 to 192.168.255.255)
            if (ip.StartsWith("172.") && (int.Parse(ip.Split('.')[1]) >= 16 && int.Parse(ip.Split('.')[1]) <= 31)) {
                return true;
            }
            return IPAddress.IsLoopback(IPAddress.Parse(ip)) || ip.StartsWith("192.168.") || ip.StartsWith("10.");
        }

        /// <summary>
        /// Reverses the order of the bytes in an unsigned 16-bit integer, and returns the result
        /// as a byte array. Use before sending out packet data.
        /// </summary>
        /// <param name="x"> The unsigned 16-bit integer to reverse. </param>
        /// <returns> The byte array with the order of the integer's bytes reversed. </returns>
        public static byte[] HostToNetworkOrder(ushort x) {
            byte[] y = BitConverter.GetBytes(x);
            Array.Reverse(y);
            return y;
        }

        /// <summary>
        /// Obtains two bytes from an array of bytes, flips the order, and returns the result as
        /// an unsigned 16-bit integer. Use before reading packet data.
        /// </summary>
        /// <param name="x"> The array of bytes to read. </param>
        /// <param name="offset"> The location of the first byte to read from the array. </param>
        /// <returns> The unsigned 16-bit integer that results. </returns>
        public static ushort NetworkToHostOrder(byte[] x, int offset) {
            byte[] y = new byte[2];
            Buffer.BlockCopy(x, offset, y, 0, 2);
            Array.Reverse(y);
            return BitConverter.ToUInt16(y, 0);
        }

        /// <summary>
        /// Reverses the order of the bytes in an signed 16-bit integer, and returns the result
        /// as a byte array. Use before sending out packet data.
        /// </summary>
        /// <param name="x"> The signed 16-bit integer to reverse. </param>
        /// <returns> The byte array with the order of the integer's bytes reversed. </returns>
        public static byte[] HostToNetworkOrder(short x) {
            byte[] y = BitConverter.GetBytes(x);
            Array.Reverse(y);
            return y;
        }
    }
}


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
Copyright (C) 2010-2013 David Mitchell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
namespace MCHmk {
    public static class ConvertDat {
        // TODO: remove Server parameter eventually
        public static Level Load(Server svr, Stream lvlStream, string fileName) {
            byte[] temp = new byte[8];
            using (Level lvl = new Level(svr, fileName, 0, 0, 0, "empty")) {
                byte[] data;
                int length;
                try {
                    lvlStream.Seek(-4, SeekOrigin.End);
                    lvlStream.Read(temp, 0, sizeof(int));
                    lvlStream.Seek(0, SeekOrigin.Begin);
                    length = BitConverter.ToInt32(temp, 0);
                    data = new byte[length];
                    using (GZipStream reader = new GZipStream(lvlStream, CompressionMode.Decompress, true)) {
                        reader.Read(data, 0, length);
                    }

                    for (int i = 0; i < length - 1; i++) {
                        if (data[i] == 0xAC && data[i + 1] == 0xED) {

                            // bypassing the header crap
                            int pointer = i + 6;
                            Array.Copy(data, pointer, temp, 0, sizeof(short));
                            pointer += IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                            pointer += 13;

                            int headerEnd = 0;
                            // find the end of serialization listing
                            for (headerEnd = pointer; headerEnd < data.Length - 1; headerEnd++) {
                                if (data[headerEnd] == 0x78 && data[headerEnd + 1] == 0x70) {
                                    headerEnd += 2;
                                    break;
                                }
                            }

                            // start parsing serialization listing
                            int offset = 0;
                            while (pointer < headerEnd) {
                                if (data[pointer] == 'Z') {
                                    offset++;
                                }
                                else if (data[pointer] == 'I' || data[pointer] == 'F') {
                                    offset += 4;
                                }
                                else if (data[pointer] == 'J') {
                                    offset += 8;
                                }

                                pointer += 1;
                                Array.Copy(data, pointer, temp, 0, sizeof(short));
                                short skip = IPAddress.HostToNetworkOrder(BitConverter.ToInt16(temp, 0));
                                pointer += 2;

                                // look for relevant variables
                                Array.Copy(data, headerEnd + offset - 4, temp, 0, sizeof(int));
                                if (MemCmp(data, pointer, "width")) {
                                    lvl.width = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                                }
                                else if (MemCmp(data, pointer, "depth")) {
                                    lvl.height = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                                }
                                else if (MemCmp(data, pointer, "height")) {
                                    lvl.depth = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(temp, 0));
                                }

                                pointer += skip;
                            }

                            lvl.spawnx = (ushort)(lvl.width / 1.3);
                            lvl.spawny = (ushort)(lvl.height / 1.3);
                            lvl.spawnz = (ushort)(lvl.depth / 1.3);

                            // find the start of the block array
                            bool foundBlockArray = false;
                            offset = Array.IndexOf<byte>(data, 0x00, headerEnd);
                            while (offset != -1 && offset < data.Length - 2) {
                                if (data[offset] == 0x00 && data[offset + 1] == 0x78 && data[offset + 2] == 0x70) {
                                    foundBlockArray = true;
                                    pointer = offset + 7;
                                }
                                offset = Array.IndexOf<byte>(data, 0x00, offset + 1);
                            }

                            // copy the block array... or fail
                            if (foundBlockArray) {
                                CopyBlocks(lvl, data, pointer);
                                lvl.Save(true);
                            }
                            else {
                                throw new Exception("Could not locate block array.");
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex) {
                    svr.logger.Log("Conversion failed");
                    svr.logger.ErrorLog(ex);
                    return null;
                }

                return lvl;
            }
        }

        private static bool MemCmp( byte[] data, int offset, string value ) {
            for( int i = 0; i < value.Length; i++ ) {
                if( offset + i >= data.Length || data[offset + i] != value[i] ) {
                    return false;
                }
            }
            return true;
        }        

        /// <summary>
        /// Given an array of bytes representing data from a .dat file, copy the data over into
        /// MCHmk's level format.
        /// </summary>
        /// <remarks>
        /// This method is only used when converting .dat files to MCHmk's level format, and it should not
        /// be called in any other circumstances.
        /// </remarks>
        /// <param name="lvl"> The level to copy the bytes into. </param>
        /// <param name="source"> The array of bytes representing the data of the .dat file. </param>
        /// <param name="offset"> The position in the array to start reading from. </param>
        private static void CopyBlocks(Level lvl, byte[] source, int offset) {
            // Copy the source data to the level's block array.
            lvl.blocks = new BlockId[lvl.width * lvl.height * lvl.depth];
            Array.Copy(source, offset, lvl.blocks, 0, lvl.blocks.Length);

            // Check each block for some special cases.
            for (int i = 0; i < lvl.blocks.Length; i++) {
                // Block ids over 49 are not in the vanilla Classic server.
                if (Convert.ToInt32(lvl.blocks[i]) >= 50) {
                    lvl.blocks[i] = 0;
                }
                // For some reason, we swap the active-ness of liquids. Why? - Jjp137
                switch (lvl.blocks[i]) {
                    case BlockId.StillWater:
                        lvl.blocks[i] = BlockId.ActiveWater;
                        break;
                    case BlockId.ActiveWater:
                        lvl.blocks[i] = BlockId.StillWater;
                        break;
                    case BlockId.ActiveLava:
                        lvl.blocks[i] = BlockId.StillLava;
                        break;
                    case BlockId.StillLava:
                        lvl.blocks[i] = BlockId.ActiveLava;
                        break;
                }
            }
        }
    }
}

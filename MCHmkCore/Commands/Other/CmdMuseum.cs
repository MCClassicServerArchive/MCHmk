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
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;

namespace MCHmk.Commands {
    public sealed class CmdMuseum : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"musea", "map", "lvl", "level"});

        public override string Name {
            get {
                return "museum";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "other";
            }
        }
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }
        public override bool MuseumUsable {
            get {
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }
        public CmdMuseum(Server s) : base(s) { }

        public override void Use(Player p, string args) {

            string path;
            string extension = ".mcf";  // Default to .mcf

            if (args.Split(' ').Length == 1) {
                path = "levels/" + args;
            }
            else if (args.Split(' ').Length == 2) {
                try {
                    path = @_s.props.backupLocation + "/" + args.Split(' ')[0] + "/" + args.Split(' ')[1] + "/" +
                        args.Split(' ')[0];
                }
                catch {  // TODO: find exact exception to check
                    Help(p);
                    return;
                }
            }
            else {
                Help(p);
                return;
            }

            // Try finding a .lvl file instead if an .mcf file doesn't exist.
            if (!File.Exists(path + extension)) {
                extension = ".lvl";
            }

            if (File.Exists(path + extension)) {
                _s.logger.Log(path + extension);
                FileStream fs = File.OpenRead(path + extension);
                try {

                    GZipStream gs = new GZipStream(fs, CompressionMode.Decompress);
                    byte[] ver = new byte[2];
                    gs.Read(ver, 0, ver.Length);
                    ushort version = BitConverter.ToUInt16(ver, 0);
                    ushort[] vars = new ushort[6];
                    byte[] rot = new byte[2];

                    if (version == 1874) {
                        byte[] header = new byte[16];
                        gs.Read(header, 0, header.Length);

                        vars[0] = BitConverter.ToUInt16(header, 0);
                        vars[1] = BitConverter.ToUInt16(header, 2);
                        vars[2] = BitConverter.ToUInt16(header, 4);
                        vars[3] = BitConverter.ToUInt16(header, 6);
                        vars[4] = BitConverter.ToUInt16(header, 8);
                        vars[5] = BitConverter.ToUInt16(header, 10);

                        rot[0] = header[12];
                        rot[1] = header[13];

                        //level.permissionvisit = (LevelPermission)header[14];
                        //level.permissionbuild = (LevelPermission)header[15];
                    }
                    else {
                        byte[] header = new byte[12];
                        gs.Read(header, 0, header.Length);

                        vars[0] = version;
                        vars[1] = BitConverter.ToUInt16(header, 0);
                        vars[2] = BitConverter.ToUInt16(header, 2);
                        vars[3] = BitConverter.ToUInt16(header, 4);
                        vars[4] = BitConverter.ToUInt16(header, 6);
                        vars[5] = BitConverter.ToUInt16(header, 8);

                        rot[0] = header[10];
                        rot[1] = header[11];
                    }

                    Level level = new Level(_s, Name, vars[0], vars[2], vars[1], "empty");
                    level.setPhysics(0);

                    level.spawnx = vars[3];
                    level.spawnz = vars[4];
                    level.spawny = vars[5];
                    level.rotx = rot[0];
                    level.roty = rot[1];

                    // Copied from Level.cs
                    byte[] tempBlock = new byte[extension == ".lvl" ? 1 : 2];

                    if (extension == ".mcf") {
                        for (int i = 0; i < level.blocks.Length; ++i) {
                            gs.Read(tempBlock, 0, tempBlock.Length);
                            level.blocks[i] = (BlockId)(BitConverter.ToUInt16(tempBlock, 0));
                        }
                    }
                    else {
                        for (int i = 0; i < level.blocks.Length; ++i) {
                            gs.Read(tempBlock, 0, tempBlock.Length);
                            level.blocks[i] = (BlockId)tempBlock[0];
                        }
                    }

                    gs.Close();

                    level.backedup = true;
                    level.permissionbuild = DefaultRankValue.Admin;

                    level.jailx = (ushort)(level.spawnx * 32);
                    level.jaily = (ushort)(level.spawny * 32);
                    level.jailz = (ushort)(level.spawnz * 32);
                    level.jailrotx = level.rotx;
                    level.jailroty = level.roty;

                    p.Loading = true;
                    foreach (Player pl in _s.players) if (p.level == pl.level && p != pl) {
                            p.SendDie(pl.serverId);
                        }
                    foreach (PlayerBot b in PlayerBot.playerbots) if (p.level == b.level) {
                            p.SendDie(b.id);
                        }

                    _s.GlobalDie(p, true);

                    p.level = level;
                    p.SendMotd();

                    p.SendRaw(OpCode.MapBegin);
                    byte[] buffer = new byte[level.blocks.Length + 4];
                    BitConverter.GetBytes(IPAddress.HostToNetworkOrder(level.blocks.Length)).CopyTo(buffer, 0);

                    // Copied from Player.cs
                    for (int i = 0; i < level.blocks.Length; ++i) {
                        if (p.extension) {
                            buffer[4 + i] = (byte)BlockData.Convert( level.blocks[i] );
                        }
                        else {
                            buffer[4 + i] = (byte)BlockData.Convert( BlockData.ConvertCPE( level.blocks[i] ) );
                        }
                    }

                    buffer = buffer.GZip();
                    int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
                    for (int i = 1; buffer.Length > 0; ++i) {
                        short length = (short)Math.Min(buffer.Length, 1024);
                        byte[] send = new byte[1027];
                        NetworkUtil.HostToNetworkOrder(length).CopyTo(send, 0);
                        Buffer.BlockCopy(buffer, 0, send, 2, length);
                        byte[] tempbuffer = new byte[buffer.Length - length];
                        Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
                        buffer = tempbuffer;
                        send[1026] = (byte)(i * 100 / number);
                        p.SendRaw(OpCode.MapChunk, send);
                        Thread.Sleep(10);
                    }
                    buffer = new byte[6];
                    NetworkUtil.HostToNetworkOrder((short)level.width).CopyTo(buffer, 0);
                    NetworkUtil.HostToNetworkOrder((short)level.height).CopyTo(buffer, 2);
                    NetworkUtil.HostToNetworkOrder((short)level.depth).CopyTo(buffer, 4);
                    p.SendRaw(OpCode.MapEnd, buffer);

                    ushort x = (ushort)((0.5 + level.spawnx) * 32);
                    ushort y = (ushort)((1 + level.spawny) * 32);
                    ushort z = (ushort)((0.5 + level.spawnz) * 32);

                    p.aiming = false;
                    _s.GlobalSpawn(p, x, y, z, level.rotx, level.roty, true);
                    p.ClearSelection();
                    p.Loading = false;

                    if (args.IndexOf(' ') == -1) {
                        level.name = "&cMuseum " + _s.props.DefaultColor + "(" + args.Split(' ')[0] + ")";
                    }
                    else {
                        level.name = "&cMuseum " + _s.props.DefaultColor + "(" + args.Split(' ')[0] + " " + args.Split(' ')[1] + ")";
                    }

                    if (!p.hidden) {
                        _s.GlobalMessage(p.color + p.prefix + p.name + _s.props.DefaultColor + " went to the " + level.name);
                    }
                }
                catch (Exception ex) {
                    p.SendMessage("Error loading level.");
                    _s.logger.ErrorLog(ex);
                    return;
                }
                finally {
                    fs.Close();
                }
            }
            else {
                p.SendMessage("Level or backup could not be found.");
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /museum.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/museum <map?> <backup?> - Allows you to access a specific " +
                               "backup of the specified map.");
            p.SendMessage("Works on offline maps.");
        }
    }
}

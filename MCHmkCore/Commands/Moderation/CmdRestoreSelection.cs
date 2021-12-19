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
	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;

namespace MCHmk.Commands {
    public class CmdRestoreSelection : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used to draw the cuboid.
        /// </summary>
        private static readonly string _backupKey = "rs_backup";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"restore", "select"});

        public override string Name {
            get {
                return "rs";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "mod";
            }
        }
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }
        public override bool MuseumUsable {
            get {
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }

        public CmdRestoreSelection(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                if (File.Exists(@_s.props.backupLocation + "/" + p.level.name + "/" + args + "/" + p.level.name + ".mcf")) {
                    try {
                        Dictionary<string, object> data = new Dictionary<string, object>();
                        data[_backupKey] = int.Parse(args);

                        const string prompt = "Place two blocks to select the area to be restored.";
                        TwoBlockSelection.Start(p, data, prompt, SelectionFinished);

                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        _s.logger.Log("Restore fail");
                    }
                }
                else {
                    p.SendMessage("Backup '" + args + "' does not exist.");
                }
            }
            else {
                Help(p);
            }
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the area to restore.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            int backup = c.GetData<int>(_backupKey);

            FileStream fs = File.OpenRead(@_s.props.backupLocation + "/" + p.level.name + "/" + backup + "/" + p.level.name + ".mcf");
            GZipStream gs = new GZipStream(fs, CompressionMode.Decompress);
            byte[] ver = new byte[2];
            gs.Read(ver, 0, ver.Length);
            ushort version = BitConverter.ToUInt16(ver, 0);
            ushort[] vars = new ushort[6];
            try {
                if (version == 1874) {
                    byte[] header = new byte[16];
                    gs.Read(header, 0, header.Length);

                    vars[0] = BitConverter.ToUInt16(header, 0);
                    vars[1] = BitConverter.ToUInt16(header, 2);
                    vars[2] = BitConverter.ToUInt16(header, 4);
                }
                else {
                    byte[] header = new byte[12];
                    gs.Read(header, 0, header.Length);

                    vars[0] = version;
                    vars[1] = BitConverter.ToUInt16(header, 0);
                    vars[2] = BitConverter.ToUInt16(header, 2);
                }
                byte[] blocks = new byte[vars[0] * vars[2] * vars[1]];
                gs.Read(blocks, 0, blocks.Length);
                gs.Dispose();
                fs.Dispose();

                if (blocks.Length != p.level.blocks.Length) {
                    p.SendMessage("Cant restore selection of different size maps.");
                    blocks = null;
                    return;
                }

                if (p.level.bufferblocks && !p.level.Instant) {
                    for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx) {
                        for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                            for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                                _s.blockQueue.Addblock(p, xx, yy, zz, (BlockId)blocks[xx + (zz * vars[0]) + (yy * vars[0] * vars[1])]);
                            }
                        }
                    }
                }
                else {
                    for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx) {
                        for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                            for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                                p.level.Blockchange(p, xx, yy, zz, (BlockId)blocks[xx + (zz * vars[0]) + (yy * vars[0] * vars[1])]);
                            }
                        }
                    }
                }

                blocks = null;

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _backupKey);
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                _s.logger.Log("Restore selection failed");
            }
        }

        /// <summary>
        /// Called when /help is used on /restoreselection.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/restoreselection <number?/backupname?> - Restores a " +
                               "portion of a map to what it looked like in a previous backup.");
        }
    }
}

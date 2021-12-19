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
	Copyright © 2011-2014 MCForge-Redux

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

namespace MCHmk.Commands {
    public class CmdSpheroid : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used to draw the spheroid.
        /// </summary>
        private readonly string _blockKey = "spheroid_block";
        /// <summary>
        /// Name of the key used to store and retrieve whether the spheroid is a vertical one.
        /// </summary>
        private readonly string _verticalKey = "spheroid_vertical";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"sphere"});

        public override string Name {
            get {
                return "spheroid";
            }
        }
        public override string Shortcut {
            get {
                return "e";
            }
        }
        public override string Type {
            get {
                return "build";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdSpheroid(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            BlockId type;
            bool vertical;

            if (args == String.Empty) {
                type = BlockId.Null;
                vertical = false;
            }
            else if (args.IndexOf(' ') == -1) {
                type = BlockData.Ushort(args);
                vertical = false;
                if (args.ToLower() != "vertical" && !_s.blockPerms.CanPlace(p, type)) {
                    p.SendMessage("Cannot place that.");
                    return;
                }
                if (type == BlockId.Null) {
                    if (args.ToLower() == "vertical") {
                        vertical = true;
                    }
                    else {
                        Help(p);
                        return;
                    }
                }
            }
            else {
                type = BlockData.Ushort(args.Split(' ')[0]);
                if (!_s.blockPerms.CanPlace(p, type)) {
                    p.SendMessage("Cannot place that.");
                    return;
                }
                if (type == BlockId.Null || args.Split(' ')[1].ToLower() != "vertical") {
                    Help(p);
                    return;
                }
                vertical = true;
            }

            if (!_s.blockPerms.CanPlace(p, type) && type != BlockId.Null) {
                p.SendMessage("Cannot place this block type!");
                return;
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = type;
            data[_verticalKey] = vertical;

            const string prompt = "Place two blocks to determine the spheroid's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the spheroid to be drawn.
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

            BlockId type = c.GetData<BlockId>(_blockKey);
            bool vertical = c.GetData<bool>(_verticalKey);

            if (type == BlockId.Null) {
                type = c.BlockType;
            }
            List<UShortCoords> buffer = new List<UShortCoords>();

            if (!vertical) {
                // Courtesy of fCraft's awesome Open-Source'ness :D 

                // find start/end coordinates
                int sx = Math.Min(x1, x2);
                int ex = Math.Max(x1, x2);
                int sy = Math.Min(y1, y2);
                int ey = Math.Max(y1, y2);
                int sz = Math.Min(z1, z2);
                int ez = Math.Max(z1, z2);

                // find center points
                double cx = (ex + sx) / 2 + (((ex + sx) % 2 == 1) ? 0.5 : 0);
                double cy = (ey + sy) / 2 + (((ey + sy) % 2 == 1) ? 0.5 : 0);
                double cz = (ez + sz) / 2 + (((ez + sz) % 2 == 1) ? 0.5 : 0);

                // find axis lengths
                double rx = Convert.ToDouble(ex) - cx + 0.25;
                double ry = Convert.ToDouble(ey) - cy + 0.25;
                double rz = Convert.ToDouble(ez) - cz + 0.25;

                double rx2 = 1 / (rx * rx);
                double ry2 = 1 / (ry * ry);
                double rz2 = 1 / (rz * rz);

                int totalBlocks = (int)(Math.PI * 0.75 * rx * ry * rz);

                if (totalBlocks > p.rank.maxBlocks) {
                    p.SendMessage("You tried to spheroid " + totalBlocks + " blocks.");
                    p.SendMessage("You cannot spheroid more than " + p.rank.maxBlocks + ".");
                    return;
                }

                p.SendMessage(totalBlocks + " blocks.");

                for (int xx = sx; xx <= ex; xx += 8)
                    for (int yy = sy; yy <= ey; yy += 8)
                        for (int zz = sz; zz <= ez; zz += 8)
                            for (int z3 = 0; z3 < 8 && zz + z3 <= ez; z3++)
                                for (int y3 = 0; y3 < 8 && yy + y3 <= ey; y3++)
                                    for (int x3 = 0; x3 < 8 && xx + x3 <= ex; x3++) {
                                    // get relative coordinates
                                    double dx = (xx + x3 - cx);
                                    double dy = (yy + y3 - cy);
                                    double dz = (zz + z3 - cz);

                                    // test if it's inside ellipse
                                    if ((dx * dx) * rx2 + (dy * dy) * ry2 + (dz * dz) * rz2 <= 1) {
                                        p.level.Blockchange(p, (ushort)(x3 + xx), (ushort)(yy + y3), (ushort)(zz + z3), type);
                                    }
                                }
            }
            else {
                // find start/end coordinates
                int sx = Math.Min(x1, x2);
                int ex = Math.Max(x1, x2);
                int sy = Math.Min(y1, y2);
                int ey = Math.Max(y1, y2);
                int sz = Math.Min(z1, z2);
                int ez = Math.Max(z1, z2);

                // find center points
                double cx = (ex + sx) / 2 + (((ex + sx) % 2 == 1) ? 0.5 : 0);
                double cz = (ez + sz) / 2 + (((ez + sz) % 2 == 1) ? 0.5 : 0);

                // find axis lengths
                double rx = Convert.ToDouble(ex) - cx + 0.25;
                double rz = Convert.ToDouble(ez) - cz + 0.25;

                double rx2 = 1 / (rx * rx);
                double rz2 = 1 / (rz * rz);
                double smallrx2 = 1 / ((rx - 1) * (rx - 1));
                double smallrz2 = 1 / ((rz - 1) * (rz - 1));

                UShortCoords pos = new UShortCoords();

                for (int xx = sx; xx <= ex; xx += 8)
                    for (int zz = sz; zz <= ez; zz += 8)
                        for (int z3 = 0; z3 < 8 && zz + z3 <= ez; z3++)
                            for (int x3 = 0; x3 < 8 && xx + x3 <= ex; x3++) {
                            // get relative coordinates
                            double dx = (xx + x3 - cx);
                            double dz = (zz + z3 - cz);

                            // test if it's inside ellipse
                            if ((dx * dx) * rx2 + (dz * dz) * rz2 <= 1 && (dx * dx) * smallrx2 + (dz * dz) * smallrz2 > 1) {
                                pos.X = (ushort)(x3 + xx);
                                pos.Y = (ushort)(sy);
                                pos.Z = (ushort)(zz + z3);
                                buffer.Add(pos);
                            }
                        }

                int ydiff = Math.Abs(y2 - y1) + 1;

                if (buffer.Count * ydiff > p.rank.maxBlocks) {
                    p.SendMessage("You tried to spheroid " + buffer.Count * ydiff + " blocks.");
                    p.SendMessage("You cannot spheroid more than " + p.rank.maxBlocks + ".");
                    return;
                }
                p.SendMessage(buffer.Count * ydiff + " blocks.");


                foreach (UShortCoords UShortCoords in buffer) {
                    for (ushort yy = (ushort)sy; yy <= (ushort)ey; yy++) {
                        p.level.Blockchange(p, UShortCoords.X, yy, UShortCoords.Z, type);
                    }
                }
            }

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _verticalKey);
        }

        void BufferAdd(List<UShortCoords> list, ushort x, ushort y, ushort z) {
            UShortCoords pos;
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
            list.Add(pos);
        }
        
        /// <summary>
        /// Called when /help is used on /spheroid.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/spheroid [block?] [vertical] - Create a spheroid of blocks.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
            p.SendMessage("The vertical parameter causes a vertical tube of " +
                               "blocks to be created.");
        }
    }
}

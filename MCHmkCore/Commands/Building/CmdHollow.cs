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
    public class CmdHollow : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type that will be hollowed around.
        /// </summary>
        private readonly string _blockKey = "hollow_block";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "create"});

        public override string Name {
            get {
                return "hollow";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
        public CmdHollow(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            BlockId countOther;

            if (args != String.Empty) {
                if (BlockData.Ushort(args.ToLower()) == BlockId.Null) {
                    p.SendMessage("Cannot find block entered.");
                    return;
                }
            }

            if (args != String.Empty) {
                countOther = BlockData.Ushort(args.ToLower());
            }
            else {
                countOther = BlockId.Null;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = countOther;

            const string prompt = "Place two blocks to determine the area's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the area to be hollowed.
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

            BlockId countOther = c.GetData<BlockId>(_blockKey);

            List<UShortCoords> buffer = new List<UShortCoords>();
            UShortCoords pos;

            bool AddMe = false;

            for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx)
                for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy)
                    for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                    AddMe = true;

                    if (!BlockData.RightClick(BlockData.Convert(p.level.GetTile(xx, yy, zz)), true)
                        && p.level.GetTile(xx, yy, zz) != countOther) {
                        if (BlockData.RightClick(BlockData.Convert(p.level.GetTile((ushort)(xx - 1), yy, zz)))
                            || p.level.GetTile((ushort)(xx - 1), yy, zz) == countOther) {
                            AddMe = false;
                        }
                        else if (BlockData.RightClick(BlockData.Convert(p.level.GetTile((ushort)(xx + 1), yy, zz)))
                                 || p.level.GetTile((ushort)(xx + 1), yy, zz) == countOther) {
                            AddMe = false;
                        }
                        else if (BlockData.RightClick(BlockData.Convert(p.level.GetTile(xx, (ushort)(yy - 1), zz)))
                                 || p.level.GetTile(xx, (ushort)(yy - 1), zz) == countOther) {
                            AddMe = false;
                        }
                        else if (BlockData.RightClick(BlockData.Convert(p.level.GetTile(xx, (ushort)(yy + 1), zz)))
                                 || p.level.GetTile(xx, (ushort)(yy + 1), zz) == countOther) {
                            AddMe = false;
                        }
                        else if (BlockData.RightClick(BlockData.Convert(p.level.GetTile(xx, yy, (ushort)(zz - 1))))
                                 || p.level.GetTile(xx, yy, (ushort)(zz - 1)) == countOther) {
                            AddMe = false;
                        }
                        else if (BlockData.RightClick(BlockData.Convert(p.level.GetTile(xx, yy, (ushort)(zz + 1))))
                                 || p.level.GetTile(xx, yy, (ushort)(zz + 1)) == countOther) {
                            AddMe = false;
                        }
                    }
                    else {
                        AddMe = false;
                    }

                    if (AddMe) {
                        pos.X = xx;
                        pos.Y = yy;
                        pos.Z = zz;
                        buffer.Add(pos);
                    }
                }

            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried to hollow more than " + buffer.Count + " blocks.");
                p.SendMessage("You cannot hollow more than " + p.rank.maxBlocks + ".");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey);
                return;
            }

            buffer.ForEach(delegate(UShortCoords pos1) {
                p.level.Blockchange(p, pos1.X, pos1.Y, pos1.Z, BlockId.Air);
            });

            p.SendMessage("You hollowed " + buffer.Count + " blocks.");

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey);
        }
        
        /// <summary>
        /// Called when /help is used on /hollow.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/hollow - Hollows out an area without flooding it.");
            p.SendMessage("/hollow <block?> - Hollows around the specified block.");
        }
    }
}

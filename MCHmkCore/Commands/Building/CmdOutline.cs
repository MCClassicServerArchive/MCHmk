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
    public class CmdOutline : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the first block type provided.
        /// </summary>
        private readonly string _firstKey = "outline_first";
        /// <summary>
        /// Name of the key used to store and retrieve the second block type provided.
        /// </summary>
        private readonly string _secondKey = "outline_second";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"out", "line", "layer"});

        public override string Name {
            get {
                return "outline";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdOutline(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            int number = args.Split(' ').Length;
            if (number != 2) {
                Help(p);
                return;
            }

            int pos = args.IndexOf(' ');
            string t = args.Substring(0, pos).ToLower();
            string t2 = args.Substring(pos + 1).ToLower();
            BlockId type = BlockData.Ushort(t);
            if (type == BlockId.Null) {
                p.SendMessage("There is no block \"" + t + "\".");
                return;
            }
            BlockId type2 = BlockData.Ushort(t2);
            if (type2 == BlockId.Null) {
                p.SendMessage("There is no block \"" + t2 + "\".");
                return;
            }
            if (!_s.blockPerms.CanPlace(p, type2)) {
                p.SendMessage("Cannot place that block type.");
                return;
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_firstKey] = type;
            data[_secondKey] = type2;

            const string prompt = "Place two blocks to determine the area's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the area to be affected.
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

            BlockId firstType = c.GetData<BlockId>(_firstKey);
            BlockId secondType = c.GetData<BlockId>(_secondKey);

            List<UShortCoords> buffer = new List<UShortCoords>();
            UShortCoords pos;

            bool AddMe = false;

            for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx)
                for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy)
                    for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                    AddMe = false;

                    if (p.level.GetTile((ushort)(xx - 1), yy, zz) == firstType) {
                        AddMe = true;
                    }
                    else if (p.level.GetTile((ushort)(xx + 1), yy, zz) == firstType) {
                        AddMe = true;
                    }
                    else if (p.level.GetTile(xx, (ushort)(yy - 1), zz) == firstType) {
                        AddMe = true;
                    }
                    else if (p.level.GetTile(xx, (ushort)(yy + 1), zz) == firstType) {
                        AddMe = true;
                    }
                    else if (p.level.GetTile(xx, yy, (ushort)(zz - 1)) == firstType) {
                        AddMe = true;
                    }
                    else if (p.level.GetTile(xx, yy, (ushort)(zz + 1)) == firstType) {
                        AddMe = true;
                    }

                    if (AddMe && p.level.GetTile(xx, yy, zz) != firstType) {
                        pos.X = xx;
                        pos.Y = yy;
                        pos.Z = zz;
                        buffer.Add(pos);
                    }
                }

            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried to outline more than " + buffer.Count + " blocks.");
                p.SendMessage("You cannot outline more than " + p.rank.maxBlocks + ".");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _firstKey, _secondKey);
                return;
            }

            buffer.ForEach(delegate(UShortCoords pos1) {
                p.level.Blockchange(p, pos1.X, pos1.Y, pos1.Z, secondType);
            });

            p.SendMessage("You outlined " + buffer.Count + " blocks.");

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _firstKey, _secondKey);
        }

        /// <summary>
        /// Called when /help is used on /outline.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/outline <block1?> <block2?> - Outlines anything made " +
                               "out of the first block with the second block.");
        }

    }
}

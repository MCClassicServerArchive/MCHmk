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
using System.Linq;

namespace MCHmk.Commands {
    public class CmdReplaceAll : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"all", "block", "replace", "lvl", "level", "map"});

        public override string Name {
            get {
                return "replaceall";
            }
        }
        public override string Shortcut {
            get {
                return "ra";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdReplaceAll(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string[] splitArgs = args.Split(' ');
            if (splitArgs.Length != 2) {
                p.SendMessage("Invalid number of arguments!");
                Help(p);
                return;
            }

            List<string> temp;

            if (splitArgs[0].Contains(",")) {
                temp = new List<string>(splitArgs[0].Split(','));
            }
            else
                temp = new List<string>() {
                splitArgs[0]
            };

            temp = temp.Distinct().ToList(); // Remove duplicates

            List<string> invalid = new List<string>(); //Check for invalid blocks
            foreach (string name in temp)
                if (BlockData.Ushort(name) == BlockId.Null) {
                    invalid.Add(name);
                }
            if (BlockData.Ushort(splitArgs[1]) == BlockId.Null) {
                invalid.Add(splitArgs[1]);
            }
            if (invalid.Count > 0) {
                p.SendMessage(String.Format("Invalid block{0}: {1}", invalid.Count == 1 ? String.Empty : "s", String.Join(", ",
                                            invalid.ToArray())));
                return;
            }
            if (temp.Contains(splitArgs[1])) {
                temp.Remove(splitArgs[1]);
            }
            if (temp.Count < 1) {
                p.SendMessage("Replacing a block with the same one would be pointless!");
                return;
            }

            List<BlockId> oldType = new List<BlockId>();
            foreach (string name in temp) {
                oldType.Add(BlockData.Ushort(name));
            }
            BlockId newType = BlockData.Ushort(splitArgs[1]);

            foreach (BlockId type in oldType)
                if (!_s.blockPerms.CanPlace(p, type) && !BlockData.BuildIn(type)) {
                    p.SendMessage("Cannot replace that.");
                    return;
                }
            if (!_s.blockPerms.CanPlace(p, newType)) {
                p.SendMessage("Cannot place that.");
                return;
            }

            ushort x, y, z;
            int currentBlock = 0;
            List<UShortCoords> stored = new List<UShortCoords>();
            UShortCoords pos;

            foreach (BlockId b in p.level.blocks) {
                if (oldType.Contains(b)) {
                    p.level.IntToPos(currentBlock, out x, out y, out z);
                    pos.X = x;
                    pos.Y = y;
                    pos.Z = z;
                    stored.Add(pos);
                }
                currentBlock++;
            }

            if (stored.Count > (p.rank.maxBlocks * 2)) {
                p.SendMessage("Cannot replace more than " + (p.rank.maxBlocks * 2) + " blocks.");
                return;
            }

            p.SendMessage(stored.Count + " blocks out of " + currentBlock + " will be replaced.");

            if (p.level.bufferblocks && !p.level.Instant) {
                foreach (UShortCoords coords in stored) {
                    _s.blockQueue.Addblock(p, coords.X, coords.Y, coords.Z, newType);
                }
            }
            else {
                foreach (UShortCoords coords in stored) {
                    p.level.Blockchange(p, coords.X, coords.Y, coords.Z, newType);
                }
            }

            p.SendMessage("&4/replaceall finished!");
        }

        /// <summary>
        /// Called when /help is used on /replaceall.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/replaceall <block1?> <block2?> - Replaces all of one type " +
                               "of blocks with another type in the current map.");
        }
    }
}

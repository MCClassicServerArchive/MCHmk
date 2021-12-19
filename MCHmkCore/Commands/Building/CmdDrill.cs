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
    public class CmdDrill : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the distance that will be drilled.
        /// </summary>
        private readonly string _distanceKey = "drill_distance";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"dig", "distance"});

        public override string Name {
            get {
                return "drill";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdDrill(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            int distance = 20;

            if (args != String.Empty)
                try {
                    distance = int.Parse(args);
                }
                catch {  // TODO: Find exact exception to catch
                    Help(p);
                    return;
                }

            p.ClearSelection();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_distanceKey] = distance;

            p.StartSelection(BlockSelected, data);
            p.SendMessage("Destroy the block you wish to drill into.");
        }

        private void BlockSelected(Player p, CommandTempData c) {
            // The player has selected a block, so stop listening for a block change.
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            BlockId oldType = p.level.GetTile(c.X, c.Y, c.Z);
            p.SendBlockchange(c.X, c.Y, c.Z, oldType);

            int distance = c.GetData<int>(_distanceKey);

            int diffX = 0, diffZ = 0;

            if (p.rot[0] <= 32 || p.rot[0] >= 224) {
                diffZ = -1;
            }
            else if (p.rot[0] <= 96) {
                diffX = 1;
            }
            else if (p.rot[0] <= 160) {
                diffZ = 1;
            }
            else {
                diffX = -1;
            }

            List<UShortCoords> buffer = new List<UShortCoords>();
            UShortCoords pos;
            int total = 0;

            if (diffX != 0) {
                for (ushort xx = x; total < distance; xx += (ushort)diffX) {
                    for (ushort yy = (ushort)(y - 1); yy <= (ushort)(y + 1); yy++) {
                        for (ushort zz = (ushort)(z - 1); zz <= (ushort)(z + 1); zz++) {
                            pos.X = xx;
                            pos.Y = yy;
                            pos.Z = zz;
                            buffer.Add(pos);
                        }
                        total++;
                    }
                }
            }
            else {
                for (ushort zz = z; total < distance; zz += (ushort)diffZ) {
                    for (ushort yy = (ushort)(y - 1); yy <= (ushort)(y + 1); yy++) {
                        for (ushort xx = (ushort)(x - 1); xx <= (ushort)(x + 1); xx++) {
                            pos.X = xx;
                            pos.Y = yy;
                            pos.Z = zz;
                            buffer.Add(pos);
                        }
                        total++;
                    }
                }
            }

            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried to drill " + buffer.Count + " blocks.");
                p.SendMessage("You cannot drill more than " + p.rank.maxBlocks + ".");

                HandleStaticMode(p, c);
                return;
            }

            foreach (UShortCoords coords in buffer) {
                if (p.level.GetTile(coords.X, coords.Y, coords.Z) == oldType) {
                    p.level.Blockchange(p, coords.X, coords.Y, coords.Z, BlockId.Air);
                }
            }
            p.SendMessage(buffer.Count + " blocks drilled.");

            HandleStaticMode(p, c);
        }

        private void HandleStaticMode(Player p, CommandTempData c) {
            if (!p.staticCommands) {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_distanceKey] = c.GetData<int>(_distanceKey);

            p.StartSelection(BlockSelected, data);
        }

        /// <summary>
        /// Called when /help is used on /drill.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/drill [distance?] - Drills a hole, which destroys all " +
                               "similar blocks in a 3x3 rectangle ahead of you.");
            p.SendMessage("The default distance is 20 blocks.");
        }
    }
}

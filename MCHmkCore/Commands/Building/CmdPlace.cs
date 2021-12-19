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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdPlace : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "pos"});

        public override string Name {
            get {
                return "place";
            }
        }
        public override string Shortcut {
            get {
                return "pl";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }
        public CmdPlace(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            BlockId b = BlockId.Null;
            ushort x = 0;
            ushort y = 0;
            ushort z = 0;

            x = (ushort)(p.pos[0] / 32);
            y = (ushort)((p.pos[1] / 32) - 1);
            z = (ushort)(p.pos[2] / 32);

            try {
                switch (args.Split(' ').Length) {
                case 0:
                    b = BlockId.Stone;
                    break;
                case 1:
                    b = BlockData.Ushort(args);
                    break;
                case 3:
                    x = Convert.ToUInt16(args.Split(' ')[0]);
                    y = Convert.ToUInt16(args.Split(' ')[1]);
                    z = Convert.ToUInt16(args.Split(' ')[2]);
                    break;
                case 4:
                    b = BlockData.Ushort(args.Split(' ')[0]);
                    x = Convert.ToUInt16(args.Split(' ')[1]);
                    y = Convert.ToUInt16(args.Split(' ')[2]);
                    z = Convert.ToUInt16(args.Split(' ')[3]);
                    break;
                default:
                    p.SendMessage("Invalid parameters");
                    return;
                }
            }
            catch {  // TODO: find exact exception to catch
                p.SendMessage("Invalid parameters");
                return;
            }

            if (b == BlockId.Null) {
                b = BlockId.Stone;
            }
            if (!_s.blockPerms.CanPlace(p, b)) {
                p.SendMessage("Cannot place that block type.");
                return;
            }

            if (y >= p.level.height) {
                y = (ushort)(p.level.height - 1);
            }

            if (!p.CheckBlockSpam()) {
                p.level.Blockchange(p, x, y, z, b);
                p.SendMessage("A block was placed at (" + x + ", " + y + ", " + z + ").");
            }
        }

        /// <summary>
        /// Called when /help is used on /place.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/place [block?] [x?] [y?] [z?] - Places a block at " +
                               "your feet or at the specified coordinates.");
        }
    }
}

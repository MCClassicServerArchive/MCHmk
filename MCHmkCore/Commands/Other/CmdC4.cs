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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdC4 : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"tnt", "c4", "explosion"});

        public override string Name {
            get {
                return "c4";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdC4(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }

            if (p.level.physics >= 1 && p.level.physics < 5) {
                sbyte numb = C4.NextCircuit(p.level);
                C4.C4s c4 = new C4.C4s(numb);
                p.level.C4list.Add(c4);
                p.c4circuitNumber = numb;

                p.StartSelection(BlockPlaced, null);
                p.SendMessage("Place any block for C4 and place a " + Colors.red + "red" + _s.props.DefaultColor +
                                   " block for the detonator!");
                return;
            }
            else {
                p.SendMessage("To use C4, the physics level must be 1 to 4.");
                return;
            }
        }

        private void BlockPlaced(Player p, CommandTempData c) {
            p.ClearSelection();

            if (c.BlockType == BlockId.Red) {
                DetonatorPlaced(p, c);
                return;
            }
            if (c.BlockType != BlockId.Air) {
                p.level.Blockchange(p, c.X, c.Y, c.Z, BlockId.C4);
            }
            p.StartSelection(BlockPlaced, null);
        }

        private void DetonatorPlaced(Player p, CommandTempData c) {
            p.ClearSelection();

            p.level.Blockchange(p, c.X, c.Y, c.Z, BlockId.C4Detonator);
            p.SendMessage("Placed detonator block.");
        }

        /// <summary>
        /// Called when /help is used on /c4.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/c4 - Places a block of C4.");
        }
    }
}

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

namespace MCHmk.Commands {
    public class CmdStairs : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"stair"});

        public override string Name {
            get {
                return "stairs";
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
        public CmdStairs(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }

            const string prompt = "Place two blocks to determine the height of the stairs.";
            TwoBlockSelection.Start(p, null, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two ends of the staircase.
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

            if (y1 == y2) {
                p.SendMessage("Cannot create a stairway that is zero blocks high.");
                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished);
                return;
            }

            ushort xx, zz;
            int currentState = 0;
            xx = x1;
            zz = z1;

            if (x1 > x2 && z1 > z2) {
                currentState = 0;
            }
            else if (x1 > x2 && z1 < z2) {
                currentState = 1;
            }
            else if (x1 < x2 && z1 > z2) {
                currentState = 2;
            }
            else {
                currentState = 3;
            }

            for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                if (currentState == 0) {
                    xx++;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.Slab);
                    xx++;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.DoubleSlab);
                    currentState = 1;
                }
                else if (currentState == 1) {
                    zz++;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.Slab);
                    zz++;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.DoubleSlab);
                    currentState = 2;
                }
                else if (currentState == 2) {
                    xx--;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.Slab);
                    xx--;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.DoubleSlab);
                    currentState = 3;
                }
                else {
                    zz--;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.Slab);
                    zz--;
                    p.level.Blockchange(p, xx, yy, zz, BlockId.DoubleSlab);
                    currentState = 0;
                }
            }

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished);
        }

        /// <summary>
        /// Called when /help is used on /stairs.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/stairs - Creates a spiral staircase.");
        }
    }
}

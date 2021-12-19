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
    public class CmdPaste : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"copy", "clipboard", "out"});

        public override string Name {
            get {
                return "paste";
            }
        }
        public override string Shortcut {
            get {
                return "v";
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

        public CmdPaste(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Help(p);
                return;
            }

            p.ClearSelection();

            p.StartSelection(BlockSelected, null);
            p.SendMessage("Place a block in the corner of where you want to paste.");
        }

        private void BlockSelected(Player p, CommandTempData c) {
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));

            Player.UndoPos Pos1;

            if (p.level.bufferblocks && !p.level.Instant) {
                p.CopyBuffer.ForEach(delegate(Player.CopyPos pos) {
                    Pos1.x = (ushort)(Math.Abs(pos.x) + x);
                    Pos1.y = (ushort)(Math.Abs(pos.y) + y);
                    Pos1.z = (ushort)(Math.Abs(pos.z) + z);

                    if (pos.type != BlockId.Air || p.copyAir)
                        unchecked {
                        if (p.level.GetTile(Pos1.x, Pos1.y, Pos1.z) != BlockId.Null) {
                            _s.blockQueue.Addblock(p, (ushort)(Pos1.x + p.copyoffset[0]), (ushort)(Pos1.y + p.copyoffset[1]),
                                                   (ushort)(Pos1.z + p.copyoffset[2]), pos.type);
                        }
                    }
                });
            }
            else {
                p.CopyBuffer.ForEach(delegate(Player.CopyPos pos) {
                    Pos1.x = (ushort)(Math.Abs(pos.x) + x);
                    Pos1.y = (ushort)(Math.Abs(pos.y) + y);
                    Pos1.z = (ushort)(Math.Abs(pos.z) + z);

                    if (pos.type != BlockId.Air || p.copyAir)
                        unchecked {
                        if (p.level.GetTile(Pos1.x, Pos1.y, Pos1.z) != BlockId.Null) {
                            p.level.Blockchange(p, (ushort)(Pos1.x + p.copyoffset[0]), (ushort)(Pos1.y + p.copyoffset[1]),
                                                (ushort)(Pos1.z + p.copyoffset[2]), pos.type);
                        }
                    }
                });
            }

            p.SendMessage("Pasted " + p.CopyBuffer.Count + " blocks.");

            if (p.staticCommands) {
                p.StartSelection(BlockSelected, null);
            }
        }

        /// <summary>
        /// Called when /help is used on /paste.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/paste - Pastes the stored copy.");
            p.SendMessage("The blocks will always be pasted in a set direction unless " +
                               "they are rotated with /spin.");
        }
    }
}

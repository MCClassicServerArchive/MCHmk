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

	http://www.osedu.org/licenses/ECL-2.0
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
    public class CmdTp : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"teleport", "move", "player", "user"});

        public override string Name {
            get {
                return "tp";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdTp(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                _s.commands.FindCommand("spawn");
                return;
            }
            int number = args.Split(' ').Length;
            if (number > 2) {
                Help(p);
                return;
            }
            if (number == 2) {
                if (!_s.commands.CanExecute(p, "P2P")) {
                    p.SendMessage("You cannot teleport others!");
                    return;
                }
                _s.commands.FindCommand("P2P").Use(p, args);
            }
            if (number == 1) {
                Player who = _s.players.Find(args);
                if (who == null || (who.hidden && p.rank.Permission < DefaultRankValue.Admin)) {
                    p.SendMessage("There is no player \"" + args + "\"!");
                    return;
                }
                if (p.level != who.level) {
                    if (who.level.name.Contains("cMuseum")) {
                        p.SendMessage("Player \"" + args + "\" is in a museum!");
                        return;
                    }
                    else {
                        if (_s.props.higherranktp == false) {
                            if (p.rank.Permission < who.rank.Permission) {
                                p.SendMessage("You cannot teleport to a player of higher rank!");
                                return;
                            }
                        }
                        _s.commands.FindCommand("goto").Use(p, who.level.name);
                    }
                }
                if (p.level == who.level) {
                    if (_s.props.higherranktp == false) {
                        if (p.rank.Permission < who.rank.Permission) {
                            p.SendMessage("You cannot teleport to a player of higher rank!");
                            return;
                        }
                    }

                    if (who.Loading) {
                        p.SendMessage("Waiting for " + who.color + who.name + _s.props.DefaultColor + " to spawn...");
                        while (who.Loading) { }
                    }
                    while (p.Loading) { }  //Wait for player to spawn in new map
                    unchecked {
                        p.SendPos((byte)-1, who.pos[0], who.pos[1], who.pos[2], who.rot[0], 0);
                    }
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /tp.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tp [player1?] [player2?] - Teleports player1 to player2.");
            p.SendMessage("If player2 is empty, it teleports you to player1.");
            p.SendMessage("If player1 is empty, /spawn is used.");
        }
    }
}

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
 * Written by Jack1312
 *
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
    public class CmdP2P : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"tp", "tele", "port", "player", "move"});

        public override string Name {
            get {
                return "p2p";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "mod";
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
        public CmdP2P(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            int number = args.Split(' ').Length;
            if (number > 2) {
                Help(p);
                return;
            }
            if (number == 2) {
                int pos = args.IndexOf(' ');
                string t = args.Substring(0, pos).ToLower();
                string s = args.Substring(pos + 1).ToLower();
                Player who = _s.players.Find(t);
                Player who2 = _s.players.Find(s);
                if (who == null) {
                    if (who2 == null) {
                        p.SendMessage("Neither of the players you specified, can be found or exist!");
                        return;
                    }
                    p.SendMessage("Player 1 is not online or does not exist!");
                    return;
                }
                if (who2 == null) {
                    p.SendMessage("Player 2 is not online or does not exist!");
                    return;
                }
                if (who == p) {
                    if (who2 == p) {
                        p.SendMessage("Why are you trying to teleport yourself to yourself? =S");
                        return;
                    }
                    p.SendMessage("Why not, just use /tp " + who2.name + "!");
                }
                if (who2 == p) {
                    p.SendMessage("Why not, just use /summon " + who.name + "!");
                }
                if (p.rank.Permission < who.rank.Permission) {
                    p.SendMessage("You cannot force a player of higher rank to tp to another player!");
                    return;
                }
                if (s == String.Empty) {
                    p.SendMessage("You did not specify player 2!");
                    return;
                }
                _s.commands.FindCommand("tp").Use(who, who2.name);
                p.SendMessage(who.name + " has been successfully teleported to " + who2.name + "!");
            }

            if (number == 1) {
                p.SendMessage("You did not specify player 2!");
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /p2p.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/p2p <player1?> <player2?> - Teleports the first player to the second.");
        }
    }
}

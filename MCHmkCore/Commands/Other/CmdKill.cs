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
    public class CmdKill : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"die", "player", "murder"});

        public override string Name {
            get {
                return "kill";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdKill(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            Player who;
            string killMsg;
            int killMethod = 0;
            if (args.IndexOf(' ') == -1) {
                who = _s.players.Find(args);
                if (!p.IsConsole) {
                    killMsg = " was killed by " + p.color + p.name;
                }
                else {
                    killMsg = " was killed by " + "the Console.";
                }
            }
            else {
                who = _s.players.Find(args.Split(' ')[0]);
                args = args.Substring(args.IndexOf(' ') + 1);

                if (args.IndexOf(' ') == -1) {
                    if (args.ToLower() == "explode") {
                        if (!p.IsConsole) {
                            killMsg = " was exploded by " + p.color + p.name;
                        }
                        else {
                            killMsg = " was exploded by the Console.";
                        }
                        killMethod = 1;
                    }
                    else {
                        killMsg = " " + args;
                    }
                }
                else {
                    if (args.Split(' ')[0].ToLower() == "explode") {
                        killMethod = 1;
                        args = args.Substring(args.IndexOf(' ') + 1);
                    }

                    killMsg = " " + args;
                }
            }

            if (who == null) {
                if (!p.IsConsole) {
                    p.HandleDeath(BlockId.Stone, " killed itself in its confusion");
                }
                p.SendMessage("Could not find player");
                return;
            }

            if (!p.IsConsole) {
                if (who.rank.Permission > p.rank.Permission) {
                    p.HandleDeath(BlockId.Stone, " was killed by " + who.color + who.name);
                    p.SendMessage("Cannot kill someone of higher rank");
                    return;
                }
            }

            if (killMethod == 1) {
                who.HandleDeath(BlockId.Stone, killMsg, true);
            }
            else {
                who.HandleDeath(BlockId.Stone, killMsg);
            }
        }

        /// <summary>
        /// Called when /help is used on /kill.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/kill <player?> [explode] [message?] - Kills a player.");
            p.SendMessage("If a message is provided, that message will be shown when the player is killed.");
            p.SendMessage("The explode parameter changes the message shown as well.");
        }
    }
}

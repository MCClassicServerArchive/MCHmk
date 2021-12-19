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
    public class CmdImpersonate : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"possess", "player"});

        public override string Name {
            get {
                return "impersonate";
            }
        }
        public override string Shortcut {
            get {
                return "imp";
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

        public CmdImpersonate(Server s) : base(s) { }

        public void SendIt(Player p, string message, Player player) {
            if (message.Split(' ').Length > 1) {
                if (player != null) {
                    message = message.Substring(message.IndexOf(' ') + 1);
                    _s.GlobalChat(player, message);
                }
                else {
                    string playerName = message.Split(' ')[0];
                    message = message.Substring(message.IndexOf(' ') + 1);
                    _s.GlobalMessage(playerName + ": &f" + message);
                }
            }
            else {
                p.SendMessage("No message was given.");
            }
        }
        public override void Use(Player p, string args) {
            if ((args == String.Empty)) {
                this.Help(p);
            }
            else {
                Player player = _s.players.Find(args.Split(' ')[0]);
                if (player != null) {
                    if (p.IsConsole) {
                        this.SendIt(p, args, player);
                    }
                    else {
                        if (player == p) {
                            this.SendIt(p, args, player);
                        }
                        else {
                            if (p.rank.Permission > player.rank.Permission) {
                                this.SendIt(p, args, player);
                            }
                            else {
                                p.SendMessage("You cannot impersonate a player of equal or greater rank.");
                            }
                        }
                    }
                }
                else {
                    if (!p.IsConsole) {
                        if (p.rank.Permission >= DefaultRankValue.Admin) {
                            Uuid uuid = Uuid.FindUuid(_s.database, args.Split(' ')[0]);
                            if (!uuid.IsValid) {
                                p.SendMessage("The player name is invalid or has never joined the server.");
                                return;
                            }

                            if (_s.ranks.FindPlayerRank(uuid).Permission < p.rank.Permission) {
                                this.SendIt(p, args, null);
                            }
                            else {
                                p.SendMessage("You cannot impersonate a player of equal or greater rank.");
                            }
                        }
                        else {
                            p.SendMessage("You are not allowed to impersonate offline players");
                        }
                    }
                    else {
                        this.SendIt(p, args, null);
                    }
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /impersonate.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/impersonate <player?> <message?> - Sends a message using " +
                               "the given player's name.");
        }
    }
}

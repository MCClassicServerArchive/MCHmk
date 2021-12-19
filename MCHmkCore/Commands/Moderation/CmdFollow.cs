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
    public class CmdFollow : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"possess", "impersonate"});

        public override string Name {
            get {
                return "follow";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdFollow(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            if (!p.canBuild) {
                p.SendMessage("You're currently being &4possessed" + _s.props.DefaultColor + "!");
                return;
            }
            try {
                bool stealth = false;

                if (args != String.Empty) {
                    if (args == "#") {
                        if (p.following != String.Empty) {
                            stealth = true;
                            args = String.Empty;
                        }
                        else {
                            Help(p);
                            return;
                        }
                    }
                    else if (args.IndexOf(' ') != -1) {
                        if (args.Split(' ')[0] == "#") {
                            if (p.hidden) {
                                stealth = true;
                            }
                            args = args.Split(' ')[1];
                        }
                    }
                }

                Player who = _s.players.Find(args);
                if (args == String.Empty && p.following == String.Empty) {
                    Help(p);
                    return;
                }
                else if (args == String.Empty && p.following != String.Empty || args == p.following) {
                    who = _s.players.Find(p.following);
                    p.following = String.Empty;
                    if (p.hidden) {
                        if (who != null) {
                            p.SendSpawn(who.serverId, who.color + who.name, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1]);
                        }
                        if (!stealth) {
                            _s.commands.FindCommand("hide").Use(p, String.Empty);
                        }
                        else {
                            if (who != null) {
                                p.SendMessage("You have stopped following " + who.color + who.name + _s.props.DefaultColor +
                                                   " and remained hidden.");
                            }
                            else {
                                p.SendMessage("Following stopped.");
                            }
                        }
                        return;
                    }
                }
                if (who == null) {
                    p.SendMessage("Could not find player.");
                    return;
                }
                else if (who == p) {
                    p.SendMessage("Cannot follow yourself.");
                    return;
                }
                else if (who.rank.Permission >= p.rank.Permission) {
                    p.SendMessage("Cannot follow someone of equal or greater rank.");
                    return;
                }
                else if (who.following != String.Empty) {
                    p.SendMessage(who.name + " is already following " + who.following);
                    return;
                }

                if (!p.hidden) {
                    _s.commands.FindCommand("hide").Use(p, String.Empty);
                }

                if (p.level != who.level) {
                    _s.commands.FindCommand("tp").Use(p, who.name);
                }
                if (p.following != String.Empty) {
                    who = _s.players.Find(p.following);
                    p.SendSpawn(who.serverId, who.color + who.name, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1]);
                }
                who = _s.players.Find(args);
                p.following = who.name;
                p.SendMessage("Following " + who.name + ". Use /follow to stop.");
                p.SendDie(who.serverId);
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("Error occured");
            }
        }

        /// <summary>
        /// Called when /help is used on /follow.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/follow <player?> - Follows the specfied player until the command is used again.");
            p.SendMessage("/follow # <player?> - Causes /hide not to be toggled.");
        }
    }
}

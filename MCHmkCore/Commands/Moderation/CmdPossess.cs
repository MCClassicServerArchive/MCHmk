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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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
    public class CmdPossess : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"imp", "control"});

        public override string Name {
            get {
                return "possess";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdPossess(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length > 2) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("Console possession?  Nope.avi.");
                return;
            }
            try {
                string skin = (args.Split(' ').Length == 2) ? args.Split(' ')[1] : String.Empty;
                args = args.Split(' ')[0];
                if (args == String.Empty) {
                    if (p.possess == String.Empty) {
                        Help(p);
                        return;
                    }
                    else {
                        Player who = _s.players.Find(p.possess);
                        if (who == null) {
                            p.possess = String.Empty;
                            p.SendMessage("Possession disabled.");
                            return;
                        }
                        who.following = String.Empty;
                        who.canBuild = true;
                        p.possess = String.Empty;
                        if (!who.MarkPossessed()) {
                            return;
                        }
                        p.invincible = false;
                        _s.commands.FindCommand("hide").Use(p, String.Empty);
                        p.SendMessage("Stopped possessing " + who.color + who.name + _s.props.DefaultColor + ".");
                        return;
                    }
                }
                else if (args == p.possess) {
                    Player who = _s.players.Find(p.possess);
                    if (who == null) {
                        p.possess = String.Empty;
                        p.SendMessage("Possession disabled.");
                        return;
                    }
                    if (who == p) {
                        p.SendMessage("Cannot possess yourself!");
                        return;
                    }
                    who.following = String.Empty;
                    who.canBuild = true;
                    p.possess = String.Empty;
                    if (!who.MarkPossessed()) {
                        return;
                    }
                    p.invincible = false;
                    _s.commands.FindCommand("hide").Use(p, String.Empty);
                    p.SendMessage("Stopped possessing " + who.color + who.name + _s.props.DefaultColor + ".");
                    return;
                }
                else {
                    Player who = _s.players.Find(args);
                    if (who == null) {
                        p.SendMessage("Could not find player.");
                        return;
                    }
                    if (who.rank.Permission >= p.rank.Permission) {
                        p.SendMessage("Cannot possess someone of equal or greater rank.");
                        return;
                    }
                    if (who.possess != String.Empty) {
                        p.SendMessage("That player is currently possessing someone!");
                        return;
                    }
                    if (who.following != String.Empty) {
                        p.SendMessage("That player is either following someone or already possessed.");
                        return;
                    }
                    if (p.possess != String.Empty) {
                        Player oldwho = _s.players.Find(p.possess);
                        if (oldwho != null) {
                            oldwho.following = String.Empty;
                            oldwho.canBuild = true;
                            if (!oldwho.MarkPossessed()) {
                                return;
                            }
                            //p.SendSpawn(oldwho.id, oldwho.color + oldwho.name, oldwho.pos[0], oldwho.pos[1], oldwho.pos[2], oldwho.rot[0], oldwho.rot[1]);
                        }
                    }
                    _s.commands.FindCommand("tp").Use(p, who.name);
                    if (!p.hidden) {
                        _s.commands.FindCommand("hide").Use(p, String.Empty);
                    }
                    p.possess = who.name;
                    who.following = p.name;
                    if (!p.invincible) {
                        p.invincible = true;
                    }
                    bool result = (skin == "#") ? who.MarkPossessed() : who.MarkPossessed(p.name);
                    if (!result) {
                        return;
                    }
                    p.SendDie(who.serverId);
                    who.canBuild = false;
                    p.SendMessage("Successfully possessed " + who.color + who.name + _s.props.DefaultColor + ".");
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("There was an error.");
            }
        }

        /// <summary>
        /// Called when /help is used on /possess.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/possess <player?> [#] - Possesses the specified player.");
            p.SendMessage("Using # after player name makes the possessed player keep " +
                               "his or her custom skin during possession.");
            p.SendMessage("Not using it makes them lose his or her skin, and makes " +
                               "his or her name show as \"Player (YourName)\".");
        }
    }
}

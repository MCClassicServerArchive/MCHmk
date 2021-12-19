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
    public class CmdMove : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"player", "pos"});

        public override string Name {
            get {
                return "move";
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
                return DefaultRankValue.Banned;
            }
        }
        public CmdMove(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // /move name map
            // /move x y z
            // /move name x y z

            string[] param = args.Split(' ');

            if (param.Length < 1 || param.Length > 4) {
                Help(p);
                return;
            }

            // /move name
            if (param.Length == 1) {
                // Use main world by default
                // Add the world name to the 2nd param so that the IF block below is used
                param = new string[] { param[0], _s.mainLevel.name };
            }

            if (param.Length == 2) {   // /move name map
                Player who = _s.players.Find(param[0]);
                Level where = _s.levels.Find(param[1]);
                if (who == null) {
                    p.SendMessage("Could not find player specified");
                    return;
                }
                if (where == null) {
                    p.SendMessage("Could not find level specified");
                    return;
                }
                if (!p.IsConsole && who.rank.Permission > p.rank.Permission) {
                    p.SendMessage("Cannot move someone of greater rank");
                    return;
                }

                _s.commands.FindCommand("goto").Use(who, where.name);
                if (who.level == where) {
                    p.SendMessage("Sent " + who.color + who.name + _s.props.DefaultColor + " to " + where.name);
                }
                else {
                    p.SendMessage(where.name + " is not loaded");
                }
            }
            else {
                // /move name x y z
                // /move x y z

                Player who;

                if (param.Length == 4) {
                    who = _s.players.Find(param[0]);
                    if (who == null) {
                        p.SendMessage("Could not find player specified");
                        return;
                    }
                    if (!p.IsConsole && who.rank.Permission > p.rank.Permission) {
                        p.SendMessage("Cannot move someone of greater rank");
                        return;
                    }
                    args = args.Substring(args.IndexOf(' ') + 1);
                }
                else {
                    who = p;
                }

                try {
                    ushort x = System.Convert.ToUInt16(args.Split(' ')[0]);
                    ushort y = System.Convert.ToUInt16(args.Split(' ')[1]);
                    ushort z = System.Convert.ToUInt16(args.Split(' ')[2]);
                    x *= 32;
                    x += 16;
                    y *= 32;
                    y += 32;
                    z *= 32;
                    z += 16;
                    unchecked {
                        who.SendPos((byte)-1, x, y, z, p.rot[0], p.rot[1]);
                    }
                    if (p != who) {
                        p.SendMessage("Moved " + who.color + who.name);
                    }
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Invalid co-ordinates");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /move.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/move [player?] <map?>|<x?> <y?> <z?> - Moves a player to the given coordinates or map.");
            p.SendMessage("If a player name is not given, you will be moved.");
            p.SendMessage("The map parameter must be left blank if x, y or z is used and vice versa.");
            p.SendMessage("If both are empty, the main map will be used.");
        }
    }
}

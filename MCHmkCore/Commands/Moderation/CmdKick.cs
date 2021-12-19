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
    public class CmdKick : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"moderate", "ban", "player"});

        public override string Name {
            get {
                return "kick";
            }
        }
        public override string Shortcut {
            get {
                return "k";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdKick(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(args.Split(' ')[0]);
            if (who == null) {
                p.SendMessage("Could not find player specified.");
                return;
            }
            if (args.Split(' ').Length > 1) {
                args = args.Substring(args.IndexOf(' ') + 1);
            }
            else if (p.IsConsole) {
                args = "You were kicked by the Console!";
            }
            else if (!p.IsConsole) {
                args = "You were kicked by " + p.name + "!";
            }

            if (!p.IsConsole) {
                if (who == p) {
                    p.SendMessage("You cannot kick yourself!");
                    return;
                }
                if (who.rank.Permission >= p.rank.Permission) {
                    _s.GlobalChat(p, p.color + p.name + _s.props.DefaultColor + " tried to kick " + who.color + who.name + _s.props.DefaultColor +
                                      " but failed.", false);
                    return;
                }
            }
            who.Kick(args);
        }

        /// <summary>
        /// Called when /help is used on /kick.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/kick <player?> [reason?] - Kicks a player.");
        }
    }
}

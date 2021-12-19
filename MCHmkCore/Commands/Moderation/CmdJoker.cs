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
    public class CmdJoker : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"joke", "troll", "fun"});

        public override string Name {
            get {
                return "joker";
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
        public static string keywords {
            get {
                return String.Empty;
            }
        }
        public CmdJoker(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            bool stealth = false;
            if (args[0] == '#') {
                args = args.Remove(0, 1).Trim();
                stealth = true;
                _s.logger.Log("Stealth joker attempted");
            }

            Player who = _s.players.Find(args);
            if (who == null) {
                p.SendMessage("Could not find player.");
                return;
            }
            if (!p.IsConsole && who.rank.Permission > p.rank.Permission) {
                p.SendMessage("Cannot joker someone of equal or greater rank.");
                return;
            }

            if (!who.joker) {
                who.joker = true;
                if (stealth) {
                    _s.GlobalMessageOps(who.color + who.name + _s.props.DefaultColor + " is now STEALTH joker'd. ");
                    return;
                }
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " is now a &aJ&bo&ck&5e&9r" + _s.props.DefaultColor +
                                  ".", false);
            }
            else {
                who.joker = false;
                if (stealth) {
                    _s.GlobalMessageOps(who.color + who.name + _s.props.DefaultColor + " is now STEALTH Unjoker'd. ");
                    return;
                }
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " is no longer a &aJ&bo&ck&5e&9r" +
                                  _s.props.DefaultColor + ".", false);
            }
        }

        /// <summary>
        /// Called when /help is used on /joker.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/joker <player?> - Causes a player to become a joker.");
            p.SendMessage("/joker # <player?> - Sliently makes a player a joker.");
        }
    }
}

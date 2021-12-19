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
    public class CmdFreeze : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"ice", "move", "player"});

        public override string Name {
            get {
                return "freeze";
            }
        }
        public override string Shortcut {
            get {
                return "fz";
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
        public CmdFreeze(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(args);
            if (who == null) {
                p.SendMessage("Could not find player.");
                return;
            }
            else if (who == p) {
                p.SendMessage("Cannot freeze yourself.");
                return;
            }
            else if (!p.IsConsole) {
                if (who.rank.Permission >= p.rank.Permission) {
                    p.SendMessage("Cannot freeze someone of equal or greater rank.");
                    return;
                }
            }
            if (!who.frozen) {
                who.frozen = true;
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " has been &bfrozen" + _s.props.DefaultColor + " by "
                                  + p.color + p.name + _s.props.DefaultColor + ".", false);
            }
            else {
                who.frozen = false;
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " has been &adefrosted" + _s.props.DefaultColor +
                                  " by " + p.color + p.name + _s.props.DefaultColor + ".", false);
            }
        }

        /// <summary>
        /// Called when /help is used on /freeze.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/freeze <player?> - Stops a player from moving.");
            p.SendMessage("Use /freeze on that player again to unfreeze.");
        }
    }
}

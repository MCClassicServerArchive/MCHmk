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
    public class CmdInvincible : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"god", "life", "inf"});

        public override string Name {
            get {
                return "invincible";
            }
        }
        public override string Shortcut {
            get {
                return "inv";
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
        public CmdInvincible(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            Player who;
            if (args != String.Empty) {
                who = _s.players.Find(args);
            }
            else {
                who = p;
            }

            if (who == null) {
                p.SendMessage("Cannot find player.");
                return;
            }

            if (!p.IsConsole && who.rank.Permission > p.rank.Permission) {
                p.SendMessage("Cannot toggle invincibility for someone of higher rank");
                return;
            }

            if (who.invincible) {
                who.invincible = false;
                if (!p.IsConsole && who == p) {
                    p.SendMessage("You are no longer invincible.");
                }
                else {
                    p.SendMessage(who.color + who.name + _s.props.DefaultColor + " is no longer invincible.");
                }

                if (_s.props.cheapMessage && !p.hidden) {
                    _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " has stopped being immortal", false);
                }
            }
            else {
                if (!p.IsConsole && who == p) {
                    p.SendMessage("You are now invincible.");
                }
                else {
                    p.SendMessage(who.color + who.name + _s.props.DefaultColor + "is now invincible.");
                }
                who.invincible = true;
                if (_s.props.cheapMessage && !p.hidden) {
                    _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " " + _s.props.cheapMessageGiven, false);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /invincible.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/invincible [player?] - Turns invincible mode on/off.");
            p.SendMessage("If a player's name is given, that player's invincibility is toggled.");
        }
    }
}

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
    public class CmdTnt : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"c4", "explo"});

        public override string Name {
            get {
                return "tnt";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Builder;
            }
        }
        public CmdTnt(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length > 1) {
                Help(p);
                return;
            }

            if (p.blockAction == 13 || p.blockAction == 14) {
                if (p.allowTnt == false) {
                    p.SendMessage("Tnt usage is not allowed at the moment!");
                    return;
                }

                p.blockAction = 0;
                p.SendMessage("TNT mode is now &cOFF" + _s.props.DefaultColor + ".");
            }
            else if (args.ToLower() == "small" || args == String.Empty) {
                if (p.allowTnt == true) {
                    p.blockAction = 13;
                    p.SendMessage("TNT mode is now &aON" + _s.props.DefaultColor + ".");
                    return;
                }
                p.SendMessage("Tnt usage is not allowed at the moment!");
                return;
            }
            else if (args.ToLower() == "big") {
                if (p.allowTnt == false) {
                    p.SendMessage("Tnt usage is not allowed at the moment!");
                    return;
                }
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 1)) {
                    p.blockAction = 14;
                    p.SendMessage("TNT (Big) mode is now &aON" + _s.props.DefaultColor + ".");
                }
                else {
                    p.SendMessage("This mode is reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 1)).name + "+");
                }
            }
            else if (args.ToLower() == "allow") {
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 2)) {
                    p.allowTnt = true;
                    p.SendMessage("&cTnt usage has now been enabled!");
                    return;
                }
                p.SendMessage("You must be " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                   2)).name + "+ to use this command.");
                return;
            }
            else if (args.ToLower() == "disallow") {
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 2)) {
                    p.allowTnt = false;
                    p.SendMessage("&cTnt usage has now been disabled!");
                    return;
                }
                p.SendMessage("You must be " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                   2)).name + "+ to use this command.");
                return;
            }
            else if (args.ToLower() == "nuke") {
                if (p.allowTnt == false) {
                    p.SendMessage("Tnt usage is not allowed at the moment!");
                    return;
                }
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 3)) {
                    p.blockAction = 15;
                    p.SendMessage("TNT (Nuke) mode is now &aON" + _s.props.DefaultColor + ".");
                }
                else {
                    p.SendMessage("This mode is reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 3)).name + "+");
                }
            }
            else {
                Help(p);
            }

            p.painting = false;
        }

        /// <summary>
        /// Called when /help is used on /tnt.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tnt [small/big/nuke] - Turns on TNT mode, which allows you " +
                               "to place TNT that explodes.");
            p.SendMessage("Big and Nuke TNT are reserved for "
                               + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 3)).name + "+");
            if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 2)) {
                p.SendMessage("/tnt allow - Allows the use of TNT server-wide.");
                p.SendMessage("/tnt disallow - Disallows the use of TNT server-wide.");
            }


        }
    }
}

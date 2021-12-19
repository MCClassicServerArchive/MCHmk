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
    public class CmdMoney : Command {
        private string[] _keywords = new string[] {"cash", String.Empty};

        public override string Name {
            get {
                return "money";
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
                // Crappy hack to set the proper value at runtime -Jjp137
                _keywords[1] = _s.props.moneys;

                return Array.AsReadOnly<string>(_keywords);
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

        public CmdMoney(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            bool emptyMessage = args == String.Empty || args == null || args == string.Empty;
            if (!p.IsConsole && emptyMessage) {
                p.SendMessage("You currently have %f" + p.money + " %3" + _s.props.moneys);
            }
            else if (args.Split().Length == 1) {
                Player who = _s.players.Find(args);
                if (who == null) {
                    p.SendMessage("Error: Player is not online.");
                }
                else {
                    p.SendMessage(who.color + who.name + _s.props.DefaultColor + " currently has %f" + who.money + " %3" +
                                        _s.props.moneys);
                }
            }
            else if (p.IsConsole && emptyMessage) {
                p.SendMessage("%Console can't have %3" + _s.props.moneys);
            }
            else {
                p.SendMessage("%cInvalid parameters!");
                Help(p);
            }
        }

        /// <summary>
        /// Called when /help is used on /money.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/money - Shows how much " + _s.props.moneys + " you have.");
            p.SendMessage("/money <player?> - Shows how much " + _s.props.moneys +  " a player has.");
        }
    }
}

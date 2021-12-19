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
    public class CmdFakeRank : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "mod", "fake", "troll"});

        public override string Name {
            get {
                return "fakerank";
            }
        }
        public override string Shortcut {
            get {
                return "frk";
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
                return DefaultRankValue.Admin;
            }
        }

        public CmdFakeRank(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            Player plr = _s.players.Find(args.Split (' ')[0]);
            Rank grp = _s.ranks.Find(args.Split (' ')[1]);

            if (plr == null) {
                p.SendMessage(_s.props.DefaultColor + "Player not found!");
                return;
            }
            if (grp == null) {
                p.SendMessage(_s.props.DefaultColor + "No rank entered.");
                return;
            }
            if (_s.ranks.Contains(grp)) {

                if (grp.name == "banned") {
                    _s.GlobalMessage(plr.color + plr.name + _s.props.DefaultColor + " is now &8banned" + _s.props.DefaultColor + "!");
                }
                else {
                    _s.GlobalMessage(plr.color + plr.name + _s.props.DefaultColor + "'s rank was set to " + grp.color + grp.name +
                                         _s.props.DefaultColor + ".");
                    _s.GlobalMessage("&6Congratulations!");
                }
            }

            else {
                p.SendMessage(_s.props.DefaultColor + "Invalid Rank Entered!");
                return;
            }

        }
        
        /// <summary>
        /// Called when /help is used on /fakerank.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/fakerank <player?> <rank?> - Sends a fake /setrank " +
                               "message regarding the specified player.");
        }
    }
}

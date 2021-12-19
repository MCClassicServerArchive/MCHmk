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
    public class CmdLastCmd : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"cmd", "command", "last", "info"});

        public override string Name {
            get {
                return "lastcmd";
            }
        }
        public override string Shortcut {
            get {
                return "last";
            }
        }
        public override string Type {
            get {
                return "information";
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
        public CmdLastCmd(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                foreach (Player pl in _s.players) {
                    if (pl.lastCMD.Contains("setpass") || pl.lastCMD.Contains("pass")) {
                        pl.lastCMD = String.Empty;
                    }
                    if (pl.rank.Permission <= p.rank.Permission && pl.hidden) {
                        p.SendMessage(pl.color + pl.name + _s.props.DefaultColor + " last used \"" + pl.lastCMD + "\"");
                    }
                    if (!pl.hidden) {
                        p.SendMessage(pl.color + pl.name + _s.props.DefaultColor + " last used \"" + pl.lastCMD + "\"");
                    }
                }
            }
            else {
                Player who = _s.players.Find(args);
                if (who == null || who.rank.Permission > p.rank.Permission && who.hidden) {
                    p.SendMessage("Could not find player entered");
                    return;
                }
                if (who.lastCMD.Contains("setpass") || who.lastCMD.Contains("pass")) {
                    who.lastCMD = String.Empty;
                }
                p.SendMessage(who.color + who.name + _s.props.DefaultColor + " last used \"" + who.lastCMD + "\"");
            }
        }

        /// <summary>
        /// Called when /help is used on /lastcmd.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/lastcmd <player?> - Shows the last command used by a player.");
            p.SendMessage("/lastcmd - Shows the most recent commands used by everyone on the server.");
        }
    }
}

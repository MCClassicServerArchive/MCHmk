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
    public class CmdInfo : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"server", "detail"});

        public override string Name {
            get {
                return "info";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return DefaultRankValue.Banned;
            }
        }
        public CmdInfo(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Help(p);
            }
            else {
                p.SendMessage("This server's name is &b" + _s.props.name + _s.props.DefaultColor + ".");
                p.SendMessage("There are currently " + _s.players.Count.ToString() + " players on this server");
                p.SendMessage("This server currently has $banned people that are &8banned" + _s.props.DefaultColor + ".");
                p.SendMessage("This server currently has " + _s.levels.Count.ToString() + " levels loaded.");
                p.SendMessage("This server's currency is: " + _s.props.moneys);
                p.SendMessage("This server runs on &bMCHmk" + _s.props.DefaultColor + ", which is based on &bMCForge" + _s.props.DefaultColor + ".");
                p.SendMessage("This server's version: &ar" + _s.Version.Revision);
#if DEBUG
                p.SendMessage("This server is running a %adebug build.");
#endif
                TimeSpan up = DateTime.Now - _s.timeOnline;
                string upTime = "Time online: &b";
                if (up.Days == 1) {
                    upTime += up.Days.ToString() + " day, ";
                }
                else if (up.Days > 0) {
                    upTime += up.Days.ToString() + " days, ";
                }
                if (up.Hours == 1) {
                    upTime += up.Hours.ToString() + " hour, ";
                }
                else if (up.Days > 0 || up.Hours > 0) {
                    upTime += up.Hours.ToString() + " hours, ";
                }
                if (up.Minutes == 1) {
                    upTime += up.Minutes.ToString() + " minute and ";
                }
                else if (up.Hours > 0 || up.Days > 0 || up.Minutes > 0) {
                    upTime += up.Minutes.ToString() + " minutes and ";
                }
                if (up.Seconds == 1) {
                    upTime += up.Seconds.ToString() + " second";
                }
                else {
                    upTime += up.Seconds.ToString() + " seconds";
                }
                p.SendMessage(upTime);
            }
        }

        /// <summary>
        /// Called when /help is used on /info.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/info - Displays information about the server.");
        }
    }
}

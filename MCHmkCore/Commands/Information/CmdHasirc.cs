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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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
    public class CmdHasirc : Command {
        private string[] _keywords = new string[] {"has", String.Empty};

        public override string Name {
            get {
                return "hasirc";
            }
        }
        public override string Shortcut {
            get {
                return "irc";
            }
        }
        public override string Type {
            get {
                return "information";
            }
        }
        public override ReadOnlyCollection<string> Keywords {
            get {
                // Crappy hack to set the proper value at runtime -Jjp137
                _keywords[1] = _s.props.ircChannel;

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
        public CmdHasirc(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Help(p);
                return;
            }
            else {
                string hasirc;
                string ircdetails = String.Empty;
                if (_s.props.irc) {
                    hasirc = "&aEnabled" + _s.props.DefaultColor + ".";
                    ircdetails = _s.props.ircServer + " > " + _s.props.ircChannel;
                }
                else {
                    hasirc = "&cDisabled" + _s.props.DefaultColor + ".";
                }
                p.SendMessage("IRC is " + hasirc);
                if (ircdetails != String.Empty) {
                    p.SendMessage("Location: " + ircdetails);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /hasirc.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/hasirc - Displays whether the server has enabled IRC.");
            p.SendMessage("If IRC is active, the location of the IRC channel is displayed.");
        }
    }
}

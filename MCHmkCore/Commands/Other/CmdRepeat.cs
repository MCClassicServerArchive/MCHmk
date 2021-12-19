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
    public class CmdRepeat : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rep", "again"});

        public override string Name {
            get {
                return "repeat";
            }
        }
        public override string Shortcut {
            get {
                return "m";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdRepeat(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                if (p.lastCMD == String.Empty) {
                    p.SendMessage("No commands used yet.");
                    return;
                }
                if (p.lastCMD.Length > 5)
                    if (p.lastCMD.Substring(0, 6) == "static") {
                        p.SendMessage("Can't repeat static");
                        return;
                    }

                p.SendMessage("Using &b/" + p.lastCMD);

                if (p.lastCMD.IndexOf(' ') == -1) {
                    _s.commands.FindCommand(p.lastCMD).Use(p, String.Empty);
                }
                else {
                    _s.commands.FindCommand(p.lastCMD.Substring(0, p.lastCMD.IndexOf(' '))).Use(p,
                            p.lastCMD.Substring(p.lastCMD.IndexOf(' ') + 1));
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("An error occured!");
            }
        }

        /// <summary>
        /// Called when /help is used on /repeat.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/repeat - Repeats the command that was last used.");
        }
    }
}

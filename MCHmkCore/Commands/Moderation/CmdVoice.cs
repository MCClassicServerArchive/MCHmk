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
    public class CmdVoice : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"speak", "moderate"});

        public override string Name {
            get {
                return "voice";
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
        public CmdVoice(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(args);
            if (who != null) {
                if (who.voice) {
                    who.voice = false;
                    p.SendMessage("Removing voice status from " + who.name);
                    who.SendMessage("Your voice status has been revoked.");
                    who.voicestring = String.Empty;
                }
                else {
                    who.voice = true;
                    p.SendMessage("Giving voice status to " + who.name);
                    who.SendMessage("You have received voice status.");
                    who.voicestring = "&f+";
                }
            }
            else {
                p.SendMessage("There is no player online named '" + args + "'");
            }
        }

        /// <summary>
        /// Called when /help is used on /voice.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/voice <name?> - Toggles voice status for a player.");
            p.SendMessage("Voiced players can talk when /moderate is in effect.");
        }
    }
}

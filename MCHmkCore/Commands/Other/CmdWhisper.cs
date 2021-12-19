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
    public class CmdWhisper : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"tell", "private"});

        public override string Name {
            get {
                return "whisper";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }
        public CmdWhisper(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                p.whisper = !p.whisper;
                p.whisperTo = String.Empty;
                if (p.whisper) {
                    p.SendMessage("All messages sent will now auto-whisper");
                }
                else {
                    p.SendMessage("Whisper chat turned off");
                }
            }
            else {
                Player who = _s.players.Find(args);
                if (who == null) {
                    p.whisperTo = String.Empty;
                    p.whisper = false;
                    p.SendMessage("Could not find player.");
                    return;
                }
                if (who.hidden) {
                    if (p.hidden == false || who.rank.Permission > p.rank.Permission) {
                        p.SendMessage("Could not find player.");
                        return;
                    }
                }

                p.whisper = true;
                p.whisperTo = who.name;
                p.SendMessage("Auto-whisper enabled.  All messages will now be sent to " + who.name + ".");
            }

        }

        /// <summary>
        /// Called when /help is used on /whisper.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/whisper <player?> - Makes all messages you send act like " +
                               "whispers to the specified player by default.");
        }
    }
}

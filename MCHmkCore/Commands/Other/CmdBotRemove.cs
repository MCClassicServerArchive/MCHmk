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
    public class CmdBotRemove : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"bot", "remove", "del"});

        public override string Name {
            get {
                return "botremove";
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
                return DefaultRankValue.Admin;
            }
        }
        public string[,] botlist;
        public CmdBotRemove(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            try {
                if (args.ToLower() == "all") {
                    for (int i = 0; i < PlayerBot.playerbots.Count; i++) {
                        if (PlayerBot.playerbots[i].level == p.level) {
                            PlayerBot Pb = PlayerBot.playerbots[i];
                            Pb.removeBot();
                            i--;
                        }
                    }
                }
                else {
                    PlayerBot who = PlayerBot.Find(args);
                    if (who == null) {
                        p.SendMessage("There is no bot " + who + "!");
                        return;
                    }
                    if (p.level != who.level) {
                        p.SendMessage(who.name + " is in a different level.");
                        return;
                    }
                    who.removeBot();
                    p.SendMessage("Removed bot.");
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("Error caught");
            }
        }

        /// <summary>
        /// Called when /help is used on /botremove.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/botremove <name?> - Remove a bot on the same level as you.");

            // p.SendMessage("If All is used, all bots on the current level are removed");
            // ^ Idk why this is here. Maybe its broken?
        }
    }
}

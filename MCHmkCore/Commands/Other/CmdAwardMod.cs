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
    public class CmdAwardMod : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"price", "list", "trophy", "info"});

        public override string Name {
            get {
                return "awardmod";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdAwardMod(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }

            bool add = true;
            if (args.Split(' ')[0].ToLower() == "add") {
                args = args.Substring(args.IndexOf(' ') + 1);
            }
            else if (args.Split(' ')[0].ToLower() == "del") {
                add = false;
                args = args.Substring(args.IndexOf(' ') + 1);
            }

            if (add) {
                if (args.IndexOf(":") == -1) {
                    p.SendMessage("&cMissing a colon!");
                    Help(p);
                    return;
                }
                string awardName = args.Split(':')[0].Trim();
                string description = args.Split(':')[1].Trim();

                if (!Awards.addAward(awardName, description)) {
                    p.SendMessage("This award already exists!");
                }
                else {
                    _s.GlobalMessage("Award added: &6" + awardName + " : " + description);
                }
            }
            else {
                if (!Awards.removeAward(args)) {
                    p.SendMessage("This award doesn't exist!");    //corrected spelling error
                }
                else {
                    _s.GlobalMessage("Award removed: &6" + args);
                }
            }

            Awards.Save();
        }

        /// <summary>
        /// Called when /help is used on /awardmod.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/awardmod <add/del> <*award name*> : <description?>");
            p.SendMessage("Adds or deletes an award with the given award name.");
            p.SendMessage("Example: /awardmod add Play : Play on an MCHmk Server!");
        }
    }
}

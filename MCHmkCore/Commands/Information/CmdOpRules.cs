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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdOpRules : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"op", "rules", "info"});

        public override string Name {
            get {
                return "oprules";
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
        public CmdOpRules(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            //Do you really need a list for this?
            List<string> oprules = new List<string>();
            if (!File.Exists("text/oprules.txt")) {
                File.WriteAllText("text/oprules.txt", "No oprules entered yet!");
            }

            using (StreamReader r = File.OpenText("text/oprules.txt")) {
                while (!r.EndOfStream) {
                    oprules.Add(r.ReadLine());
                }
            }

            Player who = null;
            if (args != String.Empty) {
                who = _s.players.Find(args);
                if (p.rank.Permission < who.rank.Permission) {
                    p.SendMessage("You cant send /oprules to another player!");
                    return;
                }
            }
            else {
                who = p;
            }

            if (who != null) {
                //if (who.level == _s.mainLevel && _s.mainLevel.permissionbuild == LevelPermission.Guest) { who.SendMessage("You are currently on the guest map where anyone can build"); }
                who.SendMessage("Server OPRules:");
                foreach (string s in oprules) {
                    who.SendMessage(s);
                }
            }
            else {
                p.SendMessage("There is no player \"" + args + "\"!");
            }
        }

        /// <summary>
        /// Called when /help is used on /oprules.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/oprules [player?]- Displays server oprules to a player.");
            p.SendMessage("If no player is specified, the rules are displayed to you.");
        }
    }
}

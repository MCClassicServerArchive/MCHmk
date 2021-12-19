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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdRules : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"read", "prot"});

        public override string Name {
            get {
                return "rules";
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
        public CmdRules(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            List<string> rules = new List<string>();
            if (!File.Exists("text/rules.txt")) {
                File.WriteAllText("text/rules.txt", "No rules entered yet!");
            }
            using (StreamReader r = File.OpenText("text/rules.txt")) {
                while (!r.EndOfStream) {
                    rules.Add(r.ReadLine());
                }
            }

            Player who = null;
            if (args != String.Empty) {
                if (!p.IsConsole && p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                    p.SendMessage("You cant send /rules to another player!");
                    return;
                }
                who = _s.players.Find(args);
            }
            else {
                who = p;
            }

            if (who != null) {
                who.hasreadrules = true;
                who.SendMessage("Server Rules:");
                foreach (string s in rules) {
                    who.SendMessage(s);
                }

            }
            else if (p.IsConsole && String.IsNullOrEmpty(args)) {
                _s.logger.Log("Server Rules:");
                foreach (string s in rules) {
                    _s.logger.Log(s);
                }
            }
            else {
                p.SendMessage("There is no player \"" + args + "\"!");
            }
        }

        /// <summary>
        /// Called when /help is used on /rules.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Update after extra permissions are rewritten -Jjp137
            p.SendMessage("/rules - Displays the server's rules to you.");
            p.SendMessage("/rules [player?] - Displays the server's rules to a player.");
        }
    }
}

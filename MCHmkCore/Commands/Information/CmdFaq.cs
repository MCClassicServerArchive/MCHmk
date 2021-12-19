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
    public class CmdFaq : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"freq", "ask", "question"});

        public override string Name {
            get {
                return "faq";
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
        public CmdFaq(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            List<string> faq = new List<string>();
            if (!File.Exists("text/faq.txt")) {
                File.WriteAllText("text/faq.txt", "Example: What does this server run on? This server runs on &bMCHmk");
            }
            using (StreamReader r = File.OpenText("text/faq.txt")) {
                while (!r.EndOfStream) {
                    faq.Add(r.ReadLine());
                }
            }

            Player who = null;
            if (args != String.Empty) {
                if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                    p.SendMessage("You cant send the FAQ to another player!");
                    return;
                }
                who = _s.players.Find(args);
            }
            else {
                who = p;
            }

            if (who != null) {
                who.SendMessage("&cFAQ&f:");
                foreach (string s in faq) {
                    who.SendMessage("&f" + s);
                }
            }
            else {
                p.SendMessage("There is no player \"" + args + "\"!");
            }
        }

        /// <summary>
        /// Called when /help is used on /faq.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Update w/ extra permissions later -Jjp137
            p.SendMessage("/faq [player?]- Displays frequently asked questions.");
            p.SendMessage("If a player name is given, sends the FAQ to that player.");
        }
    }
}

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
using System.IO;

namespace MCHmk.Commands {
    public class CmdAgree : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"accept", "yes", "rules"});

        public override string Name {
            get {
                return "agree";
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
        public CmdAgree(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (_s.props.agreetorulesonentry == false) {
                p.SendMessage("This command can only be used if agree-to-rules-on-entry is enabled!");
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }

            var agreed = File.ReadAllText("ranks/agreed.txt");

            if (p.hasreadrules == false) {
                p.SendMessage("&9You must read /rules before agreeing!");
                return;
            }
            if ((agreed+" ").Contains(" " + p.name.ToLower() + " ")) { //Edited to prevent inner names from working.
                p.SendMessage("You have already agreed to the rules!");
                return;
            }
            p.agreed = true;
            p.SendMessage("Thank you for agreeing to follow the rules. You may now build and use commands!");
            string playerspath = "ranks/agreed.txt";
            if (File.Exists(playerspath)) {
                //We don't want player "test" to have already agreed if "nate" and "stew" agrred.
                // the preveious one, though, would put "natesteve" which also allows test
                //There is a better way, namely regular expressions, but I'll worry about that later.
                File.AppendAllText(playerspath, " " + p.name.ToLower());  //Ensures every name is seperated by a space.
            }
        }

        /// <summary>
        /// Called when /help is used on /agree.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/agree - Lets you agree to the rules when entering the server.");
        }
    }
}

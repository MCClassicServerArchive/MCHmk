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
	Copyright � 2011-2014 MCForge-Redux

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
    public class CmdAdminChat : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"admin", "chat", "opchat"});

        public override string Name {
            get {
                return "adminchat";
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
        public CmdAdminChat(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            p.adminchat = !p.adminchat;
            if (p.adminchat) {
                p.SendMessage("All messages will now be sent to admins only.");
            }
            else {
                p.SendMessage("Admin chat turned off.");
            }
        }
       
        /// <summary>
        /// Called when /help is used on /adminchat.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/adminchat - Makes all messages sent by you go to admins by default. Use again to turn off.");
        }
    }
}



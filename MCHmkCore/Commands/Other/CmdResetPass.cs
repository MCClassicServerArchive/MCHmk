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
    Written by Jack1312

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
    public class CmdResetPass : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"reset", "pass"});

        public override string Name {
            get {
                return "resetpass";
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
        public CmdResetPass(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(args);
            if (_s.props.server_owner == "Notch" || _s.props.server_owner == String.Empty) {
                p.SendMessage("Please tell the server owner to change the 'Server Owner' property.");
                return;
            }
            if (!p.IsConsole && _s.props.server_owner != p.name) {
                p.SendMessage("You're not the server owner!");
                return;
            }
            if (!p.IsConsole && p.adminpen) {
                p.SendMessage("You cannot reset a password while in the admin pen!");
                return;
            }
            if (who == null) {
                p.SendMessage("The specified player does not exist.");
                return;
            }
            if (!File.Exists("extra/passwords/" + who.uuid + ".dat")) {
                p.SendMessage("The player you specified does not have a password!");
                return;
            }
            try {
                File.Delete("extra/passwords/" + who.uuid + ".dat");
                p.SendMessage("The admin password has sucessfully been removed for " + who.color + who.name + "!");
            }
            catch (Exception e) {
                p.SendMessage("Password Deletion Failed. Please manually delete the file. It is in extra/passwords.");
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /resetpass.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/resetpass <player?> - Resets a player's admin password.");
            p.SendMessage("This command may be only used by the server owner.");
        }
    }
}

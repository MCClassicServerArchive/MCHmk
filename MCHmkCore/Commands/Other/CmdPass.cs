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

	Copyright 2012 MCForge

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

using MCHmk.Util;
namespace MCHmk.Commands {
    public class CmdPass : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"verify", "password", "user"});

        public override string Name {
            get {
                return "pass";
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
                return DefaultRankValue.Banned;
            }
        }

        public CmdPass(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.rank.Permission < _s.props.verifyadminsrank) {
                p.SendMessage("You do not have the &crequired rank to use this command!");
                return;
            }

            if (!_s.props.verifyadmins) {
                p.SendMessage("Verification of admins is &cdisabled!");
                return;
            }

            if (!p.adminpen) {
                p.SendMessage("You have &calready verified.");
                return;
            }

            if (p.passtries >= 3) {
                p.Kick("Did you really think you could keep on guessing?");
                return;
            }

            if (String.IsNullOrEmpty(args.Trim())) {
                Help(p);
                return;
            }

            int number = args.Split(' ').Length;

            if (number > 1) {
                p.SendMessage("Your password must be &cone " + _s.props.DefaultColor + "word!");
                return;
            }

            if (!Directory.Exists("extra/passwords")) {
                p.SendMessage("You have not &cset a password, " + _s.props.DefaultColor +
                                   "use &a/setpass <password?> &cto set one!");
                return;
            }

            if (!File.Exists("extra/passwords/" + p.uuid + ".dat")) {
                p.SendMessage("You have not &cset a password, " + _s.props.DefaultColor +
                                   "use &a/setpass <password?> &cto set one!");
                return;
            }

            if (PasswordHasher.MatchesPass(p.uuid.Value, args)) {
                p.SendMessage("Thank you, " + p.color + p.name + _s.props.DefaultColor + "! You have now &averified " +
                                   _s.props.DefaultColor + "and have &aaccess to admin commands and features!");
                if (p.adminpen) {
                    p.adminpen = false;
                }
                return;
            }

            p.passtries++;
            p.SendMessage("&cWrong Password. " + _s.props.DefaultColor + "Remember your password is &ccase sensitive!");
            p.SendMessage("Forgot your password? " + _s.props.DefaultColor + "Contact the owner so they can reset it!");
        }

        /// <summary>
        /// Called when /help is used on /pass.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/pass <password?> - If you are an admin, use this command " +
                               "to verify your login. You will need to use this to be given " +
                               "access to any commands.");
            p.SendMessage("If you do not have a password, use /setpass <password?>.");
        }
    }
}

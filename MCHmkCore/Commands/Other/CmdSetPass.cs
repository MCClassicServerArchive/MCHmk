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
    public class CmdSetPass : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"pass", "set"});

        public override string Name {
            get {
                return "setpass";
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
        public CmdSetPass(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!Directory.Exists("extra/passwords")) {
                Directory.CreateDirectory("extra/passwords");
            }
            if (p.rank.Permission < _s.props.verifyadminsrank) {
                p.SendMessage("You do not have the &crequired rank " + _s.props.DefaultColor + "to use this command!");
                return;
            }
            if (!_s.props.verifyadmins) {
                p.SendMessage("Verification of admins is &cdisabled!");
                return;
            }
            if (p.adminpen) {
                if (File.Exists("extra/passwords/" + p.uuid + ".dat")) {
                    p.SendMessage("&cYou already have a password set. " + _s.props.DefaultColor + "You &ccannot change " +
                                       _s.props.DefaultColor + "it unless &cyou verify it with &a/pass [Password]. " + _s.props.DefaultColor +
                                       "If you have &cforgotten " + _s.props.DefaultColor + "your password, contact &c" + _s.props.server_owner +
                                       _s.props.DefaultColor + " and they can &creset it!");
                    return;
                }
            }
            if (String.IsNullOrEmpty(args.Trim())) {
                Help(p);
                return;

            }
            int number = args.Split(' ').Length;
            if (number > 1) {
                p.SendMessage("Your password must be one word!");
                return;
            }
            PasswordHasher.StoreHash(p.uuid.Value, args);
            p.SendMessage("Your password has &asuccessfully &abeen set to:");
            p.SendMessage("&c" + args);
        }

        /// <summary>
        /// Called when /help is used on /setpass.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/setpass <password?> - Sets your admin password.");
            p.SendMessage("Warning: Do not set this as your Minecraft password!");
        }
    }
}

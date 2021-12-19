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

namespace MCHmk.Commands {
    public class CmdWarn : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"kick", "user"});

        public override string Name {
            get {
                return "warn";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "mod";
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
                return DefaultRankValue.Builder;
            }
        }

        public CmdWarn(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string warnedby;

            if (args == String.Empty) {
                Help(p);
                return;
            }

            Player who = _s.players.Find(args.Split(' ')[0]);

            // Make sure we have a valid player
            if (who == null) {
                p.SendMessage("Player not found!");
                return;
            }

            // Don't warn yourself... derp
            if (who == p) {
                p.SendMessage("You can't warn yourself.");
                return;
            }

            // Check the caller's rank
            if (!p.IsConsole && p.rank.Permission <= who.rank.Permission) {
                p.SendMessage("You can't warn a player of equal or higher rank.");
                return;
            }

            string reason;

            // We need a reason
            if (args.Split(' ').Length == 1) {
                // No reason was given
                reason = "you know why.";
            }
            else {
                reason = args.Substring(args.IndexOf(' ') + 1).Trim();
            }

            warnedby = (p.IsConsole) ? "<CONSOLE>" : p.color + p.name;
            _s.GlobalMessage(warnedby + " %ewarned " + who.color + who.name + " %ebecause:");
            _s.GlobalMessage("&c" + reason);

            if (who.warn == 0) {
                who.SendMessage("Do it again twice and you will get kicked!");
                who.warn = 1;
                return;
            }
            if (who.warn == 1) {
                who.SendMessage("Do it one more time and you will get kicked!");
                who.warn = 2;
                return;
            }
            if (who.warn == 2) {
                _s.GlobalMessage(who.color + who.name + " " + _s.props.DefaultColor + "was warn-kicked by " + warnedby);
                who.warn = 0;
                who.Kick("KICKED BECAUSE " + reason + String.Empty);
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /warn.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/warn <player?> [reason?] - Warns a player.");
            p.SendMessage("Players will get kicked after 3 warnings.");
        }
    }
}

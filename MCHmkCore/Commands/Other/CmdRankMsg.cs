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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdRankMsg : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "msg", "edit", "set"});

        public override string Name {
            get {
                return "rankmsg";
            }
        }
        public override string Shortcut {
            get {
                return "rm";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdRankMsg(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string[] command = args.ToLower().Split(' ');
            string msg1 = String.Empty;
            string msg2 = String.Empty;
            try {
                msg1 = command[0];
                msg2 = command[1];
            }
            catch {  // TODO: find exact exception to catch
            }
            if (msg1 == String.Empty) {
                Help(p);
                return;
            }
            if (msg2 == String.Empty) {
                _s.commands.FindCommand("rankmsg").Use(p, p.rank.name + " " + msg1);
                return;
            }
            Rank findgroup = _s.ranks.Find(msg1);
            if (findgroup == null) {
                p.SendMessage("Could not find group specified!");
                return;
            }
            foreach (Player pl in _s.players) {
                if (pl.rank.name == findgroup.name) {
                    pl.SendMessage(p.color + p.name + ": " + _s.props.DefaultColor + (args.Replace(msg1, String.Empty).Trim()));
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /rankmsg.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/rankmsg [rank?] <message?> - " +
                               "Sends a message to players belong to a given rank.");
            p.SendMessage("If a rank is not given, the message is sent to those of " +
                               "your current rank.");
        }
    }
}

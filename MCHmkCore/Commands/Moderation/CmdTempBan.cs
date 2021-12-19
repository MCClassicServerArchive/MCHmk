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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdTempBan : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"temp", "ban"});

        public override string Name {
            get {
                return "tempban";
            }
        }
        public override string Shortcut {
            get {
                return "tb";
            }
        }
        public override string Type {
            get {
                return "moderation";
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
        public CmdTempBan(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (args.IndexOf(' ') == -1) {
                args = args + " 60";
            }

            Player who = _s.players.Find(args.Split(' ')[0]);
            if (who == null) {
                p.SendMessage("Could not find player");
                return;
            }
            if (!p.IsConsole && who.rank.Permission >= p.rank.Permission) {
                p.SendMessage("Cannot ban someone of the same rank");
                return;
            }
            int minutes;
            try {
                minutes = int.Parse(args.Split(' ')[1]);
            }
            catch {  // TODO: Find exact exception to catch
                p.SendMessage("Invalid minutes");
                return;
            }
            if (minutes > 1440) {
                p.SendMessage("Cannot ban for more than a day");
                return;
            }
            if (minutes < 1) {
                p.SendMessage("Cannot ban someone for less than a minute");
                return;
            }

            Server.TempBan tBan;
            tBan.name = who.name;
            tBan.allowedJoin = DateTime.Now.AddMinutes(minutes);
            _s.tempBans.Add(tBan);
            who.Kick("Banned for " + minutes + " minutes!");
        }

        /// <summary>
        /// Called when /help is used on /tempban.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tempban <player?> <minutes?> - Bans a player for the specified amount of minutes.");
            p.SendMessage("The maximum time is 1440, which is one day. Default is 60.");
            p.SendMessage("Temporary bans will reset when the server is restarted.");
        }
    }
}

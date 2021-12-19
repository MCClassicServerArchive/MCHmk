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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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
using System.Data;
using System.IO;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdPCount : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"player", "online", "total", "number", "count"});

        public override string Name {
            get {
                return "pcount";
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
        public CmdPCount(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            int bancount = _s.ranks.FindPerm(DefaultRankValue.Banned).playerList.Count;

            Int64 count = _s.database.ExecuteCount("SELECT COUNT(id) FROM Players");
            p.SendMessage("A total of " + count.ToString() + " unique players have visited this server.");
            p.SendMessage("Of these players, " + bancount + " have been banned.");

            int playerCount = 0;
            int hiddenCount = 0;

            foreach (Player pl in _s.players) {
                if (!pl.hidden || p.IsConsole || p.rank.Permission > DefaultRankValue.AdvBuilder) {
                    playerCount++;
                    if (pl.hidden && pl.rank.Permission <= p.rank.Permission && (p.IsConsole
                            || p.rank.Permission > DefaultRankValue.AdvBuilder)) {
                        hiddenCount++;
                    }
                }
            }
            if (playerCount == 1) {
                if (hiddenCount == 0) {
                    p.SendMessage("There is 1 player currently online.");
                }
                else {
                    p.SendMessage("There is 1 player currently online (" + hiddenCount + " hidden).");
                }
            }
            else {
                if (hiddenCount == 0) {
                    p.SendMessage("There are " + playerCount + " players online.");
                }
                else {
                    p.SendMessage("There are " + playerCount + " players online (" + hiddenCount + " hidden).");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /pcount.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/pcount - Displays the number of players online, the " +
                               "total amount of players, and the number of players that have been banned.");
        }
    }
}

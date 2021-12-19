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
using System.Data;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdTopTen : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"top", "ten", "user"});

        public override string Name {
            get {
                return "topten";
            }
        }
        public override string Shortcut {
            get {
                return "10";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdTopTen(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            if (String.Compare(args, "1", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, totallogin FROM Players order by totallogin desc limit 10");

                p.SendMessage("TOP TEN NUMBER OF LOGINS:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" +
                                       playerDb.Rows[i]["TotalLogin"].ToString() + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "2", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, totaldeaths FROM Players order by totaldeaths desc limit 10");

                p.SendMessage("TOP TEN NUMBER OF DEATHS:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" +
                                       playerDb.Rows[i]["TotalDeaths"].ToString() + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "3", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, money FROM Players order by money desc limit 10");

                p.SendMessage("TOP TEN AMOUNTS OF MONEY:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" +
                                       playerDb.Rows[i]["money"].ToString() + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "4", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, firstlogin FROM Players order by firstlogin asc limit 10");

                p.SendMessage("FIRST PLAYERS:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" +
                                       playerDb.Rows[i]["firstlogin"].ToString() + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "5", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, lastlogin  FROM Players order by lastlogin desc limit 10");

                p.SendMessage("MOST RECENT PLAYERS:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" + playerDb.Rows[i]["lastlogin"] + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "6", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, totalblocks FROM Players order by totalblocks desc limit 10");

                p.SendMessage("TOP TEN NUMBER OF BLOCKS MODIFIED:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - ["
                                       + playerDb.Rows[i]["TotalBlocks"].ToString() + "]");
                }

                playerDb.Dispose();
            }
            if (String.Compare(args, "7", true) == 0) {
                DataTable playerDb = _s.database.ObtainData("SELECT distinct name, totalkicked FROM Players order by totalkicked desc limit 10");

                p.SendMessage("TOP TEN NUMBER OF KICKS:");
                for (int i = 0; i < playerDb.Rows.Count; i++) {
                    p.SendMessage((i + 1) + ") " + playerDb.Rows[i]["Name"] + " - [" +
                                       playerDb.Rows[i]["TotalKicked"].ToString() + "]");
                }

                playerDb.Dispose();
            }
        }

        /// <summary>
        /// Called when /help is used on /topten.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/topten <number?> - Prints a particular top ten list.");
            p.SendMessage("The lists are:");
            p.SendMessage("1 - Number of Logins");
            p.SendMessage("2 - Number of Deaths");
            p.SendMessage("3 - Money");
            p.SendMessage("4 - First Players");
            p.SendMessage("5 - Recent Players");
            p.SendMessage("6 - Blocks Modified");
            p.SendMessage("7 - Number of Kicks");
        }
    }
}

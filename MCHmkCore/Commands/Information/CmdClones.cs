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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text;

using MCHmk.SQL;

namespace MCHmk.Commands {
    class CmdClones : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"ip", "mod", "player"});

        public override string Name {
            get {
                return "clones";
            }
        }
        public override string Shortcut {
            get {
                return "alts";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdClones(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // Fix an exploit (or at least patch it up)
            if (!Player.ValidName(args)) {
                p.SendMessage("Invalid player name.");
                return;
            }

            if (args == String.Empty) {
                args = p.name;
            }

            string originalName = args.ToLower();

            Player who = _s.players.Find(args);
            if (who == null) {
                p.SendMessage("Could not find player. Searching Player DB.");

                // FIXME: PreparedStatement
                DataTable FindIP = _s.database.ObtainData("SELECT IP FROM Players WHERE Name='" + args + "'");

                if (FindIP.Rows.Count == 0) {
                    p.SendMessage("Could not find any player by the name entered.");
                    FindIP.Dispose();
                    return;
                }

                args = FindIP.Rows[0]["IP"].ToString();
                FindIP.Dispose();
            }
            else {
                args = who.ip;
            }

            // FIXME: PreparedStatement
            DataTable Clones = _s.database.ObtainData("SELECT Name FROM Players WHERE IP='" + args + "'");

            if (Clones.Rows.Count == 0) {
                p.SendMessage("Could not find any record of the player entered.");
                return;
            }

            List<string> foundPeople = new List<string>();
            for (int i = 0; i < Clones.Rows.Count; ++i) {
                if (!foundPeople.Contains(Clones.Rows[i]["Name"].ToString().ToLower())) {
                    foundPeople.Add(Clones.Rows[i]["Name"].ToString().ToLower());
                }
            }

            Clones.Dispose();
            if (foundPeople.Count <= 1) {
                p.SendMessage(originalName + " has no clones.");
                return;
            }

            p.SendMessage("These people have the same IP address:");
            p.SendMessage(string.Join(", ", foundPeople.ToArray()));
        }

        /// <summary>
        /// Called when /help is used on /clones.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/clones <player?> - Finds everyone with the same IP address as the specified player.");
        }
    }
}

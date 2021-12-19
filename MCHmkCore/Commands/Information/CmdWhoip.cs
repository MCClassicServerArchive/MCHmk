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
using System.IO;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdWhoip : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"who", "player", "info", "ip"});

        public override string Name {
            get {
                return "whoip";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdWhoip(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (args.IndexOf("'") != -1) {
                p.SendMessage("Not a valid IP.");
                return;
            }

            // FIXME: PreparedStatement
            string query = "SELECT Name FROM Players WHERE IP='" + args + "'";
            DataTable playerDb = _s.database.ObtainData(query);

            if (playerDb.Rows.Count == 0) {
                p.SendMessage("Could not find anyone with this IP");
                return;
            }

            string playerNames = "Players with this IP: ";

            for (int i = 0; i < playerDb.Rows.Count; i++) {
                playerNames += playerDb.Rows[i]["Name"] + ", ";
            }
            playerNames = playerNames.Remove(playerNames.Length - 2);

            p.SendMessage(playerNames);
            playerDb.Dispose();
        }

        /// <summary>
        /// Called when /help is used on /whoip.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/whoip <ip?> - Displays players associated with a given IP address.");
        }
    }
}

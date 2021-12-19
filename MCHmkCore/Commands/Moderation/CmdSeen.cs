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
using System.Linq;
using System.Text;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdSeen : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"saw", "last", "user"});

        public override string Name {
            get {
                return "seen";
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
                return DefaultRankValue.Banned;
            }
        }
        public CmdSeen(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // Fix an exploit (or at least patch it up)
            if (!Player.ValidName(args)) {
                p.SendMessage("Invalid player name.");
                return;
            }

            Player pl = _s.players.Find(args);
            if (pl != null && !pl.hidden) {
                p.SendMessage(pl.color + pl.name + _s.props.DefaultColor + " is currently online.");
                return;
            }

            // FIXME: PreparedStatement
            using (DataTable playerDb = _s.database.ObtainData("SELECT * FROM Players WHERE Name='" + args + "'")) {
                if (playerDb.Rows != null && playerDb.Rows.Count > 0) {
                    p.SendMessage(args + " was last seen: " + playerDb.Rows[0]["LastLogin"]);
                }
                else {
                    p.SendMessage("Unable to find player");
                }
            }

        }

        /// <summary>
        /// Called when /help is used on /seen.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/seen <player?> - Displays when a player was last seen on the server.");
        }
    }
}

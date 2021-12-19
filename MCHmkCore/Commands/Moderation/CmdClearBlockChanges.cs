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
    /// <summary>
    /// The code for the /clearblockchanges command, which clears a level's block change history.
    /// </summary>
    public class CmdClearBlockChanges : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"about", "block", "change", "remove", "del"});

        public override string Name {
            get {
                return "clearblockchanges";
            }
        }
        public override string Shortcut {
            get {
                return "cbc";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }

        public CmdClearBlockChanges(Server s) : base(s) { }

        /// <summary>
        /// The code that runs when /clearblockchanges is called.
        /// </summary>
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }

            Level l = _s.levels.Find(args);
            if (l == null && args != String.Empty) {
                p.SendMessage("Could not find level.");
                return;
            }
            if (l == null) {
                l = p.level;
            }

            _s.database.ExecuteStatement("DELETE FROM Block" + l.name);

            p.SendMessage("Cleared &cALL" + _s.props.DefaultColor + " recorded block changes in: &d" + l.name);
        }

        /// <summary>
        /// Called when /help is used on /clearblockchanges.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/clearblockchanges [map?] - Clears a map's block change history.");
            p.SendMessage("By default, the current map's history is cleared.");
            p.SendMessage("Be careful! This can not be undone.");
        }
    }
}

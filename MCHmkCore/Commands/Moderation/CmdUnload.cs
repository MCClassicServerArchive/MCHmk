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
    public class CmdUnload : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"load", "un", "map", "level", "lvl"});

        public override string Name {
            get {
                return "unload";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdUnload(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.ToLower() == "empty") {
                UnloadEmptyLevels(p);
                return;
            }

            Level level = _s.levels.FindExact(args);
            if (level != null) {
                if (!level.Unload()) {
                    p.SendMessage("You cannot unload the main level.");
                }
                return;
            }

            p.SendMessage("There is no level '" + args + "' loaded.");
        }

        private void UnloadEmptyLevels(Player p) {
            int emptyLevels = 0;
            // Fix taken from MCGalaxy
            for (int i = 0; i < _s.levels.Count; i++) {
                Level l = _s.levels[i];
                if (l.players.Count <= 0 && l.Unload(true)) {
                    emptyLevels++;
                    i--;  // Unload() removes the level from _s.levels, so subtract 1 to account for that
                }
            }
            if (emptyLevels > 0) {
                string noun = (emptyLevels == 1) ? "level" : "levels";
                p.SendMessage("Unloaded " + emptyLevels.ToString() + " empty " + noun + ".");
            }
            else {
                p.SendMessage("There were no empty levels to unload.");
            }
        }

        /// <summary>
        /// Called when /help is used on /unload.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/unload <map?> - Unloads a map.");
            p.SendMessage("/unload empty - Unloads all empty maps.");
            p.SendMessage("The main level cannot be unloaded.");
        }
    }
}

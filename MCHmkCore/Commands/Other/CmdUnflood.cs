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
    public class CmdUnflood : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"flood", "un", "restore"});

        public override string Name {
            get {
                return "unflood";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }

        public CmdUnflood(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (String.IsNullOrEmpty(args)) {
                Help(p);
                return;
            }
            if (args.ToLower() != "all" && BlockData.Ushort(args) == BlockId.Null) {
                p.SendMessage("There is no block \"" + args + "\".");
                return;
            }
            int phys = p.level.physics;
            _s.commands.FindCommand("physics").Use(p, "0");
            if (!p.level.Instant) {
                _s.commands.FindCommand("map").Use(p, "instant");
            }

            if (args.ToLower() == "all") {
                _s.commands.FindCommand("replaceall").Use(p, "lavafall air");
                _s.commands.FindCommand("replaceall").Use(p, "waterfall air");
                _s.commands.FindCommand("replaceall").Use(p, "lava_fast air");
                _s.commands.FindCommand("replaceall").Use(p, "active_lava air");
                _s.commands.FindCommand("replaceall").Use(p, "active_water air");
                _s.commands.FindCommand("replaceall").Use(p, "active_hot_lava air");
                _s.commands.FindCommand("replaceall").Use(p, "active_cold_water air");
                _s.commands.FindCommand("replaceall").Use(p, "fast_hot_lava air");
                _s.commands.FindCommand("replaceall").Use(p, "magma air");
            }
            else {
                _s.commands.FindCommand("replaceall").Use(p, args + " air");
            }

            if (p.level.Instant) {
                _s.commands.FindCommand("map").Use(p, "instant");
            }
            _s.commands.FindCommand("reveal").Use(p, "all");
            _s.commands.FindCommand("physics").Use(p, phys.ToString());
            _s.GlobalMessage("Unflooded!");
        }
        
        /// <summary>
        /// Called when /help is used on /unflood.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/unflood <liquid?> - Unfloods the current map of the " +
                               "specified active liquid.");
            p.SendMessage("/unflood all - Unfloods the current map of all active liquids.");
        }
    }
}

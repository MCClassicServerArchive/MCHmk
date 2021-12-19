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
    public class CmdOZone : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"zone", "map", "lvl", "level", "entire"});

        public override string Name {
            get {
                return "ozone";
            }
        }
        public override string Shortcut {
            get {
                return "oz";
            }
        }
        public override string Type {
            get {
                return "mod";
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
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

        public CmdOZone(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                this.Help(p);
            }
            else {
                int x2 = p.level.width - 1;
                int y2 = p.level.height - 1;
                int z2 = p.level.depth - 1;
                Command zone = _s.commands.FindCommand("zone");
                Command click = _s.commands.FindCommand("click");
                zone.Use(p, "add " + args);
                click.Use(p, 0 + " " + 0 + " " + 0);
                click.Use(p, x2 + " " + y2 + " " + z2);
            }
        }
        
        /// <summary>
        /// Called when /help is used on /ozone.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/ozone <rank?/player?> - Zones the entire map to the specified rank or player.");
            p.SendMessage("To delete this zone, just use /zone del anywhere on the map.");
        }
    }
}

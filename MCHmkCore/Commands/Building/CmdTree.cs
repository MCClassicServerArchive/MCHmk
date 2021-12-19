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

namespace MCHmk.Commands {
    public class CmdTree : Command {
        private readonly string _typeKey = "tree_type";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"log"});

        public override string Name {
            get {
                return "tree";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "build";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdTree(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            p.ClearSelection();
            p.painting = false;

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_typeKey] = args;

            p.StartSelection(BlockSelected, data);
            p.SendMessage("Place a block where the tree should grow.");
        }

        private void BlockSelected(Player p, CommandTempData c) {
            p.ClearSelection();

            string type = c.GetData<string>(_typeKey);

            switch (type.ToLower()) {
                case "2":
                case "cactus":
                    _s.MapGen.AddCactus(p.level, c.X, c.Y, c.Z, p.random, true, true, p);
                    break;
                case "3":
                case "notch":
                    _s.MapGen.AddNotchTree(p.level, c.X, c.Y, c.Z, p.random, true, true, p);
                    break;
                case "4":
                case "swamp":
                    _s.MapGen.AddNotchSwampTree(p.level, c.X, c.Y, c.Z, p.random, true, true, p);
                    break;
                default:
                    _s.MapGen.AddTree(p.level, c.X, c.Y, c.Z, p.random, true, true, p);
                    break;
            }

            HandleStaticMode(p, c);
        }

        private void HandleStaticMode(Player p, CommandTempData c) {
            if (!p.staticCommands) {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_typeKey] = c.GetData<string>(_typeKey);

            p.StartSelection(BlockSelected, data);
        }

        /// <summary>
        /// Called when /help is used on /tree.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tree [type?] - Places a tree.");
            p.SendMessage("Tree types: fern (1), cactus (2), notch (3), swamp (4)");
            p.SendMessage("A fern tree is placed by default.");
        }
    }
}

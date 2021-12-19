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

	http://www.osedu.org/licenses/ECL-2.0
	http://www.gnu.org/licenses/gpl-3.0.html

	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdReload : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"level", "lvl", "map"});

        public override string Name {
            get {
                return "reload";
            }
        }
        public override string Shortcut {
            get {
                return "rd";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdReload(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            if (!File.Exists("levels/" + args + ".mcf")) {
                p.SendMessage("The specified level does not exist!");
                return;
            }
            if (_s.mainLevel.name == args) {
                p.SendMessage("You cannot reload the main level!");
                return;
            }
            if (p.IsConsole) {
                foreach (Player pl in _s.players) {
                    if (pl.level.name.ToLower() == args.ToLower()) {
                        _s.commands.FindCommand("unload").Use(p, args);
                        _s.commands.FindCommand("load").Use(p, args);
                        _s.commands.FindCommand("goto").Use(pl, args);
                    }
                }
                _s.GlobalMessage("&cThe map, " + args + " has been reloaded!");
                _s.IRC.Say("The map, " + args + " has been reloaded.");
                _s.logger.Log("The map, " + args + " was reloaded by the console");
                return;
            }
            if (!p.IsConsole) {
                foreach (Player pl in _s.players) {
                    if (pl.level.name.ToLower() == args.ToLower()) {
                        _s.commands.FindCommand("unload").Use(p, args);
                        _s.commands.FindCommand("load").Use(p, args);
                        _s.commands.FindCommand("goto").Use(pl, args);
                    }
                }
                _s.GlobalMessage("&cThe map, " + args + " has been reloaded!");
                _s.IRC.Say("The map, " + args + " has been reloaded.");
                _s.logger.Log("The map, " + args + " was reloaded by " + p.name);
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /reload.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/reload [map?] - Reloads a map. ");
            p.SendMessage("Reloads the current map if a map name is not given.");
        }
    }
}

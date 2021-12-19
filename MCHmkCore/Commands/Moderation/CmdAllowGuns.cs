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
    Written by Jack1312
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
using System.IO;

namespace MCHmk.Commands {
    public class CmdAllowGuns : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"allow", "gun", "missile", "shoot", "boom", "terrorism"});

        public override string Name {
            get {
                return "allowguns";
            }
        }
        public override string Shortcut {
            get {
                return "ag";
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
        public CmdAllowGuns(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (String.IsNullOrEmpty(args)) {
                if (p.level.guns) {
                    p.level.guns = false;
                    _s.GlobalMessage("&9Gun usage has been disabled on &c" + p.level.name + "&9!");
                    p.level.SaveSettings();

                    foreach (Player pl in _s.players)
                        if (pl.level == p.level) {
                            pl.aiming = false;
                        }
                }
                else {
                    p.level.guns = true;
                    _s.GlobalMessage("&9Gun usage has been enabled on &c" + p.level.name + "&9!");
                    p.level.SaveSettings();
                }
                return;
            }

            if (!p.IsConsole) {
                Level foundLevel;
                if (args == String.Empty) {
                    if (p.level.guns == true) {
                        p.level.guns = false;
                        _s.GlobalMessage("&9Gun usage has been disabled on &c" + p.level.name + "&9!");
                        p.level.SaveSettings();
                        foreach (Player pl in _s.players) {
                            if (pl.level.name.ToLower() == p.level.name.ToLower()) {
                                pl.aiming = false;
                                p.aiming = false;
                                return;
                            }
                            return;
                        }
                        return;
                    }
                    if (p.level.guns == false) {
                        p.level.guns = true;
                        _s.GlobalMessage("&9Gun usage has been enabled on &c" + p.level.name + "&9!");
                        p.level.SaveSettings();
                        return;
                    }
                }

                if (args != String.Empty) {
                    foundLevel = _s.levels.Find(args);
                    if (!File.Exists("levels/" + args + ".mcf")) {
                        p.SendMessage("&9The level, &c" + args + " &9does not exist!");
                        return;
                    }
                    if (foundLevel.guns == true) {
                        foundLevel.guns = false;
                        _s.GlobalMessage("&9Gun usage has been disabled on &c" + args + "&9!");
                        foundLevel.SaveSettings();
                        foreach (Player pl in _s.players) {
                            if (pl.level.name.ToLower() == args.ToLower()) {
                                pl.aiming = false;
                            }
                            if (p.level.name.ToLower() == args.ToLower()) {
                                p.aiming = false;

                            }
                        }
                        return;
                    }
                    else {
                        foundLevel.guns = true;
                        _s.GlobalMessage("&9Gun usage has been enabled on &c" + args + "&9!");
                        foundLevel.SaveSettings();
                        return;
                    }
                }
            }
            if (p.IsConsole) {
                if (args == null) {
                    p.SendMessage("You must specify a level!");
                    return;
                }
                Level foundLevel;
                foundLevel = _s.levels.Find(args);
                if (!File.Exists("levels/" + args + ".mcf")) {
                    p.SendMessage("The level, " + args + " does not exist!");
                    return;
                }
                if (foundLevel.guns == true) {
                    foundLevel.guns = false;
                    _s.GlobalMessage("&9Gun usage has been disabled on &c" + args + "&9!");
                    foundLevel.SaveSettings();
                    p.SendMessage("Gun usage has been disabled on " + args + "!");
                    foreach (Player pl in _s.players) {
                        if (pl.level.name.ToLower() == args.ToLower()) {
                            pl.aiming = false;
                            return;
                        }
                    }
                }
                foundLevel.guns = true;
                _s.GlobalMessage("&9Gun usage has been enabled on &c" + args + "&9!");
                foundLevel.SaveSettings();
                p.SendMessage("Gun usage has been enabled on " + args + "!");
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /allowguns.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/allowguns [map?] - Allow/disallow guns and missiles on the " +
                               "specified map, or the current map if no map name is given.");
            p.SendMessage("Note: If guns are allowed on a map, and /allowguns is used on "
                               + "that map, all guns and missiles will be disabled.");
        }
    }
}

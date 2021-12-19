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
using System.IO;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdDeleteLvl : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"delete", "remove", "level", "lvl"});

        public override string Name {
            get {
                return "deletelvl";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }
        public CmdDeleteLvl(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.Split().Length > 1) {
                Help(p);
                return;
            }
            Level foundLevel = _s.levels.FindExact(args);
            if (foundLevel != null) {
                if (foundLevel.permissionbuild > p.rank.Permission) {
                    p.SendMessage("%cYou can't delete levels with a perbuild rank higher than yours!");
                    return;
                }
                foundLevel.Unload();
            }

            if (foundLevel == _s.mainLevel) {
                p.SendMessage("Cannot delete the main level.");
                return;
            }

            try {
                if (!Directory.Exists("levels/deleted")) {
                    Directory.CreateDirectory("levels/deleted");
                }

                if (File.Exists("levels/" + args + ".mcf")) {

                    using (StreamReader reader = new StreamReader("levels/level properties/" + args + ".properties")) {
                        string line;
                        while ((line = reader.ReadLine()) != null) {
                            if (line[0] == '#') {
                                continue;
                            }
                            if (line.Split()[0].ToLower() == "perbuild") {
                                if (_s.ranks.PermFromName(line.Split()[2].ToLower()) > p.rank.Permission) {
                                    p.SendMessage("%cYou can't delete levels with a perbuild rank higher than yours!");
                                    return;
                                }
                                break;
                            }
                        }
                    }

                    if (File.Exists("levels/deleted/" + args + ".mcf")) {
                        int currentNum = 0;
                        while (File.Exists("levels/deleted/" + args + currentNum + ".mcf")) {
                            currentNum++;
                        }

                        File.Move("levels/" + args + ".mcf", "levels/deleted/" + args + currentNum + ".mcf");
                    }
                    else {
                        File.Move("levels/" + args + ".mcf", "levels/deleted/" + args + ".mcf");
                    }
                    p.SendMessage("Created backup.");

                    try {
                        File.Delete("levels/level properties/" + args + ".properties");
                    }
                    catch (Exception e) { 
                        _s.logger.ErrorLog(e);
                    }
                    try {
                        File.Delete("levels/level properties/" + args);
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }

                    // FIXME: transaction
                    _s.database.ExecuteStatement("DROP TABLE \"Block" + args + "\"");
                    _s.database.ExecuteStatement("DROP TABLE \"Portals" + args + "\"");
                    _s.database.ExecuteStatement("DROP TABLE \"Messages" + args + "\"");
                    _s.database.ExecuteStatement("DROP TABLE \"Zone" + args + "\"");

                    _s.GlobalMessage("Level " + args + " was deleted.");
                }
                else {
                    p.SendMessage("Could not find specified level.");
                }
            }
            catch (Exception e) {
                p.SendMessage("Error when deleting.");
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /deletelvl.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/deletelvl <map?> - Completely deletes a map " +
                               "and all of its mb, portal, zone, and block change data.");
            p.SendMessage("A backup of the map will be placed in the levels/deleted folder.");
        }
    }
}

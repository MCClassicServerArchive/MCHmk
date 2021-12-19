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
using System.Data;
using System.IO;

namespace MCHmk.Commands {
    public class CmdSave : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"store", "load"});

        public override string Name {
            get {
                return "save";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdSave(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.ToLower() == "all") {
                foreach (Level l in _s.levels) {
                    try {
                        l.Save();
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                }
                _s.GlobalMessage("All levels have been saved.");
            }
            else {
                if (args == String.Empty) { // for empty string/no parameters.
                    if (p.IsConsole) {
                        Use(p, "all");
                    }
                    else {
                        p.level.Save(true);
                        p.SendMessage("Level \"" + p.level.name + "\" saved.");

                        int backupNumber = p.level.Backup(true);
                        if (backupNumber != -1) {
                            // Notify console and the player who called /save
                            _s.logger.Log("Backup " + backupNumber.ToString() + " saved for " + p.level.name);
                            if (!p.IsConsole) {
                                p.level.ChatLevel("Backup " + backupNumber.ToString() + " saved.");
                            }
                        }
                    }
                }
                else if (args.Split(' ').Length == 1) {   //Just save level given
                    Level foundLevel = _s.levels.Find(args);
                    if (foundLevel != null) {
                        foundLevel.Save(true);
                        p.SendMessage("Level \"" + foundLevel.name + "\" saved.");
                        int backupNumber = foundLevel.Backup(true);
                        if (backupNumber != -1) {
                            // Notify console and the player who called /save
                            _s.logger.Log("Backup " + backupNumber.ToString() + " saved for " + foundLevel.name);
                            if (!p.IsConsole) {
                                p.level.ChatLevel("Backup " + backupNumber.ToString() + " saved.");
                            }
                        }
                    }
                    else {
                        p.SendMessage("Could not find level specified");
                    }
                }
                else if (args.Split(' ').Length == 2) {
                    Level foundLevel = _s.levels.Find(args.Split(' ')[0]);
                    string restoreName = args.Split(' ')[1].ToLower();
                    if (foundLevel != null) {
                        foundLevel.Save(true);
                        p.level.Backup(true, restoreName);
                        _s.GlobalMessage(foundLevel.name + " had a backup created named &b" + restoreName);
                        _s.logger.Log(foundLevel.name + " had a backup created named &b" + restoreName);
                    }
                    else {
                        p.SendMessage("Could not find level specified");
                    }
                }
                else {   // Invalid number of arguments
                    Help(p);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /save.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/save - Saves the map that you are currently in.");
            p.SendMessage("/save all - Saves all loaded maps.");
            p.SendMessage("/save <map?> - Saves the specified map.");
            p.SendMessage("/save <map?> <name?> - Backups the map with a given name.");
        }
    }
}

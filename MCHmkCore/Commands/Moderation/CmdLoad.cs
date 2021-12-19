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
using System.IO;
using System.Threading;

namespace MCHmk.Commands {
    public class CmdLoad : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"level", "map", "lvl"});

        public override string Name {
            get {
                return "load";
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
        public CmdLoad(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                if (args == String.Empty) {
                    Help(p);
                    return;
                }
                if (args.Split(' ').Length > 2) {
                    Help(p);
                    return;
                }
                int pos = args.IndexOf(' ');
                string phys = "0";
                if (pos != -1) {
                    phys = args.Substring(pos + 1);
                    args = args.Substring(0, pos).ToLower();
                }
                else {
                    args = args.ToLower();
                }

                while (_s.levels == null) {
                    Thread.Sleep(100);    // Do nothing while we wait on the levels list...
                }

                foreach (Level l in _s.levels) {
                    if (l.name == args) {
                        p.SendMessage(args + " is already loaded!");
                        return;
                    }
                }

                if (_s.levels.Count == _s.levels.MaxLevels) {
                    if (_s.levels.MaxLevels == 1) {
                        p.SendMessage("You can't load any levels!");
                    }
                    else {
                        _s.commands.FindCommand("unload").Use(p, "empty");
                        if (_s.levels.MaxLevels == 1) {
                            p.SendMessage("No maps are empty to unload. Cannot load map.");
                            return;
                        }
                    }
                }

                // Prioritize the new level format, but try loading a level saved in the older
                // format if it has not been saved under the new format yet.
                Level level = null;
                if (File.Exists("levels/" + args + ".mcf")) {
                    level = Level.Load(_s, args, 0, false);
                }
                else if (File.Exists("levels/" + args + ".lvl")) {
                    level = Level.Load(_s, args, 0, true);
                }
                else {
                    p.SendMessage("Level \"" + args + "\" doesn't exist!");
                    return;
                }

                if (level == null) {
                    if (File.Exists("levels/" + args + ".lvl.backup")) {
                        if (File.Exists("levels/" + args + ".mcf")) {
                            _s.logger.Log(args + ".mcf file is corrupt. Deleting and replacing with " + args + ".lvl.backup file.");
                            File.Delete("levels/" + args + ".mcf");
                        }
                        _s.logger.Log("Attempting to load backup");
                        File.Copy("levels/" + args + ".lvl.backup", "levels/" + args + ".mcf", true);
                        level = Level.Load(_s, args);
                        if (level == null) {
                            p.SendMessage("Loading backup failed.");
                            string backupPath = @_s.props.backupLocation;
                            if (Directory.Exists(backupPath + "/" + args)) {
                                int backupNumber = Directory.GetDirectories(backupPath + "/" + args).Length;
                                _s.logger.Log("Attempting to load latest backup, number " + backupNumber + " instead.");
                                File.Copy(backupPath + "/" + args + "/" + backupNumber + "/" + args + ".mcf", "levels/" + args + ".mcf", true);
                                level = Level.Load(_s, args);
                                if (level == null) {
                                    p.SendMessage("Loading latest backup failed as well.");
                                }
                            }
                            return;
                        }
                    }
                    else {
                        p.SendMessage("Backup of " + args + " does not exist.");
                        return;
                    }
                }

                if (!p.IsConsole) {
                    if (level.permissionvisit > p.rank.Permission) {
                        p.SendMessage("This map is for " + _s.ranks.PermToName(level.permissionvisit) + " only!");
                        return;
                    }
                }

                foreach (Level l in _s.levels) {
                    if (l.name == args) {
                        p.SendMessage(args + " is already loaded!");
                        return;
                    }
                }

                lock (_s.levels) {
                    _s.addLevel(level);
                }
                _s.GlobalMessage("Level \"" + level.name + "\" loaded.");

                try {
                    int temp = int.Parse(phys);
                    if (temp >= 1 && temp <= 5) {
                        level.setPhysics(temp);
                    }
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Physics variable invalid");
                }
            }
            catch (Exception e) {
                _s.GlobalMessage("An error occured with /load");
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /load.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/load <map?> [physics?] - Loads a map.");
            p.SendMessage("If a physics setting is given, loads the map with those physics.");
        }
    }
}

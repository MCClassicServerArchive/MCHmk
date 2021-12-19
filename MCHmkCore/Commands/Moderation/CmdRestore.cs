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

namespace MCHmk.Commands {
    public class CmdRestore : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"old", "rest", "copy"});

        public override string Name {
            get {
                return "restore";
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
        public CmdRestore(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Level lvl;
                string[] text = new string[2];
                text[0] = String.Empty;
                text[1] = String.Empty;
                try {
                    text[0] = args.Split(' ')[0].ToLower();
                    text[1] = args.Split(' ')[1].ToLower();
                }
                catch {  // TODO: find exact exception to catch
                    text[1] = p.level.name;
                }
                if (args.Split(' ').Length >= 2) {
                    lvl = _s.levels.FindExact(text[1]);
                    if (lvl == null) {
                        p.SendMessage("Level not found!");
                        return;
                    }
                }
                else {
                    if (!p.IsConsole && p.level != null) {
                        lvl = p.level;
                    }
                    else {
                        _s.logger.Log("u dun derped, specify the level, silly head");
                        return;
                    }
                }

                // Default to the older level format unless the level has an .mcf file associated with it.
                string extension = ".lvl";
                if (File.Exists(@_s.props.backupLocation + "/" + lvl.name + "/" + text[0] + "/" + lvl.name + ".mcf")) {
                    extension = ".mcf";
                }

                _s.logger.Log(@_s.props.backupLocation + "/" + lvl.name + "/" + text[0] + "/" + lvl.name + extension);
                if (File.Exists(@_s.props.backupLocation + "/" + lvl.name + "/" + text[0] + "/" + lvl.name + extension)) {
                    try {
                        File.Copy(@_s.props.backupLocation + "/" + lvl.name + "/" + text[0] + "/" + lvl.name + extension,
                                  "levels/" + lvl.name + extension, true);
                        Level temp = Level.Load(_s, lvl.name, 0, extension == ".lvl");
                        temp.physic.StartPhysics(lvl);
                        if (temp != null) {
                            lvl.setPhysics(0);
                            lvl.physic.ClearPhysics(lvl);

                            lvl.spawnx = temp.spawnx;
                            lvl.spawny = temp.spawny;
                            lvl.spawnz = temp.spawnz;

                            lvl.depth = temp.depth;
                            lvl.width = temp.width;
                            lvl.height = temp.height;

                            lvl.blocks = temp.blocks;

                            lvl.RestoreSQLData(@_s.props.backupLocation + "/" + lvl.name + "/" + text[0]);

                            _s.commands.FindCommand("reveal").Use(_s.ServerConsole, "all " + text[1]);
                        }
                        else {
                            _s.logger.Log("Restore nulled");
                            File.Copy("levels/" + lvl.name + extension + ".backup", "levels/" + lvl.name + extension, true);
                        }

                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        _s.logger.Log("Restore fail");
                    }
                }
                else {
                    p.SendMessage("Backup " + text[0] + " does not exist.");
                }
            }
            else {
                if (Directory.Exists(@_s.props.backupLocation + "/" + p.level.name)) {
                    string[] directories = Directory.GetDirectories(@_s.props.backupLocation + "/" + p.level.name);
                    int backupNumber = directories.Length;
                    p.SendMessage(p.level.name + " has " + backupNumber + " backups .");

                    bool foundOne = false;
                    string foundRestores = String.Empty;
                    foreach (string s in directories) {
                        string directoryName = s.Substring(s.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                        try {
                            int.Parse(directoryName);
                        }
                        catch {  // TODO: find exact exception to catch
                            foundOne = true;
                            foundRestores += ", " + directoryName;
                        }
                    }

                    if (foundOne) {
                        p.SendMessage("Custom-named restores:");
                        p.SendMessage("> " + foundRestores.Remove(0, 2));
                    }
                }
                else {
                    p.SendMessage(p.level.name + " has no backups yet.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /restore.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/restore - List all backups of the map you are on.");
            p.SendMessage("/restore <number?/backupname?> - Restores a previous backup of the current map.");
            p.SendMessage("/restore <number?/backupname?> <map?> - Restores a previous backup of the specified map.");
        }
    }
}

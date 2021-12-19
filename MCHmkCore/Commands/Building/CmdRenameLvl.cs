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
using System.Data.Common;
using System.IO;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdRenameLvl : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rename", "lvl", "level", "map"});

        public override string Name {
            get {
                return "renamelvl";
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
        public CmdRenameLvl(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }
            Level foundLevel = _s.levels.FindExact(args.Split(' ')[0]);
            if (foundLevel == null) {
                p.SendMessage("Level not found");
                return;
            }

            string newName = args.Split(' ')[1];

            if (File.Exists("levels/" + newName + ".mcf")) {
                p.SendMessage("Level already exists.");
                return;
            }
            if (foundLevel == _s.mainLevel) {
                p.SendMessage("Cannot rename the main level.");
                return;
            }

            foundLevel.Unload();

            try {
                try {
                    File.Move("levels/" + foundLevel.name + ".mcf", "levels/" + newName + ".mcf");
                    File.Move("levels/" + foundLevel.name + ".mcf.backup", "levels/" + newName + ".mcf.backup");
                }
                catch (Exception e) { // TODO: figure out exception to catch
                    _s.logger.ErrorLog(e);
                }
                   
                try {
                    File.Move("levels/level properties/" + foundLevel.name + ".properties",
                              "levels/level properties/" + newName + ".properties");
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }
                try {
                    File.Move("levels/level properties/" + foundLevel.name, "levels/level properties/" + newName + ".properties");
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }

                //Move and rename backups
                try {
                    string foundLevelDir, newNameDir;
                    for (int i = 1; ; i++) {
                        foundLevelDir = @_s.props.backupLocation + "/" + foundLevel.name + "/" + i + "/";
                        newNameDir = @_s.props.backupLocation + "/" + newName + "/" + i + "/";

                        if (File.Exists(foundLevelDir + foundLevel.name + ".mcf")) {
                            Directory.CreateDirectory(newNameDir);
                            File.Move(foundLevelDir + foundLevel.name + ".mcf", newNameDir + newName + ".mcf");
                            if (DirectoryEmpty(foundLevelDir)) {
                                Directory.Delete(foundLevelDir);
                            }
                        }
                        else {
                            if (DirectoryEmpty(@_s.props.backupLocation + "/" + foundLevel.name + "/")) {
                                Directory.Delete(@_s.props.backupLocation + "/" + foundLevel.name + "/");
                            }
                            break;
                        }
                    }
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }

                if (_s.props.useMySQL)
                    _s.database.ExecuteStatement(String.Format("RENAME TABLE `Block{0}` TO `Block{1}`, " +
                                                        "`Portals{0}` TO `Portals{1}`, " +
                                                        "`Messages{0}` TO `Messages{1}`, " +
                                                        "`Zone{0}` TO `Zone{1}`", foundLevel.name.ToLower(), newName.ToLower()));
                else {
                    using (DbConnection conn = _s.database.OpenConnection()) {
                        using (TransactionHelper trans = new TransactionHelper(conn)) {
                            _s.database.ExecuteStatement(conn, String.Format("ALTER TABLE Block{0} RENAME TO Block{1}", foundLevel.name.ToLower(), newName.ToLower()));
                            _s.database.ExecuteStatement(conn, String.Format("ALTER TABLE Portals{0} RENAME TO Portals{1}", foundLevel.name.ToLower(), newName.ToLower()));
                            _s.database.ExecuteStatement(conn, String.Format("ALTER TABLE Messages{0} RENAME TO Messages{1}", foundLevel.name.ToLower(), newName.ToLower()));
                            _s.database.ExecuteStatement(conn, String.Format("ALTER TABLE Zone{0} RENAME TO Zone{1}", foundLevel.name.ToLower(), newName.ToLower()));

                            trans.CommitOrRollback();
                        }
                        conn.Close();
                    }
                }
                try {
                    _s.commands.FindCommand("load").Use(p, newName);
                }
                catch (Exception e) { 
                    _s.logger.ErrorLog(e);
                }
                _s.GlobalMessage("Renamed " + foundLevel.name + " to " + newName);
            }
            catch (Exception e) {
                p.SendMessage("Error when renaming.");
                _s.logger.ErrorLog(e);
            }
        }

        public static bool DirectoryEmpty(string dir) {
            if (!Directory.Exists(dir)) {
                return true;
            }
            if (Directory.GetDirectories(dir).Length > 0) {
                return false;
            }
            if (Directory.GetFiles(dir).Length > 0) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when /help is used on /renamelvl.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/renamelvl <map?> <newname?> - Renames a map.");
            p.SendMessage("Portals going to the specified map will not work.");
        }
    }
}

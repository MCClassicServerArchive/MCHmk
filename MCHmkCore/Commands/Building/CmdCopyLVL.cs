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
    public class CmdCopyLVL : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"copy", "lvl", "level", "map"});

        public override string Name {
            get {
                return "copylvl";
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
        public CmdCopyLVL(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string currentName = String.Empty;
            string copyName = String.Empty;
            try {
                currentName = args.Split(' ')[0];
                copyName = args.Split(' ')[1];
            }
            catch {  // TODO: Find exact exception to catch

            }

            if (!_s.commands.CanExecute(p, "newlvl")) {
                p.SendMessage("You cannot use this command, unless you can use /newlvl!");
                return;
            }
            int number = args.Split(' ').Length;
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (number < 2) {
                p.SendMessage("You did not specify the level it would be copied to as!");
                return;
            }
            try {
                string mcfPath = Path.Combine("levels", currentName + ".mcf");
                string lvlPath = Path.Combine("levels", currentName + ".lvl");
                string propsPath = Path.Combine("levels", "level properties", currentName + ".properties");

                if (File.Exists(mcfPath)) {
                    File.Copy(mcfPath, Path.Combine("levels", copyName + ".mcf"));
                }
                else if (File.Exists(lvlPath)) {
                    File.Copy(lvlPath, Path.Combine("levels", copyName + ".lvl"));
                }
                else {
                    p.SendMessage("The level '" + currentName + "' does not exist.");
                    return;
                }

                if (File.Exists(propsPath)) {
                    File.Copy(propsPath, Path.Combine("levels", "level properties", copyName + ".properties"));
                }
                p.SendMessage("The level '" + currentName + "' has been copied to '" + copyName + "'.");

            }
            catch (FileNotFoundException) {
                p.SendMessage("The level '" + currentName + "' does not exist.");
            }
            catch (IOException) {
                p.SendMessage("The level '" + copyName + "' already exists.");
            }
            catch (ArgumentException) {
                p.SendMessage("One or both level names are either invalid or corrupt.");
            }
        }

        /// <summary>
        /// Called when /help is used on /copylvl.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/copylvl <map?> <newname?> - Makes a copy of a map that is " +
                               "then named with the specified name.");
        }
    }
}

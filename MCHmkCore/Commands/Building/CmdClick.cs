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

namespace MCHmk.Commands {
    public class CmdClick : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "use"});

        public override string Name {
            get {
                return "click";
            }
        }
        public override string Shortcut {
            get {
                return "x";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdClick(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            string[] parameters = args.Split(' ');
            ushort[] click = p.lastClick;

            if (args.IndexOf(' ') != -1) {
                if (parameters.Length != 3) {
                    Help(p);
                    return;
                }
                else {
                    for (int value = 0; value < 3; value++) {
                        if (parameters[value].ToLower() == "x" || parameters[value].ToLower() == "y" || parameters[value].ToLower() == "z") {
                            click[value] = p.lastClick[value];
                        }
                        else if (isValid(parameters[value], value, p)) {
                            click[value] = ushort.Parse(parameters[value]);
                        }
                        else {
                            p.SendMessage("\"" + parameters[value] + "\" was not valid");
                            return;
                        }
                    }
                }
            }

            p.lastCMD = "click";
            p.manualChange(click[0], click[1], click[2], 0, BlockId.Stone);
            p.SendMessage("Clicked &b(" + click[0] + ", " + click[1] + ", " + click[2] + ")");
        }

        private bool isValid(string message, int dimension, Player p) {
            ushort testValue;
            try {
                testValue = ushort.Parse(message);
            }
            catch {  // TODO: Find exact exception to catch
                return false;
            }

            if (testValue < 0) {
                return false;
            }

            if (testValue >= p.level.width && dimension == 0) {
                return false;
            }
            else if (testValue >= p.level.height && dimension == 1) {
                return false;
            }
            else if (testValue >= p.level.depth && dimension == 2) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called when /help is used on /click.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/click <x?> <y?> <z?> - Simulates a click at the given " +
                               "coordinates. If x, y, or z are used for the coordinates, it uses the last place " +
                               "that you clicked on that particular axis.");
            p.SendMessage("Example: /click 200 y 200 will cause it to click at " +
                               "(200, the last y position you clicked, 200).");
        }
    }
}

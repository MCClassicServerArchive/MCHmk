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

namespace MCHmk.Commands {
    public class CmdPause : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"physics", "reset"});

        public override string Name {
            get {
                return "pause";
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
        public CmdPause(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                if (!p.IsConsole) {
                    args = p.level.name + " 30";
                }
                else {
                    args = _s.mainLevel + " 30";
                }
            }
            int foundNum = 0;
            Level foundLevel;

            if (args.IndexOf(' ') == -1) {
                try {
                    foundNum = int.Parse(args);
                    if (!p.IsConsole) {
                        foundLevel = p.level;
                    }
                    else {
                        foundLevel = _s.mainLevel;
                    }
                }
                catch {  // TODO: find exact exception to catch
                    foundNum = 30;
                    foundLevel = _s.levels.FindExact(args);
                }
            }
            else {
                try {
                    foundNum = int.Parse(args.Split(' ')[1]);
                    foundLevel = _s.levels.FindExact(args.Split(' ')[0]);
                }
                catch {  // TODO: find exact exception to check
                    p.SendMessage("Invalid input");
                    return;
                }
            }

            if (foundLevel == null) {
                p.SendMessage("Could not find entered level.");
                return;
            }

            try {
                if (foundLevel.physic.physPause) {
                    foundLevel.PhysicsEnabled = true;
                    foundLevel.physic.physResume = DateTime.Now;
                    foundLevel.physic.physPause = false;
                    _s.GlobalMessage("Physics on " + foundLevel.name + " were re-enabled.");
                }
                else {
                    foundLevel.PhysicsEnabled  = false;
                    foundLevel.physic.physResume = DateTime.Now.AddSeconds(foundNum);
                    foundLevel.physic.physPause = true;
                    _s.GlobalMessage("Physics on " + foundLevel.name + " were temporarily disabled.");

                    foundLevel.physic.physTimer.Elapsed += delegate {
                        if (DateTime.Now > foundLevel.physic.physResume) {
                            foundLevel.physic.physPause = false;
                            try {
                                foundLevel.PhysicsEnabled = true;
                            }
                            catch (Exception e) {
                                _s.logger.ErrorLog(e);
                            }
                            _s.GlobalMessage("Physics on " + foundLevel.name + " were re-enabled.");
                            foundLevel.physic.physTimer.Stop();
                            foundLevel.physic.physTimer.Dispose();
                        }
                    };
                    foundLevel.physic.physTimer.Start();
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /pause.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/pause <map?> [seconds?] - Pauses physics on a map for the " +
                               "given amount of seconds.");
            p.SendMessage("If the number of seconds is not specified, the physics are " +
                               "paused for 30 seconds.");
        }
    }
}

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
    public class CmdPermissionBuild : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"perm", "build", "rank"});

        public override string Name {
            get {
                return "perbuild";
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
        public CmdPermissionBuild(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!p.IsConsole) {
                if (args == String.Empty) {
                    Help(p);
                    return;
                }
                int number = args.Split(' ').Length;
                if (number > 2 || number < 1) {
                    Help(p);
                    return;
                }
                if (number == 1) {
                    int Perm = _s.ranks.PermFromName(args);
                    if (Perm == DefaultRankValue.Null) {
                        p.SendMessage("Not a valid rank");
                        return;
                    }
                    if (p.level.permissionbuild > p.rank.Permission) {
                        p.SendMessage("You cannot change the perbuild of a level with a perbuild higher than your rank.");
                        return;
                    }
                    else if (Perm > p.rank.Permission) {
                        p.SendMessage("You cannot set the build permission higher than your rank.");
                        return;
                    }
                    p.level.permissionbuild = Perm;
                    p.level.SaveSettings();
                    _s.logger.Log(p.level.name + " build permission changed to " + args + ".");
                    _s.GlobalMessageLevel(p.level, "build permission changed to " + args + ".");
                }
                else {
                    int pos = args.IndexOf(' ');
                    string t = args.Substring(0, pos).ToLower();
                    string s = args.Substring(pos + 1).ToLower();
                    int Perm = _s.ranks.PermFromName(s);
                    if (Perm == DefaultRankValue.Null) {
                        p.SendMessage("Not a valid rank");
                        return;
                    }

                    Level level = _s.levels.FindExact(t);
                    if (level != null) {
                        if (level.permissionbuild > p.rank.Permission) {
                            p.SendMessage("You cannot change the perbuild of a level with a perbuild higher than your rank.");
                            return;
                        }

                        level.permissionbuild = Perm;
                        level.SaveSettings();
                        _s.logger.Log(level.name + " build permission changed to " + s + ".");
                        _s.GlobalMessageLevel(level, "build permission changed to " + s + ".");
                        if (!p.IsConsole)
                            if (p.level != level) {
                                p.SendMessage("build permission changed to " + s + " on " + level.name + ".");
                            }
                        return;
                    }
                    else {
                        p.SendMessage("There is no level \"" + s + "\" loaded.");
                    }
                }
            }
            else {
                string[] splitArgs = args.Split(' ');

                int Perm = _s.ranks.PermFromName(splitArgs[1]);
                if (Perm == DefaultRankValue.Null) {
                    p.SendMessage("Not a valid rank");
                    return;
                }

                Level level = _s.levels.FindExact(splitArgs[0]);
                if (level != null) {
                    level.permissionbuild = Perm;
                    level.SaveSettings();
                    _s.logger.Log(level.name + " build permission changed to " + splitArgs[1] + ".");
                    _s.GlobalMessageLevel(level, "build permission changed to " + splitArgs[1] + ".");
                    if (!p.IsConsole)
                        if (p.level != level) {
                            p.SendMessage("build permission changed to " + splitArgs[1] + " on " + level.name + ".");
                        }
                    return;
                }
                else {
                    _s.logger.Log("There is no level \"" + splitArgs[1] + "\" loaded.");
                }
            }

        }

        /// <summary>
        /// Called when /help is used on /perbuild.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/perbuild [map?] <rank?> - Sets the minimum rank needed" +
                               "to build on the specified map, or the current map if no map name is given.");
        }
    }
}

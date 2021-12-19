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
    public class CmdPermissionVisit : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"perm", "visit", "rank"});

        public override string Name {
            get {
                return "pervisit";
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
        public CmdPermissionVisit(Server s) : base(s) { }

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
                    if (p.level.permissionvisit > p.rank.Permission) {
                        p.SendMessage("You cannot change the pervisit of a level with a pervisit higher than your rank.");
                        return;
                    }
                    else if (Perm > p.rank.Permission) {
                        p.SendMessage("You cannot set the visit permission higher than your own rank.");
                        return;
                    }
                    p.level.permissionvisit = Perm;
                    p.level.SaveSettings();
                    _s.logger.Log(p.level.name + " visit permission changed to " + args + ".");
                    _s.GlobalMessageLevel(p.level, "visit permission changed to " + args + ".");
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
                        if (level.permissionvisit > p.rank.Permission) {
                            p.SendMessage("You cannot change the pervisit of a level with a pervisit higher than your rank.");
                            return;
                        }

                        level.permissionvisit = Perm;
                        level.SaveSettings();
                        _s.logger.Log(level.name + " visit permission changed to " + s + ".");
                        _s.GlobalMessageLevel(level, "visit permission changed to " + s + ".");
                        if (!p.IsConsole) {
                            if (p.level != level) {
                                p.SendMessage("visit permission changed to " + s + " on " + level.name + ".");
                            }
                        }
                        return;
                    }
                    else {
                        p.SendMessage("There is no level \"" + t + "\" loaded.");
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
                    level.permissionvisit = Perm;
                    level.SaveSettings();
                    _s.logger.Log(level.name + " visit permission changed to " + splitArgs[1] + ".");
                    _s.GlobalMessageLevel(level, "visit permission changed to " + splitArgs[1] + ".");
                    return;
                }
                else {
                    _s.logger.Log("There is no level \"" + splitArgs[0] + "\" loaded.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /pervisit.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/pervisit [map?] <rank?> - Sets the minimum rank needed " +
                               "to visit the specified map, or the current map if no map name is given.");
        }
    }
}

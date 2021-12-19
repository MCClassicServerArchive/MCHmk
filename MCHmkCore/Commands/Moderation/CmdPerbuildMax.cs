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

namespace MCHmk.Commands {
    public class CmdPerbuildMax : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"perm", "build", "max", "rank"});

        public override string Name {
            get {
                return "perbuildmax";
            }
        }
        public override string Shortcut {
            get {
                return "pbm";
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
        public CmdPerbuildMax(Server s) : base(s) { }

        public override void Use(Player p, string args) {
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
                if (p.level.perbuildmax > p.rank.Permission) {
                    if (p.level.perbuildmax != DefaultRankValue.Nobody) {
                        p.SendMessage("You cannot change the perbuildmax of a level with a perbuildmax higher than your rank.");
                        return;
                    }
                }
                p.level.perbuildmax = Perm;
                p.level.SaveSettings();
                _s.logger.Log(p.level.name + " buildmax permission changed to " + args + ".");
                _s.GlobalMessageLevel(p.level, "buildmax permission changed to " + args + ".");
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
                if (level.perbuildmax > p.rank.Permission) {
                    if (level.perbuildmax != DefaultRankValue.Nobody) {
                        p.SendMessage("You cannot change the perbuildmax of a level with a perbuildmax higher than your rank.");
                        return;
                    }
                }
                if (level != null) {
                    level.perbuildmax = Perm;
                    level.SaveSettings();
                    _s.logger.Log(level.name + " buildmax permission changed to " + s + ".");
                    _s.GlobalMessageLevel(level, "buildmax permission changed to " + s + ".");
                    if (!p.IsConsole)
                        if (p.level != level) {
                            p.SendMessage("buildmax permission changed to " + s + " on " + level.name + ".");
                        }
                    return;
                }
                else {
                    p.SendMessage("There is no level \"" + s + "\" loaded.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /perbuildmax.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/perbuildmax [map?] <rank?> - Sets the highest rank that is " +
                               "able to visit the specified map, or the current map if no map name is given.");
        }
    }
}

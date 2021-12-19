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
    public class CmdMoveAll : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"move", "all", "player", "pos"});

        public override string Name {
            get {
                return "moveall";
            }
        }
        public override string Shortcut {
            get {
                return "ma";
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

        public CmdMoveAll(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            Level level = _s.levels.FindExact(args.Split(' ')[0]);
            if (level == null) {
                p.SendMessage("There is no level named '" + args.Split(' ')[0] + "'.");
                return;
            }
            if (p.IsConsole)
                foreach (Player pl in _s.players) {
                    _s.commands.FindCommand("move").Use(_s.ServerConsole, pl.name + " " + level.name);
                }
            else {
                foreach (Player pl in _s.players) {
                    if (pl.rank.Permission < p.rank.Permission) {
                        _s.commands.FindCommand("move").Use(p, pl.name + " " + level.name);
                    }
                    else {
                        p.SendMessage("You cannot move " + pl.color + pl.name + _s.props.DefaultColor +
                            " because they are of equal or higher rank.");
                    }
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /moveall.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/moveall <map?> - Moves all players to the specified map.");
        }
    }
}

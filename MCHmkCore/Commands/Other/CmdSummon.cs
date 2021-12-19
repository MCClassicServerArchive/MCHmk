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
using System.Threading;

namespace MCHmk.Commands {
    public class CmdSummon : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"move", "teleport", "tp", "player", "user"});

        public override string Name {
            get {
                return "summon";
            }
        }
        public override string Shortcut {
            get {
                return "s";
            }
        }
        public override string Type {
            get {
                return "other";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdSummon(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("You cannot use this command from the console");
                return;
            }
            if (args.ToLower() == "all") {
                try {
                    foreach (Player pl in _s.players) {
                        if (pl.level == p.level && pl != p && p.rank.Permission > pl.rank.Permission) {
                            unchecked {
                                pl.SendPos((byte)-1, p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0);
                            }
                            pl.SendMessage("You were summoned by " + p.color + p.name + _s.props.DefaultColor + ".");
                        }
                    }
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }
                _s.GlobalMessage(p.color + p.name + _s.props.DefaultColor + " summoned " +
                                     "everyone in &b" + p.level.name + _s.props.DefaultColor + "!");
                return;
            }

            Player who = _s.players.Find(args);
            if (who == null || who.hidden) {
                p.SendMessage("There is no player \"" + args + "\"!");
                return;
            }
            if (p.rank.Permission < who.rank.Permission) {
                p.SendMessage("You cannot summon someone ranked higher than you!");
                return;
            }
            if (p.level != who.level) {
                p.SendMessage(who.name + " is in a different Level. Forcefetching has started!");
                Level where = p.level;
                _s.commands.FindCommand("goto").Use(who, where.name);
                Thread.Sleep(1000);
                // Sleep for a bit while they load
                while (who.Loading) {
                    Thread.Sleep(250);
                }
            }

            unchecked {
                who.SendPos((byte)-1, p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0);
            }
            who.SendMessage("You were summoned by " + p.color + p.name + _s.props.DefaultColor + ".");
        }

        /// <summary>
        /// Called when /help is used on /summon.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/summon <player?> - Summons a player to your current position.");
            p.SendMessage("/summon all - Summons all players in the current map.");
        }
    }
}

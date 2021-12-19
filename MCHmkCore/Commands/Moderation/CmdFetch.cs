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

	Author: SebbiUltimate

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
    /// <summary>
    /// This is the command /fetch
    /// use /help fetch in-game for more info
    /// </summary>
    public class CmdFetch : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"summon", "move"});

        public override string Name {
            get {
                return "fetch";
            }
        }
        public override string Shortcut {
            get {
                return "fb";
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

        public CmdFetch(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("Console cannot use this command. Try using /move instead.");
                return;
            }

            Player who = _s.players.Find(args);
            if (who == null || who.hidden) {
                p.SendMessage("Could not find player.");
                return;
            }

            if (p.rank.Permission <= who.rank.Permission) {
                p.SendMessage("You cannot fetch a player of equal or greater rank!");
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
        }

        /// <summary>
        /// Called when /help is used on /fetch.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Er...doesn't /summon do this already? -Jjp137
            p.SendMessage("/fetch <player?> - Fetches a player to your current position.");
        }
    }
}

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

	Written by jordanneil23

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
    public class CmdXspawn : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"extra", "spawn"});

        public override string Name {
            get {
                return "xspawn";
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

        public CmdXspawn(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            Player player = _s.players.Find(args.Split(' ')[0]);
            if (player == null) {
                p.SendMessage("Error: " + player.color + player.name + _s.props.DefaultColor + " was not found");
                return;
            }
            if (player == p) {
                p.SendMessage("Error: Seriously? Just use /spawn!");
                return;
            }
            if (player.rank.Permission > p.rank.Permission) {
                p.SendMessage("Cannot move someone of greater rank");
                return;
            }
            _s.commands.FindCommand("spawn").Use(player, String.Empty);
            p.SendMessage("Succesfully spawned " + player.color + player.name + _s.props.DefaultColor + ".");
            _s.GlobalMessage(player.color + player.name + _s.props.DefaultColor + " was respawned by " + p.color + p.name +
                                 _s.props.DefaultColor + ".");
        }

        /// <summary>
        /// Called when /help is used on /xspawn.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/xspawn - Respawns another player.");
            p.SendMessage("Other players are notified.");
        }
    }
}

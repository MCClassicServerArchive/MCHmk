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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MCHmk.Commands {
    public class CmdPatrol : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"teleport", "random"});
        public override string Name {
            get {
                return "patrol";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdPatrol(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("Are you stupid? =S You can't use this in the console!");
                return;
            }
            List<string> getpatrol = (from pl in _s.players where pl.rank.Permission <= _s.commands.GetOtherPerm(
                                          this) select pl.name).ToList();
            if (getpatrol.Count <= 0) {
                p.SendMessage("There must be at least one guest online to use this command!");
                return;
            }
            Random random = new Random();
            int index = random.Next(getpatrol.Count);
            string value = getpatrol[index];
            Player who = _s.players.Find(value);
            _s.commands.FindCommand("tp").Use(p, who.name);
            p.SendMessage("You are now visiting " + who.color + who.name + "!");
        }
        
        /// <summary>
        /// Called when /help is used on /patrol.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/patrol - Teleports you to a random "
                               + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name + " or lower.");
        }
    }
}

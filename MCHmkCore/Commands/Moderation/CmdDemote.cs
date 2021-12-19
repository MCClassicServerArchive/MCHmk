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
    public class CmdDemote : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "lower"});

        public override string Name {
            get {
                return "demote";
            }
        }
        public override string Shortcut {
            get {
                return "de";
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
        public CmdDemote(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') != -1) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(args);
            string foundName;
            Rank foundGroup;
            if (who == null) {
                foundName = args;
                Uuid uuid = Uuid.FindWithFallback(_s.database, foundName);
                foundGroup = _s.ranks.FindPlayerRank(uuid);
            }
            else {
                foundName = who.name;
                foundGroup = who.rank;
            }

            Rank nextGroup = null;
            bool nextOne = false;
            for (int i = _s.ranks.Count - 1; i >= 0; i--) {
                Rank grp = _s.ranks[i];
                if (nextOne) {
                    if (grp.Permission <= DefaultRankValue.Banned) {
                        break;
                    }
                    nextGroup = grp;
                    break;
                }
                if (grp == foundGroup) {
                    nextOne = true;
                }
            }

            if (nextGroup != null) {
                _s.commands.FindCommand("setrank").Use(p, foundName + " " + nextGroup.name + " " + _s.props.customDemoteMessage);
            }
            else {
                p.SendMessage("The player is already at the lowest rank.");
            }
        }

        /// <summary>
        /// Called when /help is used on /demote.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/demote <name?> - Demotes a player by one rank.");
        }
    }
}

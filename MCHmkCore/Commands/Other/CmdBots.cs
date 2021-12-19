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
    public class CmdBots : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"bot", "list", "ai"});

        public override string Name {
            get {
                return "bots";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdBots(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            args = String.Empty;
            foreach (PlayerBot Pb in PlayerBot.playerbots) {
                if (Pb.AIName != String.Empty) {
                    args += ", " + Pb.name + "(" + Pb.level.name + ")[" + Pb.AIName + "]";
                }
                else if (Pb.hunt) {
                    args += ", " + Pb.name + "(" + Pb.level.name + ")[Hunt]";
                }
                else {
                    args += ", " + Pb.name + "(" + Pb.level.name + ")";
                }

                if (Pb.kill) {
                    args += "-kill";
                }
            }

            if (args != String.Empty) {
                p.SendMessage("&1Bots: " + _s.props.DefaultColor + args.Remove(0, 2));
            }
            else {
                p.SendMessage("No bots are alive.");
            }
        }

        /// <summary>
        /// Called when /help is used on /bots.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/bots - Shows a list of bots, their AIs and levels.");
        }
    }
}

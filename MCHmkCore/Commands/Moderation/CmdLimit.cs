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
    public class CmdLimit : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"lim", "moderate", "type"});

        public override string Name {
            get {
                return "limit";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }
        public CmdLimit(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length != 2) {
                Help(p);
                return;
            }
            int newLimit;
            try {
                newLimit = int.Parse(args.Split(' ')[1]);
            }
            catch {  // TODO: find exact exception to catch
                p.SendMessage("Invalid limit amount");
                return;
            }
            if (newLimit < 1) {
                p.SendMessage("Cannot set below 1.");
                return;
            }

            Rank foundGroup = _s.ranks.Find(args.Split(' ')[0]);
            if (foundGroup != null) {
                foundGroup.maxBlocks = newLimit;
                _s.GlobalMessage(foundGroup.color + foundGroup.name + _s.props.DefaultColor + "'s building limits were set to &b" +
                                     newLimit);
                _s.ranks.SaveRanks();
            }
            else {
                switch (args.Split(' ')[0].ToLower()) {
                case "rp":
                case "restartphysics":
                    _s.props.rpLimit = newLimit;
                    _s.GlobalMessage("Custom /rp's limit was changed to &b" + newLimit.ToString());
                    break;
                case "rpnorm":
                case "rpnormal":
                    _s.props.rpNormLimit = newLimit;
                    _s.GlobalMessage("Normal /rp's limit was changed to &b" + newLimit.ToString());
                    break;

                default:
                    p.SendMessage("No supported /limit");
                    break;
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /limit.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/limit <rank?/command?> <amount?> - Sets the block limit for a rank or a command.");
            p.SendMessage("Possible ranks/commands: " + _s.ranks.ConcatNames(_s.props.DefaultColor, true) +
                               ", RP, RPNormal");
        }
    }
}

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
    public class CmdBlockSet : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "mod", "block"});

        public override string Name {
            get {
                return "blockset";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdBlockSet(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }

            BlockId foundBlock = BlockData.Ushort(args.Split(' ')[0]);
            if (foundBlock == BlockId.Null) {
                p.SendMessage("Could not find block entered");
                return;
            }
            int newPerm = _s.ranks.PermFromName(args.Split(' ')[1]);
            if (newPerm == DefaultRankValue.Null) {
                p.SendMessage("Could not find rank specified");
                return;
            }
            if (!p.IsConsole && newPerm > p.rank.Permission) {
                p.SendMessage("Cannot set to a rank higher than yourself.");
                return;
            }

            if (!p.IsConsole && !_s.blockPerms.CanPlace(p, foundBlock)) {
                p.SendMessage("Cannot modify a block set for a higher rank");
                return;
            }

            _s.blockPerms.ChangeRank(foundBlock, newPerm);
            _s.blockPerms.SaveBlocks();

            _s.GlobalMessage("&d" + BlockData.Name(foundBlock) + _s.props.DefaultColor + "'s permission was changed to " +
                                 _s.ranks.PermToName(newPerm));
            if (p.IsConsole) {
                p.SendMessage(BlockData.Name(foundBlock) + "'s permission was changed to " + _s.ranks.PermToName(newPerm));
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /blockset.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/blockset <block?> <rank?> - Changes a block's minimum rank " +
                               "to the specified rank.");
            p.SendMessage("You may only use this command on blocks you can use.");
            p.SendMessage("Available ranks: " + _s.ranks.ConcatNames(_s.props.DefaultColor));
        }
    }
}

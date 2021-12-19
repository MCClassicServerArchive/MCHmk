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
	Copyright (C) 2010-2013 David Mitchell

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.
*/

using System;
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public sealed class CmdBind : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"alias", "block"});

        public override string Name {
            get {
                return "bind";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "build";
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
        public CmdBind(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            if (args.Split(' ').Length > 2) {
                Help(p);
                return;
            }
            args = args.ToLower();
            if (args == "clear") {
                for (byte d = 0; d < 128; d++) {
                    p.bindings[d] = (BlockId)d;
                }
                p.SendMessage("All bindings were unbound.");
                return;
            }

            int pos = args.IndexOf(' ');
            if (pos != -1) {
                BlockId b1 = BlockData.Ushort(args.Substring(0, pos));
                BlockId b2 = BlockData.Ushort(args.Substring(pos + 1));
                if (b1 == BlockId.Null) {
                    p.SendMessage("There is no block \"" + args.Substring(0, pos) + "\".");
                    return;
                }
                if (b2 == BlockId.Null) {
                    p.SendMessage("There is no block \"" + args.Substring(pos + 1) + "\".");
                    return;
                }

                if (!BlockData.Placable(b1)) {
                    p.SendMessage(BlockData.Name(b1) + " isn't a special block.");
                    return;
                }
                if (!_s.blockPerms.CanPlace(p, b2)) {
                    p.SendMessage("You can't bind " + BlockData.Name(b2) + ".");
                    return;
                }
                if (Convert.ToByte(b1) > (byte)64) {
                    p.SendMessage("Cannot bind anything to this block.");
                    return;
                }

                if (p.bindings[Convert.ToInt32(b1)] == b2) {
                    p.SendMessage(BlockData.Name(b1) + " is already bound to " + BlockData.Name(b2) + ".");
                    return;
                }

                p.bindings[Convert.ToInt32(b1)] = b2;
                args = BlockData.Name(b1) + " bound to " + BlockData.Name(b2) + ".";

                p.SendMessage(args);
            }
            else {
                BlockId b = BlockData.Ushort(args);
                if (Convert.ToInt32(b) > 100) {
                    p.SendMessage("This block cannot be bound");
                    return;
                }

                if (p.bindings[Convert.ToInt32(b)] == b) {
                    p.SendMessage(BlockData.Name(b) + " isn't bound.");
                    return;
                }
                p.bindings[Convert.ToInt32(b)] = b;
                p.SendMessage("Unbound " + BlockData.Name(b) + ".");
            }
        }

        /// <summary>
        /// Called when /help is used on /bind.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/bind <block1?> <block2?> - Uses a placeable block as an " +
                               "alias for another block, binding the two blocks together.");
            p.SendMessage("/bind <clear> - Clears all binds.");
            p.SendMessage("Example: /bind sponge water - Makes all sponges you place " +
                               "turn into water.");
        }
    }
}

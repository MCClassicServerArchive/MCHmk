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
    public sealed class CmdMode : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "place"});

        public override string Name {
            get {
                return "mode";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdMode(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                if (p.modeType != 0) {
                    p.SendMessage("&b" + BlockData.Name(p.modeType)[0].ToString().ToUpper() + BlockData.Name(p.modeType).Remove(0,
                                       1).ToLower() + _s.props.DefaultColor + " mode: &cOFF");
                    p.modeType = 0;
                    p.blockAction = 0;
                }
                else {
                    Help(p);
                    return;
                }
            }
            else {
                BlockId b = BlockData.Ushort(args);
                if (b == BlockId.Null) {
                    p.SendMessage("Could not find block given.");
                    return;
                }
                if (b == BlockId.Air) {
                    p.SendMessage("Cannot use Air Mode.");
                    return;
                }
                if (p.allowTnt == false) {
                    if (b == BlockId.Tnt) {
                        p.SendMessage("Tnt usage is not allowed at the moment");
                        return;
                    }
                }

                if (p.allowTnt == false) {
                    if (b == BlockId.BigTnt) {
                        p.SendMessage("Tnt usage is not allowed at the moment");
                        return;
                    }
                }

                if (p.allowTnt == false) {
                    if (b == BlockId.NukeTnt) {
                        p.SendMessage("Tnt usage is not allowed at the moment");
                        return;
                    }
                }

                if (p.allowTnt == false) {
                    if (b == BlockId.Embers) {
                        p.SendMessage("Tnt usage is not allowed at the moment, fire is a lighter for tnt and is also disabled");
                        return;
                    }
                }

                if (p.allowTnt == false) {
                    if (b == BlockId.TntExplosion) {
                        p.SendMessage("Tnt usage is not allowed at the moment");
                        return;
                    }
                }

                if (p.allowTnt == false) {
                    if (b == BlockId.SmallTnt) {
                        p.SendMessage("Tnt usage is not allowed at the moment");
                        return;
                    }
                }

                if (!_s.blockPerms.CanPlace(p, b)) {
                    p.SendMessage("Cannot place this block at your rank.");
                    return;
                }

                if (p.modeType == b) {
                    p.SendMessage("&b" + BlockData.Name(p.modeType)[0].ToString().ToUpper() + BlockData.Name(p.modeType).Remove(0,
                                       1).ToLower() + _s.props.DefaultColor + " mode: &cOFF");
                    p.modeType = 0;
                    p.blockAction = 0;
                }
                else {
                    p.blockAction = 6;
                    p.modeType = b;
                    p.SendMessage("&b" + BlockData.Name(p.modeType)[0].ToString().ToUpper() + BlockData.Name(p.modeType).Remove(0,
                                       1).ToLower() + _s.props.DefaultColor + " mode: &aON");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /mode.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/mode <block?> - Makes every block placed become the " +
                               "specified block. Use again to turn off.");
            p.SendMessage("/<block?> also works as a shortcut.");
        }
    }
}

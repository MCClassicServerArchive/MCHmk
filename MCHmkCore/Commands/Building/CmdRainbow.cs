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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public sealed class CmdRainbow : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rain", "bow", "dash"});  // Yes, really. -Jjp137

        public override string Name {
            get {
                return "rainbow";
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
        public CmdRainbow(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }

            const string prompt = "Place two blocks to determine the edges.";
            TwoBlockSelection.Start(p, null, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the cuboid to be drawn.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            List<Tuple<UShortCoords, BlockId>> buffer = new List<Tuple<UShortCoords, BlockId>>();
            BlockId newType = BlockId.Pink;

            int xdif = Math.Abs(x1 - x2);
            int ydif = Math.Abs(y1 - y2);
            int zdif = Math.Abs(z1 - z2);

            if (xdif >= ydif && xdif >= zdif) {
                for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); xx++) {
                    newType += 1;
                    if (newType > BlockId.Pink) {
                        newType = BlockId.Red;
                    }
                    for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); yy++) {
                        for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); zz++) {
                            if (p.level.GetTile(xx, yy, zz) != BlockId.Air) {
                                BufferAdd(buffer, xx, yy, zz, newType);
                            }
                        }
                    }
                }
            }
            else if (ydif > xdif && ydif > zdif) {
                for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); yy++) {
                    newType += 1;
                    if (newType > BlockId.Pink) {
                        newType = BlockId.Red;
                    }
                    for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); xx++) {
                        for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); zz++) {
                            if (p.level.GetTile(xx, yy, zz) != BlockId.Air) {
                                BufferAdd(buffer, xx, yy, zz, newType);
                            }
                        }
                    }
                }
            }
            else if (zdif > ydif && zdif > xdif) {
                for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); zz++) {
                    newType += 1;
                    if (newType > BlockId.Pink) {
                        newType = BlockId.Red;
                    }
                    for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); yy++) {
                        for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); xx++) {
                            if (p.level.GetTile(xx, yy, zz) != BlockId.Air) {
                                BufferAdd(buffer, xx, yy, zz, newType);
                            }
                        }
                    }
                }
            }

            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried to replace " + buffer.Count + " blocks.");
                p.SendMessage("You cannot replace more than " + p.rank.maxBlocks + ".");
                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished);
                return;
            }

            p.SendMessage(buffer.Count.ToString() + " blocks.");
            buffer.ForEach(delegate(Tuple<UShortCoords, BlockId> pos) {
                p.level.Blockchange(p, pos.Item1.X, pos.Item1.Y, pos.Item1.Z, pos.Item2);                  //update block for everyone
            });

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished);
        }

        void BufferAdd(List<Tuple<UShortCoords, BlockId>> list, ushort x, ushort y, ushort z, BlockId newType) {
            UShortCoords coords = new UShortCoords(x, y, z);
            list.Add(new Tuple<UShortCoords, BlockId>(coords, newType));
        }
        
        /// <summary>
        /// Called when /help is used on /rainbow.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/rainbow - Paints a rainbow on any blocks within an area.");
        }
    }
}

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
    public sealed class CmdFill : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used for the flood fill.
        /// </summary>
        private readonly string _blockKey = "fill_block";
        /// <summary>
        /// Name of the key used to store and retrieve the type of flood fill being performed.
        /// </summary>
        private readonly string _fillTypeKey = "fill_type";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"cuboid", "edit"});

        public override string Name {
            get {
                return "fill";
            }
        }
        public override string Shortcut {
            get {
                return "f";
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
        public CmdFill(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            BlockId blockType;
            FillType fillType;

            int number = args.Split(' ').Length;
            if (number > 2) {
                Help(p);
                return;
            }
            if (number == 2) {
                int pos = args.IndexOf(' ');
                string t = args.Substring(0, pos).ToLower();
                string s = args.Substring(pos + 1).ToLower();
                blockType = BlockData.Ushort(t);
                if (blockType == BlockId.Null) {
                    p.SendMessage("There is no block \"" + t + "\".");
                    return;
                }

                if (!_s.blockPerms.CanPlace(p, blockType)) {
                    p.SendMessage("Cannot place that.");
                    return;
                }

                if (s == "up") {
                    fillType = FillType.Up;
                }
                else if (s == "down") {
                    fillType = FillType.Down;
                }
                else if (s == "layer") {
                    fillType = FillType.Layer;
                }
                else if (s == "vertical_x") {
                    fillType = FillType.VerticalX;
                }
                else if (s == "vertical_z") {
                    fillType = FillType.VerticalZ;
                }
                else {
                    p.SendMessage("Invalid fill type");
                    return;
                }
            }
            else if (args != String.Empty) {
                args = args.ToLower();
                if (args == "up") {
                    fillType = FillType.Up;
                    blockType = BlockId.Null;
                }
                else if (args == "down") {
                    fillType = FillType.Down;
                    blockType = BlockId.Null;
                }
                else if (args == "layer") {
                    fillType = FillType.Layer;
                    blockType = BlockId.Null;
                }
                else if (args == "vertical_x") {
                    fillType = FillType.VerticalX;
                    blockType = BlockId.Null;
                }
                else if (args == "vertical_z") {
                    fillType = FillType.VerticalZ;
                    blockType = BlockId.Null;
                }
                else {
                    blockType = BlockData.Ushort(args);
                    if (blockType == BlockId.Null) {
                        p.SendMessage("Invalid block or fill type");    //Just use BlockId.Zero or byte.MaxValue??
                        return;
                    }
                    if (!_s.blockPerms.CanPlace(p, blockType)) {
                        p.SendMessage("Cannot place that.");
                        return;
                    }

                    fillType = FillType.Default;
                }
            }
            else {
                blockType = BlockId.Null;
                fillType = FillType.Default;
            }

            p.ClearSelection();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = blockType;
            data[_fillTypeKey] = fillType;

            p.SendMessage("Select the block you wish to fill from.");
            p.StartSelection(BlockSelected, data);
        }

        private void BlockSelected(Player p, CommandTempData c) {
            try {
                p.ClearSelection();

                ushort x = c.X;
                ushort y = c.Y;
                ushort z = c.Z;

                BlockId blockType = c.GetData<BlockId>(_blockKey);
                FillType fillType = c.GetData<FillType>(_fillTypeKey);

                if (blockType == BlockId.Null) {
                    blockType = p.bindings[Convert.ToInt32(c.BlockType)];
                }

                BlockId oldType = p.level.GetTile(x, y, z);
                p.SendBlockchange(x, y, z, oldType);

                if (blockType == oldType) {
                    p.SendMessage("Cannot fill with the same type.");
                    return;
                }
                if (!_s.blockPerms.CanPlace(p, oldType) && !BlockData.BuildIn(oldType)) {
                    p.SendMessage("Cannot fill with that.");
                    return;
                }

                BlockId[] mapBlocks = new BlockId[p.level.blocks.Length];
                List<UShortCoords> buffer = new List<UShortCoords>();
                p.level.blocks.CopyTo(mapBlocks, 0);

                fromWhere.Clear();
                deep = 0;
                FloodFill(p, x, y, z, blockType, oldType, fillType, ref mapBlocks, ref buffer);

                int totalFill = fromWhere.Count;
                for (int i = 0; i < totalFill; i++) {
                    totalFill = fromWhere.Count;
                    UShortCoords pos = fromWhere[i];
                    deep = 0;
                    FloodFill(p, pos.X, pos.Y, pos.Z, blockType, oldType, fillType, ref mapBlocks, ref buffer);
                    totalFill = fromWhere.Count;
                }
                fromWhere.Clear();

                if (buffer.Count > p.rank.maxBlocks) {
                    p.SendMessage("You tried to fill " + buffer.Count + " blocks.");
                    p.SendMessage("You cannot fill more than " + p.rank.maxBlocks + ".");

                    HandleStaticMode(p, c);
                    return;
                }

                if (p.level.bufferblocks && !p.level.Instant) {
                    foreach (UShortCoords pos in buffer) {
                        _s.blockQueue.Addblock(p, pos.X, pos.Y, pos.Z, blockType);
                    }
                }
                else {
                    foreach (UShortCoords pos in buffer) {
                        p.level.Blockchange(p, pos.X, pos.Y, pos.Z, blockType);
                    }
                }

                p.SendMessage("Filled " + buffer.Count + " blocks.");
                buffer.Clear();
                buffer = null;
                mapBlocks = null;

                HandleStaticMode(p, c);
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        private void HandleStaticMode(Player p, CommandTempData c) {
            if (!p.staticCommands) {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = c.GetData<BlockId>(_blockKey);
            data[_fillTypeKey] = c.GetData<FillType>(_fillTypeKey);

            p.StartSelection(BlockSelected, data);
        }

        int deep;
        List<UShortCoords> fromWhere = new List<UShortCoords>();
        public void FloodFill(Player p, ushort x, ushort y, ushort z, BlockId b, BlockId oldType, FillType fillType,
                              ref BlockId[] blocks, ref List<UShortCoords> buffer) {
            try {
                UShortCoords pos;
                pos.X = x;
                pos.Y = y;
                pos.Z = z;

                if (deep > 4000) {
                    fromWhere.Add(pos);
                    return;
                }

                blocks[x + p.level.width * z + p.level.width * p.level.depth * y] = b;
                buffer.Add(pos);

                //x
                if (fillType != FillType.VerticalX) {
                    if (GetTile((ushort)(x + 1), y, z, p.level, blocks) == oldType) {
                        deep++;
                        FloodFill(p, (ushort)(x + 1), y, z, b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }

                    if (x > 0)
                        if (GetTile((ushort)(x - 1), y, z, p.level, blocks) == oldType) {
                            deep++;
                            FloodFill(p, (ushort)(x - 1), y, z, b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }

                //z
                if (fillType != FillType.VerticalZ) {
                    if (GetTile(x, y, (ushort)(z + 1), p.level, blocks) == oldType) {
                        deep++;
                        FloodFill(p, x, y, (ushort)(z + 1), b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }

                    if (z > 0)
                        if (GetTile(x, y, (ushort)(z - 1), p.level, blocks) == oldType) {
                            deep++;
                            FloodFill(p, x, y, (ushort)(z - 1), b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }

                //y
                if (fillType == 0 || fillType == FillType.Up || fillType > FillType.Layer) {
                    if (GetTile(x, (ushort)(y + 1), z, p.level, blocks) == oldType) {
                        deep++;
                        FloodFill(p, x, (ushort)(y + 1), z, b, oldType, fillType, ref blocks, ref buffer);
                        deep--;
                    }
                }

                if (fillType == 0 || fillType == FillType.Down || fillType > FillType.Layer) {
                    if (y > 0)
                        if (GetTile(x, (ushort)(y - 1), z, p.level, blocks) == oldType) {
                            deep++;
                            FloodFill(p, x, (ushort)(y - 1), z, b, oldType, fillType, ref blocks, ref buffer);
                            deep--;
                        }
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        public BlockId GetTile(ushort x, ushort y, ushort z, Level l, BlockId[] blocks) {
            //Avoid internal overflow
            if (x < 0) {
                return BlockId.Null;
            }
            if (x >= l.width) {
                return BlockId.Null;
            }
            if (y < 0) {
                return BlockId.Null;
            }
            if (y >= l.height) {
                return BlockId.Null;
            }
            if (z < 0) {
                return BlockId.Null;
            }
            if (z >= l.depth) {
                return BlockId.Null;
            }
            try {
                return blocks[l.PosToInt(x, y, z)];
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                return BlockId.Null;
            }
        }

        public enum FillType : int {
            Default = 0,
            Up = 1,
            Down = 2,
            Layer = 3,
            VerticalX = 4,
            VerticalZ = 5
        }

        /// <summary>
        /// Called when /help is used on /fill.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/fill [block?] [type?] - Does a flood-fill.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
            p.SendMessage("Valid fill types: up, down, layer, vertical_x, and vertical_z");
        }
    }
}

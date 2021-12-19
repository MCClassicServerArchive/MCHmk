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

using MCHmk.Drawing;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /measure command, which measures the number of blocks within the given area.
    /// </summary>
    public sealed class CmdMeasure : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the shape of the cuboid being drawn.
        /// </summary>
        private readonly string _blockKey = "measure_block";

        /// <summary>
        /// The list of keywords that are associated with /measure.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"meas", "block", "length", "distance"});

        /// <summary>
        /// Gets the name of /measure.
        /// </summary>
        public override string Name {
            get {
                return "measure";
            }
        }

        /// <summary>
        /// Gets the shortcut for /measure.
        /// </summary>
        public override string Shortcut {
            get {
                return "ms";
            }
        }

        /// <summary>
        /// Gets the category that /measure belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /measure. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /measure can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /measure.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        /// <summary>
        /// Constructs an instance of the /measure command.
        /// </summary>
        /// <param name="s"> The server that this instance of /measure will belong to.
        /// <seealso cref="Server"/></param>
        public CmdMeasure(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /measure.
        /// </summary>
        /// <param name="p"> The player that used /measure. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // The console can't use this command.
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }

            // Don't allow more than one argument to be given.
            if (args.IndexOf(' ') != -1) {
                p.SendMessage("This command only accepts one option.");
                return;
            }

            // Attempt to parse the given block type, if specified.
            BlockId blockType = BlockId.Null;
            if (args.Length != 0) {
                blockType = BlockData.Ushort(args);

                if (blockType == BlockId.Null) {
                    p.SendMessage("There is no block named '" + blockType + "'.");
                }
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = blockType;

            // Begin the area selection.
            const string prompt = "Place two blocks to determine the area's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player finishes selecting the area to be measured.
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

            // Retrieve the type of block that should be counted. BlockId.Zero means that the user didn't
            // provide a block type.
            BlockId blockType = c.GetData<BlockId>(_blockKey);

            // If a block type to count wasn't specified, the calcuation is simple. Otherwise, checking
            // every block is required in order to make sure that only blocks of the given type are counted.
            int count = 0;
            if (blockType == BlockId.Null) {
                count = Measurement.SelectionCount(x1, y1, z1, x2, y2, z2);
            }
            else {
                // Figure out the start and end points of the area, which is a cuboid.
                ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
                ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
                ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

                for (ushort curX = startX; curX <= endX; curX++) {
                    for (ushort curY = startY; curY <= endY; curY++) {
                        for (ushort curZ = startZ; curZ <= endZ; curZ++) {
                            if (p.level.GetTile(curX, curY, curZ) == blockType) {
                                count++;
                            }
                        }
                    }
                }
            }

            // Tell the user how many blocks are within that area.
            string firstCoords = "(" + x1.ToString() + ", " + y1.ToString() + ", " + z1.ToString() + ")";
            string secondCoords = "(" + x2.ToString() + ", " + y2.ToString() + ", " + z2.ToString() + ")";
            if (blockType == BlockId.Null) {
                p.SendMessage(count + " blocks are between " + firstCoords + " and " + secondCoords + ".");
            }
            else {
                string blockName = BlockData.Name(blockType);
                p.SendMessage(count + " " + blockName + " blocks are between " + firstCoords + " and " + secondCoords + ".");
            }

            // Handle /static.
            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey);
        }
        
        /// <summary>
        /// Called when /help is used on /measure.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/measure [block?] - Measures the number of blocks in a cuboid.");
            p.SendMessage("If a block type is provided, only those blocks are counted.");
        }
    }
}

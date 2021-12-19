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
    Thanks to aaron1tasker

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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using MCHmk.Drawing;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /pyramid command, which draws a pyramid into the player's current level.
    /// </summary>
    public class CmdPyramid : Command {
        /// <summary>
        /// Name the key used to store and retrieve the block type used to draw the pyramid.
        /// </summary>
        private readonly string _blockKey = "pyramid_block";
        /// <summary>
        /// Name of the key used to store and retrieve the shape of the pyramid being drawn.
        /// </summary>
        private readonly string _shapeKey = "pyramid_shape";

        /// <summary>
        /// The list of keywords that are associated with /pyramid.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"egypt", "pyram", "piram", "sand"});

        /// <summary>
        /// Gets the name of /pyramid.
        /// </summary>
        public override string Name {
            get {
                return "pyramid";
            }
        }

        /// <summary>
        /// Gets the shortcut for /pyramid.
        /// </summary>
        public override string Shortcut {
            get {
                return "pd";
            }
        }

        /// <summary>
        /// Gets the category that /pyramid belongs to.
        /// </summary>
        public override string Type {
            get {
                return "build";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /pyramid. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /pyramid can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the default permission value for /pyramid.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Builder;
            }
        }

        /// <summary>
        /// Constructs an instance of the /pyramid command.
        /// </summary>
        /// <param name="s"> The server that this instance of /pyramid will belong to.
        /// <seealso cref="Server"/></param>
        public CmdPyramid(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /pyramid.
        /// </summary>
        /// <param name="p"> The player that used /pyramid. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // The console can't use this command.
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }

            // The player has to be able to build on the level they are on.
            if (!p.CanModifyCurrentLevel()) {
                p.SendMessage("Your rank prohibits you from modifying this map.");
                return;
            }

            // Obtain the provided arguments and make sure that there aren't too many.
            string[] splitArgs = args.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (splitArgs.Length > 2) {
                Help(p);
                return;
            }

            // If no arguments were given, assume that the pyramid is a solid one that will use the block
            // that the player is currently carrying.
            BlockId chosenBlock = BlockId.Null;
            PyramidType chosenShape = PyramidType.Solid;

            // If two arguments are provided, it is expected that the first argument is a block type and
            // the second argument is a pyramid type.
            if (splitArgs.Length == 2) {
                string blockName = splitArgs[0];
                string shapeName = splitArgs[1];

                chosenBlock = BlockData.Ushort(blockName);
                if (chosenBlock == BlockId.Null) {
                    p.SendMessage("There is no block named \"" + blockName + "\".");
                    return;
                }
                else if (!_s.blockPerms.CanPlace(p, chosenBlock)) {
                    p.SendMessage("You're not allowed to place that block type.");
                    return;
                }

                chosenShape = Pyramids.ParsePyramidType(shapeName);
                if (chosenShape == PyramidType.Null) {
                    p.SendMessage("\"" + shapeName + "\" is not one of the recognized pyramid types.");
                    return;
                }
            }
            // If there is one argument only, parse it as a pyramid type first, and if that fails, parse
            // it as a block type. If that still fails, it's an invalid argument.
            else if (splitArgs.Length == 1) {
                string option = splitArgs[0];

                PyramidType tempShape = Pyramids.ParsePyramidType(option);
                if (tempShape != PyramidType.Null) {
                    chosenShape = tempShape;
                }
                else {
                    BlockId tempBlock = BlockData.Ushort(option);

                    if (tempBlock == BlockId.Null) {
                        p.SendMessage("There is no block named \"" + option + "\".");
                        return;
                    }
                    else if (!_s.blockPerms.CanPlace(p, tempBlock)) {
                        p.SendMessage("You're not allowed to place that block type.");
                        return;
                    }
                    else {
                        chosenBlock = tempBlock;
                    }
                }
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = chosenBlock;
            data[_shapeKey] = chosenShape;

            const string prompt = "Place two blocks to determine the pyramid's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the pyramid to be drawn.
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

            // For pyramids, both corners must be on the same y coordinate.
            if (y1 != y2) {
                p.SendMessage("The two corners must be on the same elevation.");
                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                return;
            }

            // Obtain the type of block to be used, and if it was not provided, set it to the block that is
            // bound to whatever block the player was holding.
            BlockId block = c.GetData<BlockId>(_blockKey);
            if (block == BlockId.Null) {
                block = p.bindings[(int)c.BlockType];
            }

            // Obtain the shape of the pyramid.
            PyramidType shape = c.GetData<PyramidType>(_shapeKey);

            // Enforce the depth limit if the player is a guest, anti-tunneling is enabled, and /trust hasn't
            // been used on that player.
            if (_s.props.antiTunnel && p.rank.Permission == DefaultRankValue.Guest && !p.ignoreGrief) {
                ushort depthLimit = (ushort) (p.level.height / 2 - _s.props.maxDepth);
                if (y1 < depthLimit || y2 < depthLimit) {
                    p.SendMessage("You're not allowed to build this far down!");

                    TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                    return;
                }
            }

            // Calculate the number of blocks beforehand and stop if the player's limit is too low unless
            // the force-cuboid option is on.
            int count = Pyramids.CountBlocks(shape, x1, y1, z1, x2, y2, z2);
            if (!_s.props.forceCuboid && count > p.rank.maxBlocks) {
                p.SendMessage("The pyramid would contain " + count.ToString() + " blocks.");
                p.SendMessage("However, your command limit is " + p.rank.maxBlocks.ToString() + " blocks.");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                return;
            }

            // Prepare the pyramid.
            IEnumerable<UShortCoords> pyramid = Pyramids.GeneratePyramid(shape, p.level, block, x1, y1, z1, x2, y2, z2);

            // Iterate through each block within the generated cuboid and place it.
            int blocksPlaced = 0;
            foreach (UShortCoords pos in pyramid) {
                // If force-cuboid is on, build only until the limit has been reached.
                if (_s.props.forceCuboid && blocksPlaced >= p.rank.maxBlocks) {
                    p.SendMessage("You tried to create " + count.ToString() +
                                       " blocks, but your limit is " + p.rank.maxBlocks.ToString() + ".");
                    p.SendMessage("The pyramid was built up to your limit.");
                    break;
                }
                // If this happens, it's a bug, as the number of blocks should have been checked earlier.
                else if (blocksPlaced >= p.rank.maxBlocks) {
                    throw new InvalidOperationException("In /pyramid, command limit was going to be exceeded.");
                }

                // Use the block queue if it is enabled.
                if (p.level.bufferblocks && !p.level.Instant) {
                    _s.blockQueue.Addblock(p, pos.X, pos.Y, pos.Z, block);
                }
                else {
                    p.level.Blockchange(p, pos.X, pos.Y, pos.Z, block);
                }
                blocksPlaced++;
            }

            // Tell the user how many blocks were changed.
            p.SendMessage(blocksPlaced.ToString() + " blocks were changed.");

            // Handle /static.
            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
        }

        /// <summary>
        /// Called when /help is used on /pyramid.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/pyramid [block?] [solid/hollow/reverse] - Creates a pyramid " +
                               "of blocks. The second parameter, if given, controls the type of pyramid.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
        }
    }
}

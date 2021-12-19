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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using MCHmk.Drawing;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /cuboid command, which draws a cuboid into the player's current level.
    /// </summary>
    public class CmdCuboid : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used to draw the cuboid.
        /// </summary>
        private readonly string _blockKey = "cuboid_block";
        /// <summary>
        /// Name of the key used to store and retrieve the shape of the cuboid being drawn.
        /// </summary>
        private readonly string _shapeKey = "cuboid_shape";

        /// <summary>
        /// The list of keywords that are associated with /cuboid.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"box", "set", "area", "block"});

        /// <summary>
        /// Gets the name of /cuboid.
        /// </summary>
        public override string Name {
            get {
                return "cuboid";
            }
        }

        /// <summary>
        /// Gets the shortcut for /cuboid.
        /// </summary>
        public override string Shortcut {
            get {
                return "z";
            }
        }

        /// <summary>
        /// Gets the category that /cuboid belongs to.
        /// </summary>
        public override string Type {
            get {
                return "build";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /cuboid. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /cuboid can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the default permission value for /cuboid.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Builder;
            }
        }

        /// <summary>
        /// Constructs an instance of the /cuboid command.
        /// </summary>
        /// <param name="s"> The server that this instance of /cuboid will belong to.
        /// <seealso cref="Server"/></param>
        public CmdCuboid(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /cuboid.
        /// </summary>
        /// <param name="p"> The player that used /cuboid. <seealso cref="Player"/></param>
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

            // If no arguments were given, assume that the cuboid is a solid one that will use the block
            // that the player is currently carrying.
            BlockId chosenBlock = BlockId.Null;
            CuboidType chosenShape = CuboidType.Solid;

            // If two arguments are provided, it is expected that the first argument is a block type and
            // the second argument is a cuboid type.
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

                chosenShape = Cuboids.ParseCuboidType(shapeName);
                if (chosenShape == CuboidType.Null) {
                    p.SendMessage("\"" + shapeName + "\" is not one of the recognized cuboid types.");
                    return;
                }
            }
            // If there is one argument only, parse it as a cuboid type first, and if that fails, parse
            // it as a block type. If that still fails, it's an invalid argument.
            else if (splitArgs.Length == 1) {
                string option = splitArgs[0];

                CuboidType tempShape = Cuboids.ParseCuboidType(option);
                if (tempShape != CuboidType.Null) {
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

            // Prompt the user to place two blocks.
            const string prompt = "Place two blocks to determine the cuboid's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
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

            // Obtain the type of block to be used, and if it was not provided, set it to the block that is
            // bound to whatever block the player was holding.
            BlockId block = c.GetData<BlockId>(_blockKey);
            if (block == BlockId.Null) {
                block = p.bindings[(int)c.BlockType];
            }

            // Obtain the shape of the cuboid.
            CuboidType shape = c.GetData<CuboidType>(_shapeKey);

            // Enforce the depth limit if the player is a guest, anti-tunneling is enabled, and /trust hasn't
            // been used on that player.
            // TODO: make this a function
            if (_s.props.antiTunnel && p.rank.Permission == DefaultRankValue.Guest && !p.ignoreGrief) {
                ushort depthLimit = (ushort) (p.level.height / 2 - _s.props.maxDepth);
                if (y1 < depthLimit || y2 < depthLimit) {
                    p.SendMessage("You're not allowed to build this far down!");

                    TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                    return;
                }
            }

            // Calculate the number of blocks beforehand and stop if the player cannot cuboid that many unless
            // the force-cuboid option is on.
            int count = Cuboids.CountBlocks(shape, x1, y1, z1, x2, y2, z2);
            if (!_s.props.forceCuboid && count > p.rank.maxBlocks) {
                // For random cuboids, the worst case scenario is that every block is filled in, Of course,
                // that's basically impossible, but the area it covers can interfere with another player's
                // attempts to build in that area.
                if (shape == CuboidType.Random) {
                    p.SendMessage("The selected area includes " + count.ToString() + " blocks.");
                }
                else {
                    p.SendMessage("You tried to cuboid " + count.ToString() + " blocks.");
                }
                p.SendMessage("You cannot cuboid more than " + p.rank.maxBlocks.ToString() + " blocks.");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                return;
            }

            // Prepare the cuboid.
            IEnumerable<UShortCoords> cuboid = Cuboids.GenerateCuboid(shape, p.level, block, x1, y1, z1, x2, y2, z2);

            // Iterate through each block within the generated cuboid and place it.
            int blocksPlaced = 0;
            foreach (UShortCoords pos in cuboid) {
                // If force-cuboid is on, build only until the limit has been reached.
                if (_s.props.forceCuboid && blocksPlaced >= p.rank.maxBlocks) {
                    // The below message doesn't make sense for random cuboids, so just omit it.
                    if (shape != CuboidType.Random) {
                        p.SendMessage("You tried to cuboid " + count.ToString() +
                                           " blocks, but your limit is " + p.rank.maxBlocks.ToString() + ".");
                    }
                    p.SendMessage("The cuboid was built up to your limit.");
                    break;
                }
                // If this happens, it's a bug, as the number of blocks should have been checked earlier.
                else if (blocksPlaced >= p.rank.maxBlocks) {
                    throw new InvalidOperationException("In /cuboid, command limit was going to be exceeded.");
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
        /// Called when /help is used on /cuboid.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/cuboid [block?] [type?] - Creates a cuboid of blocks.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
            p.SendMessage("Cuboid types: solid, hollow, walls, holes, wire, random");
        }
    }
}

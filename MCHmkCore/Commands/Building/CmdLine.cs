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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using MCHmk.Drawing;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /line command, which draws a line of blocks into the player's current level.
    /// </summary>
    public class CmdLine : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used to draw the line.
        /// </summary>
        private readonly string _blockKey = "line_block";
        /// <summary>
        /// Name of the key used to store and retrieve the block limit of the line being drawn.
        /// </summary>
        private readonly string _limitKey = "line_limit";
        /// <summary>
        /// Name of the key used to store and retrieve any additional flags for the line being drawn.
        /// </summary>
        private readonly string _flagsKey = "line_flags";

        /// <summary>
        /// The list of keywords that are associated with /line.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"draw", "block", "paint"});

        /// <summary>
        /// Gets the name of /line.
        /// </summary>
        public override string Name {
            get {
                return "line";
            }
        }

        /// <summary>
        /// Gets the shortcut of /line.
        /// </summary>
        public override string Shortcut {
            get {
                return "l";
            }
        }

        /// <summary>
        /// Gets the category that /line belongs to.
        /// </summary>
        public override string Type {
            get {
                return "build";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /line. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /line can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the default permission value for /line.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Builder;
            }
        }

        /// <summary>
        /// Constructs an instance of the /line command.
        /// </summary>
        /// <param name="s"> The server that this instance of /line will belong to.
        /// <seealso cref="Server"/></param>
        public CmdLine(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /line.
        /// </summary>
        /// <param name="p"> The player that used /line. <seealso cref="Player"/></param>
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

            // These are the defaults if no options are provided.
            ushort maxLen = Lines.MaxLimit;
            LineFlags flags = LineFlags.None;
            BlockId chosenBlock = BlockId.Null;

            // Obtain the provided arguments and make sure that there aren't too many.
            string[] splitArgs = args.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (splitArgs.Length > 4) {
                Help(p);
                return;
            }

            // Parse the arguments in any order, checking for any duplicate arguments.
            for (int i = 0; i < splitArgs.Length; i++) {
                int givenNum = 0;

                // Interpret numbers as the length limit. If maxLen is still the highest value possible, then a lower
                // limit hasn't been set yet.
                if (int.TryParse(splitArgs[i], out givenNum)) {
                    // FIXME: minor bug: duplicates are allowed if the first value is 2048
                    if (maxLen == Lines.MaxLimit && givenNum > 0 && givenNum <= Lines.MaxLimit) {
                        maxLen = (ushort) givenNum;
                    }
                    else if (maxLen == Lines.MaxLimit && givenNum > Lines.MaxLimit) {
                        p.SendMessage("Given block length is too long. Setting to " + Lines.MaxLimit.ToString());
                        maxLen = Lines.MaxLimit;
                    }
                    else if (maxLen == Lines.MaxLimit && givenNum <= 0) {
                        p.SendMessage("Invalid block length provided.");
                        return;
                    }
                    else {
                        p.SendMessage("A number was provided more than once.");
                        return;
                    }
                }
                // If the argument isn't a number, check if it's the 'wall' or 'straight' options.
                else if (splitArgs[i] == "wall") {
                    if (flags.HasFlag(LineFlags.Wall)) {
                        p.SendMessage("'wall' was provided more than once.");
                        return;
                    }
                    flags |= LineFlags.Wall;
                }
                else if (splitArgs[i] == "straight") {
                    if (flags.HasFlag(LineFlags.Straight)) {
                        p.SendMessage("'straight' was provided more than once.");
                        return;
                    }
                    flags |= LineFlags.Straight;
                }
                // Otherwise, interpret the option as a block type.
                else {
                    BlockId givenBlock = BlockData.Ushort(splitArgs[i]);
                    if (givenBlock != BlockId.Null) {
                        if (chosenBlock == BlockId.Null) {
                            if (!_s.blockPerms.CanPlace(p, givenBlock)) {
                                p.SendMessage("You're not allowed to place this block type.");
                                return;
                            }
                            chosenBlock = givenBlock;
                        }
                        else {
                            p.SendMessage("A block type was provided more than once.");
                            return;
                        }
                    }
                    else {
                        if (chosenBlock == BlockId.Null) {
                            p.SendMessage("There is no block named \"" + splitArgs[i] + "\".");
                            return;
                        }
                        else {
                            p.SendMessage("An invalid parameter was provided.");
                            return;
                        }
                    }
                }
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = chosenBlock;
            data[_limitKey] = maxLen;
            data[_flagsKey] = flags;

            const string prompt = "Place two blocks to determine the line's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player selected both ends of the line.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both ends. The first end is stored in the CommandTempData's Dictionary,
            // while the second end is contained within the X, Y, and Z properties of the CommandTempData since
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

            // Get the length limit and any line-related options.
            ushort limit = c.GetData<ushort>(_limitKey);
            LineFlags flags = c.GetData<LineFlags>(_flagsKey);

            // Enforce the depth limit if the player is a guest, anti-tunneling is enabled, and /trust hasn't
            // been used on that player.
            // TODO: make this a function
            if (_s.props.antiTunnel && p.rank.Permission == DefaultRankValue.Guest && !p.ignoreGrief) {
                ushort depthLimit = (ushort) (p.level.height / 2 - _s.props.maxDepth);
                if (y1 < depthLimit || y2 < depthLimit) {
                    p.SendMessage("You're not allowed to build this far down!");

                    TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _limitKey, _flagsKey);
                    return;
                }
            }

            // Calculate the number of blocks beforehand and stop if the player's limit is too low unless
            // the force-cuboid option is on.
            int count = Lines.CountBlocks(flags, limit, x1, y1, z1, x2, y2, z2);

            if (!_s.props.forceCuboid && count > p.rank.maxBlocks) {
                p.SendMessage("You tried to modify " + count.ToString() + " blocks.");
                p.SendMessage("However, your command limit is " + p.rank.maxBlocks.ToString() + " blocks.");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _limitKey, _flagsKey);
                return;
            }

            // Prepare the line.
            IEnumerable<UShortCoords> line = Lines.GenerateLine(flags, limit, p.level, block, x1, y1, z1, x2, y2, z2);

            // Iterate through each block within the generated line and place it.
            int blocksPlaced = 0;
            foreach (UShortCoords pos in line) {
                // If force-cuboid is on, build only until the limit has been reached.
                if (_s.props.forceCuboid && blocksPlaced >= p.rank.maxBlocks) {
                    p.SendMessage("The line was built up to your limit.");
                    break;
                }
                // If this happens, it's a bug, as the number of blocks should have been checked earlier.
                else if (blocksPlaced >= p.rank.maxBlocks) {
                    throw new InvalidOperationException("In /line, command limit was going to be exceeded.");
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
            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _limitKey, _flagsKey);
        }

        /// <summary>
        /// Called when /help is used on /line.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/line [block?] [maxlen?] [wall] [straight] - Creates a line of blocks.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
            p.SendMessage("The maxlen option restricts the line's length.");
            p.SendMessage("The wall option makes the line a wall.");
            p.SendMessage("The straight option forces the line to be straight.");
            p.SendMessage("The options can be provided in any order.");
        }
    }
}

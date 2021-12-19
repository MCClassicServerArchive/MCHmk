/*
    Copyright 2016 Jjp137

    This file includes source code from MCForge-Redux.

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MCHmk {
    /// <summary>
    /// The BlockPermList class holds information about the permission levels of MCHmk's block types.
    /// It also manages the blocks.properties file.
    /// </summary>
    public sealed class BlockPermList : IEnumerable<BlockPerm> {
        private readonly string blockPropsPath = Path.Combine("properties", "block.properties");

        /// <summary>
        /// The list of permission values for every block.
        /// </summary>
        private List<BlockPerm> _perms;
        /// <summary>
        /// A reference to the server's list of ranks.
        /// </summary>
        private RankList _ranks;
        /// <summary>
        /// A reference to the logger that will log any errors.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Gets or sets the logger used to log errors related to the BlockPermList.
        /// </summary>
        public Logger Logger {
            get {
                return _logger;
            }
            set {
                _logger = value;
            }
        }

        /// <summary>
        /// Constructs a BlockPermList object.
        /// </summary>
        /// <param name="ranks"> The list of ranks to use. </param>
        public BlockPermList(RankList ranks) {
            _ranks = ranks;
        }

        /// <summary>
        /// Initializes every block, and sets every individual block's permission values.
        /// </summary>
        public void SetBlocks() {
            this._perms = new List<BlockPerm>();
            BlockPerm b = new BlockPerm();
            b.lowestRank = DefaultRankValue.Guest;

            // Initially, create a placeholder List of all the blocks in MCHmk. This is
            // done so that the permission values can be inserted using the proper index
            // later on.
            for (int i = 0; i < (BlockData.maxblocks+ 1); i++) {
                b = new BlockPerm();
                b.type = (BlockId)i;
                this._perms.Add(b);
            }

            // Make a temporary list to hold the blocks and their permission values.
            List<BlockPerm> storedList = new List<BlockPerm>();

            // For every entry in BlockList, use its type (as specified by its ushort variable)
            // to figure out what the default rank of that block is.
            foreach (BlockPerm bs in this._perms) {
                // Create a temporary Blocks object.
                b = new BlockPerm();
                b.type = bs.type;

                // You have got to be kidding me. -Jjp137
                // Anyway, set the temporary object's minimum rank based on the below
                // switch-case statement, which holds the default rank values.
                switch (bs.type) {
                    case BlockId.Null:
                        b.lowestRank = DefaultRankValue.Admin;
                        break;

                    case BlockId.OpGlass:
                    case BlockId.Opsidian:
                    case BlockId.OpBrick:
                    case BlockId.OpStone:
                    case BlockId.OpCobblestone:
                    case BlockId.OpAir:
                    case BlockId.OpWater:
                    case BlockId.OpLava:
                    case BlockId.Bedrock:

                    case BlockId.GrieferStone:

                    case BlockId.AirFlood:
                    case BlockId.AirFloodDown:
                    case BlockId.AirFloodLayer:
                    case BlockId.AirFloodUp:

                    case BlockId.BigTnt:
                    case BlockId.NukeTnt:
                    case BlockId.RocketStart:
                    case BlockId.RocketHead:

                    case BlockId.Creeper:
                    case BlockId.ZombieBody:
                    case BlockId.ZombieHead:

                    case BlockId.BirdRed:
                    case BlockId.BirdKill:
                    case BlockId.BirdBlue:

                    case BlockId.FishGold:
                    case BlockId.FishSponge:
                    case BlockId.FishShark:
                    case BlockId.FishSalmon:
                    case BlockId.FishBetta:
                    case BlockId.FishLavaShark:

                    case BlockId.Snake:
                    case BlockId.SnakeTail:
                    case BlockId.FlagBase:
                    case BlockId.RedFlag:
                    case BlockId.BlueFlag:
                    case BlockId.Mine:
                    case BlockId.Trap:

                        b.lowestRank = DefaultRankValue.Operator;
                        break;

                    case BlockId.WoodFloat:
                    case BlockId.LavaSponge:

                    case BlockId.DoorTreeWoodActive:
                    case BlockId.DoorObsidianActive:
                    case BlockId.DoorGlassActive:
                    case BlockId.DoorStoneActive:
                    case BlockId.DoorLeavesActive:
                    case BlockId.DoorSandActive:
                    case BlockId.DoorWoodPlanksActive:
                    case BlockId.DoorGreenActive:
                    case BlockId.DoorTntActive:
                    case BlockId.DoorSlabActive:
                    case BlockId.DoorAirSwitchActive:
                    case BlockId.DoorWaterActive:
                    case BlockId.DoorLavaActive:
                    case BlockId.DoorAirActive:
                    case BlockId.DoorIronActive:
                    case BlockId.DoorGoldActive:
                    case BlockId.DoorCobblestoneActive:
                    case BlockId.DoorRedActive:
                    case BlockId.DoorGrassActive:
                    case BlockId.DoorDirtActive:
                    case BlockId.DoorPurpleActive:
                    case BlockId.DoorBookshelfActive:

                    case BlockId.DoorOrangeActive:
                    case BlockId.DoorYellowActive:
                    case BlockId.DoorLimeActive:
                    case BlockId.DoorAquaGreenActive:
                    case BlockId.DoorCyanActive:
                    case BlockId.DoorBlueActive:
                    case BlockId.DoorIndigoActive:
                    case BlockId.DoorVioletActive:
                    case BlockId.DoorMagentaActive:
                    case BlockId.DoorPinkActive:
                    case BlockId.DoorBlackActive:
                    case BlockId.DoorGrayActive:
                    case BlockId.DoorWhiteActive:

                    case BlockId.ODoorTreeWoodActive:
                    case BlockId.ODoorObsidianActive:
                    case BlockId.ODoorGlassActive:
                    case BlockId.ODoorStoneActive:
                    case BlockId.ODoorLeavesActive:
                    case BlockId.ODoorSandActive:
                    case BlockId.ODoorWoodPlanksActive:
                    case BlockId.ODoorGreenActive:
                    case BlockId.ODoorTntActive:
                    case BlockId.ODoorSlabActive:
                    case BlockId.ODoorAirActive:
                    case BlockId.ODoorWaterActive:

                    case BlockId.AirMessage:
                    case BlockId.BlackMessage:
                    case BlockId.LavaMessage:
                    case BlockId.WaterMessage:
                    case BlockId.WhiteMessage:
                    case BlockId.AirPortal:
                    case BlockId.WaterPortal:
                    case BlockId.LavaPortal:
                    case BlockId.BluePortal:
                    case BlockId.OrangePortal:

                    case BlockId.ActiveWater:
                    case BlockId.ActiveLava:
                    case BlockId.FastLava:
                    case BlockId.Waterfall:
                    case BlockId.Lavafall:
                    case BlockId.WaterFaucet:
                    case BlockId.LavaFaucet:
                    case BlockId.FiniteWater:
                    case BlockId.FiniteLava:
                    case BlockId.FiniteWaterFaucet:
                    case BlockId.FiniteLavaFaucet:
                    case BlockId.ActiveMagma:
                    case BlockId.Geyser:
                    case BlockId.HotLava:
                    case BlockId.ColdWater:
                    case BlockId.NerveGas:
                    case BlockId.ActiveColdWater:
                    case BlockId.ActiveHotLava:
                    case BlockId.FastHotLava:
                    case BlockId.Embers:

                    case BlockId.C4:
                    case BlockId.C4Detonator:
                    case BlockId.SmallTnt:
                    case BlockId.TntExplosion:
                    case BlockId.Firework:

                    case BlockId.Train:

                    case BlockId.BirdWhite:
                    case BlockId.BirdBlack:
                    case BlockId.BirdWater:
                    case BlockId.BirdLava:
                        b.lowestRank = DefaultRankValue.AdvBuilder;
                        break;

                    case BlockId.DoorTreeWood:
                    case BlockId.DoorObsidian:
                    case BlockId.DoorGlass:
                    case BlockId.DoorStone:
                    case BlockId.DoorLeaves:
                    case BlockId.DoorSand:
                    case BlockId.DoorWoodPlanks:
                    case BlockId.DoorGreen:
                    case BlockId.DoorTnt:
                    case BlockId.DoorSlab:
                    case BlockId.DoorAir:
                    case BlockId.AirSwitch:
                    case BlockId.DoorWater:
                    case BlockId.DoorLava:
                    case BlockId.DoorIron:
                    case BlockId.DoorGold:
                    case BlockId.DoorGrass:
                    case BlockId.DoorDirt:
                    case BlockId.DoorPurple:
                    case BlockId.DoorBookshelf:
                    case BlockId.DoorCobblestone:
                    case BlockId.DoorRed:

                    case BlockId.DoorOrange:
                    case BlockId.DoorYellow:
                    case BlockId.DoorLime:
                    case BlockId.DoorAquaGreen:
                    case BlockId.DoorCyan:
                    case BlockId.DoorBlue:
                    case BlockId.DoorIndigo:
                    case BlockId.DoorViolet:
                    case BlockId.DoorMagenta:
                    case BlockId.DoorPink:
                    case BlockId.DoorBlack:
                    case BlockId.DoorGray:
                    case BlockId.DoorWhite:

                    case BlockId.TDoorTreeWood:
                    case BlockId.TDoorObsidian:
                    case BlockId.TDoorGlass:
                    case BlockId.TDoorStone:
                    case BlockId.TDoorLeaves:
                    case BlockId.TDoorSand:
                    case BlockId.TDoorWoodPlanks:
                    case BlockId.TDoorGreen:
                    case BlockId.TDoorTnt:
                    case BlockId.TDoorSlab:
                    case BlockId.TDoorAir:
                    case BlockId.TDoorWater:
                    case BlockId.TDoorLava:

                    case BlockId.ODoorTreeWood:
                    case BlockId.ODoorObsidian:
                    case BlockId.ODoorGlass:
                    case BlockId.ODoorStone:
                    case BlockId.ODoorLeaves:
                    case BlockId.ODoorSand:
                    case BlockId.ODoorWoodPlanks:
                    case BlockId.ODoorGreen:
                    case BlockId.ODoorTnt:
                    case BlockId.ODoorSlab:
                    case BlockId.ODoorAir:
                    case BlockId.ODoorWater:

                        b.lowestRank = DefaultRankValue.Builder;
                        break;

                    default:
                        b.lowestRank = DefaultRankValue.Banned;
                        break;
                }

                // Add the temporary Blocks object, which now has the default rank, to the list.
                storedList.Add(b);
            }

            // If block.properties exist, use the data within to override the default rank values.
            if (File.Exists(blockPropsPath)) {
                string[] lines = File.ReadAllLines(blockPropsPath);

                // Yes, there were two different versions of this file.
                if (lines[0] == "#Version 2") {
                    string[] colon = new string[] { " : " };  // The characters to split by

                    foreach (string line in lines) {
                        if (line != String.Empty && line[0] != '#') {  // Ignore comments and blank lines
                            // Format of each line - Name : Lowest : Disallow : Allow
                            string[] block = line.Split(colon, StringSplitOptions.None);
                            BlockPerm newBlock = new BlockPerm();  // Make another temporary Blocks object

                            // Ignore invalid entries in the blocks.properties file
                            if (BlockData.Ushort(block[0]) == BlockId.Null) {
                                continue;
                            }
                            // Using the name provided, get the block's equivalent numerical value.
                            newBlock.type = BlockData.Ushort(block[0]);

                            // Obtain the disallowed and allowed permission values if there are any.
                            string[] disallow = new string[0];
                            if (block[2] != String.Empty) {
                                disallow = block[2].Split(',');
                            }
                            string[] allow = new string[0];
                            if (block[3] != String.Empty) {
                                allow = block[3].Split(',');
                            }

                            // Set the permission values for that block type.
                            try {
                                newBlock.lowestRank = int.Parse(block[1]);
                                foreach (string s in disallow) {
                                    newBlock.disallow.Add(int.Parse(s));
                                }
                                foreach (string s in allow) {
                                    newBlock.allow.Add(int.Parse(s));
                                }
                            }
                            catch {  // TODO: find exact exception to catch
                                Log("Hit an error on the block " + line);
                                continue;
                            }

                            // Go through storedList and override the default permission values of that particular
                            // block type with the values that were read from blocks.properties.
                            int current = 0;
                            foreach (BlockPerm bS in storedList) {
                                if (newBlock.type == bS.type) {
                                    storedList[current] = newBlock;
                                    break;
                                }
                                current++;
                            }
                        }
                    }
                }
                else {  // Version 1 of blocks.properties.
                    foreach (string s in lines) {
                        if (s[0] != '#') {  // Ignore comments
                            try {
                                // Version 1 did not have disallowed and allowed permission values, so
                                // just get the block's name and permission value.
                                BlockPerm newBlock = new BlockPerm();
                                newBlock.type = BlockData.Ushort(s.Split(' ')[0]);
                                newBlock.lowestRank = _ranks.PermFromName(s.Split(' ')[2]);

                                // Go through storedList and overrid the default permission values of that
                                // particular block type with the values that were read from blocks.properties.
                                if (newBlock.lowestRank != DefaultRankValue.Null) {
                                    storedList[storedList.FindIndex(sL => sL.type == newBlock.type)] = newBlock;
                                }
                                else {
                                    throw new Exception();
                                }
                            }
                            catch {  // TODO: find exact exception to catch
                                Log("Could not find the rank given on " + s + ". Using default");
                            }
                        }
                    }
                }
            }

            // Replace the list of blocks with storedList, which now contains the correct
            // permission values for each block. Save blocks.properties afterwards.
            this._perms.Clear();
            this._perms.AddRange(storedList);
            SaveBlocks();
        }

        /// <summary>
        /// Saves the block permission list to block.properties.
        /// </summary>
        public void SaveBlocks() {
            try {
                using (StreamWriter w = File.CreateText(blockPropsPath)) {
                    // Write the header.
                    w.WriteLine("#Version 2");
                    w.WriteLine("#   This file dictates what levels may use what blocks");
                    w.WriteLine("#   If someone has royally screwed up the ranks, just delete this file and let the server restart");
                    w.WriteLine("#   Allowed ranks: " + _ranks.ConcatPerms());
                    w.WriteLine("#   Disallow and allow can be left empty, just make sure there's 2 spaces between the colons");
                    w.WriteLine("#   This works entirely on permission values, not names. Do not enter a rank name. Use it's permission value");
                    w.WriteLine("#   BlockName : LowestRank : Disallow : Allow");
                    w.WriteLine("#   lava : 60 : 80,67 : 40,41,55");
                    w.WriteLine("");

                    // Create a line for every block.
                    foreach (BlockPerm bs in this._perms) {
                        if (bs.IncludeInBlockProperties()) {  // Some blocks are excluded from being saved.
                            string line = BlockData.Name(bs.type) + " : " + bs.lowestRank + " : " + RankUtil.GetInts(
                                bs.disallow) + " : " + RankUtil.GetInts(bs.allow);
                            w.WriteLine(line);
                        }
                    }
                }
            }
            catch (Exception e) {
                ErrorLog(e);
            }
        }

        /// <summary>
        /// Checks whether a player can place a particular block.
        /// </summary>
        /// <param name="p"> The player that is placing the block. <seealso cref="Player"/></param>
        /// <param name="b"> The numerical id of the block type that is being placed. <seealso cref="BlockId"/></param>
        /// <returns> Whether the player is allowed to place the block. </returns>
        public bool CanPlace(Player p, BlockId b) {
            return CanPlace(p.rank.Permission, b);
        }

        /// <summary>
        /// Checks whether players with the given permission value can place a particular block.
        /// </summary>
        /// <param name="givenPerm"> The permission value to check against. </param>
        /// <param name="givenBlock"> The numerical id of the block type that is being placed.
        /// <seealso cref="BlockId"/></param>
        /// <returns> Whether those of that permission value can place the block. </returns>
        public bool CanPlace(int givenPerm, BlockId givenBlock) {
            foreach (BlockPerm b in this._perms) {
                if (givenBlock == b.type) {
                    if ((b.lowestRank <= givenPerm && !b.disallow.Contains(givenPerm)) || b.allow.Contains(givenPerm)) {
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Obtains the BlockPerm object associated with the given block id.
        /// </summary>
        /// <param name="id"> The id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> The BlockPerm object associated with the given id. <seealso cref="BlockPerm"/></returns>
        /// <exception cref="ArgumentException"> Thrown when the given block id does not map to an actual block
        /// type. </exception>
        public BlockPerm FindById(BlockId id) {
            if (!Enum.IsDefined(typeof(BlockId), id)) {
                throw new ArgumentException("The given BlockId does not represent an actual block type.");
            }

            return this._perms.Find(perm => perm.type == id);
        }

        /// <summary>
        /// Changes the minimum rank required to use a block.
        /// </summary>
        /// <param name="id"> The id of a block type. <seealso cref="BlockId"/></param>
        /// <param name="newPerm"> The permission value representing the minimum rank required to use that block. </param>
        /// <exception cref="ArgumentException"> Thrown when the given block id does not map to an actual block
        /// type. </exception>
        public void ChangeRank(BlockId id, int newPerm) {
            if (!Enum.IsDefined(typeof(BlockId), id)) {
                throw new ArgumentException("The given BlockId does not represent an actual block type.");
            }

            int index = this._perms.FindIndex(perm => perm.type == id);
            _perms[index].lowestRank = newPerm;
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of block permissions.
        /// </summary>
        /// <returns> An IEnumerator<BlockPerm> object for this list of block permissions. </returns>
        public IEnumerator<BlockPerm> GetEnumerator() {
            return this._perms.GetEnumerator();
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of block permissions.
        /// </summary>
        /// <returns> An IEnumerator object for this list of block permissions. </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Logs a BlockPermList-related error message.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        private void Log(string message) {
            Logger temp = _logger;
            if (temp != null) {
                temp.Log(message);
            }
        }

        /// <summary>
        /// Logs a BlockPermList-related exception.
        /// </summary>
        /// <param name="ex"> The exception to log. </param>
        private void ErrorLog(Exception ex) {
            Logger temp = _logger;
            if (temp != null) {
                temp.ErrorLog(ex);
            }
        }
    }
}

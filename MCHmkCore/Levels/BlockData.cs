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

// Disable obsolete warnings in this file since Block.cs has to know about each block, deprecated or not.
// The obsolete warnings are intended for other classes that may use these blocks.
#pragma warning disable 612, 618

using System;
using System.Collections.Generic;

namespace MCHmk {
    /// <summary>
    /// The BlockData class holds information about the characteristics of every block type in MCHmk.
    /// </summary>
    public static class BlockData {
        /// <summary>
        /// The highest block id that is being used.
        /// </summary>
        public const int maxblocks = 273;

        /// <summary>
        /// Checks whether a particular block type can be walked through by a player.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can be walked through by a player. </returns>
        public static bool Walkthrough(BlockId type) {
            switch (type) {
                case BlockId.Air:
                case BlockId.ActiveWater:
                case BlockId.StillWater:
                case BlockId.ActiveLava:
                case BlockId.StillLava:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                case BlockId.Sapling:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type can be activated by any player, regardless of rank.
        /// This is often needed for portals, message blocks, and doors to work properly.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the action of breaking a block of that type should ignore rank. </returns>
        public static bool AllowBreak(BlockId type) {
            switch (type) {
                case BlockId.BluePortal:
                case BlockId.OrangePortal:

                case BlockId.WhiteMessage:
                case BlockId.BlackMessage:

                case BlockId.BlueFlag:
                case BlockId.RedFlag:
                case BlockId.Mine:
                case BlockId.Trap:

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
                case BlockId.DoorIron:
                case BlockId.DoorGold:
                case BlockId.DoorDirt:
                case BlockId.DoorGrass:
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

                case BlockId.C4:
                case BlockId.SmallTnt:
                case BlockId.BigTnt:
                case BlockId.NukeTnt:
                case BlockId.RocketStart:
                case BlockId.Firework:

                case BlockId.ZombieBody:
                case BlockId.Creeper:
                case BlockId.ZombieHead:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type is an ordinary block that can be manually placed by
        /// a non-op player without using commands. These blocks would appear in the vanilla Minecraft's
        /// block menu.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can be manually placed by a non-op player. </returns>
        public static bool Placable(BlockId type) {
            switch (type) {
                case BlockId.Bedrock:
                case BlockId.ActiveWater:
                case BlockId.StillWater:
                case BlockId.ActiveLava:
                case BlockId.StillLava:
                    return false;
            }

            if (System.Convert.ToInt32(type) > 49) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Checks whether a particular block type can be manually replaced by another block.
        /// This method is named RightClick since, by default, right-clicking on an air, water, or lava block
        /// would replace it with the currently held block.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <param name="countAir"> Whether the air block counts. Defaults to false. </param>
        /// <returns> Whether the specified block type can manually replaced by another block. </returns>
        public static bool RightClick(BlockId type, bool countAir = false) {
            if (countAir && type == BlockId.Air) {
                return true;
            }

            switch (type) {
                case BlockId.ActiveWater:
                case BlockId.ActiveLava:
                case BlockId.StillWater:
                case BlockId.StillLava:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type is classified as an operator-only block.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type is classified as an operator-only block.
        public static bool OPBlocks(BlockId type) {
            switch (type) {
                case BlockId.RedFlag:
                case BlockId.BlueFlag:
                case BlockId.Mine:
                case BlockId.Trap:
                case BlockId.Bedrock:
                case BlockId.OpAir:
                case BlockId.OpBrick:
                case BlockId.OpCobblestone:
                case BlockId.OpGlass:
                case BlockId.OpStone:
                case BlockId.OpWater:
                case BlockId.OpLava:
                case BlockId.Opsidian:
                case BlockId.RocketStart:

                case BlockId.Null:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type can cause a player's death.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can cause a player's death. </returns>
        public static bool Death(BlockId type) {
            switch (type) {
                case BlockId.TntExplosion:

                case BlockId.ColdWater:
                case BlockId.HotLava:
                case BlockId.NerveGas:
                case BlockId.ActiveHotLava:
                case BlockId.ActiveColdWater:
                case BlockId.FastHotLava:

                case BlockId.ActiveMagma:
                case BlockId.Geyser:

                case BlockId.BirdKill:
                case BlockId.FishShark:
                case BlockId.FishLavaShark:

                case BlockId.Train:

                case BlockId.Snake:

                case BlockId.Embers:
                case BlockId.RocketHead:

                case BlockId.Creeper:
                case BlockId.ZombieBody:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the provided block id represents non-op lava, non-op water, or air, 
        /// which are blocks that you can "build in" or replace with a standard block.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type is either non-op lava, non-op water, or air. </returns>
        public static bool BuildIn(BlockId type) {
            if (type == BlockId.OpWater || type == BlockId.OpLava || BlockData.portal(type) || BlockData.mb(type)) {
                return false;
            }

            switch (BlockData.Convert(type)) {
                case BlockId.Air:
                case BlockId.ActiveWater:
                case BlockId.ActiveLava:
                case BlockId.StillWater:
                case BlockId.StillLava:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the provided block id represents a block that is activated when the player walks over it.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type is activated by walking over it </returns>
        public static bool Mover(BlockId type) {
            switch (type) {
                case BlockId.AirPortal:
                case BlockId.WaterPortal:
                case BlockId.LavaPortal:

                case BlockId.AirSwitch:
                case BlockId.DoorWater:
                case BlockId.DoorLava:

                case BlockId.AirMessage:
                case BlockId.WaterMessage:
                case BlockId.LavaMessage:

                case BlockId.FlagBase:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type can be destroyed by active lava or fire.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can be destroyed by active lava or fire. </returns>
        public static bool LavaKill(BlockId type) {
            switch (type) {
                case BlockId.WoodPlanks:
                case BlockId.Sapling:
                case BlockId.TreeWood:
                case BlockId.Leaves:
                case BlockId.Sponge:
                case BlockId.Red:
                case BlockId.Orange:
                case BlockId.Yellow:
                case BlockId.Lime:
                case BlockId.Green:
                case BlockId.AquaGreen:
                case BlockId.Cyan:
                case BlockId.Blue:
                case BlockId.Purple:
                case BlockId.Indigo:
                case BlockId.Violet:
                case BlockId.Magenta:
                case BlockId.Pink:
                case BlockId.Black:
                case BlockId.Gray:
                case BlockId.White:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                case BlockId.Bookshelf:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type can be destroyed by active water.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can be destroyed by active water. </returns>
        public static bool WaterKill(BlockId type) {
            switch (type) {
                case BlockId.Sapling:
                case BlockId.Leaves:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether light can pass through a particular block type.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether light can pass through the specified block type. </returns>
        public static bool LightPass(BlockId type) {
            switch (Convert(type)) {
                case BlockId.Air:
                case BlockId.Glass:
                case BlockId.Leaves:
                case BlockId.RedFlower:
                case BlockId.YellowFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                case BlockId.Sapling:
                case BlockId.Rope:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks whether a particular block type needs its physics restarted.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type needs its physics restarted. </returns>
        public static bool NeedRestart(BlockId type) {
            switch (type) {
                case BlockId.Train:

                case BlockId.Snake:
                case BlockId.SnakeTail:

                case BlockId.Embers:
                case BlockId.RocketHead:
                case BlockId.Firework:

                case BlockId.Creeper:
                case BlockId.ZombieBody:
                case BlockId.ZombieHead:

                case BlockId.BirdBlack:
                case BlockId.BirdBlue:
                case BlockId.BirdKill:
                case BlockId.BirdLava:
                case BlockId.BirdRed:
                case BlockId.BirdWater:
                case BlockId.BirdWhite:

                case BlockId.FishBetta:
                case BlockId.FishGold:
                case BlockId.FishSalmon:
                case BlockId.FishShark:
                case BlockId.FishLavaShark:
                case BlockId.FishSponge:

                case BlockId.TntExplosion:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type is a portal.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type is a portal. </returns>
        public static bool portal(BlockId type) {
            switch (type) {
                case BlockId.BluePortal:
                case BlockId.OrangePortal:
                case BlockId.AirPortal:
                case BlockId.WaterPortal:
                case BlockId.LavaPortal:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a particular block type is a message block.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type is a message block. </returns>
        public static bool mb(BlockId type) {
            switch (type) {
                case BlockId.AirMessage:
                case BlockId.WaterMessage:
                case BlockId.LavaMessage:
                case BlockId.BlackMessage:
                case BlockId.WhiteMessage:
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether placing a particular block type can immediately affect physics.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the specified block type can immediately affect physics if placed. </returns>
        public static bool Physics(BlockId type) {
            switch (type) {
                case BlockId.Stone:
                case BlockId.Cobblestone:
                case BlockId.Bedrock:
                case BlockId.StillWater:
                case BlockId.StillLava:
                case BlockId.GoldOre:
                case BlockId.IronOre:
                case BlockId.CoalOre:

                case BlockId.GoldSolid:
                case BlockId.IronSolid:
                case BlockId.DoubleSlab:
                case BlockId.Brick:
                case BlockId.Tnt:
                case BlockId.MossyCobblestone:
                case BlockId.Obsidian:

                case BlockId.OpGlass:
                case BlockId.Opsidian:
                case BlockId.OpBrick:
                case BlockId.OpStone:
                case BlockId.OpCobblestone:
                case BlockId.OpAir:
                case BlockId.OpWater:

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
                case BlockId.DoorIron:
                case BlockId.DoorGold:
                case BlockId.DoorDirt:
                case BlockId.DoorGrass:
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

                case BlockId.DoorAir:
                case BlockId.AirSwitch:
                case BlockId.DoorWater:
                case BlockId.DoorLava:

                case BlockId.AirMessage:
                case BlockId.WaterMessage:
                case BlockId.LavaMessage:
                case BlockId.BlackMessage:
                case BlockId.WhiteMessage:

                case BlockId.BluePortal:
                case BlockId.OrangePortal:
                case BlockId.AirPortal:
                case BlockId.WaterPortal:
                case BlockId.LavaPortal:

                case BlockId.NerveGas:
                case BlockId.HotLava:
                case BlockId.ColdWater:

                case BlockId.FlagBase:
                    return false;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Given a numerical id of a block type, retrieve a string describing that block type.
        /// </summary>
        /// <param name="type"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> A string describing that block type. </returns>
        public static string Name(BlockId type) {
            switch (type) {
                case BlockId.Air:
                    return "air";
                case BlockId.Stone:
                    return "stone";
                case BlockId.Grass:
                    return "grass";
                case BlockId.Dirt:
                    return "dirt";
                case BlockId.Cobblestone:
                    return "cobblestone";
                case BlockId.WoodPlanks:
                    return "wood";
                case BlockId.Sapling:
                    return "plant";
                case BlockId.Bedrock:
                    return "adminium";
                case BlockId.ActiveWater:
                    return "active_water";
                case BlockId.StillWater:
                    return "water";
                case BlockId.ActiveLava:
                    return "active_lava";
                case BlockId.StillLava:
                    return "lava";
                case BlockId.Sand:
                    return "sand";
                case BlockId.Gravel:
                    return "gravel";
                case BlockId.GoldOre:
                    return "gold_ore";
                case BlockId.IronOre:
                    return "iron_ore";
                case BlockId.CoalOre:
                    return "coal";
                case BlockId.TreeWood:
                    return "tree";
                case BlockId.Leaves:
                    return "leaves";
                case BlockId.Sponge:
                    return "sponge";
                case BlockId.Glass:
                    return "glass";
                case BlockId.Red:
                    return "red";
                case BlockId.Orange:
                    return "orange";
                case BlockId.Yellow:
                    return "yellow";
                case BlockId.Lime:
                    return "greenyellow";
                case BlockId.Green:
                    return "green";
                case BlockId.AquaGreen:
                    return "springgreen";
                case BlockId.Cyan:
                    return "cyan";
                case BlockId.Blue:
                    return "blue";
                case BlockId.Purple:
                    return "blueviolet";
                case BlockId.Indigo:
                    return "indigo";
                case BlockId.Violet:
                    return "purple";
                case BlockId.Magenta:
                    return "magenta";
                case BlockId.Pink:
                    return "pink";
                case BlockId.Black:
                    return "black";
                case BlockId.Gray:
                    return "gray";
                case BlockId.White:
                    return "white";
                case BlockId.YellowFlower:
                    return "yellow_flower";
                case BlockId.RedFlower:
                    return "red_flower";
                case BlockId.BrownMushroom:
                    return "brown_shroom";
                case BlockId.RedMushroom:
                    return "red_shroom";
                case BlockId.GoldSolid:
                    return "gold";
                case BlockId.IronSolid:
                    return "iron";
                case BlockId.DoubleSlab:
                    return "double_stair";
                case BlockId.Slab:
                    return "stair";
                case BlockId.Brick:
                    return "brick";
                case BlockId.Tnt:
                    return "tnt";
                case BlockId.Bookshelf:
                    return "bookcase";
                case BlockId.MossyCobblestone:
                    return "mossy_cobblestone";
                case BlockId.Obsidian:
                    return "obsidian";
                case BlockId.CobblestoneSlab:
                    return "cobblestoneslab";
                case BlockId.Rope:
                    return "rope";
                case BlockId.Sandstone:
                    return "sandstone";
                case BlockId.Snow:
                    return "snowreal";
                case BlockId.Fire:
                    return "fire";
                case BlockId.LightPink:
                    return "lightpinkwool";
                case BlockId.ForestGreen:
                    return "forestgreenwool";
                case BlockId.Brown:
                    return "brownwool";
                case BlockId.DeepBlue:
                    return "deepblue";
                case BlockId.Turquoise:
                    return "turquoise";
                case BlockId.Ice:
                    return "ice";
                case BlockId.CeramicTile:
                    return "ceramictile";
                case BlockId.Magma:
                    return "magma";
                case BlockId.Pillar:
                    return "pillar";
                case BlockId.Crate:
                    return "crate";
                case BlockId.StoneBrick:
                    return "stonebrick";
                case BlockId.FlagBase:
                    return "flagbase";
                case BlockId.OldFallingSnow:
                    return "fallsnow";
                case BlockId.OldSnow:
                    return "snow";
                case BlockId.FastHotLava:
                    return "fast_hot_lava";
                case BlockId.OpGlass:
                    return "op_glass";
                case BlockId.Opsidian:
                    return "opsidian";              //TODO Add command or just use bind?
                case BlockId.OpBrick:
                    return "op_brick";              //TODO
                case BlockId.OpStone:
                    return "op_stone";              //TODO
                case BlockId.OpCobblestone:
                    return "op_cobblestone";        //TODO
                case BlockId.OpAir:
                    return "op_air";                //TODO
                case BlockId.OpWater:
                    return "op_water";              //TODO
                case BlockId.OpLava:
                    return "op_lava";

                case BlockId.GrieferStone:
                    return "griefer_stone";
                case BlockId.LavaSponge:
                    return "lava_sponge";

                case BlockId.WoodFloat:
                    return "wood_float";            //TODO
                case BlockId.DoorTreeWood:
                    return "door_wood";
                case BlockId.FastLava:
                    return "lava_fast";
                case BlockId.DoorObsidian:
                    return "door_obsidian";
                case BlockId.DoorGlass:
                    return "door_glass";
                case BlockId.DoorStone:
                    return "door_stone";
                case BlockId.DoorLeaves:
                    return "door_leaves";
                case BlockId.DoorSand:
                    return "door_sand";
                case BlockId.DoorWoodPlanks:
                    return "door_wood";
                case BlockId.DoorGreen:
                    return "door_green";
                case BlockId.DoorTnt:
                    return "door_tnt";
                case BlockId.DoorSlab:
                    return "door_stair";
                case BlockId.DoorIron:
                    return "door_iron";
                case BlockId.DoorGold:
                    return "door_gold";
                case BlockId.DoorCobblestone:
                    return "door_cobblestone";
                case BlockId.DoorRed:
                    return "door_red";
                case BlockId.DoorGrass:
                    return "door_grass";
                case BlockId.DoorDirt:
                    return "door_dirt";
                case BlockId.DoorPurple:
                    return "door_blue";
                case BlockId.DoorBookshelf:
                    return "door_book";

                case BlockId.DoorOrange:
                    return "door_orange";
                case BlockId.DoorYellow:
                    return "door_yellow";
                case BlockId.DoorLime:
                    return "door_lightgreen";
                case BlockId.DoorAquaGreen:
                    return "door_aquagreen";
                case BlockId.DoorCyan:
                    return "door_cyan";
                case BlockId.DoorBlue:
                    return "door_lightblue";
                case BlockId.DoorIndigo:
                    return "door_purple";
                case BlockId.DoorViolet:
                    return "door_lightpurple";
                case BlockId.DoorMagenta:
                    return "door_pink";
                case BlockId.DoorPink:
                    return "door_darkpink";
                case BlockId.DoorBlack:
                    return "door_darkgrey";
                case BlockId.DoorGray:
                    return "door_lightgrey";
                case BlockId.DoorWhite:
                    return "door_white";

                case BlockId.TDoorTreeWood:
                    return "tdoor_wood";
                case BlockId.TDoorObsidian:
                    return "tdoor_obsidian";
                case BlockId.TDoorGlass:
                    return "tdoor_glass";
                case BlockId.TDoorStone:
                    return "tdoor_stone";
                case BlockId.TDoorLeaves:
                    return "tdoor_leaves";
                case BlockId.TDoorSand:
                    return "tdoor_sand";
                case BlockId.TDoorWoodPlanks:
                    return "tdoor_wood";
                case BlockId.TDoorGreen:
                    return "tdoor_green";
                case BlockId.TDoorTnt:
                    return "tdoor_tnt";
                case BlockId.TDoorSlab:
                    return "tdoor_stair";
                case BlockId.TDoorAir:
                    return "tdoor_air";
                case BlockId.TDoorWater:
                    return "tdoor_water";
                case BlockId.TDoorLava:
                    return "tdoor_lava";

                case BlockId.ODoorTreeWood:
                    return "odoor_wood";
                case BlockId.ODoorObsidian:
                    return "odoor_obsidian";
                case BlockId.ODoorGlass:
                    return "odoor_glass";
                case BlockId.ODoorStone:
                    return "odoor_stone";
                case BlockId.ODoorLeaves:
                    return "odoor_leaves";
                case BlockId.ODoorSand:
                    return "odoor_sand";
                case BlockId.ODoorWoodPlanks:
                    return "odoor_wood";
                case BlockId.ODoorGreen:
                    return "odoor_green";
                case BlockId.ODoorTnt:
                    return "odoor_tnt";
                case BlockId.ODoorSlab:
                    return "odoor_stair";
                case BlockId.ODoorAir:
                    return "odoor_lava";
                case BlockId.ODoorWater:
                    return "odoor_water";

                case BlockId.ODoorTreeWoodActive:
                    return "odoor_wood_air";
                case BlockId.ODoorObsidianActive:
                    return "odoor_obsidian_air";
                case BlockId.ODoorGlassActive:
                    return "odoor_glass_air";
                case BlockId.ODoorStoneActive:
                    return "odoor_stone_air";
                case BlockId.ODoorLeavesActive:
                    return "odoor_leaves_air";
                case BlockId.ODoorSandActive:
                    return "odoor_sand_air";
                case BlockId.ODoorWoodPlanksActive:
                    return "odoor_wood_air";
                case BlockId.ODoorGreenActive:
                    return "odoor_red";
                case BlockId.ODoorTntActive:
                    return "odoor_tnt_air";
                case BlockId.ODoorSlabActive:
                    return "odoor_stair_air";
                case BlockId.ODoorAirActive:
                    return "odoor_lava_air";
                case BlockId.ODoorWaterActive:
                    return "odoor_water_air";

                case BlockId.WhiteMessage:
                    return "white_message";
                case BlockId.BlackMessage:
                    return "black_message";
                case BlockId.AirMessage:
                    return "air_message";
                case BlockId.WaterMessage:
                    return "water_message";
                case BlockId.LavaMessage:
                    return "lava_message";

                case BlockId.Waterfall:
                    return "waterfall";
                case BlockId.Lavafall:
                    return "lavafall";
                case BlockId.WaterFaucet:
                    return "water_faucet";
                case BlockId.LavaFaucet:
                    return "lava_faucet";

                case BlockId.FiniteWater:
                    return "finite_water";
                case BlockId.FiniteLava:
                    return "finite_lava";
                case BlockId.FiniteWaterFaucet:
                    return "finite_faucet";
                case BlockId.FiniteLavaFaucet:
                    return "finitelava_faucet";

                case BlockId.AirPortal:
                    return "air_portal";
                case BlockId.WaterPortal:
                    return "water_portal";
                case BlockId.LavaPortal:
                    return "lava_portal";

                case BlockId.DoorAir:
                    return "air_door";
                case BlockId.AirSwitch:
                    return "air_switch";
                case BlockId.DoorWater:
                    return "door_water";
                case BlockId.DoorLava:
                    return "door_lava";

                case BlockId.BluePortal:
                    return "blue_portal";
                case BlockId.OrangePortal:
                    return "orange_portal";

                case BlockId.C4:
                    return "c4";
                case BlockId.C4Detonator:
                    return "c4_det";
                case BlockId.SmallTnt:
                    return "small_tnt";
                case BlockId.BigTnt:
                    return "big_tnt";
                case BlockId.NukeTnt:
                    return "nuke_tnt";
                case BlockId.TntExplosion:
                    return "tnt_explosion";

                case BlockId.Embers:
                    return "embers";

                case BlockId.RocketStart:
                    return "rocketstart";
                case BlockId.RocketHead:
                    return "rockethead";
                case BlockId.Firework:
                    return "firework";

                case BlockId.HotLava:
                    return "hot_lava";
                case BlockId.ColdWater:
                    return "cold_water";
                case BlockId.NerveGas:
                    return "nerve_gas";
                case BlockId.ActiveColdWater:
                    return "active_cold_water";
                case BlockId.ActiveHotLava:
                    return "active_hot_lava";

                case BlockId.ActiveMagma:
                    return "active_magma";
                case BlockId.Geyser:
                    return "geyser";

                //Blocks after this are converted before saving
                case BlockId.AirFlood:
                    return "air_flood";
                case BlockId.DoorTreeWoodActive:
                    return "door_air";
                case BlockId.AirFloodLayer:
                    return "air_flood_layer";
                case BlockId.AirFloodDown:
                    return "air_flood_down";
                case BlockId.AirFloodUp:
                    return "air_flood_up";
                case BlockId.DoorObsidianActive:
                    return "door2_air";
                case BlockId.DoorGlassActive:
                    return "door3_air";
                case BlockId.DoorStoneActive:
                    return "door4_air";
                case BlockId.DoorLeavesActive:
                    return "door5_air";
                case BlockId.DoorSandActive:
                    return "door6_air";
                case BlockId.DoorWoodPlanksActive:
                    return "door7_air";
                case BlockId.DoorGreenActive:
                    return "door8_air";
                case BlockId.DoorTntActive:
                    return "door9_air";
                case BlockId.DoorSlabActive:
                    return "door10_air";
                case BlockId.DoorAirSwitchActive:
                    return "door11_air";
                case BlockId.DoorWaterActive:
                    return "door12_air";
                case BlockId.DoorLavaActive:
                    return "door13_air";
                case BlockId.DoorAirActive:
                    return "door14_air";
                case BlockId.DoorIronActive:
                    return "door_iron_air";
                case BlockId.DoorGoldActive:
                    return "door_gold_air";
                case BlockId.DoorDirtActive:
                    return "door_dirt_air";
                case BlockId.DoorGrassActive:
                    return "door_grass_air";
                case BlockId.DoorPurpleActive:
                    return "door_blue_air";
                case BlockId.DoorBookshelfActive:
                    return "door_book_air";
                case BlockId.DoorCobblestoneActive:
                    return "door_cobblestone_air";
                case BlockId.DoorRedActive:
                    return "door_red_air";

                case BlockId.DoorOrangeActive:
                    return "door_orange_air";
                case BlockId.DoorYellowActive:
                    return "door_yellow_air";
                case BlockId.DoorLimeActive:
                    return "door_lightgreen_air";
                case BlockId.DoorAquaGreenActive:
                    return "door_aquagreen_air";
                case BlockId.DoorCyanActive:
                    return "door_cyan_air";
                case BlockId.DoorBlueActive:
                    return "door_lightblue_air";
                case BlockId.DoorIndigoActive:
                    return "door_purple_air";
                case BlockId.DoorVioletActive:
                    return "door_lightpurple_air";
                case BlockId.DoorMagentaActive:
                    return "door_pink_air";
                case BlockId.DoorPinkActive:
                    return "door_darkpink_air";
                case BlockId.DoorBlackActive:
                    return "door_darkgrey_air";
                case BlockId.DoorGrayActive:
                    return "door_lightgrey_air";
                case BlockId.DoorWhiteActive:
                    return "door_white_air";

                //"AI" blocks
                case BlockId.Train:
                    return "train";

                case BlockId.Snake:
                    return "snake";
                case BlockId.SnakeTail:
                    return "snake_tail";

                case BlockId.Creeper:
                    return "creeper";
                case BlockId.ZombieBody:
                    return "zombie";
                case BlockId.ZombieHead:
                    return "zombie_head";

                case BlockId.BirdBlue:
                    return "blue_bird";
                case BlockId.BirdRed:
                    return "red_robin";
                case BlockId.BirdWhite:
                    return "dove";
                case BlockId.BirdBlack:
                    return "pidgeon";
                case BlockId.BirdWater:
                    return "duck";
                case BlockId.BirdLava:
                    return "phoenix";
                case BlockId.BirdKill:
                    return "killer_phoenix";

                case BlockId.FishBetta:
                    return "betta_fish";
                case BlockId.FishGold:
                    return "goldfish";
                case BlockId.FishSalmon:
                    return "salmon";
                case BlockId.FishShark:
                    return "shark";
                case BlockId.FishSponge:
                    return "sea_sponge";
                case BlockId.FishLavaShark:
                    return "lava_shark";

                case BlockId.BlueFlag:
                    return "blueflag";
                case BlockId.RedFlag:
                    return "redflag";
                case BlockId.Mine:
                    return "mine";
                case BlockId.Trap:
                    return "trap";

                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Given a string describing a block type, retrieve the numerical id of that block type.
        /// </summary>
        /// <param name="type"> The string describing a block type. </param>
        /// <returns> The equivalent numerical id of that block type. <seealso cref="BlockId"/></returns>
        public static BlockId Ushort(string type) {
            switch (type.ToLower()) {
                case "air":
                    return BlockId.Air;
                case "stone":
                    return BlockId.Stone;
                case "grass":
                    return BlockId.Grass;
                case "dirt":
                    return BlockId.Dirt;
                case "cobblestone":
                    return BlockId.Cobblestone;
                case "wood":
                    return BlockId.WoodPlanks;
                case "plant":
                    return BlockId.Sapling;
                case "solid":
                case "admintite":
                case "blackrock":
                case "adminium":
                    return BlockId.Bedrock;
                case "activewater":
                case "active_water":
                    return BlockId.ActiveWater;
                case "water":
                    return BlockId.StillWater;
                case "activelava":
                case "active_lava":
                    return BlockId.ActiveLava;
                case "lava":
                    return BlockId.StillLava;
                case "sand":
                    return BlockId.Sand;
                case "gravel":
                    return BlockId.Gravel;
                case "gold_ore":
                    return BlockId.GoldOre;
                case "iron_ore":
                    return BlockId.IronOre;
                case "coal":
                    return BlockId.CoalOre;
                case "tree":
                    return BlockId.TreeWood;
                case "leaves":
                    return BlockId.Leaves;
                case "sponge":
                    return BlockId.Sponge;
                case "glass":
                    return BlockId.Glass;
                case "red":
                    return BlockId.Red;
                case "orange":
                    return BlockId.Orange;
                case "yellow":
                    return BlockId.Yellow;
                case "greenyellow":
                    return BlockId.Lime;
                case "green":
                    return BlockId.Green;
                case "springgreen":
                    return BlockId.AquaGreen;
                case "cyan":
                    return BlockId.Cyan;
                case "blue":
                    return BlockId.Blue;
                case "blueviolet":
                    return BlockId.Purple;
                case "indigo":
                    return BlockId.Indigo;
                case "purple":
                    return BlockId.Violet;
                case "magenta":
                    return BlockId.Magenta;
                case "pink":
                    return BlockId.Pink;
                case "black":
                    return BlockId.Black;
                case "gray":
                    return BlockId.Gray;
                case "white":
                    return BlockId.White;
                case "yellow_flower":
                    return BlockId.YellowFlower;
                case "red_flower":
                    return BlockId.RedFlower;
                case "brown_shroom":
                    return BlockId.BrownMushroom;
                case "red_shroom":
                    return BlockId.RedMushroom;
                case "gold":
                    return BlockId.GoldSolid;
                case "iron":
                    return BlockId.IronSolid;
                case "double_stair":
                    return BlockId.DoubleSlab;
                case "stair":
                    return BlockId.Slab;
                case "brick":
                    return BlockId.Brick;
                case "tnt":
                    return BlockId.Tnt;
                case "bookcase":
                    return BlockId.Bookshelf;
                case "mossy_cobblestone":
                    return BlockId.MossyCobblestone;
                case "obsidian":
                    return BlockId.Obsidian;
                case "cobblestoneslab":
                    return BlockId.CobblestoneSlab;
                case "rope":
                    return BlockId.Rope;
                case "sandstone":
                    return BlockId.Sandstone;
                case "snowreal":
                    return BlockId.Snow;
                case "fire":
                    return BlockId.Fire;
                case "lightpinkwool":
                    return BlockId.LightPink;
                case "forestgreenwool":
                    return BlockId.ForestGreen;
                case "brownwool":
                    return BlockId.Brown;
                case "deepblue":
                    return BlockId.DeepBlue;
                case "turquoise":
                    return BlockId.Turquoise;
                case "ice":
                    return BlockId.Ice;
                case "ceramictile":
                    return BlockId.CeramicTile;
                case "magma":
                    return BlockId.Magma;
                case "pillar":
                    return BlockId.Pillar;
                case "crate":
                    return BlockId.Crate;
                case "stonebrick":
                    return BlockId.StoneBrick;
                case "fallsnow":
                    return BlockId.OldFallingSnow;
                case "snow":
                    return BlockId.OldSnow;
                case "fhl":
                case "fast_hot_lava":
                    return BlockId.FastHotLava;
                case "op_glass":
                    return BlockId.OpGlass;
                case "opsidian":
                    return BlockId.Opsidian;              //TODO Add command or just use bind?
                case "op_brick":
                    return BlockId.OpBrick;              //TODO
                case "op_stone":
                    return BlockId.OpStone;              //TODO
                case "op_cobblestone":
                    return BlockId.OpCobblestone;        //TODO
                case "op_air":
                    return BlockId.OpAir;                //TODO
                case "op_water":
                    return BlockId.OpWater;              //TODO
                case "op_lava":
                    return BlockId.OpLava;

                case "griefer_stone":
                    return BlockId.GrieferStone;
                case "lava_sponge":
                    return BlockId.LavaSponge;

                case "wood_float":
                    return BlockId.WoodFloat;            //TODO
                case "lava_fast":
                    return BlockId.FastLava;

                case "door_tree":
                case "door":
                    return BlockId.DoorTreeWood;
                case "door_obsidian":
                case "door2":
                    return BlockId.DoorObsidian;
                case "door_glass":
                case "door3":
                    return BlockId.DoorGlass;
                case "door_stone":
                case "door4":
                    return BlockId.DoorStone;
                case "door_leaves":
                case "door5":
                    return BlockId.DoorLeaves;
                case "door_sand":
                case "door6":
                    return BlockId.DoorSand;
                case "door_wood":
                case "door7":
                    return BlockId.DoorWoodPlanks;
                case "door_green":
                case "door8":
                    return BlockId.DoorGreen;
                case "door_tnt":
                case "door9":
                    return BlockId.DoorTnt;
                case "door_stair":
                case "door10":
                    return BlockId.DoorSlab;
                case "door11":
                case "door_iron":
                    return BlockId.DoorIron;
                case "door12":
                case "door_dirt":
                    return BlockId.DoorDirt;
                case "door13":
                case "door_grass":
                    return BlockId.DoorGrass;
                case "door14":
                case "door_blue":
                    return BlockId.DoorPurple;
                case "door15":
                case "door_book":
                    return BlockId.DoorBookshelf;
                case "door16":
                case "door_gold":
                    return BlockId.DoorGold;
                case "door17":
                case "door_cobblestone":
                    return BlockId.DoorCobblestone;
                case "door18":
                case "door_red":
                    return BlockId.DoorRed;

                case "door_orange":
                    return BlockId.DoorOrange;
                case "door_yellow":
                    return BlockId.DoorYellow;
                case "door_lightgreen":
                    return BlockId.DoorLime;
                case "door_aquagreen":
                    return BlockId.DoorAquaGreen;
                case "door_cyan":
                    return BlockId.DoorCyan;
                case "door_lightblue":
                    return BlockId.DoorBlue;
                case "door_purple":
                    return BlockId.DoorIndigo;
                case "door_lightpurple":
                    return BlockId.DoorViolet;
                case "door_pink":
                    return BlockId.DoorMagenta;
                case "door_darkpink":
                    return BlockId.DoorPink;
                case "door_darkgrey":
                    return BlockId.DoorBlack;
                case "door_lightgrey":
                    return BlockId.DoorGray;
                case "door_white":
                    return BlockId.DoorWhite;

                case "tdoor_tree":
                case "tdoor":
                    return BlockId.TDoorTreeWood;
                case "tdoor_obsidian":
                case "tdoor2":
                    return BlockId.TDoorObsidian;
                case "tdoor_glass":
                case "tdoor3":
                    return BlockId.TDoorGlass;
                case "tdoor_stone":
                case "tdoor4":
                    return BlockId.TDoorStone;
                case "tdoor_leaves":
                case "tdoor5":
                    return BlockId.TDoorLeaves;
                case "tdoor_sand":
                case "tdoor6":
                    return BlockId.TDoorSand;
                case "tdoor_wood":
                case "tdoor7":
                    return BlockId.TDoorWoodPlanks;
                case "tdoor_green":
                case "tdoor8":
                    return BlockId.TDoorGreen;
                case "tdoor_tnt":
                case "tdoor9":
                    return BlockId.TDoorTnt;
                case "tdoor_stair":
                case "tdoor10":
                    return BlockId.TDoorSlab;
                case "tair_switch":
                case "tdoor11":
                    return BlockId.TDoorAir;
                case "tdoor_water":
                case "tdoor12":
                    return BlockId.TDoorWater;
                case "tdoor_lava":
                case "tdoor13":
                    return BlockId.TDoorLava;

                case "odoor_tree":
                case "odoor":
                    return BlockId.ODoorTreeWood;
                case "odoor_obsidian":
                case "odoor2":
                    return BlockId.ODoorObsidian;
                case "odoor_glass":
                case "odoor3":
                    return BlockId.ODoorGlass;
                case "odoor_stone":
                case "odoor4":
                    return BlockId.ODoorStone;
                case "odoor_leaves":
                case "odoor5":
                    return BlockId.ODoorLeaves;
                case "odoor_sand":
                case "odoor6":
                    return BlockId.ODoorSand;
                case "odoor_wood":
                case "odoor7":
                    return BlockId.ODoorWoodPlanks;
                case "odoor_green":
                case "odoor8":
                    return BlockId.ODoorGreen;
                case "odoor_tnt":
                case "odoor9":
                    return BlockId.ODoorTnt;
                case "odoor_stair":
                case "odoor10":
                    return BlockId.ODoorSlab;
                case "odoor_lava":
                case "odoor11":
                    return BlockId.ODoorAir;
                case "odoor_water":
                case "odoor12":
                    return BlockId.ODoorWater;
                case "odoor_red":
                    return BlockId.ODoorGreenActive;

                case "white_message":
                    return BlockId.WhiteMessage;
                case "black_message":
                    return BlockId.BlackMessage;
                case "air_message":
                    return BlockId.AirMessage;
                case "water_message":
                    return BlockId.WaterMessage;
                case "lava_message":
                    return BlockId.LavaMessage;

                case "waterfall":
                    return BlockId.Waterfall;
                case "lavafall":
                    return BlockId.Lavafall;
                case "water_faucet":
                    return BlockId.WaterFaucet;
                case "lava_faucet":
                    return BlockId.LavaFaucet;

                case "finite_water":
                    return BlockId.FiniteWater;
                case "finite_lava":
                    return BlockId.FiniteLava;
                case "finite_faucet":
                    return BlockId.FiniteWaterFaucet;
                case "finitelava_faucet":
                    return BlockId.FiniteLavaFaucet;

                case "air_portal":
                    return BlockId.AirPortal;
                case "water_portal":
                    return BlockId.WaterPortal;
                case "lava_portal":
                    return BlockId.LavaPortal;

                case "air_door":
                    return BlockId.DoorAir;
                case "air_switch":
                    return BlockId.AirSwitch;
                case "door_water":
                case "water_door":
                    return BlockId.DoorWater;
                case "door_lava":
                case "lava_door":
                    return BlockId.DoorLava;

                case "blue_portal":
                    return BlockId.BluePortal;
                case "orange_portal":
                    return BlockId.OrangePortal;

                case "c4":
                    return BlockId.C4;
                case "c4_det":
                    return BlockId.C4Detonator;
                case "small_tnt":
                    return BlockId.SmallTnt;
                case "big_tnt":
                    return BlockId.BigTnt;
                case "nuke_tnt":
                    return BlockId.NukeTnt;
                case "tnt_explosion":
                    return BlockId.TntExplosion;

                case "embers":
                    return BlockId.Embers;

                case "rocketstart":
                    return BlockId.RocketStart;
                case "rockethead":
                    return BlockId.RocketHead;
                case "firework":
                    return BlockId.Firework;

                case "hot_lava":
                    return BlockId.HotLava;
                case "cold_water":
                    return BlockId.ColdWater;
                case "nerve_gas":
                    return BlockId.NerveGas;
                case "acw":
                case "active_cold_water":
                    return BlockId.ActiveColdWater;
                case "ahl":
                case "active_hot_lava":
                    return BlockId.ActiveHotLava;

                case "active_magma":
                    return BlockId.ActiveMagma;
                case "geyser":
                    return BlockId.Geyser;

                //Blocks after this are converted before saving
                case "air_flood":
                    return BlockId.AirFlood;
                case "air_flood_layer":
                    return BlockId.AirFloodLayer;
                case "air_flood_down":
                    return BlockId.AirFloodDown;
                case "air_flood_up":
                    return BlockId.AirFloodUp;
                case "door_air":
                    return BlockId.DoorTreeWoodActive;
                case "door2_air":
                    return BlockId.DoorObsidianActive;
                case "door3_air":
                    return BlockId.DoorGlassActive;
                case "door4_air":
                    return BlockId.DoorStoneActive;
                case "door5_air":
                    return BlockId.DoorLeavesActive;
                case "door6_air":
                    return BlockId.DoorSandActive;
                case "door7_air":
                    return BlockId.DoorWoodPlanksActive;
                case "door8_air":
                    return BlockId.DoorGreenActive;
                case "door9_air":
                    return BlockId.DoorTntActive;
                case "door10_air":
                    return BlockId.DoorSlabActive;
                case "door11_air":
                    return BlockId.DoorAirSwitchActive;
                case "door12_air":
                    return BlockId.DoorWaterActive;
                case "door13_air":
                    return BlockId.DoorLavaActive;
                case "door14_air":
                    return BlockId.DoorAirActive;
                case "door_iron_air":
                    return BlockId.DoorIronActive;
                case "door_dirt_air":
                    return BlockId.DoorDirtActive;
                case "door_grass_air":
                    return BlockId.DoorGrassActive;
                case "door_blue_air":
                    return BlockId.DoorPurpleActive;
                case "door_book_air":
                    return BlockId.DoorBookshelfActive;
                case "door_gold_air":
                    return BlockId.DoorGoldActive;
                case "door_cobblestone_air":
                    return BlockId.DoorCobblestoneActive;
                case "door_red_air":
                    return BlockId.DoorRedActive;

                case "door_orange_air":
                    return BlockId.DoorOrangeActive;
                case "door_yellow_air":
                    return BlockId.DoorYellowActive;
                case "door_lightgreen_air":
                    return BlockId.DoorLimeActive;
                case "door_aquagreen_air":
                    return BlockId.DoorAquaGreenActive;
                case "door_cyan_air":
                    return BlockId.DoorCyanActive;
                case "door_lightblue_air":
                    return BlockId.DoorBlueActive;
                case "door_purple_air":
                    return BlockId.DoorIndigoActive;
                case "door_lightpurple_air":
                    return BlockId.DoorVioletActive;
                case "door_pink_air":
                    return BlockId.DoorMagentaActive;
                case "door_darkpink_air":
                    return BlockId.DoorPinkActive;
                case "door_darkgrey_air":
                    return BlockId.DoorBlackActive;
                case "door_lightgrey_air":
                    return BlockId.DoorGrayActive;
                case "door_white_air":
                    return BlockId.DoorWhiteActive;

                case "train":
                    return BlockId.Train;

                case "snake":
                    return BlockId.Snake;
                case "snake_tail":
                    return BlockId.SnakeTail;

                case "creeper":
                    return BlockId.Creeper;
                case "zombie":
                    return BlockId.ZombieBody;
                case "zombie_head":
                    return BlockId.ZombieHead;

                case "blue_bird":
                    return BlockId.BirdBlue;
                case "red_robin":
                    return BlockId.BirdRed;
                case "dove":
                    return BlockId.BirdWhite;
                case "pidgeon":
                    return BlockId.BirdBlack;
                case "duck":
                    return BlockId.BirdWater;
                case "phoenix":
                    return BlockId.BirdLava;
                case "killer_phoenix":
                    return BlockId.BirdKill;

                case "betta_fish":
                    return BlockId.FishBetta;
                case "goldfish":
                    return BlockId.FishGold;
                case "salmon":
                    return BlockId.FishSalmon;
                case "shark":
                    return BlockId.FishShark;
                case "sea_sponge":
                    return BlockId.FishSponge;
                case "lava_shark":
                    return BlockId.FishLavaShark;
                case "redflag":
                    return BlockId.RedFlag;
                case "blueflag":
                    return BlockId.BlueFlag;

                default:
                    return BlockId.Null;
            }
        }

        /// <summary>
        /// Given a block id in MCHmk, return the block id that should be written to the server's Set Block packet. 
        /// This is needed since clients are not aware of the block ids used by custom server software.
        /// </summary>
        /// <param name="b"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> The numerical id of the block type that should be sent to the client.
        /// <seealso cref="BlockId"/></returns>
        public static BlockId Convert(BlockId b) {
            switch (b) {
                case BlockId.RedFlag:
                    return BlockId.Red;
                case BlockId.BlueFlag:
                    return BlockId.Purple;
                case BlockId.Mine:
                    return BlockId.Black;
                case BlockId.Trap:
                    return BlockId.BrownMushroom;
                case BlockId.FlagBase:
                    return BlockId.BrownMushroom; //CTF Flagbase
                case BlockId.OpGlass:
                    return BlockId.Glass; //Op_glass
                case BlockId.Opsidian:
                    return BlockId.Obsidian; //Opsidian
                case BlockId.OpBrick:
                    return BlockId.Brick; //Op_brick
                case BlockId.OpStone:
                    return BlockId.Stone; //Op_stone
                case BlockId.OpCobblestone:
                    return BlockId.Cobblestone; //Op_cobblestone
                case BlockId.OpAir:
                    return BlockId.Air; //Op_air - Must be cuboided / replaced
                case BlockId.OpWater:
                    return BlockId.StillWater; //Op_water
                case BlockId.OpLava:
                    return BlockId.StillLava; //Op_lava

                case BlockId.GrieferStone:
                    return BlockId.Stone; //Griefer_stone
                case BlockId.LavaSponge:
                    return BlockId.Sponge; //Lava_sponge

                case BlockId.WoodFloat:
                    return BlockId.WoodPlanks; //wood_float
                case BlockId.FastLava:
                    return BlockId.ActiveLava;
                case BlockId.OldFallingSnow:
                case BlockId.OldSnow:
                    return BlockId.White;
                case BlockId.DoorTreeWood:
                    return BlockId.TreeWood;//door show by treetype
                case BlockId.DoorObsidian:
                    return BlockId.Obsidian;//door show by obsidian
                case BlockId.DoorGlass:
                    return BlockId.Glass;//door show by glass
                case BlockId.DoorStone:
                    return BlockId.Stone;//door show by stone
                case BlockId.DoorLeaves:
                    return BlockId.Leaves;//door show by leaves
                case BlockId.DoorSand:
                    return BlockId.Sand;//door show by sand
                case BlockId.DoorWoodPlanks:
                    return BlockId.WoodPlanks;//door show by wood
                case BlockId.DoorGreen:
                    return BlockId.Green;
                case BlockId.DoorTnt:
                    return BlockId.Tnt;//door show by TNT
                case BlockId.DoorSlab:
                    return BlockId.Slab;//door show by Stair
                case BlockId.DoorIron:
                    return BlockId.IronSolid;
                case BlockId.DoorDirt:
                    return BlockId.Dirt;
                case BlockId.DoorGrass:
                    return BlockId.Grass;
                case BlockId.DoorPurple:
                    return BlockId.Purple;
                case BlockId.DoorBookshelf:
                    return BlockId.Bookshelf;
                case BlockId.DoorGold:
                    return BlockId.GoldSolid;
                case BlockId.DoorCobblestone:
                    return BlockId.Cobblestone;
                case BlockId.DoorRed:
                    return BlockId.Red;

                case BlockId.DoorOrange:
                    return BlockId.Orange;
                case BlockId.DoorYellow:
                    return BlockId.Yellow;
                case BlockId.DoorLime:
                    return BlockId.Lime;
                case BlockId.DoorAquaGreen:
                    return BlockId.AquaGreen;
                case BlockId.DoorCyan:
                    return BlockId.Cyan;
                case BlockId.DoorBlue:
                    return BlockId.Blue;
                case BlockId.DoorIndigo:
                    return BlockId.Indigo;
                case BlockId.DoorViolet:
                    return BlockId.Violet;
                case BlockId.DoorMagenta:
                    return BlockId.Magenta;
                case BlockId.DoorPink:
                    return BlockId.Pink;
                case BlockId.DoorBlack:
                    return BlockId.Black;
                case BlockId.DoorGray:
                    return BlockId.Gray;
                case BlockId.DoorWhite:
                    return BlockId.White;

                case BlockId.TDoorTreeWood:
                    return BlockId.TreeWood;//tdoor show by treetype
                case BlockId.TDoorObsidian:
                    return BlockId.Obsidian;//tdoor show by obsidian
                case BlockId.TDoorGlass:
                    return BlockId.Glass;//tdoor show by glass
                case BlockId.TDoorStone:
                    return BlockId.Stone;//tdoor show by stone
                case BlockId.TDoorLeaves:
                    return BlockId.Leaves;//tdoor show by leaves
                case BlockId.TDoorSand:
                    return BlockId.Sand;//tdoor show by sand
                case BlockId.TDoorWoodPlanks:
                    return BlockId.WoodPlanks;//tdoor show by wood
                case BlockId.TDoorGreen:
                    return BlockId.Green;
                case BlockId.TDoorTnt:
                    return BlockId.Tnt;//tdoor show by TNT
                case BlockId.TDoorSlab:
                    return BlockId.Slab;//tdoor show by Stair
                case BlockId.TDoorAir:
                    return BlockId.Air;
                case BlockId.TDoorWater:
                    return BlockId.StillWater;
                case BlockId.TDoorLava:
                    return BlockId.StillLava;

                case BlockId.ODoorTreeWood:
                    return BlockId.TreeWood;//odoor show by treetype
                case BlockId.ODoorObsidian:
                    return BlockId.Obsidian;//odoor show by obsidian
                case BlockId.ODoorGlass:
                    return BlockId.Glass;//odoor show by glass
                case BlockId.ODoorStone:
                    return BlockId.Stone;//odoor show by stone
                case BlockId.ODoorLeaves:
                    return BlockId.Leaves;//odoor show by leaves
                case BlockId.ODoorSand:
                    return BlockId.Sand;//odoor show by sand
                case BlockId.ODoorWoodPlanks:
                    return BlockId.WoodPlanks;//odoor show by wood
                case BlockId.ODoorGreen:
                    return BlockId.Green;
                case BlockId.ODoorTnt:
                    return BlockId.Tnt;//odoor show by TNT
                case BlockId.ODoorSlab:
                    return BlockId.Slab;//odoor show by Stair
                case BlockId.ODoorAir:
                    return BlockId.StillLava;
                case BlockId.ODoorWater:
                    return BlockId.StillWater;

                case BlockId.WhiteMessage:
                    return BlockId.White;  //upVator
                case BlockId.BlackMessage:
                    return BlockId.Black;  //upVator
                case BlockId.AirMessage:
                    return BlockId.Air;   //upVator
                case BlockId.WaterMessage:
                    return BlockId.StillWater;   //upVator
                case BlockId.LavaMessage:
                    return BlockId.StillLava;  //upVator

                case BlockId.Waterfall:
                    return BlockId.ActiveWater;
                case BlockId.Lavafall:
                    return BlockId.ActiveLava;
                case BlockId.WaterFaucet:
                    return BlockId.Cyan;
                case BlockId.LavaFaucet:
                    return BlockId.Orange;

                case BlockId.FiniteWater:
                    return BlockId.ActiveWater;
                case BlockId.FiniteLava:
                    return BlockId.ActiveLava;
                case BlockId.FiniteWaterFaucet:
                    return BlockId.Blue;
                case BlockId.FiniteLavaFaucet:
                    return BlockId.Orange;

                case BlockId.AirPortal:
                    return BlockId.Air;//air portal
                case BlockId.WaterPortal:
                    return BlockId.StillWater;//water portal
                case BlockId.LavaPortal:
                    return BlockId.StillLava;//lava portal

                case BlockId.DoorAir:
                    return BlockId.Air;
                case BlockId.AirSwitch:
                    return BlockId.Air;//air door
                case BlockId.DoorWater:
                    return BlockId.StillWater;//water door
                case BlockId.DoorLava:
                    return BlockId.StillLava;

                case BlockId.BluePortal:
                    return BlockId.Blue;//blue portal
                case BlockId.OrangePortal:
                    return BlockId.Orange;//orange portal

                case BlockId.C4:
                    return BlockId.Tnt;
                case BlockId.C4Detonator:
                    return BlockId.Red;
                case BlockId.SmallTnt:
                    return BlockId.Tnt;//smalltnt
                case BlockId.BigTnt:
                    return BlockId.Tnt;//bigtnt
                case BlockId.NukeTnt:
                    return BlockId.Tnt;//nuketnt
                case BlockId.TntExplosion:
                    return BlockId.ActiveLava;//explosion

                case BlockId.Embers:
                    return BlockId.ActiveLava;

                case BlockId.RocketStart:
                    return BlockId.Glass;
                case BlockId.RocketHead:
                    return BlockId.GoldSolid;
                case BlockId.Firework:
                    return BlockId.IronSolid;

                case BlockId.ColdWater:
                    return BlockId.StillWater;
                case BlockId.HotLava:
                    return BlockId.StillLava;
                case BlockId.NerveGas:
                    return BlockId.Air;
                case BlockId.ActiveColdWater:
                    return BlockId.ActiveWater;
                case BlockId.ActiveHotLava:
                    return BlockId.ActiveLava;
                case BlockId.FastHotLava:
                    return BlockId.ActiveLava;

                case BlockId.ActiveMagma:
                    return BlockId.ActiveLava;
                case BlockId.Geyser:
                    return BlockId.ActiveWater;

                case BlockId.AirFlood: //air_flood
                case BlockId.DoorTreeWoodActive: //door_air
                case BlockId.AirFloodLayer: //air_flood_layer
                case BlockId.AirFloodDown: //air_flood_down
                case BlockId.AirFloodUp: //air_flood_up
                case BlockId.DoorObsidianActive: //door2_air
                case BlockId.DoorGlassActive: //door3_air
                case BlockId.DoorStoneActive: //door4_air
                case BlockId.DoorLeavesActive: //door5_air
                case BlockId.DoorSandActive: //door6_air
                case BlockId.DoorWoodPlanksActive: //door7_air
                case BlockId.DoorSlabActive: //door10_air
                case BlockId.DoorAirSwitchActive: //door10_air
                case BlockId.DoorWaterActive: //door10_air
                case BlockId.DoorLavaActive: //door10_air
                case BlockId.DoorAirActive:
                case BlockId.DoorIronActive:
                case BlockId.DoorGoldActive:
                case BlockId.DoorCobblestoneActive:
                case BlockId.DoorRedActive:
                case BlockId.DoorDirtActive:
                case BlockId.DoorGrassActive:
                case BlockId.DoorPurpleActive:
                case BlockId.DoorBookshelfActive:
                    return BlockId.Air;
                case BlockId.DoorTntActive:
                    return BlockId.ActiveLava;
                case BlockId.DoorGreenActive:
                    return BlockId.Red;

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
                case BlockId.ODoorSlabActive:
                case BlockId.ODoorAirActive:
                case BlockId.ODoorWaterActive:
                    return BlockId.Air;
                case BlockId.ODoorGreenActive:
                    return BlockId.Red;
                case BlockId.ODoorTntActive:
                    return BlockId.StillLava;

                case BlockId.Train:
                    return BlockId.Cyan;

                case BlockId.Snake:
                    return BlockId.Black;
                case BlockId.SnakeTail:
                    return BlockId.CoalOre;

                case BlockId.Creeper:
                    return BlockId.Tnt;
                case BlockId.ZombieBody:
                    return BlockId.MossyCobblestone;
                case BlockId.ZombieHead:
                    return BlockId.Lime;

                case BlockId.BirdWhite:
                    return BlockId.White;
                case BlockId.BirdBlack:
                    return BlockId.Black;
                case BlockId.BirdLava:
                    return BlockId.ActiveLava;
                case BlockId.BirdRed:
                    return BlockId.Red;
                case BlockId.BirdWater:
                    return BlockId.ActiveWater;
                case BlockId.BirdBlue:
                    return BlockId.Purple;
                case BlockId.BirdKill:
                    return BlockId.ActiveLava;

                case BlockId.FishBetta:
                    return BlockId.Purple;
                case BlockId.FishGold:
                    return BlockId.GoldSolid;
                case BlockId.FishSalmon:
                    return BlockId.Red;
                case BlockId.FishShark:
                    return BlockId.Gray;
                case BlockId.FishSponge:
                    return BlockId.Sponge;
                case BlockId.FishLavaShark:
                    return BlockId.Obsidian;

                default:
                    if (System.Convert.ToInt32(b) < 66 || b == BlockId.Air) {
                        return b;
                    }
                    else {
                        return BlockId.Orange;
                    }
            }
        }

        /// <summary>
        /// Given a block id, checks if the block id represents a CPE block, and if so, returns its fallback id 
        /// for clients that do not support CPE.
        /// <param name="b"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> If the provided block is a CPE block, returns the id of the fallback block type. 
        /// Otherwise, returns the provided id. <seealso cref="BlockId"/></returns>
        public static BlockId ConvertCPE(BlockId b) {
            switch (b) {
                case BlockId.CobblestoneSlab:
                    return BlockId.Slab;
                case BlockId.Rope:
                    return BlockId.BrownMushroom;
                case BlockId.Sandstone:
                    return BlockId.Sand;
                case BlockId.Snow:
                    return BlockId.Air;
                case BlockId.Fire:
                    return BlockId.ActiveLava;
                case BlockId.LightPink:
                    return BlockId.Pink;
                case BlockId.ForestGreen:
                    return BlockId.Green;
                case BlockId.Brown:
                    return BlockId.Dirt;
                case BlockId.DeepBlue:
                    return BlockId.Purple;
                case BlockId.Turquoise:
                    return BlockId.Blue;
                case BlockId.Ice:
                    return BlockId.Glass;
                case BlockId.CeramicTile:
                    return BlockId.IronSolid;
                case BlockId.Magma:
                    return BlockId.Obsidian;
                case BlockId.Pillar:
                    return BlockId.White;
                case BlockId.Crate:
                    return BlockId.WoodPlanks;
                case BlockId.StoneBrick:
                    return BlockId.Stone;
                default:
                    return b;
            }
        }

        /// <summary>
        /// Given a block id, check if the block should not be saved in the level file, and if so, returns a
        /// block id that should be saved instead.
        /// 
        /// This is used, for example, to save door_air blocks as doors instead upon saving, since loading 
        /// door_air blocks may cause unintended or broken physics behavior.
        /// </summary>
        /// <param name="b"> The numerical id of a block type that is about to be written to a level file. 
        /// <seealso cref="BlockId"/></param>
        /// <returns> If the provided block is excluded from being saved to a level file, a suitable block 
        /// id is returned instead. Otherwise, the provided block id is returned.
        /// <seealso cref="BlockId"/></returns>
        public static BlockId SaveConvert(BlockId b) {
            switch (b) {
                case BlockId.OldSnow:
                    return BlockId.Grass;
                case BlockId.OldFallingSnow:
                case BlockId.AirFlood:
                case BlockId.AirFloodLayer:
                case BlockId.AirFloodDown:
                case BlockId.AirFloodUp:
                    return BlockId.Air; //air_flood must be converted to air on save to prevent issues
                case BlockId.DoorTreeWoodActive:
                    return BlockId.DoorTreeWood; //door_air back into door
                case BlockId.DoorObsidianActive:
                    return BlockId.DoorObsidian; //door_air back into door
                case BlockId.DoorGlassActive:
                    return BlockId.DoorGlass; //door_air back into door
                case BlockId.DoorStoneActive:
                    return BlockId.DoorStone; //door_air back into door
                case BlockId.DoorLeavesActive:
                    return BlockId.DoorLeaves; //door_air back into door
                case BlockId.DoorSandActive:
                    return BlockId.DoorSand; //door_air back into door
                case BlockId.DoorWoodPlanksActive:
                    return BlockId.DoorWoodPlanks; //door_air back into door
                case BlockId.DoorGreenActive:
                    return BlockId.DoorGreen; //door_air back into door
                case BlockId.DoorTntActive:
                    return BlockId.DoorTnt; //door_air back into door
                case BlockId.DoorSlabActive:
                    return BlockId.DoorSlab; //door_air back into door
                case BlockId.DoorAirSwitchActive:
                    return BlockId.AirSwitch; //door_air back into door
                case BlockId.DoorWaterActive:
                    return BlockId.DoorWater; //door_air back into door
                case BlockId.DoorLavaActive:
                    return BlockId.DoorLava; //door_air back into door
                case BlockId.DoorAirActive:
                    return BlockId.DoorAir; //door_air back into door
                case BlockId.DoorIronActive:
                    return BlockId.DoorIron;
                case BlockId.DoorGoldActive:
                    return BlockId.DoorGold;
                case BlockId.DoorDirtActive:
                    return BlockId.DoorDirt;
                case BlockId.DoorGrassActive:
                    return BlockId.DoorGrass;
                case BlockId.DoorPurpleActive:
                    return BlockId.DoorPurple;
                case BlockId.DoorBookshelfActive:
                    return BlockId.DoorBookshelf;
                case BlockId.DoorCobblestoneActive:
                    return BlockId.DoorCobblestone;
                case BlockId.DoorRedActive:
                    return BlockId.DoorRed;

                case BlockId.DoorOrangeActive:
                    return BlockId.DoorOrange;
                case BlockId.DoorYellowActive:
                    return BlockId.DoorYellow;
                case BlockId.DoorLimeActive:
                    return BlockId.DoorLime;
                case BlockId.DoorAquaGreenActive:
                    return BlockId.DoorAquaGreen;
                case BlockId.DoorCyanActive:
                    return BlockId.DoorCyan;
                case BlockId.DoorBlueActive:
                    return BlockId.DoorBlue;
                case BlockId.DoorIndigoActive:
                    return BlockId.DoorIndigo;
                case BlockId.DoorVioletActive:
                    return BlockId.DoorViolet;
                case BlockId.DoorMagentaActive:
                    return BlockId.DoorMagenta;
                case BlockId.DoorPinkActive:
                    return BlockId.DoorPink;
                case BlockId.DoorBlackActive:
                    return BlockId.DoorBlack;
                case BlockId.DoorGrayActive:
                    return BlockId.DoorGray;
                case BlockId.DoorWhiteActive:
                    return BlockId.DoorWhite;

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
                    return odoor(b);

                default:
                    return b;
            }
        }

        /// <summary>
        /// Obtains the equivalent door_air block of a regular door block.
        /// </summary>
        /// <param name="b"> The numerical id of a block type, which should represent a door block.
        /// <seealso cref="BlockId"/></param>
        /// <returns> The numerical id of the equivalent door_air block, or the air block if the provided 
        /// block is not a door_air. <seealso cref="BlockId"/></returns>
        public static BlockId DoorAirs(BlockId b) {
            switch (b) {
                case BlockId.DoorTreeWood:
                    return BlockId.DoorTreeWoodActive;
                case BlockId.DoorObsidian:
                    return BlockId.DoorObsidianActive;
                case BlockId.DoorGlass:
                    return BlockId.DoorGlassActive;
                case BlockId.DoorStone:
                    return BlockId.DoorStoneActive;
                case BlockId.DoorLeaves:
                    return BlockId.DoorLeavesActive;
                case BlockId.DoorSand:
                    return BlockId.DoorSandActive;
                case BlockId.DoorWoodPlanks:
                    return BlockId.DoorWoodPlanksActive;
                case BlockId.DoorGreen:
                    return BlockId.DoorGreenActive;
                case BlockId.DoorTnt:
                    return BlockId.DoorTntActive;
                case BlockId.DoorSlab:
                    return BlockId.DoorSlabActive;
                case BlockId.AirSwitch:
                    return BlockId.DoorAirSwitchActive;
                case BlockId.DoorWater:
                    return BlockId.DoorWaterActive;
                case BlockId.DoorLava:
                    return BlockId.DoorLavaActive;
                case BlockId.DoorAir:
                    return BlockId.DoorAirActive;
                case BlockId.DoorIron:
                    return BlockId.DoorIronActive;
                case BlockId.DoorGold:
                    return BlockId.DoorGoldActive;
                case BlockId.DoorDirt:
                    return BlockId.DoorDirtActive;
                case BlockId.DoorGrass:
                    return BlockId.DoorGrassActive;
                case BlockId.DoorPurple:
                    return BlockId.DoorPurpleActive;
                case BlockId.DoorBookshelf:
                    return BlockId.DoorBookshelfActive;
                case BlockId.DoorCobblestone:
                    return BlockId.DoorCobblestoneActive;
                case BlockId.DoorRed:
                    return BlockId.DoorRedActive;

                case BlockId.DoorOrange:
                    return BlockId.DoorOrangeActive;
                case BlockId.DoorYellow:
                    return BlockId.DoorYellowActive;
                case BlockId.DoorLime:
                    return BlockId.DoorLimeActive;
                case BlockId.DoorAquaGreen:
                    return BlockId.DoorAquaGreenActive;
                case BlockId.DoorCyan:
                    return BlockId.DoorCyanActive;
                case BlockId.DoorBlue:
                    return BlockId.DoorBlueActive;
                case BlockId.DoorIndigo:
                    return BlockId.DoorIndigoActive;
                case BlockId.DoorViolet:
                    return BlockId.DoorVioletActive;
                case BlockId.DoorMagenta:
                    return BlockId.DoorMagentaActive;
                case BlockId.DoorPink:
                    return BlockId.DoorPinkActive;
                case BlockId.DoorBlack:
                    return BlockId.DoorBlackActive;
                case BlockId.DoorGray:
                    return BlockId.DoorGrayActive;
                case BlockId.DoorWhite:
                    return BlockId.DoorWhiteActive;
            default:
                return BlockId.Air;
            }
        }

        /// <summary>
        /// Checks if a block type is a tdoor.
        /// </summary>
        /// <param name="b"> The numerical id of a block type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the block type with that id is a tdoor. </returns>
        public static bool tDoor(BlockId b) {
            switch (b) {
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
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Obtains the equivalent air block if an odoor is provided, or obtains the equivalent oDoor block 
        /// if an odoor_air is provided.
        /// </summary>
        /// <param name="b"> The numerical id of a block type, which should be an oDoor or oDoor_air. 
        /// <seealso cref="BlockId""/></param>
        /// <returns> The numerical id of the equivalent odoor or odoor_air block, or BlockId.Zero if the 
        /// provided block is not an odoor or odoor_air. <seealso cref="BlockId""/></returns>
        public static BlockId odoor(BlockId b) {
            switch (b) {
                case BlockId.ODoorTreeWood:
                    return BlockId.ODoorTreeWoodActive;
                case BlockId.ODoorObsidian:
                    return BlockId.ODoorObsidianActive;
                case BlockId.ODoorGlass:
                    return BlockId.ODoorGlassActive;
                case BlockId.ODoorStone:
                    return BlockId.ODoorStoneActive;
                case BlockId.ODoorLeaves:
                    return BlockId.ODoorLeavesActive;
                case BlockId.ODoorSand:
                    return BlockId.ODoorSandActive;
                case BlockId.ODoorWoodPlanks:
                    return BlockId.ODoorWoodPlanksActive;
                case BlockId.ODoorGreen:
                    return BlockId.ODoorGreenActive;
                case BlockId.ODoorTnt:
                    return BlockId.ODoorTntActive;
                case BlockId.ODoorSlab:
                    return BlockId.ODoorSlabActive;
                case BlockId.ODoorAir:
                    return BlockId.ODoorAirActive;
                case BlockId.ODoorWater:
                    return BlockId.ODoorWaterActive;

                case BlockId.ODoorTreeWoodActive:
                    return BlockId.ODoorTreeWood;
                case BlockId.ODoorObsidianActive:
                    return BlockId.ODoorObsidian;
                case BlockId.ODoorGlassActive:
                    return BlockId.ODoorGlass;
                case BlockId.ODoorStoneActive:
                    return BlockId.ODoorStone;
                case BlockId.ODoorLeavesActive:
                    return BlockId.ODoorLeaves;
                case BlockId.ODoorSandActive:
                    return BlockId.ODoorSand;
                case BlockId.ODoorWoodPlanksActive:
                    return BlockId.ODoorWoodPlanks;
                case BlockId.ODoorGreenActive:
                    return BlockId.ODoorGreen;
                case BlockId.ODoorTntActive:
                    return BlockId.ODoorTnt;
                case BlockId.ODoorSlabActive:
                    return BlockId.ODoorSlab;
                case BlockId.ODoorAirActive:
                    return BlockId.ODoorAir;
                case BlockId.ODoorWaterActive:
                    return BlockId.ODoorWater;
                }
            return BlockId.Null;
        }
    }
}
/*
    Copyright 2016 Jjp137

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

namespace MCHmk {
    /// <summary>
    /// The BlockId enumeration represents the block types that are recognized by MCHmk.
    /// </summary>
    public enum BlockId : ushort {
        /// <summary>
        /// The air block.
        /// </summary>
        Air = 0,
        /// <summary>
        /// The smooth stone block.
        /// </summary>
        Stone = 1,
        /// <summary>
        /// The grass block.
        /// </summary>
        Grass = 2,
        /// <summary>
        /// The dirt block.
        /// </summary>
        Dirt = 3,
        /// <summary>
        /// The cobblestone block.
        /// </summary>
        Cobblestone = 4,
        /// <summary>
        /// The wooden planks block.
        /// </summary>
        WoodPlanks = 5,
        /// <summary>
        /// The sapling.
        /// </summary>
        Sapling = 6,
        /// <summary>
        /// The adminium block, also known as bedrock.
        /// </summary>
        Bedrock = 7,
        /// <summary>
        /// The active water block.
        /// </summary>
        ActiveWater = 8,
        /// <summary>
        /// The still water block. Is "water" in-game.
        /// </summary>
        StillWater = 9,
        /// <summary>
        /// The active lava block.
        /// </summary>
        ActiveLava = 10,
        /// <summary>
        /// The still lava block. Is "lava" in-game.
        /// </summary>
        StillLava = 11,
        /// <summary>
        /// The sand block.
        /// </summary>
        Sand = 12,
        /// <summary>
        /// The gravel block.
        /// </summary>
        Gravel = 13,
        /// <summary>
        /// The gold ore block.
        /// </summary>
        GoldOre = 14,
        /// <summary>
        /// The iron ore block.
        /// </summary>
        IronOre = 15,
        /// <summary>
        /// The coal block.
        /// </summary>
        CoalOre = 16,
        /// <summary>
        /// The tree wood block.
        /// </summary>
        TreeWood = 17,
        /// <summary>
        /// The leaves block.
        /// </summary>
        Leaves = 18,
        /// <summary>
        /// The sponge block.
        /// </summary>
        Sponge = 19,
        /// <summary>
        /// The glass block.
        /// </summary>
        Glass = 20,
        /// <summary>
        /// The red cloth block.
        /// </summary>
        Red = 21,
        /// <summary>
        /// The orange cloth block.
        /// </summary>
        Orange = 22,
        /// <summary>
        /// The yellow cloth block.
        /// </summary>
        Yellow = 23,
        /// <summary>
        /// The lime cloth block. Is "yellowgreen" in-game.
        /// </summary>
        Lime = 24,
        /// <summary>
        /// The green cloth block.
        /// </summary>
        Green = 25,
        /// <summary>
        /// The aqua green cloth block. Is "springgreen" in-game.
        /// </summary>
        AquaGreen = 26,
        /// <summary>
        /// The cyan cloth block.
        /// </summary>
        Cyan = 27,
        /// <summary>
        /// The blue cloth block. Is "blue" in-game.
        /// </summary>
        Blue = 28,
        /// <summary>
        /// The purple cloth block. Is "blueviolet" in-game.
        /// </summary>
        Purple = 29,
        /// <summary>
        /// The indigo cloth block.
        /// </summary>
        Indigo = 30,
        /// <summary>
        /// The violet cloth block.
        /// </summary>
        Violet = 31,
        /// <summary>
        /// The magneta cloth block.
        /// </summary>
        Magenta = 32,
        /// <summary>
        /// The pink cloth block.
        /// </summary>
        Pink = 33,
        /// <summary>
        /// The black cloth block.
        /// </summary>
        Black = 34,
        /// <summary>
        /// The gray cloth block.
        /// </summary>
        Gray = 35,
        /// <summary>
        /// The white cloth block.
        /// </summary>
        White = 36,
        /// <summary>
        /// The yellow flower.
        /// </summary>
        YellowFlower = 37,
        /// <summary>
        /// The red flower.
        /// </summary>
        RedFlower = 38,
        /// <summary>
        /// The brown mushroom.
        /// </summary>
        BrownMushroom = 39,
        /// <summary>
        /// The red mushroom.
        /// </summary>
        RedMushroom = 40,
        /// <summary>
        /// The solid gold block.
        /// </summary>
        GoldSolid = 41,
        /// <summary>
        /// The solid iron block.
        /// </summary>
        IronSolid = 42,
        /// <summary>
        /// The double staircase block.
        /// </summary>
        DoubleSlab = 43,
        /// <summary>
        /// The staircase slab.
        /// </summary>
        Slab = 44,
        /// <summary>
        /// The brick block.
        /// </summary>
        Brick = 45,
        /// <summary>
        /// The TNT block.
        /// </summary>
        Tnt = 46,
        /// <summary>
        /// The bookshelf block.
        /// </summary>
        Bookshelf = 47,
        /// <summary>
        /// The mossy cobblestone block.
        /// </summary>
        MossyCobblestone = 48,
        /// <summary>
        /// The obsidian block.
        /// </summary>
        Obsidian = 49,
        /// <summary>
        /// The cobblestone slab block. CPE only.
        /// </summary>
        CobblestoneSlab = 50,
        /// <summary>
        /// The rope. CPE only.
        /// </summary>
        Rope = 51,
        /// <summary>
        /// The sandstone block. CPE only.
        /// </summary>
        Sandstone = 52,
        /// <summary>
        /// The snow layer, as seen in the full version of Minecraft. CPE only.
        /// </summary>
        Snow = 53,
        /// <summary>
        /// The fire block, as seen in the full version of Minecraft. CPE only.
        /// </summary>
        Fire = 54,
        /// <summary>
        /// The light pink cloth block. CPE only.
        /// </summary>
        LightPink = 55,
        /// <summary>
        /// The forest green cloth block. CPE only.
        /// </summary>
        ForestGreen = 56,
        /// <summary>
        /// The brown cloth block. CPE only.
        /// </summary>
        Brown = 57,
        /// <summary>
        /// The deep blue cloth block. CPE only.
        /// </summary>
        DeepBlue = 58,
        /// <summary>
        /// The turquoise cloth block. CPE only.
        /// </summary>
        Turquoise = 59,
        /// <summary>
        /// The ice block. CPE only.
        /// </summary>
        Ice = 60,
        /// <summary>
        /// The ceramic tile block. CPE only.
        /// </summary>
        CeramicTile = 61,
        /// <summary>
        /// The magma block. CPE only.
        /// </summary>
        Magma = 62,
        /// <summary>
        /// The pillar block. CPE only.
        /// </summary>
        Pillar = 63,
        /// <summary>
        /// The crate block. CPE only.
        /// </summary>
        Crate = 64,
        /// <summary>
        /// The stone brick block. CPE only.
        /// </summary>
        StoneBrick = 65,

        /// <summary>
        /// The CTF flag base. Appears as a brown mushroom. Deprecated.
        /// </summary>
        FlagBase = 70,

        /// <summary>
        /// The falling variant of the snow block. Deprecated.
        /// </summary>
        [Obsolete("MCHmk's falling snow block is no longer used.")]
        OldFallingSnow = 71,
        /// <summary>
        /// The snow block. Appears as white cloth. Deprecated.
        /// </summary>
        [Obsolete("MCHmk's snow block is no longer used.")]
        OldSnow = 72,

        /// <summary>
        /// The fast variant of active lava that kills players.
        /// </summary>
        FastHotLava = 73,

        /// <summary>
        /// The C4 block. Appears as TNT.
        /// </summary>
        C4 = 74,
        /// <summary>
        /// The detonator for a C4 block. Appears as a red cloth block.
        /// </summary>
        C4Detonator = 75,

        /// <summary>
        /// The door block with the appearance of cobblestone.
        /// </summary>
        DoorCobblestone = 80,
        /// <summary>
        /// The equivalent air block for a door made out of cobblestone.
        /// </summary>
        DoorCobblestoneActive = 81,
        /// <summary>
        /// The door block with the appearance of red cloth. Not to be confused with door8_air,
        /// which is door_green's air block.
        /// </summary>
        DoorRed = 83,
        /// <summary>
        /// The equivalent air block for a door made out of red cloth.
        /// </summary>
        DoorRedActive = 84,

        /// <summary>
        /// The door block with the appearance of orange cloth.
        /// </summary>
        DoorOrange = 85,
        /// <summary>
        /// The door block with the appearance of yellow cloth.
        /// </summary>
        DoorYellow = 86,
        /// <summary>
        /// The door block with the appearance of lime cloth.
        /// </summary>
        DoorLime = 87,
        /// <summary>
        /// The door block with the appearance of aqua green cloth.
        /// </summary>
        DoorAquaGreen = 89,
        /// <summary>
        /// The door block with the appearance of cyan cloth.
        /// </summary>
        DoorCyan = 90,
        /// <summary>
        /// The door block with the appearance of blue cloth.
        /// </summary>
        DoorBlue = 91,
        /// <summary>
        /// The door block with the appearance of indigo cloth.
        /// </summary>
        DoorIndigo = 92,
        /// <summary>
        /// The door block with the appearance of violet cloth.
        /// </summary>
        DoorViolet = 93,
        /// <summary>
        /// The door block with the appearance of magenta cloth.
        /// </summary>
        DoorMagenta = 94,
        /// <summary>
        /// The door block with the appearance of pink cloth.
        /// </summary>
        DoorPink = 95,
        /// <summary>
        /// The door block with the appearance of black cloth.
        /// </summary>
        DoorBlack = 96,
        /// <summary>
        /// The door block with the appearance of gray cloth.
        /// </summary>
        DoorGray = 97,
        /// <summary>
        /// The door block with the appearance of white cloth.
        /// </summary>
        DoorWhite = 98,

        /// <summary>
        /// The operator-only glass block.
        /// </summary>
        OpGlass = 100,
        /// <summary>
        /// The operator-only obsidian block.
        /// </summary>
        Opsidian = 101,
        /// <summary>
        /// The operator-only brick block.
        /// </summary>
        OpBrick = 102,
        /// <summary>
        /// The operator-only smooth stone block.
        /// </summary>
        OpStone = 103,
        /// <summary>
        /// The operator-only cobblestone block.
        /// </summary>
        OpCobblestone = 104,
        /// <summary>
        /// The operator-only air block.
        /// </summary>
        OpAir = 105,
        /// <summary>
        /// The operator-only water block.
        /// </summary>
        OpWater = 106,
        /// <summary>
        /// The operator-only lava block.
        /// </summary>
        OpLava = 107,

        /// <summary>
        /// The griefer stone block, which punishes those that break it. Its appearance can be set
        /// in server.properties.
        /// </summary>
        [Obsolete("Griefer stones are no longer used.")]
        GrieferStone = 108,
        /// <summary>
        /// The sponge block that works for lava.
        /// </summary>
        LavaSponge = 109,
        /// <summary>
        /// The falling wood block.
        /// </summary>
        WoodFloat = 110,
        /// <summary>
        /// The door block with the appearance of tree wood.
        /// </summary>
        DoorTreeWood = 111,
        /// <summary>
        /// The fast variant of active lava.
        /// </summary>
        FastLava = 112,
        /// <summary>
        /// The door block with the appearance of obsidian.
        /// </summary>
        DoorObsidian = 113,
        /// <summary>
        /// The door block with the appearance of glass.
        /// </summary>
        DoorGlass = 114,
        /// <summary>
        /// The door block with the appearance of smooth stone.
        /// </summary>
        DoorStone = 115,
        /// <summary>
        /// The door block with the appearance of tree leaves.
        /// </summary>
        DoorLeaves = 116,
        /// <summary>
        /// The door block with the appearance of sand.
        /// </summary>
        DoorSand = 117,
        /// <summary>
        /// The door block with the appearance of wooden planks.
        /// </summary>
        DoorWoodPlanks = 118,
        /// <summary>
        /// The door block with the appearance of green cloth. Unlike other door blocks,
        /// this door block acts as a switch. It turns into red cloth when activated.
        /// </summary>
        DoorGreen = 119,
        /// <summary>
        /// The door block with the appearance of TNT. Turns into lava when activated.
        /// </summary>
        DoorTnt = 120,
        /// <summary>
        /// The door block with the appearance of a staircase slab.
        /// </summary>
        DoorSlab = 121,

        /// <summary>
        /// The tdoor block with the appearance of tree wood.
        /// </summary>
        TDoorTreeWood = 122,
        /// <summary>
        /// The tdoor block with the appearance of obsidian.
        /// </summary>
        TDoorObsidian = 123,
        /// <summary>
        /// The tdoor block with the appearance of glass.
        /// </summary>
        TDoorGlass = 124,
        /// <summary>
        /// The tdoor block with the appearance of smooth stone.
        /// </summary>
        TDoorStone = 125,
        /// <summary>
        /// The tdoor block with the appearance of tree leaves.
        /// </summary>
        TDoorLeaves = 126,
        /// <summary>
        /// The tdoor block with the appearance of sand.
        /// </summary>
        TDoorSand = 127,
        /// <summary>
        /// The tdoor block with the appearance of wooden planks.
        /// </summary>
        TDoorWoodPlanks = 128,
        /// <summary>
        /// The tdoor block with the appearance of green cloth. Unlike its door equivalent,
        /// it disappears upon activation.
        /// </summary>
        TDoorGreen = 129,

        /// <summary>
        /// The message block that looks like white cloth. Activates when broken.
        /// </summary>
        WhiteMessage = 130,
        /// <summary>
        /// The message block that looks like black cloth. Activates when broken.
        /// </summary>
        BlackMessage = 131,
        /// <summary>
        /// The message block that is invisible. Activates when walked over.
        /// </summary>
        AirMessage = 132,
        /// <summary>
        /// The message block that looks like water. Activates when walked over.
        /// </summary>
        WaterMessage = 133,
        /// <summary>
        /// The message block that looks like lava. Activates when walked over.
        /// </summary>
        LavaMessage = 134,

        /// <summary>
        /// The tdoor block with the appearance of TNT.
        /// </summary>
        TDoorTnt = 135,
        /// <summary>
        /// The tdoor block with the appearance of a staircase slab.
        /// </summary>
        TDoorSlab = 136,
        /// <summary>
        /// The tdoor block that is invisible.
        /// </summary>
        TDoorAir = 137,
        /// <summary>
        /// The tdoor block with the appearance of water.
        /// </summary>
        TDoorWater = 138,
        /// <summary>
        /// The tdoor block with the appearance of lava.
        /// </summary>
        TDoorLava = 139,

        /// <summary>
        /// The active water block that spreads straight downwards until it touches the floor.
        /// Afterwards, it spreads horizontally.
        /// </summary>
        Waterfall = 140,
        /// <summary>
        /// The active lava block that spreads straight downwards until it touches the floor.
        /// Afterwards, it spreads horizontally.
        /// </summary>
        Lavafall = 141,
        /// <summary>
        /// The block that generates the waterfall block below it periodically.
        /// Appears as a cyan cloth block.
        /// </summary>
        WaterFaucet = 143,
        /// <summary>
        /// The block that generates the lavafall block below it periodically.
        /// Appears as an orange cloth block.
        /// </summary>
        LavaFaucet = 144,

        /// <summary>
        /// The block that acts like a drop of water. It moves as if affected by gravity and does not spread.
        /// </summary>
        FiniteWater = 145,
        /// <summary>
        /// The block that acts like a drop of lava. It moves as if affected by gravity and does not spread.
        /// </summary>
        FiniteLava = 146,
        /// <summary>
        /// The block that continuously generates finite water blocks. Appears as a light blue cloth block.
        /// </summary>
        FiniteWaterFaucet = 147,

        /// <summary>
        /// The odoor block with the appearance of tree wood.
        /// </summary>
        ODoorTreeWood = 148,
        /// <summary>
        /// The odoor block with the appearance of obsidian.
        /// </summary>
        ODoorObsidian = 149,
        /// <summary>
        /// The odoor block with the appearance of glass.
        /// </summary>
        ODoorGlass = 150,
        /// <summary>
        /// The odoor block with the appearance of smooth stone.
        /// </summary>
        ODoorStone = 151,
        /// <summary>
        /// The odoor block with the appearance of tree leaves.
        /// </summary>
        ODoorLeaves = 152,
        /// <summary>
        /// The odoor block with the appearance of sand.
        /// </summary>
        ODoorSand = 153,
        /// <summary>
        /// The odoor block with the appearance of wooden planks.
        /// </summary>
        ODoorWoodPlanks = 154,
        /// <summary>
        /// The odoor block with the appearance of green cloth. Turns into red cloth when activated.
        /// </summary>
        ODoorGreen = 155,
        /// <summary>
        /// The odoor block with the appearance of TNT.
        /// </summary>
        ODoorTnt = 156,
        /// <summary>
        /// The odoor block with the appearance of a staircase slab.
        /// </summary>
        ODoorSlab = 157,
        /// <summary>
        /// The odoor block that is invisible.
        /// </summary>
        ODoorAir = 158,
        /// <summary>
        /// The odoor block with the appearance of water.
        /// </summary>
        ODoorWater = 159,

        /// <summary>
        /// The portal block that is invisible. Activates when walked over.
        /// </summary>
        AirPortal = 160,
        /// <summary>
        /// The portal block that looks like water. Activates when walked over.
        /// </summary>
        WaterPortal = 161,
        /// <summary>
        /// The portal block that looks likes lava. Activates when walked over.
        /// </summary>
        LavaPortal = 162,

        /// <summary>
        /// The door block that is invisible and is only activated by other doors.
        /// </summary>
        DoorAir = 164,
        /// <summary>
        /// The door block that is invisible and activates when walked over.
        /// </summary>
        AirSwitch = 165,
        /// <summary>
        /// The door block with the appearance of water. Activates when walked over.
        /// </summary>
        DoorWater = 166,
        /// <summary>
        /// The door block with the appearance of lava. Activates when walked over.
        /// </summary>
        DoorLava = 167,

        /// <summary>
        /// The equivalent air block for an odoor made out of tree wood.
        /// </summary>
        ODoorTreeWoodActive = 168,
        /// <summary>
        /// The equivalent air block for an odoor made out of obsidian.
        /// </summary>
        ODoorObsidianActive = 169,
        /// <summary>
        /// The equivalent air block for an odoor made out of glass.
        /// </summary>
        ODoorGlassActive = 170,
        /// <summary>
        /// The equivalent air block for an odoor made out of smooth stone.
        /// </summary>
        ODoorStoneActive = 171,
        /// <summary>
        /// The equivalent air block for an odoor made out of tree leaves.
        /// </summary>
        ODoorLeavesActive = 172,
        /// <summary>
        /// The equivalent air block for an odoor made out of sand.
        /// </summary>
        ODoorSandActive = 173,
        /// <summary>
        /// The equivalent air block for an odoor made out of wooden planks.
        /// </summary>
        ODoorWoodPlanksActive = 174,

        /// <summary>
        /// The portal block that looks like blue cloth. Activates when broken.
        /// </summary>
        BluePortal = 175,
        /// <summary>
        /// The portal block that looks like orange cloth. Activates when broken.
        /// </summary>
        OrangePortal = 176,

        /// <summary>
        /// The equivalent air block for an odoor made out of green cloth. Appears as red cloth and changes
        /// back to green cloth when activated.
        /// </summary>
        ODoorGreenActive = 177,
        /// <summary>
        /// The equivalent air block for an odoor made out of TNT.
        /// </summary>
        ODoorTntActive = 178,
        /// <summary>
        /// The equivalent air block for an odoor made out of a staircase slab.
        /// </summary>
        ODoorSlabActive = 179,
        /// <summary>
        /// The equivalent air block for an odoor made out of nothing but air.
        /// </summary>
        ODoorAirActive = 180,
        /// <summary>
        /// The equivalent air block for an odoor made out of water.
        /// </summary>
        ODoorWaterActive = 181,

        /// <summary>
        /// The TNT block that creates a small explosion.
        /// </summary>
        SmallTnt = 182,
        /// <summary>
        /// The TNT block that creates a large explosion.
        /// </summary>
        BigTnt = 183,
        /// <summary>
        /// The lava block generated by TNT explosions.
        /// </summary>
        TntExplosion = 184,

        /// <summary>
        /// The block that spreads among burnable blocks and destroys them. Appears as a lava block.
        /// </summary>
        Embers = 185,

        /// <summary>
        /// The TNT block that creates a huge explosion.
        /// </summary>
        NukeTnt = 186,

        /// <summary>
        /// The block that generates a rocket when broken. Appears as a glass block.
        /// </summary>
        RocketStart = 187,
        /// <summary>
        /// The block that represents the head of a rocket. Appears as a solid gold block.
        /// </summary>
        RocketHead = 188,
        /// <summary>
        /// The block that generates fireworks when broken. Appears as a solid iron block.
        /// </summary>
        Firework = 189,

        /// <summary>
        /// The hot lava block. Kills players on contact.
        /// </summary>
        HotLava = 190,
        /// <summary>
        /// The cold water block. Kills players on contact.
        /// </summary>
        ColdWater = 191,
        /// <summary>
        /// The nerve gas block. Kills players on contact.
        /// </summary>
        NerveGas = 192,

        /// <summary>
        /// The active variant of the cold water block. Kills players on contact.
        /// </summary>
        ActiveColdWater = 193,
        /// <summary>
        /// The active variant of the hot lava block. Kills players on contact.
        /// </summary>
        ActiveHotLava = 194,

        /// <summary>
        /// The block that acts the same as active hot lava. Kills players on contact. The death message
        /// is different from active lava.
        /// </summary>
        ActiveMagma = 195,
        /// <summary>
        /// The block that acts the same as active cold water, except that the death message suggests
        /// that the water is actually boiling. Kills players on contact.
        /// </summary>
        Geyser = 196,

        /// <summary>
        /// The block that replaces its current block with air, and then attempts to spread this effect
        /// to adjacent blocks. Used to remove active liquids. Is invisible.
        /// </summary>
        AirFlood = 200,
        /// <summary>
        /// The equivalent air block for a door made out of tree wood.
        /// </summary>
        DoorTreeWoodActive = 201,
        /// <summary>
        /// The variant of air_flood that only spreads its effect on the same horizontal plane.
        /// </summary>
        AirFloodLayer = 202,
        /// <summary>
        /// The variant of air_flood that only spreads its effect downwards.
        /// </summary>
        AirFloodDown = 203,
        /// <summary>
        /// The variant of air_flood that only spreads its effect upwards.
        /// </summary>
        AirFloodUp = 204,
        /// <summary>
        /// The equivalent air block for a door made out of obsidian.
        /// </summary>
        DoorObsidianActive = 205,
        /// <summary>
        /// The equivalent air block for a door made out of glass.
        /// </summary>
        DoorGlassActive = 206,
        /// <summary>
        /// The equivalent air block for a door made out of smooth stone.
        /// </summary>
        DoorStoneActive = 207,
        /// <summary>
        /// The equivalent air block for a door made out of tree leaves.
        /// </summary>
        DoorLeavesActive = 208,
        /// <summary>
        /// The equivalent air block for a door made out of sand.
        /// </summary>
        DoorSandActive = 209,
        /// <summary>
        /// The equivalent air block for a door made out of wooden planks.
        /// </summary>
        DoorWoodPlanksActive = 210,
        /// <summary>
        /// The equivalent air block for the door that is actually a switch made out of green cloth.
        /// This block appears as red cloth, and it turns back into green cloth after a while.
        /// </summary>
        DoorGreenActive = 211,
        /// <summary>
        /// The equivalent air block for a door made out of TNT. Appears as a lava block.
        /// </summary>
        DoorTntActive = 212,
        /// <summary>
        /// The equivalent air block for a door made out of a staircase slab.
        /// </summary>
        DoorSlabActive = 213,
        /// <summary>
        /// The equivalent air block for an air switch.
        /// </summary>
        DoorAirSwitchActive = 214,
        /// <summary>
        /// The equivalent air block for a door made out of water.
        /// </summary>
        DoorWaterActive = 215,
        /// <summary>
        /// The equivalent air block for a door made out of lava.
        /// </summary>
        DoorLavaActive = 216,
        /// <summary>
        /// The equivalent air block for an air_door. (It works bizarrely, though.)
        /// </summary>
        DoorAirActive = 217,

        /// <summary>
        /// The door block with the appearance of a solid iron block.
        /// </summary>
        DoorIron = 220,
        /// <summary>
        /// The door block with the appearance of a dirt block.
        /// </summary>
        DoorDirt = 221,
        /// <summary>
        /// The door block with the appearance of a grass block.
        /// </summary>
        DoorGrass = 222,
        /// <summary>
        /// The door block with the appearance of a purple cloth block.
        /// </summary>
        DoorPurple = 223,
        /// <summary>
        /// The door block with the appearance of a bookshelf block.
        /// </summary>
        DoorBookshelf = 224,
        /// <summary>
        /// The equivalent air block for a door made out of solid iron.
        /// </summary>
        DoorIronActive = 225,
        /// <summary>
        /// The equivalent air block for a door made out of dirt.
        /// </summary>
        DoorDirtActive = 226,
        /// <summary>
        /// The equivalent air block for a door made out of grass.
        /// </summary>
        DoorGrassActive = 227,
        /// <summary>
        /// The equivalent air block for a door made out of purple cloth.
        /// </summary>
        DoorPurpleActive = 228,
        /// <summary>
        /// The equivalent air block for a door made out of a bookshelf.
        /// </summary>
        DoorBookshelfActive = 229,

        /// <summary>
        /// The block that represents a train, which moves around on red cloth rails.
        /// </summary>
        Train = 230,

        /// <summary>
        /// The block that represents a creeper, which explodes on contact with a player. Appears
        /// as a TNT block.
        /// </summary>
        Creeper = 231,
        /// <summary>
        /// The block that represents the lower part of a zombie. Zombies kill players on contact.
        /// Appears as a mossy cobblestone block.
        /// </summary>
        ZombieBody = 232,
        /// <summary>
        /// The block that represents the head of both creepers and zombies, 
        /// </summary>
        ZombieHead = 233,

        /// <summary>
        /// The block that represents a dove. Moves randomly. Appears as a white cloth block.
        /// </summary>
        BirdWhite = 235,
        /// <summary>
        /// The block that represents a pidgeon. Moves randomly. Appears as a black cloth block.
        /// </summary>
        BirdBlack = 236,
        /// <summary>
        /// The block that represents a duck. Moves randomly. Appears as a water block.
        /// </summary>
        BirdWater = 237,
        /// <summary>
        /// The block that represents a phoenix. Moves randomly. Appears as a lava block.
        /// </summary>
        BirdLava = 238,
        /// <summary>
        /// The block that represents a red robin. Moves towards the nearest player.
        /// Appears as a red cloth block.
        /// </summary>
        BirdRed = 239,
        /// <summary>
        /// The block that represents a bluebird. Moves towards the nearest player.
        /// Appears as a purple cloth block.
        /// </summary>
        BirdBlue = 240,
        /// <summary>
        /// The block that represents a killer phoenix. Moves towards the nearest player.
        /// Kills players on contact. Appears as a lava block.
        /// </summary>
        BirdKill = 242,

        /// <summary>
        /// The block that represents a goldfish. Moves randomly. Appears as a solid gold block.
        /// </summary>
        FishGold = 245,
        /// <summary>
        /// The block that represents a sea sponge. Moves randomly. Appears as a sponge block.
        /// </summary>
        FishSponge = 246,
        /// <summary>
        /// The block that represents a water shark. Moves towards the nearest player.
        /// Kills players on contact. Appears as a gray cloth block.
        /// </summary>
        FishShark = 247,
        /// <summary>
        /// The block that represents a salmon. Moves randomly. Appears as a red cloth block.
        /// </summary>
        FishSalmon = 248,
        /// <summary>
        /// The block that represents a betta. Moves towards the nearest player.
        /// Appears as a purple cloth block.
        /// </summary>
        FishBetta = 249,
        /// <summary>
        /// The block that represents a lava shark. Moves towards the nearest player.
        /// Kills players on contact. Appears as an obsidian block.
        /// </summary>
        FishLavaShark = 250,

        /// <summary>
        /// The block that represents the snake, which moves around and eats certain blocks. Kills players
        /// on contact. Appears as a black cloth block.
        /// </summary>
        Snake = 251,
        /// <summary>
        /// The block that represents the tail of a snake. Appears as a coal block.
        /// </summary>
        SnakeTail = 252,

        /// <summary>
        /// The door block with the appearance of a solid gold block.
        /// </summary>
        DoorGold = 253,
        /// <summary>
        /// The equivalent air block for a door made out of solid gold.
        /// </summary>
        DoorGoldActive = 254,

        /// <summary>
        /// An invalid block. Used as the equivalent for a null value in many cases.
        /// </summary>
        Null = 255,

        /// <summary>
        /// The block that continuously generates finite lava blocks. Appears as an orange cloth block.
        /// </summary>
        FiniteLavaFaucet = 256,
        /// <summary>
        /// The red flag in CTF. Appears as a red cloth block. Deprecated.
        /// </summary>
        RedFlag = 257,
        /// <summary>
        /// The blue flag in CTF. Appears as a blue cloth block. Deprecated.
        /// </summary>
        BlueFlag = 258,
        /// <summary>
        /// The mine block in CTF. Appears as a black cloth block. Deprecated.
        /// </summary>
        Mine = 259,
        /// <summary>
        /// The trap block in CTF. Appears as a brown mushroom. Deprecated.
        /// </summary>
        Trap = 260,

        /// <summary>
        /// The equivalent air block for a door made out of orange cloth.
        /// </summary>
        DoorOrangeActive = 261,
        /// <summary>
        /// The equivalent air block for a door made out of yellow cloth.
        /// </summary>
        DoorYellowActive = 262,
        /// <summary>
        /// The equivalent air block for a door made out of lime cloth.
        /// </summary>
        DoorLimeActive = 263,
        /// <summary>
        /// The equivalent air block for a door made out of aqua green cloth.
        /// </summary>
        DoorAquaGreenActive = 264,
        /// <summary>
        /// The equivalent air block for a door made out of cyan cloth.
        /// </summary>
        DoorCyanActive = 265,
        /// <summary>
        /// The equivalent air block for a door made out of blue cloth.
        /// </summary>
        DoorBlueActive = 266,
        /// <summary>
        /// The equivalent air block for a door made out of indigo cloth.
        /// </summary>
        DoorIndigoActive = 267,
        /// <summary>
        /// The equivalent air block for a door made out of violet cloth.
        /// </summary>
        DoorVioletActive = 268,
        /// <summary>
        /// The equivalent air block for a door made out of magenta cloth.
        /// </summary>
        DoorMagentaActive = 269,
        /// <summary>
        /// The equivalent air block for a door made out of pink cloth.
        /// </summary>
        DoorPinkActive = 270,
        /// <summary>
        /// The equivalent air block for a door made out of black cloth.
        /// </summary>
        DoorBlackActive = 271,
        /// <summary>
        /// The equivalent air block for a door made out of gray cloth.
        /// </summary>
        DoorGrayActive = 272,
        /// <summary>
        /// The equivalent air block for a door made out of white cloth.
        /// </summary>
        DoorWhiteActive = 273
    }
}

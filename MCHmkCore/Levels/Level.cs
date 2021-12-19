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
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

using MCHmk.SQL;

using Timer = System.Timers.Timer;
//WARNING! DO NOT CHANGE THE WAY THE LEVEL IS SAVED/LOADED!
//You MUST make it able to save and load as a new version other wise you will make old levels incompatible!

namespace MCHmk {
    /// <summary>
    /// The Level class represents a level on the server.
    /// </summary>
    public sealed class Level : IDisposable {
        /// <summary>
        /// The name of the level.
        /// </summary>
        public string name;

        /// <summary>
        /// The size of the map on the x axis.
        /// </summary>
        public ushort width;
        /// <summary>
        /// The size of the map on the y axis. Note that the y axis in the game is the vertical axis
        /// and not a horizontal axis, which can be confusing.
        /// </summary>
        public ushort height;
        /// <summary>
        /// The size of the map on the z axis. Note that the z axis in the game is a horizontal axis
        /// and not the vertical axis, which can be confusing.
        /// </summary>
        public ushort depth;

        /// <summary>
        /// Alias for the size of the level in the z axis.
        /// </summary>
        public ushort length {
            get {
                return depth;
            }
        }

        /// <summary>
        /// The current position of the undo buffer.
        /// </summary>
        public int currentUndo;
        /// <summary>
        /// A List of block placement actions that can be undone. This List only stores changes
        /// caused by physics. Use Player.UndoBuffer to undo a player's actions.
        /// </summary>
        public List<UndoPos> UndoBuffer = new List<UndoPos>();

        /// <summary>
        /// Holds all the block changes that have yet to be saved to the SQL table.
        /// </summary>
        public List<BlockPos> blockCache = new List<BlockPos>();

        /// <summary>
        /// Whether block changes from certain commands should be queued instead of sent
        /// immediately. Applies to this level only.
        /// </summary>
        public bool bufferblocks;
        /// <summary>
        /// The queue of block changes waiting to be sent to other players in the level.
        /// </summary>
        public List<BlockQueue.block> blockqueue = new List<BlockQueue.block>();

        /// <summary>
        /// The x-coordinate of the spawn point.
        /// </summary>
        public ushort spawnx;
        /// <summary>
        /// The y-coordinate of the spawn point.
        /// </summary>
        public ushort spawny;
        /// <summary>
        /// The z-coordinate of the spawn point.
        /// </summary>
        public ushort spawnz;
        /// <summary>
        /// The angle on the x-axis that the player is facing when they spawn on the level.
        /// </summary>
        public byte rotx;
        /// <summary>
        /// The angle on the y-axis that the player is facing when they spawn on the level.
        /// </summary>
        public byte roty;

        /// <summary>
        /// The x-coordinate of the jail spawn point.
        /// </summary>
        public ushort jailx;
        /// <summary>
        /// The y-coordinate of the jail spawn point.
        /// </summary>
        public ushort jaily;
        /// <summary>
        /// The z-coordinate of the jail spawn point.
        /// </summary>
        public ushort jailz;
        /// <summary>
        /// The rotation of the player's head on the x-axis at the jail spawn point.
        /// </summary>
        public byte jailrotx;
        /// <summary>
        /// The rotation of the player's head on the y-axis at the jail spawn point.
        /// </summary>
        public byte jailroty;

        /// <summary>
        /// The physics simulation for this level.
        /// </summary>
        public Physics physic = new Physics();
        /// <summary>
        /// The integer representing the physics setting of the level.
        /// </summary>
        int Physicsint;
        /// <summary>
        /// The physics setting of the level.
        /// </summary>
        public int physics {
            get {
                return Physicsint;
            }
            set {
                if (value > 0 && Physicsint == 0) {
                    physic.StartPhysics(this);
                }
                Physicsint = value;
            }
        }
        /// <summary>
        /// Whether the physics is currently changing.
        /// </summary>
        public bool physicschanged {
            get {
                return ListCheck.Count > 0;
            }
        }

        /// <summary>
        /// The players that are on this level.
        /// </summary>
        public List<Player> players {
            get {
                return getPlayers();
            }
        }

        /// <summary>
        /// Whether the water on the edges of the level can flow into the level boundaries.
        /// </summary>
        public bool edgeWater;
        /// <summary>
        /// Whether active liquids act like finite liquids instead of spreading outwards.
        /// </summary>
        public bool finite;
        /// <summary>
        /// Whether active liquids flow randomly.
        /// </summary>
        public bool randomFlow = true;
        /// <summary>
        /// Whether block entities consider nearby players when moving.
        /// </summary>
        public bool ai = true;
        /// <summary>
        /// Whether fall damage and drowning are enabled.
        /// </summary>
        public bool Death;
        /// <summary>
        /// The distance that a player needs to fall before a player dies from falling damage.
        /// </summary>
        public int fall = 9;
        /// <summary>
        /// The amount of time it takes before a player dies from drowning.
        /// </summary>
        public int drown = 70;
        /// <summary>
        /// Whether the level is unloaded if there are no players on it.
        /// </summary>
        public bool unload = true;
        /// <summary>
        /// Whether physics automatically start for the map. (Is not currently being used. -Jjp137)
        /// </summary>
        public bool rp = true;
        /// <summary>
        /// Whether instant building is enabled, which allows players to instantly do certain
        /// building operations without lag. As a consequence, however, the block changes are not
        /// sent to other players in the server until those players reload the map. Block changes
        /// from other players are also not sent while a map is in instant mode.
        /// </summary>
        public bool Instant;
        /// <summary>
        /// Whether blocks that kill players work.
        /// </summary>
        public bool Killer = true;
        /// <summary>
        /// Whether grass below a block turns to dirt if that block is destroyed.
        /// </summary>
        public bool GrassDestroy = true;
        /// <summary>
        /// Whether placed dirt turns into grass if physics is on.
        /// </summary>
        public bool GrassGrow = true;
        /// <summary>
        /// Whether trees grow in the map.
        /// </summary>
        public bool growTrees;
        /// <summary>
        /// Whether leaves decay if no tree wood is nearby.
        /// </summary>
        public bool leafDecay;
        /// <summary>
        /// Whether players can use guns and missiles in the level.
        /// </summary>
        public bool guns = true;
        /// <summary>
        /// The byte value representing the current weather on the map.
        /// </summary>
        public byte weather;
        /// <summary>
        /// Whether the level is loaded if a player tries to go to this level when it is unloaded.
        /// </summary>
        public bool loadOnGoto = true;
        /// <summary>
        /// Whether messages sent by players in this level are sent to everyone in the server.
        /// If false, they are only sent to players in the same level.
        /// </summary>
        public bool worldChat = true;

        /// <summary>
        /// The array of blocks that together make up the entire level.
        /// </summary>
        public BlockId[] blocks;
        /// <summary>
        /// The list of zones on the map.
        /// </summary>
        public List<Zone> ZoneList;

        /// <summary>
        /// A list of blocks that may need to be updated due to physics-related changes.
        /// </summary>
        public readonly List<Check> ListCheck = new List<Check>();
        /// <summary>
        /// A list of blocks that will be updated after physics calculations are complete.
        /// </summary>
        public readonly List<Update> ListUpdate = new List<Update>();

        /// <summary>
        /// The number of blocks that were checked during the last iteration of the physics loop.
        /// </summary>
        public int lastCheck;
        /// <summary>
        /// The number of blocks that were updated during the last iteration of the physics loop.
        /// </summary>
        public int lastUpdate;

        /// <summary>
        /// Whether the map has changed since it was last saved to disk.
        /// </summary>
        public bool changed;
        /// <summary>
        /// Whether the level has been backed up. Any change to a level makes this false.
        /// </summary>
        public bool backedup;

        /// <summary>
        /// The time in milliseconds between physics updates.
        /// </summary>
        public int speedphysics = 250;
        /// <summary>
        /// The number of milliseconds that the physics must take to update before the physics in
        /// the level is considered to be overloaded.
        /// </summary>
        public int overload = 1500;

        /// <summary>
        /// The message that appears on the loading screen. If it is "ignore", the default
        /// message, as specified in server.properties, is used.
        /// </summary>
        public string motd = "ignore";
        /// <summary>
        /// The maximum permission level that can build on the map (excluding those that can use /pervisitmax).
        /// </summary>
        public int perbuildmax = DefaultRankValue.Nobody;
        /// <summary>
        /// The minimum permission level required to build on the map.
        /// </summary>
        public int permissionbuild = DefaultRankValue.Builder;
        /// <summary>
        /// The minimum permission level required to visit the map.
        /// </summary>
        public int permissionvisit = DefaultRankValue.Guest;
        /// <summary>
        /// The maximum permission level that can visit the map (excluding those that can use /pervisitmax).
        /// </summary>
        public int pervisitmax = DefaultRankValue.Nobody;

        /// <summary>
        /// A list of C4s in the level.
        /// </summary>
        public List<C4.C4s> C4list = new List<C4.C4s>();

        /// <summary>
        /// A reference to the server that has loaded this level.
        /// </summary>
        private Server _server;
        /// <summary>
        /// A reference to the server's logger.
        /// </summary>
        private Logger _logger;
        /// <summary>
        /// A reference to the server's database.
        /// </summary>
        private Database _db;

        /// <summary>
        /// Gets the server that is hosting this level.
        /// </summary>
        public Server Server {
            get {
                return _server;
            }
        }

        /// <summary>
        /// Gets the logger that logs events about this level.
        /// </summary>
        public Logger Logger {
            get {
                return _logger;
            }
        }

        /// <summary>
        /// Gets the database that this level uses.
        /// </summary>
        public Database Db {
            get {
                return _db;
            }
        }

        /// <summary>
        /// Constructs a Level object and creates a level.
        /// </summary>
        /// <param name="svr"> The server creating this level. <seealso cref="Server"/></param> 
        /// <param name="n"> The name of the level. </param>
        /// <param name="x"> The size of the level on the x-axis. </param>
        /// <param name="y"> The size of the level on the y-axis. </param>
        /// <param name="z"> The size of the level on the z-axis. </param>
        /// <param name="type"> The level type. </param>
        /// <param name="seed"> The seed used to randomly generate the level. Defaults to 0. </param>
        /// <param name="useSeed"> Whether the given seed should be used. Defaults to false. </param>
        public Level(Server svr, string n, ushort x, ushort y, ushort z, string type, int seed = 0, bool useSeed = false) {
            // Store a reference to the server and the logger.
            _server = svr;
            _logger = svr.logger;
            _db = svr.database;

            // Initialize this variable first.
            bufferblocks = svr.props.bufferblocks;

            width = x;
            height = y;
            depth = z;

            // Do not make levels that are too small.
            if (width < 16) {
                width = 16;
            }
            if (height < 16) {
                height = 16;
            }
            if (depth < 16) {
                depth = 16;
            }

            name = n;
            blocks = new BlockId[width * height * depth];
            ZoneList = new List<Zone>();

            // As you read this, remember that all the values in the blocks array are initialized
            // to zero, and zero represents the air block.

            var half = (ushort)(height / 2);
            switch (type) {
            case "flat":
                for (x = 0; x < width; ++x)
                    for (z = 0; z < depth; ++z)
                        for (y = 0; y <= half; ++y) {
                            SetTile(x, y, z, y < half ? BlockId.Dirt : BlockId.Grass);
                        }
                break;
            case "pixel":
                for (x = 0; x < width; ++x)
                    for (z = 0; z < depth; ++z)
                        for (y = 0; y < height; ++y)
                            if (y == 0) {
                                SetTile(x, y, z, BlockId.Bedrock);  // The bedrock layer
                            }
                            // The walls of the pixel map
                            else if (x == 0 || x == width - 1 || z == 0 || z == depth - 1) {
                                SetTile(x, y, z, BlockId.White);  // The white wool
                            }
                break;

            case "space":
                Random rand = useSeed ? new Random(seed) : new Random();

                for (x = 0; x < width; ++x)
                    for (z = 0; z < depth; ++z)
                        for (y = 0; y < height; ++y)
                            if (y == 0) {
                                SetTile(x, y, z, BlockId.Bedrock);
                            }
                            else if (x == 0 || x == width - 1 || z == 0 || z == depth - 1 || y == 1 ||
                                     y == height - 1) {
                                SetTile(x, y, z, rand.Next(100) == 0 ? BlockId.IronSolid : BlockId.Obsidian);
                            }
                break;

            case "rainbow":
                Random random = useSeed ? new Random(seed) : new Random();
                for (x = 0; x < width; ++x)
                    for (z = 0; z < depth; ++z)
                        for (y = 0; y < height; ++y)
                            if (y == 0 || y == height - 1 || x == 0 || x == width - 1 || z == 0 || z == depth - 1) {
                                SetTile(x, y, z, (BlockId)random.Next(21, 36));
                            }

                break;


            case "hell":
                Random random2 = useSeed ? new Random(seed) : new Random();
                for (x = 0; x < width; ++x)
                    for (z = 0; z < depth; ++z)
                        for (y = 0; y < height; ++y)
                            if (y == 0) {
                                SetTile(x, y, z, BlockId.Bedrock);
                            }
                            else if (x == 0 || x == width - 1 || z == 0 || z == depth - 1 || y == 0 ||
                                     y == height - 1) {
                                SetTile(x, y, z, BlockId.Obsidian);
                            }
                            else if (x == 1 || x == width - 2 || z == 1 || z == depth - 2) {
                                if (random2.Next(1000) == 7) {
                                    for (int i = 1; i < (height - y); ++i) {
                                        SetTile(x, (ushort)(height - i), z, BlockId.ActiveLava);
                                    }
                                }
                            }
                // TODO: document this later
                _server.MapGen.GenerateMap(this, type, seed, useSeed);
                break;
            case "island":
            case "mountains":
            case "ocean":
            case "forest":
            case "desert":
                _server.MapGen.GenerateMap(this, type, seed, useSeed);
                break;

                // If the constructor was called from Load(), the type would be "empty", which
                // would not match any of the previous cases. This leaves all the block values in the array
                // as zero, which allows Load() to work from a clean slate and populate the array with
                // the values from the file.
            }

            // Set the default spawn location.
            spawnx = (ushort)(width / 2);
            spawny = (ushort)(height * 0.75f);  // The default spawn is always high above the ground.
            spawnz = (ushort)(depth / 2);
            rotx = 0;
            roty = 0;
        }

        /// <summary>
        /// Checks whether a level's name consists of only valid characters.
        /// </summary>
        /// <param name="name"> The name to check. </param>
        /// <returns> Whether the name is valid. </returns>
        public static bool ValidName(string name) {
            // This is similar to the Player equivalent, but also allow the hyphen. Also, allow the at sign
            // and plus sign anywhere in the name, not just in the end. That is done for compatibility reasons.
            // Don't allow the tilde, though.
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890._@+-";
            return StringUtil.ContainsOnlyChars(name, allowedChars);
        }

        /// <summary>
        /// Unloads the level, removing it from memory.
        /// </summary>
        /// <param name="silent"> Whether to not tell ops about the level being unloaded. Defaults to false. </param>
        /// <returns> Whether the level was unloaded successfully. </returns>
        public bool Unload(bool silent = false) {
            // Never unload the main level.
            if (_server.mainLevel == this) {
                return false;
            }

            // Never unload museum levels since they are never added to the server's list of loaded levels.
            if (name.Contains("&cMuseum ")) {
                return false;
            }

            // Send everyone in the level that is being unloaded to the main level.
            _server.players.ForEach(delegate(Player pl) {
                if (pl.level == this) {
                    _server.commands.FindCommand("goto").Use(pl, _server.mainLevel.name);
                }
            });

            // Save the level's data.
            if (changed) {
                Save();
                saveChanges();
            }

            // Remove the level from the server's list of loaded levels.
            _server.levels.Remove(this);

            // Stop the physics thread, if possible, and then dispose the Level object.
            try {
                physic.physThread.Abort();
                physic.physThread.Join();
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
            finally {
                Dispose();

                if (!silent) {
                    _server.GlobalMessageOps("&3" + name + _server.props.DefaultColor + " was unloaded.");
                }
                _logger.Log(string.Format("{0} was unloaded.", name));
            }
            return true;
        }

        /// <summary>
        /// Disposes the Level object.
        /// </summary>
        public void Dispose() {
            physic.liquids.Clear();
            physic.leaves.Clear();
            ListCheck.Clear();
            ListUpdate.Clear();
            UndoBuffer.Clear();
            blockCache.Clear();
            ZoneList.Clear();
            blockqueue.Clear();
            blocks = null;
        }

        /// <summary>
        /// Saves all block changes to the corresponding SQL table.
        /// </summary>
        public void saveChanges() {
            if (blockCache.Count == 0) {
                return;
            }

            // Copy the current cache over to a temporary variable and then empty the current cache
            // in order to reduce the probability of a race condition.
            List<BlockPos> tempCache = blockCache;
            blockCache = new List<BlockPos>();

            // Write all the changes to the database.
            string template = "INSERT INTO Block" + name +
                              " (Username, TimePerformed, X, Y, Z, type, deleted) VALUES" +
                              " (@name, @time, @x, @y, @z, @type, @deleted)";

            using (PreparedStatement stmt = _db.MakePreparedStatement(template)) {
                stmt.AddMultipleParams("@name", "@time", "@x", "@y", "@z", "@type", "@deleted");

                using (TransactionHelper trans = stmt.BeginTransaction()) {
                    foreach (BlockPos bP in tempCache) {
                        stmt["@name"] = bP.name;
                        stmt["@time"] = bP.TimePerformed.ToString("yyyy-MM-dd HH:mm:ss");
                        stmt["@x"] = (int)bP.x;
                        stmt["@y"] = (int)bP.y;
                        stmt["@z"] = (int)bP.z;
                        stmt["@type"] = Convert.ToInt32(bP.type);
                        stmt["@deleted"] = bP.deleted ? 1 : 0;

                        stmt.Execute();
                    }
                    trans.CommitOrRollback();
                }
            }

            tempCache.Clear();
        }

        /// <summary>
        /// Gets the current block type located at the given coordinates.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <returns> The id of the block type at those coordinates. <seealso cref="BlockId"/></returns>
        public BlockId GetTile(ushort x, ushort y, ushort z) {
            if (blocks == null) {
                return BlockId.Null;
            }

            return !InBound(x, y, z) ? BlockId.Null : blocks[PosToInt(x, y, z)];
        }

        /// <summary>
        /// Gets the current block type located at the given index in the block array.
        /// </summary>
        /// <param name="b"> An index within the block array. </param>
        /// <returns> The id of the block type at the given position in the array. <seealso cref="BlockId"/></returns>
        public BlockId GetTile(int b) {
            ushort x = 0, y = 0, z = 0;
            IntToPos(b, out x, out y, out z);
            return GetTile(x, y, z);
        }

        /// <summary>
        /// Sets the block type at the given index in the block array.
        /// </summary>
        /// <param name="b"> An index within the block array. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        public void SetTile(int b, BlockId type) {
            if (blocks == null) {
                return;
            }

            // Check for indices that are out of bounds.
            if (b >= blocks.Length) {
                return;
            }
            if (b < 0) {
                return;
            }

            // Change the block at that position in the array.
            blocks[b] = type;
        }

        /// <summary>
        /// Sets the block type at the given block coordinates.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        public void SetTile(ushort x, ushort y, ushort z, BlockId type) {
            if (blocks == null) {
                return;
            }
            if (!InBound(x, y, z)) {
                return;
            }
            blocks[PosToInt(x, y, z)] = type;
        }

        /// <summary>
        /// Checks if the given coordinates are within the level's boundaries.
        /// </summary>
        /// <param name="x"> The x coordinate. </param>
        /// <param name="y"> The y coordinate. </param>
        /// <param name="z"> The z coordinate. </param>
        /// <returns> Whether the coordinates are within the level's boundaries. </returns>
        public bool InBound(ushort x, ushort y, ushort z) {
            return x >= 0 && y >= 0 && z >= 0 && x < width && y < height && z < depth;
        }

        /// <summary>
        /// Carries out a block change caused directly by a player placing or breaking a block.
        /// </summary>
        /// <param name="p"> The player that affected the block. <seealso cref="Player"/></param>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        /// <param name="addaction"> Not used. </param>
        public void Blockchange(Player p, ushort x, ushort y, ushort z, BlockId type, bool addaction = true) {
            string errorLocation = "start";
            try {
                // Stop if the provided coordinates are outside the level.
                if (x < 0 || y < 0 || z < 0) {
                    return;
                }
                if (x >= width || y >= height || z >= depth) {
                    return;
                }

                // Get the block that is currently there.
                BlockId b = GetTile(x, y, z);

                errorLocation = "Block rank checking";

                // If the block is a portal, mb, door, or other special block, this condition is false.
                if (!BlockData.AllowBreak(b)) {
                    // If the player's rank is high enough or the block being replaced is a non-op
                    // liquid, this condition is false.
                    if (!_server.blockPerms.CanPlace(p, b) && !BlockData.BuildIn(b)) {
                        p.SendBlockchange(x, y, z, b);  // Resend the original block.
                        return;
                    }
                }

                errorLocation = "Zone checking";

                #region zones

                // AllowBuild = if the player is allowed to build
                // foundDel = if a zone should be deleted
                // inZone = if the block to be changed is inside a zone
                bool AllowBuild = true, foundDel = false, inZone = false;
                string Owners = String.Empty;
                var toDel = new List<Zone>();

                // Those that are using /zone or have a permission value of less than 100 are subject
                // to zones unless they are interacting with portals, message blocks, or doors.
                if ((p.rank.Permission < DefaultRankValue.Admin || p.ZoneCheck || p.zoneDel) && !BlockData.AllowBreak(b)) {
                    if (ZoneList.Count == 0) {
                        AllowBuild = true;
                    }
                    else {
                        // Iterate through the list of zones on the level.
                        for (int index = 0; index < ZoneList.Count; index++) {
                            Zone Zn = ZoneList[index];

                            // Check if the block is located within the zone.
                            if (Zn.smallX <= x && x <= Zn.bigX && Zn.smallY <= y && y <= Zn.bigY && Zn.smallZ <= z &&
                                    z <= Zn.bigZ) {
                                inZone = true;

                                // If "/zone del" is being used, delete the zone.
                                if (p.zoneDel) {
                                    // FIXME: PreparedStatement
                                    _db.ExecuteStatement("DELETE FROM `Zone" + p.level.name + "` WHERE Owner='" +
                                                          Zn.owner + "' AND SmallX='" + Zn.smallX.ToString() + "' AND SMALLY='" +
                                                          Zn.smallY.ToString() + "' AND SMALLZ='" + Zn.smallZ.ToString() + "' AND BIGX='" +
                                                          Zn.bigX.ToString() + "' AND BIGY='" + Zn.bigY.ToString() + "' AND BIGZ='" + Zn.bigZ.ToString() +
                                                          "'");
                                    toDel.Add(Zn);

                                    p.SendBlockchange(x, y, z, b);
                                    p.SendMessage("Zone deleted for &b" + Zn.owner);
                                    foundDel = true;
                                }
                                else {
                                    // If the zone belongs to a rank, check if the player has the minimum
                                    // rank required to build in the zone, and either allow or deny building
                                    // in that zone. Also, don't actually place the block if the player is
                                    // simply checking for zones.
                                    if (Zn.owner.Substring(0, 3) == "grp") {
                                        if (_server.ranks.Find(Zn.owner.Substring(3)).Permission <= p.rank.Permission &&
                                                !p.ZoneCheck) {
                                            AllowBuild = true;
                                            break;
                                        }
                                        AllowBuild = false;
                                        Owners += ", " + Zn.owner.Substring(3);
                                    }
                                    // If the zone belongs to a specific player, check if the player that is
                                    // building has the same name of the player that owns the zone. Also, don't 
                                    // actually place the block if the player is simply checking for zones.
                                    else {
                                        if (Zn.owner.ToLower() == p.name.ToLower() && !p.ZoneCheck) {
                                            AllowBuild = true;
                                            break;
                                        }
                                        AllowBuild = false;
                                        Owners += ", " + Zn.owner;
                                    }
                                }
                            }
                        }
                    }

                    // Delete all zones that include the block being changed if "/zone del" is being used.
                    if (p.zoneDel) {
                        if (!foundDel) {
                            p.SendMessage("No zones found to delete.");
                        }
                        else {
                            foreach (Zone Zn in toDel) {
                                ZoneList.Remove(Zn);
                            }
                        }
                        p.zoneDel = false;
                        return;
                    }

                    // Those that are not allowed to build in the zone or are currently checking zones are
                    // informed of who owns the zone.
                    if (!AllowBuild || p.ZoneCheck) {
                        if (Owners != String.Empty) {
                            p.SendMessage("This zone belongs to &b" + Owners.Remove(0, 2) + ".");
                        }
                        else {
                            p.SendMessage("This zone belongs to no one.");
                        }

                        // Don't spam these messages to the player too often.
                        p.ZoneSpam = DateTime.Now;
                        // The block was not actually placed on the server, so resend the original block.
                        p.SendBlockchange(x, y, z, b);

                        // If the player used /static with /zone, do not toggle off zone checking.
                        if (p.ZoneCheck) if (!p.staticCommands) {
                                p.ZoneCheck = false;
                            }
                        return;
                    }
                }

                #endregion

                errorLocation = "Map rank checking";
                // Don't allow building if the player has too low of a rank and is not in a zone that they
                // are not allowed to build in.
                if (Owners == String.Empty) {
                    if (p.rank.Permission < permissionbuild && (!inZone || !AllowBuild)) {
                        p.SendBlockchange(x, y, z, b);
                        p.SendMessage("Must be at least " + _server.ranks.PermToName(permissionbuild) + " to build here");
                        return;
                    }
                }

                // Conversely, don't allow building if the player has too high of a rank and is not in a zone that they
                // are not allowed to build in. Those that can use /perbuildmax, however, are exempted from this
                // check since they would be able to change the setting anyway.
                errorLocation = "Map Max Rank Checking";
                if (Owners == String.Empty) {
                    if (p.rank.Permission > perbuildmax && (!inZone || !AllowBuild)) {
                        if (!_server.commands.CanExecute(p, "perbuildmax")) {
                            p.SendBlockchange(x, y, z, b);
                            p.SendMessage("Your rank must be " + perbuildmax + " or lower to build here!");
                            return;
                        }
                    }
                }

                errorLocation = "Block sending";
                // If the new block does not have the same appearance as the old one and instant mode is not on,
                // send the block to everyone else.
                if (BlockData.Convert(b) != BlockData.Convert(type) && !Instant) {
                    _server.GlobalBlockchange(this, x, y, z, type);
                }

                // Handle any physics caused by removal of sponges.
                if (b == BlockId.Sponge && physics > 0 && type != BlockId.Sponge) {
                    physic.PhysSpongeRemoved(this, PosToInt(x, y, z));
                }
                if (b == BlockId.LavaSponge && physics > 0 && type != BlockId.LavaSponge) {
                    physic.PhysSpongeRemoved(p.level, PosToInt(x, y, z), true);
                }

                // Add an entry to the undo buffer.
                errorLocation = "Undo buffer filling";
                Player.UndoPos Pos;
                Pos.x = x;
                Pos.y = y;
                Pos.z = z;
                Pos.mapName = name;
                Pos.type = b;
                Pos.newtype = type;
                Pos.timePlaced = DateTime.Now;
                p.UndoBuffer.Add(Pos);

                // Change the block itself and increment the player's block count.
                errorLocation = "Setting tile";
                p.loginBlocks++;
                p.overallBlocks++;
                SetTile(x, y, z, type); //Updates server level blocks

                errorLocation = "Growing grass";
                if (GetTile(x, (ushort)(y - 1), z) == BlockId.Grass && GrassDestroy && !BlockData.LightPass(type)) {
                    Blockchange(p, x, (ushort)(y - 1), z, BlockId.Dirt);
                }

                errorLocation = "Adding physics";
                // Check if the block being placed can affect physics.
                if (physics > 0) if (BlockData.Physics(type)) {
                        AddCheck(PosToInt(x, y, z), String.Empty, false, p);
                    }

                // Mark that the level has been changed and that the most recent backup does not reflect this change.
                changed = true;
                backedup = false;
            }
            catch (OutOfMemoryException e) {
                Environment.FailFast("Out of memory in Level.Blockchange()", e);
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                _server.GlobalMessageOps(p.name + " triggered a non-fatal error on " + name);
                _server.GlobalMessageOps("Error location: " + errorLocation);
                _logger.Log(p.name + " triggered a non-fatal error on " + name);
                _logger.Log("Error location: " + errorLocation);
            }
        }

        /// <summary>
        /// Saves the level's properties.
        /// </summary>
        public void SaveSettings() {
            try {
                string propsPath = Path.Combine("levels", "level properties", this.name + ".properties");

                File.Create(propsPath).Dispose();
                using (StreamWriter SW = File.CreateText(propsPath)) {
                    SW.WriteLine("#Level properties for " + this.name);
                    SW.WriteLine("#Drown-time in seconds is [drown time] * 200 / 3 / 1000");
                    SW.WriteLine("Physics = " + this.physics.ToString());
                    SW.WriteLine("Physics speed = " + this.speedphysics.ToString());
                    SW.WriteLine("Physics overload = " + this.overload.ToString());
                    SW.WriteLine("Finite mode = " + this.finite.ToString());
                    SW.WriteLine("Animal AI = " + this.ai.ToString());
                    SW.WriteLine("Edge water = " + this.edgeWater.ToString());
                    SW.WriteLine("Survival death = " + this.Death.ToString());
                    SW.WriteLine("Fall = " + this.fall.ToString());
                    SW.WriteLine("Drown = " + this.drown.ToString());
                    SW.WriteLine("MOTD = " + this.motd);
                    SW.WriteLine("JailX = " + this.jailx.ToString());
                    SW.WriteLine("JailY = " + this.jaily.ToString());
                    SW.WriteLine("JailZ = " + this.jailz.ToString());
                    SW.WriteLine("Unload = " + this.unload.ToString());
                    SW.WriteLine("WorldChat = " + this.worldChat.ToString());
                    SW.WriteLine("PerBuild = " +
                                 (_server.ranks.Exists(_server.ranks.PermToName(this.permissionbuild).ToLower())
                                  ? _server.ranks.PermToName(this.permissionbuild).ToLower()
                                  : _server.ranks.PermToName(DefaultRankValue.Guest)));
                    SW.WriteLine("PerVisit = " +
                                 (_server.ranks.Exists(_server.ranks.PermToName(this.permissionvisit).ToLower())
                                  ? _server.ranks.PermToName(this.permissionvisit).ToLower()
                                  : _server.ranks.PermToName(DefaultRankValue.Guest)));
                    SW.WriteLine("PerBuildMax = " +
                                 (_server.ranks.Exists(_server.ranks.PermToName(this.perbuildmax).ToLower())
                                  ? _server.ranks.PermToName(this.perbuildmax).ToLower()
                                  : _server.ranks.PermToName(DefaultRankValue.Nobody)));
                    SW.WriteLine("PerVisitMax = " +
                                 (_server.ranks.Exists(_server.ranks.PermToName(this.pervisitmax).ToLower())
                                  ? _server.ranks.PermToName(this.pervisitmax).ToLower()
                                  : _server.ranks.PermToName(DefaultRankValue.Nobody)));
                    SW.WriteLine("Guns = " + this.guns.ToString());
                    SW.WriteLine("LoadOnGoto = " + this.loadOnGoto.ToString());
                    SW.WriteLine("LeafDecay = " + this.leafDecay.ToString());
                    SW.WriteLine("RandomFlow = " + this.randomFlow.ToString());
                    SW.WriteLine("GrowTrees = " + this.growTrees.ToString());
                    SW.WriteLine("weather = " + this.weather.ToString());
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                _logger.Log("Failed to save level properties!");
            }
        }

        /// <summary>
        /// Carries out a block change caused by physics.
        /// </summary>
        /// <param name="b"> The index of the block in the block array. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        /// <param name="overRide"> Whether the block change should be carried out even for blocks
        /// that are usually reserved for operators. </param>
        /// <param name="extraInfo"> A string that contains extra physics-related information. </param>
        public void Blockchange(int b, BlockId type, bool overRide = false, string extraInfo = "")
        {
            // Do some bounds checking.
            if (b < 0) {
                return;
            }
            if (b >= blocks.Length) {
                return;
            }

            // Get the current block type at that position of the array.
            BlockId bb = GetTile(b);

            try {
                // In the normal case, do not do anything if the original block is an operator-only
                // block or if the new block is an operator-only block with extra physics-related data.
                if (!overRide)
                    if (BlockData.OPBlocks(bb) || (BlockData.OPBlocks(type) && extraInfo != String.Empty)) {
                        return;
                    }

                // Save bandwidth if the new block has the same appearance as the old one. Otherwise,
                // send the block change to everyone in the level.
                if (BlockData.Convert(bb) != BlockData.Convert(type))
                {
                    _server.GlobalBlockchange(this, b, type);
                }

                // Handle removal of sponges if physics is on.
                // FIXME: bb should be used, not b
                if (b == Convert.ToInt32(BlockId.Sponge) && physics > 0 && type != BlockId.Sponge) {
                    physic.PhysSpongeRemoved(this, b);
                }
                if (b == Convert.ToInt32(BlockId.LavaSponge) && physics > 0 && type != BlockId.LavaSponge) {
                    physic.PhysSpongeRemoved(this, b, true);
                }

                // Add the block change to the level's undo buffer.
                try {
                    UndoPos uP;
                    uP.location = b;
                    uP.newType = type;
                    uP.oldType = bb;
                    uP.timePerformed = DateTime.Now;

                    // Wrap around if the server's physics undo limit is reached.
                    // TODO: this probably causes problems
                    if (currentUndo > _server.props.physUndo - 1) {
                        currentUndo = 0;
                        UndoBuffer[currentUndo] = uP;
                        currentUndo = 1;
                    }
                    // If the undo buffer has not reached the limit yet, append to the buffer.
                    else if (UndoBuffer.Count < _server.props.physUndo) {
                        currentUndo++;
                        UndoBuffer.Add(uP);
                    }
                    // Otherwise, we must have already wrapped around at least once, so replace what is there instead.
                    else {
                        UndoBuffer[currentUndo] = uP;
                        currentUndo++;
                    }
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }

                // Change the block type on the server's end.
                SetTile(b, type);

                // Handle physics, if necessary.
                if (physics > 0)
                    if (BlockData.Physics(type) || extraInfo != String.Empty) {
                        AddCheck(b, extraInfo);
                    }
            }
            catch (Exception e) {  // If an error occurs, just change the block on the server's end.
                _logger.ErrorLog(e);
                SetTile(b, type);
            }
        }

        /// <summary>
        /// Carries out a block change caused by physics.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        /// <param name="overRide"> Whether the block change should be carried out even for blocks
        /// that are usually reserved for operators. </param>
        /// <param name="extraInfo"> A string that contains extra physics-related information. </param>
        public void Blockchange(ushort x, ushort y, ushort z, BlockId type, bool overRide = false, string extraInfo = "")
        {
            // Do some bounds checking.
            if (x < 0 || y < 0 || z < 0) {
                return;
            }
            if (x >= width || y >= height || z >= depth) {
                return;
            }

            // Get the current block type at those coordinates.
            BlockId b = GetTile(x, y, z);

            try {
                // In the normal case, do not do anything if the original block is an operator-only
                // block or if the new block is an operator-only block with extra physics-related data.
                if (!overRide)
                    if (BlockData.OPBlocks(b) || (BlockData.OPBlocks(type) && extraInfo != String.Empty)) {
                        return;
                    }

                // Save bandwidth if the new block has the same appearance as the old one. Otherwise,
                // send the block change to everyone in the level.
                if (BlockData.Convert(b) != BlockData.Convert(type))
                {
                    _server.GlobalBlockchange(this, x, y, z, type);
                }

                // Handle removal of sponges if physics is on.
                if (b == BlockId.Sponge && physics > 0 && type != BlockId.Sponge) {
                    physic.PhysSpongeRemoved(this, PosToInt(x, y, z));
                }
                if (b == BlockId.LavaSponge && physics > 0 && type != BlockId.LavaSponge) {
                    physic.PhysSpongeRemoved(this, PosToInt(x, y, z), true);
                }

                // Add the block change to the level's undo buffer.
                try {
                    UndoPos uP;
                    uP.location = PosToInt(x, y, z);
                    uP.newType = type;
                    uP.oldType = b;
                    uP.timePerformed = DateTime.Now;

                    // Wrap around if the server's physics undo limit is reached.
                    // TODO: this probably causes problems
                    if (currentUndo > _server.props.physUndo - 1) {
                        currentUndo = 0;
                        UndoBuffer[currentUndo] = uP;
                        currentUndo = 1;
                    }
                    // If the undo buffer has not reached the limit yet, append to the buffer.
                    else if (UndoBuffer.Count < _server.props.physUndo) {
                        currentUndo++;
                        UndoBuffer.Add(uP);
                    }
                    // Otherwise, we must have already wrapped around at least once, so replace what is there instead.
                    else {
                        UndoBuffer[currentUndo] = uP;
                        currentUndo++;
                    }
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }

                // Change the block type on the server's end.
                SetTile(x, y, z, type);

                // Handle physics, if necessary.
                if (physics > 0)
                    if (BlockData.Physics(type) || extraInfo != String.Empty) {
                        AddCheck(PosToInt(x, y, z), extraInfo);
                    }
            }
            catch (Exception e) {  // If an error occurs, just change the block on the server's end.
                _logger.ErrorLog(e);
                SetTile(x, y, z, type);
            }
        }

        /// <summary>
        /// Checks if a physics check does not exist for the block at the given location.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <returns> Whether a physics check does not exist for that block. </returns>
        public bool CheckClear(ushort x, ushort y, ushort z) {
            int b = PosToInt(x, y, z);
            return !ListCheck.Exists(Check => Check.b == b);
        }

        /// <summary>
        /// Changes a block at the given coordinates on the server's side only.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        public void skipChange(ushort x, ushort y, ushort z, BlockId type) {
            if (x < 0 || y < 0 || z < 0) {
                return;
            }
            if (x >= width || y >= height || z >= depth) {
                return;
            }

            SetTile(x, y, z, type);
        }

        /// <summary>
        /// Saves the level.
        /// </summary>
        /// <param name="Override"> Whether to save even if the level is unchanged. Defaults to false. </param>
        /// <param name="clearPhysics"> Whether to clear the physics state before saving. Defaults to true. </param>
        public void Save(bool Override = false, bool clearPhysics = false) {
            if (blocks == null) {
                return;
            }
            string path = Path.Combine("levels", name + ".mcf");

            try {
                // Create needed directories if they do not exist.
                if (!Directory.Exists("levels")) {
                    Directory.CreateDirectory("levels");
                }
                if (!Directory.Exists(Path.Combine("levels", "level properties"))) {
                    Directory.CreateDirectory(Path.Combine("levels", "level properties"));
                }

                // Proceed with saving if the level has changed in any way or if the save was
                // requested by the user.
                if (changed || !File.Exists(path) || Override || (physicschanged && clearPhysics)) {
                    // Clear the physics state if requested.
                    if (clearPhysics) {
                        physic.ClearPhysics(this);
                    }

                    // The .back file is a temporary file used while saving. It does not write
                    // directly to the .mcf file at first in case the save operation fails.
                    string backFile = string.Format("{0}.back", path);
                    string backupFile = string.Format("{0}.backup", path);

                    using (FileStream fs = File.OpenWrite(backFile)) {
                        using (GZipStream gs = new GZipStream(fs, CompressionMode.Compress)) {
                            // The version number for .mcf files is 1874.
                            var header = new byte[16];
                            BitConverter.GetBytes(1874).CopyTo(header, 0);
                            gs.Write(header, 0, 2);

                            // Write the dimensions of the level to the header array.
                            BitConverter.GetBytes(width).CopyTo(header, 0);
                            BitConverter.GetBytes(depth).CopyTo(header, 2);
                            BitConverter.GetBytes(height).CopyTo(header, 4);

                            // Why is this in the middle of nowhere? - jjp137
                            changed = false;

                            // Write the location of the spawn point to the header array.
                            BitConverter.GetBytes(spawnx).CopyTo(header, 6);
                            BitConverter.GetBytes(spawnz).CopyTo(header, 8);
                            BitConverter.GetBytes(spawny).CopyTo(header, 10);
                            header[12] = rotx;
                            header[13] = roty;

                            // Write the pervisit and perbuild values to the header array, even though
                            // they are unused.
                            header[14] = (byte)permissionvisit;
                            header[15] = (byte)permissionbuild;

                            // Write the header to the file.
                            gs.Write(header, 0, header.Length);

                            // Save 1024 blocks at a time. We don't want to copy the whole array since that
                            // array is rather huge, so just write 1024 blocks to the file.
                            const int blocksWritten = 1024;
                            byte[] buffer = new byte[blocksWritten * 2]; // Each block is two bytes in .mcf format.

                            for (int i = 0; i < blocks.Length; i += blocksWritten) {
                                for (int j = 0; j < blocksWritten; j++) {
                                    BlockId blockId = blocks[i + j]; // 0 to 1023, 1024 to 2047, ...
                                    ushort blockVal = 0;

                                    // BlockId.air is 0, and the array is initialized to all zeroes, so do not update the
                                    // value in the array if it is already 0.
                                    if (Convert.ToInt32(blockId) < 57) { // Why 57? - Jjp137
                                        if (blockId != BlockId.Air) {
                                            blockVal = (ushort)blockId;
                                        }
                                    }
                                    else {
                                        // Some blocks, such as door_air blocks, are converted to their
                                        // inactive form before they are saved.
                                        if (BlockData.SaveConvert(blockId) != BlockId.Air) {
                                            blockVal = (ushort)BlockData.SaveConvert(blockId);
                                        }
                                    }

                                    // The high byte of the unsigned short is stored after the low byte.
                                    buffer[j * 2] = (byte)blockVal;
                                    buffer[j * 2 + 1] = (byte)(blockVal >> 8);
                                }
                                // Every 1024 blocks, write the buffer to the file.
                                gs.Write(buffer, 0, buffer.Length);
                            }
                        }
                    }

                    // Safely replace the original file (if it exists) after making a backup.
                    if (File.Exists(path)) {
                        File.Delete(backupFile);
                        File.Replace(backFile, path, backupFile);
                    }
                    else {
                        File.Move(backFile, path);
                    }

                    // Save the level's settings.
                    SaveSettings();

                    _logger.Log(string.Format("SAVED: Level \"{0}\". ({1}/{2}/{3})", name, players.Count,
                                               _server.players.Count, _server.props.maxPlayers));

                }
                // If the level is unchanged, do not do anything.
                else {
                    _logger.Log("Skipping level save for " + name + ".");
                }
            }
            // Oh dear... - Jjp137
            catch (OutOfMemoryException e) {
                Environment.FailFast("Out of memory in Level.Save()", e);
            }
            catch (Exception e) {
                _logger.Log("FAILED TO SAVE :" + name);
                _server.GlobalMessage("FAILED TO SAVE :" + name);

                _logger.ErrorLog(e);
                return;
            }
        }

        /// <summary>
        /// Backup the level.
        /// </summary>
        /// <param name="Forced"> Whether the backup operation should occur even if the level has not
        /// changed yet. Defaults to false.  </param>
        /// <param name="backupName"> The name of the backup. Optional. </param>
        /// <returns> The backup number, or -1 if a backup was not made. </returns>
        public int Backup(bool Forced = false, string backupName = "") {
            // Perform a backup if the level has changed or if it is explicitly requested.
            if (!backedup || Forced) {
                int backupNumber = 1;
                string backupLoc = @_server.props.backupLocation;
                string levelDir = Path.Combine(backupLoc, name);

                // If the directory containing the level's backups already exists, then the backup number
                // is the number of folders already in that directory. (TODO: this is a bug so fix it)
                // Otherise, the backup number starts at 1.
                if (Directory.Exists(levelDir)) {
                    backupNumber = Directory.GetDirectories(levelDir).Length + 1;
                }
                else {
                    Directory.CreateDirectory(levelDir);
                }

                // Create the folder containing the backup to be saved.
                string backupDirPath = Path.Combine(levelDir, backupNumber.ToString());
                if (backupName != String.Empty) {
                    backupDirPath = Path.Combine(levelDir, backupName);
                }
                Directory.CreateDirectory(backupDirPath);

                // Save the backup's data and mark that the level has been backed up.
                string backupMcfPath = Path.Combine(backupDirPath, name + ".mcf");
                string currentLevelPath = Path.Combine("levels", name + ".mcf");
                try {
                    File.Copy(currentLevelPath, backupMcfPath, true);
                    BackupSQLData(backupDirPath);
                    backedup = true;
                    return backupNumber;
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                    _logger.Log(string.Format("FAILED TO INCREMENTAL BACKUP :{0}", name));
                    return -1;
                }
            }
            // Otherwise, skip the backup operation.
            _logger.Log("Level unchanged, skipping backup");
            return -1;
        }

        /// <summary>
        /// Backs up most SQL data for a given map, including portals, mbs, and zones, but not
        /// block changes.
        /// </summary>
        /// <param name="backupPath"> Path to the folder that holds a particular backup. </param>
        public void BackupSQLData(string backupPath) {
            // TODO: include zones
            try {
                // Workaround for /mb show and /portal show.
                DeleteUnusedSQLRows();

                // First, handle the portals.
                using (StreamWriter fout = new StreamWriter(File.Create(Path.Combine(backupPath, "portals.txt")))) {
                    // Get the portal data from the SQL database.
                    using (DataTable portals = _db.ObtainData("SELECT * FROM Portals" + name)) {
                        // Write each portal as a line.
                        foreach (DataRow portal in portals.Rows) {
                            string entryX = portal["EntryX"].ToString();
                            string entryY = portal["EntryY"].ToString();
                            string entryZ = portal["EntryZ"].ToString();
                            string exitMap = portal["ExitMap"].ToString();
                            string exitX = portal["ExitX"].ToString();
                            string exitY = portal["ExitY"].ToString();
                            string exitZ = portal["ExitZ"].ToString();

                            // The | is the separator to split by later.
                            fout.WriteLine(entryX + "|" + entryY + "|" + entryZ + "|" +
                                           exitMap + "|" + exitX + "|" + exitY + "|" + exitZ);
                        }
                        fout.Close();
                    }
                }

                // Then, handle the message blocks.
                using (StreamWriter fout = new StreamWriter(File.Create(Path.Combine(backupPath, "mbs.txt")))) {
                    // Get the message block data from the SQL database.
                    using (DataTable mbs = _db.ObtainData("SELECT * FROM Messages" + name)) {
                        // Write each message block as a line.
                        foreach (DataRow mb in mbs.Rows) {
                            string x = mb["X"].ToString();
                            string y = mb["Y"].ToString();
                            string z = mb["Z"].ToString();
                            string message = mb["Message"].ToString();

                            // The | is the separator to split by later.
                            fout.WriteLine(x + "|" + y + "|" + z + "|" + message);
                        }
                        fout.Close();
                    }
                }
            }
            catch (IOException e) {
                _logger.Log("Warning: a file is in use! Not continuing.");
                _logger.ErrorLog(e);
            }
            catch (UnauthorizedAccessException e) {
                _logger.Log("Warning: a file can't be read due to insufficent permissions! Not continuing.");
                _logger.ErrorLog(e);
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Restores most SQL data from a backup, including portals, mbs, and zones, but not
        /// block change data, from the text files in the backup's folder.
        /// </summary>
        /// <remarks>
        /// This is only done if portals.txt, mbs.txt, and/or zones.txt exists. As other
        /// software don't really backup SQL data with each backup, this code is only relevant
        /// for backups that MCHmk r4+ makes.
        /// </remarks>
        /// <param name="backupPath"> Path to the folder that holds a particular backup. </param>
        public void RestoreSQLData(string backupPath) {
            // TODO: restore zones
            try {
                // Handle portals first.
                if (File.Exists(Path.Combine(backupPath, "portals.txt"))) {
                    // Make a clean slate first.
                    _db.ExecuteStatement("DELETE FROM Portals" + name);

                    // Add each portal back by reading each line. The | is the delimiter.
                    string[] lines = File.ReadAllLines(Path.Combine(backupPath, "portals.txt"));
                    foreach (string line in lines) {
                        if (!String.IsNullOrEmpty(line)) {
                            string[] data = line.Split('|');

                            // FIXME: PreparedStatement
                            // The data in the array is as follows:
                            // EntryX, EntryY, EntryZ, ExitMap, ExitX, ExitY, ExitZ
                            _db.ExecuteStatement("INSERT INTO Portals" + name + " VALUES " +
                                                  "(" + data[0] + ", " + data[1] + ", " +  data[2] + ", '" +
                                                  data[3] + "', " + data[4] + ", " + data[5] + ", " + data[6] + ")");
                        }
                    }
                }

                // Handle message boxes next.
                if (File.Exists(Path.Combine(backupPath, "mbs.txt"))) {
                    // Make a clean slate as well.
                    _db.ExecuteStatement("DELETE FROM Messages" + name);

                    // Add each message box back by reading each line.
                    string[] lines = File.ReadAllLines(Path.Combine(backupPath, "mbs.txt"));
                    foreach (string line in lines) {
                        if (!String.IsNullOrEmpty(line)) {
                            // Only return 4 substrings b/c the message itself may have a | in it.
                            string[] data = line.Split(new char[] {'|'}, 4);

                            // Escape single quotes.
                            data[3] = data[3].Replace("'", _server.props.useMySQL ? "\\'" : "''");

                            // FIXME: PreparedStatement
                            // The data in the array is as follows:
                            // X, Y, Z, Message
                            _db.ExecuteStatement("INSERT INTO Messages" + name + " VALUES " +
                                                  "(" + data[0] + ", " + data[1] + ", " +data[2] + ", '"
                                                  + data[3] + "')");
                        }
                    }
                }

                // Workaround for /mb show and /portal show.
                DeleteUnusedSQLRows();
            }
            catch (IOException e) {
                _logger.Log("Warning: a file is in use! Not continuing.");
                _logger.ErrorLog(e);
            }
            catch (UnauthorizedAccessException e) {
                _logger.Log("Warning: a file can't be read due to insufficent permissions! Not continuing.");
                _logger.ErrorLog(e);
            }
            // The source code is so bad that some random exception might happen -Jjp137
            catch (Exception e) {
                _logger.Log("This shouldn't happen...");
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Temporary function to delete unused portals/mbs until a proper solution is made.
        /// </summary>
        public void DeleteUnusedSQLRows() {
            // This will be left in for backwards compatibility with old backups -Jjp137
            // And this is also actually useful for working around the inability for
            // the /portal show and /mb show commands to update right after a portal or
            // mb is fixed...for now.
            //
            // The below code removes all portal and mbs that do not have an actual
            // block associated with them.
            try {
                using (DataTable portals = _db.ObtainData("SELECT * FROM `Portals" + name + "`")) {
                    foreach (DataRow portal in portals.Rows) {
                        if (!BlockData.portal(GetTile(ushort.Parse(portal["EntryX"].ToString()),
                                                      ushort.Parse(portal["EntryY"].ToString()),
                                                      ushort.Parse(portal["EntryZ"].ToString())))) {
                            // FIXME: PreparedStatement
                            _db.ExecuteStatement("DELETE FROM `Portals" + name + "` WHERE EntryX=" +
                                                  portal["EntryX"] + " AND EntryY=" +
                                                  portal["EntryY"] + " AND EntryZ=" +
                                                  portal["EntryZ"]);
                        }
                    }
                }

                using (DataTable mbs = _db.ObtainData("SELECT * FROM `Messages" + name + "`")) {
                    foreach (DataRow mb in mbs.Rows) {
                        if (!BlockData.mb(GetTile(ushort.Parse(mb["X"].ToString()),
                                              ushort.Parse(mb["Y"].ToString()),
                                              ushort.Parse(mb["Z"].ToString())))) {
                            // FIXME: PreparedStatement
                            _db.ExecuteStatement("DELETE FROM `Messages" + name + "` WHERE X=" +
                                                  mb["X"] + " AND Y=" + mb["Y"] +
                                                  " AND Z=" + mb["Z"]);
                        }
                    }
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Creates the SQL tables related to the level's portals, mbs, zones, and block history.
        /// </summary>
        /// <param name="givenName"> The name of the level. </param>
        public static void CreateLeveldb(Database db, string givenName) {
            db.ExecuteStatement("CREATE TABLE if not exists `Block" + givenName +
                                "` (Username CHAR(20), TimePerformed DATETIME, X SMALLINT UNSIGNED, Y SMALLINT UNSIGNED, Z SMALLINT UNSIGNED, Type TINYINT UNSIGNED, Deleted INTEGER)");
            db.ExecuteStatement("CREATE TABLE if not exists `Portals" + givenName +
                                "` (EntryX SMALLINT UNSIGNED, EntryY SMALLINT UNSIGNED, EntryZ SMALLINT UNSIGNED, ExitMap CHAR(20), ExitX SMALLINT UNSIGNED, ExitY SMALLINT UNSIGNED, ExitZ SMALLINT UNSIGNED)");
            db.ExecuteStatement("CREATE TABLE if not exists `Messages" + givenName +
                                "` (X SMALLINT UNSIGNED, Y SMALLINT UNSIGNED, Z SMALLINT UNSIGNED, Message CHAR(255));");
            db.ExecuteStatement("CREATE TABLE if not exists `Zone" + givenName +
                                "` (SmallX SMALLINT UNSIGNED, SmallY SMALLINT UNSIGNED, SmallZ SMALLINT UNSIGNED, BigX SMALLINT UNSIGNED, BigY SMALLINT UNSIGNED, BigZ SMALLINT UNSIGNED, Owner VARCHAR(20));");
        }

        /// <summary>
        /// Loads a level with no physics turned on.
        /// </summary>
        /// <param name="svr"> The server loading this level. <seealso cref="Server"/></param> 
        /// <param name="givenName"> The name of the level to load. </param>
        /// <returns> The level that was loaded. <seealso cref="Level"/> </returns>
        // TODO: refactor this static mess and remove that Server parameter
        public static Level Load(Server svr, string givenName) {
            return Load(svr, givenName, 0);
        }

        /// <summary>
        /// Loads a level with the given physics setting.
        /// </summary>
        /// <param name="svr"> The server loading this level. <seealso cref="Server"/></param> 
        /// <param name="givenName"> The name of the level to load. </param>
        /// <param name="phys"> The physics setting to set the level to. </param>
        /// <param name="old_format"> Whether the level to be loaded is using the old .lvl format. 
        /// Defaults to false. </param>
        /// <returns> The level that was loaded. <seealso cref="Level"/></returns>
        // TODO: refactor this static mess and remove that Server parameter
        public static Level Load(Server svr, string givenName, byte phys, bool old_format = false) {
            // Create the level's SQL tables if they do not exist.
            CreateLeveldb(svr.database, givenName);

            // Deciphering the below code reveals the level format, which is as follows:
            //
            // - The first two bytes hold the version number, which is a 16-bit integer.
            // It is not a power of two because level dimensions are in powers of two, so if
            // the version number was, for instance, 256, it would be impossible to tell if that
            // is the version number or a dimension of the map.
            //
            // If the version number is 1874:
            // - The next six bytes are three 16-bit integers indicating the dimensions of the
            // map. They are in the following order: x, z, y.
            // - The next six bytes after that are three 16-bit integers indicating the spawn point
            // of the map, again in x, z, y order.
            // - The next four bytes are two 16-bit integers that depict the direction that the
            // player's head is pointing at when that player goes to that map.
            // - The next four bytes are two 16-bit integers representing the pervisit and perbuild
            // values of the map, but they are unused.
            // - All the bytes after that represents the array of blocks that make up the level.
            // If the file being loaded is a .lvl file, every byte, which is an 8-bit integer
            // represents the block type at that position in the block array. If the file being
            // loaded is a .mcf file, every two bytes, which together represent a 16-bit integer,
            // represent a block type.
            //
            // If the version number is not 1874:
            // - The version number is then assumed to be the x dimension, and everything else
            // follows from there. There are no bytes representing the pervisit or perbuild.
            //
            // One last note: these integers are all unsigned.
            //
            // Knowing this, you can edit levels with a hex editor if you really wanted to :p
            // -Jjp137

            // Old levels are stored in .lvl format.
            string path = string.Format("levels/{0}.{1}", givenName, old_format ? "lvl" : "mcf");

            if (File.Exists(path)) {
                FileStream fs = File.OpenRead(path);
                try {
                    var gs = new GZipStream(fs, CompressionMode.Decompress);
                    var ver = new byte[2];

                    // Read the version number of the file.
                    gs.Read(ver, 0, ver.Length);
                    ushort version = BitConverter.ToUInt16(ver, 0);

                    var vars = new ushort[6];
                    var rot = new byte[2];

                    if (version == 1874) {
                        var header = new byte[16];
                        // The GZipStream continues reading from where it left off.
                        gs.Read(header, 0, header.Length);

                        vars[0] = BitConverter.ToUInt16(header, 0);  // Size in x-dimension
                        vars[1] = BitConverter.ToUInt16(header, 2);  // Size in z-dimension
                        vars[2] = BitConverter.ToUInt16(header, 4);  // Size in y-dimension
                        vars[3] = BitConverter.ToUInt16(header, 6);  // x coordinate of spawn
                        vars[4] = BitConverter.ToUInt16(header, 8);  // y coordinate of spawn
                        vars[5] = BitConverter.ToUInt16(header, 10);  // z coordinate of spawn

                        rot[0] = header[12];  // Rotation of player's head on x
                        rot[1] = header[13];  // Rotation of player's head on y
                    }
                    else {  // For old .lvl files.
                        var header = new byte[12];
                        gs.Read(header, 0, header.Length);

                        vars[0] = version; // Assume that the version number is the size in x
                        vars[1] = BitConverter.ToUInt16(header, 0); // Size in z-dimension
                        vars[2] = BitConverter.ToUInt16(header, 2); // Size in y-dimension
                        vars[3] = BitConverter.ToUInt16(header, 4); // x coordinate of spawn
                        vars[4] = BitConverter.ToUInt16(header, 6); // y coordinate of spawn
                        vars[5] = BitConverter.ToUInt16(header, 8); // z coordinate of spawn

                        rot[0] = header[10]; // Rotation of player's head on x
                        rot[1] = header[11]; // Rotation of player's head on y
                    }

                    // Construct a new level with the information obtained from the header.
                    // Anything in the braces replaces the equivalent assignment statements in
                    // the constructor itself. The "empty" type indicates to the constructor
                    // that the block array should not be touched.
                    var level = new Level(svr, givenName, vars[0], vars[2], vars[1], "empty") {
                        permissionbuild = 30,
                        spawnx = vars[3],
                        spawnz = vars[4],
                        spawny = vars[5],
                        rotx = rot[0],
                        roty = rot[1],
                        name = givenName
                    };

                    // Set the level's physics setting.
                    level.setPhysics(phys);

                    // Create a temporary buffer to hold data for several blocks at a time. We don't want
                    // to just read the whole stream at once and store it in an array because that
                    // array will be huge. The number 1024 is used because that is the number of blocks
                    // sent in a single LevelChunk packet.
                    const int blocksRead = 1024;
                    // .mcf uses two bytes for each block.
                    byte[] tempBlocks = new byte[(old_format ? 1 : 2) * blocksRead];

                    // Read several blocks at a time, and copy the data to the appropriate place in the
                    // level's block array.
                    if (!old_format) { // The new .mcf format.
                        for (int i = 0; i < level.blocks.Length; i += blocksRead) {
                            gs.Read(tempBlocks, 0, tempBlocks.Length);
                            for (int j = 0; j < blocksRead; j++) {
                                // Each block is represented as a 16-bit integer, or two bytes, in .mcf,
                                // so we have to make sure to read both bytes. They are also saved as
                                // little-endian, so the least significant byte comes first. Thus, some
                                // bitwise magic needs to be done to get the proper value.
                                ushort result = (ushort)(tempBlocks[j * 2] | (tempBlocks[j * 2 + 1] << 8));
                                level.blocks[i + j] = (BlockId)result;
                            }
                        }
                    }
                    else { // The old .lvl format.
                        for (int i = 0; i < level.blocks.Length; i += blocksRead) {
                            gs.Read(tempBlocks, 0, tempBlocks.Length);
                            for (int j = 0; j < blocksRead; j++) {
                                // Each block is represented as one byte. It's simple in this case.
                                level.blocks[i + j] = (BlockId)tempBlocks[j];
                            }
                        }
                    }

                    // Close the stream.
                    gs.Close();
                    gs.Dispose();
                    level.backedup = true;

                    // Load all zones from the SQL table.
                    using (DataTable ZoneDB = svr.database.ObtainData("SELECT * FROM `Zone" + givenName + "`")) {
                        Zone Zn;
                        for (int i = 0; i < ZoneDB.Rows.Count; ++i) {
                            Zn.smallX = ushort.Parse(ZoneDB.Rows[i]["SmallX"].ToString());
                            Zn.smallY = ushort.Parse(ZoneDB.Rows[i]["SmallY"].ToString());
                            Zn.smallZ = ushort.Parse(ZoneDB.Rows[i]["SmallZ"].ToString());
                            Zn.bigX = ushort.Parse(ZoneDB.Rows[i]["BigX"].ToString());
                            Zn.bigY = ushort.Parse(ZoneDB.Rows[i]["BigY"].ToString());
                            Zn.bigZ = ushort.Parse(ZoneDB.Rows[i]["BigZ"].ToString());
                            Zn.owner = ZoneDB.Rows[i]["Owner"].ToString();
                            level.ZoneList.Add(Zn);
                        }
                    }

                    // Set the default location of the jail.
                    level.jailx = (ushort)(level.spawnx * 32);  // It is in player coordinates, so multiply by 32.
                    level.jaily = (ushort)(level.spawny * 32);
                    level.jailz = (ushort)(level.spawnz * 32);
                    level.jailrotx = level.rotx;
                    level.jailroty = level.roty;

                    // Start the physics thread.
                    level.physic.StartPhysics(level);

                    // Delete any portals or mbs that don't actually exist in the level.
                    level.DeleteUnusedSQLRows();

                    // Read the level's settings from its properties file.
                    try {
                        // Create the properties file if it does not exist.
                        string foundLocation;
                        foundLocation = "levels/level properties/" + level.name + ".properties";
                        if (!File.Exists(foundLocation)) {
                            foundLocation = "levels/level properties/" + level.name;
                        }

                        foreach (string line in File.ReadAllLines(foundLocation)) {
                            try {
                                if (line[0] == '#') {  // Ignore commented lines.
                                    continue;
                                }
                                // Each setting has a value separated by an equals sign, so do a
                                // split-like operation to separate the two.
                                string value = line.Substring(line.IndexOf(" = ") + 3);

                                switch (line.Substring(0, line.IndexOf(" = ")).ToLower()) {
                                case "physics":
                                    level.setPhysics(int.Parse(value));
                                    break;
                                case "physics speed":
                                    level.speedphysics = int.Parse(value);
                                    break;
                                case "physics overload":
                                    level.overload = int.Parse(value);
                                    break;
                                case "finite mode":
                                    level.finite = bool.Parse(value);
                                    break;
                                case "animal ai":
                                    level.ai = bool.Parse(value);
                                    break;
                                case "edge water":
                                    level.edgeWater = bool.Parse(value);
                                    break;
                                case "survival death":
                                    level.Death = bool.Parse(value);
                                    break;
                                case "fall":
                                    level.fall = int.Parse(value);
                                    break;
                                case "drown":
                                    level.drown = int.Parse(value);
                                    break;
                                case "motd":
                                    level.motd = value;
                                    break;
                                case "jailx":
                                    level.jailx = ushort.Parse(value);
                                    break;
                                case "jaily":
                                    level.jaily = ushort.Parse(value);
                                    break;
                                case "jailz":
                                    level.jailz = ushort.Parse(value);
                                    break;
                                case "unload":
                                    level.unload = bool.Parse(value);
                                    break;
                                case "worldchat":
                                    level.worldChat = bool.Parse(value);
                                    break;
                                case "perbuild":
                                    level.permissionbuild = svr.ranks.PermFromName(value) != DefaultRankValue.Null ? svr.ranks.PermFromName(
                                                                value) : DefaultRankValue.Guest;
                                    break;
                                case "pervisit":
                                    level.permissionvisit = svr.ranks.PermFromName(value) != DefaultRankValue.Null ? svr.ranks.PermFromName(
                                                                value) : DefaultRankValue.Guest;
                                    break;
                                case "perbuildmax":
                                    level.perbuildmax = svr.ranks.PermFromName(value) != DefaultRankValue.Null ? svr.ranks.PermFromName(
                                                            value) : DefaultRankValue.Guest;
                                    break;
                                case "pervisitmax":
                                    level.pervisitmax = svr.ranks.PermFromName(value) != DefaultRankValue.Null ? svr.ranks.PermFromName(
                                                            value) : DefaultRankValue.Guest;
                                    break;
                                case "guns":
                                    level.guns = bool.Parse(value);
                                    break;
                                case "loadongoto":
                                    level.loadOnGoto = bool.Parse(value);
                                    break;
                                case "leafdecay":
                                    level.leafDecay = bool.Parse(value);
                                    break;
                                case "randomflow":
                                    level.randomFlow = bool.Parse(value);
                                    break;
                                case "growtrees":
                                    level.growTrees = bool.Parse(value);
                                    break;
                                case "weather":
                                    level.weather = byte.Parse(value);
                                    break;
                                }
                            }
                            catch (Exception e) {
                                svr.logger.ErrorLog(e);
                            }
                        }
                    }
                    catch (Exception e) {
                        svr.logger.ErrorLog(e);
                    }

                    // The level is finally loaded.
                    svr.logger.Log(string.Format("Level \"{0}\" loaded.", level.name));
                    return level;
                }
                catch (Exception ex) {
                    svr.logger.ErrorLog(ex);
                    return null;
                }
                finally {
                    fs.Close();
                    fs.Dispose();
                }
            }
            svr.logger.Log("ERROR loading level.");
            return null;
        }

        /// <summary>
        /// Gets a property from a level's properties file.
        /// </summary>
        /// <param name="level"> The name of the level to get the property for. </param>
        /// <param name="key"> The string representing the property to obtain. </param>
        /// <returns> The value associated with the given property. This may return null if the given property is
        /// not set for that particular level. </returns>
        public static string GetLevelProperty(string level, string key) {
            // Set the location of the properties file.
            string location = Path.Combine("levels", "level properties", level + ".properties");

            try {
                // Read each line of the file and return a value if the option we are looking
                // for is on the current line.
                using (StreamReader reader = new StreamReader(location)) {
                    string line;
                    while ((line = reader.ReadLine()) != null) {
                        string[] lineSplit = line.Split(' ');
                        if (lineSplit.Length != 3) {
                            continue;
                        }

                        string currentKey = lineSplit[0];
                        string currentValue = lineSplit[2];

                        if (currentKey.Equals(key, StringComparison.OrdinalIgnoreCase)) {
                            return currentValue;
                        }
                    }
                }
            }
            catch (IOException) {
                throw;  // Let the caller handle it.
            }

            return null;
        }

        /// <summary>
        /// Sends a message to everyone in the level.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void ChatLevel(string message) {
            foreach (Player pl in _server.players.Where(pl => pl.level == this)) {
                pl.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends a message to every op in the level.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void ChatLevelOps(string message) {
            foreach (
                Player pl in
                _server.players.Where(
                    pl =>
                    pl.level == this &&
                    (pl.rank.Permission >= _server.props.opchatperm))) {
                pl.SendMessage(message);
            }
        }

        /// <summary>
        /// Sends a message to every admin in the level.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void ChatLevelAdmins(string message) {
            foreach (
                Player pl in
                _server.players.Where(
                    pl =>
                    pl.level == this &&
                    (pl.rank.Permission >= _server.props.adminchatperm))) {
                pl.SendMessage(message);
            }
        }

        /// <summary>
        /// Sets the physics for the map.
        /// </summary>
        /// <param name="newValue"> The new physics setting. </param>
        public void setPhysics(int newValue) {
            // If physics is being turned on, then some blocks needs their physics restarted.
            if (physics == 0 && newValue != 0 && blocks != null) {
                for (int i = 0; i < blocks.Length; i++)
                    // Only check blocks whose ids are over 183 because there are no blocks below that
                    // id that need a restart. (TODO: probably remove this)
                    if (Convert.ToInt32(blocks[i]) > 183)
                        // Add a check for blocks that need to be restarted.
                        if (BlockData.NeedRestart(blocks[i])) {
                            AddCheck(i);
                        }
            }

            // Change the physics setting afterward.
            physics = newValue;
        }

        /// <summary>
        /// Gets or sets a value indicating whether physics are enabled.
        /// </summary>
        /// <value> Whether physics are enabled or not. </value>
        public bool PhysicsEnabled {
            get;
            set;
        }

        /// <summary>
        /// Converts a set of coordinates to its corresponding index within the block array.
        /// </summary>
        /// <param name="x"> The x coordinate. </param>
        /// <param name="y"> The y coordinate. </param>
        /// <param name="z"> The z coordinate. </param>
        /// <returns> The index of the block within the block array, or -1 if the coordinates are outside
        /// the level's boundaries. </returns>
        public int PosToInt(ushort x, ushort y, ushort z) {
            if (x < 0 || x >= width || y < 0 || y >= height || z < 0 || z >= depth) {
                return -1;
            }
            return x + (z * width) + (y * width * depth);
        }

        /// <summary>
        /// Converts an index within the block array to a set of coordinates.
        /// </summary>
        /// <param name="pos"> The index of the block within the block array. </param>
        /// <param name="x"> An output parameter for the x coordinate. </param>
        /// <param name="y"> An output parameter for the y coordinate. </param>
        /// <param name="z"> An output parameter for the z coordinate. </param>
        public void IntToPos(int pos, out ushort x, out ushort y, out ushort z) {
            y = (ushort)(pos / width / depth);
            pos -= y * width * depth;
            z = (ushort)(pos / width);
            pos -= z * width;
            x = (ushort)pos;
        }

        /// <summary>
        /// Given the index of one block, obtains the index of another block that is the provided
        /// distance away from it.
        /// </summary>
        /// <param name="pos"> The index of a block within the block array. </param>
        /// <param name="x"> The distance away from the given block on the x axis. </param>
        /// <param name="y"> The distance away from the given block on the y axis. </param>
        /// <param name="z"> The distance away from the given block on the z axis. </param>
        /// <returns> The index of the block that is the provided distance away from the given block. </returns>
        public int IntOffset(int pos, int x, int y, int z) {
            return pos + x + z * width + y * width * depth;
        }

        /// <summary>
        /// Gets a list of players that are in the level.
        /// </summary>
        /// <returns> The list of players on the level. </returns>
        public List<Player> getPlayers() {
            return _server.players.Where(p => p.level == this).ToList();
        }

        #region ==Physics==

        /// <summary>
        /// Obtains any extra physics-related information about the block in the given position.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <returns> Any physics-related information for the given block. </returns>
        public string foundInfo(ushort x, ushort y, ushort z) {
            Check foundCheck = null;
            try {
                foundCheck = ListCheck.Find(Check => Check.b == PosToInt(x, y, z));
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
            if (foundCheck != null) {
                return foundCheck.extraInfo;
            }
            return String.Empty;
        }

        /// <summary>
        /// Adds a block to the list of blocks to be checked by the physics thread.
        /// </summary>
        /// <param name="b"> The index of the block to be checked. </param>
        /// <param name="extraInfo"> Any extra physics-related information. </param>
        /// <param name="overRide"> Whether to ignore the possibility of a physics check already being
        /// scheduled for that block. Defaults to false. </param>
        /// <param name="Placer"> The player that placed the block. <seealso cref="Player"/></param> 
        public void AddCheck(int b, string extraInfo = "", bool overRide = false, MCHmk.Player Placer = null) {
            try {
                // Add a check for the block if one does not exist.
                // FIXME: Check can be null for some reason
                if (!ListCheck.Exists(Check => Check.b == b)) {
                    ListCheck.Add(new Check(b, extraInfo, Placer));
                }
                else {
                    // If requested, overwrite the physics-related information for the physics check that
                    // already exists for that block.
                    if (overRide) {
                        foreach (Check C2 in ListCheck) {
                            if (C2.b == b) {
                                // According to an old comment, if the list is actively being used, then
                                // physics is active, so don't check physics here.
                                C2.extraInfo = extraInfo;
                                return;
                            }
                        }
                    }
                }

                // Start the physics loop if it has not already started.
                if (!physic.physicssate && physics > 0) {
                    physic.StartPhysics(this);
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Adds a block to the list of blocks that need to be updated by the physics thread.
        /// </summary>
        /// <param name="b"> The index of the block to be updated. </param>
        /// <param name="type"> The id of the new block type. <seealso cref="BlockId"/></param>
        /// <param name="overRide"> Whether the update is immediately performed instead of being part
        /// of the rest of the physics updates. Defaults to false. </param>
        /// <param name="extraInfo"> Any extra physics-related information. </param>
        /// <returns> Whether a new physics update was successfully added </returns>
        public bool AddUpdate(int b, BlockId type, bool overRide = false, string extraInfo = "") {
            try {
                // If requested, change the block immediately.
                if (overRide) {
                    ushort x, y, z;
                    IntToPos(b, out x, out y, out z);
                    // The physics setting does not need to be checked here because it will be checked
                    // in AddCheck() anyway.
                    AddCheck(b, extraInfo, true);
                    Blockchange(x, y, z, type, true, extraInfo);
                    return true;
                }

                // Add the block to the list of pending physics updates if it has not been added already.
                if (!ListUpdate.Exists(Update => Update.b == b)) {
                    ListUpdate.Add(new Update(b, type, extraInfo));

                    // Start the physics loop if it has not started.
                    if (!physic.physicssate && physics > 0) {
                        physic.StartPhysics(this);
                    }
                    return true;
                }
                else {
                    // Handle changes to sand and gravel as a special case.
                    if (type == BlockId.Sand || type == BlockId.Gravel) {
                        // Remove all pending physics updates for that block before adding the new one.
                        ListUpdate.RemoveAll(Update => Update.b == b);
                        ListUpdate.Add(new Update(b, type, extraInfo));

                        // Start the physics loop if it has not started.
                        if (!physic.physicssate && physics > 0) {
                            physic.StartPhysics(this);
                        }
                        return true;
                    }
                }

                return false;
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                return false;
            }
        }

        /// <summary>
        /// Called from the physics loop to handle odoors.
        /// </summary>
        /// <param name="C"> The physics check that is currently being examined. <seealso cref="Check"/></param>
        public void odoor(Check C) {
            // Check for any odoor blocks immediately adjacent to the current block being examined.
            // The odoors need to be the same. If that is the case, change the blocks immediately since
            // "true" is given to the override parameter.
            if (C.time == 0) {
                BlockId foundBlock;

                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, -1, 0, 0)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, -1, 0, 0), foundBlock, true);
                }
                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, 1, 0, 0)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, 1, 0, 0), foundBlock, true);
                }
                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, 0, -1, 0)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, 0, -1, 0), foundBlock, true);
                }
                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, 0, 1, 0)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, 0, 1, 0), foundBlock, true);
                }
                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, 0, 0, -1)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, 0, 0, -1), foundBlock, true);
                }
                foundBlock = BlockData.odoor(GetTile(IntOffset(C.b, 0, 0, 1)));
                if (foundBlock == blocks[C.b]) {
                    AddUpdate(IntOffset(C.b, 0, 0, 1), foundBlock, true);
                }
            }
            else {
                C.time = 255;
            }
            C.time++;  // TODO: is this in the wrong place?
        }

        /// <summary>
        /// Called by the physics loop for door_air blocks. Attempts to activate any adjacent doors or
        /// other blocks that can be activated. Also checks if the door_air should revert back to a door.
        /// </summary>
        /// <param name="C"> The physics check that is currently being examined. <seealso cref="Check"/></param>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="timer"> The time before the door_air blocks turns back into a door. This value is the
        /// number of iterations through the physics loop that must occur before the door is restored. </param>
        /// <param name="instaUpdate"> Whether door_air blocks that are turning back into doors should
        /// update immediately instead of waiting for the current iteration of the physics loop to finish.
        /// Defaults to false. </param>
        public void AnyDoor(Check C, ushort x, ushort y, ushort z, int timer, bool instaUpdate = false) {
            // If this was the first time that this door was checked, which happens after the player breaks
            // it, check if any of the adjacent blocks are doors as well.
            if (C.time == 0) {
                try {
                    PhysDoor((ushort)(x + 1), y, z, instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
                try {
                    PhysDoor((ushort)(x - 1), y, z, instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
                try {
                    PhysDoor(x, y, (ushort)(z + 1), instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
                try {
                    PhysDoor(x, y, (ushort)(z - 1), instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
                try {
                    PhysDoor(x, (ushort)(y - 1), z, instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
                try {
                    PhysDoor(x, (ushort)(y + 1), z, instaUpdate);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }

                try {
                    // If the door that was broken was a door_green, check if any special blocks that can be
                    // activated are next to the door, and if so, activate their functions.
                    if (blocks[C.b] == BlockId.DoorGreenActive) {
                        // This for-loop actually includes blocks that are not immediately adjacent to the door,
                        // such as those that are diagonally above or below the block. I'm not sure if that is
                        // intentional or an oversight. -Jjp137
                        for (int xx = -1; xx <= 1; xx++) {
                            for (int yy = -1; yy <= 1; yy++) {
                                for (int zz = -1; zz <= 1; zz++) {
                                    // Get the type of one of the surrounding blocks.
                                    BlockId b = GetTile(IntOffset(C.b, xx, yy, zz));
                                    // TODO: door_green triggers rocketstart blocks on physics == 1, probably a bug
                                    if (b == BlockId.RocketStart) {
                                        // TODO: The intent of this if-statement is to prevent any non-door-related
                                        // physics events from happening, but the Blockchange() call causes the
                                        // door to disappear instead. Shouldn't it just return? - Jjp137
                                        if (physics == 5) {
                                            Blockchange(x, y, z, BlockId.Air);
                                            return;
                                        }

                                        // Otherwise, check if the path ahead of the rocketstart block is clear. Note
                                        // that the rocket that gets fired goes in a different direction depending
                                        // on the angle that the rocketstart block is activated from. If the path
                                        // is clear and has no pending physics updates, fire a rocket.
                                        int b1 = IntOffset(C.b, xx * 3, yy * 3, zz * 3);
                                        int b2 = IntOffset(C.b, xx * 2, yy * 2, zz * 2);
                                        bool unblocked = blocks[b1] == BlockId.Air&& blocks[b2] == BlockId.Air&&
                                                         !ListUpdate.Exists(Update => Update.b == b1) &&
                                                         !ListUpdate.Exists(Update => Update.b == b2);
                                        if (unblocked) {
                                            AddUpdate(IntOffset(C.b, xx * 3, yy * 3, zz * 3), BlockId.RocketHead);
                                            AddUpdate(IntOffset(C.b, xx * 2, yy * 2, zz * 2), BlockId.Embers);
                                        }
                                    }
                                    else if (b == BlockId.Firework) {
                                        // TODO: The intent of this if-statement is to prevent any non-door-related
                                        // physics events from happening, but the Blockchange() call causes the
                                        // door to disappear instead. Shouldn't it just return? - Jjp137
                                        if (physics == 5) {
                                            Blockchange(x, y, z, BlockId.Air);
                                            return;
                                        }

                                        // Otherwise, launch the firework if the two blocks above it do not have any
                                        // obstructions or pending physics updates.
                                        int b1 = IntOffset(C.b, xx, yy + 1, zz);
                                        int b2 = IntOffset(C.b, xx, yy + 2, zz);
                                        bool unblocked = blocks[b1] == BlockId.Air&& blocks[b2] == BlockId.Air&&
                                                         !ListUpdate.Exists(Update => Update.b == b1) &&
                                                         !ListUpdate.Exists(Update => Update.b == b2);
                                        if (unblocked) {
                                            AddUpdate(b2, BlockId.Firework);
                                            AddUpdate(b1, BlockId.StillLava, false, "dissipate 100");
                                        }
                                    }
                                    else if (b == BlockId.Tnt) {
                                        // TODO: The intent of this if-statement is to prevent any non-door-related
                                        // physics events from happening, but the Blockchange() call causes the
                                        // door to disappear instead. Shouldn't it just return? - Jjp137
                                        if (physics == 5) {
                                            Blockchange(x, y, z, BlockId.Air);
                                            return;
                                        }

                                        // Cause an explosion if we are not in doors-only mode.
                                        MakeExplosion((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz), 0);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
            }
            // Otherwise, we may be still waiting for the door to come back.
            if (C.time < timer) {
                C.time++;
            }
            // If the time has elapsed, restore the door and mark the Check object as being ready for deletion.
            else {
                AddUpdate(C.b, BlockData.SaveConvert(blocks[C.b]));
                C.time = 255;  // All Check structs that need to be deleted have a value of 255.
            }
        }

        /// <summary>
        /// Called by AnyDoor(). Activates the door located at the given location, if possible.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="instaUpdate"> Whether door_air blocks that are turning back into doors should
        /// update immediately instead of waiting for the current iteration of the physics loop to finish.
        /// Defaults to false. </param>
        public void PhysDoor(ushort x, ushort y, ushort z, bool instaUpdate) {
            // Check if the block at the given location is a door.
            int foundInt = PosToInt(x, y, z);
            if (foundInt < 0) {  // Handle blocks that are out of bounds.
                return;
            }

            BlockId FoundAir = BlockData.DoorAirs(blocks[foundInt]);

            // If the block is a regular door, change the door to its air equivalent. This can be done
            // as part of a physics update or immediately.
            if (FoundAir != 0) {
                if (!instaUpdate) {
                    AddUpdate(foundInt, FoundAir);
                }
                else {
                    Blockchange(x, y, z, FoundAir);
                }
                return;
            }

            // If the block is a tDoor, change the tdoor block to an air block with physics properties that
            // convert the block back to a normal door after some time.
            // FIXME: tdoor blocks can be lost when saving
            if (BlockData.tDoor(blocks[foundInt])) {
                AddUpdate(foundInt, BlockId.Air, false, "wait 16 door 1 revert " + Convert.ToInt32(blocks[foundInt]).ToString());
            }

            // If the block is an odoor or odoor_air, change the block to its equivalent block immediately.
            if (BlockData.odoor(blocks[foundInt]) != BlockId.Null) {
                AddUpdate(foundInt, BlockData.odoor(blocks[foundInt]), true);
            }
        }

        /// <summary>
        /// Makes an explosion at a given location.
        /// </summary>
        /// <remarks>
        /// The explosion's maximum diameter is 2*(size + 3) + 1 blocks.
        /// </remarks>
        /// <param name="x"> The x coordinate of the explosion's center. </param>
        /// <param name="y"> The y coordinate of the explosion's center. </param>
        /// <param name="z"> The z coordinate of the explosion's center. </param>
        /// <param name="size"> The size of the explosion. </param>
        /// <param name="force"> Whether an explosion should occur even if the current physics setting does
        /// not allow it. Defaults to false. </param>
        public void MakeExplosion(ushort x, ushort y, ushort z, int size, bool force = false) {
            int xx, yy, zz;
            var rand = new Random();
            BlockId b;

            // Unless desired, do not do anything if the current physics setting does not allow explosions.
            if (physics < 2 && force == false) {
                return;
            }
            if (physics == 5 && force == false) {
                return;
            }

            // Change the block at the center of the explosion.
            AddUpdate(PosToInt(x, y, z), BlockId.TntExplosion, true);

            // Do a first pass over the blocks surrounding the center of the explosion.
            for (xx = (x - (size + 1)); xx <= (x + (size + 1)); ++xx)
                for (yy = (y - (size + 1)); yy <= (y + (size + 1)); ++yy)
                    for (zz = (z - (size + 1)); zz <= (z + (size + 1)); ++zz)
                        try {
                            b = GetTile((ushort)xx, (ushort)yy, (ushort)zz);
                            // Convert any vanilla TNT blocks into the equivalent block type in MCHmk.
                            if (b == BlockId.Tnt) {
                                AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.SmallTnt);
                            }
                            // Destroy any other non-TNT blocks within the explosion.
                            else if (b != BlockId.SmallTnt && b != BlockId.BigTnt && b != BlockId.NukeTnt) {
                                // Some blocks get turned into ashes, basically.
                                if (rand.Next(1, 11) <= 4) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.TntExplosion);
                                }
                                // Other blocks may simply get destroyed.
                                else if (rand.Next(1, 11) <= 8) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.Air);
                                }
                                // The blocks that survive drop down and disappear after some time.
                                else {
                                    AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), "drop 50 dissipate 8");
                                }
                            }
                            // Activate any TNT blocks within the area.
                            else {
                                AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz));
                            }
                        }
                        catch (Exception e) {
                            _logger.ErrorLog(e);
                        }

            // Do a second pass, and check a larger range of blocks this time.
            for (xx = (x - (size + 2)); xx <= (x + (size + 2)); ++xx)
                for (yy = (y - (size + 2)); yy <= (y + (size + 2)); ++yy)
                    for (zz = (z - (size + 2)); zz <= (z + (size + 2)); ++zz) {
                        b = GetTile((ushort)xx, (ushort)yy, (ushort)zz);
                        // Blocks have a 66% chance of being affected this time, making blocks that are farther
                        // away less likely to be affected.
                        if (rand.Next(1, 10) < 7)
                            if (BlockData.Convert(b) != BlockId.Tnt) {
                                // Some blocks get turned into ashes, basically.
                                if (rand.Next(1, 11) <= 4) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.TntExplosion);
                                }
                                // Other blocks may simply get destroyed.
                                else if (rand.Next(1, 11) <= 8) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.Air);
                                }
                                // The blocks that survive drop down and disappear after some time. This includes
                                // the tnt_explosion blocks that have already been formed previously.
                                else {
                                    AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), "drop 50 dissipate 8");
                                }
                            }
                        // Convert any vanilla TNT blocks into the equivalent block type in MCHmk.
                        if (b == BlockId.Tnt) {
                            AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.SmallTnt);
                        }
                        // Activate any TNT blocks within the area.
                        else if (b == BlockId.SmallTnt || b == BlockId.BigTnt || b == BlockId.NukeTnt) {
                            AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz));
                        }
                    }

            // Do a third pass, and check the largest range of blocks.
            for (xx = (x - (size + 3)); xx <= (x + (size + 3)); ++xx)
                for (yy = (y - (size + 3)); yy <= (y + (size + 3)); ++yy)
                    for (zz = (z - (size + 3)); zz <= (z + (size + 3)); ++zz) {
                        b = GetTile((ushort)xx, (ushort)yy, (ushort)zz);
                        // Blocks have a 22% chance of being affected this time, making blocks that are farther
                        // away less likely to be affected.
                        if (rand.Next(1, 10) < 3)
                            if (BlockData.Convert(b) != BlockId.Tnt) {
                                // Some blocks get turned into ashes, basically.
                                if (rand.Next(1, 11) <= 4) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.TntExplosion);
                                }
                                // Other blocks may simply get destroyed.
                                else if (rand.Next(1, 11) <= 8) {
                                    AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.Air);
                                }
                                // The blocks that survive drop down and disappear after some time. This includes
                                // the tnt_explosion blocks that have already been formed previously.
                                else {
                                    AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), "drop 50 dissipate 8");
                                }
                            }
                        // Convert any vanilla TNT blocks into the equivalent block type in MCHmk.
                        if (b == BlockId.Tnt) {
                            AddUpdate(PosToInt((ushort)xx, (ushort)yy, (ushort)zz), BlockId.SmallTnt);
                        }
                        // Activate any TNT blocks within the area.
                        else if (b == BlockId.SmallTnt || b == BlockId.BigTnt || b == BlockId.NukeTnt) {
                            AddCheck(PosToInt((ushort)xx, (ushort)yy, (ushort)zz));
                        }
                    }
        }

        /// <summary>
        /// Changes a block at the given location and then adds a physics check for that block.
        /// </summary>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        /// <param name="b"> The id of the new block type. <seealso cref="BlockId"/></param>
        public void placeBlock(ushort x, ushort y, ushort z, BlockId b) {
            AddUpdate(PosToInt((ushort)x, (ushort)y, (ushort)z), b, true);
            AddCheck(PosToInt((ushort)x, (ushort)y, (ushort)z));
        }

        /// <summary>
        /// Creates fireworks at a given location.
        /// </summary>
        /// <remarks>
        /// The fireworks' diameter is 2*(size + 1) + 1 blocks.
        /// </remarks>
        /// <param name="x"> The x coordinate of the fireworks' center. </param>
        /// <param name="y"> The y coordinate of the fireworks' center. </param>
        /// <param name="z"> The z coordinate of the fireworks' center. </param>
        /// <param name="size"> The size of the fireworks. </param>
        public void Firework(ushort x, ushort y, ushort z, int size) {
            ushort xx, yy, zz;
            var rand = new Random();
            int storedRand1, storedRand2;

            // Don't do anything if the physics setting does not allow fireworks.
            if (physics < 1) {
                return;
            }
            if (physics == 5) {
                return;
            }

            // Figure out the range of cloth blocks that will appear in the radius of the fireworks explosion.
            storedRand1 = rand.Next(21, 36);
            storedRand2 = rand.Next(21, 36);

            // Change the center of the fireworks explosion to air.
            // An old comment says that override is not being used because it may cause a colored block to
            // be generated without any extra physics-related information, which causes it not to disappear.
            // This is due to AddUpdate with "false" for the override parameter adding a physics check for
            // that position with no extra physics-related information.
            AddUpdate(PosToInt(x, y, z), BlockId.Air);

            // Generate the blocks for the fireworks. These blocks steadily drop and have a 25% chance of
            // disappearing per physics update.
            for (xx = (ushort)(x - (size + 1)); xx <= (ushort)(x + (size + 1)); ++xx)
                for (yy = (ushort)(y - (size + 1)); yy <= (ushort)(y + (size + 1)); ++yy)
                    for (zz = (ushort)(z - (size + 1)); zz <= (ushort)(z + (size + 1)); ++zz)
                        if (GetTile(xx, yy, zz) == BlockId.Air)
                            if (rand.Next(1, 40) < 2)  // 2.56% chance
                                AddUpdate(PosToInt(xx, yy, zz),
                                          (BlockId)rand.Next(Math.Min(storedRand1, storedRand2), Math.Max(storedRand1, storedRand2)), 
                                          false, "drop 100 dissipate 25");
        }

        /// <summary>
        /// Handles movement of finite blocks. Called by the physics loop.
        /// </summary>
        /// <param name="C"> The physics check that is currently being examined. <seealso cref="Check"/></param>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        public void finiteMovement(Check C, ushort x, ushort y, ushort z) {
            var rand = new Random();

            var bufferfiniteWater = new List<int>();
            var bufferfiniteWaterList = new List<UShortCoords>();

            // If there is air below the block, then the block is currently falling, so "move" the block
            // one space below, turning the block being currently checked into air.
            if (GetTile(x, (ushort)(y - 1), z) == BlockId.Air) {
                // The new block inherits the finite behavior.
                AddUpdate(PosToInt(x, (ushort)(y - 1), z), blocks[C.b], false, C.extraInfo);
                AddUpdate(C.b, BlockId.Air);
                // The old block no longer has finite behavior.
                C.extraInfo = String.Empty;
            }
            // If the block below it is non-active water or lava, then the finite block gets "absorbed"
            // into that block, so just turn the current block being checked into air.
            else if (GetTile(x, (ushort)(y - 1), z) == BlockId.StillWater ||
                     GetTile(x, (ushort)(y - 1), z) == BlockId.StillLava) {
                AddUpdate(C.b, BlockId.Air);
                // The old block no longer has finite behavior.
                C.extraInfo = String.Empty;
            }
            // Otherwise, it is on a surface, so move the block in some sort of random way.
            else {
                // Make a list with the numbers 0 to 24. These will be used as indices later on.
                for (int i = 0; i < 25; ++i) {
                    bufferfiniteWater.Add(i);
                }

                // Shuffle this list.
                for (int k = bufferfiniteWater.Count - 1; k > 1; --k) {
                    int randIndx = rand.Next(k); //
                    int temp = bufferfiniteWater[k];
                    bufferfiniteWater[k] = bufferfiniteWater[randIndx]; // move random num to end of list.
                    bufferfiniteWater[randIndx] = temp;
                }

                UShortCoords pos;

                // Obtain the nearest 25 blocks on the horizontal plane, including itself, and add them
                // to another list.
                for (var xx = (ushort)(x - 2); xx <= x + 2; ++xx) {
                    for (var zz = (ushort)(z - 2); zz <= z + 2; ++zz) {
                        pos.X = xx;
                        pos.Y = 0; // Unused
                        pos.Z = zz;
                        bufferfiniteWaterList.Add(pos);
                    }
                }

                // Keep picking random indices until the finite block moves or until we went through all
                // the indices, which means that no moves are possible.
                foreach (int i in bufferfiniteWater) {
                    // Get one of the 25 blocks.
                    pos = bufferfiniteWaterList[i];

                    // The finite block may be able to move to that position if the block in that position
                    // and the block below it is air.
                    if (GetTile(pos.X, (ushort)(y - 1), pos.Z) == BlockId.Air&&
                            GetTile(pos.X, y, pos.Z) == BlockId.Air) {

                        // Change the position to reflect where the finite block will be.
                        if (pos.X < x) {
                            pos.X = (ushort)(Math.Floor((double)(pos.X + x) / 2));
                        }
                        else {
                            pos.X = (ushort)(Math.Ceiling((double)(pos.X + x) / 2));
                        }
                        if (pos.Z < z) {
                            pos.Z = (ushort)(Math.Floor((double)(pos.Z + z) / 2));
                        }
                        else {
                            pos.Z = (ushort)(Math.Ceiling((double)(pos.Z + z) / 2));
                        }

                        // Since the position changed, check again to see if the position is an air block.
                        // If so, attempt to move the block there and turn the block at its former position
                        // into air if the move succeeded. The new block inherits the finite movement, while
                        // the old block loses that property.
                        if (GetTile(pos.X, y, pos.Z) == BlockId.Air) {
                            if (AddUpdate(PosToInt(pos.X, y, pos.Z), blocks[C.b], false, C.extraInfo)) {
                                AddUpdate(C.b, BlockId.Air);
                                C.extraInfo = String.Empty;
                                break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Nested type: BlockPos

        /// <summary>
        /// The BlockPos struct represents a block change.
        /// </summary>
        public struct BlockPos {
            /// <summary>
            /// When the block change occurred.
            /// </summary>
            public DateTime TimePerformed;
            /// <summary>
            /// Whether the block was deleted. If false, the block was created instead.
            /// </summary>
            public bool deleted;
            /// <summary>
            /// The name of the player that modified the block.
            /// </summary>
            public string name;
            /// <summary>
            /// The block that the player was holding when the block was changed.
            /// </summary>
            public BlockId type;
            /// <summary>
            /// The x coordinate of the block.
            /// </summary>
            public ushort x;
            /// <summary>
            /// The y coordinate of the block.
            /// </summary>
            public ushort y;
            /// <summary>
            /// The z coordinate of the block.
            /// </summary>
            public ushort z;
        }

        #endregion

        #region Nested type: UndoPos

        /// <summary>
        /// The UndoPos struct represents an entry in the undo buffer.
        /// </summary>
        public struct UndoPos {
            /// <summary>
            /// The index of the block within the block array.
            /// </summary>
            public int location;
            /// <summary>
            /// The id of the new block type.
            /// </summary>
            public BlockId newType;
            /// <summary>
            /// The id of the former block type.
            /// </summary>
            public BlockId oldType;
            /// <summary>
            /// When the block change occurred.
            /// </summary>
            public DateTime timePerformed;
        }

        #endregion

        #region Nested type: Zone

        /// <summary>
        /// The Zone struct represents a zone in the level.
        /// </summary>
        public struct Zone {
            /// <summary>
            /// The name of the player or group that owns the zone.
            /// </summary>
            public string owner;
            /// <summary>
            /// The x coordinate of the corner with the greater coordinates.
            /// </summary>
            public ushort bigX;
            /// <summary>
            /// The y coordinate of the corner with the greater coordinates.
            /// </summary>
            public ushort bigY;
            /// <summary>
            /// The z coordinate of the corner with the greater coordinates.
            /// </summary>
            public ushort bigZ;
            /// <summary>
            /// The x coordinate of the corner with the lesser coordinates.
            /// </summary>
            public ushort smallX;
            /// <summary>
            /// The y coordinate of the corner with the lesser coordinates.
            /// </summary>
            public ushort smallY;
            /// <summary>
            /// The z coordinate of the corner with the lesser coordinates
            /// </summary>
            public ushort smallZ;
        }

        #endregion
    }

    /// <summary>
    /// The Check class represents a block that the physics loop needs to calculate any
    /// physics-related changes for.
    /// </summary>
    public class Check {
        /// <summary>
        /// The index of the block within the block array to check.
        /// </summary>
        public int b;
        /// <summary>
        /// Any extra physics-related information associated with the block.
        /// </summary>
        public string extraInfo = String.Empty;
        /// <summary>
        /// The number of physics updates that have passed since this block was first checked.
        /// This value can be set to 255 to schedule the block's removal from the list of blocks
        /// to be checked for physics changes. This is also manually changed in some situations.
        /// </summary>
        public byte time;
        /// <summary>
        /// The player that modified the block, if any. Defaults to null.
        /// </summary>
        public Player p;

        /// <summary>
        /// Constructs a Check object.
        /// </summary>
        /// <param name="b"> An index of a block within the block array.</param>
        /// <param name="extraInfo"> Any extra physics-related information associated with the block. </param>
        /// <param name="placer"> The player that modified the block, if any. Defaults to null.
        /// <seealso cref="Player"/></param>
        public Check(int b, string extraInfo = "", Player placer = null) {
            this.b = b;
            time = 0;
            this.extraInfo = extraInfo;
            p = placer;
        }
    }

    /// <summary>
    /// The Update class represents a block that is going to be changed by the physics loop.
    /// </summary>
    public class Update {
        /// <summary>
        /// The index of the block within the block array to check.
        /// </summary>
        public int b;
        /// <summary>
        /// Any extra physics-related information associated with the block.
        /// </summary>
        public string extraInfo = String.Empty;
        /// <summary>
        /// The id of the new block type.
        /// </summary>
        public BlockId type;

        /// <summary>
        /// Constructs an Update object.
        /// </summary>
        /// <param name="b"> An index of a block within the block array. </param>
        /// <param name="type"> The id of the new block type.</param>
        /// <param name="extraInfo"> Any extra physics-related information associated with the block. </param>
        public Update(int b, BlockId type, string extraInfo = "") {
            this.b = b;
            this.type = type;
            this.extraInfo = extraInfo;
        }
    }
}

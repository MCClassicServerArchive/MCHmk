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
using System.Data;

using MCHmk.SQL;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /about command, which displays information about a block's change history.
    /// </summary>
    public class CmdAbout : Command {
        /// <summary>
        /// The list of keywords that are associated with /about.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"info", "block", "history", "grief"});

        /// <summary>
        /// Gets the name of /about.
        /// </summary>
        public override string Name {
            get {
                return "about";
            }
        }

        /// <summary>
        /// Gets the shortcut for /about.
        /// </summary>
        public override string Shortcut {
            get {
                return "b";
            }
        }

        /// <summary>
        /// Gets the category that /about belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /about. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /about can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the default rank of /about.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        /// <summary>
        /// Constructs an instance of the /about command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /about will belong to. <seealso cref="Server"/></param>
        public CmdAbout(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /about.
        /// </summary>
        /// <param name="p"> The player that used /about. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            if (!p.IsConsole) {
                // Abort any conflicting commands, such as /cuboid.
                p.ClearSelection();

                // Listen for a block change by the user. The next block change will determine the target block.
                p.StartSelection(BlockSelected, null);
                p.SendMessage("Select a block to examine by building or breaking it.");
            }
            else {
                // /about makes no sense when used from the console.
                p.SendMessage("This command can only be used by an in-game player.");
            }
        }

        /// <summary>
        /// Called when a player selects a block to examine.
        /// </summary>
        /// <param name="p"> The player that selected that block. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the block selection. <seealso cref="CommandTempData"/></param>
        private void BlockSelected(Player p, CommandTempData c) {
            // The player selected a block, so unless /static is being used, don't listen for a block change anymore.
            if (!p.staticCommands) {
                p.ClearSelection();
            }

            BlockId b = p.level.GetTile(c.X, c.Y, c.Z);
            // Does this ever happen? - Jjp137
            if (b == BlockId.Null) {
                p.SendMessage("The block that was selected is invalid.");
                _s.logger.Log("Invalid /about block at " + c.X.ToString() + ", " + c.Y.ToString() + ", " + c.Z.ToString());
                return;
            }

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            p.SendBlockchange(c.X, c.Y, c.Z, b);

            // Create and send the first line to the player, which consists of the coordinates and the current block
            // type at that location.
            string firstLine = "Block at (" + c.X.ToString() + ", " + c.Y.ToString() + ", " + c.Z.ToString() + "): ";
            firstLine += "&f" + Convert.ToInt32(b).ToString() + " = " + BlockData.Name(b);
            p.SendMessage(firstLine);

            // If physics have been applied to this block, display that information too.
            string physicsInfo = p.level.foundInfo(c.X, c.Y, c.Z);
            if (physicsInfo.Length != 0) {
                p.SendMessage("Physics information: &a" + physicsInfo);
            }

            // Send the rest of the information.
            SendBlockHistory(p, c.X, c.Y, c.Z);
        }

        /// <summary>
        /// Sends the change history of a block.
        /// </summary>
        /// <param name="p"> The player that selected the block to examine. <seealso cref="Player"/></param>
        /// <param name="x"> The x coordinate of the block. </param>
        /// <param name="y"> The y coordinate of the block. </param>
        /// <param name="z"> The z coordinate of the block. </param>
        private void SendBlockHistory(Player p, ushort x, ushort y, ushort z) {
            // Since obtaining the color of the name requires a database query, cache any usernames and their
            // colors here to prevent repeated queries. This especially helps if a user modified the same block
            // over and over.
            Dictionary<string, string> colorCache = new Dictionary<string, string>();

            string username, time, block, color;
            bool deleted;
            bool foundEntry = false;  // True if there is at least one entry

            // FIXME: PreparedStatement
            string query = "SELECT * FROM Block" + p.level.name + 
                           " WHERE X=" + x.ToString() + " AND Y=" + y.ToString() + " AND Z=" + z.ToString();

            // Look in the corresponding SQL table for this level first.
            using (DataTable Blocks = _s.database.ObtainData(query)) {
                foundEntry = Blocks.Rows.Count > 0;

                foreach (DataRow row in Blocks.Rows) {
                    // Extract the needed information from each row and put it in the proper format.
                    username = row["Username"].ToString().Trim();
                    time = DateTime.Parse(row["TimePerformed"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    block = BlockData.Name((BlockId)Convert.ToByte(row["Type"]));
                    deleted = Convert.ToBoolean(row["Deleted"]);

                    // Obtain the color of the player's rank, either by retrieving it from the cache or querying
                    // the database.
                    if (colorCache.ContainsKey(username)) {
                        color = colorCache[username];
                    }
                    else {
                        color = GetOfflineRankColor(username);
                        colorCache[username] = color;
                    }

                    // Send each block change to the player.
                    p.SendMessage(CreateInfoString(username, color, block, deleted));
                    p.SendMessage(CreateTimeString(time));
                }
            }

            // Look for any recent changes that have not been saved to the corresponding SQL table yet.
            List<Level.BlockPos> inCache = p.level.blockCache.FindAll(bP => bP.x == x && bP.y == y && bP.z == z);

            // It's possible that there are no relevant changes in the SQL table, yet there are changes in memory
            // that are waiting to be saved to the database, so include these as well.
            if (!foundEntry) {
                foundEntry = inCache.Count > 0;
            }

            foreach (Level.BlockPos pos in inCache) {
                // Extract the needed information from each BlockPos object and put it in the proper format.
                username = pos.name.Trim();
                time = pos.TimePerformed.ToString("yyyy-MM-dd HH:mm:ss");
                block = BlockData.Name(pos.type);
                deleted = pos.deleted;

                // Obtain the color of the player's rank, either by retrieving it from the cache or querying
                // the database.
                if (colorCache.ContainsKey(username)) {
                    color = colorCache[username];
                }
                else {
                    color = GetOfflineRankColor(username);
                    colorCache[username] = color;
                }

                // Send each block change to the player.
                p.SendMessage(CreateInfoString(username, color, block, deleted));
                p.SendMessage(CreateTimeString(time));
            }

            // Let the player know if no changes for that block was found.
            if (!foundEntry) {
                p.SendMessage("No change history found for this block.");
            }
        }

        /// <summary>
        /// Creates the message that tells the player how the block was modified.
        /// </summary>
        /// <param name="username"> The name of the player that modified the block. </param>
        /// <param name="userColor"> The color of the player's rank. </param>
        /// <param name="block"> The block type that was equipped when the block was modified. </param>
        /// <param name="deleted"> Whether the block was deleted. If false, it was created. </param>
        /// <returns> The string that will be sent to the user. </returns>
        private string CreateInfoString(string username, string userColor, string block, bool deleted) {
            string result = String.Empty;
            result += deleted ? "&4Deleted by " : "&3Created by ";
            result += userColor + username + _s.props.DefaultColor;
            result += " using &3" + block + _s.props.DefaultColor;

            return result;
        }

        /// <summary>
        /// Creates the message that tells the player when a block was modified.
        /// </summary>
        /// <param name="time"> A string representing the modification time. </param>
        /// <returns> The string that will be sent to the user. </returns>
        private string CreateTimeString(string time) {
            return "Time modified: &2" + time;
        }

        /// <summary>
        /// Obtains the color of a player's rank. Works with both online and offline players.
        /// </summary>
        /// <remarks>
        /// This is best used when the player in question might be offline, although it can be used for online 
        /// players too. If possible, however, it is better to obtain the color from the Player instance since
        /// using this method requires accessing the database.
        /// </remarks>
        /// <param name="username"> The player's name. </param>
        /// <returns> The uuid of the player. </returns>
        private string GetOfflineRankColor(string username) {
            Uuid uuid = Uuid.FindUuid(_s.database, username);
            // If the resulting uuid is invalid, it is possible that the account is an unpaid one, so try again with a
            // tilde appended at the end of the name.
            if (!uuid.IsValid) {
                uuid = Uuid.FindUuid(_s.database, username + "~");
            }

            return _s.ranks.FindPlayerColor(uuid);
        }

        /// <summary>
        /// Called when /help is used on /about.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/about - Displays information about a particular block, including the history of " +
                "any changes made to it.");
        }
    }
}

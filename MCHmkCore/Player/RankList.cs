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
using System.Globalization;
using System.IO;
using System.Linq;

namespace MCHmk {
    /// <summary>
    /// The RankList class represents a list of the ranks on the server. It also manages the ranks.properties file.
    /// </summary>
    public class RankList : IEnumerable<Rank> {
        /// <summary>
        /// The list of all ranks on the server.
        /// </summary>
        private List<Rank> _ranks = new List<Rank>();
        /// <summary>
        /// A reference to the logger that will log any errors.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// The default rank on the server. Players that were just unbanned are given this rank.
        /// </summary>
        private Rank _defaultRank;

        /// <summary>
        /// Gets the number of ranks in the list.
        /// </summary>
        public int Count {
            get {
                return _ranks.Count;
            }
        }

        /// <summary>
        /// Gets the Rank object at the specified index.
        /// </summary>
        public Rank this[int index] {
            get {
                return _ranks[index];
            }
        }

        /// <summary>
        /// Gets the default rank in the server.
        /// </summary>
        public Rank DefaultRank {
            get {
                return _defaultRank;
            }
        }

        /// <summary>
        /// Gets or sets the logger used to log errors related to the RankList.
        /// </summary>
        public Logger Logger {
            get {
                return _logger;
            }
            set {
                _logger = value;
            }
        }

        private readonly string ranksPath = Path.Combine("properties", "ranks.properties");

        /// <summary>
        /// Constructs a RankList object.
        /// </summary>
        public RankList() { }

        /// <summary>
        /// Initializes all ranks. Reads the ranks.properties file if it exists.
        /// </summary>
        /// <param name="defaultRank"> The name of the default rank. </param>
        public void InitAll(string defaultRank) {
            _ranks = new List<Rank>();

            // Read ranks.properties if it exists.
            if (File.Exists(ranksPath)) {
                string[] lines = File.ReadAllLines(ranksPath);

                // Create a new, empty, and temporary group that will be used later.
                Rank thisGroup = new Rank();

                // gots = number of settings per rank
                // version = the version of ranks.properties being read
                // Both of these affect parsing.
                int gots = 0, version = 1;

                // Try to parse the version of ranks.properties.
                if (lines.Length > 0 && lines[0].StartsWith("#Version ")) {
                    try {
                        version = int.Parse(lines[0].Remove(0, 9)); // Uh...no -Jjp137
                    }
                    catch {  // TODO: find exact exception to catch
                        Log("The ranks.properties version header is invalid! Ranks may fail to load!");
                    }
                }

                // Examine all lines in the file.
                foreach (string s in lines) {
                    try {
                        // Skip commented and blank lines.
                        if (s == String.Empty || s[0] == '#') {
                            continue;
                        }
                        // Examine only valid lines.
                        if (s.Split('=').Length == 2) {
                            string property = s.Split('=')[0].Trim();
                            string value = s.Split('=')[1].Trim();

                            // It expects 'rankname' to be the first field.
                            if (thisGroup.name == String.Empty && property.ToLower() != "rankname") {
                                Log("Hitting an error at " + s + " of ranks.properties");
                            }
                            else {
                                switch (property.ToLower()) {
                                    case "rankname": // Name of the rank
                                        // This is the beginning of another rank, so reset the number of properties
                                        // we have read for that rank to zero.
                                        gots = 0; 
                                        // Make a new rank object.
                                        thisGroup = new Rank();

                                        // Hardcoded values...really? -Jjp137
                                        if (value.ToLower() == "adv" || value.ToLower() == "op" || value.ToLower() == "super" || value.ToLower() == "nobody" || value.ToLower() == "noone") {
                                            Log("Cannot have a rank named \"" + value.ToLower() + "\", this rank is hard-coded.");
                                        }

                                        // Check if a rank with that name has already been parsed, and if so, do
                                        // not add the same rank twice.
                                        else if (_ranks.Find(grp => grp.name == value.ToLower()) == null) {
                                            thisGroup.trueName = value;
                                        }
                                        else {
                                            Log("Cannot add the rank " + value + " twice");
                                        }
                                        break;
                                    case "permission": // Permission value
                                        int foundPermission;

                                        // Parse the permission value, if possible.
                                        try {
                                            foundPermission = int.Parse(value);
                                        }
                                        catch {  // TODO: find exact exception to check
                                            Log("Invalid permission on " + s);
                                            break;
                                        }

                                        // When you create an empty Rnk object, its level permission is basically a null value.
                                        // Thus, if it is not a null value, that means that we are assigning another permission
                                        // value to the same rank.
                                        if (thisGroup.Permission != DefaultRankValue.Null) {
                                            Log("Setting permission again on " + s);
                                            gots--;
                                        }

                                        // Make sure that another rank with the same permission value does not exist.
                                        bool allowed = _ranks.Find(grp => grp.Permission == foundPermission) == null;

                                        // The permission value must be between -50 and 119 (inclusive).
                                        if (foundPermission > 119 || foundPermission < -50) {
                                            Log("Permission must be between -50 and 119 for ranks");
                                            break;
                                        }

                                        // Set the new rank's permission value if another rank with the given
                                        // permission value does not exist.
                                        if (allowed) {
                                            gots++;
                                            thisGroup.Permission = foundPermission;
                                        }
                                        else {
                                            Log("Cannot have 2 ranks set at permission level " + value);
                                        }
                                        break;
                                    case "limit": // Command block limit
                                        int foundLimit;

                                        // Parse the command block limit value, if possible.
                                        try {
                                            foundLimit = int.Parse(value);
                                        }
                                        catch {  // TODO: find exact exception to check
                                            Log("Invalid limit on " + s);
                                            break;
                                        }

                                        gots++;
                                        thisGroup.maxBlocks = foundLimit;
                                        break;
                                    case "maxundo": // Maximum undo limit
                                        int foundMax;

                                        // Parse the maximum undo limit, if possible.
                                        try {
                                            foundMax = int.Parse(value);
                                        }
                                        catch {  // TODO: Find exact exception to check
                                            Log("Invalid maximum on " + s);
                                            break;
                                        }

                                        gots++;
                                        thisGroup.maxUndo = foundMax;
                                        break;
                                    case "color": // Color of those with that rank
                                        char foundChar;

                                        // Parse the color code, if possible.
                                        try {
                                            foundChar = char.Parse(value);
                                        }
                                        catch {  // TODO: find exact exception to catch
                                            Log("Incorrect color on " + s);
                                            break;
                                        }

                                        // Make sure that the color code is valid.
                                        if ((foundChar >= '0' && foundChar <= '9') || (foundChar >= 'a' && foundChar <= 'f')) {
                                            gots++;
                                            thisGroup.color = foundChar.ToString(CultureInfo.InvariantCulture);
                                        }
                                        else {
                                            Log("Invalid color code at " + s);
                                        }
                                        break;
                                    case "filename": // File name where the list of players with that rank are saved
                                        // File names should not contain directory separators.
                                        if (value.Contains("\\") || value.Contains("/")) {
                                            Log("Invalid filename on " + s);
                                            break;
                                        }

                                        gots++;
                                        thisGroup.fileName = value;
                                        break;
                                    case "motd": // Rank-specific motd
                                        // Only set the rank-specific motd if it is not empty.
                                        if (!String.IsNullOrEmpty(value)) {
                                            thisGroup.MOTD = value;
                                        }
                                        gots++;
                                        break;
                                }

                                // The maximum possible number of gots is 6, but different versions of the ranks.properties
                                // file had fewer properties per rank, so this is accounting for that as well.
                                if ((gots >= 4 && version < 2) || (gots >= 5 && version < 3) || gots >= 6) {
                                    // Older versions of ranks.properties did not have a max undo setting back
                                    // then, so determine a default value for that setting based on the permission
                                    // value of the rank.
                                    if (version < 2) {
                                        if (thisGroup.Permission >= 100) {
                                            thisGroup.maxUndo = int.MaxValue;
                                        }
                                        else if (thisGroup.Permission >= 80) {
                                            thisGroup.maxUndo = 5400;
                                        }
                                    }

                                    // Add our new rank if we have enough info for one.
                                    // ...wait can't we just add the existing Group object already? -Jjp137
                                    _ranks.Add(new Rank(thisGroup.Permission, thisGroup.maxBlocks, thisGroup.maxUndo, thisGroup.trueName, thisGroup.color[0], thisGroup.MOTD, thisGroup.fileName));
                                }
                            }
                        }
                        // Print a warning message if a line is not in the format of "property = value".
                        else {
                            Log("In ranks.properties, the line " + s + " is wrongly formatted");
                        }
                    }
                    catch (Exception e) {
                        Log("Encountered an error at line \"" + s + "\" in ranks.properties");
                        ErrorLog(e);
                    }
                }
            }

            // The server actually expects ranks with certain permission values to be present,
            // so if a particular permission value is not present in any of the ranks, a rank with that
            // permission value is created.
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.Banned) == null) {
                _ranks.Add(new Rank(DefaultRankValue.Banned, 1, 1, "Banned", '8', String.Empty, "banned.txt"));
                Log("CREATED NEW: ranks/banned.txt");  // TODO: refactor this
            }
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.Guest) == null) {
                _ranks.Add(new Rank(DefaultRankValue.Guest, 1, 120, "Guest", '7', String.Empty, "guest.txt"));
                Log("CREATED NEW: ranks/guest.txt");  // TODO: refactor this
            }
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.Builder) == null) {
                _ranks.Add(new Rank(DefaultRankValue.Builder, 400, 300, "Builder", '2', String.Empty, "builders.txt"));
                Log("CREATED NEW: ranks/builder.txt");  // TODO: refactor this
            }
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.AdvBuilder) == null) {
                _ranks.Add(new Rank(DefaultRankValue.AdvBuilder, 1200, 900, "AdvBuilder", '3', String.Empty, "advbuilders.txt"));
                Log("CREATED NEW: ranks/advbuilders.txt");  // TODO: refactor this
            }
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.Operator) == null) {
                _ranks.Add(new Rank(DefaultRankValue.Operator, 2500, 5400, "Operator", 'c', String.Empty, "operators.txt"));
                Log("CREATED NEW: ranks/operators.txt");  // TODO: refactor this
            }
            if (_ranks.Find(grp => grp.Permission == DefaultRankValue.Admin) == null) {
                _ranks.Add(new Rank(DefaultRankValue.Admin, 65536, int.MaxValue, "SuperOP", 'e', String.Empty, "uberOps.txt"));
                Log("CREATED NEW: ranks/uberOps.txt");  // TODO: refactor this
            }
            // Create the internal "nobody" rank.
            _ranks.Add(new Rank(DefaultRankValue.Nobody, 65536, -1, "Nobody", '0', String.Empty, "nobody.txt"));

            // This looks like bubble sort...lol it's O(n^2) :\ -Jjp137
            // It seems to sort the ranks in ascending order.
            bool swap = true;
            Rank storedGroup;
            while (swap) {
                swap = false;
                for (int i = 0; i < _ranks.Count - 1; i++) {
                    if (_ranks[i].Permission > _ranks[i + 1].Permission) {
                        swap = true;
                        storedGroup = _ranks[i];
                        _ranks[i] = _ranks[i + 1];
                        _ranks[i + 1] = storedGroup;
                    }
                }
            }

            // Obtain the default rank if it is set.
            if (Find(defaultRank) != null) {
                _defaultRank = Find(defaultRank);
            }
            // Otherwise, set the default rank to be one with a permission value of 0.
            else {
                _defaultRank = FindPerm(DefaultRankValue.Guest);
            }

            // Finally, save all the created ranks to ranks.properties.
            SaveRanks();
        }
        /// <summary>
        /// Save all ranks to ranks.properties.
        /// </summary>
        public void SaveRanks() {
            // Create the file, overriding it if it exists.
            File.Create(ranksPath).Dispose();

            using (StreamWriter SW = File.CreateText(ranksPath)) {
                // Write the header
                SW.WriteLine("#Version 3");
                SW.WriteLine("#RankName = string");
                SW.WriteLine("#     The name of the rank, use capitalization.");
                SW.WriteLine("#");
                SW.WriteLine("#Permission = num");
                SW.WriteLine("#     The \"permission\" of the rank. It's a number.");
                SW.WriteLine("#     There are pre-defined permissions already set. (for the old ranks)");
                SW.WriteLine("#     Banned = -20, Guest = 0, Builder = 30, AdvBuilder = 50, Operator = 80");
                SW.WriteLine("#     SuperOP = 100, Nobody = 120");
                SW.WriteLine("#     Must be greater than -50 and less than 120");
                SW.WriteLine("#     The higher the number, the more commands do (such as undo allowing more seconds)");
                SW.WriteLine("#Limit = num");
                SW.WriteLine("#     The command limit for the rank (can be changed in-game with /limit)");
                SW.WriteLine("#     Must be greater than 0 and less than 10000000");
                SW.WriteLine("#MaxUndo = num");
                SW.WriteLine("#     The undo limit for the rank, only applies when undoing others.");
                SW.WriteLine("#     Must be greater than 0 and less than " + int.MaxValue.ToString());
                SW.WriteLine("#Color = char");
                SW.WriteLine("#     A single letter or number denoting the color of the rank");
                SW.WriteLine("#     Possibilities:");
                SW.WriteLine("#         0, 1, 2, 3, 4, 5, 6, 7, 8, 9, a, b, c, d, e, f");
                SW.WriteLine("#FileName = string.txt");
                SW.WriteLine("#     The file which players of this rank will be stored in");
                SW.WriteLine("#     It doesn't need to be a .txt file, but you may as well");
                SW.WriteLine("#     Generally a good idea to just use the same file name as the rank name");
                SW.WriteLine("#MOTD = string");
                SW.WriteLine("#     Alternate MOTD players of the rank will see when joining the server.");
                SW.WriteLine("#     Leave blank to use the server MOTD.");
                SW.WriteLine();
                SW.WriteLine();

                // Write all the properties for every rank.
                foreach (Rank grp in _ranks) {
                    // Exclude the 'nobody' rank from being saved, since it is an internal rank.
                    if (grp.name != "nobody") {
                        SW.WriteLine("RankName = " + grp.trueName);
                        SW.WriteLine("Permission = " + grp.Permission);
                        SW.WriteLine("Limit = " + grp.maxBlocks.ToString());
                        SW.WriteLine("MaxUndo = " + grp.maxUndo.ToString());
                        SW.WriteLine("Color = " + grp.color[1].ToString());
                        SW.WriteLine("MOTD = " + grp.MOTD);
                        SW.WriteLine("FileName = " + grp.fileName);
                        SW.WriteLine();
                    }
                }
            }
        }
        /// <summary>
        /// Checks to see if a rank with the given name exists.
        /// </summary>
        /// <param name="name"> The name of the rank to search for. </param>
        /// <returns> Whether the rank with the given name exists. </returns>
        public bool Exists(string name) {
            name = name.ToLower();
            return _ranks.Any(gr => gr.name == name);
        }

        // TODO: temporary?
        public bool Contains(Rank g) {
            return _ranks.Contains(g);
        }

        /// <summary>
        /// Finds the rank with the given name.
        /// </summary>
        /// <param name="name"> The name of the rank to search for. </param>
        /// <returns> The Rank object with the given name, or null if a rank with that name was not found.
        /// <seealso cref="Rank"/></returns>
        public Rank Find(string name) {
            name = name.ToLower();

            // Hardcoded values... :\ -Jjp137
            if (name == "adv") {
                name = "advbuilder";
            }
            if (name == "op") {
                name = "operator";
            }
            if (name == "super" || (name == "admin" && !Exists("admin"))) {
                name = "superop";
            }
            if (name == "noone") {
                name = "nobody";
            }

            return _ranks.FirstOrDefault(gr => gr.name == name.ToLower());
        }

        /// <summary>
        /// Find the rank with the given permission value.
        /// </summary>
        /// <param name="Perm"> The permission value to search for. </param>
        /// <returns> The Rank object with that permission value, or null if a rank with that permission value does
        /// not exist. <seealso cref="Rank"/></returns>
        public Rank FindPerm(int Perm) {
            return _ranks.FirstOrDefault(grp => grp.Permission == Perm);
        }

        /// <summary>
        /// Find the group with the given permission value as an integer.
        /// </summary>
        /// <param name="Perm"> The permission value to search for. </param>
        /// <returns> The Rank object with that permission value, or null if a rank with that permission value does
        /// not exist. <seealso cref="Rank"></returns>
        public Rank FindPermInt(int Perm) {
            return _ranks.FirstOrDefault(grp => grp.Permission == Perm);
        }

        /// <summary>
        /// Given a name of a rank, obtains its permission value.
        /// </summary>
        /// <param name="name"> The name of a rank. </param>
        /// <returns> The permission value of the rank with the given name. </returns>
        public int PermFromName(string name) {
            Rank foundGroup = Find(name);
            return foundGroup != null ? foundGroup.Permission : DefaultRankValue.Null;
        }

        /// <summary>
        /// Given a permission value, obtain the rank's name.
        /// </summary>
        /// <param name="perm"> The permission value of a rank. </param>
        /// <returns> The name of the rank with the given permission value. </returns>
        public string PermToName(int perm) {
            Rank foundGroup = FindPerm(perm);
            return foundGroup != null ? foundGroup.name : perm.ToString();
        }

        /// <summary>
        /// Find the name of the rank that the player with the given uuid is in.
        /// </summary>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <returns> The name of the rank that the player is in, or the name of the default rank if the player
        /// is not part of another rank. </returns>
        public string FindPlayer(Uuid uuid) {
            foreach (Rank grp in _ranks.Where(grp => grp.playerList.Contains(uuid))) {
                return grp.name;
            }
            return _defaultRank.name;
        }

        /// <summary>
        /// Find the rank that the player with the given uuid is in.
        /// </summary>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <returns> The rank that a player is in, or the default rank if the player is not part of another rank.
        /// <seealso cref="Rank"/></returns>
        public Rank FindPlayerRank(Uuid uuid) {
            foreach (Rank grp in _ranks.Where(grp => grp.playerList.Contains(uuid))) {
                return grp;
            }
            return _defaultRank;
        }

        /// <summary>
        /// Obtains the string representing the color of the rank that the player with the given uuid belongs to.
        /// </summary>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <returns> The string that represents the color of the player's rank. </returns>
        public string FindPlayerColor(Uuid uuid) {
            foreach (Rank grp in _ranks.Where(grp => grp.playerList.Contains(uuid))) {
                return grp.color;
            }
            return _defaultRank.color;
        }

        /// <summary>
        /// Returns a list of all the ranks on the server as a string.
        /// </summary>
        /// <param name="commaColor"> The color code of the comma. </param>
        /// <param name="skipExtra"> Whether the nobody rank should be skipped. Defaults to false. </param>
        /// <returns> The list of ranks as a string. </returns>
        public string ConcatNames(string commaColor, bool skipExtra = false) {
            string returnString = String.Empty;
            foreach (Rank grp in _ranks.Where(grp => !skipExtra || (grp.Permission >= -50 && grp.Permission < DefaultRankValue.Nobody))) {
                returnString += ", " + grp.color + grp.name + commaColor;
            }

            // Remove the color code at the end to prevent client crashes.
            returnString = returnString.Remove(returnString.Length - 2);

            // Remove the comma and the space next to it in the beginning of the string.
            return returnString.Remove(0, 2);
        }

        /// <summary>
        /// Returns a list of all the ranks' permission values as a string.
        /// </summary>
        /// <returns> The list of permission values as a string. </returns>
        public string ConcatPerms() {
            string returnString = String.Empty;

            foreach (Rank grp in _ranks) {
                returnString += ", " + grp.Permission.ToString(CultureInfo.InvariantCulture);
            }

            return returnString.Remove(0, 2);
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of ranks.
        /// </summary>
        /// <returns> An IEnumerator<Rank> object for this list of ranks. </returns>
        public IEnumerator<Rank> GetEnumerator() {
            return this._ranks.GetEnumerator();
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of ranks.
        /// </summary>
        /// <returns> An IEnumerator object for this list of ranks. </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Logs a RankList-related error message.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        private void Log(string message) {
            Logger temp = _logger;
            if (temp != null) {
                temp.Log(message);
            }
        }

        /// <summary>
        /// Logs a RankList-related exception.
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

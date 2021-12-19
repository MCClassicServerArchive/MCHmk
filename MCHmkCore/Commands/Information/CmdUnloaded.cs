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
using System.IO;
using System.Text;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /unloaded command, which displays a list of unloaded levels to the user.
    /// </summary>
    public class CmdUnloaded : Command {
        /// <summary>
        /// The list of keywords that are associated with /unloaded.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"map", "level", "lvl", "list"});

        /// <summary>
        /// Gets the name of /unloaded.
        /// </summary>
        public override string Name {
            get {
                return "unloaded";
            }
        }
        /// <summary>
        /// Gets the shortcut for /unloaded.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /unloaded belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /unloaded. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /unloaded can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /unloaded.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        /// <summary>
        /// Constructs an instance of the /unloaded command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /unloaded will belong to.
        ///  <seealso cref="Server"/></param>
        public CmdUnloaded(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /unloaded.
        /// </summary>
        /// <param name="p"> The player that used /unloaded. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            const int LevelsPerPage = 20;
            int page = 0;  // 0 = all levels

            // If given, attempt to parse the page parameter.
            if (!String.IsNullOrEmpty(args)) {
                bool success = Int32.TryParse(args, out page);
                if (!success || page <= 0) {  // Sanity check
                    Help(p);
                    return;
                }
            }

            // Generate the list of unloaded levels.
            List<string> unloadedLevels;
            try {
                unloadedLevels = GenerateUnloadedList();
            }
            catch (Exception e) {
                if (e is IOException || e is UnauthorizedAccessException ||
                    e is PathTooLongException || e is DirectoryNotFoundException) {
                    _s.logger.Log("[Warning] During /unloaded, levels directory cannot be read!");
                    p.SendMessage("An error has occurred while using /unloaded.");
                    return;
                }
                throw;
            }

            // Stop if there are no unloaded levels.
            if (unloadedLevels.Count == 0) {
                p.SendMessage("No levels are unloaded.");
                return;
            }

            // Start determining what range of levels to display.
            int startIndex = 0;
            int endIndex = 0;

            // If the page parameter is given, determine the range of levels to display.
            if (page > 0) {
                startIndex = (page - 1) * LevelsPerPage;
                endIndex = startIndex + LevelsPerPage - 1;  // Subtract 1 due to zero-indexing (0 to 24, 25 to 49, etc.)
            }

            // If no page parameter is given, display all of the levels.
            // Also, if the last page is selected and it does not have 25 levels, display
            // as many as possible.
            if (page == 0 || endIndex > unloadedLevels.Count - 1) {
                endIndex = unloadedLevels.Count - 1;
            }

            // Stop if the player specifies a page that does not exist.
            if (startIndex > endIndex) {
                // Adding 1 is basically the same as rounding up due to integer division.
                int maxPages = (unloadedLevels.Count / LevelsPerPage) + 1;
                p.SendMessage("There are only " + maxPages.ToString() + " pages.");
                return;
            }

            // Display the appropriate header.
            if (page == 0) {  // Display all levels
                p.SendMessage("Unloaded levels: &f(* = Can Visit)" + _s.props.DefaultColor + ":");
            }
            else {  // Display a particular page
                int maxPages = (unloadedLevels.Count / LevelsPerPage) + 1;
                p.SendMessage("Unloaded levels (Page " + page.ToString() + "/" + maxPages.ToString() + ")" +
                                      " &f(* = Can Visit)" + _s.props.DefaultColor + ":");
            }

            // Put together the string that will be displayed to the user and send it.
            p.SendMessage(GenerateString(p, unloadedLevels, startIndex, endIndex));
            if (endIndex - startIndex + 1 > LevelsPerPage) {  // Remind the user that pages exist.
                p.SendMessage("To view one page at a time, use &b/unloaded <1/2/3/...>");
            }
        }

        /// <summary>
        /// Generates a list containing the names of unloaded levels.
        /// </summary>
        /// <returns> The list containing the names of unloaded levels. </returns>
        private List<string> GenerateUnloadedList() {
            // Get the names of the levels that are currently loaded.
            HashSet<string> loadedLevels = new HashSet<string>();
            foreach (Level lvl in _s.levels) {
                loadedLevels.Add(lvl.name.ToLower());
            }

            // Get the names of all the files in the levels directory.
            List<string> levelFileNames;
            string levelsDir = "levels" + Path.DirectorySeparatorChar;
            try {
                levelFileNames = new List<string>(Directory.GetFiles(levelsDir));
            }
            catch (Exception) {
                throw;  // Let the caller handle it.
            }

            // From each of the filenames, determine which ones are unloaded.
            List<string> unloadedLevels = new List<string>();

            foreach (string fileName in levelFileNames) {
                // Remove the folder name.
                string levelName = fileName.ToLower().Replace(levelsDir, String.Empty);
                bool isLevelFile = false;  // Backups do not count, though.

                // Look for .mcf or .lvl files, but not .backup files.
                if (levelName.EndsWith(".mcf")) {
                    levelName = levelName.Replace(".mcf", String.Empty);
                    isLevelFile = true;
                }
                else if (levelName.EndsWith(".lvl")) {
                    levelName = levelName.Replace(".lvl", String.Empty);
                    isLevelFile = true;
                }

                // Check that the level is not loaded and that the level is not
                // already in the unloaded list. The latter is for cases where
                // there's both a .lvl and an .mcf file.
                if (isLevelFile && !loadedLevels.Contains(levelName) &&
                    !unloadedLevels.Contains(levelName)) {
                    unloadedLevels.Add(levelName);
                }
            }
            return unloadedLevels;
        }

        /// <summary>
        /// Generates the string that will be sent to the player.
        /// </summary>
        /// <param name="p"> The player that ran the command. <seealso cref="Player"/></param>
        /// <param name="unloadedLevels"> A list of the names of unloaded levels, as generated
        /// by GenerateUnloadedList(). </param>
        /// <param name="start"> The index to start from in the list. </param>
        /// <param name="end"> The index to end at. Note that the range is inclusive. </param>
        /// <returns> The string describing the unloaded levels. </returns>
        private string GenerateString(Player p, List<string> unloadedLevels, int start, int end) {
            // A StringBuilder is faster if a string will be appended to multiple times.
            StringBuilder unloadedString = new StringBuilder();
            for (int i = start; i <= end; i++) {
                // Obtain the level name.
                string currentLevel = unloadedLevels[i];

                // Initialize some defaults in case the below operations fail.
                Rank buildRank = null;
                Rank visitRank = null;
                bool loadOnGoto = false;

                // Obtain the perbuild, pervisit, and load-on-goto properties for each level.
                try {
                    buildRank = _s.ranks.Find(Level.GetLevelProperty(currentLevel, "perbuild"));
                    visitRank = _s.ranks.Find(Level.GetLevelProperty(currentLevel, "pervisit"));
                    // The ?? operator means that if the left operand is null, then the right operand is used instead.
                    loadOnGoto = Boolean.Parse(Level.GetLevelProperty(currentLevel, "loadongoto") ?? "false");
                }
                catch (IOException) {
                    _s.logger.Log("[Warning] During /unloaded, a level properties file cannot be read.");
                    _s.logger.Log("[Warning] The level is: " + currentLevel);
                }
                catch (FormatException) {
                    _s.logger.Log("[Warning] A load-on-goto value in a level properties file is invalid.");
                    _s.logger.Log("[Warning] The level is: " + currentLevel);
                }

                // Use the obtained information to determine how the level will be displayed to the player.
                string buildColor = buildRank != null ? buildRank.color : "&7";
                int visitPerm = visitRank != null ? visitRank.Permission : DefaultRankValue.Guest;
                string canVisit = loadOnGoto && p.rank.Permission >= visitPerm ? "&f*" : String.Empty;

                // Put the string for that particular level together.
                unloadedString.Append(", " + buildColor + currentLevel + canVisit + _s.props.DefaultColor);
            }
            // Remove the comma that is in the beginning of the string.
            unloadedString.Remove(0, 2);
            return unloadedString.ToString();
        }

        /// <summary>
        /// Called when /help is used on /unloaded.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/unloaded - Lists all unloaded levels.");
            p.SendMessage("/unloaded <1/2/3/...> - Shows a page of the list.");
            p.SendMessage("Each level is colored according to their perbuild rank.");
            p.SendMessage("An asterisk appears next to levels that you can visit.");
        }
    }
}

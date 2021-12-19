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

namespace MCHmk {
    /// <summary>
    /// The LevelList class represents a collection of loaded levels.
    /// </summary>
    public class LevelList : IEnumerable<Level> {
        /// <summary>
        /// The list of loaded levels.
        /// </summary>
        private List<Level> _levels;
        /// <summary>
        /// The maximum number of levels that can be loaded at once.
        /// </summary>
        private int _limit;

        /// <summary>
        /// Gets the Level object at the specified index.
        /// </summary>
        public Level this[int i] {
            get {
                return _levels[i];
            }
        }

        /// <summary>
        /// Gets the number of loaded levels.
        /// </summary>
        public int Count {
            get {
                return _levels.Count;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of levels that can be loaded at once.
        /// </summary>
        public int MaxLevels {
            get {
                return _limit;
            }
            set {
                _limit = value;
            }
        }

        /// <summary>
        /// Constructs a new LevelList instance.
        /// </summary>
        /// <param name="maxLevels"> The maximum number of levels that the list should hold. </param>
        public LevelList(int maxLevels) {
            _levels = new List<Level>(maxLevels);
            _limit = maxLevels;
        }

        /// <summary>
        /// Adds a level to the list.
        /// </summary>
        /// <param name="lvl"> The level to add. </param>
        public void Add(Level lvl) {
            _levels.Add(lvl);
        }

        /// <summary>
        /// Removes a level from the list.
        /// </summary>
        /// <param name="lvl"> The level to remove. </param>
        public void Remove(Level lvl) {
            _levels.Remove(lvl);
        }

        /// <summary>
        /// Given a name, finds a level with that name. Performs partial matches as well.
        /// </summary>
        /// <param name="levelName"> The name of the level to find. Can be a partial name. </param>
        /// <returns> A Level with the given name, or null if a level with that name was not found or
        /// if there are two or more levels that contain the provided string in their names.
        /// <seealso cref="Level"/></returns>
        public Level Find(string levelName) {
            Level tempLevel = null;
            bool returnNull = false;

            foreach (Level level in _levels) {
                // Exact matches have first priority and end the search immediately.
                if (level.name.ToLower() == levelName) {
                    return level;
                }
                // Partial matches are checked next.
                if (level.name.ToLower().IndexOf(levelName.ToLower(), System.StringComparison.Ordinal) == -1) {
                    continue;
                }

                // If there is only one level that contains the string in their name, return that level.
                // However, if two or more levels contain that string in their names, return null since
                // the match is ambiguous.
                if (tempLevel == null) {
                    tempLevel = level;
                }
                else {
                    returnNull = true;
                }
            }

            return returnNull ? null : tempLevel;
        }

        /// <summary>
        /// Given a name, finds a level that has that exact name.
        /// </summary>
        /// <param name="levelName"> The name to search for. </param>
        /// <returns> The level with that name, or null if no level with that name can be found.
        /// <seealso cref="Level"/></returns>
        public Level FindExact(string levelName) {
            return _levels.Find(lvl => levelName.ToLower() == lvl.name.ToLower());
        }

        /// <summary>
        /// Performs an action on each Level object.
        /// </summary>
        /// <param name="action"> The function to perform on each Level object. </param>
        public void ForEach(Action<Level> action) {
            _levels.ForEach(action);
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of levels.
        /// </summary>
        /// <returns> An IEnumerator object for this list of levels. </returns>
        public IEnumerator<Level> GetEnumerator() {
            return _levels.GetEnumerator();
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of levels.
        /// </summary>
        /// <returns> An IEnumerator object for this list of levels. </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}


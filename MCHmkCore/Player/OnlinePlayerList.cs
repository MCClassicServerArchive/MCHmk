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
    /// The OnlinePlayerList class represents the list of online players.
    /// </summary>
    public class OnlinePlayerList : IEnumerable<Player> {
        /// <summary>
        /// The list of players on the server.
        /// </summary>
        private List<Player> _players;

        /// <summary>
        /// Gets the Player object at the specified index.
        /// </summary>
        public Player this[int i] {
            get {
                return _players[i];
            }
        }

        /// <summary>
        /// Gets the number of online players.
        /// </summary>
        public int Count {
            get {
                return _players.Count;
            }
        }

        /// <summary>
        /// Constructs a new OnlinePlayerList object.
        /// </summary>
        public OnlinePlayerList() { 
            _players = new List<Player>();
        }

        /// <summary>
        /// Adds a player to the list.
        /// </summary>
        /// <param name="p"> The player to add. </param>
        public void Add(Player p) {
            _players.Add(p);
        }

        /// <summary>
        /// Removes a player from the list.
        /// </summary>
        /// <param name="p"> The player to remove. </param>
        public void Remove(Player p) {
            _players.Remove(p);
        }

        /// <summary>
        /// Checks whether the given player is in the list.
        /// </summary>
        /// <param name="p"> The player to check for. </param>
        /// <returns> Whether the player is in the list. </returns>
        public bool Contains(Player p) {
            return _players.Contains(p);
        }

        /// <summary>
        /// Checks if there is a player with the given name.
        /// </summary>
        /// <param name="name"> The name to check for. </param>
        /// <returns> Whether a player with that name is on the server. </returns>
        public bool Exists(string name) {
            foreach (Player p in _players) {
                if (p.name.ToLower() == name.ToLower()) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if there is a player with the given player id on the server.
        /// </summary>
        /// <param name="id"> The player id to check for. </param>
        /// <returns> Whether a player with that id is on the server. </returns>
        public bool Exists(byte id) {
            foreach (Player p in _players) {
                if (p.serverId == id) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds a player in the player list.
        /// </summary>
        /// <param name="name"> The name of the player to find. It could be an incomplete name. </param>
        /// <returns> The player that was found, or null if no player was found. <seealso cref="Player"/></returns>
        public Player Find(string name) {
            List<Player> tempList = new List<Player>();
            tempList.AddRange(_players);
            Player tempPlayer = null;
            bool returnNull = false;

            foreach (Player p in tempList) {
                // Exact match.
                if (p.name.ToLower() == name.ToLower()) {
                    return p;
                }
                // Inexact match.
                if (p.name.ToLower().IndexOf(name.ToLower()) != -1) {
                    // This if-else block is how the auto-complete for inexact names does not
                    // match two or more players. It prevents the method from returning any player
                    // if there is ambiguity. The first match is set to the tempPlayer variable,
                    // and if another match is found, the method is forced to return null so that
                    // this method does not have to guess.
                    if (tempPlayer == null) {
                        tempPlayer = p;
                    }
                    else {
                        returnNull = true;
                    }
                }
            }

            if (returnNull == true) {
                return null;
            }
            if (tempPlayer != null) {
                return tempPlayer;
            }
            return null;
        }

        /// <summary>
        /// Returns the list of online players as an array.
        /// </summary>
        /// <returns> The array containing online players. </returns>
        public Player[] ToArray() {
            return _players.ToArray();
        }


        public void ForEach(Action<Player> action) {
            _players.ForEach(action);
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of online players.
        /// </summary>
        /// <returns> An IEnumerator<Player> object for this list of online players. </returns>
        public IEnumerator<Player> GetEnumerator() {
            return _players.GetEnumerator();
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of online players.
        /// </summary>
        /// <returns> An IEnumerator object for this list of online players. </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}


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
using System;
using System.Collections.Generic;
using System.IO;

namespace MCHmk {
    /// <summary>
    /// The Awards class, which contains methods that manipulate award-related data.
    /// </summary>
    public class Awards {

        /// <summary>
        /// The playerAwards structure, which contains information about an individual player's awards.
        /// </summary>
        public struct playerAwards {
            /// <summary>
            /// The uuid of the player who owns the awards.
            /// </summary>
            public string uuid;
            /// <summary>
            /// The player who owns the awards.
            /// </summary>
            public string playerName;
            /// <summary>
            /// The list of awards the player has obtained.
            /// </summary>
            public List<string> awards;
        }

        /// <summary>
        /// The awardData sub-class, which contains information about an award.
        /// </summary>
        public class awardData {
            /// <summary>
            /// The name of the award.
            /// </summary>
            public string awardName;
            /// <summary>
            /// The description of the award.
            /// </summary>
            public string description;
            /// <summary>
            /// Changes the name of an award.
            /// </summary>
            /// <param name="name"> The new name of the award. </param>
            public void setAward(string name) {
                awardName = camelCase(name);
            }
        }

        /// <summary>
        /// The list of every player and their awards.
        /// </summary>
        public static List<Awards.playerAwards> playersAwards = new List<Awards.playerAwards>();
        /// <summary>
        /// The list of every award on the server.
        /// </summary>
        public static List<Awards.awardData> allAwards = new List<Awards.awardData>();

        /// <summary>
        /// The path to the file containing information about each award.
        /// </summary>
        private static readonly string awardsPath = Path.Combine("text", "awardsList.txt");

        /// <summary>
        /// The path to the file containing information about awards that players have earned.
        /// </summary>
        private static readonly string playersPath = Path.Combine("text", "playerAwards.txt");

        /// <summary>
        /// Loads all awards from the awardsList.txt file.
        /// </summary>
        public static void Load() {
            // Create the default file if it doesn't exist.
            if (!File.Exists(awardsPath)) {
                using (StreamWriter SW = File.CreateText(awardsPath)) {
                    SW.WriteLine("#This is a full list of awards. The server will load these and they can be awarded as you please");
                    SW.WriteLine("#Format is:");
                    SW.WriteLine("# awardName : Description of award goes after the colon");
                    SW.WriteLine();
                    SW.WriteLine("Gotta start somewhere : Built your first house");
                    SW.WriteLine("Climbing the ladder : Earned a rank advancement");
                    SW.WriteLine("Do you live here? : Joined the server a huge bunch of times");
                }
            }

            allAwards = new List<awardData>(); // What's the point of doing it again? -Jjp137

            // Parse every line and add the award to the allAwards list.
            foreach (string s in File.ReadAllLines(awardsPath)) {
                if (s == String.Empty || s[0] == '#') {
                    continue;
                }
                if (s.IndexOf(" : ") == -1) {
                    continue;
                }

                awardData aD = new awardData();

                aD.setAward(s.Split(new string[] { " : " }, StringSplitOptions.None)[0]);
                aD.description = s.Split(new string[] { " : " }, StringSplitOptions.None)[1];

                allAwards.Add(aD);
            }

            // Create the list of players with awards.
            playersAwards = new List<playerAwards>();

            // Parse playerAwards.txt, which contains the list of awards that each player has earned.
            if (File.Exists(playersPath)) {
                foreach (String s in File.ReadAllLines(playersPath)) {
                    if (s.IndexOf(" : ") == -1) {
                        continue;
                    }

                    playerAwards pA; // Temporary variable

                    // Format of each line - Uuid : Name : List of comma-separated awards
                    pA.uuid = s.Split(new string[] { " : " }, StringSplitOptions.None)[0];
                    pA.playerName = s.Split(new string[] { " : " }, StringSplitOptions.None)[1].ToLower();
                    string myAwards = s.Split(new string[] { " : " }, StringSplitOptions.None)[2];

                    pA.awards = new List<string>();
                    if (myAwards.IndexOf(',') != -1) // If there are many awards...
                        foreach (string a in myAwards.Split(',')) { // ...separated by commas
                            pA.awards.Add(camelCase(a));
                        }
                    else if (myAwards.Trim() != String.Empty) { // There might be only one award
                        pA.awards.Add(camelCase(myAwards));
                    }

                    // Add the list of this player's awards to the global list
                    playersAwards.Add(pA);
                }
            }

            Save(); // How many times have I seen this pattern...? -Jjp137
        }

        /// <summary>
        /// Saves awardsList.txt and playerAwards.txt.
        /// </summary>
        public static void Save() {
            using (StreamWriter SW = File.CreateText(awardsPath)) {
                // The usual header.
                SW.WriteLine("#This is a full list of awards. The server will load these and they can be awarded as you please");
                SW.WriteLine("#Format is:");
                SW.WriteLine("# awardName : Description of award goes after the colon");
                SW.WriteLine();
                // A line for every award.
                foreach (awardData aD in allAwards) {
                    SW.WriteLine(camelCase(aD.awardName) + " : " + aD.description);
                }
            }
            // A line for every player.
            using (StreamWriter SW = File.CreateText(playersPath)) {
                foreach (playerAwards pA in playersAwards) {
                    SW.WriteLine(pA.uuid + " : " + pA.playerName.ToLower() + " : " + string.Join(",", pA.awards.ToArray()));
                }
            }
        }

        /// <summary>
        /// Gives an award to the player.
        /// </summary>
        /// <param name="uuid"> The uuid of the player that the award is being given to. <seealso cref="Uuid"/></param>
        /// <param name="playerName"> The name of the player to give the award to. </param>
        /// <param name="awardName"> The name of the award. </param>
        /// <returns> Whether the player was given the award. </returns>
        public static bool giveAward(Uuid uuid, string playerName, string awardName) {
            foreach (playerAwards pA in playersAwards) {
                if (pA.uuid == uuid.Value) { // The player has at least one award
                    if (pA.awards.Contains(camelCase(awardName))) { // Don't add the same one twice
                        return false;
                    }
                    pA.awards.Add(camelCase(awardName));
                    return true;
                }
            }

            // Create a new list for that player if he/she hasn't earned an award yet, and include
            // the award that player just obtained.
            playerAwards newPlayer;
            newPlayer.uuid = uuid.Value;
            newPlayer.playerName = playerName.ToLower();
            newPlayer.awards = new List<string>();
            newPlayer.awards.Add(camelCase(awardName));
            playersAwards.Add(newPlayer);
            return true;
        }

        /// <summary>
        /// Takes an award away from the player.
        /// </summary>
        /// <param name="uuid"> The uuid of the player that the award is being taken from. <seealso cref="Uuid"/></param>
        /// <param name="playerName"> The name of the player to take the award from. </param>
        /// <param name="awardName"> The name of the award.  </param>
        /// <returns> Whether taking the award away succeeded. </returns>
        public static bool takeAward(Uuid uuid, string playerName, string awardName) {
            foreach (playerAwards pA in playersAwards) {
                if (pA.uuid == uuid.Value) {
                    if (!pA.awards.Contains(camelCase(awardName))) {
                        return false;
                    }
                    pA.awards.Remove(camelCase(awardName));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Given a uuid, obtains a list of a player's awards.
        /// </summary>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <returns> The list of all the awards the player earned. </returns>
        public static List<string> GetAwards(Uuid uuid) {
            foreach (playerAwards pA in playersAwards) {
                if (pA.uuid == uuid.Value) {
                    return pA.awards;
                }
            }

            return new List<string>();
        }

        /// <summary>
        /// Obtains an award's description.
        /// </summary>
        /// <param name="awardName"> The name of the award. </param>
        /// <returns> The description of the award. </returns>
        public static string getDescription(string awardName) {
            foreach (awardData aD in allAwards)
                if (camelCase(aD.awardName) == camelCase(awardName)) {
                    return aD.description;
                }

            return String.Empty;
        }

        /// <summary>
        /// Obtains the percentage of awards a person has obtained as a string.
        /// </summary>
        /// <param name="playerName"> The name of the player. </param>
        /// <returns> A string statement that reports the percentage of awards a person has earned. </returns>
        public static string awardAmount(string playerName) {
            foreach (playerAwards pA in playersAwards)
                if (pA.playerName == playerName.ToLower())
                    return "&f" + pA.awards.Count.ToString() + "/" + allAwards.Count.ToString() + " (" +
                           Math.Round((double)((double)pA.awards.Count / allAwards.Count) * 100, 2).ToString() +
                           "%)";

            // Assume that the player earned no awards if he/she can't be found in the list
            return "&f0/" + allAwards.Count.ToString() + " (0%)";
        }

        /// <summary>
        /// Adds a new award to the server's list.
        /// </summary>
        /// <param name="awardName"> The name of the award. </param>
        /// <param name="awardDescription"> The description of the award. </param>
        /// <returns> Whether adding the award succeeded. </returns>
        public static bool addAward(string awardName, string awardDescription) {
            if (awardExists(awardName)) { // Don't add two awards with the same name.
                return false;
            }

            awardData aD = new awardData();
            aD.awardName = camelCase(awardName);
            aD.description = awardDescription;
            allAwards.Add(aD);
            return true;
        }

        /// <summary>
        /// Removes an award from the server's list.
        /// </summary>
        /// <param name="awardName"> The name of the award. </param>
        /// <returns> Whether the removal succeeded. </returns>
        public static bool removeAward(string awardName) {
            foreach (awardData aD in allAwards) {
                if (camelCase(aD.awardName) == camelCase(awardName)) {
                    allAwards.Remove(aD);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a given award is actually in the server's list.
        /// </summary>
        /// <param name="awardName"> The name of the award.  </param>
        /// <returns> Whether it was in the list. </returns>
        public static bool awardExists(string awardName) {
            foreach (awardData aD in allAwards)
                if (camelCase(aD.awardName) == camelCase(awardName)) {
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Capitalizes the first letter of each word in an award's name.
        /// </summary>
        /// <param name="givenName"> The award's name. </param>
        /// <returns> A 'corrected' version of the award's name. </returns>
        public static string camelCase(string givenName) { // Worst-named method ever. -Jjp137
            string returnString = String.Empty; // Temporary variable
            if (givenName != String.Empty) {
                foreach (string s in givenName.Split(' ')) {
                    if (s.Length > 1) {
                        returnString += s[0].ToString().ToUpper() + s.Substring(1).ToLower() + " ";
                    }
                    else {
                        returnString += s.ToUpper() + " ";
                    }
                }
            }
            return returnString.Trim(); // Get rid of whitespace
        }
    }
}

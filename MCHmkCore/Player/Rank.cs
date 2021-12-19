/*
    Copyright 2016 Jjp137

    This file have been changed from the original source code by MCForge.

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

namespace MCHmk {
    /// <summary>
    /// The Rank class represents a rank on the server and the players that belong to it.
    /// </summary>
    public class Rank {
        /// <summary>
        /// The name of the rank, but in lowercase.
        /// </summary>
        public string name;
        /// <summary>
        /// The name of the rank.
        /// </summary>
        public string trueName;
        /// <summary>
        /// Default color of players that have that rank.
        /// </summary>
        public string color;
        /// <summary>
        /// The rank's permission value.
        /// </summary>
        public int Permission;
        /// <summary>
        /// The rank's block limit for commands.
        /// </summary>
        public int maxBlocks;
        /// <summary>
        /// The rank's undo limit.
        /// </summary>
        public long maxUndo;
        /// <summary>
        /// The text file where all players of that rank are stored.
        /// </summary>
        public string fileName;
        /// <summary>
        /// The list of players, both offline and online, with that rank.
        /// </summary>
        public UuidList playerList;
        /// <summary>
        /// The rank-specific message of the day.
        /// </summary>
        public string MOTD = String.Empty;

        /// <summary>
        /// Constructs an empty Rank object.
        /// </summary>
        public Rank() {
            Permission = DefaultRankValue.Null;
        }

        /// <summary>
        /// Constructs a Rank object with the given parameters.
        /// </summary>
        /// <param name="Perm"> The rank's permission value. </param>
        /// <param name="maxB"> The rank's command block limit. </param>
        /// <param name="maxUn"> The rank's maximum undo limit. </param>
        /// <param name="fullName"> The rank's name. </param>
        /// <param name="newColor"> The color associated with that rank. Members of that rank have that color 
        /// by default. Exclude the & or % that is normally there. </param>
        /// <param name="motd"> The custom MOTD that those who belong the rank see when logging in. </param>
        /// <param name="file"> The file where members of that rank are listed. </param>
        public Rank(int Perm, int maxB, long maxUn, string fullName, char newColor, string motd, string file) {
            Permission = Perm;
            maxBlocks = maxB;
            maxUndo = maxUn;
            trueName = fullName;
            name = trueName.ToLower();
            color = "&" + newColor.ToString();
            MOTD = motd;
            fileName = file;
            playerList = name != "nobody" ? UuidList.Load(fileName, this) : new UuidList();
        }
    }
}

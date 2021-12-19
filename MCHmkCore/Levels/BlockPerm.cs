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
using System.Collections.Generic;

namespace MCHmk {
    /// <summary>
    /// The Blocks class associates a block type with its permission values.
    /// </summary>
    public class BlockPerm {
        /// <summary>
        /// The type of block that the permission values are for.
        /// </summary>
        public BlockId type;
        /// <summary>
        /// The lowest permission value required to place the block.
        /// </summary>
        public int lowestRank;
        /// <summary>
        /// Specific permission values that are not allowed to place the block,
        /// regardless of whether these values are above the minimum value required.
        /// </summary>
        public List<int> disallow = new List<int>();
        /// <summary>
        /// Specific permission values that can place the block, regardless of
        /// whether those values are above the minimum value required.
        /// </summary>
        public List<int> allow = new List<int>();

        /// <summary>
        /// Constructs a BlockPerm object.
        /// </summary>
        public BlockPerm() { }

        /// <summary>
        /// Returns whether a certain block and its permission values should be saved in blocks.properties.
        /// </summary>
        /// <returns> Whether the block should be included in blocks.properties. </returns>
        internal bool IncludeInBlockProperties() {
            // Exclude all blocks that are invalid.
            if (BlockData.Name(type).ToLower() == "unknown") {
                return false;
            }

            // Exclude the CTF flag base.
            if (type == BlockId.FlagBase) {
                return false;
            }

            // Exclude all odoor_air.
            if (type >= BlockId.ODoorTreeWoodActive && type <= BlockId.ODoorWoodPlanksActive) {
                return false;
            }

            if (type >= BlockId.ODoorGreenActive && type <= BlockId.ODoorWaterActive) {
                return false;
            }

            // Otherwise, save that block in block.properties.
            return true;
        }
    }
}

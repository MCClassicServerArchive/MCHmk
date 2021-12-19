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

namespace MCHmk.Drawing {
    /// <summary>
    /// The Measurement class contains functions related to measuring shapes.
    /// </summary>
    public static class Measurement {
        /// <summary>
        /// Calculates the length of a line made out of blocks.
        /// </summary>
        /// <remarks>
        /// Note that a line from (1, 1, 1) to (1, 1, 1) is one block long, not zero, so
        /// if both values are the same, this function will return one.
        /// </remarks>
        /// <param name="a"> The starting coordinate. </param>
        /// <param name="b"> The ending coordinate. </param>
        /// <returns> The length of the line, in blocks. </returns>
        public static int LineBlockLen(int a, int b) {
            // FIXME: Can overflow if int.MinValue and int.MaxValue are the provided arguments.
            return Math.Abs(b - a) + 1;
        }

        /// <summary>
        /// Calculates the length of a line made out of blocks.
        /// </summary>
        /// <remarks>
        /// Note that a line from (1, 1, 1) to (1, 1, 1) is one block long, not zero, so
        /// if both values are the same, this function will return one.
        /// </remarks>
        /// <param name="a"> The starting coordinate. </param>
        /// <param name="b"> The ending coordinate. </param>
        /// <returns> The length of the line, in blocks. </returns>
        public static ushort LineBlockLen(ushort a, ushort b) {
            // FIXME: Can overflow if ushort.MinValue and ushort.MaxValue are the provided arguments.
            return (ushort)(Math.Abs(b - a) + 1);
        }

        /// <summary>
        /// Determines the area, in blocks, of the selection defined by the provided corners.
        /// </summary>
        /// <remarks>
        /// This is an alias of the equivalent Cuboids.SolidCount() call, and this should be used if
        /// the only intent of the measurement was to check the area of the selection.
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks within the given area. </returns>
        public static int SelectionCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            return Cuboids.SolidCount(x1, y1, z1, x2, y2, z2);
        }
    }
}


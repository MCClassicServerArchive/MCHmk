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
using System.Collections.Generic;

namespace MCHmk.Drawing {
    /// <summary>
    /// The Lines static class contains a collection of functions related to the drawing of lines.
    /// </summary>
    public static class Lines {
        /// <summary>
        /// The longest possible length for a line.
        /// </summary>
        public static readonly ushort MaxLimit = 2048;

        /// <summary>
        /// Given two positions, calculates the number of blocks that a line will consist of.
        /// </summary>
        /// <param name="flags"> Additional options for the line to be drawn. <seealso cref="LineFlags"/></param>
        /// <param name="limit"> The maximum length of the line. Pass Lines.MaxLimit if no limit is desired. </param>
        /// <param name="x1"> The x-coordinate of the first end of the line. </param>
        /// <param name="y1"> The y-coordinate of the first end of the line. </param>
        /// <param name="z1"> The z-coordinate of the first end of the line. </param>
        /// <param name="x2"> The x-coordinate of the second end of the line. </param>
        /// <param name="y2"> The y-coordinate of the second end of the line. </param>
        /// <param name="z2"> The z-coordinate of the second end of the line. </param>
        /// <returns> The number of blocks that the line will consist of. </returns>
        public static int CountBlocks(LineFlags flags, int limit, int x1, int y1, int z1, int x2, int y2, int z2) {
            // Get the lengths of each axis and figure out which one is the longest, as that determines the
            // axis that the line is most aligned with.
            int xLen = Measurement.LineBlockLen(x1, x2);
            int yLen = Measurement.LineBlockLen(y1, y2);
            int zLen = Measurement.LineBlockLen(z1, z2);
            int longest = Math.Max(xLen, Math.Max(yLen, zLen));

            // If a limit is specified, reduce its length if necessary.
            int result = Math.Min(limit, longest);

            // If the line is a tall wall, then the blocks needed to make the line a wall needs to be added.
            if (flags.HasFlag(LineFlags.Wall)) {
                // Non-straight vertical lines should be multipled by the length of the second-longest axis
                // instead. It's easier to think of a wall as a series of columns that mainly move along the
                // x-axis or z-axis.
                if (longest == yLen && !flags.HasFlag(LineFlags.Straight)) {
                    result *= Math.Max(xLen, zLen);
                }
                // Otherwise, multiply the length of the line by the height of the wall if it isn't already a
                // straight vertical line. If the line is a straight vertical line, then the length of the line
                // is the same as its height, so don't adjust the total.
                else if (longest != yLen) {
                    result *= yLen;
                }
            }

            // And we're done.
            return result;
        }

        /// <summary>
        /// Generates a line.
        /// </summary>
        /// <remarks>
        /// This function should be called in most scenarios if a line needs to be drawn.
        ///
        /// Lines are a subset of cuboids. Like a cuboid, a line in MCHmk is an IEnumerable<UShortCoords>
        /// that can be iterated through to return one block at a time, which is represented as a set of coordinates.
        /// Usually, the coordinates are passed to another method that actually places the block.
        /// </remarks>
        /// <param name="flags"> Additional options for the line to be drawn. <seealso cref="LineFlags"/></param>
        /// <param name="limit"> The maximum length of the line. Pass Lines.MaxLimit if no limit is desired. </param>
        /// <param name="lvl"> The level that the pyramid is being drawn on. <seealso cref="Level"/></param>
        /// <param name="block"> The type of block that the pyramid is using. <seealso cref="BlockId"/></param>
        /// <param name="x1"> The x-coordinate of the first end of the line. </param>
        /// <param name="y1"> The y-coordinate of the first end of the line. </param>
        /// <param name="z1"> The z-coordinate of the first end of the line. </param>
        /// <param name="x2"> The x-coordinate of the second end of the line. </param>
        /// <param name="y2"> The y-coordinate of the second end of the line. </param>
        /// <param name="z2"> The z-coordinate of the second end of the line. </param>
        /// <param name="filterSame"> Whether blocks that are already the same as the block type being
        /// drawn should be skipped. Normally, this should be true to save bandwidth. Default: true. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the line. </returns>
        public static IEnumerable<UShortCoords> GenerateLine(LineFlags flags, ushort limit, Level lvl, BlockId block,
                ushort x1, ushort y1, ushort z1, ushort x2, ushort y2, ushort z2, bool filterSame = true) {
            bool isWall = flags.HasFlag(LineFlags.Wall);
            bool makeStraight = flags.HasFlag(LineFlags.Straight);

            foreach (UShortCoords pos in makeStraight ? StraightLineImpl(x1, y1, z1, x2, y2, z2, limit, isWall) :
                                                        NormalLineImpl(x1, y1, z1, x2, y2, z2, limit, isWall)) {
                // Unless otherwise instructed, don't draw blocks that are already the same block type
                // as the provided one.
                if (!filterSame || lvl.GetTile(pos.X, pos.Y, pos.Z) != block) {
                    yield return pos;
                }
            }
        }

        /// <summary>
        /// Generates a normal line.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first end of the line. </param>
        /// <param name="y1"> The y-coordinate of the first end of the line. </param>
        /// <param name="z1"> The z-coordinate of the first end of the line. </param>
        /// <param name="x2"> The x-coordinate of the second end of the line. </param>
        /// <param name="y2"> The y-coordinate of the second end of the line. </param>
        /// <param name="z2"> The z-coordinate of the second end of the line. </param>
        /// <param name="limit"> The maximum length of the line. Pass Lines.MaxLimit if no limit is desired. </param>
        /// <param name="isWall"> Whether the line should be a wall. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the line. </returns>
        private static IEnumerable<UShortCoords> NormalLineImpl(ushort x1, ushort y1, ushort z1,
                ushort x2, ushort y2, ushort z2, ushort limit, bool isWall) {
            // Calculate the difference from one point to the next on each axis. Note that this is
            // done without considering that a line from (1, 1, 1) to (1, 1, 1) is one block long, not zero.
            // This will be taken into account during the actual for-loop.
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dz = z2 - z1;

            // Store their absolute values separately because we need the signed values later in
            // order to determine the direction of the line.
            ushort xLen = (ushort)Math.Abs(dx);
            ushort yLen = (ushort)Math.Abs(dy);
            ushort zLen = (ushort)Math.Abs(dz);

            // Figure out the longest length among the three axes and use that as the number of
            // blocks, or columns of blocks if the line is a wall, to place.
            ushort[] lengths = {xLen, yLen, zLen};
            Array.Sort(lengths);
            Array.Reverse(lengths);
            ushort mainLen = lengths[0];

            // If the longest axis is the y-axis and the line is a wall, it's easier to think of this
            // line as being along the other axes instead, so pick the second-longest length. It doesn't
            // matter if it's the x-axis or z-axis.
            if (mainLen == yLen && isWall) {
                mainLen = lengths[1];
            }

            // For this section, note that a line in three-dimensional space can be expressed as:
            //
            // <x, y, z> = <x0, y0, z0> + t<mx, my, mz>
            //
            // where <x0, y0, z0> is the first point and <mx, my, mz> is the difference between the second
            // point and the first point. t = 0 leads to the first end of the line, and t = 1 leads to the other
            // end of the line. Let n be the 'mainLen' variable. We need n + 1 points in order to from a contiguous
            // line of blocks. If t starts at 0 and is incremented by 1.0 / n each iteration until it reaches 1, we
            // will get the position of every block along the line.
            //
            // Special case: if mainLen is 0, division by zero occurs, but it's known that one block should be
            // placed in this case since a line from (1, 1, 1) to (1, 1, 1) is one block long, not zero.
            double tInc = mainLen == 0 ? 0 : 1.0 / mainLen;
            double curX = x1, curY = y1, curZ = z1;

            for (int i = 0; i < mainLen + 1; i++) {  // +1 so that the last block is included
                // If a limit is specified and we've reached it, don't place any more blocks.
                if (i == limit) {
                    yield break;
                }

                // Use truncated integers as the coordinates since there are no fractional block positions.
                ushort newX = (ushort)Math.Truncate(curX);
                ushort newY = (ushort)Math.Truncate(curY);
                ushort newZ = (ushort)Math.Truncate(curZ);

                // If the line is a wall, place a column of blocks. Otherwise, place a single block.
                if (isWall) {
                    ushort wallY = y1;
                    for (int j = 0; j < yLen + 1; j++) {  // +1 for the same reason
                        yield return new UShortCoords(newX, wallY, newZ);
                        wallY = (ushort)(wallY + Math.Sign(dy) * 1);
                    }
                }
                else {
                    yield return new UShortCoords(newX, newY, newZ);
                }

                // Figure out where the next block will be placed by incrementing t, and therefore
                // the current coordinates, by the previously calculated interval.
                curX += tInc * dx;
                if (!isWall) {
                    curY += tInc * dy;
                }
                curZ += tInc * dz;
            }
        }

        /// <summary>
        /// Generates a line that is corrected so that it is straight.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first end of the line. </param>
        /// <param name="y1"> The y-coordinate of the first end of the line. </param>
        /// <param name="z1"> The z-coordinate of the first end of the line. </param>
        /// <param name="x2"> The x-coordinate of the second end of the line. </param>
        /// <param name="y2"> The y-coordinate of the second end of the line. </param>
        /// <param name="z2"> The z-coordinate of the second end of the line. </param>
        /// <param name="limit"> The maximum length of the line. Pass Lines.MaxLimit if no limit is desired. </param>
        /// <param name="isWall"> Whether the line should be a wall. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the line. </returns>
        private static IEnumerable<UShortCoords> StraightLineImpl(ushort x1, ushort y1, ushort z1,
                ushort x2, ushort y2, ushort z2, ushort limit, bool isWall) {
            // Get the lengths of each axis.
            ushort xLen = Measurement.LineBlockLen(x1, x2);
            ushort yLen = Measurement.LineBlockLen(y1, y2);
            ushort zLen = Measurement.LineBlockLen(z1, z2);

            IEnumerable<UShortCoords> line = null;
            // If the line is a wall, then the second y-coordinate should remain the same so that
            // the height of the wall can be determined. Otherwise, force the line to be parallel
            // to the y-axis by setting the second y-coordinate to be equal to the first.
            ushort yEnd = isWall ? y2 : y1;

            // Based on which axis is the longest, create a line parallel to that axis. This is
            // done by passing coordinates such that the lengths of the other axes turns out to be 1,
            // although this is not done for the y-axis if the line is a wall.
            if (xLen > yLen && xLen > zLen) {
                line = Lines.NormalLineImpl(x1, y1, z1, x2, yEnd, z1, limit, isWall);
            }
            else if (yLen > xLen && yLen > zLen) {
                // A straight vertical line and the equivalent wall is the same, so don't use the
                // yEnd variable here.
                line = Lines.NormalLineImpl(x1, y1, z1, x1, y2, z1, limit, isWall);
            }
            else if (zLen > xLen && zLen > yLen) {
                line = Lines.NormalLineImpl(x1, y1, z1, x1, yEnd, z2, limit, isWall);
            }
            // If none of the axes are longer than the other two, it is a straight diagonal line.
            // FIXME: some non-straight diagonal lines make it here
            // (such as if the two corners cover a 3x3x2 space)
            else {
                line = Lines.NormalLineImpl(x1, y1, z1, x2, y2, z2, limit, isWall);
            }

            // Return each set of coordinates.
            foreach (UShortCoords pos in line) {
                yield return pos;
            }
        }
    }
}


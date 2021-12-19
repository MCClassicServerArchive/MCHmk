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
    /// The Cuboids static class contains a collection of functions related to the drawing of cuboids.
    /// </summary>
    public static class Cuboids {
        /// <summary>
        /// Delegate for functions that estimate how many blocks a cuboid consists of.
        /// </summary>
        private delegate int CuboidCount(int x1, int y1, int z1, int x2, int y2, int z2);
        /// <summary>
        /// Delegate for cuboid generators.
        /// </summary>
        private delegate IEnumerable<UShortCoords> CuboidGen(ushort x1, ushort y1, ushort z1,
                                                             ushort x2, ushort y2, ushort z2);

        /// <summary>
        /// A mapping of strings to the cuboid types that they represent.
        /// </summary>
        private static readonly Dictionary<string, CuboidType> _cuboidMapping = 
            new Dictionary<string, CuboidType>(StringComparer.OrdinalIgnoreCase) {
                {"solid", CuboidType.Solid},
                {"hollow", CuboidType.Hollow},
                {"walls", CuboidType.Walls},
                {"holes", CuboidType.Holes},
                {"wire", CuboidType.Wire},
                {"random", CuboidType.Random}
            };

        /// <summary>
        /// A mapping of cuboid types to their respective generators.
        /// </summary>
        private static readonly Dictionary<CuboidType, CuboidGen> _cuboidGenMapping =
            new Dictionary<CuboidType, CuboidGen>() {
                {CuboidType.Solid, SolidCuboid},
                {CuboidType.Hollow, HollowCuboid},
                {CuboidType.Walls, WallsCuboid},
                {CuboidType.Holes, HolesCuboid},
                {CuboidType.Wire, WireCuboid},
                {CuboidType.Random, RandomCuboid}
            };

        /// <summary>
        /// A mapping of cuboid types to their respective counting functions.
        /// </summary>
        private static readonly Dictionary<CuboidType, CuboidCount> _blockCountMapping =
            new Dictionary<CuboidType, CuboidCount>() {
                {CuboidType.Solid, SolidCount},
                {CuboidType.Hollow, HollowCount},
                {CuboidType.Walls, WallsCount},
                {CuboidType.Holes, HolesCount},
                {CuboidType.Wire, WireCount},
                // The worst case for random cuboids is that every block is placed.
                {CuboidType.Random, SolidCount}
            };

        /// <summary>
        /// Given a string, determine what cuboid type it represents.
        /// </summary>
        /// <param name="type"> A string representing the type of cuboid to be drawn. </param>
        /// <returns> The corresponding CuboidType, or CuboidType.Null if an invalid cuboid type was
        /// provided. <seealso cref="CuboidType"/></returns>
        public static CuboidType ParseCuboidType(string type) {
            return _cuboidMapping.ContainsKey(type) ?_cuboidMapping[type] : CuboidType.Null;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks that a cuboid will consist of.
        /// </summary>
        /// <remarks>
        /// This is a helper function that picks the correct block count function automatically.
        /// </remarks>
        /// <param name="type"> The type of cuboid to be drawn. <seealso cref="CuboidType"/></param>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks that the cuboid will consist of. </returns>
        /// <exception cref="ArgumentException"> Thrown if CuboidType.Null is passed as the cuboid type. </exception>
        public static int CountBlocks(CuboidType type, int x1, int y1, int z1, int x2, int y2, int z2) {
            if (type == CuboidType.Null) {
                throw new ArgumentException("CuboidType.Null is not a valid cuboid type.");
            }

            CuboidCount func = _blockCountMapping[type];
            return func(x1, y1, z1, x2, y2, z2);
        }

        /// <summary>
        /// Given two corners, counts the number of blocks needed to make a solid cuboid.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks needed to make a solid cuboid.. </returns>
        public static int SolidCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            // Get the lengths of the cuboid on each axis.
            int xLen = Measurement.LineBlockLen(x1, x2);
            int yLen = Measurement.LineBlockLen(y1, y2);
            int zLen = Measurement.LineBlockLen(z1, z2);

            // This is the formula for a rectangular prism's volume.
            return xLen * yLen * zLen;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks needed to make a hollow cuboid.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks needed to make a hollow cuboid. </returns>
        public static int HollowCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            // Get the lengths of the cuboid on each axis.
            int xLen = Measurement.LineBlockLen(x1, x2);
            int yLen = Measurement.LineBlockLen(y1, y2);
            int zLen = Measurement.LineBlockLen(z1, z2);

            // If any dimension of the cuboid has a length of two or less, there is no hollow part.
            // Just reuse the block count function for solid cuboids if the cuboid being drawn is small enough.
            // If the formula below was used instead, it would give an incorrect result for 1x1x1 cuboids.
            if (xLen < 3 || yLen < 3 || zLen < 3) {
                return xLen * yLen * zLen;
            }

            // This is the formula for the surface area of a rectangular prism. However, using this
            // formula alone is not enough because it doesn't take into account that the blocks that
            // make up the edges and corners are part of more than one side of the prism. This will
            // cause those blocks to be counted multiple times, and that is accounted for later.
            int surfArea = 2 * (xLen * yLen + yLen * zLen + xLen * zLen);

            // The eight corners are counted two additional times by the above formula, so subtract sixteen.
            const int corners = 16;

            // Each of the twelve edges is counted an additional time by the above formula, but we have
            // to subtract two from each dimension becuase the edges include the corner blocks, which
            // have already been subtracted from the total.
            int edges = (xLen - 2) * 4 + (yLen - 2) * 4 + (zLen - 2) * 4;

            // Put everything together.
            return surfArea - edges - corners;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks needed to make a walls-type cuboid.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks needed to make a walls-type cuboid. </returns>
        public static int WallsCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            // Get the lengths of the cuboid on each axis.
            int xLen = Measurement.LineBlockLen(x1, x2);
            int yLen = Measurement.LineBlockLen(y1, y2);
            int zLen = Measurement.LineBlockLen(z1, z2);

            // If the x-axis or z-axis of the cuboid has a length of two or less, then there is no hollow part.
            // Just reuse the block count function for solid cuboids if the cuboid being drawn is small enough.
            // If the formula below was used instead, it would give an incorrect result for small cuboids.
            // Note that the y-axis isn't checked since a walls-type cuboid doesn't have top and bottom faces.
            if (xLen < 3 || zLen < 3) {
                return xLen * yLen * zLen;
            }

            // This is a modified form of the formula used to calculate the surface area of a rectangular prism.
            // It excludes the top and bottom faces.
            int surfArea = 2 * (xLen * yLen + yLen * zLen);
            // The edges are counted an additional time by the above formula.
            int edges = yLen * 4;

            // Put both numbers together.
            return surfArea - edges;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks needed to make a holes-type cuboid.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks needed to make a holes-type cuboid. </returns>
        public static int HolesCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            // On a checkerboard, half the blocks are one color, while half of them are another color.
            // Thus, reuse the solid cuboid's block count function, and then halve it. However, if there
            // was an odd number of blocks within the cuboid's area, the result is not an integer. This
            // can be resolved by noting that the first block in a holes-type cuboid is always air, which
            // is an implementation detail of the holes-type cuboid. Thus, the result should round down,
            // which integer division does for positive numbers.
            return SolidCount(x1, y1, z1, x2, y2, z2) / 2;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks needed to make a wire-type cuboid.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks needed to make a wire-type cuboid. </returns>
        public static int WireCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            // Get the lengths of the cuboid on each axis.
            int xLen = Measurement.LineBlockLen(x1, x2);
            int yLen = Measurement.LineBlockLen(y1, y2);
            int zLen = Measurement.LineBlockLen(z1, z2);

            // Sort the lengths in increasing order.
            int[] lengths = {xLen, yLen, zLen};
            Array.Sort(lengths);

            // If the two shortest lengths are both less than three, there is no hollow part.
            // Just reuse the block count function for solid cuboids if the cuboid being drawn is small enough.
            // If the other formulas below were used instead, they would give an incorrect result for small cuboids.
            if (lengths[0] < 3 && lengths[1] < 3) {
                return xLen * yLen * zLen;
            }

            // If one of the lengths is exactly one, then the cuboid is really just an outline of a square.
            // Add the lengths of the edges, making sure not to count the corners twice.
            if (lengths[0] == 1) {
                return lengths[1] * 2 + (lengths[2] - 2) * 2;
            }

            // Otherwise, the formula below works for any wire-type cuboid that is 3x3x2 or larger.
            // It avoids counting the corners multiple times.
            return xLen * 4 + (yLen - 2) * 4 + (zLen - 2) * 4;
        }

        /// <summary>
        /// Generates a cuboid.
        /// </summary>
        /// <remarks>
        /// This function should be called in most scenarios if a cuboid needs to be drawn.
        ///
        /// A cuboid in MCHmk is an IEnumerable<UShortCoords> that can be iterated through to return one
        /// block at a time, which is represented as a set of coordinates. Usually, the coordinates are
        /// passed to another method that actually places the block.
        ///
        /// Note that this function is actually a C# generator.
        /// </remarks>
        /// <param name="cuboid"> The type of cuboid being drawn. <seealso cref="CuboidType"/></param>
        /// <param name="lvl"> The level that the cuboid is being drawn on. <seealso cref="Level"/></param>
        /// <param name="block"> The type of block that the cuboid is using. <seealso cref="BlockId"/></param>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <param name="filterSame"> Whether blocks that are already the same as the block type being
        /// drawn should be skipped. Normally, this should be true to save bandwidth. Default: true. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the cuboid. </returns>
        /// <exception cref="ArgumentException"> Thrown if CuboidType.Null is passed as the cuboid type. </exception>
        public static IEnumerable<UShortCoords> GenerateCuboid(CuboidType cuboid, Level lvl, BlockId block,
                ushort x1, ushort y1, ushort z1, ushort x2, ushort y2, ushort z2, bool filterSame = true) {
            if (cuboid == CuboidType.Null) {
                throw new ArgumentException("CuboidType.Null is not a valid cuboid type.");
            }

            CuboidGen gen = _cuboidGenMapping[cuboid];
            foreach (UShortCoords pos in gen(x1, y1, z1, x2, y2, z2)) {
                // Unless otherwise instructed, don't draw blocks that are already the same block type
                // as the provided one.
                if (!filterSame || lvl.GetTile(pos.X, pos.Y, pos.Z) != block) {
                    yield return pos;
                }
            }
        }

        /// <summary>
        /// Generates a solid cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unaltered solid cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the solid cuboid. </returns>
        public static IEnumerable<UShortCoords> SolidCuboid(ushort x1, ushort y1, ushort z1,
                                                            ushort x2, ushort y2, ushort z2) {
            // Figure out the start and end points of the cuboid.
            ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
            ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
            ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

            // Draw each block within the boundaries of the cuboid.
            for (ushort curX = startX; curX <= endX; curX++) {
                for (ushort curY = startY; curY <= endY; curY++) {
                    for (ushort curZ = startZ; curZ <= endZ; curZ++) {
                        yield return new UShortCoords(curX, curY, curZ);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a hollow cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unfiltered hollow cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the hollow cuboid. </returns>
        public static IEnumerable<UShortCoords> HollowCuboid(ushort x1, ushort y1, ushort z1,
                                                             ushort x2, ushort y2, ushort z2) {
            // Figure out the start and end points of the cuboid.
            ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
            ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
            ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

            // If the cuboid has a length of two or less on any dimension, the cuboid can't be
            // hollow, so treat it like a solid cuboid.
            if (Measurement.LineBlockLen(startX, endX) < 3 || Measurement.LineBlockLen(startY, endY) < 3 ||
                    Measurement.LineBlockLen(startZ, endZ) < 3) {
                foreach (UShortCoords block in SolidCuboid(x1, y1, z1, x2, y2, z2)) {
                    yield return block;
                }
                // Don't continue any further.
                yield break;
            }

            // Draw the two walls that align with the z-axis.
            for (ushort curY = startY; curY <= endY; curY++) {
                for (ushort curZ = startZ; curZ <= endZ; curZ++) {
                    yield return new UShortCoords(startX, curY, curZ);
                    yield return new UShortCoords(endX, curY, curZ);
                }
            }

            // Draw the top and bottom of the cuboid, making sure not to redraw blocks
            // that share edges with the previously drawn walls.
            for (ushort curX = (ushort)(startX + 1); curX <= (ushort)(endX - 1); curX++) {
                for (ushort curZ = startZ; curZ <= endZ; curZ++) {
                    yield return new UShortCoords(curX, startY, curZ);
                    yield return new UShortCoords(curX, endY, curZ);
                }
            }

            // Draw the two walls that align with the x axis, making sure not to redraw
            // blocks that share edges with either the top and bottom of the cuboid or
            // the walls that are along the z-axis.
            for (ushort curX = (ushort)(startX + 1); curX <= (ushort)(endX - 1); curX++) {
                for (ushort curY = (ushort)(startY + 1); curY <= (ushort)(endY - 1); curY++) {
                    yield return new UShortCoords(curX, curY, startZ);
                    yield return new UShortCoords(curX, curY, endZ);
                }
            }
        }

        /// <summary>
        /// Generates a walls-type cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unfiltered walls-type cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the walls-type cuboid. </returns>
        public static IEnumerable<UShortCoords> WallsCuboid(ushort x1, ushort y1, ushort z1,
                                                            ushort x2, ushort y2, ushort z2) {
            // Figure out the start and end points of the cuboid.
            ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
            ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
            ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

            // If the cuboid has a length of two or less on the x or z axes, the cuboid doesn't
            // have a hollow part, so treat it like a solid cuboid. The y-axis isn't checked because
            // the walls-type cuboid doesn't have a top or bottom face.
            if (Measurement.LineBlockLen(startX, endX) < 3 || Measurement.LineBlockLen(startZ, endZ) < 3) {
                foreach (UShortCoords block in SolidCuboid(x1, y1, z1, x2, y2, z2)) {
                    yield return block;
                }
                // Don't continue any further.
                yield break;
            }

            // Draw the walls that align with the z-axis.
            for (ushort curY = startY; curY <= endY; curY++) {
                for (ushort curZ = startZ; curZ <= endZ; curZ++) {
                    yield return new UShortCoords(startX, curY, curZ);
                    yield return new UShortCoords(endX, curY, curZ);
                }
            }

            // Draw the walls that align with the x-axis, making sure not to redraw blocks that share
            // an edge with the z-axis aligned walls.
            for (ushort curX = (ushort)(startX + 1); curX <= (ushort)(endX - 1); curX++) {
                for (ushort curY = startY; curY <= endY; curY++) {
                    yield return new UShortCoords(curX, curY, startZ);
                    yield return new UShortCoords(curX, curY, endZ);
                }
            }
        }

        /// <summary>
        /// Generates a holes-type cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unfiltered holes-type cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the holes-type cuboid. </returns>
        public static IEnumerable<UShortCoords> HolesCuboid(ushort x1, ushort y1, ushort z1,
                                                            ushort x2, ushort y2, ushort z2) {
            // Figure out the start and end points of the cuboid.
            ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
            ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
            ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

            // Obtain the lengths of the cuboid on each axis as well.
            ushort xLen = Measurement.LineBlockLen(startX, endX);
            ushort yLen = Measurement.LineBlockLen(startY, endY);
            ushort zLen = Measurement.LineBlockLen(startZ, endZ);

            // The corner closest to the origin will act as (1, 1, 1). With that in mind,
            // iterate through each block.
            for (ushort curX = 1; curX <= xLen; curX++) {
                for (ushort curY = 1; curY <= yLen; curY++) {
                    for (ushort curZ = 1; curZ <= zLen; curZ++) {
                        // If the translated coordinates add up to an even number, place a block there.
                        if ((curX + curY + curZ) % 2 == 0) {
                            // Get the actual location of the block by adding the starting coordinates
                            // to the ones we're using. Subtract 1 to account for the corner closest to
                            // the origin being treated as (1, 1, 1) instead of (0, 0, 0).
                            ushort xPos = (ushort)(startX + curX - 1);
                            ushort yPos = (ushort)(startY + curY - 1);
                            ushort zPos = (ushort)(startZ + curZ - 1);

                            yield return new UShortCoords(xPos, yPos, zPos);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Generates a wire-type cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unfiltered wire-type cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the wire-type cuboid. </returns>
        public static IEnumerable<UShortCoords> WireCuboid(ushort x1, ushort y1, ushort z1,
                                                           ushort x2, ushort y2, ushort z2) {
            // Figure out the start and end points of the cuboid.
            ushort startX = Math.Min(x1, x2), endX = Math.Max(x1, x2);
            ushort startY = Math.Min(y1, y2), endY = Math.Max(y1, y2);
            ushort startZ = Math.Min(z1, z2), endZ = Math.Max(z1, z2);

            // Obtain the lengths of the cuboid on each axis as well.
            ushort xLen = Measurement.LineBlockLen(startX, endX);
            ushort yLen = Measurement.LineBlockLen(startY, endY);
            ushort zLen = Measurement.LineBlockLen(startZ, endZ);

            // Sort the lengths in increasing order.
            int[] lengths = {xLen, yLen, zLen};
            Array.Sort(lengths);

            // If the two smallest dimensions have a length of two or less, the cuboid can't be
            // hollow, so treat it like a solid cuboid.
            if (lengths[0] < 3 && lengths[1] < 3) {
                foreach (UShortCoords block in SolidCuboid(x1, y1, z1, x2, y2, z2)) {
                    yield return block;
                }
            }
            // If one of the lengths is one, then the cuboid is really just an outline of a square.
            // In this case, draw the four edges, making sure not to draw the corners twice.
            else if (xLen == 1) {
                for (ushort curY = startY; curY <= endY; curY++) {
                    yield return new UShortCoords(startX, curY, startZ);
                    yield return new UShortCoords(startX, curY, endZ);
                }
                for (ushort curZ = (ushort)(startZ + 1); curZ <= (ushort)(endZ - 1); curZ++) {
                    yield return new UShortCoords(startX, startY, curZ);
                    yield return new UShortCoords(startX, endY, curZ);
                }
            }
            else if (yLen == 1) {
                for (ushort curX = startX; curX <= endX; curX++) {
                    yield return new UShortCoords(curX, startY, startZ);
                    yield return new UShortCoords(curX, startY, endZ);
                }
                for (ushort curZ = (ushort)(startZ + 1); curZ <= (ushort)(endZ - 1); curZ++) {
                    yield return new UShortCoords(startX, startY, curZ);
                    yield return new UShortCoords(endX, startY, curZ);
                }
            }
            else if (zLen == 1) {
                for (ushort curX = startX; curX <= endX; curX++) {
                    yield return new UShortCoords(curX, startY, startZ);
                    yield return new UShortCoords(curX, endY, startZ);
                }
                for (ushort curY = (ushort)(startY + 1); curY <= (ushort)(endY - 1); curY++) {
                    yield return new UShortCoords(startX, curY, startZ);
                    yield return new UShortCoords(endX, curY, startZ);
                }
            }
            // Otherwise, the cuboid is at least 3x3x2. A rectangular prism has twelve edges, so
            // draw each of them. The +1 and -1 make sure that the corners are not drawn multiple times.
            else {
                for (ushort curX = startX; curX <= endX; curX++) {
                    yield return new UShortCoords(curX, startY, startZ);
                    yield return new UShortCoords(curX, endY, startZ);
                    yield return new UShortCoords(curX, startY, endZ);
                    yield return new UShortCoords(curX, endY, endZ);
                }
                for (ushort curY = (ushort)(startY + 1); curY <= (ushort)(endY - 1); curY++) {
                    yield return new UShortCoords(startX, curY, startZ);
                    yield return new UShortCoords(endX, curY, startZ);
                    yield return new UShortCoords(startX, curY, endZ);
                    yield return new UShortCoords(endX, curY, endZ);
                }
                for (ushort curZ = (ushort)(startZ + 1); curZ <= (ushort)(endZ - 1); curZ++) {
                    yield return new UShortCoords(startX, startY, curZ);
                    yield return new UShortCoords(endX, startY, curZ);
                    yield return new UShortCoords(startX, endY, curZ);
                    yield return new UShortCoords(endX, endY, curZ);
                }
            }
        }

        /// <summary>
        /// Generates a random cuboid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unfiltered random cuboid is the desired result.
        /// Otherwise, use GenerateCuboid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the random cuboid. </returns>
        public static IEnumerable<UShortCoords> RandomCuboid(ushort x1, ushort y1, ushort z1,
                                                             ushort x2, ushort y2, ushort z2) {
            // Obtain a solid cuboid that covers the area specified by the provided corners,
            // but randomly decide whether to draw each individual block from that cuboid.
            Random rand = new Random();
            foreach (UShortCoords block in SolidCuboid(x1, y1, z1, x2, y2, z2)) {
                if (rand.Next(2) == 0) { // 0 to 1; basically a coin flip
                    yield return block;
                }
            }
        }
    }
}

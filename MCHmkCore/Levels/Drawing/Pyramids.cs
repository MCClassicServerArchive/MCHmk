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
    /// The Pyramids static class contains a collection of functions related to the drawing of pyramids.
    /// </summary>
    public static class Pyramids {
        /// <summary>
        /// Delegate for functions that estimate how many blocks a pyramid consists of.
        /// </summary>
        private delegate int PyramidCount(int x1, int y1, int z1, int x2, int y2, int z2);
        /// <summary>
        /// Delegate for pyramid generators.
        /// </summary>
        private delegate IEnumerable<UShortCoords> PyramidGen(ushort x1, ushort y1, ushort z1,
                                                              ushort x2, ushort y2, ushort z2);

        /// <summary>
        /// A mapping of strings to the pyramid types that they represent.
        /// </summary>
        private static readonly Dictionary<string, PyramidType> _pyramidMapping = 
            new Dictionary<string, PyramidType>(StringComparer.OrdinalIgnoreCase) {
                {"solid", PyramidType.Solid},
                {"hollow", PyramidType.Hollow},
                {"reverse", PyramidType.Reverse}
            };

        /// <summary>
        /// A mapping of pyramid types to their respective generators.
        /// </summary>
        private static readonly Dictionary<PyramidType, PyramidGen> _pyramidGenMapping =
            new Dictionary<PyramidType, PyramidGen>() {
                {PyramidType.Solid, SolidPyramid},
                {PyramidType.Hollow, HollowPyramid},
                {PyramidType.Reverse, ReversePyramid}
            };

        /// <summary>
        /// A mapping of pyramid types to their respective counting functions.
        /// </summary>
        private static readonly Dictionary<PyramidType, PyramidCount> _blockCountMapping =
            new Dictionary<PyramidType, PyramidCount>() {
                {PyramidType.Solid, SolidCount},
                {PyramidType.Hollow, HollowCount},
                {PyramidType.Reverse, ReverseCount}
            };

        /// <summary>
        /// Given a string, determine what pyramid type it represents.
        /// </summary>
        /// <param name="type"> A string representing the type of pyramid to be drawn. </param>
        /// <returns> The corresponding PyramidType, or PyramidType.Null if an invalid pyramid type was
        /// provided. <seealso cref="PyramidType"/></returns>
        public static PyramidType ParsePyramidType(string type) {
            return _pyramidMapping.ContainsKey(type) ?_pyramidMapping[type] : PyramidType.Null;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks that a pyramid will consist of.
        /// </summary>
        /// <remarks>
        /// This is a helper function that picks the correct block count function automatically.
        /// </remarks>
        /// <param name="type"> The type of pyramid to be drawn. <seealso cref="PyramidType"/></param>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks that the pyramid will consist of. </returns>
        /// <exception cref="ArgumentException"> Thrown if PyramidType.Null was passed as the pyramid type,
        /// or the two corners have different y coordinates. </exception>
        public static int CountBlocks(PyramidType type, int x1, int y1, int z1, int x2, int y2, int z2) {
            if (type == PyramidType.Null) {
                throw new ArgumentException("PyramidType.Null is not a valid pyramid type.");
            }
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            PyramidCount func = _blockCountMapping[type];
            return func(x1, y1, z1, x2, y2, z2);
        }

        /// <summary>
        /// Given two corners, counts the number of blocks that a solid pyramid will consist of.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks that the solid pyramid will consist of. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static int SolidCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            // Figure out the length and width of the pyramid.
            int curXLen = Measurement.LineBlockLen(x1, x2);
            int curZLen = Measurement.LineBlockLen(z1, z2);

            int total = 0;
            while (curXLen > 0 && curZLen > 0) {
                // Treat each layer of the pyramid like a solid cuboid.
                total += curXLen * curZLen;

                // The edges of the next layer are closer together.
                curXLen -= 2;
                curZLen -= 2;
            }
            return total;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks that a hollow pyramid will consist of.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks that the hollow pyramid will consist of. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static int HollowCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            // Figure out the length and width of the pyramid.
            int curXLen = Measurement.LineBlockLen(x1, x2);
            int curZLen = Measurement.LineBlockLen(z1, z2);

            // The bottom layer of the pyramid is a solid floor, so treat it like a solid cuboid.
            int total = curXLen * curZLen;
            curXLen -= 2;
            curZLen -= 2;

            while (curXLen > 0 && curZLen > 0) {
                // If the next layer has a length or width of two or less, there is no air in the middle,
                // so treat it like a very small solid cuboid.
                if (curXLen < 3 || curZLen < 3) {
                    total += curXLen * curZLen;
                }
                // Otherwise, the blocks come together to form the outline of a square, so add up the
                // lengths of the edges, making sure not to count the corners twice.
                else {
                    total += curXLen * 2 + (curZLen - 2) * 2;
                }

                // The edges of the next layer are closer together.
                curXLen -= 2;
                curZLen -= 2;
            }
            return total;
        }

        /// <summary>
        /// Given two corners, counts the number of blocks that a reverse pyramid will consist of.
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> The number of blocks that the reverse pyramid will consist of. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static int ReverseCount(int x1, int y1, int z1, int x2, int y2, int z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            // Reverse pyramids are just upside-down versions of solid pyramids, so use the same function.
            return SolidCount(x1, y1, z1, x2, y2, z2);
        }

        /// <summary>
        /// Generates a pyramid.
        /// </summary>
        /// <remarks>
        /// This function should be called in most scenarios if a pyramid needs to be drawn.
        ///
        /// Pyramids work similarly to cuboids. Like a cuboid, a pyramid in MCHmk is an IEnumerable<UShortCoords>
        /// that can be iterated through to return one block at a time, which is represented as a set of coordinates.
        /// Usually, the coordinates are passed to another method that actually places the block.
        /// </remarks>
        /// <param name="pyramid"> The type of pyramid being drawn. <seealso cref="PyramidType"/></param>
        /// <param name="lvl"> The level that the pyramid is being drawn on. <seealso cref="Level"/></param>
        /// <param name="block"> The type of block that the pyramid is using. <seealso cref="BlockId"/></param>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <param name="filterSame"> Whether blocks that are already the same as the block type being
        /// drawn should be skipped. Normally, this should be true to save bandwidth. Default: true. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the pyramid. </returns>
        /// <exception cref="ArgumentException"> Thrown if PyramidType.Null was passed as the pyramid type,
        /// or the two corners have different y coordinates. </exception>
        public static IEnumerable<UShortCoords> GeneratePyramid(PyramidType pyramid, Level lvl, BlockId block,
                ushort x1, ushort y1, ushort z1, ushort x2, ushort y2, ushort z2, bool filterSame = true) {
            if (pyramid == PyramidType.Null) {
                throw new ArgumentException("PyramidType.Null is not a valid pyramid type.");
            }
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            PyramidGen gen = _pyramidGenMapping[pyramid];
            foreach (UShortCoords pos in gen(x1, y1, z1, x2, y2, z2)) {
                // Unless otherwise instructed, don't draw blocks that are already the same block type
                // as the provided one.
                if (!filterSame || lvl.GetTile(pos.X, pos.Y, pos.Z) != block) {
                    yield return pos;
                }
            }
        }

        /// <summary>
        /// Generates a solid pyramid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unaltered solid pyramid is the desired result.
        /// Otherwise, use GeneratePyramid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the solid pyramid. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static IEnumerable<UShortCoords> SolidPyramid(ushort x1, ushort y1, ushort z1,
                                                             ushort x2, ushort y2, ushort z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }
            foreach (UShortCoords block in SolidPyramidImpl(x1, y1, z1, x2, y2, z2, false)) {
                yield return block;
            }
        }

        /// <summary>
        /// Generates a hollow pyramid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unaltered hollow pyramid is the desired result.
        /// Otherwise, use GeneratePyramid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the solid pyramid. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static IEnumerable<UShortCoords> HollowPyramid(ushort x1, ushort y1, ushort z1,
                                                              ushort x2, ushort y2, ushort z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }

            // Figure out the length and width of the pyramid and the starting height.
            ushort lowX = Math.Min(x1, x2), highX = Math.Max(x1, x2);
            ushort curY = y1;  // The actual height is determined by the x and z coordinates, so y2 is unused.
            ushort lowZ = Math.Min(z1, z2), highZ = Math.Max(z1, z2);

            // The first layer of a hollow pyramid is solid, so treat it like a solid cuboid.
            foreach (UShortCoords block in Cuboids.SolidCuboid(lowX, curY, lowZ, highX, curY, highZ)) {
                yield return block;
            }

            // Bring the edges closer together.
            lowX++;
            highX--;
            lowZ++;
            highZ--;

            // The next layer is on top of the previous one.
            curY++;

            // Treat the rest of the layers as if they were walls-type cuboids with a height of one.
            while (lowX <= highX && lowZ <= highZ) {
                foreach (UShortCoords block in Cuboids.WallsCuboid(lowX, curY, lowZ, highX, curY, highZ)) {
                    yield return block;
                }

                // Each layer is smaller than the previous one.
                lowX++;
                highX--;
                lowZ++;
                highZ--;

                // Each layer is also on top of the other one.
                curY++;
            }
        }

        /// <summary>
        /// Generates a reverse pyramid.
        /// </summary>
        /// <remarks>
        /// This function does not skip over any blocks that are already of the same type as the block
        /// being placed. Use this function only if an unaltered reverse pyramid is the desired result.
        /// Otherwise, use GeneratePyramid().
        /// </remarks>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the solid pyramid. </returns>
        /// <exception cref="ArgumentException"> Thrown if the two corners have different y coordinates. </exception>
        public static IEnumerable<UShortCoords> ReversePyramid(ushort x1, ushort y1, ushort z1,
                                                               ushort x2, ushort y2, ushort z2) {
            if (y1 != y2) {
                throw new ArgumentException("The two corners must be on the same y coordinate.");
            }
            foreach (UShortCoords block in SolidPyramidImpl(x1, y1, z1, x2, y2, z2, true)) {
                yield return block;
            }
        }


        /// <summary>
        /// Generates a solid pyramid. This function holds code shared by SolidPyramid() and ReversePyramid().
        /// </summary>
        /// <param name="x1"> The x-coordinate of the first corner. </param>
        /// <param name="y1"> The y-coordinate of the first corner. </param>
        /// <param name="z1"> The z-coordinate of the first corner. </param>
        /// <param name="x2"> The x-coordinate of the second corner. </param>
        /// <param name="y2"> The y-coordinate of the second corner. </param>
        /// <param name="z2"> The z-coordinate of the second corner. </param>
        /// <param name="reverse"> Whether the pyramid should be upside-down. </param>
        /// <returns> An IEnumerable<UShortCoords> that can be iterated through to obtain the coordinates
        /// of the blocks within the solid pyramid. </returns>
        private static IEnumerable<UShortCoords> SolidPyramidImpl(ushort x1, ushort y1, ushort z1,
                                                                  ushort x2, ushort y2, ushort z2, bool reverse) {
            // Figure out the length and width of the pyramid and the starting height.
            ushort lowX = Math.Min(x1, x2), highX = Math.Max(x1, x2);
            ushort curY = y1;  // The actual height is determined by the x and z coordinates, so y2 is unused.
            ushort lowZ = Math.Min(z1, z2), highZ = Math.Max(z1, z2);

            // Treat each layer of the pyramid as a solid cuboid with a height of one.
            while (lowX <= highX && lowZ <= highZ) {
                foreach (UShortCoords block in Cuboids.SolidCuboid(lowX, curY, lowZ, highX, curY, highZ)) {
                    yield return block;
                }

                // Each layer is smaller than the previous one.
                lowX++;
                highX--;
                lowZ++;
                highZ--;

                // Each layer is either below or above the previous one.
                if (reverse) {
                    curY--;
                }
                else {
                    curY++;
                }
            }
        }
    }
}


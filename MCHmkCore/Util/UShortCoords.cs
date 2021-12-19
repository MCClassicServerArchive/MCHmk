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

namespace MCHmk {
    /// <summary>
    /// The UShortCoords struct represents a set of three-dimensional coordinates given as unsigned 16-bit integers.
    /// </summary>
    /// <remarks>
    /// This struct is mutable for performance reasons. Care should be taken when passing UShortCoords instances
    /// into methods, as they will be copied-by-value unless the parameter has the ref keyword. This can lead to
    /// subtle bugs if reference semantics were expected. This struct is best used for adding sets of coordinates in
    /// bulk to collections such as Lists. If only one set of coordinates is required for the implementation of a
    /// feature, consider just using three ushort variables instead since that is more readable, especially to
    /// programmers that are unaware of the existence of C# structs.
    /// </remarks>
    public struct UShortCoords {
        /// <summary>
        /// The x coordinate.
        /// </summary>
        public ushort X;
        /// <summary>
        /// The y coordinate.
        /// </summary>
        public ushort Y;
        /// <summary>
        /// The z coordinate.
        /// </summary>
        public ushort Z;

        /// <summary>
        /// Constructs a new UShortCoords instance.
        /// </summary>
        /// <param name="x"> The x coordinate. </param>
        /// <param name="y"> The y coordinate. </param>
        /// <param name="z"> The z coordinate. </param>
        public UShortCoords(ushort x, ushort y, ushort z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}

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
    /// The CuboidType enumeration represents the types of cuboids that can be drawn.
    /// </summary>
    public enum CuboidType {
        /// <summary>
        /// An invalid cuboid type. Used mainly if an invalid argument was passed to /cuboid.
        /// </summary>
        Null = 0,
        /// <summary>
        /// A solid cuboid, which is a rectangular prism. The most common type.
        /// </summary>
        Solid = 1,
        /// <summary>
        /// A cuboid with air inside of it.
        /// </summary>
        Hollow = 2,
        /// <summary>
        /// A cuboid that consists of the outer vertical walls only.
        /// </summary>
        Walls = 3,
        /// <summary>
        /// A cuboid with a checkerboard pattern throughout its area.
        /// </summary>
        Holes = 4,
        /// <summary>
        /// A cuboid that consists of only its edges.
        /// </summary>
        Wire = 5,
        /// <summary>
        /// A cuboid in which each individual block within it may or may not be placed.
        /// </summary>
        Random = 6
    }
}

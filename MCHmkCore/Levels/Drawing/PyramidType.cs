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
    /// The PyramidType enumeration represents the types of pyramids that can be drawn.
    /// </summary>
    public enum PyramidType {
        /// <summary>
        /// An invalid pyramid type. Used mainly if an invalid argument was passed to /pyramid.
        /// </summary>
        Null = 0,
        /// <summary>
        /// A solid pyramid.
        /// </summary>
        Solid = 1,
        /// <summary>
        /// A pyramid with air inside of it.
        /// </summary>
        Hollow = 2,
        /// <summary>
        /// An upside-down solid pyramid.
        /// </summary>
        Reverse = 3
    }
}


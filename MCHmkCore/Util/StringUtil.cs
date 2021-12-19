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
using System.Linq;

namespace MCHmk {
    /// <summary>
    /// The StringUtil class contains commonly-used code for dealing with strings in MCHmk's codebase.
    /// </summary>
    // TODO: better name than StringUtil
    public static class StringUtil {
        /// <summary>
        /// Checks whether a string contains only the provided characters.
        /// </summary>
        /// <param name="name"> The string to check. </param>
        /// <param name="chars"> A string containing the allowed characters. Each character in the string
        /// should be unique. </param>
        /// <returns> Whether the string contains only the allowed characters. </returns>
        public static bool ContainsOnlyChars(string s, string chars) {
            return s.All(ch => chars.IndexOf(ch) != -1);
        }
    }
}

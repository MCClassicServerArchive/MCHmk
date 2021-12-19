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
    // TODO: a class with one method is kinda silly
    public static class RankUtil {
        /// <summary>
        /// Returns the given list of permission values as a comma-separated list.
        /// </summary>
        /// <param name="givenList"> The list of permission values. </param>
        /// <returns> A comma-seperated string of permission values. </returns>
        public static string GetInts(List<int> givenList) {
            string returnString = String.Empty;
            bool foundOne = false; // Used to remove the comma and space later.
            foreach (int Perm in givenList) {
                foundOne = true;
                returnString += "," + Perm;
            }
            if (foundOne) {
                returnString = returnString.Remove(0, 1);
            }
            return returnString;
        }
    }
}

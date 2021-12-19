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

namespace MCHmk {
    /// <summary>
    /// The DefaultRankValue static class contains constants representing the default rank values in MCHmk.
    /// </summary>
    public static class DefaultRankValue {
        public const int Banned = -20;
        public const int Guest = 0;
        public const int Builder = 30;
        public const int AdvBuilder = 50;
        public const int Operator = 80;
        public const int Admin = 100;
        public const int Nobody = 120;
        public const int Null = 150;
    }
}

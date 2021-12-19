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

using NUnit.Framework;

using MCHmk;
using MCHmk.Drawing;

namespace MCHmkTests {
    /// <summary>
    /// Tests the block counting functions for cuboids.
    /// </summary>
    [TestFixture]
    public class CuboidCountTests {
        /// <summary>
        /// Tests whether cuboids that begin and end at the same point are counted properly.
        /// </summary>
        [Test]
        public void SolidCountSingle() {
            Assert.AreEqual(Cuboids.SolidCount(0, 0, 0, 0, 0, 0), 1);
        }

        /// <summary>
        /// Tests that a proper exception is thrown when Cuboid.Null is passed.
        /// </summary>
        [Test]
        public void NullHandling() {
            Assert.Throws<ArgumentException>(() => Cuboids.CountBlocks(CuboidType.Null, 0, 0, 0, 0, 0, 0));
        }
    }
}

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

namespace MCHmk {
    /// <summary>
    /// The CommandTempData block contains data related to the block that was just selected by the player in
    /// response to a command while holding data from an in-progress command at the same time.
    /// </summary>
    public class CommandTempData {
        /// <summary>
        /// The coordinates of the block.
        /// </summary>
        private UShortCoords _coords;
        /// <summary>
        /// The type of the block that the player was holding.
        /// </summary>
        private BlockId _blockType;
        /// <summary>
        /// Additional data that the command listening for the selected block needs.
        /// </summary>
        private IDictionary<string, object> _extraData = null;

        /// <summary>
        /// Gets the x coordinate.
        /// </summary>
        public ushort X {
            get {
                return _coords.X;
            }
        }

        /// <summary>
        /// Gets the y coordinate.
        /// </summary>
        public ushort Y {
            get {
                return _coords.Y;
            }
        }

        /// <summary>
        /// Gets the z coordinate.
        /// </summary>
        public ushort Z {
            get {
                return _coords.Z;
            }
        }

        /// <summary>
        /// Gets the type of the block that the player was holding.
        /// </summary>
        public BlockId BlockType {
            get {
                return _blockType;
            }
        }

        /// <summary>
        /// Constructs a new CommandTempData object.
        /// </summary>
        /// <param name="x"> The x coordinate of the selected block. </param>
        /// <param name="y"> The y coordinate of the selected block. </param>
        /// <param name="z"> The z coordinate of the selected block. </param>
        /// <param name="type"> The type of block that the player was holding. </param>
        /// <param name="data"> Additional data to be passed to the command that was waiting for a block to
        /// be selected. Can be null if no other data is needed. </param>
        public CommandTempData(ushort x, ushort y, ushort z, BlockId type, IDictionary<string, object> data) {
            _coords.X = x;
            _coords.Y = y;
            _coords.Z = z;
            _blockType = type;
            _extraData = data;
        }

        /// <summary>
        /// Obtains the value associated with the given key from the dictionary that was passed into
        /// the constructor when this CommandTempData instance was created.
        /// </summary>
        /// <param name="key"> The key that the value is associated with. </param>
        /// <typeparam name="T"> The type that the value was stored as. </typeparam>
        /// <returns> The value associated with the given key. </returns>
        /// <exception cref="ArgumentException"> Thrown if a null Dictionary was passed earlier when the object
        /// was being constructed or if the Dictionary has no values in it. </exception>
        public T GetData<T>(string key) {
            if (_extraData == null) {
                throw new ArgumentException("null was passed for the data when creating this object.");
            }
            if (_extraData.Count == 0) {
                throw new ArgumentException("The data passed has no values in it.");
            }

            return (T)_extraData[key];
        }

        /// <summary>
        /// Copies over the CommandTempData's data and coordinates into a Dictionary and return it.
        /// </summary>
        /// <remarks>
        /// If there are objects in the CommandTempData's extra data, only a shallow copy will be performed.
        /// </remarks>
        /// <param name="xKey"> The key that will be used to obtain the x coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <param name="yKey"> The key that will be used to obtain the y coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <param name="zKey"> The key that will be used to obtain the z coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <returns> The Dictionary containing the CommandTempData object's data. </returns>
        public Dictionary<string, object> CloneAsDict(string xKey, string yKey, string zKey) {
            return CloneAsDict(xKey, yKey, zKey, null);
        }

        /// <summary>
        /// Copies over the CommandTempData's data, coordinates, and block type into a Dictionary and return it.
        /// </summary>
        /// <remarks>
        /// If there are objects in the CommandTempData's data, only a shallow copy will be performed.
        /// </remarks>
        /// <param name="xKey"> The key that will be used to obtain the x coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <param name="yKey"> The key that will be used to obtain the y coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <param name="zKey"> The key that will be used to obtain the z coordinate. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <param name="blkKey"> The key that will be used to obtain the block type. Pass null or the empty
        /// string if this value isn't needed. </param>
        /// <returns> The Dictionary containing the CommandTempData object's data. </returns>
        public Dictionary<string, object> CloneAsDict(string xKey, string yKey, string zKey, string blkKey) {
            Dictionary<string, object> dict = new Dictionary<string, object>();

            if (!String.IsNullOrEmpty(xKey)) {
                dict[xKey] = _coords.X;
            }
            if (!String.IsNullOrEmpty(yKey)) {
                dict[yKey] = _coords.Y;
            }
            if (!String.IsNullOrEmpty(zKey)) {
                dict[zKey] = _coords.Z;
            }
            if (!String.IsNullOrEmpty(blkKey)) {
                dict[blkKey] = _blockType;
            }

            if (_extraData != null) {
                foreach (KeyValuePair<string, object> kvp in _extraData) {
                    dict[kvp.Key] = kvp.Value;
                }
            }

            return dict;
        }
    }
}


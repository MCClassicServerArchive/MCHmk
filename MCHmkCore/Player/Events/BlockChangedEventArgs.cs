using System;

namespace MCHmk {
    /// <summary>
    /// Provides data for the BlockChanged event, which occurs when the player places or removes a block.
    /// </summary>
    public class BlockChangedEventArgs : EventArgs {
        /// <summary>
        /// The coordinates of the block that was changed.
        /// </summary>
        private UShortCoords _coords;
        /// <summary>
        /// The block type that the player was holding.
        /// </summary>
        private BlockId _blockType;

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
        /// Gets the block type that the player was holding.
        /// </summary>
        public BlockId BlockType {
            get {
                return _blockType;
            }
        }

        /// <summary>
        /// Constructs a new BlockChangedEventArgs instance.
        /// </summary>
        /// <param name="x"> The x coordinate of the block that was changed. </param>
        /// <param name="y"> The y coordinate of the block that was changed. </param>
        /// <param name="z"> The z coordinate of the block that was changed. </param>
        /// <param name="type"> The block type that the player was holding. </param>
        public BlockChangedEventArgs(ushort x, ushort y, ushort z, BlockId type) {
            _coords.X = x;
            _coords.Y = y;
            _coords.Z = z;
            _blockType = type;
        }
    }
}


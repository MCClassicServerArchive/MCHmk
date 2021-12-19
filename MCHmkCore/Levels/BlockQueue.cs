/*
    Copyright 2016 Jjp137

    This file has been changed from the original source code by MCForge.

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

/*
Copyright (C) 2010-2013 David Mitchell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Timers;

namespace MCHmk {
    /// <summary>
    /// The BlockQueue class regulates the speed at which block changes caused by certain
    /// commands are sent out to clients.
    /// </summary>
    public class BlockQueue {
        /// <summary>
        /// The amount of time between block updates.
        /// </summary>
        public int time {
            get {
                return (int)blocktimer.Interval;
            }
            set {
                blocktimer.Interval = value;
            }
        }
        /// <summary>
        /// The maximum number of block changes to send out every time.
        /// </summary>
        public int blockupdates = 200;
        /// <summary>
        /// Temporary variable for a block.
        /// </summary>
        private block b = new block();
        /// <summary>
        /// The timer that is responsible for the delay between each batch of block updates.
        /// </summary>
        private Timer blocktimer = new Timer(100);
        /// <summary>
        /// If this value is 1, the block queue is currently active.
        /// </summary>
        private byte started = 0;

        /// <summary>
        /// The server that owns this block queue.
        /// </summary>
        private Server _s;

        /// <summary>
        /// Constructs a BlockQueue object.
        /// </summary>
        /// <param name="s"> The server that will be using this object. </param>
        internal BlockQueue(Server s) {
            _s = s;
        }

        public void Start() {
            blocktimer.Elapsed += delegate {
                if (started == 1) {  // Prevent any conflicts.
                    return;
                }
                started = 1;
                _s.levels.ForEach((l) => {
                    try {
                        // Make sure there are blocks in the queue.
                        if (l.blockqueue.Count < 1) {
                            return;
                        }

                        // The maximum number of blocks to update is either the maximum amount
                        // set, or the number of blocks currently in the queue, whichever is lower.
                        // Also, if there are no players, nothing is sent out over the network,
                        // so it is okay to update all of the blocks at once.
                        int count;
                        if (l.blockqueue.Count < blockupdates || l.players.Count == 0) {
                            count = l.blockqueue.Count;
                        }
                        else {
                            count = blockupdates;
                        }

                        // Call Blockchange() for every block in the queue up until the determined amount.
                        for (int c = 0; c < count; c++) {
                            l.Blockchange(l.blockqueue[c].p, l.blockqueue[c].x, l.blockqueue[c].y, l.blockqueue[c].z, l.blockqueue[c].type);
                        }
                        // Remove those blocks from the queue.
                        l.blockqueue.RemoveRange(0, count);
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        _s.logger.Log(String.Format("Block cache failed for map: {0}. {1} lost.", l.name, l.blockqueue.Count));
                        l.blockqueue.Clear();
                    }
                });
                started = 0;
            };
            blocktimer.Start();
        }

        /// <summary>
        /// Pauses the operation of the block queue.
        /// </summary>
        public void pause() {
            blocktimer.Enabled = false;
        }

        /// <summary>
        /// Resumes the operation of the block queue.
        /// </summary>
        public void resume() {
            blocktimer.Enabled = true;
        }

        /// <summary>
        /// Adds a block change to a level's block queue.
        /// </summary>
        /// <param name="P"> The player that placed the block. <seealso cref="Player"/></param>
        /// <param name="X"> The x coordinate of the block. </param>
        /// <param name="Y"> The y coordinate of the block. </param>
        /// <param name="Z"> The z coordinate of the block. </param>
        /// <param name="type"> The id of the new block type. </param>
        public void Addblock(Player P, ushort X, ushort Y, ushort Z, BlockId type) {
            b.x = X;
            b.y = Y;
            b.z = Z;
            b.type = type;
            b.p = P;
            P.level.blockqueue.Add(b);
        }

        /// <summary>
        /// The block struct contains information about a single block change.
        /// </summary>
        public struct block {
            /// <summary>
            /// The player that placed the block.
            /// </summary>
            public Player p;
            /// <summary>
            /// The x coordinate of the block.
            /// </summary>
            public ushort x;
            /// <summary>
            /// The y coordinate of the block.
            /// </summary>
            public ushort y;
            /// <summary>
            /// The z coordinate of the block.
            /// </summary>
            public ushort z;
            /// <summary>
            /// The id of the new block type.
            /// </summary>
            public BlockId type;
        }
    }
}
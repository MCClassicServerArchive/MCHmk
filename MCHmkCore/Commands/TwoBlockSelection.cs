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
    /// The TwoBlockSelection static class contains helper methods for commands that requires the user to
    /// select two blocks before the command is able to produce a result.
    /// </summary>
    /// <remarks>
    /// The methods in this class work well for commands that behave similarly to /cuboid. Any command that relies on
    /// selecting two blocks to mark some sort of area should use this class whenever possible to reduce code
    /// duplication. Specifically, if the command follows these steps:
    /// 
    /// - Prompts the user and aborts any other in-progress commands.
    /// - Add the coordinates of the first block when it is selected to the command's temporary data,
    /// prompts the user again, and does nothing else until the second block is selected.
    /// - Performs an action, such as generating a cuboid, when the player selects the second block.
    /// 
    /// then these methods are suitable for that command. If the command deviates from this pattern, such as
    /// requiring more than two blocks to be placed, or modifying Player or Level properties after the first block
    /// is placed, then these methods are unsuitable for that command. Also, if calling Player.ClearSelection()
    /// after each step is undesired, these methods should not be used. In either case, the command should implement
    /// the selection logic itself. An example of a command that doesn't fit this pattern is /portal.
    /// 
    /// In order to get the coordinates of the first block from the CommandTempData object that is passed to
    /// the method given as the onSecondBlock parameter, use the XKey, YKey, and ZKey static fields as the keys.
    /// 
    /// Refer to /cuboid's implementation for an example of the intended usage of the methods within this class.
    /// </remarks>
    public static class TwoBlockSelection {
        /// <summary>
        /// Name of the key that can be used to store and retrieve the x-coordinate of the first block's position.
        /// </summary>
        public static readonly string XKey = "generic_x";
        /// <summary>
        /// Name of the key that can be used to store and retrieve the y-coordinate of the first block's position.
        /// </summary>
        public static readonly string YKey = "generic_y";
        /// <summary>
        /// Name of the key that can be used to store and retrieve the z-coordinate of the first corner's position.
        /// </summary>
        public static readonly string ZKey = "generic_z";

        /// <summary>
        /// Starts the block selection process.
        /// </summary>
        /// <param name="p"> The player associated with the selection. <seealso cref="Player"/></param>
        /// <param name="data"> Data regarding the selection. Can be null if no data needs to be passed. </param>
        /// <param name="prompt"> The message to display to the player. </param>
        /// <param name="onSecondBlock"> The method to call after the second block is placed. This method will
        /// receive information about the second block that is placed. </param>
        public static void Start(Player p, Dictionary<string, object> data, string prompt,
                                 Action<Player, CommandTempData> onSecondBlock) {
            // Abort any conflicting commands, such as /about.
            p.ClearSelection();

            // Listen for a block change by the player.
            p.StartSelection((p2, c) => FirstBlockSelected(p2, c, onSecondBlock), data);
            p.SendMessage(prompt);
        }

        /// <summary>
        /// Checks whether the player has enabled static mode, and restarts the block selection process if they did.
        /// </summary>
        /// <param name="p"> The player which has enabled static mode. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the previous block selection and the command that was just used.
        /// <seealso cref="CommandTempData"/></param>
        /// <param name="onSecondBlock"> The method to call after the second block is placed. This method will
        /// receive information about the second block that is placed. </param>
        /// <param name="keysToKeep"> The keys of the values that should be carried over. </param>
        public static void RestartIfStatic(Player p, CommandTempData c, Action<Player, CommandTempData> onSecondBlock,
                                           params string[] keysToKeep) {
            // Don't do anything if static mode isn't active for this user.
            if (!p.staticCommands) {
                return;
            }

            // Reuse the arguments that were given earlier, but only copy the requested data.
            Dictionary<string, object> oldData = c.CloneAsDict(null, null, null, null);
            Dictionary<string, object> newData = new Dictionary<string, object>();

            foreach (string key in keysToKeep) {
                newData[key] = oldData[key];
            }

            // Listen for a block change by the user again.
            p.StartSelection((p2, c2) => FirstBlockSelected(p2, c2, onSecondBlock), newData);
        }

        /// <summary>
        /// Called when the first block is selected.
        /// </summary>
        /// <param name="p"> The player associated with the selection. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the block selection and the command being used.
        /// <seealso cref="CommandTempData"/></param>
        /// <param name="onSecondBlock"> The method to call after the second block is placed. This method will
        /// receive information about the second block that is placed. </param>
        private static void FirstBlockSelected(Player p, CommandTempData c,
                                               Action<Player, CommandTempData> onSecondBlock) {
            // Don't wait for the first block to be placed anymore.
            p.ClearSelection();

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            p.SendBlockchange(c.X, c.Y, c.Z, p.level.GetTile(c.X, c.Y, c.Z));

            // Get the CommandTempData's data as a dictionary so that the data will be carried over
            // to the next step of the command.
            Dictionary<string, object> data = c.CloneAsDict(XKey, YKey, ZKey);

            // Listen for another block change by the player.
            p.StartSelection((p2, c2) => SecondBlockSelected(p2, c2, onSecondBlock), data);
            p.SendMessage("First block placed. Place the second block.");
        }

        /// <summary>
        /// Called when the second block is selected.
        /// </summary>
        /// <param name="p"> The player associated with the selection. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the block selection and the command being used.
        /// <seealso cref="CommandTempData"/></param>
        /// <param name="onSecondBlock"> The method to call in response to the selection. This method will
        /// receive information about the second block that is placed. </param>
        private static void SecondBlockSelected(Player p, CommandTempData c,
                                                Action<Player, CommandTempData> onSecondBlock) {
            // Don't wait for the second block to be placed anymore.
            p.ClearSelection();

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            p.SendBlockchange(c.X, c.Y, c.Z, p.level.GetTile(c.X, c.Y, c.Z));

            // Perform the rest of the command.
            onSecondBlock(p, c);
        }
    }
}

/*
    Copyright 2016 Jjp137/LegoBricker

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
	Copyright © 2011-2014 MCForge-Redux

	Dual-licensed under the	Educational Community License, Version 2.0 and
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
using System.Collections.ObjectModel;

using MCHmk.Drawing;

namespace MCHmk.Commands {
    public class CmdWrite : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the message to be written.
        /// </summary>
        private readonly string _messageKey = "write_message";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "text"});

        public override string Name {
            get {
                return "write";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "build";
            }
        }
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }
        public override bool MuseumUsable {
            get {
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdWrite(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /write.
        /// </summary>
        /// <param name="p"> The player that used /write. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // /write needs something to write.
            if (args == String.Empty) {
                Help(p);
                return;
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_messageKey] = args.ToUpper();

            const string prompt = "Place two blocks to determine the message's direction.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the message's starting position and direciton.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        public void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            // Get the message to be written.
            string message = c.GetData<string>(_messageKey);

            // Consider bindings by /bind
            BlockId blockID = p.bindings[(int)c.BlockType];

            // Variable to compare the movement direction.
            ushort cur;

            // Ensure that the second block's location increases in either the x or z direction from the first block.
            if (x1 == x2 && z1 == z2) {
                p.SendMessage("The direction cannot be determined.");
                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _messageKey);
                return;
            }

            // If the x increase from block 1 to 2 is greater than the z increase.
            if (Math.Abs(x1 - x2) > Math.Abs(z1 - z2)) {
                cur = x1;
                // Increase is in the positive x direction. (Direction to go = 0)
                if (x2 > x1) {
                    foreach (char ch in message) {
                        cur = (ushort)CharacterWriter.writeCharacter(p, ch, cur, y1, z1, blockID, 0);
                    }
                }
                // Increase is in the negative x direction. (Direction to go = 1)
                else {
                    foreach (char ch in message) {
                        cur = (ushort)CharacterWriter.writeCharacter(p, ch, cur, y1, z1, blockID, 1);
                    }
                }
            }
            else {
                cur = z1;
                // Increase is in the positive z direction. (Direction to go = 2)
                if (z2 > z1) {
                    foreach (char ch in message) {
                        cur = (ushort)CharacterWriter.writeCharacter(p, ch, x1, y1, cur, blockID, 2);
                    }
                }
                // Increase is in the negative z direction. (Direction to go = 3)
                else {
                    foreach (char ch in message) {
                        cur = (ushort)CharacterWriter.writeCharacter(p, ch, x1, y1, cur, blockID, 3);
                    }
                }
            }

            // Handle /static.
            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _messageKey);
        }

        /// <summary>
        /// Called when /help is used on /write.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/write <message?> - Writes the given message using blocks.");
        }
    }
}

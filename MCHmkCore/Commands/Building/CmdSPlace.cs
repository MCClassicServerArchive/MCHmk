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

namespace MCHmk.Commands {
    /// <summary>
    /// This is the command /splace
    /// use /help splace in game for more info
    /// </summary>
    public class CmdSPlace : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the distance given.
        /// </summary>
        private readonly string _distKey = "splace_distance";
        /// <summary>
        /// Name of the key used to store and retrieve the interval given.
        /// </summary>
        private readonly string _intervalKey = "splace_interval";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"measure", "place"});

        public override string Name {
            get {
                return "splace";
            }
        }
        public override string Shortcut {
            get {
                return "set";
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
                return DefaultRankValue.Builder;
            }
        }

        public CmdSPlace(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            ushort distance = 0;
            ushort interval = 0;

            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (args.Split(' ').Length > 1) {
                try {
                    ushort.TryParse(args.Split(' ')[0], out distance);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Distance must be a number.");
                    return;
                }
                try {
                    ushort.TryParse(args.Split(' ')[1], out interval);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Interval must be a number.");
                    return;
                }
            }
            else {
                try {
                    ushort.TryParse(args, out distance);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Distance must be a number.");
                    return;
                }
            }
            if (distance < 1) {
                p.SendMessage("Enter a distance greater than 0.");
                return;
            }
            if (interval >= distance) {
                p.SendMessage("The interval cannot be greater than the distance.");
                return;
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_distKey] = distance;
            data[_intervalKey] = interval;

            const string prompt = "Place two blocks to determine the direction.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player selected both the starting point and the direction of the line.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            ushort distance = c.GetData<ushort>(_distKey);
            ushort interval = c.GetData<ushort>(_intervalKey);

            BlockId blocktype = p.bindings[(int)c.BlockType];

            if (x2 == x1 && z2 == z1) {
                p.SendMessage("The direction cannot be determined.");
                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _distKey, _intervalKey);
                return;
            }

            if (Math.Abs(x1 - x2) > Math.Abs(z1 - z2)) {
                if (x2 > x1) {
                    p.level.Blockchange(p, (ushort)(x1 + (distance - 1)), y1, z1, blocktype);
                    p.level.Blockchange(p, x1, y1, z1, blocktype);
                    if (interval > 0) {
                        for (ushort offset = interval; x1 + offset < x1 + (distance - 1); offset += interval) {
                            p.level.Blockchange(p, (ushort)(x1 + offset), y1, z1, blocktype);
                        }
                    }
                }
                else {
                    p.level.Blockchange(p, (ushort)(x1 - (distance - 1)), y1, z1, blocktype);
                    p.level.Blockchange(p, (ushort)x1, (ushort)y1, (ushort)z1,  blocktype);
                    if (interval > 0) {
                        for (ushort offset = interval; x1 - (distance - 1) < x1 - offset; offset += interval) {
                            p.level.Blockchange(p, (ushort)(x1 - offset), y1, z1, blocktype);
                        }
                    }
                }
            }
            else {
                if (z2 > z1) {
                    p.level.Blockchange(p, x1, y1, (ushort)(z1 + (distance - 1)), blocktype);
                    p.level.Blockchange(p, x1, y1, z1, blocktype);
                    if (interval > 0) {
                        for (ushort offset = interval; z1 + offset < z1 + (distance - 1); offset += interval) {
                            p.level.Blockchange(p, x1, y1, (ushort)(z1 + offset), blocktype);
                        }
                    }
                }
                else {
                    p.level.Blockchange(p, x1, y1, (ushort)(z1 - (distance - 1)), blocktype);
                    p.level.Blockchange(p, x1, y1, z1, blocktype);
                    if (interval > 0) {
                        for (ushort offset = interval; z1 - (distance - 1) < z1 - offset; offset += interval) {
                            p.level.Blockchange(p, x1, y1, (ushort)(z1 - offset), blocktype);
                        }
                    }
                }
            }
            if (interval > 0) {
                p.SendMessage("Placed blocks " + interval + " apart.");
            }
            else {
                p.SendMessage("Placed blocks " + distance + " apart.");
            }

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _distKey, _intervalKey);
        }

        /// <summary>
        /// Called when /help is used on /splace.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/splace <distance?> [interval?] - Measures a given distance " +
                               "and places the currently held block at each end.");
            p.SendMessage("Optionally places a block at set intervals between them.");
        }
    }
}

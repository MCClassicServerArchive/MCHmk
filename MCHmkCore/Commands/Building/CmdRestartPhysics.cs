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
    public class CmdRestartPhysics : Command {
        /// <summary>
        /// Name of the key used to store and retrieve extra physics information.
        /// </summary>
        private readonly string _extraDataKey = "rp_extradata";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"restart", "physics"});

        public override string Name {
            get {
                return "restartphysics";
            }
        }
        public override string Shortcut {
            get {
                return "rp";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdRestartPhysics(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            args = args.ToLower();

            string extraInfo = String.Empty;

            if (args != String.Empty) {
                int currentLoop = 0;
                string[] storedArray;
                bool skip = false;

                retry:
                foreach (string s in args.Split(' ')) {
                    if (currentLoop % 2 == 0) {
                        switch (s) {
                        case "drop":
                        case "explode":
                        case "dissipate":
                        case "finite":
                        case "wait":
                        case "rainbow":
                            break;
                        case "revert":
                            if (skip) {
                                break;
                            }
                            storedArray = args.Split(' ');
                            try {
                                storedArray[currentLoop + 1] = Convert.ToInt32(BlockData.Ushort(args.Split(' ')[currentLoop + 1].ToLower())).ToString();
                                if (storedArray[currentLoop + 1].ToString() == BlockData.maxblocks.ToString()) {
                                    throw new OverflowException();
                                }
                            }
                            catch {  // TODO: find exact exception to catch
                                p.SendMessage("Invalid block type.");
                                return;
                            }

                            args = string.Join(" ", storedArray);
                            skip = true;
                            currentLoop = 0;

                            goto retry;
                        default:
                            p.SendMessage(s + " is not supported.");
                            return;
                        }
                    }
                    else {
                        try {
                            if (int.Parse(s) < 1) {
                                p.SendMessage("Values must be above 0");
                                return;
                            }
                        }
                        catch {  // TODO: Find exact exception to catch
                            p.SendMessage("/rp [text] [num] [text] [num]");
                            return;
                        }
                    }

                    currentLoop++;
                }

                if (currentLoop % 2 != 1) {
                    extraInfo = args;
                }
                else {
                    p.SendMessage("Number of parameters must be even");
                    Help(p);
                    return;
                }
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_extraDataKey] = extraInfo;

            const string prompt = "Place two blocks to determine the area's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting both corners of the area.
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

            string extraInfo = c.GetData<string>(_extraDataKey);

            List<UShortCoords> buffer = new List<UShortCoords>();

            for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx) {
                for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                    for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                        if (p.level.GetTile(xx, yy, zz) != BlockId.Air) {
                            buffer.Add(new UShortCoords(xx, yy, zz));
                        }
                    }
                }
            }

            try {
                if (extraInfo == String.Empty) {
                    if (buffer.Count > _s.props.rpNormLimit) {
                        p.SendMessage("Cannot restart more than " + _s.props.rpNormLimit + " blocks.");
                        p.SendMessage("Tried to restart " + buffer.Count + " blocks.");
                        TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _extraDataKey);
                        return;
                    }
                }
                else {
                    if (buffer.Count > _s.props.rpLimit) {
                        p.SendMessage("Tried to add physics to " + buffer.Count + " blocks.");
                        p.SendMessage("Cannot add physics to more than " + _s.props.rpLimit + " blocks.");
                        TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _extraDataKey);
                        return;
                    }
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                return;
            }

            foreach (UShortCoords pos1 in buffer) {
                p.level.AddCheck(p.level.PosToInt(pos1.X, pos1.Y, pos1.Z), extraInfo, true);
            }

            p.SendMessage("Activated " + buffer.Count + " blocks.");

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _extraDataKey);
        }

        /// <summary>
        /// Called when /help is used on /restartphysics.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // TODO: Come back to this after the /help additions -Jjp137
            p.SendMessage("/restartphysics [type1?] [num] [type2?] [num] ... - Restarts every physics block in an area.");
            p.SendMessage("The specified type will set custom physics for selected blocks");
            p.SendMessage("Possible types: drop, explode, dissipate, finite, wait, rainbow, revert.");
            p.SendMessage("/restartphysics revert - Takes block names.");
        }

    }
}

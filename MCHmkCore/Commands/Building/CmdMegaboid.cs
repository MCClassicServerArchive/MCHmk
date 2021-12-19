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

using MCHmk.Drawing;

namespace MCHmk.Commands {
    public class CmdMegaboid : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the block type used to draw the megaboid.
        /// </summary>
        private readonly string _blockKey = "megaboid_block";
        /// <summary>
        /// Name of the key used to store and retrieve the shape of the megaboid being drawn.
        /// </summary>
        private readonly string _shapeKey = "megaboid_shape";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"box", "cuboid", "mega"});

        public override string Name {
            get {
                return "megaboid";
            }
        }
        public override string Shortcut {
            get {
                return "zm";
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
        public CmdMegaboid(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("The console may not use megaboid!");
                return;
            }
            if (p.megaBoid == true) {
                p.SendMessage("You may only have one Megaboid going at a time. Use /abort to cancel the current megaboid.");
                return;
            }
            if (p.level.permissionbuild > p.rank.Permission) {
                p.SendMessage("You may not megaboid on this level!");
                return;
            }
            int number = args.Split(' ').Length;
            if (number > 2) {
                Help(p);
                return;
            }

            BlockId type = BlockId.Null;
            CuboidType shape = CuboidType.Solid;

            if (number == 2) {
                int pos = args.IndexOf(' ');
                string t = args.Substring(0, pos).ToLower();
                string s = args.Substring(pos + 1).ToLower();

                type = BlockData.Ushort(t);
                if (type == BlockId.Null) {
                    p.SendMessage("There is no block \"" + t + "\".");
                    return;
                }

                if (!_s.blockPerms.CanPlace(p, type)) {
                    p.SendMessage("Cannot place that.");
                    return;
                }

                if (s == "solid") {
                    shape = CuboidType.Solid;
                }
                else if (s == "hollow") {
                    shape = CuboidType.Hollow;
                }
                else if (s == "walls") {
                    shape = CuboidType.Walls;
                }
                else {
                    Help(p);
                    return;
                }
            }
            else if (args != String.Empty) {
                shape = CuboidType.Solid;
                args = args.ToLower();

                type = BlockId.Null;

                if (args == "solid") {
                    shape = CuboidType.Solid;
                }
                else if (args == "hollow") {
                    shape = CuboidType.Hollow;
                }
                else if (args == "walls") {
                    shape = CuboidType.Walls;
                }
                else {
                    BlockId t = BlockData.Ushort(args);
                    if (t == BlockId.Null) {
                        p.SendMessage("There is no block \"" + args + "\".");
                        return;
                    }
                    if (!_s.blockPerms.CanPlace(p, t)) {
                        p.SendMessage("Cannot place that.");
                        return;
                    }

                    type = t;
                }
            }
            else {
                type = BlockId.Null;
                shape = CuboidType.Solid;
            }

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = type;
            data[_shapeKey] = shape;

            // Prompt the user to place two blocks.
            const string prompt = "Place two blocks to determine the megaboid's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the megaboid to be drawn.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            System.Timers.Timer megaTimer = new System.Timers.Timer(1);

            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            BlockId type = c.GetData<BlockId>(_blockKey);
            CuboidType shape = c.GetData<CuboidType>(_shapeKey);

            if (type == BlockId.Null) {
                type = p.bindings[(int)c.BlockType];
            }

            List<UShortCoords> buffer = new List<UShortCoords>();

            ushort xx;
            ushort yy;
            ushort zz;

            switch (shape) {
                case CuboidType.Solid:
                    buffer.Capacity = Math.Abs(x1 - x2) * Math.Abs(y1 - y2) * Math.Abs(z1 - z2);
                    for (xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx)
                        for (yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy)
                            for (zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                            if (p.level.GetTile(xx, yy, zz) != type) {
                                buffer.Add(new UShortCoords(xx, yy, zz));
                            }
                        }
                    break;
                    case CuboidType.Hollow:
                    //todo work out if theres 800 blocks used before making the buffer
                    for (yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy)
                        for (zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                        if (p.level.GetTile(x1, yy, zz) != type) {
                            buffer.Add(new UShortCoords(x1, yy, zz));
                        }
                        if (x1 != x2) {
                            if (p.level.GetTile(x2, yy, zz) != type) {
                                buffer.Add(new UShortCoords(x2, yy, zz));
                            }
                        }
                    }
                    if (Math.Abs(x1 - x2) >= 2) {
                        for (xx = (ushort)(Math.Min(x1, x2) + 1); xx <= Math.Max(x1, x2) - 1; ++xx)
                            for (zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                            if (p.level.GetTile(xx, y1, zz) != type) {
                                buffer.Add(new UShortCoords(xx, y1, zz));
                            }
                            if (y1 != y2) {
                                if (p.level.GetTile(xx, y2, zz) != type) {
                                    buffer.Add(new UShortCoords(xx, y2, zz));
                                }
                            }
                        }
                        if (Math.Abs(y1 - y2) >= 2) {
                            for (xx = (ushort)(Math.Min(x1, x2) + 1); xx <= Math.Max(x1, x2) - 1; ++xx)
                                for (yy = (ushort)(Math.Min(y1, y2) + 1); yy <= Math.Max(y1, y2) - 1; ++yy) {
                                if (p.level.GetTile(xx, yy, z1) != type) {
                                    buffer.Add(new UShortCoords(xx, yy, z1));
                                }
                                if (z1 != z2) {
                                    if (p.level.GetTile(xx, yy, z2) != type) {
                                        buffer.Add(new UShortCoords(xx, yy, z2));
                                    }
                                }
                            }
                        }
                    }
                    break;
                    case CuboidType.Walls:
                    for (yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy)
                        for (zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                        if (p.level.GetTile(x1, yy, zz) != type) {
                            buffer.Add(new UShortCoords(x1, yy, zz));
                        }
                        if (x1 != x2) {
                            if (p.level.GetTile(x2, yy, zz) != type) {
                                buffer.Add(new UShortCoords(x2, yy, zz));
                            }
                        }
                    }
                    if (Math.Abs(x1 - x2) >= 2) {
                        if (Math.Abs(z1 - z2) >= 2) {
                            for (xx = (ushort)(Math.Min(x1, x2) + 1); xx <= Math.Max(x1, x2) - 1; ++xx)
                                for (yy = (ushort)(Math.Min(y1, y2)); yy <= Math.Max(y1, y2); ++yy) {
                                if (p.level.GetTile(xx, yy, z1) != type) {
                                    buffer.Add(new UShortCoords(xx, yy, z1));
                                }
                                if (z1 != z2) {
                                    if (p.level.GetTile(xx, yy, z2) != type) {
                                        buffer.Add(new UShortCoords(xx, yy, z2));
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            if (buffer.Count > 450000) {
                p.SendMessage("You cannot megaboid more than 450000 blocks.");
                p.SendMessage("You tried to megaboid " + buffer.Count + " blocks.");

                TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
                return;
            }

            p.SendMessage(buffer.Count.ToString() + " blocks.");
            p.SendMessage("Use /abort to cancel the megaboid at any time.");
            p.megaBoid = true;
            UShortCoords pos;
            int CurrentLoop = 0;
            Level currentLevel = p.level;
            megaTimer.Start();
            megaTimer.Elapsed += delegate {
                if (p.megaBoid == true) {
                    pos = buffer[CurrentLoop];
                    try {
                        currentLevel.Blockchange(p, pos.X, pos.Y, pos.Z, type);
                    }
                    catch (Exception e) { 
                        _s.logger.ErrorLog(e);
                    }
                    CurrentLoop++;
                    if (CurrentLoop % 1000 == 0) {
                        p.SendMessage(CurrentLoop + " blocks down, " + (buffer.Count - CurrentLoop) + " to go.");
                    }
                    if (CurrentLoop >= buffer.Count) {
                        p.SendMessage("Completed megaboid");
                        buffer.Clear();
                        p.megaBoid = false;
                        megaTimer.Stop();
                    }
                }
                else {
                    megaTimer.Stop();
                }
            };

            TwoBlockSelection.RestartIfStatic(p, c, SelectionFinished, _blockKey, _shapeKey);
        }

        /// <summary>
        /// Called when /help is used on /megaboid.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/megaboid [block?] [solid/hollow/walls] - " +
                               "Creates a cuboid of blocks at a slow pace.");
            p.SendMessage("If a block is not specified, the currently held block is used.");
            p.SendMessage("Unlike /cuboid, megaboid displays the current status.");
        }
    }
}

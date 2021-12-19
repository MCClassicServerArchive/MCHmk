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
using System.Threading;

namespace MCHmk.Commands {
    /// <summary>
    /// This is the command /gun
    /// use /help gun in-game for more info
    /// </summary>
    public class CmdGun : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the type of gun being used.
        /// </summary>
        private static readonly string _endingKey = "gun_ending";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"shoot", "boom", "terrorism", "missile"});

        public override string Name {
            get {
                return "gun";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "other";
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
        public CmdGun(Server s) : base(s) { }
        public override void Use(Player p, string args) {
            Level foundLevel;
            foundLevel = p.level;
            if (foundLevel.guns == false) {
                p.SendMessage("Guns and missiles cannot be used on this map!");
                return;
            }
            int ending = 0;

            if (p.aiming) {
                if (args == String.Empty) {
                    p.aiming = false;
                    p.ClearSelection();
                    p.SendMessage("Disabled gun");
                    return;
                }
            }

            ending = 0;
            if (args.ToLower() == "destroy") {
                ending = 1;
            }
            else if (p.allowTnt == false) {
                if (args.ToLower() == "explode") {
                    p.SendMessage(_s.props.DefaultColor + "Since tnt usage is disallowed at the moment, switching to normal gun!");
                    ending = 1;
                }
                else if (args.ToLower() == "laser") {
                    p.SendMessage(_s.props.DefaultColor + "Since tnt usage is disallowed at the moment, switching to normal gun!");
                    ending = 1;
                }
                else if (args.ToLower() == "teleport" || args.ToLower() == "tp") {
                    ending = -1;
                }
                else if (args != String.Empty) {
                    Help(p);
                    return;
                }
            }


            else if (args.ToLower() == "explode") {
                ending = 2;
            }
            else if (args.ToLower() == "laser") {
                ending = 3;
            }
            else if (args.ToLower() == "teleport" || args.ToLower() == "tp") {
                ending = -1;
            }
            else if (args != String.Empty) {
                Help(p);
                return;
            }

            p.ClearSelection();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_endingKey] = ending;

            p.StartSelection(BlockSelected, data);
            p.SendMessage("Gun mode engaged, fire at will");

            if (p.aiming) {
                return;
            }

            p.aiming = true;
            Thread aimThread = new Thread(new ThreadStart(delegate {
                UShortCoords pos;
                List<UShortCoords> buffer = new List<UShortCoords>();
                while (p.aiming) {
                    List<UShortCoords> tempBuffer = new List<UShortCoords>();

                    double a = Math.Sin(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                    double b = Math.Cos(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                    double c = Math.Cos(((double)(p.rot[1] + 64) / 256) * 2 * Math.PI);

                    try {
                        ushort x = (ushort)(p.pos[0] / 32);
                        x = (ushort)Math.Round(x + (double)(a * 3));

                        ushort y = (ushort)(p.pos[1] / 32 + 1);
                        y = (ushort)Math.Round(y + (double)(c * 3));

                        ushort z = (ushort)(p.pos[2] / 32);
                        z = (ushort)Math.Round(z + (double)(b * 3));

                        if (x > p.level.width || y > p.level.height || z > p.level.depth) {
                            throw new Exception();
                        }
                        if (x < 0 || y < 0 || z < 0) {
                            throw new Exception();
                        }

                        for (ushort xx = x; xx <= x + 1; xx++) {
                            for (ushort yy = (ushort)(y - 1); yy <= y; yy++) {
                                for (ushort zz = z; zz <= z + 1; zz++) {
                                    if (p.level.GetTile(xx, yy, zz) == BlockId.Air) {
                                        pos.X = xx;
                                        pos.Y = yy;
                                        pos.Z = zz;
                                        tempBuffer.Add(pos);
                                    }
                                }
                            }
                        }

                        List<UShortCoords> toRemove = new List<UShortCoords>();
                        foreach (UShortCoords cP in buffer) {
                            if (!tempBuffer.Contains(cP)) {
                                p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Air);
                                toRemove.Add(cP);
                            }
                        }

                        foreach (UShortCoords cP in toRemove) {
                            buffer.Remove(cP);
                        }

                        foreach (UShortCoords cP in tempBuffer) {
                            if (!buffer.Contains(cP)) {
                                buffer.Add(cP);
                                p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Glass);
                            }
                        }

                        tempBuffer.Clear();
                        toRemove.Clear();
                    }
                    catch (Exception e) { 
                        _s.logger.ErrorLog(e);
                    }
                    Thread.Sleep(20);
                }

                foreach (UShortCoords cP in buffer) {
                    p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Air);
                }
            }));
            aimThread.Start();
        }

        private void BlockSelected(Player p, CommandTempData cd) {
            ushort x = cd.X;
            ushort y = cd.Y;
            ushort z = cd.Z;

            BlockId by = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, by);
            BlockId type = cd.BlockType;

            int ending = cd.GetData<int>(_endingKey);

            double a = Math.Sin(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
            double b = Math.Cos(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
            double c = Math.Cos(((double)(p.rot[1] + 64) / 256) * 2 * Math.PI);

            double bigDiag = Math.Sqrt(Math.Sqrt(p.level.width * p.level.width + p.level.depth * p.level.depth) + p.level.height *
                                       p.level.height + p.level.width * p.level.width);

            List<UShortCoords> previous = new List<UShortCoords>();
            List<UShortCoords> allBlocks = new List<UShortCoords>();
            UShortCoords pos;

            if (p.modeType != BlockId.Air) {
                type = p.modeType;
            }

            Thread gunThread = new Thread(new ThreadStart(delegate {
                ushort startX = (ushort)(p.pos[0] / 32);
                ushort startY = (ushort)(p.pos[1] / 32);
                ushort startZ = (ushort)(p.pos[2] / 32);

                pos.X = (ushort)Math.Round(startX + (double)(a * 3));
                pos.Y = (ushort)Math.Round(startY + (double)(c * 3));
                pos.Z = (ushort)Math.Round(startZ + (double)(b * 3));

                for (double t = 4; bigDiag > t; t++) {
                    pos.X = (ushort)Math.Round(startX + (double)(a * t));
                    pos.Y = (ushort)Math.Round(startY + (double)(c * t));
                    pos.Z = (ushort)Math.Round(startZ + (double)(b * t));

                    by = p.level.GetTile(pos.X, pos.Y, pos.Z);

                    if (by != BlockId.Air && !allBlocks.Contains(pos)) {
                        if (p.level.physics < 2 || ending <= 0) {
                            break;
                        }
                        else {
                            if (ending == 1) {
                                if ((!BlockData.LavaKill(by) && !BlockData.NeedRestart(by)) && by != BlockId.Glass) {
                                    break;
                                }
                            }
                            else if (p.level.physics >= 3) {
                                if (by != BlockId.Glass) {
                                    if (p.allowTnt == true) {
                                        p.level.MakeExplosion(pos.X, pos.Y, pos.Z, 1);
                                        break;
                                    }
                                }
                            }
                            else {
                                break;
                            }
                        }
                    }

                    p.level.Blockchange(pos.X, pos.Y, pos.Z, type);
                    previous.Add(pos);
                    allBlocks.Add(pos);

                    bool comeOut = false;
                    foreach (Player pl in _s.players) {
                        if (pl.level == p.level) {
                            if ((ushort)(pl.pos[0] / 32) == pos.X || (ushort)(pl.pos[0] / 32  + 1) == pos.X
                                || (ushort)(pl.pos[0] / 32 - 1) == pos.X) {
                                if ((ushort)(pl.pos[1] / 32) == pos.Y || (ushort)(pl.pos[1] / 32 + 1) == pos.Y
                                    || (ushort)(pl.pos[1] / 32 - 1) == pos.Y) {
                                    if ((ushort)(pl.pos[2] / 32) == pos.Z || (ushort)(pl.pos[2] / 32 + 1) == pos.Z
                                        || (ushort)(pl.pos[2] / 32 - 1) == pos.Z) {
                                        if (p.level.physics >= 3 && ending >= 2) {
                                            pl.HandleDeath(BlockId.Cobblestone, " was blown up by " + p.color + p.name, true);
                                        }
                                        else {
                                            pl.HandleDeath(BlockId.Cobblestone, " was shot by " + p.color + p.name);
                                        }
                                        comeOut = true;
                                    }
                                }
                            }
                        }
                    }
                    if (comeOut) {
                        break;
                    }

                    if (t > 12 && ending != 3) {
                        pos = previous[0];
                        p.level.Blockchange(pos.X, pos.Y, pos.Z, BlockId.Air);
                        previous.Remove(pos);
                    }

                    if (ending != 3) {
                        Thread.Sleep(20);
                    }
                }

                if (ending == -1)
                    try {
                    unchecked {
                        p.SendPos((byte)-1, (ushort)(previous[previous.Count - 3].X * 32), (ushort)(previous[previous.Count - 3].Y * 32 + 32),
                                  (ushort)(previous[previous.Count - 3].Z * 32), p.rot[0], p.rot[1]);
                    }
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }
                if (ending == 3) {
                    Thread.Sleep(400);
                }

                foreach (UShortCoords pos1 in previous) {
                    p.level.Blockchange(pos1.X, pos1.Y, pos1.Z, BlockId.Air);
                    if (ending != 3) {
                        Thread.Sleep(20);
                    }
                }
            }));
            gunThread.Start();
        }

        /// <summary>
        /// Called when /help is used on /gun.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/gun [type?] - Allows you to fire bullets.");
            p.SendMessage("Available types of guns: &cexplode, destroy, laser, tp");
        }
    }
}

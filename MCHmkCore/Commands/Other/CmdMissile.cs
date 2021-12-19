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
    public class CmdMissile : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the type of missile being shot.
        /// </summary>
        private static readonly string _endingKey = "missile_ending";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"gun", "missil"});  // Intentional typo? -Jjp137

        public override string Name {
            get {
                return "missile";
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

        public CmdMissile(Server s) : base(s) { }

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
                    p.SendMessage("Disabled missiles");
                    return;
                }
            }

            ending = 0;
            if (args.ToLower() == "destroy") {
                ending = 1;
            }
            if (p.allowTnt == false) {
                if (args.ToLower() == "explode") {
                    p.SendMessage("Since tnt usage is currently disabled, normal missile enabled");
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
            p.SendMessage("Missile mode engaged, fire and guide!");

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
            if (!p.staticCommands) {
                p.ClearSelection();
                p.aiming = false;
            }

            ushort x = cd.X;
            ushort y = cd.Y;
            ushort z = cd.Z;

            BlockId by = p.level.GetTile(x, y, z);
            p.SendBlockchange(x, y, z, by);
            BlockId type = cd.BlockType;

            int ending = cd.GetData<int>(_endingKey);

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
                pos.X = startX;
                pos.Y = startY;
                pos.Z = startZ;

                int total = 0;

                while (true) {
                    startX = (ushort)(p.pos[0] / 32);
                    startY = (ushort)(p.pos[1] / 32);
                    startZ = (ushort)(p.pos[2] / 32);

                    total++;
                    double a = Math.Sin(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                    double b = Math.Cos(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                    double c = Math.Cos(((double)(p.rot[1] + 64) / 256) * 2 * Math.PI);

                    UShortCoords lookedAt;
                    int i;
                    for (i = 1; true; i++) {
                        lookedAt.X = (ushort)Math.Round(startX + (double)(a * i));
                        lookedAt.Y = (ushort)Math.Round(startY + (double)(c * i));
                        lookedAt.Z = (ushort)Math.Round(startZ + (double)(b * i));

                        by = p.level.GetTile(lookedAt.X, lookedAt.Y, lookedAt.Z);

                        if (by == BlockId.Null) {
                            break;
                        }

                        if (by != BlockId.Air && !allBlocks.Contains(lookedAt)) {
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
                                        break;
                                    }
                                }
                                else {
                                    break;
                                }
                            }
                        }

                        bool comeInner = false;
                        foreach (Player pl in _s.players) {
                            if (pl.level == p.level && pl != p) {
                                if ((ushort)(pl.pos[0] / 32) == lookedAt.X || (ushort)(pl.pos[0] / 32 + 1) == lookedAt.X
                                        || (ushort)(pl.pos[0] / 32 - 1) == lookedAt.X) {
                                    if ((ushort)(pl.pos[1] / 32) == lookedAt.Y || (ushort)(pl.pos[1] / 32 + 1) == lookedAt.Y
                                            || (ushort)(pl.pos[1] / 32 - 1) == lookedAt.Y) {
                                        if ((ushort)(pl.pos[2] / 32) == lookedAt.Z || (ushort)(pl.pos[2] / 32 + 1) == lookedAt.Z
                                                || (ushort)(pl.pos[2] / 32 - 1) == lookedAt.Z) {
                                            lookedAt.X = (ushort)(pl.pos[0] / 32);
                                            lookedAt.Y = (ushort)(pl.pos[1] / 32);
                                            lookedAt.Z = (ushort)(pl.pos[2] / 32);
                                            comeInner = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (comeInner) {
                            break;
                        }
                    }

                    lookedAt.X = (ushort)Math.Round(startX + (double)(a * (i - 1)));
                    lookedAt.Y = (ushort)Math.Round(startY + (double)(c * (i - 1)));
                    lookedAt.Z = (ushort)Math.Round(startZ + (double)(b * (i - 1)));

                    findNext(lookedAt, ref pos);

                    by = p.level.GetTile(pos.X, pos.Y, pos.Z);
                    if (total > 3) {
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
                                        break;
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
                            if (pl.level == p.level && pl != p) {
                                if ((ushort)(pl.pos[0] / 32) == pos.X || (ushort)(pl.pos[0] / 32 + 1) == pos.X
                                        || (ushort)(pl.pos[0] / 32 - 1) == pos.X) {
                                    if ((ushort)(pl.pos[1] / 32) == pos.Y || (ushort)(pl.pos[1] / 32 + 1) == pos.Y
                                            || (ushort)(pl.pos[1] / 32 - 1) == pos.Y) {
                                        if ((ushort)(pl.pos[2] / 32) == pos.Z || (ushort)(pl.pos[2] / 32 + 1) == pos.Z
                                                || (ushort)(pl.pos[2] / 32 - 1) == pos.Z) {
                                            if (p.level.physics >= 3 && ending >= 2) {
                                                pl.HandleDeath(BlockId.Cobblestone, " was blown up by " + p.color + p.name, true);
                                            }
                                            else {
                                                pl.HandleDeath(BlockId.Cobblestone, " was hit a missile from " + p.color + p.name);
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

                        if (pos.X == lookedAt.X && pos.Y == lookedAt.Y && pos.Z == lookedAt.Z) {
                            if (p.level.physics >= 3 && ending >= 2) {
                                if (p.allowTnt == true) {
                                    p.level.MakeExplosion(lookedAt.X, lookedAt.Y, lookedAt.Z, 2);
                                    break;
                                }
                            }
                        }

                        if (previous.Count > 12) {
                            p.level.Blockchange(previous[0].X, previous[0].Y, previous[0].Z, BlockId.Air);
                            previous.Remove(previous[0]);
                        }
                        Thread.Sleep(100);
                    }
                }

                foreach (UShortCoords pos1 in previous) {
                    p.level.Blockchange(pos1.X, pos1.Y, pos1.Z, BlockId.Air);
                    Thread.Sleep(100);
                }
            }));
            gunThread.Start();
        }

        public void findNext(UShortCoords lookedAt, ref UShortCoords pos) {
            int dx, dy, dz, l, m, n, x_inc, y_inc, z_inc, err_1, err_2, dx2, dy2, dz2;
            int[] pixel = new int[3];

            pixel[0] = pos.X;
            pixel[1] = pos.Y;
            pixel[2] = pos.Z;
            dx = lookedAt.X - pos.X;
            dy = lookedAt.Y - pos.Y;
            dz = lookedAt.Z - pos.Z;

            x_inc = (dx < 0) ? -1 : 1;
            l = Math.Abs(dx);
            y_inc = (dy < 0) ? -1 : 1;
            m = Math.Abs(dy);
            z_inc = (dz < 0) ? -1 : 1;
            n = Math.Abs(dz);

            dx2 = l << 1;
            dy2 = m << 1;
            dz2 = n << 1;

            if ((l >= m) && (l >= n)) {
                err_1 = dy2 - l;
                err_2 = dz2 - l;

                pixel[0] += x_inc;
                if (err_1 > 0) {
                    pixel[1] += y_inc;
                    err_1 -= dx2;
                }
                if (err_2 > 0) {
                    pixel[2] += z_inc;
                    err_2 -= dx2;
                }
                err_1 += dy2;
                err_2 += dz2;

                pos.X = (ushort)pixel[0];
                pos.Y = (ushort)pixel[1];
                pos.Z = (ushort)pixel[2];
            }
            else if ((m >= l) && (m >= n)) {
                err_1 = dx2 - m;
                err_2 = dz2 - m;

                pixel[1] += y_inc;
                if (err_1 > 0) {
                    pixel[0] += x_inc;
                    err_1 -= dy2;
                }
                if (err_2 > 0) {
                    pixel[2] += z_inc;
                    err_2 -= dy2;
                }
                err_1 += dx2;
                err_2 += dz2;

                pos.X = (ushort)pixel[0];
                pos.Y = (ushort)pixel[1];
                pos.Z = (ushort)pixel[2];
            }
            else {
                err_1 = dy2 - n;
                err_2 = dx2 - n;

                pixel[2] += z_inc;
                if (err_1 > 0) {
                    pixel[1] += y_inc;
                    err_1 -= dz2;
                }
                if (err_2 > 0) {
                    pixel[0] += x_inc;
                    err_2 -= dz2;
                }
                err_1 += dy2;
                err_2 += dx2;

                pos.X = (ushort)pixel[0];
                pos.Y = (ushort)pixel[1];
                pos.Z = (ushort)pixel[2];
            }
        }

        /// <summary>
        /// Called when /help is used on /missile.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/missile [type?] - Allows you to fire missiles.");
            p.SendMessage("Missile types:");
            p.SendMessage("explode - Missiles explode on contact.");
            p.SendMessage("destroy - Missiles destroy blocks.");
            p.SendMessage("Unlike /gun, the missile is controlled.");
        }
    }
}

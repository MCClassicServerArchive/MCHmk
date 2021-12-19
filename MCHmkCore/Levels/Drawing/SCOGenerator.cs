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

//StormCom Object Generator
//
//Full use to all StormCom Server System codes (in regards to minecraft classic) have been granted to McForge without restriction.
//
// ~Merlin33069

using System;
using System.Collections.Generic;

namespace MCHmk.Drawing {
    public static class SCOGenerator {
        static Random random = new Random();

        public static void AddTree(Player p, ushort x, ushort y, ushort z, BlockId type) {
            ushort num = (ushort)random.Next(5, 8);
            for (ushort i = 0; i < num; i = (ushort)(i + 1)) {
                p.level.Blockchange(p, x, (ushort)(y + i), z, BlockId.TreeWood);
            }
            short num3 = (short)(num - random.Next(2, 4));
            for (short j = Convert.ToInt16(-num3); j <= num3; j = (short)(j + 1)) {
                if ((x + j) < 0 || (x + j) > p.level.width) {
                    continue;
                }
                for (short k = Convert.ToInt16(-num3); k <= num3; k = (short)(k + 1)) {
                    if ((y + k) < 0 || (y + k) > p.level.height) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-num3); m <= num3; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }
                        short maxValue = (short)Math.Sqrt((double)(((j * j) + (k * k)) + (m * m)));
                        if ((maxValue < (num3 + 1)) && (random.Next(maxValue) < 2)) {
                            try {
                                BlockId that = p.level.GetTile((ushort)(x + j), (ushort)((y + k) + num), (ushort)(z + m));
                                if (that == 0) {
                                    p.level.Blockchange(p, (ushort)(x + j), (ushort)((y + k) + num), (ushort)(z + m), BlockId.Leaves);
                                }
                            }
                            catch (Exception e) {
                                p.Logger.ErrorLog(e);
                            }
                        }
                    }
                }
            }
        }

        public static void AddCactus(Level l, ushort x, ushort y, ushort z, BlockId type) {
            ushort num2;
            ushort num = (ushort)random.Next(3, 6);
            for (num2 = 0; num2 <= num; num2 = (ushort)(num2 + 1)) {
                l.Blockchange(x, (ushort)(y + num2), z, BlockId.Green);
            }
            int num3 = 0;
            int num4 = 0;
            switch (random.Next(1, 3)) {
            case 1:
                num3 = -1;
                break;

            default:
                num4 = -1;
                break;
            }
            num2 = num;
            while (num2 <= random.Next(num + 2, num + 5)) {
                ushort x2 = (ushort)(x + num3); //width
                ushort y2 = (ushort)(y + num2); //depth
                ushort z2 = (ushort)(z + num4); //height
                if (l.GetTile(x2, y2, z2) == 0) {
                    l.Blockchange((ushort)(x + num3), (ushort)(y + num2), (ushort)(z + num4), BlockId.Green);
                }
                num2 = (ushort)(num2 + 1);
            }
            for (num2 = num; num2 <= random.Next(num + 2, num + 5); num2 = (ushort)(num2 + 1)) {
                ushort x2 = (ushort)(x - num3); //width
                ushort y2 = (ushort)(y + num2); //depth
                ushort z2 = (ushort)(z - num4); //height
                if (l.GetTile(x2, y2, z2) == 0) {
                    l.Blockchange((ushort)(x - num3), (ushort)(y + num2), (ushort)(z - num4), BlockId.Green);
                }
            }
        }

        public static void Nuke(Level l, ushort x, ushort y, ushort z) {
            foreach (Player p in l.Server.players) {
                if (p.level == l) {
                    p.SendBlockchange(x, y, z, 0);
                }
            }

            short num3 = (short)(random.Next(15, 20));

            for (short j = Convert.ToInt16(-num3); j <= num3; j = (short)(j + 1)) {
                if ((x + j) < 0 || (x + j) > l.width) {
                    continue;
                }
                for (short k = Convert.ToInt16(-num3); k <= num3; k = (short)(k + 1)) {
                    if ((y + k) < 0 || (y + k) > l.height) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-num3); m <= num3; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > l.length) {
                            continue;
                        }
                        short maxValue = (short)Math.Sqrt((double)(((j * j) + (k * k)) + (m * m)));
                        if ((maxValue < (num3 + 1)) && (random.Next(maxValue) < 15)) {
                            try {
                                ushort x2 = (ushort)(x + j); //width
                                ushort y2 = (ushort)(y + k); //depth
                                ushort z2 = (ushort)(z + m); //height

                                if (x2 <= l.width && y2 <= l.height && z2 <= l.length) {
                                    BlockId that = l.GetTile((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m));

                                    if (that != BlockId.Bedrock) {
                                        l.Blockchange((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m), 0);
                                    }
                                }
                            }
                            catch (Exception e) {
                                l.Logger.Log(e.Message);
                            }
                        }
                    }
                }
            }
        }

        public static void NukeS(Level l, ushort x, ushort y, ushort z, int size) {
            foreach (Player p in l.Server.players) {
                if (p.level == l) {
                    p.SendBlockchange(x, y, z, 0);
                }
            }

            int num3 = size;

            for (short j = Convert.ToInt16(-num3); j <= num3; j = (short)(j + 1)) {
                if ((x + j) < 0 || (x + j) > l.width) {
                    continue;
                }
                for (short k = Convert.ToInt16(-num3); k <= num3; k = (short)(k + 1)) {
                    if ((y + k) < 0 || (y + k) > l.height) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-num3); m <= num3; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > l.length) {
                            continue;
                        }

                        short maxValue = (short)Math.Sqrt((double)(((j * j) + (k * k)) + (m * m))); //W00t FOUND THE SECRET!
                        if ((maxValue < (num3 + 1)) && (random.Next(maxValue) < size)) {
                            try {
                                ushort x2 = (ushort)(x + j); //width
                                ushort y2 = (ushort)(y + k); //depth
                                ushort z2 = (ushort)(z + m); //height

                                if (x2 <= l.width && y2 <= l.height && z2 <= l.length) {
                                    BlockId that = l.GetTile((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m));

                                    if (that != BlockId.Bedrock) {
                                        l.Blockchange((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m), 0);
                                    }
                                }
                            }
                            catch (Exception e) {
                                l.Logger.Log(e.Message);
                            }
                        }
                    }
                }
            }
        }

        public static void Cone(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = height - k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            double pointradius = Math.Sqrt((absx * absx) + (absz * absz));

                            if (pointradius <= currentradius) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = block;
                                    buffer.Add(temp);
                                }
                            }
                        }

                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried Coning " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y + height), z, block);
            }
        }

        public static void HCone(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();

            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = height - k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            double pointradius = Math.Sqrt((absx * absx) + (absz * absz));

                            if (pointradius <= currentradius && pointradius >= (currentradius - 1)) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = block;
                                    buffer.Add(temp);
                                }
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried HConing " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y + height), z, block);
            }
        }

        public static void ICone(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();

            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            double pointradius = Math.Sqrt((absx * absx) + (absz * absz));

                            if (pointradius <= currentradius) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = block;
                                    buffer.Add(temp);
                                }
                            }
                        }

                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried IConing " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            p.level.Blockchange(p, x, y, z, block);
        }

        public static void HICone(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();

            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            double pointradius = Math.Sqrt((absx * absx) + (absz * absz));

                            if (pointradius <= currentradius && pointradius >= (currentradius - 1)) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = block;
                                    buffer.Add(temp);
                                }
                            }
                        }

                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried HIConing " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            p.level.Blockchange(p, x, y, z, block);
        }

        //For the pyramid commands, Radius still refers to the distance from the center point, but is axis independant, rather than a referance to both axes
        public static void Pyramid(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = height - k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            if (absx > currentradius) {
                                continue;
                            }
                            if (absz > currentradius) {
                                continue;
                            }

                            BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                            if (ctile == 0) {
                                Player.CopyPos temp = new Player.CopyPos();
                                temp.x = (ushort)cx;
                                temp.y = (ushort)cy;
                                temp.z = (ushort)cz;
                                temp.type = block;
                                buffer.Add(temp);
                            }
                        }

                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried Pyramiding " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y + height), z, block);
            }
        }

        public static void HPyramid(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = height - k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            if (absx > currentradius || absz > currentradius) {
                                continue;
                            }
                            if (absx < (currentradius - 1) && absz < (currentradius - 1)) {
                                continue;
                            }

                            BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                            if (ctile == 0) {
                                Player.CopyPos temp = new Player.CopyPos();
                                temp.x = (ushort)cx;
                                temp.y = (ushort)cy;
                                temp.z = (ushort)cz;
                                temp.type = block;
                                buffer.Add(temp);
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried HPyramiding " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y), z, block);
            }
        }

        public static void IPyramid(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            if (absx > currentradius) {
                                continue;
                            }
                            if (absz > currentradius) {
                                continue;
                            }

                            BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                            if (ctile == 0) {
                                Player.CopyPos temp = new Player.CopyPos();
                                temp.x = (ushort)cx;
                                temp.y = (ushort)cy;
                                temp.z = (ushort)cz;
                                temp.type = block;
                                buffer.Add(temp);
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried IPyramiding " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y + height), z, block);
            }
        }

        public static void HIPyramid(Player p, ushort x, ushort y, ushort z, int height, int radius, BlockId block) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            if (absx > currentradius || absz > currentradius) {
                                continue;
                            }
                            if (absx < (currentradius - 1) && absz < (currentradius - 1)) {
                                continue;
                            }

                            BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                            if (ctile == 0) {
                                Player.CopyPos temp = new Player.CopyPos();
                                temp.x = (ushort)cx;
                                temp.y = (ushort)cy;
                                temp.z = (ushort)cz;
                                temp.type = block;
                                buffer.Add(temp);
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried HIPyramiding " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
            if ((y + height) <= p.level.height) {
                p.level.Blockchange(p, x, (ushort)(y), z, block);
            }
        }

        public static void Sphere(Player p, ushort x, ushort y, ushort z, int radius, BlockId type) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                if ((x + j) < 0 || (x + j) > p.level.width) {
                    continue;
                }
                for (short k = Convert.ToInt16(-radius); k <= radius; k = (short)(k + 1)) {
                    if ((y + k) < 0 || (y + k) > p.level.height) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }
                        short maxValue = (short)Math.Sqrt((double)(((j * j) + (k * k)) + (m * m)));
                        if ((maxValue < (radius + 1))) {
                            try {
                                ushort x2 = (ushort)(x + j);
                                ushort y2 = (ushort)(y + k);
                                ushort z2 = (ushort)(z + m);
                                if (x2 <= p.level.width && y2 <= p.level.height && z2 <= p.level.length) {
                                    BlockId that = p.level.GetTile((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m));
                                    if (that != BlockId.Bedrock) {
                                        Player.CopyPos temp = new Player.CopyPos();
                                        temp.x = (ushort)(x + j);
                                        temp.y = (ushort)(y + k);
                                        temp.z = (ushort)(z + m);
                                        temp.type = type;
                                        buffer.Add(temp);
                                    }
                                }
                            }
                            catch (Exception e) {
                                p.Logger.ErrorLog(e);
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried Sphering " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
        }

        public static void HSphere(Player p, ushort x, ushort y, ushort z, int radius, BlockId type) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();
            for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                if ((x + j) < 0 || (x + j) > p.level.width) {
                    continue;
                }
                for (short k = Convert.ToInt16(-radius); k <= radius; k = (short)(k + 1)) {
                    if ((y + k) < 0 || (y + k) > p.level.height) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }
                        short maxValue = (short)Math.Sqrt((double)(((j * j) + (k * k)) + (m * m)));
                        if (maxValue < (radius + 1) && maxValue >= (radius - 1)) {
                            try {
                                ushort x2 = (ushort)(x + j);
                                ushort y2 = (ushort)(y + k);
                                ushort z2 = (ushort)(z + m);
                                if (x2 <= p.level.width && y2 <= p.level.height && z2 <= p.level.length) {
                                    BlockId that = p.level.GetTile((ushort)(x + j), (ushort)((y + k)), (ushort)(z + m));
                                    if (that != BlockId.Bedrock) {
                                        Player.CopyPos temp = new Player.CopyPos();
                                        temp.x = (ushort)(x + j);
                                        temp.y = (ushort)(y + k);
                                        temp.z = (ushort)(z + m);
                                        temp.type = type;
                                        buffer.Add(temp);
                                    }
                                }
                            }
                            catch (Exception e) {
                                p.Logger.ErrorLog(e);
                            }
                        }
                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried HSphering " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
        }

        public static void Volcano(Player p, ushort x, ushort y, ushort z, int height, int radius) {
            List<Player.CopyPos> buffer = new List<Player.CopyPos>();

            for (short k = 0; k <= height; k = (short)(k + 1)) {
                if ((y + k) < 0 || (y + k) > p.level.height) {
                    continue;
                }
                for (short j = Convert.ToInt16(-radius); j <= radius; j = (short)(j + 1)) {
                    if ((x + j) < 0 || (x + j) > p.level.width) {
                        continue;
                    }
                    for (short m = Convert.ToInt16(-radius); m <= radius; m = (short)(m + 1)) {
                        if ((z + m) < 0 || (z + m) > p.level.length) {
                            continue;
                        }

                        int cx = (x + j);
                        int cy = (y + k);
                        int cz = (z + m);

                        double currentheight = height - k;

                        double currentradius;
                        if (currentheight == 0) {
                        }
                        else {
                            currentradius = (double)((double)radius * (double)((double)currentheight / (double)height));
                            int absx = Math.Abs(j);
                            int absz = Math.Abs(m);

                            double pointradius = Math.Sqrt((absx * absx) + (absz * absz));

                            if (pointradius <= currentradius && pointradius >= (currentradius - 1)) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = BlockId.Grass;
                                    buffer.Add(temp);
                                }
                            }
                            else if (pointradius <= currentradius) {
                                BlockId ctile = p.level.GetTile((ushort)cx, (ushort)cy, (ushort)cz);
                                if (ctile == 0) {
                                    Player.CopyPos temp = new Player.CopyPos();
                                    temp.x = (ushort)cx;
                                    temp.y = (ushort)cy;
                                    temp.z = (ushort)cz;
                                    temp.type = BlockId.ActiveLava;
                                    buffer.Add(temp);
                                }
                            }
                        }

                    }
                }
            }
            if (buffer.Count > p.rank.maxBlocks) {
                p.SendMessage("You tried Volcanoing " + buffer.Count + " blocks, your limit is " + p.rank.maxBlocks);
                buffer = null;
                return;
            }
            buffer.ForEach(delegate(Player.CopyPos pos) {
                p.level.Blockchange(p, pos.x, pos.y, pos.z, pos.type);
            });
            buffer = null;
        }
    }
}

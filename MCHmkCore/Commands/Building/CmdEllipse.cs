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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdEllipse : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"create", "art"});

        public override string Name {
            get {
                return "ellipse";
            }
        }
        public override string Shortcut {
            get {
                return "el";
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
        public CmdEllipse(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                Help(p);
                return;
            }

            const string prompt = "Place two blocks to determine the ellipse's area.";
            TwoBlockSelection.Start(p, null, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the area that the ellipse will cover.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData cd) {
            BlockId type = cd.BlockType;

            double x1 = cd.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = cd.GetData<ushort>(TwoBlockSelection.YKey);
            double z1 = cd.GetData<ushort>(TwoBlockSelection.ZKey);

            double x2 = cd.X;
            ushort y2 = cd.Y;
            double z2 = cd.Z;

            int height = Math.Abs(y1 - y2) + 1;

            double xstart = Math.Min(x1, x2);
            double zstart = Math.Min(z1, z2);

            double a = ((Math.Abs(x1 - x2) + 1) / 2);
            double b = ((Math.Abs(z1 - z2) + 1) / 2);

            int dimensionx = (int)(Math.Abs(x1 - x2) + 1);
            int dimensionz = (int)(Math.Abs(z1 - z2) + 1);

            bool OVX;
            bool OVZ;

            double[] yc = new double[dimensionx / 2 + 1];
            double[] length = new double[dimensionx / 2 + 1];

            if (dimensionx % 2 == 0) { // x is even
                OVX = false;
            }
            else {
                // It's odd
                OVX = true;
            }

            if (dimensionz % 2 == 0) {
                OVZ = false;
            }
            else {
                OVZ = true;
            }

            int limit = 0;

            for (int i = 0; i < ((int)a); i++) {
                if (i == 0) {
                    yc[i] = Math.Ceiling(Math.Abs(Math.Sqrt((Math.Pow(a, 2) - Math.Pow(i + 0.5, 2))) * Math.Abs(b / a)));
                    length[i] = 0;
                    limit++;
                }
                else {
                    if (i == (int)a - 1) {
                        yc[i] = 1;
                        if (yc[i - 1] - yc[i] > 1) {
                            length[i] = Math.Round(yc[i - 1]) - Math.Round(yc[i]) - 1;
                            limit++;
                        }
                        else {
                            length[i] = 0;
                            limit++;
                        }
                    }
                    else {
                        yc[i] = Math.Abs(Math.Sqrt((Math.Pow(a, 2) - Math.Pow(i + 0.5, 2))) * Math.Abs(b / a));
                        if (yc[i - 1] - yc[i] > 1) {
                            length[i] = Math.Round(yc[i - 1]) - Math.Round(yc[i]) - 1;
                            limit++;
                        }
                        else {
                            length[i] = 0;
                            limit++;
                        }
                    }
                }
            }

            if ((limit * height) > p.rank.maxBlocks) {
                p.SendMessage("You tried to place " + (limit * height) + " blocks.");
                p.SendMessage("You cannot replace more than " + p.rank.maxBlocks + ".");
                TwoBlockSelection.RestartIfStatic(p, cd, SelectionFinished);
                return;
            }

            //firststart
            int startx = (int)xstart + (int)a;
            int startz = (int)zstart + (int)b;

            ushort starth = Math.Min(y1, y2);

            for (int h = 0; h < height; h++) {
                for (int i = 0; i < ((int)a); i++) {
                    if (length[i] == 0) {
                        p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1)), type);
                    }
                    if (length[i] != 0) {
                        for (int ii = 0; ii <= length[i]; ii++) {
                            p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1 + ii)), type);
                        }
                    }
                }
                if (OVX) {
                    for (int i = 0; i < ((int)a); i++) {
                        if (length[i] == 0) {
                            p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1)), type);
                        }
                        if (length[i] != 0) {
                            for (int ii = 0; ii <= length[i]; ii++) {
                                p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1 + ii)), type);
                            }
                        }
                    }
                }
                else {
                    for (int i = 0; i < ((int)a); i++) {
                        if (length[i] == 0) {
                            p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1)), type);
                        }
                        if (length[i] != 0) {
                            for (int ii = 0; ii <= length[i]; ii++) {
                                p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz + ((int)Math.Round(yc[i]) - 1 + ii)), type);
                            }
                        }
                    }
                }
                // OVZ STARTS HERE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if (OVZ) {
                    for (int i = 0; i < ((int)a); i++) {
                        if (length[i] == 0) {
                            p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1)), type);
                        }
                        if (length[i] != 0) {
                            for (int ii = 0; ii <= length[i]; ii++) {
                                p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1 + ii)), type);
                            }
                        }
                    }
                    if (OVX) {
                        for (int i = 0; i < ((int)a); i++) {
                            if (length[i] == 0) {
                                p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1)), type);
                            }
                            if (length[i] != 0) {
                                for (int ii = 0; ii <= length[i]; ii++) {
                                    p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1 + ii)), type);
                                }
                            }
                        }
                    }
                    else {
                        for (int i = 0; i < ((int)a); i++) {
                            if (length[i] == 0) {
                                p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1)), type);
                            }
                            if (length[i] != 0) {
                                for (int ii = 0; ii <= length[i]; ii++) {
                                    p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) - 1 + ii)), type);
                                }
                            }
                        }
                    }
                }
                // NO OVZ !!!!!!!!!!!!!!!!!!!!!!!!
                else {
                    for (int i = 0; i < ((int)a); i++) {
                        if (length[i] == 0) {
                            p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz - ((int)Math.Round(yc[i]))), type);
                        }
                        if (length[i] != 0) {
                            for (int ii = 0; ii <= length[i]; ii++) {
                                p.level.Blockchange(p, (ushort)(startx + i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) + ii)), type);
                            }
                        }
                    }
                    if (OVX) {
                        for (int i = 0; i < ((int)a); i++) {
                            if (length[i] == 0) {
                                p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]))), type);
                            }
                            if (length[i] != 0) {
                                for (int ii = 0; ii <= length[i]; ii++) {
                                    p.level.Blockchange(p, (ushort)(startx - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) + ii)), type);
                                }
                            }
                        }
                    }
                    else {
                        for (int i = 0; i < ((int)a); i++) {
                            if (length[i] == 0) {
                                p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]))), type);
                            }
                            if (length[i] != 0) {
                                for (int ii = 0; ii <= length[i]; ii++) {
                                    p.level.Blockchange(p, (ushort)(startx - 1 - i), starth, (ushort)(startz - ((int)Math.Round(yc[i]) + ii)), type);
                                }
                            }
                        }
                    }
                }
                starth++;
            }

            TwoBlockSelection.RestartIfStatic(p, cd, SelectionFinished);
        }

        /// <summary>
        /// Called when /help is used on /ellipse.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/ellipse - Creates an ellipse using the currently held block.");
        }
    }
}

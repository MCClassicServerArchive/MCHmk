/*
    Copyright 2016 Jjp137

    This file have been changed from the original source code by MCForge.

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
using System.Collections;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;

namespace MCHmk.Commands {
    public class CmdMaze : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"labyrinth", "create"});

        public override string Name {
            get {
                return "maze";
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
                return DefaultRankValue.AdvBuilder;
            }
        }

        public CmdMaze(Server s) : base(s) { }

        public static int randomizer = 0;
        public static bool[,] wall;

        public override void Use(Player p, string args) {
            String[] split = args.Split(' ');
            if (split.Length >= 1 && args.Length > 0) {
                try {
                    randomizer = int.Parse(split[0]);
                }
                catch (Exception) {  // TODO: find exact exception to catch
                    this.Help(p);
                    return;
                }
            }

            const string prompt = "Place two blocks to determine the maze's edges.";
            TwoBlockSelection.Start(p, null, prompt, SelectionFinished);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the maze to be drawn.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            p.SendMessage("Generating maze... this could take a while");

            // Check to make sure that the player isn't building over his cuboid limit
            int numberOfBlocks = 0;
            for (int xx = Math.Min(x2, x1); xx <= Math.Max(x2, x1); ++xx) {
                for (int yy = 0; yy <= 2; ++yy) { // The maze is always 3 blocks tall -Jjp137
                    for (int zz = Math.Min(z2, z1); zz <= Math.Max(z2, z1); ++zz) {
                        numberOfBlocks++;
                    }
                }
            }

            if (numberOfBlocks > p.rank.maxBlocks) {
                p.SendMessage("You tried to make a maze with " + numberOfBlocks.ToString() + " blocks.");
                p.SendMessage("You cannot cuboid more than " + p.rank.maxBlocks.ToString() + ".");
                return;
            }

            int width = Math.Max(x2, x1) - Math.Min(x2, x1);
            if (width % 2 != 0) {
                width++;
                x2--;
            }
            width -= 2;
            int height = Math.Max(z2, z1) - Math.Min(z2, z1);
            if (height % 2 != 0) {
                height++;
                z2--;
            }
            height -= 2;
            //substract 2 cause we will just make the inner. the outer wall is made seperately
            wall = new bool[width+1, height+1];//+1 cause we begin at 0 so we need one object more
            for (int w = 0; w <= width; w++) {
                for (int h = 0; h <= height; h++) {
                    wall[w, h] = true;
                }
            }
            GridNode.maxX = width;
            GridNode.maxY = height;
            //Make a Stack
            Stack s = new Stack(width * height);
            //Random rand = new Random(DateTime.Now.Millisecond);//ha yeah randomized :P
            //lets begin in the lower left corner eh?(0,0)
            s.Push(new GridNode(0, 0));
            wall[0, 0] = false;
            while (true) {
                GridNode node = (GridNode)s.Peek();
                if (node.turnsPossible()) {
                    GridNode[] nodearray = node.getRandomNext();
                    wall[nodearray[0].X, nodearray[0].Y] = false;
                    wall[nodearray[1].X, nodearray[1].Y] = false;
                    s.Push(nodearray[1]);
                    //we get the next two nodes
                    //the first is a middle node from which there shouldnt start a new corridor
                    //the second is added to the stack. next try will be with this node
                    //i hope this will work this time...
                }
                else {
                    s.Pop();//if this node is a dead and it will be removed
                }

                if (s.Count < 1) {
                    break;//if no nodes are free anymore we will end the generation here
                }
            }
            p.SendMessage("Maze is generated. now painting...");
            //seems to be there are no more moves possible
            //paint that shit :P
            ushort minx = Math.Min(x2, x1);
            ushort minz = Math.Min(z2, z1);
            ushort maxx = Math.Max(x2, x1);
            maxx++;
            ushort maxz = Math.Max(z2, z1);
            maxz++;
            for (ushort xx = 0; xx <= width; xx++) {
                for (ushort zz = 0; zz <= height; zz++) {
                    if (wall[xx, zz]) {
                        p.level.Blockchange(p, (ushort)(xx + minx+1), y2, (ushort)(zz + minz+1), BlockId.DoubleSlab);
                        p.level.Blockchange(p, (ushort)(xx + minx+1), (ushort)(y2 + 1), (ushort)(zz + minz+1), BlockId.Leaves);
                        p.level.Blockchange(p, (ushort)(xx + minx+1), (ushort)(y2 + 2), (ushort)(zz + minz+1), BlockId.Leaves);
                    }
                }
            }

            _s.commands.FindCommand("cuboid").Use(p, "walls");
            p.manualChange(minx, y2, minz, 0, BlockId.DoubleSlab);
            p.manualChange(maxx, y2, maxz, 0, BlockId.DoubleSlab);
            _s.commands.FindCommand("cuboid").Use(p, "walls");
            p.manualChange(minx, (ushort)(y2 + 1), minz, 0, BlockId.Leaves);
            p.manualChange(maxx, (ushort)(y2 + 2), maxz, 0, BlockId.Leaves);
            p.SendMessage("Maze painted. Build your entrance and exit yourself");
            randomizer = 0;
        }

        private class GridNode {
            public static int maxX = 0;
            public static int maxY = 0;
            public ushort X;
            public ushort Y;
            private Random rand2 = new Random(Environment.TickCount);
            public GridNode[] getRandomNext() {
                byte[] r = new byte[1];
                switch (randomizer) {
                    case 0:
                        RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
                        rand.GetBytes(r);
                        r[0] /= (255 / 4);
                        break;
                    case 1:
                        r[0] = (byte)rand2.Next(4);
                        break;
                    default:
                        r[0] = (byte)rand2.Next(4);
                        break;
                }
                ushort rx = 0, ry = 0, rx2 = 0, ry2 = 0;
                switch (r[0]) {
                    case 0:
                        if (isWall(X, Y + 2)) {
                            //go up
                            rx = X;
                            rx2 = X;
                            ry = (ushort)(Y + 1);
                            ry2 = (ushort)(Y + 2);
                        }
                        else {
                            return this.getRandomNext();
                        }
                        break;
                    case 1:
                        if (isWall(X, Y - 2)) {
                            //go down
                            rx = X;
                            rx2 = X;
                            ry = (ushort)(Y - 1);
                            ry2 = (ushort)(Y - 2);
                        }
                        else {
                            return this.getRandomNext();
                        }
                        break;
                    case 2:
                        if (isWall(X + 2, Y)) {
                            //go right
                            rx = (ushort)(X + 1);
                            rx2 = (ushort)(X + 2);
                            ry = Y;
                            ry2 = Y;
                        }
                        else {
                            return this.getRandomNext();
                        }
                        break;
                    case 3:
                        if (isWall(X - 2, Y)) {
                            //go left
                            rx = (ushort)(X - 1);
                            rx2 = (ushort)(X - 2);
                            ry = Y;
                            ry2 = Y;
                        }
                        else {
                            return this.getRandomNext();
                        }
                        break;
                }
                return new GridNode[] { new GridNode(rx, ry), new GridNode(rx2, ry2) };
            }
            public bool turnsPossible() {
                return (isWall(X, Y + 2) || isWall(X, Y - 2) || isWall(X + 2, Y) || isWall(X - 2, Y));

            }

            private bool isWall(int x, int y) {
                try {
                    return wall[x, y];
                }
                catch (IndexOutOfRangeException) {
                    return false;
                }
            }
            public GridNode(ushort x, ushort y) {
                X = x;
                Y = y;
            }
        }

        /// <summary>
        /// Called when /help is used on /maze.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/maze - Generates a maze.");
        }
    }
}

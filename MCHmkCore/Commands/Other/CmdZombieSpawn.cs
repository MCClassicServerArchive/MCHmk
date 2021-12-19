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
	Copyright 2011 MCForge

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

using System.Threading;

namespace MCHmk.Commands {
    public class CmdZombieSpawn : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"game", "brains"});

        public override string Name {
            get {
                return "zombiespawn";
            }
        }
        public override string Shortcut {
            get {
                return "zs";
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
                return DefaultRankValue.Operator;
            }
        }

        public CmdZombieSpawn(Server s) : base(s) { }

        private int wavesNum;
        private int wavesLength;
        private int zombiesNum;
        private int thex;
        private int they;
        private int thez;
        private bool isRandom;

        public override void Use(Player theP, string args) {
            int number = args.Split(' ').Length;
            String[] param = args.Split(' ');

            if (number == 1) {
                if (String.Compare(param[0], "x", true) == 0) {
                    _s.commands.FindCommand("replaceall").Use(theP, "zombie air");
                    theP.SendMessage("&aAll zombies have been destroyed.");
                    return;
                }
            }

            if (number != 4) {
                Help(theP);
                return;
            }

            try {
                if (String.Compare(param[0], "r", true) == 0) {
                    isRandom = true;
                }
                else if (String.Compare(param[0], "d", true) == 0) {
                    isRandom = false;
                }
                else {
                    theP.SendMessage("Flag set must be 'r' or 'd'.");
                    return;
                }

                wavesNum = Int32.Parse(param[1]);
                wavesLength = Int32.Parse(param[2]);
                zombiesNum = Int32.Parse(param[3]);

                if (!isRandom) {
                    theP.ClearSelection();

                    theP.StartSelection(CenterSelected, null);
                    theP.SendMessage("Place a block for center of zombie spawn.");
                }
                else {
                    Thread t = new Thread(ZombieMob);
                    t.Start(theP);
                }
            }
            catch (FormatException) {
                theP.SendMessage("&4All parameters must be numbers!");
            }
        }

        private void CenterSelected(Player p, CommandTempData c) {
            // Don't wait for the block to be placed anymore.
            p.ClearSelection();

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            p.SendBlockchange(c.X, c.Y, c.Z, p.level.GetTile(c.X, c.Y, c.Z));

            thex = c.X;
            they = c.Y + 2;
            thez = c.Z;

            Thread t = new Thread(ZombieMob);
            t.Start(p);
        }

        private void ZombieMob(Object person) {
            int xBegin = 0;
            int zBegin = 0;
            Player p = (Player)person;

            if (zombiesNum % 2 == 0 && isRandom == false) {
                xBegin = thex - (zombiesNum / 2);
                zBegin = thez - (zombiesNum / 2);
            }

            if (zombiesNum % 2 == 1 && isRandom == false) {
                xBegin = thex - ((zombiesNum - 1) / 2);
                zBegin = thez - ((zombiesNum - 1) / 2);
            }

            _s.commands.FindCommand("say").Use(p, "&aInitiating zombie attack!");
            _s.commands.FindCommand("say").Use(p, "&a" + wavesNum + " wave(s)");
            _s.commands.FindCommand("say").Use(p, "&a" + wavesLength + " second(s) each wave");
            for (int num = 1; num <= wavesNum; num++) {
                if (isRandom) {
                    randomZombies(p);
                }
                else {
                    placedZombies(p, xBegin, zBegin);
                }

                _s.commands.FindCommand("say").Use(p, "&aZombie wave # " + num);
                Thread.Sleep(wavesLength*1000);
            }
            _s.commands.FindCommand("say").Use(p, "&aZombie attack is over.");
        }

        private void randomZombies(Player p) {
            Random randomCoord = new Random();
            int ranx = 0;
            int rany = 0;
            int ranz = 0;

            for (int k = 0; k < zombiesNum; k++) {
                ranx = randomCoord.Next(0, p.level.width);
                rany = randomCoord.Next((p.level.height / 2), p.level.height);
                ranz = randomCoord.Next(0, p.level.depth);

                _s.commands.FindCommand("place").Use(p, "zombie " + ranx + " " + rany + " " + ranz);
            }
        }

        private void placedZombies(Player p, int xBegin, int zBegin) {
            for (int j = xBegin; j < xBegin + zombiesNum; j++) {
                for (int k = zBegin; k < zBegin + zombiesNum; k++) {
                    _s.commands.FindCommand("place").Use(p, "zombie " + j + " " + they + " " + k);
                }
            }
        }

        // This one controls what happens when you use /help [commandname].
        public override void Help(Player p) {
            p.SendMessage("/zombiespawn <flag?> <x?> <y?> <z?> - Spawns waves of zombies.");
            p.SendMessage("<flag> - 'r' for random or 'd' for diameter");
            p.SendMessage("<x> - the number of waves");
            p.SendMessage("<y> - the length of the waves in seconds");
            p.SendMessage("<z> - the number of zombies spawned/diameter of spawn");
            p.SendMessage("/zombiespawn x - Destroys all zombies.");
        }
    }
}

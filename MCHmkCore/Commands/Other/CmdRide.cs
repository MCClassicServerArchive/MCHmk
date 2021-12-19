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
	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)

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
    public class CmdRide : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"drive", "train"});

        public override string Name {
            get {
                return "ride";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdRide(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            p.onTrain = !p.onTrain;

            if (!p.onTrain) {
                return;
            }

            Thread trainThread = new Thread(new ThreadStart(delegate {
                while (p.onTrain) {
                    Thread.Sleep(3);

                    ushort x = (ushort)(p.pos[0] / 32);
                    ushort y = (ushort)(p.pos[1] / 32);
                    ushort z = (ushort)(p.pos[2] / 32);

                    for (ushort xx = (ushort)(x - 1); xx <= x + 1; xx++) {
                        for (ushort yy = (ushort)(y - 1); yy <= y + 1; yy++) {
                            for (ushort zz = (ushort)(z - 1); zz <= z + 1; zz++) {
                                if (p.level.GetTile(xx, yy, zz) == BlockId.Train) {
                                    p.invincible = true;
                                    p.trainGrab = true;
                                    byte newY = 0;

                                    if (y - yy == -1) {
                                        newY = 240;
                                    }
                                    else if (y - yy == 0) {
                                        newY = 0;
                                    }
                                    else {
                                        newY = 8;
                                    }

                                    unchecked {
                                        if (x - xx == -1)
                                            if (z - zz == -1) {
                                                p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)96, newY);
                                            }
                                            else if (z - zz == 0) {
                                                p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)64, newY);
                                            }
                                            else {
                                                p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)32, newY);
                                            }
                                        else if (x - xx == 0)
                                            if (z - zz == -1) {
                                                p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)128, newY);
                                            }
                                            else if (z - zz == 0) { }
                                            else {
                                                p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)0, newY);
                                            }
                                        else if (z - zz == -1) {
                                            p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)160, newY);
                                        }
                                        else if (z - zz == 0) {
                                            p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)192, newY);
                                        }
                                        else {
                                            p.SendPos((byte)-1, (ushort)(xx * 32 + 16), (ushort)((yy + 1) * 32 - 2), (ushort)(zz * 32 + 16), (byte)224, newY);
                                        }
                                    }
                                    goto skip;
                                }
                            }
                        }
                    }

                    Thread.Sleep(3);
                    p.invincible = false;
                    p.trainGrab = false;
skip:   ;
                }

                p.SendMessage("Dismounted");
                Thread.Sleep(1000);
                p.invincible = false;
                p.trainGrab = false;
            }));
            trainThread.Start();
            p.SendMessage("Stand near a train to mount it");
        }

        /// <summary>
        /// Called when /help is used on /ride.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/ride - Rides a nearby train.");
        }
    }
}

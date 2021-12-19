/*
    Copyright 2012 Jjp137

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace MCHmk.Commands {
    public class CmdFly : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"air", "ctf", "glass", "carpet"});

        public override string Name {
            get {
                return "fly";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }
        public CmdFly(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            p.isFlying = !p.isFlying;
            if (!p.isFlying) {
                return;
            }

            p.SendMessage("You are now flying. &cJump!");

            Thread flyThread = new Thread(new ThreadStart(delegate {
                UShortCoords pos;
                ushort[] oldpos = new ushort[3];
                List<UShortCoords> buffer = new List<UShortCoords>();
                while (p.isFlying) {
                    Thread.Sleep(20);
                    if (p.pos == oldpos) {
                        continue;
                    }
                    try {
                        List<UShortCoords> tempBuffer = new List<UShortCoords>();
                        List<UShortCoords> toRemove = new List<UShortCoords>();
                        ushort x = (ushort)((p.pos[0]) / 32);
                        ushort y = (ushort)((p.pos[1] - 60) / 32);
                        ushort z = (ushort)((p.pos[2]) / 32);

                        try {
                            for (ushort xx = ZeroOrAbove(x); xx <= x + 1; xx++) {
                                for (ushort yy = ZeroOrAbove(y); yy <= y; yy++) {
                                    for (ushort zz = ZeroOrAbove(z); zz <= z + 1; zz++) {
                                        if (p.level.GetTile(xx, yy, zz) == BlockId.Air) {
                                            pos.X = xx;
                                            pos.Y = yy;
                                            pos.Z = zz;
                                            tempBuffer.Add(pos);
                                        }
                                    }
                                }
                            }
                            foreach (UShortCoords cP in tempBuffer) {
                                if (!buffer.Contains(cP)) {
                                    buffer.Add(cP);
                                    p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Glass);
                                }
                            }
                            foreach (UShortCoords cP in buffer) {
                                if (!tempBuffer.Contains(cP)) {
                                    p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Air);
                                    toRemove.Add(cP);
                                }
                            }
                            foreach (UShortCoords cP in toRemove) {
                                buffer.Remove(cP);
                            }
                            tempBuffer.Clear();
                            toRemove.Clear();
                        }
                        catch (Exception e) {
                            _s.logger.ErrorLog(e);
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                    p.pos.CopyTo(oldpos, 0);
                }

                foreach (UShortCoords cP in buffer) {
                    p.SendBlockchange(cP.X, cP.Y, cP.Z, BlockId.Air);
                }

                p.SendMessage("Stopped flying");
            }));
            flyThread.Start();
        }

        /// <summary>
        /// Returns a number for the /fly for-loop that is zero or above.
        /// </summary>
        /// <param name="x"> A coordinate of the player. </param>
        /// <returns> An unsigned short guaranteed to be zero or above. </returns>
        private ushort ZeroOrAbove(ushort x) {
            if (x == 0) {
                return 0;
            }
            else {
                return (ushort)(x - 1);
            }
        }
        
        /// <summary>
        /// Called when /help is used on /fly.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/fly - Toggles fly mode.");
            p.SendMessage("To fly, simply jump, and glass will appear below you. " +
                               "This glass keeps you from falling. To descend, turn off fly mode or break the " +
                               "glass below you.");
            p.SendMessage("Note that /fly does not work well if you are lagging.");
        }

    }
}

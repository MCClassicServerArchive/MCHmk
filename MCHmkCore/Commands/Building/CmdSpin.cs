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
    public class CmdSpin : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rotate"});

        public override string Name {
            get {
                return "spin";
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
        public CmdSpin(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length > 1) {
                Help(p);
                return;
            }
            if (args == String.Empty) {
                args = "y";
            }

            List<Player.CopyPos> newBuffer = new List<Player.CopyPos>();
            int TotalLoop = 0;
            ushort temp;
            newBuffer.Clear();

            switch (args) {
            case "y":
                for (int i = 0; i < p.CopyBuffer.Count; i++) {
                    Player.CopyPos pos = p.CopyBuffer[i];
                    temp = pos.z;
                    pos.z = pos.x;
                    pos.x = temp;
                    p.CopyBuffer[i] = pos;
                }
            goto case "m";
            case "180":
                TotalLoop = p.CopyBuffer.Count;
                p.CopyBuffer.ForEach(delegate(Player.CopyPos Pos) {
                    TotalLoop -= 1;
                    Pos.x = p.CopyBuffer[TotalLoop].x;
                    Pos.z = p.CopyBuffer[TotalLoop].z;
                    newBuffer.Add(Pos);
                });
                p.CopyBuffer.Clear();
                p.CopyBuffer = newBuffer;
                break;
            case "upsidedown":
            case "u":
                TotalLoop = p.CopyBuffer.Count;
                p.CopyBuffer.ForEach(delegate(Player.CopyPos Pos) {
                    TotalLoop -= 1;
                    Pos.y = p.CopyBuffer[TotalLoop].y;
                    newBuffer.Add(Pos);
                });
                p.CopyBuffer.Clear();
                p.CopyBuffer = newBuffer;
                break;
            case "mirror":
            case "m":
                TotalLoop = p.CopyBuffer.Count;
                p.CopyBuffer.ForEach(delegate(Player.CopyPos Pos) {
                    TotalLoop -= 1;
                    Pos.x = p.CopyBuffer[TotalLoop].x;
                    newBuffer.Add(Pos);
                });
                p.CopyBuffer.Clear();
                p.CopyBuffer = newBuffer;
                break;
            case "z":
                TotalLoop = p.CopyBuffer.Count;
                p.CopyBuffer.ForEach(delegate(Player.CopyPos Pos) {
                    TotalLoop -= 1;
                    Pos.x = (ushort)(p.CopyBuffer[TotalLoop].y - (2 * p.CopyBuffer[TotalLoop].y));
                    Pos.y = p.CopyBuffer[TotalLoop].x;
                    newBuffer.Add(Pos);
                });
                p.CopyBuffer.Clear();
                p.CopyBuffer = newBuffer;
                break;
            case "x":
                TotalLoop = p.CopyBuffer.Count;
                p.CopyBuffer.ForEach(delegate(Player.CopyPos Pos) {
                    TotalLoop -= 1;
                    Pos.z = (ushort)(p.CopyBuffer[TotalLoop].y - (2 * p.CopyBuffer[TotalLoop].y));
                    Pos.y = p.CopyBuffer[TotalLoop].z;
                    newBuffer.Add(Pos);
                });
                p.CopyBuffer.Clear();
                p.CopyBuffer = newBuffer;
                break;

            default:
                p.SendMessage("Incorrect syntax");
                Help(p);
                return;
            }

            p.SendMessage("Spun: &b" + args);
        }

        /// <summary>
        /// Called when /help is used on /spin.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/spin <parameter?> - Spins a copied object.");
            p.SendMessage("Valid Parameters:");
            p.SendMessage("x - Spins the object 90 degrees around the x axis.");
            p.SendMessage("y - Spins the object 90 degrees around the y axis.");
            p.SendMessage("z - Spins the object 90 degrees around the z axis.");
            p.SendMessage("mirror - Mirrors the object along the y axis.");
            p.SendMessage("180 - Spins the object 180 degrees around the y axis.");
            p.SendMessage("upsidedown - Mirrors the object along the z axis.");
        }
    }
}

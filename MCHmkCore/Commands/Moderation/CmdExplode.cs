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
    Written by Jack1312

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
    public class CmdExplode : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"explosion", "boom"});

        public override string Name {
            get {
                return "explode";
            }
        }
        public override string Shortcut {
            get {
                return "ex";
            }
        }
        public override string Type {
            get {
                return "mod";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdExplode(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            int number = args.Split(' ').Length;
            if (number > 3) {
                p.SendMessage("What are you on about?");
                return;
            }
            if (args == "me") {
                if (p.level.physics <3) {
                    p.SendMessage("The physics on this level are not sufficient for exploding!");
                    return;
                }
                _s.commands.FindCommand("explode").Use(p, p.name);
                return;
            }
            if (number == 1) {
                if (!p.IsConsole) {
                    if (p.level.physics < 3) {
                        p.SendMessage("The physics on this level are not sufficient for exploding!");
                        return;
                    }
                    Player who = _s.players.Find(args);
                    ushort x = (ushort)(who.pos[0] / 32);
                    ushort y = (ushort)(who.pos[1] / 32);
                    ushort z = (ushort)(who.pos[2] / 32);
                    who.level.MakeExplosion(x, y, z, 1);
                    p.SendMessage(who.color + who.name + _s.props.DefaultColor + " has been exploded!");
                    return;
                }
                p.SendMessage("The specified player does not exist!");
                return;
            }
            if (number == 3) {
                {
                    //  BlockId b = BlockId.Zero;
                    ushort x = 0;
                    ushort y = 0;
                    ushort z = 0;

                    x = (ushort)(p.pos[0] / 32);
                    y = (ushort)((p.pos[1] / 32) - 1);
                    z = (ushort)(p.pos[2] / 32);

                    try {
                        switch (args.Split(' ').Length) {
                        //     case 0: b = BlockId.rock; break;
                        //  case 1: b = Block.Ushort(message); break;
                        case 3:
                            x = Convert.ToUInt16(args.Split(' ')[0]);
                            y = Convert.ToUInt16(args.Split(' ')[1]);
                            z = Convert.ToUInt16(args.Split(' ')[2]);
                            break;
                        case 4:
                            //    b = Block.Ushort(message.Split(' ')[0]);
                            x = Convert.ToUInt16(args.Split(' ')[1]);
                            y = Convert.ToUInt16(args.Split(' ')[2]);
                            z = Convert.ToUInt16(args.Split(' ')[3]);
                            break;
                        default:
                            p.SendMessage("Invalid parameters");
                            return;
                        }
                    }
                    catch {  // TODO: find exact exception to catch
                        p.SendMessage("Invalid parameters");
                        return;
                    }

                    //  Level level = p.level;

                    if (y >= p.level.height) {
                        y = (ushort)(p.level.height - 1);
                    }

                    if (p.level.physics < 3) {
                        p.SendMessage("The physics on this level are not sufficient for exploding!");
                        return;
                    }
                    p.level.MakeExplosion(x, y, z, 1);
                    p.SendMessage("An explosion was made at (" + x + ", " + y + ", " + z + ").");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /explode.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/explode <location?> - Creates an explosion.");
            p.SendMessage("Possible locations: me, player, x y z");
            p.SendMessage("me - Explodes at your location.");
            p.SendMessage("player - Explodes the specified player.");
            p.SendMessage("x y z - Explodes at the specified coordinates.");
        }
    }
}

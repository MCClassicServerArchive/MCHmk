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
    public class CmdAscend : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"up", "level", "block", "move", "player"});

        public override string Name {
            get {
                return "ascend";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdAscend(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            ushort max = p.level.height;
            ushort posy = (ushort)(p.pos[1] / 32);
            bool found = false;
            ushort xpos = (ushort)(p.pos[0] / 32);
            ushort zpos = (ushort)(p.pos[2] / 32);
            while (!found && posy < max) {
                posy++;
                BlockId block = p.level.GetTile(xpos, posy, zpos);
                if (block == BlockId.Air|| block == BlockId.DoorAir || block == BlockId.AirSwitch || block == BlockId.Null) {
                    ushort blockabove = (ushort) (posy + 1);
                    ushort blockunder = (ushort) (posy - 1);
                    if (p.level.GetTile(xpos, blockabove, zpos) == BlockId.Air|| p.level.GetTile(xpos, blockabove, zpos) == BlockId.DoorAir
                            || p.level.GetTile(xpos, blockabove, zpos) == BlockId.AirSwitch
                            || p.level.GetTile(xpos, blockabove, zpos) == BlockId.Null) {
                        if (p.level.GetTile(xpos, blockunder, zpos) != BlockId.Air && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirSwitch
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.DoorAir
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirFlood
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirFloodDown
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirFloodLayer
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirFloodUp
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.AirPortal
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.RedFlower
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.YellowFlower
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.FiniteWater
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.FiniteLava && p.level.GetTile(xpos, blockunder, zpos) != BlockId.Embers
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.ActiveWater
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.DoorWater
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.WaterPortal
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.Waterfall
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.WaterFaucet
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.ActiveLava && p.level.GetTile(xpos, blockunder, zpos) != BlockId.DoorLava
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.FastLava
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.LavaPortal
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.Lavafall
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.StillLava
                                && p.level.GetTile(xpos, blockunder, zpos) != BlockId.Null) {
                            p.SendMessage("Teleported you up!");
                            unchecked {
                                p.SendPos((byte)-1, p.pos[0], (ushort)((posy + 1) * 32), p.pos[2], p.rot[0], p.rot[1]);
                            }
                            found = true;
                        }
                    }
                }
            }
            if (!found) {
                p.SendMessage("No free spaces found above you");
            }
        }

        /// <summary>
        /// Called when /help is used on /ascend.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/ascend - Teleports you to the first free space above you.");
        }
    }
}

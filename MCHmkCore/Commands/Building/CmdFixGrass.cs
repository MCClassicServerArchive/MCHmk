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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)


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
    public class CmdFixGrass : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"grass", "fix"});

        public override string Name {
            get {
                return "fixgrass";
            }
        }
        public override string Shortcut {
            get {
                return "fg";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdFixGrass(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            int totalFixed = 0;

            switch (args.ToLower()) {
            case "":
                for (int i = 0; i < p.level.blocks.Length; i++) {
                    try {
                        ushort x, y, z;
                        p.level.IntToPos(i, out x, out y, out z);

                        if (p.level.blocks[i] == BlockId.Dirt) {
                            if (BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)])) {
                                p.level.Blockchange(p, x, y, z, BlockId.Grass);
                                totalFixed++;
                            }
                        }
                        else if (p.level.blocks[i] == BlockId.Grass) {
                            if (!BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)])) {
                                p.level.Blockchange(p, x, y, z, BlockId.Dirt);
                                totalFixed++;
                            }
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                } break;
            case "light":
                for (int i = 0; i < p.level.blocks.Length; i++) {
                    try {
                        ushort x, y, z;
                        bool skipMe = false;
                        p.level.IntToPos(i, out x, out y, out z);

                        if (p.level.blocks[i] == BlockId.Dirt) {
                            for (int iL = 1; iL < (p.level.height - y); iL++) {
                                if (!BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, iL, 0)])) {
                                    skipMe = true;
                                    break;
                                }
                            }
                            if (!skipMe) {
                                p.level.Blockchange(p, x, y, z, BlockId.Grass);
                                totalFixed++;
                            }
                        }
                        else if (p.level.blocks[i] == BlockId.Grass) {
                            for (int iL = 1; iL < (p.level.height - y); iL++) {
                                // Used to change grass to dirt only if all the upper blocks weren't Lightpass.
                                if (!BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, iL, 0)])) {
                                    skipMe = true;
                                    break;
                                }
                            }
                            if (skipMe) {
                                p.level.Blockchange(p, x, y, z, BlockId.Dirt);
                                totalFixed++;
                            }
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                } break;
            case "grass":
                for (int i = 0; i < p.level.blocks.Length; i++) {
                    try {
                        ushort x, y, z;
                        p.level.IntToPos(i, out x, out y, out z);

                        if (p.level.blocks[i] == BlockId.Grass)
                            if (!BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)])) {
                                p.level.Blockchange(p, x, y, z, BlockId.Dirt);
                                totalFixed++;
                            }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                } break;
            case "dirt":
                for (int i = 0; i < p.level.blocks.Length; i++) {
                    try {
                        ushort x, y, z;
                        p.level.IntToPos(i, out x, out y, out z);

                        if (p.level.blocks[i] == BlockId.Dirt)
                            if (BlockData.LightPass(p.level.blocks[p.level.IntOffset(i, 0, 1, 0)])) {
                                p.level.Blockchange(p, x, y, z, BlockId.Grass);
                                totalFixed++;
                            }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                } break;
            default:
                Help(p);
                return;
            }

            p.SendMessage("Fixed " + totalFixed + " blocks.");
        }

        /// <summary>
        /// Called when /help is used on /fixgrass.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/fixgrass [type?] - Fixes the grass.");
            p.SendMessage("By default, any grass with something on top is made into " +
                               "dirt, and dirt with nothing on top is made grass.");
            p.SendMessage("Other types:");
            p.SendMessage("light - Dirt/grass in sunlight becomes grass.");
            p.SendMessage("grass - Only turns grass to dirt when under stuff.");
            p.SendMessage("dirt - Only turns dirt with nothing on top to grass.");
        }
    }
}

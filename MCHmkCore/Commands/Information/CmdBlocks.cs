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

namespace MCHmk.Commands {
    public class CmdBlocks : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "info", "list"});

        public override string Name {
            get {
                return "blocks";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "information";
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
        public CmdBlocks(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                if (args == String.Empty) {
                    p.SendMessage("Basic blocks: ");
                    for (byte i = 0; i < 50; i++) {
                        BlockId b = (BlockId)i;
                        args += ", " + BlockData.Name(b);
                    }
                    p.SendMessage(args.Remove(0, 2));
                    p.SendMessage("&d/blocks all <0/1/2/3/4> " + _s.props.DefaultColor + "will show the rest.");
                }
                else if (args.ToLower() == "all") {
                    p.SendMessage("Complex blocks: ");
                    for (ushort i = 50; i < BlockData.maxblocks; i++) {
                        BlockId b = (BlockId)i;
                        if (BlockData.Name(b).ToLower() != "unknown") {
                            args += ", " + BlockData.Name(b);
                        }
                    }
                    p.SendMessage(args.Remove(0, 2));
                    p.SendMessage("Use &d/blocks all <0/1/2/3/4> " + _s.props.DefaultColor + "for a readable list.");
                }
                else if (args.ToLower().IndexOf(' ') != -1 && args.Split(' ')[0] == "all") {
                    int foundRange = 0;
                    try {
                        foundRange = int.Parse(args.Split(' ')[1]);
                    }
                    catch {  // TODO: find exact exception to catch
                        p.SendMessage("Incorrect syntax");
                        return;
                    }

                    if (foundRange >= 5 || foundRange < 0) {
                        p.SendMessage("Number must be between 0 and 4");
                        return;
                    }

                    args = String.Empty;
                    p.SendMessage("Blocks between " + foundRange * 51 + " and " + (foundRange + 1) * 51);
                    for (byte i = (byte)(foundRange * 51); i < (byte)((foundRange + 1) * 51); i++) {
                        BlockId b = (BlockId)i;
                        if (BlockData.Name(b).ToLower() != "unknown") {
                            args += ", " + BlockData.Name(b);
                        }
                    }
                    p.SendMessage(args.Remove(0, 2));
                }
                else {
                    string printMessage = ">>>&b";

                    if (BlockData.Ushort(args) != BlockId.Null) {
                        BlockId b = BlockData.Ushort(args);
                        if (Convert.ToInt32(b) < 51) {
                            for (ushort i = 51; i < BlockData.maxblocks; i++) {
                                BlockId b2 = (BlockId)i;
                                if (BlockData.Convert(b2) == b) {
                                    printMessage += BlockData.Name(b2) + ", ";
                                }
                            }

                            if (printMessage != ">>>&b") {
                                p.SendMessage("Blocks which look like \"" + args + "\":");
                                p.SendMessage(printMessage.Remove(printMessage.Length - 2));
                            }
                            else {
                                p.SendMessage("No Complex Blocks look like \"" + args + "\"");
                            }
                        }
                        else {
                            p.SendMessage("&bComplex information for \"" + args + "\":");
                            p.SendMessage("&cBlock will appear as a \"" + BlockData.Name(BlockData.Convert(b)) + "\" block");

                            if (BlockData.LightPass(b)) {
                                p.SendMessage("Block will allow light through");
                            }
                            if (BlockData.Physics(b)) {
                                p.SendMessage("Block affects physics in some way");    //AFFECT!
                            }
                            else {
                                p.SendMessage("Block will not affect physics in any way");    //It's AFFECT!
                            }
                            if (BlockData.NeedRestart(b)) {
                                p.SendMessage("The block's physics will auto-start");
                            }

                            if (BlockData.OPBlocks(b)) {
                                p.SendMessage("Block is unaffected by explosions");
                            }

                            if (BlockData.AllowBreak(b)) {
                                p.SendMessage("Anybody can activate the block");
                            }
                            if (BlockData.Walkthrough(b)) {
                                p.SendMessage("Block can be walked through");
                            }
                            if (BlockData.Death(b)) {
                                p.SendMessage("Walking through block will kill you");
                            }

                            if (BlockData.DoorAirs(b) != (byte)0) {
                                p.SendMessage("Block is an ordinary door");
                            }
                            if (BlockData.tDoor(b)) {
                                p.SendMessage("Block is a tdoor, which allows other blocks through when open");
                            }
                            if (BlockData.odoor(b) != BlockId.Null) {
                                p.SendMessage("Block is an odoor, which toggles (GLITCHY)");
                            }

                            if (BlockData.Mover(b)) {
                                p.SendMessage("Block can be activated by walking through it");
                            }
                        }
                    }
                    else if (_s.ranks.Find(args) != null) {
                        int Perm = _s.ranks.Find(args).Permission;
                        foreach (BlockPerm bL in _s.blockPerms) {
                            if (_s.blockPerms.CanPlace(Perm, bL.type) && BlockData.Name(bL.type).ToLower() != "unknown") {
                                printMessage += BlockData.Name(bL.type) + ", ";
                            }
                        }

                        if (printMessage != ">>>&b") {
                            p.SendMessage("Blocks which " + _s.ranks.Find(args).color + _s.ranks.Find(args).name + _s.props.DefaultColor +
                                               " can place: ");
                            p.SendMessage(printMessage.Remove(printMessage.Length - 2));
                        }
                        else {
                            p.SendMessage("No blocks are specific to this rank");
                        }
                    }
                    else if (args.IndexOf(' ') == -1) {
                        if (args.ToLower() == "count") {
                            p.SendMessage("Blocks in this map: " + p.level.blocks.Length);
                        }
                        else {
                            Help(p);
                        }
                    }
                    else if (args.Split(' ')[0].ToLower() == "count") {
                        int foundNum = 0;
                        BlockId foundBlock = BlockData.Ushort(args.Split(' ')[1]);
                        if (foundBlock == BlockId.Null) {
                            p.SendMessage("Could not find block specified");
                            return;
                        }

                        for (int i = 0; i < p.level.blocks.Length; i++) {
                            if (foundBlock == p.level.blocks[i]) {
                                foundNum++;
                            }
                        }

                        if (foundNum == 0) {
                            p.SendMessage("No blocks were of type \"" + args.Split(' ')[1] + "\"");
                        }
                        else if (foundNum == 1) {
                            p.SendMessage("1 block was of type \"" + args.Split(' ')[1] + "\"");
                        }
                        else {
                            p.SendMessage(foundNum.ToString() + " blocks were of type \"" + args.Split(' ')[1] + "\"");
                        }
                    }
                    else {
                        p.SendMessage("Unable to find block or rank");
                    }
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                Help(p);
            }
        }

        /// <summary>
        /// Called when /help is used on /blocks.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Too long, needs to be rewritten later -Jjp137
            p.SendMessage("/blocks [all/basic block?/complex block?/rank?]- " +
                               "Lists all blocks, or views information about a block based on provided options.");
            p.SendMessage("List of options: ");
            p.SendMessage("all - Lists every block.");
            p.SendMessage("basic block - Lists all blocks which look like the given block.");
            p.SendMessage("complex block - Lists specific information on the given block.");
            p.SendMessage("rank - Lists all blocks that the given rank can use.");
            p.SendMessage("Available ranks: " + _s.ranks.ConcatNames(_s.props.DefaultColor));
            p.SendMessage("/blocks count [block?] - Finds the total count for a given " +
                               "block in the current map, or displays the number of blocks in the whole map.");
        }
    }
}

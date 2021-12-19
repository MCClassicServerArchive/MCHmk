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
Copyright (C) 2010-2013 David Mitchell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public sealed class CmdCopy : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the type of copy operation being performed.
        /// </summary>
        private readonly string _typeKey = "copy_type";
        /// <summary>
        /// Name of the key used to store and retrieve the blocks that will be ignored when copying.
        /// </summary>
        private readonly string _ignoreKey = "copy_ignore";
        /// <summary>
        /// Name of the key used to store and retrieve whether a copy offset will be set.
        /// </summary>
        private readonly string _offsetKey = "copy_offset";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"clipboard", "blocks", "save"});

        public override string Name {
            get {
                return "copy";
            }
        }
        public override string Shortcut {
            get {
                return "c";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.AdvBuilder;
            }
        }

        public CmdCopy(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ')[0].ToLower() == "save") {
                if (args.Split(' ').Length != 2 || String.IsNullOrEmpty(args.Split(' ')[1])) {
                    Help(p);
                    return;
                }
                SaveCopy(p, args.Split(' ')[1]);
                return;
            }
            if (args.Split(' ')[0].ToLower() == "load") {
                if (args.Split(' ').Length != 2 || String.IsNullOrEmpty(args.Split(' ')[1])) {
                    Help(p);
                    return;
                }
                Loadcopy(p, args.Split(' ')[1]);
                return;
            }
            if (args.Split(' ')[0].ToLower() == "delete") {
                if (args.Split(' ').Length != 2 || String.IsNullOrEmpty(args.Split(' ')[1])) {
                    Help(p);
                    return;
                }
                args = args.Split(' ')[1];
                if (!File.Exists("extra/savecopy/" + p.uuid + "/" + args + ".cpy")) {
                    p.SendMessage("No such copy exists");
                    return;
                }
                File.Delete("extra/savecopy/" + p.uuid + "/" + args + ".cpy");
                p.SendMessage("Deleted copy " + args);
                return;
            }
            if (args.ToLower() == "list") {
                if (!Directory.Exists("extra/savecopy/" + p.uuid)) {
                    p.SendMessage("No such directory exists");
                    return;
                }
                FileInfo[] fin = new DirectoryInfo("extra/savecopy/" + p.uuid).GetFiles();
                for (int i = 0; i < fin.Length; i++) {
                    p.SendMessage(fin[i].Name.Replace(".cpy", String.Empty));
                }
                return;
            }

            List<BlockId> ignoreTypes = new List<BlockId>();
            int copyType = 0;
            int allowOffset;

            p.copyoffset[0] = 0;
            p.copyoffset[1] = 0;
            p.copyoffset[2] = 0;

            allowOffset = (args.IndexOf('@'));
            if (allowOffset != -1) {
                args = args.Replace("@ ", String.Empty);
            }
            if (args.ToLower() == "cut") {
                copyType = 1;
                args = String.Empty;
            }
            else if (args.ToLower() == "air") {
                copyType = 2;
                args = String.Empty;
            }
            else if (args == "@") {
                args = String.Empty;
            }
            else if (args.IndexOf(' ') != -1) {
                if (args.Split(' ')[0] == "ignore") {
                    foreach (string s in args.Substring(args.IndexOf(' ') + 1).Split(' ')) {
                        if (BlockData.Ushort(s) != BlockId.Null) {
                            ignoreTypes.Add(BlockData.Ushort(s));
                            p.SendMessage("Ignoring &b" + s);
                        }
                    }
                }
                else {
                    Help(p);
                    return;
                }
                args = String.Empty;
            }

            if (args != String.Empty) {
                Help(p);
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_typeKey] = copyType;
            data[_ignoreKey] = ignoreTypes;
            data[_offsetKey] = allowOffset;

            const string prompt = "Place two blocks to determine the area's edges.";
            TwoBlockSelection.Start(p, data, prompt, SelectionFinished);
        }

        private void SelectionFinished(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            List<BlockId> ignoreTypes = c.GetData<List<BlockId>>(_ignoreKey);
            int copyType = c.GetData<int>(_typeKey);
            int allowOffset = c.GetData<int>(_offsetKey);

            p.CopyBuffer.Clear();

            p.copystart[0] = x1;
            p.copystart[1] = y1;
            p.copystart[2] = z1;

            int TotalAir = 0;
            if (copyType == 2) {
                p.copyAir = true;
            }
            else {
                p.copyAir = false;
            }

            BlockId b;

            for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx) {
                for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                    for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                        b = p.level.GetTile(xx, yy, zz);
                        if (_s.blockPerms.CanPlace(p, b)) {
                            if (b == BlockId.Air && copyType != 2 || ignoreTypes.Contains(b)) {
                                TotalAir++;
                            }

                            if (ignoreTypes.Contains(b)) {
                                BufferAdd(p, (ushort)(xx - x1), (ushort)(yy - y1), (ushort)(zz - z1), BlockId.Air);
                            }
                            else {
                                BufferAdd(p, (ushort)(xx - x1), (ushort)(yy - y1), (ushort)(zz - z1), b);
                            }
                        }
                        else {
                            BufferAdd(p, (ushort)(xx - x1), (ushort)(yy - y1), (ushort)(zz - z1), BlockId.Air);
                        }
                    }
                }
            }

            if ((p.CopyBuffer.Count - TotalAir) > p.rank.maxBlocks) {
                p.SendMessage("You tried to copy " + p.CopyBuffer.Count + " blocks.");
                p.SendMessage("You cannot copy more than " + p.rank.maxBlocks + ".");
                p.CopyBuffer.Clear();
                return;
            }

            if (copyType == 1) {
                for (ushort xx = Math.Min(x1, x2); xx <= Math.Max(x1, x2); ++xx) {
                    for (ushort yy = Math.Min(y1, y2); yy <= Math.Max(y1, y2); ++yy) {
                        for (ushort zz = Math.Min(z1, z2); zz <= Math.Max(z1, z2); ++zz) {
                            b = p.level.GetTile(xx, yy, zz);
                            if (b != BlockId.Air && _s.blockPerms.CanPlace(p, b)) {
                                p.level.Blockchange(p, xx, yy, zz, BlockId.Air);
                            }
                        }
                    }
                }
            }

            p.SendMessage((p.CopyBuffer.Count - TotalAir) + " blocks copied.");
            if (allowOffset != -1) {
                p.SendMessage("Place a block to determine where to paste from.");
                p.StartSelection(OffsetSelected, null);
            }
        }

        private void OffsetSelected(Player p, CommandTempData c) {
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));

            p.copyoffset[0] = (p.copystart[0] - x);
            p.copyoffset[1] = (p.copystart[1] - y);
            p.copyoffset[2] = (p.copystart[2] - z);

            p.SendMessage("Copy offset selected.");
        }

        private void SaveCopy(Player p, string message) {
            if (Player.ValidName(message)) {
                if (!Directory.Exists("extra/savecopy")) {
                    Directory.CreateDirectory("extra/savecopy");
                }
                if (!Directory.Exists("extra/savecopy/" + p.uuid)) {
                    Directory.CreateDirectory("extra/savecopy/" + p.uuid);
                }
                if (Directory.GetFiles("extra/savecopy/" + p.uuid).Length > 14) {
                    p.SendMessage("You can only save 15 copy's. /copy delete some.");
                    return;
                }
                using (FileStream fs = new FileStream("extra/savecopy/" + p.uuid + "/" + message + ".cpy", FileMode.Create)) {
                    ushort[] cnt = new ushort[p.CopyBuffer.Count * 7];
                    int k = 0;
                    for (int i = 0; i < p.CopyBuffer.Count; i++) {
                        BitConverter.GetBytes(p.CopyBuffer[i].x).CopyTo(cnt, 0 + k);
                        BitConverter.GetBytes(p.CopyBuffer[i].y).CopyTo(cnt, 2 + k);
                        BitConverter.GetBytes(p.CopyBuffer[i].z).CopyTo(cnt, 4 + k);
                        cnt[6 + k] = Convert.ToUInt16(p.CopyBuffer[i].type);
                        k = k + 7;
                    }
                    byte[] tmp = new byte[cnt.Length * 2];
                    for (int i = 0; i < cnt.Length * 2; ++i) {
                        if(cnt[i] != 0) {  // Air
                            BitConverter.GetBytes((ushort)cnt[i]).CopyTo(tmp, (i * 2));
                        }
                        else {
                            BitConverter.GetBytes(0).CopyTo(tmp, (i * 2));
                        }
                    }

                    fs.Write(tmp, 0, tmp.Length);
                    fs.Flush();
                    fs.Close();
                }
                p.SendMessage("Saved copy as " + message);
            }
            else {
                p.SendMessage("Bad file name");
            }
        }

        void Loadcopy(Player p, string message) {
            if (!File.Exists("extra/savecopy/" + p.uuid + "/" + message + ".cpy")) {
                p.SendMessage("No such copy exists");
                return;
            }
            p.CopyBuffer.Clear();
            using (FileStream fs = new FileStream("extra/savecopy/" + p.uuid + "/" + message + ".cpy", FileMode.Open)) {
                byte[] cnt = new byte[fs.Length];
                fs.Read(cnt, 0, (int)fs.Length);
                cnt = cnt.Decompress();
                int k = 0;
                for (int i = 0; i < cnt.Length / 7; i++) {
                    p.CopyBuffer.Add(new Player.CopyPos() {
                        x = BitConverter.ToUInt16(cnt, 0 + k), y = BitConverter.ToUInt16(cnt, 2 + k), z = BitConverter.ToUInt16(cnt, 4 + k),
                        type = (BlockId)cnt[6 + k]
                    });
                    k = k + 7;
                }
                fs.Flush();
                fs.Close();
            }
            p.SendMessage("Loaded copy as " + message);
        }

        void BufferAdd(Player p, ushort x, ushort y, ushort z, BlockId type) {
            Player.CopyPos pos;
            pos.x = x;
            pos.y = y;
            pos.z = z;
            pos.type = type;
            p.CopyBuffer.Add(pos);
        }

        /// <summary>
        /// Called when /help is used on /copy.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Too long; redo after planned additions to /help -Jjp137
            p.SendMessage("/copy - Copies the blocks in an area.");
            p.SendMessage("/copy save <save_name?> - Saves what you have copied.");
            p.SendMessage("/copy load <load_name?> - Loads what you have saved.");
            p.SendMessage("/copy delete <save_name?> - Deletes a saved copy.");
            p.SendMessage("/copy list - Lists all you have copied.");
            p.SendMessage("/copy cut - Copies the blocks in an area then removes them.");
            p.SendMessage("/copy air - Copies the blocks in an area, including air.");
            p.SendMessage("/copy ignore <block1> <block2> ... - Ignores " +
                               "the specified blocks when copying.");
            p.SendMessage("/copy @ - The @ is a toggle for all of the above, and it gives" +
                               " you a third click after copying that determines where to paste from.");
        }
    }
}

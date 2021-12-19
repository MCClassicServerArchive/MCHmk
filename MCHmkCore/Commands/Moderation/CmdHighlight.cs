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
using System.IO;

namespace MCHmk.Commands {
    public class CmdHighlight : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"mod", "high", "light", "block", "change", "history"});

        public override string Name {
            get {
                return "highlight";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "moderation";
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
        public CmdHighlight(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            BlockId b;
            Int64 seconds;
            Player who;
            Player.UndoPos Pos;
            int CurrentPos = 0;
            bool FoundUser = false;

            if (args == String.Empty) {
                args = p.name + " 300";
            }

            if (args.Split(' ').Length == 2) {
                try {
                    seconds = Int64.Parse(args.Split(' ')[1]);
                }
                catch {  // TODO: Find exact exception to catch
                    p.SendMessage("Invalid seconds.");
                    return;
                }
            }
            else {
                try {
                    seconds = int.Parse(args);
                    if (!p.IsConsole) {
                        args = p.name + " " + args;
                    }
                }
                catch {  // TODO: Find exact exception to catch
                    seconds = 300;
                    args = args + " 300";
                }
            }

            if (seconds == 0) {
                seconds = 5400;
            }

            string name = args.Split(' ')[0];

            who = _s.players.Find(name);
            if (who != null) {
                args = who.name + " " + seconds;
                FoundUser = true;
                for (CurrentPos = who.UndoBuffer.Count - 1; CurrentPos >= 0; --CurrentPos) {
                    try {
                        Pos = who.UndoBuffer[CurrentPos];
                        Level foundLevel = _s.levels.Find(Pos.mapName);
                        if (foundLevel == p.level) {
                            b = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);
                            if (Pos.timePlaced.AddSeconds(seconds) >= DateTime.Now) {
                                if (b == Pos.newtype || BlockData.Convert(b) == BlockId.ActiveWater || BlockData.Convert(b) == BlockId.ActiveLava) {
                                    if (b == BlockId.Air || BlockData.Convert(b) == BlockId.ActiveWater || BlockData.Convert(b) == BlockId.ActiveLava) {
                                        p.SendBlockchange(Pos.x, Pos.y, Pos.z, BlockId.Red);
                                    }
                                    else {
                                        p.SendBlockchange(Pos.x, Pos.y, Pos.z, BlockId.Green);
                                    }
                                }
                            }
                            else {
                                break;
                            }
                        }
                    }
                    catch (Exception e) { 
                        _s.logger.ErrorLog(e);
                    }
                }
            }

            Uuid uuid = Uuid.FindUuid(_s.database, name);
            if (!uuid.IsValid) {
                p.SendMessage("The player name is invalid or has never joined the server.");
                return;
            }

            try {
                DirectoryInfo di;
                string[] fileContent;

                if (Directory.Exists("extra/undo/" + uuid)) {
                    di = new DirectoryInfo("extra/undo/" + uuid);

                    for (int i = 0; i < di.GetFiles("*.undo").Length; i++) {
                        fileContent = File.ReadAllText("extra/undo/" + uuid + "/" + i + ".undo").Split(' ');
                        highlightStuff(fileContent, seconds, p);
                    }
                    FoundUser = true;
                }

                if (Directory.Exists("extra/undoPrevious/" + uuid)) {
                    di = new DirectoryInfo("extra/undoPrevious/" + uuid);

                    for (int i = 0; i < di.GetFiles("*.undo").Length; i++) {
                        fileContent = File.ReadAllText("extra/undoPrevious/" + uuid + "/" + i + ".undo").Split(' ');
                        highlightStuff(fileContent, seconds, p);
                    }
                    FoundUser = true;
                }

                if (FoundUser) {
                    p.SendMessage("Now highlighting &b" + seconds + _s.props.DefaultColor + " seconds for " +
                                       _s.ranks.FindPlayerColor(uuid) + name);
                    p.SendMessage("&cUse /reveal to un-highlight");
                }
                else {
                    p.SendMessage("Could not find player specified.");
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        public void highlightStuff(string[] fileContent, Int64 seconds, Player p) {
            Player.UndoPos Pos;

            for (int i = (fileContent.Length / 7 - 1); i >= 0; i--) {
                try {
                    if (Convert.ToDateTime(fileContent[(i * 7) + 4].Replace('&', ' ')).AddSeconds(seconds) >= DateTime.Now) {
                        Level foundLevel = _s.levels.Find(fileContent[i * 7]);
                        if (foundLevel != null && foundLevel == p.level) {
                            Pos.mapName = foundLevel.name;
                            Pos.x = Convert.ToUInt16(fileContent[(i * 7) + 1]);
                            Pos.y = Convert.ToUInt16(fileContent[(i * 7) + 2]);
                            Pos.z = Convert.ToUInt16(fileContent[(i * 7) + 3]);

                            Pos.type = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);

                            if (Convert.ToInt32(Pos.type) == Convert.ToByte(fileContent[(i * 7) + 6]) || BlockData.Convert(Pos.type) == BlockId.ActiveWater
                                    || BlockData.Convert(Pos.type) == BlockId.ActiveLava) {
                                if (Pos.type == BlockId.Air || BlockData.Convert(Pos.type) == BlockId.ActiveWater || BlockData.Convert(Pos.type) == BlockId.ActiveLava) {
                                    p.SendBlockchange(Pos.x, Pos.y, Pos.z, BlockId.Red);
                                }
                                else {
                                    p.SendBlockchange(Pos.x, Pos.y, Pos.z, BlockId.Green);
                                }
                            }
                        }
                    }
                    else {
                        break;
                    }
                }
                catch (Exception e) { 
                    _s.logger.ErrorLog(e);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /highlight.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/highlight <player?> <seconds?> - Highlights the blocks " +
                               "modified by a player during the given time period.");
            p.SendMessage("/highlight <player?> 0 - Will highlight 30 minutes.");
            p.SendMessage("Added blocks are green, and removed blocks are red.");
            p.SendMessage("To remove the effects of /highlight, use /reveal.");
        }
    }
}

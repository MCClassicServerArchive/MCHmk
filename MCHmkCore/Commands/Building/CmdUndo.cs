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
using System.Globalization;
using System.IO;

namespace MCHmk.Commands {
    public class CmdUndo : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"redo", "action", "block", "change"});

        public override string Name {
            get {
                return "undo";
            }
        }
        public override string Shortcut {
            get {
                return "u";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdUndo(Server s) : base(s) { }

        int MAX = -1; // This is the value changed to MAX in the Undo list, and used to allow everything undone.

        public override void Use(Player p, string args) {
            BlockId b;
            long seconds = -2;
            Player who = null;
            Player.UndoPos Pos;
            int CurrentPos = 0;
            bool undoPhysics = false;
            string whoName = String.Empty;
            if (!p.IsConsole) {
                p.RedoBuffer.Clear();
            }

            if (args == String.Empty) {
                if (p.IsConsole) {
                    _s.logger.Log("Console doesn't have an undo buffer.");
                    return;
                }
                args = p.name.ToLower() + " 30";
            }

            try {
                if (args.Split(' ').Length > 1) {
                    whoName = args.Split(' ')[0];
                    who = args.Split(' ')[0].ToLower() == "physics" ? null : _s.players.Find(args.Split(' ')[0]);
                    undoPhysics = args.Split(' ')[0].ToLower() == "physics";
                    args = args.Split(' ')[1].ToLower();

                }
                else {
                    who = (p.IsConsole || args.ToLower() == "physics") ? null : p;
                    undoPhysics = args.ToLower() == "physics";
                }
                //If user is undoing him/herself, then all is go.
                //If user is undoing someone else, then restrictions are used.
                if (p == who) {
                    seconds = ((args.ToLower() != "all") ? long.Parse(args) : int.MaxValue);
                }
                else {
                    seconds = getAllowed(p, args.ToLower());
                }
            }
            catch {  // TODO: find exact exception to catch
                p.SendMessage("Invalid seconds, or you're unable to use /xundo. Using 30 seconds."); //only run if seconds is an invalid number
                seconds = 30;
            }

            //At this point, we know the number is valid, and allowed for the particular person's group.
            if (who != null) {
                if (!p.IsConsole) {
                    if (who.rank.Permission > p.rank.Permission && who != p) {
                        p.SendMessage("Cannot undo a user of higher or equal rank");
                        return;
                    }
                    if (who != p && p.rank.Permission < _s.commands.GetOtherPerm(this, 1)) {
                        p.SendMessage("Only an " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                           1)).name + "+ may undo other people's actions");
                        return;
                    }
                }
                Level saveLevel = null;
                for (CurrentPos = who.UndoBuffer.Count - 1; CurrentPos >= 0; --CurrentPos) {
                    try {
                        Pos = who.UndoBuffer[CurrentPos];
                        Level foundLevel = _s.levels.FindExact(Pos.mapName);
                        saveLevel = foundLevel;
                        b = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);
                        if (Pos.timePlaced.AddSeconds(seconds) >= DateTime.Now) {
                            if (b == Pos.newtype || BlockData.Convert(b) == BlockId.ActiveWater || BlockData.Convert(b) == BlockId.ActiveLava) {
                                foundLevel.Blockchange(Pos.x, Pos.y, Pos.z, Pos.type, true);

                                Pos.newtype = Pos.type;
                                Pos.type = b;
                                if (!p.IsConsole) {
                                    p.RedoBuffer.Add(Pos);
                                }
                                who.UndoBuffer.RemoveAt(CurrentPos);
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
                try {
                    bool fake = false;
                    undoOfflineHelper(p, who.uuid.Value, seconds, ref fake);
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }

                if (p == who) {
                    p.SendMessage("Undid your actions for the past &b" + seconds + _s.props.DefaultColor + " seconds.");
                }
                else {
                    _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + "'s actions for the past &b" + seconds +
                                      " seconds were undone.", false);
                    // Also notify console
                    _s.logger.Log(who.name + "'s actions for the past " + seconds + " seconds were undone.");
                }
                // Don't forget to save the map;) in case someone unloads it without a manual blockchange:D
                if (saveLevel != null) {
                    saveLevel.Save(true);
                }
                return;
            }
            else if (undoPhysics) {
                if (!p.IsConsole && p.rank.Permission < _s.commands.GetOtherPerm(this, 2)) {
                    p.SendMessage("Reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 2)).name + "+");
                    return;
                }
                if (!p.IsConsole && !_s.commands.CanExecute(p, "physics")) {
                    p.SendMessage("You can only undo physics if you can use them.");
                    return;
                }

                _s.commands.FindCommand("physics").Use(p, "0");
                Level.UndoPos uP;
                ushort x, y, z;

                if (p.level.UndoBuffer.Count != _s.props.physUndo) {
                    for (CurrentPos = p.level.currentUndo; CurrentPos >= 0; CurrentPos--) {
                        try {
                            uP = p.level.UndoBuffer[CurrentPos];
                            b = p.level.GetTile(uP.location);
                            if (uP.timePerformed.AddSeconds(seconds) < DateTime.Now) {
                                break;
                            }
                            if (b == uP.newType || BlockData.Convert(b) == BlockId.ActiveWater || BlockData.Convert(b) == BlockId.ActiveLava) {
                                p.level.IntToPos(uP.location, out x, out y, out z);
                                p.level.Blockchange(p, x, y, z, uP.oldType);
                            }
                        }
                        catch (Exception e) {
                            _s.logger.ErrorLog(e);
                        }
                    }
                }
                else {
                    for (CurrentPos = p.level.currentUndo; CurrentPos != p.level.currentUndo + 1; CurrentPos--) {
                        try {
                            if (CurrentPos < 0) {
                                CurrentPos = p.level.UndoBuffer.Count - 1;
                            }
                            uP = p.level.UndoBuffer[CurrentPos];
                            b = p.level.GetTile(uP.location);
                            if (uP.timePerformed.AddSeconds(seconds) < DateTime.Now) {
                                break;
                            }
                            if (b == uP.newType || BlockData.Convert(b) == BlockId.ActiveWater || BlockData.Convert(b) == BlockId.ActiveLava) {
                                p.level.IntToPos(uP.location, out x, out y, out z);
                                p.level.Blockchange(p, x, y, z, uP.oldType, true);
                            }
                        }
                        catch (Exception e) {
                            _s.logger.ErrorLog(e);
                        }
                    }
                }

                _s.GlobalMessage("Physics were undone &b" + seconds + _s.props.DefaultColor + " seconds");
                // Also notify console
                _s.logger.Log("Physics were undone &b" + seconds + _s.props.DefaultColor + " seconds");
                // Don't forget to save the map;) in case someone unloads it without a manual blockchange:D
                p.level.Save(true);
            }
            else {
                // Here, who == null, meaning the user specified is offline
                if (!p.IsConsole) {
                    if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                        p.SendMessage("Reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name + "+");
                        return;
                    }
                    // ^^^ is using the same as the 1st other permission for the this command because the only difference is that this is for offline players so it might aswell be the same!!
                }

                bool FoundUser = false;

                try {
                    Uuid uuid = Uuid.FindUuid(_s.database, whoName);
                    if (!uuid.IsValid) {
                        p.SendMessage("The player name is invalid or has never joined the server.");
                        return;
                    }

                    undoOfflineHelper(p, uuid.Value, seconds, ref FoundUser);

                    if (FoundUser) {
                        _s.GlobalMessage(_s.ranks.FindPlayerColor(uuid) + whoName + _s.props.DefaultColor + "'s actions for the past &b" + seconds
                                             + _s.props.DefaultColor + " seconds were undone.");
                        // Also notify console
                        _s.logger.Log(whoName + "'s actions for the past " + seconds + " seconds were undone.");
                        // Don't forget to save the map;) in case someone unloads it without a manual blockchange:D
                        p.level.Save(true);
                    }
                    else {
                        p.SendMessage("Could not find player specified.");
                    }
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }
            }
        }

        private long getAllowed(Player p, string param) {
            long secs;
            if (param == "all" && _s.commands.CanExecute(p, "xundo")) {
                secs = (p.rank.maxUndo == MAX) ? int.MaxValue : p.rank.maxUndo;
            }
            else {
                secs = long.Parse(param);    //caught by try/catch in outer method
            }

            if (secs == 0) {
                secs = 5400;
            }

            if (!p.IsConsole && p.rank.maxUndo != MAX && secs > p.rank.maxUndo) {
                p.SendMessage(p.rank.name + "s may only undo up to " + p.rank.maxUndo + " seconds.");
                return p.rank.maxUndo;
            }
            return secs;
        }

        //Fixed by QuantumHive
        public bool undoOffline(string[] fileContent, long seconds, Player p) {

            Player.UndoPos Pos;

            //-1 because the last element in the array is an empty string String.Empty go check Player.SaveUndo() if you wanna know why
            for (int i = (fileContent.Length - 1) / 7; i >= 0; i--) {
                try {
                    string datetime = fileContent[(i * 7) - 3];
                    datetime = datetime.Replace('&', ' ');
                    DateTime time = DateTime.Parse(datetime, CultureInfo.InvariantCulture);
                    time = time.AddSeconds(seconds);
                    if (time < DateTime.Now)
                        //if (Convert.ToDateTime(fileContent[(i * 7) - 3].Replace('&', ' ')).AddSeconds(seconds) < DateTime.Now)
                    {
                        return false;
                    }

                    Level foundLevel = _s.levels.FindExact(fileContent[(i * 7) - 7]);
                    if (foundLevel != null) {
                        Pos.mapName = foundLevel.name;
                        Pos.x = Convert.ToUInt16(fileContent[(i * 7) - 6]);
                        Pos.y = Convert.ToUInt16(fileContent[(i * 7) - 5]);
                        Pos.z = Convert.ToUInt16(fileContent[(i * 7) - 4]);

                        Pos.type = foundLevel.GetTile(Pos.x, Pos.y, Pos.z);

                        if (Convert.ToInt32(Pos.type) == Convert.ToByte(fileContent[(i * 7) - 1]) ||
                                BlockData.Convert(Pos.type) == BlockId.ActiveWater || BlockData.Convert(Pos.type) == BlockId.ActiveLava ||
                                Pos.type == BlockId.Grass) {
                            Pos.newtype = (BlockId)Convert.ToByte(fileContent[(i * 7) - 2]);
                            Pos.timePlaced = DateTime.Now;

                            foundLevel.Blockchange(Pos.x, Pos.y, Pos.z, Pos.newtype, true);
                            if (!p.IsConsole) {
                                p.RedoBuffer.Add(Pos);
                            }
                        }
                    }
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }
            }

            return true;
        }
        private void undoOfflineHelper(Player p, string uuid, long seconds, ref bool FoundUser) {
            DirectoryInfo di;
            string[] fileContent;

            if (Directory.Exists("extra/undo/" + uuid.ToLower())) {
                di = new DirectoryInfo("extra/undo/" + uuid.ToLower());

                for (int i = di.GetFiles("*.undo").Length - 1; i >= 0; i--) {
                    fileContent = File.ReadAllText("extra/undo/" + uuid.ToLower() + "/" + i + ".undo").Split();
                    if (!undoOffline(fileContent, seconds, p)) {
                        break;
                    }
                }
                FoundUser = true;
            }

            if (Directory.Exists("extra/undoPrevious/" + uuid.ToLower())) {
                di = new DirectoryInfo("extra/undoPrevious/" + uuid.ToLower());

                for (int i = di.GetFiles("*.undo").Length - 1; i >= 0; i--) {
                    fileContent = File.ReadAllText("extra/undoPrevious/" + uuid.ToLower() + "/" + i + ".undo").Split();
                    if (!undoOffline(fileContent, seconds, p)) {
                        break;
                    }
                }
                FoundUser = true;
            }
        }

        /// <summary>
        /// Called when /help is used on /undo.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/undo <player?> [seconds?] - Undoes the block changes " +
                               "made by a player in the past given amount of seconds. Default is 30 seconds.");
            if (p.IsConsole || (p.rank.maxUndo <= 500000 || p.rank.maxUndo == 0))
                p.SendMessage("/undo <player?> all - &cWill undo 68 years, 18 days, " +
                                   "15 hours, 28 minutes, 31 seconds for the specified player.");
            if (p.IsConsole || (p.rank.maxUndo <= 1800 || p.rank.maxUndo == 0)) {
                p.SendMessage("/undo <player?> 0 - &cWill undo 30 minutes.");
            }
            p.SendMessage("/undo physics <seconds?> - " +
                               "Undoes the physics for the current map.");
        }
    }
}

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

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdZone : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the owner of the zone to be created.
        /// </summary>
        private readonly string _ownerKey = "zone_owner";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"area"});

        public override string Name {
            get {
                return "zone";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdZone(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                p.ZoneCheck = true;
                p.SendMessage("Place a block where you would like to check for zones.");
                return;
            }
            else if (p.rank.Permission < _s.commands.GetOtherPerm(this, 1)) {
                p.SendMessage("Reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 1)).name + "+");
                return;
            }

            if (args.IndexOf(' ') == -1) {
                if (p.canBuild) { //Checks if player can build there
                    switch (args.ToLower()) { //If true - they can delete the zone
                    case "del":
                        p.zoneDel = true;
                        p.SendMessage("Place a block where you would like to delete a zone.");
                        return;
                    default:
                        Help(p);
                        return;
                    }
                }
                else { //if they cant, it warns them, the ops and logs it on the server!
                    p.SendMessage("You can't delete a zone which is above your rank!");
                    _s.GlobalMessageOps(p.name + " tried to delete a zone that is above their rank!");
                    _s.logger.Log(p.name + " tried to delete a zone that is above their rank!");
                    return;
                }
            }


            if (args.ToLower() == "del all") {
                if (p.rank.Permission < _s.commands.GetOtherPerm(this, 2)) {
                    p.SendMessage("Only a " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                       2)).name + "+ may delete all zones at once");
                    return;
                }
                else {
                    for (int i = 0; i < p.level.ZoneList.Count; i++) {
                        Level.Zone Zn = p.level.ZoneList[i];
                        // FIXME: PreparedStatement, transaction
                        string query = "DELETE FROM `Zone" + p.level.name + "` WHERE Owner='" + Zn.owner + 
                            "' AND SmallX='" + Zn.smallX + "' AND SMALLY='" + Zn.smallY + "' AND SMALLZ='" + Zn.smallZ +
                            "' AND BIGX='" + Zn.bigX + "' AND BIGY='" + Zn.bigY + "' AND BIGZ='" + Zn.bigZ + "'";
                        _s.database.ExecuteStatement(query);

                        p.SendMessage("Zone deleted for &b" + Zn.owner);
                        p.level.ZoneList.Remove(p.level.ZoneList[i]);
                        if (i == p.level.ZoneList.Count) {
                            p.SendMessage("Finished removing all zones");
                            return;
                        }
                        i--;
                    }
                }
            }


            if (p.rank.Permission < _s.commands.GetOtherPerm(this, 3)) {
                p.SendMessage("Setting zones is reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this, 3)).name);
                return;
            }

            if (_s.ranks.Find(args.Split(' ')[1]) != null) {
                args = args.Split(' ')[0] + " grp" + _s.ranks.Find(args.Split(' ')[1]).name;
            }

            
            string owner = String.Empty;
            if (args.Split(' ')[0].ToLower() == "add") {
                Player foundPlayer = _s.players.Find(args.Split(' ')[1]);
                if (foundPlayer == null) {
                    owner = args.Split(' ')[1].ToString();
                }
                else {
                    owner = foundPlayer.name;
                }
            }
            else {
                Help(p);
                return;
            }

            if (!Player.ValidName(owner)) {
                p.SendMessage("Invalid name.");
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_ownerKey] = owner;

            const string prompt = "Place two blocks to determine the zone's edges.";
            TwoBlockSelection.Start(p, data, prompt, ZoneSelected);
        }

        /// <summary>
        /// Called when a player has finished selecting the two corners of the zone to be created.
        /// </summary>
        /// <param name="p"> The player that has selected both blocks. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the second block's selection. <seealso cref="CommandTempData"/></param>
        private void ZoneSelected(Player p, CommandTempData c) {
            // Obtain the coordinates of both corners. The first corner is stored in the CommandTempData's Dictionary,
            // while the second corner is contained within the X, Y, and Z properties of the CommandTempData since
            // that block change occurred just now.
            ushort x1 = c.GetData<ushort>(TwoBlockSelection.XKey);
            ushort y1 = c.GetData<ushort>(TwoBlockSelection.YKey);
            ushort z1 = c.GetData<ushort>(TwoBlockSelection.ZKey);

            ushort x2 = c.X;
            ushort y2 = c.Y;
            ushort z2 = c.Z;

            string owner = c.GetData<string>(_ownerKey);

            Level.Zone Zn = new Level.Zone();

            Zn.smallX = Math.Min(x1, x2);
            Zn.smallY = Math.Min(y1, y2);
            Zn.smallZ = Math.Min(z1, z2);
            Zn.bigX = Math.Max(x1, x2);
            Zn.bigY = Math.Max(y1, y2);
            Zn.bigZ = Math.Max(z1, z2);
            Zn.owner = owner;

            p.level.ZoneList.Add(Zn);

            // FIXME: PreparedStatement
            string query = "INSERT INTO `Zone" + p.level.name + "` (SmallX, SmallY, SmallZ, BigX, BigY, BigZ, Owner) VALUES (" +
                Zn.smallX.ToString() + ", " + Zn.smallY.ToString() + ", " + Zn.smallZ.ToString() + ", " + Zn.bigX.ToString() +
                    ", " + Zn.bigY.ToString() + ", " + Zn.bigZ.ToString() + ", '" + Zn.owner + "')";
            _s.database.ExecuteStatement(query);

            p.SendMessage("Added zone for &b" + owner + _s.props.DefaultColor + ".");
        }

        /// <summary>
        /// Called when /help is used on /zone.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/zone add <player?> - Creates a zone in which only the " +
                               "specified player can build in.");
            p.SendMessage("/zone add <rank?> - Creates a zone in which only those of " +
                               "the specified rank or higher can build in.");
            p.SendMessage("/zone del - Deletes a zone.");
        }
    }
}

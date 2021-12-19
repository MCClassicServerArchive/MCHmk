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
using System.Data;
using System.IO;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdPortal : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the list of entry blocks that were placed.
        /// </summary>
        private readonly string _entriesKey = "portal_entries";
        /// <summary>
        /// Name of the key used to store and retrieve whether multiple entry portals are being placed.
        /// </summary>
        private readonly string _multiKey = "portal_multi";
        /// <summary>
        /// Name of the key used to store and retrieve the block type used for the portals.
        /// </summary>
        private readonly string _typeKey = "portal_type";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"teleport", "tp", "move", "transport"});

        public override string Name {
            get {
                return "portal";
            }
        }
        public override string Shortcut {
            get {
                return "o";
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
        public CmdPortal(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            bool multi = false;
            BlockId type;

            if (args.IndexOf(' ') != -1) {
                if (args.Split(' ')[1].ToLower() == "multi") {
                    multi = true;
                    args = args.Split(' ')[0];
                }
                else {
                    p.SendMessage("Invalid parameters");
                    return;
                }
            }

            if (args.ToLower() == "blue" || args == String.Empty) {
                type = BlockId.BluePortal;
            }
            else if (args.ToLower() == "orange") {
                type = BlockId.OrangePortal;
            }
            else if (args.ToLower() == "air") {
                type = BlockId.AirPortal;
            }
            else if (args.ToLower() == "water") {
                type = BlockId.WaterPortal;
            }
            else if (args.ToLower() == "lava") {
                type = BlockId.LavaPortal;
            }
            else if (args.ToLower() == "show") {
                ShowPortals(p);
                return;
            }
            else {
                Help(p);
                return;
            }

            p.ClearSelection();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_entriesKey] = new List<Tuple<UShortCoords, string>>();
            data[_multiKey] = multi;
            data[_typeKey] = type;

            p.StartSelection(EntryPlaced, data);
            p.SendMessage("Place an entry block for the portal.");
        }

        private void EntryPlaced(Player p, CommandTempData c) {
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            List<Tuple<UShortCoords, string>> entries = c.GetData<List<Tuple<UShortCoords, string>>>(_entriesKey);
            bool multi = c.GetData<bool>(_multiKey);
            BlockId portalType = c.GetData<BlockId>(_typeKey);

            if (multi && c.BlockType == BlockId.Red && entries.Count > 0) {
                ExitPlaced(p, c);
                return;
            }

            p.level.Blockchange(p, x, y, z, portalType);
            p.SendBlockchange(x, y, z, BlockId.Green);

            UShortCoords coords = new UShortCoords(x, y, z);
            Tuple<UShortCoords, string> t = new Tuple<UShortCoords, string>(coords, p.level.name);
            entries.Add(t);

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_entriesKey] = entries;
            data[_multiKey] = multi;
            data[_typeKey] = portalType;

            if (!multi) {
                p.StartSelection(ExitPlaced, data);
                p.SendMessage("Entry block placed. Place the exit block.");
            }
            else {
                p.StartSelection(EntryPlaced, data);
                p.SendMessage("Entry block placed. Place more or a red block for the exit.");
            }
        }

        private void ExitPlaced(Player p, CommandTempData c) {
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            p.SendBlockchange(x, y, z, p.level.GetTile(x, y, z));

            List<Tuple<UShortCoords, string>> entries = c.GetData<List<Tuple<UShortCoords, string>>>(_entriesKey);
            BlockId portalType = c.GetData<BlockId>(_typeKey);

            foreach (Tuple<UShortCoords, string> entry in entries) {
                UShortCoords pos = entry.Item1;

                // FIXME: PreparedStatement
                string query = "SELECT * FROM `Portals" + entry.Item2 + "` WHERE EntryX=" + (int)pos.X + " AND EntryY=" + 
                    (int)pos.Y + " AND EntryZ=" + (int)pos.Z;
                DataTable Portals = _s.database.ObtainData(query);
                Portals.Dispose();

                if (Portals.Rows.Count == 0) {
                    // FIXME: PreparedStatement
                    string insert = "INSERT INTO `Portals" + entry.Item2 + 
                        "` (EntryX, EntryY, EntryZ, ExitMap, ExitX, ExitY, ExitZ) VALUES (" +
                            (int)pos.X + ", " + (int)pos.Y + ", " + (int)pos.Z + ", '" + p.level.name + "', " +
                            x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")";
                    _s.database.ExecuteStatement(insert);

                }
                else {
                    // FIXME: PreparedStatement
                    string update = "UPDATE `Portals" + entry.Item2 + "` SET ExitMap='" + p.level.name + 
                        "', ExitX=" + x.ToString() + ", ExitY=" + y.ToString() + ", ExitZ=" + z.ToString() + 
                            " WHERE EntryX=" + (int)pos.X + " AND EntryY=" + (int)pos.Y + " AND EntryZ=" + (int)pos.Z;
                    _s.database.ExecuteStatement(update);
                }

                if (entry.Item2 == p.level.name) {
                    p.SendBlockchange(pos.X, pos.Y, pos.Z, portalType);
                }
            }

            p.SendMessage("Exit block placed.");

            HandleStaticMode(p, c);
        }

        private void HandleStaticMode(Player p, CommandTempData c) {
            if (!p.staticCommands) {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();

            data[_entriesKey] = new List<Tuple<UShortCoords, string>>();
            data[_multiKey] = c.GetData<bool>(_multiKey);
            data[_typeKey] = c.GetData<BlockId>(_typeKey);

            p.StartSelection(EntryPlaced, data);
        }

        // Called when using /portal show
        private void ShowPortals(Player p) {
            p.showPortals = !p.showPortals; // Toggle the setting

            // Get the data
            string show = "SELECT * FROM Portals" + p.level.name;
            DataTable Portals = _s.database.ObtainData(show);

            int i; // Yes, this is outside the for loop because it gets used later.

            // Go through each portal
            for (i = 0; i < Portals.Rows.Count; i++) {
                // This is the kind of crap code I have to write in order to prevent an
                // InvalidCastException :\ -Jjp137

                // Anyways, get the entry/exit coordinates of the current portal
                // This is the only reliable way to get the coordinates to be unsigned shorts :\
                // Some databases use shorts and some use longs, which can cause problems
                ushort exitX = Convert.ToUInt16(Portals.Rows[i]["ExitX"].ToString());
                ushort exitY = Convert.ToUInt16(Portals.Rows[i]["ExitY"].ToString());
                ushort exitZ = Convert.ToUInt16(Portals.Rows[i]["ExitZ"].ToString());
                ushort entryX = Convert.ToUInt16(Portals.Rows[i]["EntryX"].ToString());
                ushort entryY = Convert.ToUInt16(Portals.Rows[i]["EntryY"].ToString());
                ushort entryZ = Convert.ToUInt16(Portals.Rows[i]["EntryZ"].ToString());

                // Only show/hide orange portals if the destination map is the same
                // as the source map (air is used to hide them)
                if (Portals.Rows[i]["ExitMap"].ToString() == p.level.name) {
                    p.SendBlockchange(exitX, exitY, exitZ, p.showPortals ?
                                      BlockId.OrangePortal : BlockId.Air);
                }
                // Show/hide all entry portals
                p.SendBlockchange(entryX, entryY, entryZ, p.showPortals ?  BlockId.BluePortal
                                  : p.level.GetTile(entryX, entryY, entryZ));
            }

            // Display a message
            p.SendMessage(p.showPortals ?
                               "Showing &a" + i.ToString() + _s.props.DefaultColor + " portals."
                               : "Hiding portals.");

            // And of course...
            Portals.Dispose();
        }

        /// <summary>
        /// Called when /help is used on /portal.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/portal [type?] - Activates portal mode.");
            p.SendMessage("/portal <type?> multi - Place entry blocks until you place " +
                               "an exit. All entry blocks placed will go to the same exit.");
            p.SendMessage("/portal show - Shows portals and their destinations.");
            p.SendMessage("Use again to hide.");
            p.SendMessage("Portal types: blue, orange, air, water, lava");
        }
    }
}

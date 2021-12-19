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

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdMessageBlock : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the type of message block being placed.
        /// </summary>
        private readonly string _blockKey = "mb_block";
        /// <summary>
        /// Name of the key used to store and retrieve the message that the message block will display.
        /// </summary>
        private readonly string _messageKey = "mb_message";

        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"message", "block", "msg"});

        public override string Name {
            get {
                return "mb";
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
        public CmdMessageBlock(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            BlockId type;
            string blockMsg = String.Empty;

            try {
                switch (args.Split(' ')[0]) {
                case "air":
                    type = BlockId.AirMessage;
                    break;
                case "water":
                    type = BlockId.WaterMessage;
                    break;
                case "lava":
                    type = BlockId.LavaMessage;
                    break;
                case "black":
                    type = BlockId.BlackMessage;
                    break;
                case "white":
                    type = BlockId.WhiteMessage;
                    break;
                case "show":
                    showMBs(p);
                    return;
                default:
                    type = BlockId.WhiteMessage;
                    blockMsg = args;
                    break;
                }
            }
            catch {  // TODO: find exact exception to catch (or rewrite this command)
                type = BlockId.WhiteMessage;
                blockMsg = args;
            }

            if (blockMsg == String.Empty) {
                blockMsg = args.Substring(args.IndexOf(' ') + 1);
            }

            p.ClearSelection();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = type;
            data[_messageKey] = blockMsg;

            p.StartSelection(BlockSelected, data);
            p.SendMessage("Place where you wish the message block to go.");
        }

        private void BlockSelected(Player p, CommandTempData c) {
            p.ClearSelection();

            ushort x = c.X;
            ushort y = c.Y;
            ushort z = c.Z;

            BlockId type = c.GetData<BlockId>(_blockKey);

            string blockMsg = c.GetData<string>(_messageKey);
            blockMsg = blockMsg.Replace("'", _s.props.useMySQL ? "\\'" : "''");

            // FIXME: PreparedStatement
            string query = "SELECT * FROM `Messages" + p.level.name + "` WHERE X=" + x.ToString() + " AND Y=" + 
                y.ToString() + " AND Z=" + z.ToString();
            DataTable Messages = _s.database.ObtainData(query);
            Messages.Dispose();

            if (Messages.Rows.Count == 0) {
                // FIXME: PreparedStatement
                string insert = "INSERT INTO `Messages" + p.level.name + "` (X, Y, Z, Message) VALUES (" + x.ToString() +
                    ", " + y.ToString() + ", " + z.ToString() + ", '" + blockMsg + "')";
                _s.database.ExecuteStatement(insert);
            }
            else {
                // FIXME: PreparedStatement
                string update = "UPDATE `Messages" + p.level.name + "` SET Message='" + blockMsg + "' WHERE X=" +
                    x.ToString() + " AND Y=" + y.ToString() + " AND Z=" + z.ToString();
                _s.database.ExecuteStatement(update);
            }

            p.SendMessage("Message block placed.");
            p.level.Blockchange(p, x, y, z, type);
            p.SendBlockchange(x, y, z, type);

            HandleStaticMode(p, c);
        }

        private void HandleStaticMode(Player p, CommandTempData c) {
            if (!p.staticCommands) {
                return;
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_blockKey] = c.GetData<BlockId>(_blockKey);
            data[_messageKey] = c.GetData<string>(_messageKey);

            p.StartSelection(BlockSelected, data);
        }

        // Called when /mb show is used
        private void showMBs(Player p) {
            p.showMBs = !p.showMBs; // Toggle the setting

            int i; // It's outside the for loop since it gets used later

            // Get the data
            string show = "SELECT * FROM Messages" + p.level.name;
            using (DataTable Messages = _s.database.ObtainData(show)) {
                for (i = 0; i < Messages.Rows.Count; i++) {
                    ushort x = Convert.ToUInt16(Messages.Rows[i]["X"].ToString());
                    ushort y = Convert.ToUInt16(Messages.Rows[i]["Y"].ToString());
                    ushort z = Convert.ToUInt16(Messages.Rows[i]["Z"].ToString());

                    // Show/hide the mbs
                    p.SendBlockchange(x, y, z, p.showMBs ? BlockId.WhiteMessage : p.level.GetTile(x, y, z));
                }
            }
            // Display a message
            p.SendMessage(p.showMBs ? "Showing &a" + i.ToString() + _s.props.DefaultColor + " MBs." : "Hiding MBs.");
        }

        /// <summary>
        /// Called when /help is used on /messageblock.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/mb <block?> <message?> - Places a message in your next block.");
            p.SendMessage("Valid blocks: white, black, air, water, and lava.");
            p.SendMessage("/mb show - Shows MBs. Use again to hide them.");
        }
    }
}

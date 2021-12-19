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
using System.IO;
using System.Text;

namespace MCHmk.Commands {
    /// <summary>
    /// This is the command /vip
    /// use /help vip in-game for more info
    /// </summary>
    public class CmdVIP : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"list", "add", "remove", "del"});

        public override string Name {
            get {
                return "vip";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }
        public CmdVIP(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (!File.Exists("text/vips.txt")) {
                File.Create("text/vips.txt").Dispose();
            }

            string[] split = args.Split(' ');
            if (split[0] == "add") {
                if (split.Length < 2) {
                    Help(p);
                    return;
                }
                Player pl = _s.players.Find(split[1]);
                if (pl != null) {
                    split[1] = pl.name;
                }
                if (VIP.Find(split[1])) {
                    p.SendMessage((pl == null ? String.Empty : pl.color) + split[1] + " is already a VIP!");
                }
                else {
                    VIP.Add(split[1]);
                    p.SendMessage((pl == null ? String.Empty : pl.color) + split[1] + " is now a VIP.");
                    if (pl != null) {
                        pl.SendMessage("You are now a VIP!");
                    }
                }
            }
            else if (split[0] == "remove") {
                if (split.Length < 2) {
                    Help(p);
                    return;
                }
                Player pl = _s.players.Find(split[1]);
                if (pl != null) {
                    split[1] = pl.name;
                }
                if (!VIP.Find(split[1])) {
                    p.SendMessage((pl == null ? String.Empty : pl.color) + split[1] + " is not a VIP!");
                }
                else {
                    VIP.Remove(split[1]);
                    p.SendMessage((pl == null ? String.Empty : pl.color) + split[1] + " is no longer a VIP.");
                    if (pl != null) {
                        pl.SendMessage("You are no longer a VIP!");
                    }
                }
            }
            else if (split[0] == "list") {
                List<string> list = VIP.GetAll();
                if (list.Count < 1) {
                    p.SendMessage("There are no VIPs.");
                }
                else {
                    StringBuilder sb = new StringBuilder();
                    foreach (string name in list) {
                        sb.Append(name).Append(", ");
                    }
                    p.SendMessage("There are " + list.Count + " VIPs:");
                    p.SendMessage(sb.Remove(sb.Length - 2, 2).ToString());
                }
            }
            else {
                Help(p);
            }
        }

        /// <summary>
        /// Called when /help is used on /vip.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/vip add <name?> - Add a VIP.");
            p.SendMessage("/vip remove <name?> - Remove a VIP.");
            p.SendMessage("/vip list - List all VIPs.");
            p.SendMessage("VIPs are players who can join regardless of the player limit.");
        }
    }
}

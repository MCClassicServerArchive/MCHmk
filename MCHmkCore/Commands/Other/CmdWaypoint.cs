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
    public class CmdWaypoint : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"way", "point"});

        public override string Name {
            get {
                return "waypoint";
            }
        }
        public override string Shortcut {
            get {
                return "wp";
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
        public CmdWaypoint(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            string[] command = args.ToLower().Split(' ');
            string par0 = String.Empty;
            string par1 = String.Empty;
            try {
                par0 = command[0];
                par1 = command[1];
            }
            catch {  // TODO: find exact exception to catch (or rewrite the code)

            }

            if (par0.ToLower() == "create" || par0.ToLower() == "new" || par0.ToLower() == "add") {
                if (!Waypoint.Exists(par1, p)) {
                    Waypoint.Create(par1, p);
                    p.SendMessage("Created waypoint");
                    return;
                }
                else {
                    p.SendMessage("That waypoint already exists");
                    return;
                }
            }
            else if (par0.ToLower() == "goto") {
                if (Waypoint.Exists(par1, p)) {
                    Waypoint.Goto(par1, p);
                    return;
                }
                else {
                    p.SendMessage("That waypoint doesn't exist");
                    return;
                }
            }
            else if (par0.ToLower() == "replace" || par0.ToLower() == "update" || par0.ToLower() == "edit") {
                if (Waypoint.Exists(par1, p)) {
                    Waypoint.Update(par1, p);
                    p.SendMessage("Updated waypoint");
                    return;
                }
                else {
                    p.SendMessage("That waypoint doesn't exist");
                    return;
                }
            }
            else if (par0.ToLower() == "delete" || par0.ToLower() == "remove") {
                if (Waypoint.Exists(par1, p)) {
                    Waypoint.Remove(par1, p);
                    p.SendMessage("Deleted waypoint");
                    return;
                }
                else {
                    p.SendMessage("That waypoint doesn't exist");
                    return;
                }
            }
            else if (par0.ToLower() == "list") {
                p.SendMessage("Waypoints:");
                foreach(Waypoint.WP wp in p.Waypoints) {
                    if (_s.levels.FindExact(wp.lvlname) != null) {
                        p.SendMessage(wp.name + ":" + wp.lvlname);
                    }
                }
                return;
            }
            else {
                if (Waypoint.Exists(par0, p)) {
                    Waypoint.Goto(par0, p);
                    p.SendMessage("Sent you to waypoint");
                    return;
                }
                else {
                    p.SendMessage("That waypoint or command doesn't exist");
                    return;
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /waypoint.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/waypoint create <name?> - Create a new waypoint.");
            p.SendMessage("/waypoint update <name?> - Update a waypoint.");
            p.SendMessage("/waypoint remove <name?> - Remove a waypoint.");
            p.SendMessage("/waypoint list - Shows a list of waypoints.");
            p.SendMessage("/waypoint <name?> - Teleport to a waypoint.");

        }
    }
}

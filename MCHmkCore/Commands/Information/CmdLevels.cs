/*
    Copyright 2016 Jjp137

    This file have been changed from the original source code by MCForge.

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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /levels command, which displays the list of loaded levels.
    /// </summary>
    public class CmdLevels : Command {
        /// <summary>
        /// The list of keywords that are associated with /levels.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"level", "map", "info", "list"});

        /// <summary>
        /// Gets the name of /levels.
        /// </summary>
        public override string Name {
            get {
                return "levels";
            }
        }

        /// <summary>
        /// Gets the shortcut for /levels.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /levels belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /levels. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /levels can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /levels.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        /// <summary>
        /// Constructs an instance of the /levels command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /levels will belong to.
        /// <seealso cref="Server"/></param>
        public CmdLevels(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /levels.
        /// </summary>
        /// <param name="p"> The player that used /levels. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // Display different output if the console used the command since the console can't
            // visit or build in levels.
            if (p.IsConsole) {
                ConsoleOutput(p);
            }
            else {
                PlayerOutput(p);
            }
        }

        /// <summary>
        /// Called if the user of the command is the console.
        /// </summary>
        /// <param name="p"> The Player object representing the console. </param>
        private void ConsoleOutput(Player p) {
            // The console just gets a straightforward list of all the levels.
            p.SendMessage("&aAll loaded levels:");

            StringBuilder output = new StringBuilder();
            foreach (Level lvl in _s.levels) {
                string color = _s.ranks.FindPerm(lvl.permissionbuild).color;
                output.Append(", " + color + lvl.name + " &b[" + lvl.physics.ToString() + "]");
                output.Remove(0, 2);  // Remove the leading comma.
            }
            p.SendMessage(output.ToString());

            // Remind the console that a list of unloaded levels exists.
            p.SendMessage("Use &4/unloaded for unloaded levels.");
        }

        /// <summary>
        /// Called if the user of the command is an in-game player.
        /// </summary>
        /// <param name="p"> The player that used the command. <seealso cref="Player"/></param>
        private void PlayerOutput(Player p) {
            // Use StringBuilder objects for performance reasons.
            StringBuilder canBuild = new StringBuilder();
            StringBuilder canVisit = new StringBuilder();
            StringBuilder noPerm = new StringBuilder();

            // For players, their rank's permission value is checked against the permission values of the
            // loaded levels, and each level is put in the appropriate category.
            p.SendMessage("&aAll loaded levels:");

            foreach (Level lvl in _s.levels) {
                // Save some typing.
                string color = _s.ranks.FindPerm(lvl.permissionbuild).color;

                // Check both perbuild and pervisit for the odd scenario in which the perbuild is lower
                // than the pervisit. You can't build in a map if you can't visit it.
                if (p.rank.Permission >= lvl.permissionbuild && p.rank.Permission >= lvl.permissionvisit) {
                    canBuild.Append(", " + color + lvl.name + " &b[" + lvl.physics.ToString() + "]");
                }
                // If the player can't build in a level, check if the player can at least visit it.
                else if (p.rank.Permission >= lvl.permissionvisit) {
                    canVisit.Append(", " + color + lvl.name + " &b[" + lvl.physics.ToString() + "]");
                }
                // If we get here, the player cannot build or visit that level.
                else {
                    noPerm.Append(", " + color + lvl.name + " &b[" + lvl.physics.ToString() + "]");
                }
            }

            // Display a category to the player if there is at least one level in it. Also, remove the leading
            // commas that were inserted at the start of each string.
            if (canBuild.Length > 0) {
                canBuild.Remove(0, 2);
                p.SendMessage("Can build: " + canBuild.ToString());
            }
            if (canVisit.Length > 0) {
                canVisit.Remove(0, 2);
                p.SendMessage("Can visit: " + canVisit.ToString());
            }
            if (noPerm.Length > 0) {
                noPerm.Remove(0, 2);
                p.SendMessage("Inaccessible: " + noPerm.ToString());
            }

            // Remind the player that a list of unloaded levels exists.
            p.SendMessage("Use &4/unloaded for unloaded levels.");
        }

        /// <summary>
        /// Called when /help is used on /levels.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/levels - Lists all loaded levels and their physics settings.");
        }
    }
}

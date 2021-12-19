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
    public class CmdSlap : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"slam", "facepalm"});

        public override string Name {
            get {
                return "slap";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdSlap(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            Player who = _s.players.Find(args);

            if (who == null) {
                Level which = _s.levels.FindExact(args);

                if (which == null) {
                    p.SendMessage("Could not find player or map specified");
                    return;
                }
                else {
                    foreach (Player pl in _s.players) {
                        if (pl.level == which && pl.rank.Permission < p.rank.Permission) {
                            _s.commands.FindCommand("slap").Use(p, pl.name);
                        }
                    }
                    return;
                }
            }
            if (!p.IsConsole) {
                if (who.rank.Permission > p.rank.Permission) {
                    p.SendMessage("You cannot slap someone ranked higher than you!");
                    return;
                }
            }

            ushort currentX = (ushort)(who.pos[0] / 32);
            ushort currentY = (ushort)(who.pos[1] / 32);
            ushort currentZ = (ushort)(who.pos[2] / 32);
            ushort foundHeight = 0;

            for (ushort yy = currentY; yy <= 1000; yy++) {
                if (!p.IsConsole) {
                    if (!BlockData.Walkthrough(p.level.GetTile(currentX, yy, currentZ))
                            && p.level.GetTile(currentX, yy, currentZ) != BlockId.Null) {
                        foundHeight = (ushort)(yy - 1);
                        who.level.ChatLevel(who.color + who.name + _s.props.DefaultColor + " was slapped into the roof by " + p.color + p.name);
                        break;
                    }
                }
                else {
                    if (!BlockData.Walkthrough(who.level.GetTile(currentX, yy, currentZ))
                            && who.level.GetTile(currentX, yy, currentZ) != BlockId.Null) {
                        foundHeight = (ushort)(yy - 1);
                        who.level.ChatLevel(who.color + who.name + _s.props.DefaultColor + " was slapped into the roof by " + "the Console.");
                        break;
                    }
                }
            }
            if (foundHeight == 0) {
                if (!p.IsConsole) {
                    who.level.ChatLevel(who.color + who.name + _s.props.DefaultColor + " was slapped sky high by " + p.color + p.name);
                }
                else {
                    who.level.ChatLevel(who.color + who.name + _s.props.DefaultColor + " was slapped sky high by " + "the Console.");
                }
                foundHeight = 1000;
            }

            unchecked {
                who.SendPos((byte)-1, who.pos[0], (ushort)(foundHeight * 32), who.pos[2], who.rot[0], who.rot[1]);
            }
        }

        /// <summary>
        /// Called when /help is used on /slap.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/slap <player?> - Slaps a player, knocking them into the air.");
            p.SendMessage("/slap <map?> - Slaps all players who are on the given map " +
                               "that are a lower rank than you, knocking them into the air.");
        }
    }
}

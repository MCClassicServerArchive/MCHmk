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
using System.IO;

namespace MCHmk.Commands {
    public class CmdGoto : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"level", "lvl", "change", "map"});

        public override string Name {
            get {
                return "goto";
            }
        }
        public override string Shortcut {
            get {
                return "g";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdGoto(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (args == String.Empty) {
                Help(p);
                return;
            }

            try {
                Level foundLevel = _s.levels.Find(args);
                if (foundLevel != null) {
                    Level startLevel = p.level;

                    if (p.level == foundLevel) {
                        p.SendMessage("You are already in \"" + foundLevel.name + "\".");
                        return;
                    }

                    if (p.rank.Permission < foundLevel.permissionvisit) {
                        p.SendMessage("You're not allowed to go to " + foundLevel.name + ".");
                        return;
                    }

                    if (p.rank.Permission > foundLevel.pervisitmax) {
                        if (!_s.commands.CanExecute(p, "pervisitmax")) {
                            p.SendMessage("Your rank must be " + _s.ranks.PermToName(foundLevel.pervisitmax) +
                                          " or lower to go there!");
                            return;
                        }
                    }

                    p.Loading = true;
                    foreach (Player pl in _s.players) {
                        if (p.level == pl.level && p != pl) {
                            p.SendDie(pl.serverId);
                        }
                    }
                    foreach (PlayerBot b in PlayerBot.playerbots) {
                        if (p.level == b.level) {
                            p.SendDie(b.id);
                        }
                    }

                    _s.GlobalDie(p, true);
                    p.level = foundLevel;
                    p.SendUserMOTD();
                    p.SendMap();

                    ushort x = (ushort)((0.5 + foundLevel.spawnx) * 32);
                    ushort y = (ushort)((1 + foundLevel.spawny) * 32);
                    ushort z = (ushort)((0.5 + foundLevel.spawnz) * 32);

                    if (!p.hidden) {
                        _s.GlobalSpawn(p, x, y, z, foundLevel.rotx, foundLevel.roty, true, String.Empty);
                    }
                    else unchecked {
                        p.SendPos((byte)-1, x, y, z, foundLevel.rotx, foundLevel.roty);
                    }

                    foreach (Player pl in _s.players)
                        if (pl.level == p.level && p != pl && !pl.hidden) {
                            p.SendSpawn(pl.serverId, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]);
                        }

                    foreach (PlayerBot b in PlayerBot.playerbots)
                        if (b.level == p.level) {
                            p.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);
                        }

                    if (!p.hidden) {
                        _s.GlobalChat(p, p.color + "*" + p.name + _s.props.DefaultColor + " went to &b" + foundLevel.name, false);
                    }

                    p.Loading = false;

                    bool skipUnload = false;
                    if (startLevel.unload && !startLevel.name.Contains("&cMuseum ")) {
                        foreach (Player pl in _s.players) {
                            if (pl.level == startLevel) {
                                skipUnload = true;
                                break;
                            }
                        }
                        if (!skipUnload && _s.props.AutoLoad) {
                            startLevel.Unload(true);
                        }
                    }

                }
                else if (_s.props.AutoLoad) {
                    // Check if the level exists regardless of the format it is saved in.
                    if (!File.Exists("levels/" + args + ".lvl") && !File.Exists("levels/" + args + ".mcf")) {
                        p.SendMessage("Level \"" + args + "\" doesn't exist!");
                    }
                    else if (_s.levels.Find(args) != null || Boolean.Parse(Level.GetLevelProperty(args, "loadongoto") ?? "false")) {
                        _s.commands.FindCommand("load").Use(p, args);
                        foundLevel = _s.levels.Find(args);
                        if (foundLevel != null) {
                            Use(p, args);
                        }
                    }
                    else {
                        p.SendMessage("Level \"" + args + "\" cannot be loaded using /goto!");
                    }
                }
                else {
                    p.SendMessage("There is no level \"" + args + "\" loaded.");
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /goto.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/goto <map?> - Teleports yourself to the specified map.");
        }
    }
}

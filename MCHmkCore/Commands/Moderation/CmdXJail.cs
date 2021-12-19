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
    public class CmdXJail : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"extra", "jail", "undo"});

        public override string Name {
            get {
                return "xjail";
            }
        }
        public override string Shortcut {
            get {
                return "xj";
            }
        }
        public override string Type {
            get {
                return "mod";
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
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

        public CmdXJail(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string dir = "extra/jail/";
            string jailMapFile = dir + "xjail.map.xjail";
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            if (!File.Exists(jailMapFile)) {
                using (StreamWriter SW = new StreamWriter(jailMapFile)) {
                    SW.WriteLine(_s.mainLevel.name);
                }
            }
            if (args == String.Empty) {
                Help(p);
                return;
            }
            else {
                using (StreamReader SR = new StreamReader(jailMapFile)) {
                    string xjailMap = SR.ReadLine();
                    SR.Close();
                    Command jail = _s.commands.FindCommand("jail");
                    if (args == "set") {
                        if (!p.level.name.Contains("cMuseum")) {
                            jail.Use(p, "create");
                            using (StreamWriter SW = new StreamWriter(jailMapFile)) {
                                SW.WriteLine(p.level.name);
                            }
                            p.SendMessage("The xjail map was set from '" + xjailMap + "' to '" + p.level.name + "'");
                            return;
                        }
                        else {
                            p.SendMessage("You are in a museum!");
                            return;
                        }
                    }
                    else {
                        Player player = _s.players.Find(args);

                        if (player != null) {
                            Command move = _s.commands.FindCommand("move");
                            Command spawn = _s.commands.FindCommand("spawn");
                            Command freeze = _s.commands.FindCommand("freeze");
                            Command mute = _s.commands.FindCommand("mute");
                            string playerFile = dir + player.name + "_temp.xjail";
                            if (!File.Exists(playerFile)) {
                                using (StreamWriter writeFile = new StreamWriter(playerFile)) {
                                    writeFile.WriteLine(player.level.name);
                                }
                                if (!player.muted) {
                                    mute.Use(p, args);
                                }
                                if (!player.frozen) {
                                    freeze.Use(p, args);
                                }
                                move.Use(p, args + " " + xjailMap);
                                while (player.Loading) {
                                }
                                if (!player.jailed) {
                                    jail.Use(p, args);
                                }
                                _s.GlobalMessage(player.color + player.name + _s.props.DefaultColor + " was XJailed!");
                                return;
                            }
                            else {
                                using (StreamReader readFile = new StreamReader(playerFile)) {
                                    string playerMap = readFile.ReadLine();
                                    readFile.Close();
                                    File.Delete(playerFile);
                                    move.Use(p, args + " " + playerMap);
                                    while (player.Loading) {
                                    }
                                    mute.Use(p, args);
                                    jail.Use(p, args);
                                    freeze.Use(p, args);
                                    spawn.Use(player, String.Empty);
                                    _s.GlobalMessage(player.color + player.name + _s.props.DefaultColor + " was released from XJail!");
                                }
                                return;
                            }
                        }
                        else {
                            p.SendMessage("Player not found");
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /xjail.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/xjail <player?> - Mutes, freezes, and sends a player to the XJail map.");
            p.SendMessage("If the player is already jailed, they will be spawned, unfrozen and unmuted.");
            p.SendMessage("/xjail set - Sets the map to be used for xjail to the " +
                               "current map, and sets the location of the jail to your current position.");
        }
    }
}

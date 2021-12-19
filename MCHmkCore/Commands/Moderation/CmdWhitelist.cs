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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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
    public class CmdWhitelist : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"white", "list", "allow", "access", "server"});

        public override string Name {
            get {
                return "whitelist";
            }
        }
        public override string Shortcut {
            get {
                return "w";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdWhitelist(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!_s.props.useWhitelist) {
                p.SendMessage("Whitelist is not enabled.");
                return;
            }
            if (args == String.Empty) {
                Help(p);
                return;
            }
            int pos = args.IndexOf(' ');
            if (pos != -1) {
                string action = args.Substring(0, pos);
                string player = args.Substring(pos + 1);
                Uuid uuid = Uuid.FindWithFallback(_s.database, player);

                switch (action) {
                case "add":
                    if (_s.whiteList.Contains(uuid)) {
                        p.SendMessage("&f" + player + _s.props.DefaultColor + " is already on the whitelist!");
                        break;
                    }

                    _s.whiteList.Add(uuid, player);
                    _s.GlobalMessageOps(p.color + p.prefix + p.name + _s.props.DefaultColor + " added &f" + player + _s.props.DefaultColor +
                                            " to the whitelist.");
                    _s.whiteList.Save("whitelist.txt");
                    _s.logger.Log("SAVED: ranks/whitelist.txt");
                    _s.logger.Log("WHITELIST: Added " + player);
                    break;
                case "del":
                    if (!_s.whiteList.Contains(uuid)) {
                        p.SendMessage("&f" + player + _s.props.DefaultColor + " is not on the whitelist!");
                        break;
                    }
                    _s.whiteList.Remove(uuid);
                    _s.GlobalMessageOps(p.color + p.prefix + p.name + _s.props.DefaultColor + " removed &f" + player + _s.props.DefaultColor
                                            + " from the whitelist.");
                    _s.whiteList.Save("whitelist.txt");
                    _s.logger.Log("SAVED: ranks/whitelist.txt");
                    _s.logger.Log("WHITELIST: Removed " + player);
                    break;
                case "list":
                    string output = "Whitelist:&f";
                    foreach (UuidEntry entry in _s.whiteList.All()) {
                        output += " " + entry.Name + ",";
                    }
                    output = output.Substring(0, output.Length - 1);
                    p.SendMessage(output);
                    break;
                default:
                    Help(p);
                    return;
                }
            }
            else {
                if (args == "list") {
                    string output = "Whitelist:&f";
                    foreach (UuidEntry entry in _s.whiteList.All()) {
                        output += " " + entry.Name + ",";
                    }
                    output = output.Substring(0, output.Length - 1);
                    p.SendMessage(output);
                }
                else {
                    Help(p);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /whitelist.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/whitelist <add/del/list> <player?> - " +
                               "Adds or deletes whitelist entry for the specified player, " +
                               "or lists all whitelist entries.");
        }
    }
}

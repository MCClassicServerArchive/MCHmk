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
using System.Data;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdUnbanip : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"undo", "ban", "ip", "kick", "mod"});

        public override string Name {
            get {
                return "unbanip";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdUnbanip(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            if (args[0] == '@') {
                args = args.Remove(0, 1).Trim();
                if (!Player.ValidName(args)) {
                    p.SendMessage("Invalid player name.");
                    return;
                }
                Player who = _s.players.Find(args);
                if (who == null) {
                    DataTable ip;
                    int tryCounter = 0;
                    rerun: try {
                        // FIXME: PreparedStatement
                        string query = "SELECT IP FROM Players WHERE Name = '" + args + "'";
                        ip = _s.database.ObtainData(query);
                    }
                    catch (Exception e) {
                        tryCounter++;
                        if (tryCounter < 10) {
                            goto rerun;
                        }
                        else {
                            _s.logger.ErrorLog(e);
                            p.SendMessage("There was a database error fetching the IP address.  It has been logged.");
                            return;
                        }
                    }
                    if (ip.Rows.Count > 0) {
                        args = ip.Rows[0]["IP"].ToString();
                    }
                    else {
                        p.SendMessage("Unable to find an IP address for that user.");
                        return;
                    }
                    ip.Dispose();
                }
                else {
                    args = who.ip;
                }
            }

            if (args.IndexOf('.') == -1) {
                p.SendMessage("Not a valid ip!");
                return;
            }
            if (!p.IsConsole) if (p.ip == args) {
                    p.SendMessage("You shouldn't be able to use this command...");
                    return;
                }
            if (!_s.bannedIP.Contains(args)) {
                p.SendMessage(args + " doesn't seem to be banned...");
                return;
            }
            _s.bannedIP.Remove(args);
            _s.SaveBannedIP(false);

            if (!p.IsConsole) {
                _s.IRC.Say(args.ToLower() + " was un-ip-banned by " + p.name + ".");
                _s.logger.Log("IP-UNBANNED: " + args.ToLower() + " by " + p.name + ".");
                _s.GlobalMessage(args + " was &8un-ip-banned" + _s.props.DefaultColor + " by " + p.color + p.name +
                                     _s.props.DefaultColor + ".");
            }
            else {
                _s.IRC.Say(args.ToLower() + " was un-ip-banned by console.");
                _s.logger.Log("IP-UNBANNED: " + args.ToLower() + " by console.");
                _s.GlobalMessage(args + " was &8un-ip-banned" + _s.props.DefaultColor + " by console.");
            }
        }

        /// <summary>
        /// Called when /help is used on /unbanip.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/unbanip <ip?/player?> - Unbans an IP address. "  +
                               "Also accepts a player name when you use a @ before the name.");
        }
    }
}

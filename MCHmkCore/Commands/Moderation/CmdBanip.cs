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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdBanip : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"ip", "ban", "kick", "mod", "punish"});

        public override string Name {
            get {
                return "banip";
            }
        }
        public override string Shortcut {
            get {
                return "bi";
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
        public CmdBanip(Server s) : base(s) { }

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
                    rerun:  try {
                        // FIXME: PreparedStatement
                        ip = _s.database.ObtainData("SELECT IP FROM Players WHERE Name = '" + args + "'");
                    }
                    catch (Exception e) {
                        tryCounter++;
                        if (tryCounter < 10) {
                            goto rerun;
                        }
                        else {
                            _s.logger.ErrorLog(e);
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
            else {
                Player who = _s.players.Find(args);
                if (who != null) {
                    args = who.ip;
                }
            }

            if (args.Equals("127.0.0.1")) {
                p.SendMessage("You can't ip-ban the server!");
                return;
            }
            if (args.IndexOf('.') == -1) {
                p.SendMessage("Invalid IP!");
                return;
            }
            if (args.Split('.').Length != 4) {
                p.SendMessage("Invalid IP!");
                return;
            }
            if (!p.IsConsole && p.ip == args) {
                p.SendMessage("You can't ip-ban yourself.!");
                return;
            }
            if (_s.bannedIP.Contains(args)) {
                p.SendMessage(args + " is already ip-banned.");
                return;
            }

            // Check if IP belongs to an op+
            // First get names of active ops+ with that ip
            List<string> opNamesWithThatIP = (from pl in _s.players where (pl.ip == args && pl.rank.Permission >= DefaultRankValue.Operator) select pl.name).ToList();
            // Next, add names from the database
            // FIXME: PreparedStatement
            DataTable dbnames = _s.database.ObtainData("SELECT Name FROM Players WHERE IP = '" + args + "'");

            foreach (DataRow row in dbnames.Rows) {
                opNamesWithThatIP.Add(row[0].ToString());
            }


            if (opNamesWithThatIP != null && opNamesWithThatIP.Count > 0) {
                // We have at least one op+ with a matching IP
                // Check permissions of everybody who matched that IP
                foreach (string opname in opNamesWithThatIP) {
                    // Console can ban anybody else, so skip this section
                    if (!p.IsConsole) {
                        // If one of these guys matches a player with a higher rank don't allow the ipban to proceed!
                        Uuid uuid = Uuid.FindUuid(_s.database, opname);
                        Rank grp = _s.ranks.FindPlayerRank(uuid);
                        if (grp != null) {
                            if (grp.Permission >= p.rank.Permission) {
                                p.SendMessage("You can only ipban IPs used by players with a lower rank.");
                                p.SendMessage(_s.props.DefaultColor + opname + "(" + grp.color + grp.name + _s.props.DefaultColor + ") uses that IP.");
                                _s.logger.Log(p.name + "failed to ipban " + args + " - IP is also used by: " + opname + "(" + grp.name + ")");
                                return;
                            }
                        }
                    }
                }
            }

            if (!p.IsConsole) {
                _s.IRC.Say(args.ToLower() + " was ip-banned by " + p.name + ".");
                _s.logger.Log("IP-BANNED: " + args.ToLower() + " by " + p.name + ".");
                _s.GlobalMessage(args + " was &8ip-banned" + _s.props.DefaultColor + " by " + p.color + p.name + _s.props.DefaultColor + ".");
            }
            else {
                _s.IRC.Say(args.ToLower() + " was ip-banned by console.");
                _s.logger.Log("IP-BANNED: " + args.ToLower() + " by console.");
                _s.GlobalMessage(args + " was &8ip-banned" + _s.props.DefaultColor + " by console.");
            }
            _s.bannedIP.Add(args);
            _s.SaveBannedIP(true);
        }

        /// <summary>
        /// Called when /help is used on /banip.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/banip <ip?/player?> - Bans an IP address. Also accepts " +
                               "a player name when you use a @ before the name.");
        }
    }
}

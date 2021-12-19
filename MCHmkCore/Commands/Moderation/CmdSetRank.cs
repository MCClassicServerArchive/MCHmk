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
    public class CmdSetRank : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "set", "user", "player"});

        public override string Name {
            get {
                return "setrank";
            }
        }
        public override string Shortcut {
            get {
                return "rank";
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
        public CmdSetRank(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            var split = args.Split(' ');
            if (split.Length < 2) {
                Help(p);
                return;
            }
            Player who = _s.players.Find(split[0]);
            Rank newRank = _s.ranks.Find(split[1]);
            string msgGave = String.Empty;
            string oldcolor = String.Empty;
            string oldgroupstr = String.Empty;

            if (who != null) {
                oldgroupstr = who.rank.name;
                oldcolor = who.rank.color;
            }
            else {
                Uuid uuid = Uuid.FindWithFallback(_s.database, split[0]);
                Rank hey = _s.ranks.FindPlayerRank(uuid);
                oldgroupstr = hey.name;
            }
            if (args.Split(' ').Length > 2) {
                msgGave = args.Substring(args.IndexOf(' ', args.IndexOf(' ') + 1));
            }
            else {
                msgGave = "Congratulations!";
            }

            if (newRank == null) {
                p.SendMessage("Could not find specified rank.");
                return;
            }

            Rank bannedGroup = _s.ranks.FindPerm(DefaultRankValue.Banned);
            if (who == null) {
                string foundName = split[0];
                Uuid uuid = Uuid.FindWithFallback(_s.database, foundName);
                Rank curRank = _s.ranks.FindPlayerRank(uuid);

                if (curRank == bannedGroup || newRank == bannedGroup) {
                    p.SendMessage("Cannot change the rank to or from \"" + bannedGroup.name + "\".");
                    return;
                }

                if (!p.IsConsole) {
                    if (curRank.Permission >= p.rank.Permission || newRank.Permission >= p.rank.Permission) {
                        p.SendMessage("Cannot change the rank of someone equal or higher than you");
                        return;
                    }
                }

                curRank.playerList.Remove(uuid);
                curRank.playerList.Save();
                _s.logger.Log("SAVED: ranks/" + curRank.fileName);

                newRank.playerList.Add(uuid, foundName);
                newRank.playerList.Save();
                _s.logger.Log("SAVED: ranks/" + newRank.fileName);

                _s.GlobalMessage(foundName + " &f(offline)" + _s.props.DefaultColor + "'s rank was set to " + newRank.color +
                                     newRank.name);
            }
            else if (who == p) {
                p.SendMessage("Cannot change your own rank.");
                return;
            }
            else {
                if (!p.IsConsole) {
                    if (who.rank == bannedGroup || newRank == bannedGroup) {
                        p.SendMessage("Cannot change the rank to or from \"" + bannedGroup.name + "\".");
                        return;
                    }

                    if (who.rank.Permission >= p.rank.Permission || newRank.Permission >= p.rank.Permission) {
                        p.SendMessage("Cannot change the rank of someone equal or higher to yourself.");
                        return;
                    }
                }
                who.rank.playerList.Remove(who.uuid);
                who.rank.playerList.Save();
                _s.logger.Log("SAVED: ranks/" + who.rank.fileName);

                newRank.playerList.Add(who.uuid, who.name);
                newRank.playerList.Save();
                _s.logger.Log("SAVED: ranks/" + newRank.fileName);

                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + "'s rank was set to " + newRank.color +
                                  newRank.name, false);
                who.SendMessage(MessageType.Chat, "&6" + msgGave);

                who.rank = newRank;
                if(who.color == String.Empty || who.color == oldcolor ) {
                    who.color = who.rank.color;
                }
                who.SetPrefix();

                _s.GlobalDie(who, false);

                who.SendMessage("You are now ranked " + newRank.color + newRank.name + _s.props.DefaultColor +
                                ", type /help for your new set of commands.");
                who.SendUserType(_s.blockPerms.CanPlace(who.rank.Permission, BlockId.Bedrock));

                string year = DateTime.Now.Year.ToString();
                string month = DateTime.Now.Month.ToString();
                string day = DateTime.Now.Day.ToString();
                string hour = DateTime.Now.Hour.ToString();
                string minute = DateTime.Now.Minute.ToString();
                string assigner;
                if (p.IsConsole) {
                    assigner = "Console";
                }
                else {
                    assigner = p.name;
                }
                string allrankinfos = String.Empty;
                foreach (string line in File.ReadAllLines("text/rankinfo.txt")) {
                    allrankinfos = allrankinfos + line + "\r\n";
                }
                File.WriteAllText("text/rankinfo.txt", allrankinfos);
                try {
                    StreamWriter sw;
                    sw = File.AppendText("text/rankinfo.txt");
                    sw.WriteLine(who.name + " " + assigner + " " + minute + " " + hour + " " + day + " " + month + " " + year + " " +
                                 split[1] + " " + oldgroupstr);
                    sw.Close();
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                    p.SendMessage("&cAn error occurred!");
                }

                _s.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
            }
        }

        /// <summary>
        /// Called when /help is used on /setrank.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/setrank <player?> <rank?> [message?] - " +
                          "Sets a player's rank to the specified rank, accompanied with an optional " +
                          "celebratory message.");
            p.SendMessage("Valid ranks are: " + _s.ranks.ConcatNames(_s.props.DefaultColor, true));
        }
    }
}

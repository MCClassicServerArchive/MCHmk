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

namespace MCHmk.Commands {
    public class CmdAwards : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"trophy", "add", "del", "price"});

        public override string Name {
            get {
                return "awards";
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
                return DefaultRankValue.Banned;
            }
        }

        public CmdAwards(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length > 2) {
                Help(p);
                return;
            }
            // /awards
            // /awards 1
            // /awards bob
            // /awards bob 1

            int totalCount = 0;
            string foundPlayer = String.Empty;

            if (args != String.Empty) {
                if (args.Split(' ').Length == 2) {
                    foundPlayer = args.Split(' ')[0];
                    Player who = _s.players.Find(foundPlayer);
                    if (who != null) {
                        foundPlayer = who.name;
                    }
                    try {
                        totalCount = int.Parse(args.Split(' ')[1]);
                    }
                    catch {  // TODO: Find exact exception to catch
                        Help(p);
                        return;
                    }
                }
                else {
                    if (args.Length <= 3) {
                        try {
                            totalCount = int.Parse(args);
                        }
                        catch {  // TODO: find exact exception to catch
                            foundPlayer = args;
                            Player who = _s.players.Find(foundPlayer);
                            if (who != null) {
                                foundPlayer = who.name;
                            }
                        }
                    }
                    else {
                        foundPlayer = args;
                        Player who = _s.players.Find(foundPlayer);
                        if (who != null) {
                            foundPlayer = who.name;
                        }
                    }
                }
            }

            if (totalCount < 0) {
                p.SendMessage("Cannot display pages less than 0");
                return;
            }

            List<Awards.awardData> awardList = new List<Awards.awardData>();
            Uuid uuid = Uuid.Empty;
            if (foundPlayer == String.Empty) {
                awardList = Awards.allAwards;
            }
            else {
                uuid = Uuid.FindUuid(_s.database, foundPlayer);
                if (!uuid.IsValid) {
                    p.SendMessage("That player is invalid or has never joined the server.");
                    return;
                }

                foreach (string s in Awards.GetAwards(uuid)) {
                    Awards.awardData aD = new Awards.awardData();
                    aD.awardName = s;
                    aD.description = Awards.getDescription(s);
                    awardList.Add(aD);
                }
            }

            if (awardList.Count == 0) {
                if (foundPlayer != String.Empty) {
                    p.SendMessage("The player has no awards!");
                }
                else {
                    p.SendMessage("There are no awards in this server yet");
                }

                return;
            }

            int max = totalCount * 5;
            int start = (totalCount - 1) * 5;
            if (start > awardList.Count) {
                p.SendMessage("There aren't that many awards. Enter a smaller number");
                return;
            }
            if (max > awardList.Count) {
                max = awardList.Count;
            }

            if (foundPlayer != String.Empty) {
                p.SendMessage(_s.ranks.FindPlayerColor(uuid) + foundPlayer + _s.props.DefaultColor +
                                   " has the following awards:");
            }
            else {
                p.SendMessage("Awards available: ");
            }

            if (totalCount == 0) {
                foreach (Awards.awardData aD in awardList) {
                    p.SendMessage("&6" + aD.awardName + ": &7" + aD.description);
                }

                if (awardList.Count > 8) {
                    p.SendMessage("&5Use &b/awards " + args + " 1/2/3/... &5for a more ordered list");
                }
            }
            else {
                for (int i = start; i < max; i++) {
                    Awards.awardData aD = awardList[i];
                    p.SendMessage("&6" + aD.awardName + ": &7" + aD.description);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /awards.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/awards [player?] - Gives a full list of awards.");
            p.SendMessage("If a player is specified, that player's awards are shown.");
            p.SendMessage("Use /awards [1/2/3/...] to view a particular page of the list.");
        }
    }
}

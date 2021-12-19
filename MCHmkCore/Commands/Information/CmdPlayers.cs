/*
    Copyright 2012 Jjp137/LegoBricker

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
	Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)

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
using System.Text;

namespace MCHmk.Commands {
    class CmdPlayers : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"player", "list", "info"});

        public override string Name {
            get {
                return "players";
            }
        }
        public override string Shortcut {
            get {
                return "who";
            }
        }
        public override string Type {
            get {
                return "information";
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
        public CmdPlayers(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            /* These declarations count the amount
             * of players on the server and are used
             * in the grammar friendly section below.*/
            string players = "players";
            string verb = "are";
            int playerCount = 0;
            int rankCounter = 0;
            foreach (Player ply in _s.players) {
                if (!ply.hidden || p.IsConsole) {
                    playerCount++;
                }
                if (ply.hidden && !p.IsConsole && (ply.rank.Permission <= p.rank.Permission)) {
                    playerCount++;
                }
            }
            if (playerCount == 0) {
                p.SendMessage( "There are 0 players online.");
                return;
            }
            /* This block makes the "players online"
             * statement grammar correct for the amount
             * of players online at the time.*/
            if (playerCount == 1) {
                players = "player";
                verb = "is";
            }
            /* The following 2 blocks print the players
             * online using the printPlayers method below.
             * If no message is provided, it prints all
             * the players. Otherwise it prints the players
             * in the given rank. The reverse method
             * reverses the list, b/c thats how Jjp wanted it.*/
            if (args == String.Empty) {
                p.SendMessage("There " + verb + " " + playerCount + " "+ players + " " + _s.props.DefaultColor + "online.");
                for (int i = _s.ranks.Count - 1; i >= 0; i--) {
                    Rank grp = _s.ranks[i];
                    PrintPlayers (grp,p);
                }
            }
            /* This else method is what happens if you enter
             * a rank. First it tries to find it, and then
             * it checks to see if the name it returned exists,
             * because sometimes people use ranks that dont exist.
             * If it does exist, it prints the players of that rank,
             * other wise it prints all players online.
             * The embedded else is for a found rank. It counts
             * the players in that rank, displays how many players of that
             * rank are online, and then shows the players online of that rank.
             */
            else {
                Rank grp = Find (args.Split(' ')[0]);
                if (grp == null) {
                    p.SendMessage("That rank does not exist.");
                }
                else {
                    foreach (Player ply in _s.players) {
                        if (ply.rank.Permission == grp.Permission) {
                            rankCounter++;
                        }
                    }
                    if (rankCounter == 1) {
                        verb = "is";
                    }
                    string groupNameToPrint;

                    groupNameToPrint = grp.name;
                    if (rankCounter > 1 || rankCounter == 0) {
                        string ending = grp.name.Substring(grp.name.Length-2);
                        if (ending != "ed") {
                            groupNameToPrint += "s";
                        }
                    }
                    p.SendMessage("There " + verb + " " + rankCounter  + " "+ groupNameToPrint + " " + _s.props.DefaultColor + "online.");
                    PrintPlayers (grp,p);
                }
            }
        }

        /* This is the method that actually does the work.
         * It prints the players in thier respective row
         * if their group permission is equal to that
         * of the group provided in the method. Thus
         * it can print all players with foreach or
         * just those in one rank by providing a rank.
         * The tester string is used to check whether
         * or not to display the rank if Show Empty Ranks is false.*/
        public void PrintPlayers(Rank grp, Player p) {
            string groupName;
            string ending = grp.name.Substring(grp.name.Length-2);
            if (ending == "ed") {
                groupName = char.ToUpper(grp.name[0]) + grp.name.Substring(1);
            }
            else {
                groupName = char.ToUpper(grp.name[0]) + grp.name.Substring(1) + "s";
            }
            string printer = ":" + grp.color + groupName + ": ";
            int playersInRank = 0;

            bool anyNobodies = false;

            /* This second foreach is here to check if
             * it should display the nobody rank in the
             * players list. It doesn't by default, unless
             * someone of nobody rank is on the server. */
            if (grp.Permission == DefaultRankValue.Nobody) {
                foreach (Player ply in _s.players) {
                    if (ply.rank.Permission == DefaultRankValue.Nobody) {
                        anyNobodies = true;
                        break;
                    }
                }
                // If it turns out that there are no nobodies online...
                if (!anyNobodies) {
                    return;
                }
            }

            foreach (Player ply in _s.players) {
                string nameToPrint = ply.name;
                // This is getting messy...if you want to make this neater, LegoBricker,
                // be my guest :p -Jjp137
                if ((p.IsConsole && ply.rank.Permission == grp.Permission) ||
                    (ply.rank.Permission == grp.Permission && !ply.hidden) || 
                    (!p.IsConsole && ply.hidden && ply.rank.Permission <= p.rank.Permission && 
                     ply.rank.Permission == grp.Permission)) {
                    playersInRank++;
                    if (ply.voice) {
                        nameToPrint = "&f+" + grp.color + nameToPrint;
                    }
                    if (_s.afkset.Contains(ply.name)) {
                        nameToPrint += "-afk";
                    }
                    if (ply.hidden) {
                        nameToPrint += "-hidden";
                    }
                    nameToPrint += " (" + ply.level.name + ")";
                    if (playersInRank == 1) {
                        printer += grp.color + nameToPrint;
                    }
                    else {
                        printer += grp.color + ", " + nameToPrint;
                    }
                }
            }
            if (_s.props.showEmptyRanks == false && playersInRank == 0) {
                return;
            }
            p.SendMessage(printer);
        }

        /* This the find method used above.
         * It matches the name against all groups
         * in the group list called finder. It is essentially
         * a transcription of the Player Find method in
         * Player.cs.*/

        // Isn't there like a one or two line way of doing this in the C# API?
        // Certainly not 10+ lines... -Jjp137
        private Rank Find(string name) {
            Rank tempGroup = null;
            bool returnNull = false;

            foreach (Rank g in _s.ranks) {
                if (g.name.ToLower() == name.ToLower()) {
                    return g;
                }
                if (g.name.ToLower().IndexOf(name.ToLower()) != -1) {
                    if (tempGroup == null) {
                        tempGroup = g;
                    }
                    else {
                        returnNull = true;
                    }
                }
            }
            if (returnNull == true) {
                return null;
            }
            if (tempGroup != null) {
                return tempGroup;
            }
            return null;
        }

        /// <summary>
        /// Called when /help is used on /players.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/players [rank?] - Shows a list of players online.");
            p.SendMessage("If a rank is provided, shows only players that have that rank.");
            p.SendMessage("Usable Ranks:");
            p.SendMessage(_s.ranks.ConcatNames(_s.props.DefaultColor, true));
        }
    }
}

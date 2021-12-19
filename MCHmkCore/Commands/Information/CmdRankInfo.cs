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
    /// The code for the /rankinfo command, which displays information about the rank history
    /// of a particular player.
    /// </summary>
    class CmdRankInfo : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rank", "info", "display"});

        public override string Name {
            get {
                return "rankinfo";
            }
        }
        public override string Shortcut {
            get {
                return "ri";
            }
        }
        public override string Type {
            get {
                return "info";
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
        public CmdRankInfo(Server s) : base(s) { }

        /// <summary>
        /// The code that runs when /rankinfo is called.
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        /// </summary>
        public override void Use(Player p, string args) {
            // Check if the player actually entered something valid.
            if (args == String.Empty) {
                Help(p);
                return;
            }
            else if (!Player.ValidName(args)) {
                p.SendMessage("Not a valid player name.");
                return;
            }

            // At this point, we know the player name is a valid one.
            args = args.Trim(); // That stupid extra space...

            // If the player in question is online, partial names are enough.
            // Otherwise, the name needs to be exact.
            Player p2 = _s.players.Find(args);
            if (p2 != null) {
                args = p2.name;
            }

            bool wasRanked = false;
            try {
                // Look for matching entries in rankinfo.txt.
                foreach (string entry in File.ReadAllLines("text/rankinfo.txt")) {
                    string[] data = entry.Split(' ');

                    // If we find a line that involves the specified player, put everything
                    // together and display it.
                    if (data[0].ToLower().Equals(args.ToLower())) {

                        string newrank = data[7];
                        string oldrank = data[8];
                        string assigner = data[1];
                        Rank newrankcolor = _s.ranks.Find(newrank);
                        Rank oldrankcolor = _s.ranks.Find(oldrank);

                        int minutes = Convert.ToInt32(data[2]);
                        int hours = Convert.ToInt32(data[3]);
                        int days = Convert.ToInt32(data[4]);
                        int months = Convert.ToInt32(data[5]);
                        int years = Convert.ToInt32(data[6]);
                        DateTime ExpireDate = new DateTime(years, months, days, hours, minutes, 0);

                        if (!wasRanked) {
                            p.SendMessage("&1Rank Information of " + args);
                            wasRanked = true;
                        }
                        p.SendMessage("&aNew rank: " + newrankcolor.color + newrank);
                        p.SendMessage("&aOld Rank: " + oldrankcolor.color + oldrank);
                        p.SendMessage("&aDate of assignment: " + ExpireDate.ToString());
                        p.SendMessage("&aRanked by: " + assigner);
                        p.SendMessage("&f----------------------------");
                    }
                }
            }
            // When things go wrong...
            catch (IOException) {
                _s.logger.Log("Warning: Please check if rankinfo.txt is not corrupted or read-only!");
                return;
            }
            catch (UnauthorizedAccessException) {
                _s.logger.Log("Warning: Please check if rankinfo.txt is not corrupted or read-only!");
                return;
            }
            catch (FormatException) {
                _s.logger.Log("Warning: rankinfo.txt has invalid entries!");
                return;
            }
            catch (OverflowException) {
                _s.logger.Log("Warning: rankinfo.txt has invalid entries!");
                return;
            }

            // If applicable, let the user of the command know that a player wasn't ranked.
            if (!wasRanked) {
                p.SendMessage("The player &a" + args + _s.props.DefaultColor + " was never ranked.");
            }
        }

        /// <summary>
        /// Called when /help is used on /rankinfo.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/rankinfo <player?> - Displays info about the rank history of a player.");
        }
    }
}

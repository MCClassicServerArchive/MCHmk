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

namespace MCHmk.Commands {
    public class CmdSearch : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"find", "block", "command", "player", "rank"});

        public override string Name {
            get {
                return "search";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdSearch(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length < 2) {
                Help(p);
                return;
            }
            string type = args.Split(' ')[0];
            string keyword = args.Remove(0, (type.Length + 1));

            if (type.ToLower().Contains("command") || type.ToLower().Contains("cmd")) {
                bool mode = true;
                string[] keywords = keyword.Split(' ');
                List<string> found = FindCommand(keywords);
                if (found.Count == 0) {
                    p.SendMessage("No commands found matching keyword(s): '" + args.Remove(0, (type.Length + 1)) + "'");
                }
                else {
                    p.SendMessage("&bfound: ");
                    foreach (string s in found) {
                        if (mode) {
                            p.SendMessage("&2/" + s);
                        }
                        else {
                            p.SendMessage("&9/" + s);
                        }
                        mode = (mode) ? false : true;
                    }
                }
            }
            else if (type.ToLower().Contains("block")) {
                string blocks = String.Empty;
                bool mode = true;
                for (ushort i = 0; i < BlockData.maxblocks; i++) {
                    BlockId b = (BlockId)i;
                    if (BlockData.Name(b).ToLower() != "unknown") {
                        if (BlockData.Name(b).Contains(keyword)) {
                            if (mode) {
                                blocks += _s.props.DefaultColor + ", &9" + BlockData.Name(b);
                            }
                            else {
                                blocks += _s.props.DefaultColor + ", &2" + BlockData.Name(b);
                            }
                            mode = (mode) ? false : true;
                        }
                    }
                }
                if (blocks == String.Empty) {
                    p.SendMessage("No blocks found containing &b" + keyword);
                }
                p.SendMessage(blocks.Remove(0, 2));
            }
            else if (type.ToLower().Contains("rank")) {
                string ranks = String.Empty;
                foreach (Rank g in _s.ranks) {
                    if (g.name.Contains(keyword)) {
                        ranks += g.color + g.name + "'";
                    }
                }
                if (ranks == String.Empty) {
                    p.SendMessage("No ranks found containing &b" + keyword);
                }
                else {
                    foreach (string r in ranks.Split('\'')) {
                        p.SendMessage(r);
                    }
                }
            }
            else if (type.ToLower().Contains("player")) {
                string players = String.Empty;
                foreach (Player who in _s.players) {
                    if (who.name.ToLower().Contains(keyword.ToLower())) {
                        players += ", " + who.color + who.name;
                    }
                }
                if (players == String.Empty) {
                    p.SendMessage("No usernames found containing &b" + keyword);
                }
                else {
                    p.SendMessage(players.Remove(0, 2));
                }
            }
            else {
                p.SendMessage("Invalid category given.");
            }
        }

        /// <summary>
        /// Finds any commands that contain all of the requested keywords.
        /// </summary>
        /// <param name="query"> The keywords provided by the user. </param>
        /// <returns> A list of command names representing the commands that contained all of the requested keywords. 
        /// </returns>
        private List<string> FindCommand(string[] query) {
            List<string> result = new List<string>();

            if (query.Length == 0) {
                return result;  // It will be empty at this point.
            }

            // Go through each command's keywords.
            foreach (Command cmd in _s.commands) {
                ReadOnlyCollection<string> keywords = cmd.Keywords;
                bool includeCmd = false;

                // Check that the command contains all, not just some, of the provided keywords.
                foreach (string term in query) {
                    string lowerTerm = term.ToLower();

                    // For each keyword, set this to false since we want all of them to be present in a command.
                    includeCmd = false;

                    // First, check the name of the command, its category, and its shortcut if it is longer than 
                    // three characters. These were considered as potential keywords in the original code.
                    if (lowerTerm.Contains(cmd.Name.ToLower()) || lowerTerm.Contains(cmd.Type.ToLower()) ||
                        (cmd.Shortcut.Length > 3 && lowerTerm.Contains(cmd.Shortcut.ToLower()))) {
                        includeCmd = true;
                        continue;  // Move on to the next term.
                    }

                    // Check the keywords themselves if the above does not yield matches.
                    foreach (string word in keywords) {
                        if (lowerTerm.Contains(word.ToLower())) {
                            includeCmd = true;
                            break;
                        }
                    }

                    // Skip the rest of the given terms if any of them aren't found.
                    if (!includeCmd) {
                        break;  
                    }
                }

                // Add the command's name to the list if it does contain every requested keyword.
                if (includeCmd) {
                    result.Add(cmd.Name);
                }
            }

            // And we're done.
            return result;
        }

        /// <summary>
        /// Called when /help is used on /search.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/search <category?> <keyword?> - Finds items within a category that match the keyword.");
            p.SendMessage("List of categories: ");
            p.SendMessage("commands - Finds commands with the specified keyword.");
            p.SendMessage("blocks - Finds blocks with the specified keyword.");
            p.SendMessage("ranks - Finds ranks with the specified keyword.");
            p.SendMessage("players - Finds players with the specified keyword.");
        }
    }
}

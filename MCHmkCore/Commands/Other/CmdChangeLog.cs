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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace MCHmk.Commands {
    /// <summary>
    /// The code for the /changelog command, which lets players view the changelog.
    /// </summary>
    public class CmdChangeLog : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"mchmk", "change", "log"});

        public override string Name {
            get {
                return "changelog";
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

        public CmdChangeLog(Server s) : base(s) { }

        /// <summary>
        /// The code that runs when /changelog is called.
        /// </summary>
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        public override void Use(Player p, string args) {
            // Crappy fix that will be replaced later -Jjp137
            string fileToRead;
            if (File.Exists("changelog.txt")) {
                fileToRead = "changelog.txt";
            }
            else if (File.Exists("Changelog.txt")) {
                fileToRead = "Changelog.txt";
            }
            else {
                p.SendMessage("Unable to find changelog.");
                return;
            }

            // Read the changelog but stop reading if it encounters a blank line
            // This is done so that a player will only see the latest changes even if multiple version info exists in the changelog
            // Because of this, its really important that blank lines are ONLY used to separate different versions
            string[] strArray = File.ReadAllLines(fileToRead).TakeWhile(s => !String.IsNullOrEmpty(s.Trim())).ToArray();
            if (args == String.Empty) {
                for (int j = 0; j < strArray.Length; j++) {
                    p.SendMessage(strArray[j]);
                }
            }
            else {
                string[] split = args.Split(' ');
                if (split.Length != 1) {
                    Help(p);
                    return;
                }

                // Send the changelog to all players if the player using the command is of high
                // enough rank.
                if (split[0] == "all") {
                    if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                        p.SendMessage("You must be at least " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name + " to send the changelog to all players.");
                        return;
                    }
                    for (int k = 0; k < strArray.Length; k++) {
                        _s.GlobalMessage(strArray[k]);
                    }

                    return;
                }
                // Send the changelog to a particular player.
                else {
                    Player player = _s.players.Find(split[0]);

                    if (player == null) {
                        p.SendMessage("Could not find player \"" + split[0] + "\"!");
                        return;
                    }

                    player.SendMessage("Changelog:");
                    for (int l = 0; l < strArray.Length; l++) {
                        player.SendMessage(strArray[l]);
                    }
                    p.SendMessage("The Changelog was successfully sent to " + player.name + ".");

                    return;
                }
            }
        }
        
        /// <summary>
        /// Called when /help is used on /changelog.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Update once extra permissions have been rewritten -Jjp137
            p.SendMessage("/changelog [player?/all]- Displays the most recent changelog.");
            p.SendMessage("player - Sends the most recent changelog to <player>.");
            p.SendMessage("all - Sends the most recent changelog to everyone.");
        }
    }
}

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
    public class CmdNews : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"info", "latest"});

        public override string Name {
            get {
                return "news";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        public CmdNews(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            string newsFile = "text/news.txt";
            if (!File.Exists(newsFile) || (File.Exists(newsFile) && File.ReadAllLines(newsFile).Length == -1)) {
                using (var SW = new StreamWriter(newsFile)) {
                    SW.WriteLine("News have not been created. Put News in '" + newsFile + "'.");
                }
                return;
            }
            string[] strArray = File.ReadAllLines(newsFile);
            if (args == String.Empty) {
                foreach (string t in strArray) {
                    p.SendMessage(t);
                }
            }
            else {
                string[] split = args.Split(' ');
                if (split[0] == "all") {
                    if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                        p.SendMessage("You must be at least " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(
                                               this)).name + " to send this to all players.");
                        return;
                    }
                    for (int k = 0; k < strArray.Length; k++) {
                        _s.GlobalMessage(strArray[k]);
                    }
                    return;
                }
                Player player = _s.players.Find(split[0]);
                if (player == null) {
                    p.SendMessage("Could not find player \"" + split[0] + "\"!");
                    return;
                }
                foreach (string t in strArray) {
                    player.SendMessage(t);
                }
                p.SendMessage("The News were successfully sent to " + player.name + ".");

            }
        }

        /// <summary>
        /// Called when /help is used on /news.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // Update w/ extra permissions later -Jjp137
            p.SendMessage("/news - Shows server news.");
            p.SendMessage("/news <player?> - Sends the news to the specified player.");
            p.SendMessage("/news all - Sends the news to everyone.");
        }
    }
}

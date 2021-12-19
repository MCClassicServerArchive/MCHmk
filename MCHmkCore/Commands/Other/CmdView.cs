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
    public class CmdView : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"file", "content", "player", "user"});

        public override string Name {
            get {
                return "view";
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
        public CmdView(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!Directory.Exists("extra/text/")) {
                Directory.CreateDirectory("extra/text");
            }
            if (args == String.Empty) {
                DirectoryInfo di = new DirectoryInfo("extra/text/");
                string allFiles = String.Empty;
                foreach (FileInfo fi in di.GetFiles("*.txt")) {
                    try {
                        string firstLine = File.ReadAllLines("extra/text/" + fi.Name.Substring(0,
                                                             fi.Name.Length - fi.Extension.Length) + ".txt")[0];
                        if (firstLine[0] == '#') {
                            if (_s.ranks.Find(firstLine.Substring(1)).Permission <= p.rank.Permission) {
                                allFiles += ", " + fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                            }
                        }
                        else {
                            allFiles += ", " + fi.Name;
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        p.SendMessage("Error");
                    }
                }

                if (allFiles == String.Empty) {
                    p.SendMessage("No files are viewable by you");
                }
                else {
                    p.SendMessage("Available files:");
                    p.SendMessage(allFiles.Remove(0, 2));
                }
            }
            else {
                Player who = null;
                if (args.IndexOf(' ') != -1) {
                    who = _s.players.Find(args.Split(' ')[args.Split(' ').Length - 1]);
                    if (who != null) {
                        args = args.Substring(0, args.LastIndexOf(' '));
                    }
                }
                if (who == null) {
                    who = p;
                }

                string textdir = Path.Combine("extra", "text");
                string filename = args + ".txt";

                // Make sure that directory traversal isn't being attempted.
                if (!FileUtil.BasePathCheck(textdir, filename)) {
                    p.SendMessage("File specified doesn't exist");
                    return;
                }

                string textPath = Path.Combine(textdir, filename);

                if (File.Exists(textPath)) {
                    try {
                        string[] allLines = File.ReadAllLines(textPath);
                        if (allLines[0][0] == '#') {
                            if (_s.ranks.Find(allLines[0].Substring(1)).Permission <= p.rank.Permission) {
                                for (int i = 1; i < allLines.Length; i++) {
                                    who.SendMessage(allLines[i]);
                                }
                            }
                            else {
                                p.SendMessage("You cannot view this file");
                            }
                        }
                        else {
                            for (int i = 1; i < allLines.Length; i++) {
                                who.SendMessage(allLines[i]);
                            }
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        p.SendMessage("An error occurred when retrieving the file");
                    }
                }
                else {
                    p.SendMessage("File specified doesn't exist");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /view.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/view - Lists all the files you can view.");
            p.SendMessage("/view <filename?> [player?] - Views a file's contents.");
            p.SendMessage("If a player is specified, that player is shown the file.");
        }
    }
}

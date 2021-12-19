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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MCHmk.Commands {
    public class CmdText : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"write", "read", "view", "able"});

        public override string Name {
            get {
                return "text";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdText(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // Create the directory if it doesn't exist
            string extraTextDir = Path.Combine("extra", "text");

            if (!Directory.Exists(extraTextDir)) {
                Directory.CreateDirectory(extraTextDir);
            }

            // Show the help if the message doesn't contain enough parameters
            if (args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }

            string[] param = args.Split(' ');

            try {
                if (param[0].ToLower() == "delete") {
                    string filename = SanitizeFileName(param[1]) + ".txt";
                    if (File.Exists(extraTextDir + filename)) {
                        File.Delete(extraTextDir + filename);
                        p.SendMessage("Deleted file: " + filename);
                        return;
                    }
                    else {
                        p.SendMessage("Could not find file: " + filename);
                        return;
                    }
                }
                else {
                    bool again = false;
                    string filename = SanitizeFileName(param[0]) + ".txt";
                    string path = extraTextDir + filename;

                    //p.SendMessage("Path is: " + path);

                    // See if we match the group
                    string group = _s.ranks.FindPerm(DefaultRankValue.Guest).name;
                    if (_s.ranks.Find(param[1]) != null) {
                        group = _s.ranks.Find(param[1]).name;
                        again = true;
                    }

                    args = args.Substring(args.IndexOf(' ') + 1);
                    if (again) {
                        args = args.Substring(args.IndexOf(' ') + 1);
                    }

                    string contents = args;
                    if (contents == String.Empty) {
                        Help(p);
                        return;
                    }

                    if (!File.Exists(path)) {
                        contents = "#" + group + System.Environment.NewLine + contents;
                    }
                    else {
                        contents = " " + contents;
                    }

                    File.AppendAllText(path, contents);
                    p.SendMessage("Added text to: " + filename);
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                Help(p);
            }
        }

        private string SanitizeFileName(string filename) {
            return Regex.Replace(filename, @"[^\d\w\-]", String.Empty);
        }

        /// <summary>
        /// Called when /help is used on /text.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/text <filename?> [rank?] <message?> - " +
                          "Makes a /view-able text file with the given filename.");
            p.SendMessage("The given rank is the minimum rank needed to view the file.");
            p.SendMessage("The given message is entered into the text file.");
            p.SendMessage("If the file already exists, text will be appended to the file.");
        }

    }
}

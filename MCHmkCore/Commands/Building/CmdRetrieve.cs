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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdRetrieve : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"copy", "paste", "file", "get", "block"});

        public override string Name {
            get {
                return "retrieve";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "build";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public List<CopyOwner> list = new List<CopyOwner>();
        public CmdRetrieve(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                if (!File.Exists("extra/copy/index.copydb")) {
                    File.Create("extra/copy/index.copydb").Dispose();
                }
                if (args == String.Empty) {
                    Help(p);
                    return;
                }
                if (args.Split(' ')[0] == "info") {
                    if (args.IndexOf(' ') != -1) {
                        args = args.Split(' ')[1];
                        if (File.Exists("extra/copy/" + args + ".copy")) {
                            using (StreamReader sR = new StreamReader("extra/copy/" + args + ".copy")) {
                                string infoline = sR.ReadLine();
                                p.SendMessage(infoline);
                            }
                            return;
                        }
                    }
                    else {
                        Help(p);
                        return;
                    }
                }
                if (args.Split(' ')[0] == "find") {
                    args = args.Replace("find", String.Empty);
                    string storedcopies = String.Empty;
                    int maxCopies = 0;
                    int findnum = 0;
                    int currentnum = 0;
                    bool isint = int.TryParse(args, out findnum);
                    if (args == String.Empty) {
                        goto retrieve;
                    }
                    if (!isint) {
                        args = args.Trim();
                        list.Clear();
                        foreach (string s in File.ReadAllLines("extra/copy/index.copydb")) {
                            CopyOwner cO = new CopyOwner();
                            cO.file = s.Split(' ')[0];
                            cO.uuid = s.Split(' ')[1];
                            cO.name = s.Split(' ')[2];
                            list.Add(cO);
                        }
                        Uuid uuid = Uuid.FindUuid(_s.database, args);
                        if (!uuid.IsValid) {
                            p.SendMessage("The player name is invalid or has never joined the server.");
                            return;
                        }

                        for (int i = 0; i < list.Count; i++) {
                            if (list[i].uuid == uuid.Value) {
                                storedcopies += ", " + list[i].file;
                            }
                        }
                        if (storedcopies == String.Empty) {
                            p.SendMessage("No saves found for player: " + args);
                        }
                        else {
                            p.SendMessage("Saved copy files: ");
                            p.SendMessage("&f " + storedcopies.Remove(0, 2));
                        }
                        return;
                    }

                    // SEARCH BASED ON NAME STUFF ABOVE HERE
                    if (isint) {
                        maxCopies = findnum * 50;
                        currentnum = maxCopies - 50;
                    }
                    retrieve:   DirectoryInfo di = new DirectoryInfo("extra/copy/");
                    FileInfo[] fi = di.GetFiles("*.copy");

                    if (maxCopies == 0) {
                        foreach (FileInfo file in fi) {
                            storedcopies += ", " + file.Name.Replace(".copy", String.Empty);
                        }
                        if (storedcopies != String.Empty) {
                            p.SendMessage("Saved copy files: ");
                            p.SendMessage("&f " + storedcopies.Remove(0, 2));
                            if (fi.Length > 50) {
                                p.SendMessage("For a more structured list, use /retrieve find <1/2/3/...>");
                            }
                        }
                        else {
                            p.SendMessage("There are no saved copies.");
                        }
                    }
                    else {
                        if (maxCopies > fi.Length) {
                            maxCopies = fi.Length;
                        }
                        if (currentnum > fi.Length) {
                            p.SendMessage("No saved copies beyond number " + fi.Length);
                            return;
                        }

                        p.SendMessage("Saved copies (" + currentnum + " to " + maxCopies + "):");
                        for (int i = currentnum; i < maxCopies; i++) {
                            storedcopies += ", " + fi[i].Name.Replace(".copy", String.Empty);
                        }
                        if (storedcopies != String.Empty) {
                            p.SendMessage("&f" + storedcopies.Remove(0, 2));
                        }
                        else {
                            p.SendMessage("There are no saved copies.");
                        }
                    }
                }
                else {
                    if (args.IndexOf(' ') == -1) {
                        args = args.Split(' ')[0];
                        if (File.Exists("extra/copy/" + args + ".copy")) {
                            p.CopyBuffer.Clear();
                            bool readFirst = false;
                            foreach (string s in File.ReadAllLines("extra/copy/" + args + ".copy")) {
                                if (readFirst) {
                                    Player.CopyPos cP;
                                    cP.x = Convert.ToUInt16(s.Split(' ')[0]);
                                    cP.y = Convert.ToUInt16(s.Split(' ')[1]);
                                    cP.z = Convert.ToUInt16(s.Split(' ')[2]);
                                    cP.type = (BlockId)Convert.ToByte(s.Split(' ')[3]);
                                    p.CopyBuffer.Add(cP);
                                }
                                else {
                                    readFirst = true;
                                }
                            }
                            p.SendMessage("&f" + args + _s.props.DefaultColor + " has been placed copybuffer.  Paste away!");
                        }
                        else {
                            p.SendMessage("Could not find copy specified");
                            return;
                        }
                    }
                    else {
                        Help(p);
                        return;
                    }
                }
            }
            catch (Exception e) {
                p.SendMessage("An error occured");
                _s.logger.ErrorLog(e);
            }
        }

        public class CopyOwner {
            public string uuid;
            public string name;
            public string file;
        }

        /// <summary>
        /// Called when /help is used on /retrieve.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/retrieve <filename?> - Retrieves a saved copy file. Use /paste to place it.");
            p.SendMessage("/retrieve info <filename?> - Gets information about the saved file.");
            p.SendMessage("/retrieve find - Prints a list of all saved copies.");
            p.SendMessage("/retrieve find <1/2/3/...> - Shows a page of the list.");
            p.SendMessage("/retrieve find <player?> - Prints a list of all saved copies made by the specified player.");
        }
    }
}



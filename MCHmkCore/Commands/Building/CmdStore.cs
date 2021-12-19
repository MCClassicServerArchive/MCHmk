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
using System.Text.RegularExpressions;

namespace MCHmk.Commands {
    public class CmdStore : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"selection", "save"});

        public override string Name {
            get {
                return "store";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.AdvBuilder;
            }
        }
        public List<CopyOwner> list = new List<CopyOwner>();
        public CmdStore(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                if (args == String.Empty) {
                    Help(p);
                    return;
                }

                if (args.IndexOf(' ') == -1) {
                    args = SanitizeFileName(args);

                    if (File.Exists("extra/copy/" + args + ".copy")) {
                        p.SendMessage("File: &f" + args + _s.props.DefaultColor + " already exists.  Delete first");
                        return;
                    }
                    else {
                        p.SendMessage("Storing: " + args);
                        File.Create("extra/copy/" + args + ".copy").Dispose();
                        using (StreamWriter sW = File.CreateText("extra/copy/" + args + ".copy")) {
                            sW.WriteLine("Saved by: " + p.name + " at " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss "));
                            for (int k = 0; k < p.CopyBuffer.Count; k++) {
                                sW.WriteLine(p.CopyBuffer[k].x.ToString() + " " + p.CopyBuffer[k].y.ToString() + " " +
                                             p.CopyBuffer[k].z.ToString() + " " + ((ushort)p.CopyBuffer[k].type).ToString());
                            }
                        }
                        using (StreamWriter sW = File.AppendText("extra/copy/index.copydb")) {
                            sW.WriteLine(args + " " + p.uuid + " " + p.name);
                        }
                    }
                }
                else {
                    if (args.Split(' ')[0] == "delete") {
                        args = SanitizeFileName(args.Split(' ')[1]);
                        list.Clear();
                        foreach (string s in File.ReadAllLines("extra/copy/index.copydb")) {
                            CopyOwner cO = new CopyOwner();
                            cO.file = s.Split(' ')[0];
                            cO.uuid = s.Split(' ')[1];
                            cO.name = s.Split(' ')[2];
                            list.Add(cO);
                        }
                        CopyOwner result = list.Find(
                        delegate(CopyOwner cO) {
                            return cO.file == args;
                        }
                                );

                        if (p.rank.Permission >= _s.commands.GetOtherPerm(this) || result.uuid == p.uuid.Value) {
                            if (File.Exists("extra/copy/" + args + ".copy")) {
                                try {
                                    if (File.Exists("extra/copyBackup/" + args + ".copy")) {
                                        File.Delete("extra/copyBackup/" + args + ".copy");
                                    }
                                    File.Move("extra/copy/" + args + ".copy", "extra/copyBackup/" + args + ".copy");
                                }
                                catch (Exception e) {
                                    _s.logger.ErrorLog(e);
                                }
                                p.SendMessage("File &f" + args + _s.props.DefaultColor + " has been deleted.");
                                list.Remove(result);
                                File.Create("extra/copy/index.copydb").Dispose();
                                using (StreamWriter sW = File.CreateText("extra/copy/index.copydb")) {
                                    foreach (CopyOwner cO in list) {
                                        sW.WriteLine(cO.file + " " + cO.uuid + " " + cO.name);
                                    }
                                }
                            }
                            else {
                                p.SendMessage("File does not exist.");
                            }
                        }
                        else {
                            p.SendMessage("You must be an " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name + "+ or file owner to delete a save.");
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
                _s.logger.ErrorLog(e);
            }
        }
        public class CopyOwner {
            public string uuid;
            public string name;
            public string file;
        }

        // Copied from /text; not the best solution, but it'll do for now -Jjp137
        private string SanitizeFileName(string filename) {
            return Regex.Replace(filename, @"[^\d\w\-]", String.Empty);
        }

        /// <summary>
        /// Called when /help is used on /store.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/store <filename?> - Stores your copied item to the server " +
                               "as a file with the specified name.");
            p.SendMessage("/store delete <filename?> - Deletes a saved copy file." +
                               " Only " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name +
                               " and the file creator may delete it.");
        }
    }
}

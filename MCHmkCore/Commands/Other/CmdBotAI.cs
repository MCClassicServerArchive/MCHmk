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
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdBotAI : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"bot", "ai", "add", "del", "remove"});

        public override string Name {
            get {
                return "botai";
            }
        }
        public override string Shortcut {
            get {
                return "bai";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdBotAI(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args.Split(' ').Length < 2) {
                Help(p);
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            string foundPath = args.Split(' ')[1].ToLower();

            if (!Player.ValidName(foundPath)) {
                p.SendMessage("Invalid AI name!");
                return;
            }
            if (foundPath == "hunt" || foundPath == "kill") {
                p.SendMessage("Reserved for special AI.");
                return;
            }

            try {
                switch (args.Split(' ')[0]) {
                case "add":
                    if (args.Split(' ').Length == 2) {
                        addPoint(p, foundPath);
                    }
                    else if (args.Split(' ').Length == 3) {
                        addPoint(p, foundPath, args.Split(' ')[2]);
                    }
                    else if (args.Split(' ').Length == 4) {
                        addPoint(p, foundPath, args.Split(' ')[2], args.Split(' ')[3]);
                    }
                    else {
                        addPoint(p, foundPath, args.Split(' ')[2], args.Split(' ')[3], args.Split(' ')[4]);
                    }
                    break;
                case "del":
                    if (!Directory.Exists("bots/deleted")) {
                        Directory.CreateDirectory("bots/deleted");
                    }

                    int currentTry = 0;
                    if (File.Exists("bots/" + foundPath)) {
                        retry:
                        try {
                            if (args.Split(' ').Length == 2) {
                                if (currentTry == 0) {
                                    File.Move("bots/" + foundPath, "bots/deleted/" + foundPath);
                                }
                                else {
                                    File.Move("bots/" + foundPath, "bots/deleted/" + foundPath + currentTry);
                                }
                            }
                            else {
                                if (args.Split(' ')[2].ToLower() == "last") {
                                    string[] Lines = File.ReadAllLines("bots/" + foundPath);
                                    string[] outLines = new string[Lines.Length - 1];
                                    for (int i = 0; i < Lines.Length - 1; i++) {
                                        outLines[i] = Lines[i];
                                    }

                                    File.WriteAllLines("bots/" + foundPath, outLines);
                                    p.SendMessage("Deleted the last waypoint from " + foundPath);
                                    return;
                                }
                                else {
                                    Help(p);
                                    return;
                                }
                            }
                        }
                        catch (IOException) {
                            currentTry++;
                            goto retry;
                        }
                        p.SendMessage("Deleted &b" + foundPath);
                    }
                    else {
                        p.SendMessage("Could not find specified AI.");
                    }
                    break;
                default:
                    Help(p);
                    return;
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        public void addPoint(Player p, string foundPath, string additional = "", string extra = "10", string more = "2") {
            string[] allLines;
            try {
                allLines = File.ReadAllLines("bots/" + foundPath);
            }
            catch {  // TODO: find exact exception to check
                allLines = new string[1];
            }

            StreamWriter SW;
            try {
                if (!File.Exists("bots/" + foundPath)) {
                    p.SendMessage("Created new bot AI: &b" + foundPath);
                    using (SW = File.CreateText("bots/" + foundPath)) {
                        SW.WriteLine("#Version 2");
                    }
                }
                else if (allLines[0] != "#Version 2") {
                    p.SendMessage("File found is out-of-date. Overwriting");
                    File.Delete("bots/" + foundPath);
                    using (SW = File.CreateText("bots/" + foundPath)) {
                        SW.WriteLine("#Version 2");
                    }
                }
                else {
                    p.SendMessage("Appended to bot AI: &b" + foundPath);
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("An error occurred when accessing the files. You may need to delete it.");
                return;
            }

            try {
                using (SW = File.AppendText("bots/" + foundPath)) {
                    switch (additional.ToLower()) {
                    case "":
                    case "walk":
                        SW.WriteLine("walk " + p.pos[0] + " " + p.pos[1] + " " + p.pos[2] + " " + p.rot[0] + " " + p.rot[1]);
                        break;
                    case "teleport":
                    case "tp":
                        SW.WriteLine("teleport " + p.pos[0] + " " + p.pos[1] + " " + p.pos[2] + " " + p.rot[0] + " " + p.rot[1]);
                        break;
                    case "wait":
                        SW.WriteLine("wait " + int.Parse(extra));
                        break;
                    case "nod":
                        SW.WriteLine("nod " + int.Parse(extra) + " " + int.Parse(more));
                        break;
                    case "speed":
                        SW.WriteLine("speed " + int.Parse(extra));
                        break;
                    case "remove":
                        SW.WriteLine("remove");
                        break;
                    case "reset":
                        SW.WriteLine("reset");
                        break;
                    case "spin":
                        SW.WriteLine("spin " + int.Parse(extra) + " " + int.Parse(more));
                        break;
                    case "reverse":
                        for (int i = allLines.Length - 1; i > 0; i--) if (allLines[i][0] != '#' && allLines[i] != String.Empty) {
                                SW.WriteLine(allLines[i]);
                            }
                        break;
                    case "linkscript":
                        if (extra != "10") {
                            SW.WriteLine("linkscript " + extra);
                        }
                        else {
                            p.SendMessage("Linkscript requires a script as a parameter");
                        }
                        break;
                    case "jump":
                        SW.WriteLine("jump");
                        break;
                    default:
                        p.SendMessage("Could not find \"" + additional + "\"");
                        break;
                    }
                }
            }
            catch {  // TODO: Find the exact exception to catch
                p.SendMessage("Invalid parameter");
            }
        }

        
        /// <summary>
        /// Called when /help is used on /botai.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/botai <add/del> <*AI name*> <extra?> - Adds or deletes specified AI.");
            p.SendMessage("Extras: walk, teleport, wait, nod, speed, spin, reset, remove, reverse, linkscript, jump.");
            p.SendMessage("Wait, nod and spin can have an extra '0.1 seconds' parameter.");
            p.SendMessage("Nod and spin can also take a 'third' speed parameter.");
            p.SendMessage("Speed sets a percentage of normal speed.");
            p.SendMessage("Linkscript takes a script name as parameter.");
        }
    }
}

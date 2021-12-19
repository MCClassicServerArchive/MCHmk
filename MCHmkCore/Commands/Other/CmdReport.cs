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
 * Written By Jack1312

	Copyright © 2009-2014 MCSharp team (Modified for use with MCZall/MCLawl/MCForge/MCForge-Redux)

	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at

	http://www.osedu.org/licenses/ECL-2.0
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
    public class CmdReport : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"rep", "alert"});

        public override string Name {
            get {
                return "report";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdReport(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can not be used in console!");
                return;
            }
            if (args == String.Empty) {
                Help(p);
                return;
            }
            int length = args.Split(' ').Length;
            try {
                switch (args.Split()[0]) {
                case "list":
                    if (length == 1 && p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                        if (!Directory.Exists("extra/reported")) {
                            Directory.CreateDirectory("extra/reported");
                        }
                        bool foundone = false;
                        FileInfo[] fi = new DirectoryInfo("extra/reported").GetFiles("*.txt");
                        p.SendMessage("The following players have been reported:");
                        foreach (FileInfo file in fi) {
                            foundone = true;
                            var parsed = file.Name.Replace(".txt", String.Empty);
                            p.SendMessage("- %c" + parsed);
                        }
                        if (foundone) {
                            p.SendMessage("Use %f/report check [Player] " + _s.props.DefaultColor + "to view report info.");
                            p.SendMessage("Use %f/report delete [Player] " + _s.props.DefaultColor + "to delete a report");
                        }
                        else {
                            p.SendMessage("%cNo reports were found!");
                        }
                    }
                    else {
                        p.SendMessage("%cYou cannot use 'list' as a report name!");
                    }
                    break;
                case "view":
                case "read":
                case "check":
                    if (args.Split().Length == 2 && p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                        if (!File.Exists("extra/reported/" + args.Split()[1] + ".txt")) {
                            p.SendMessage("%cThe player you specified has not been reported!");
                            return;
                        }
                        var readtext = File.ReadAllText("extra/reported/" + args.Split()[1] + ".txt");
                        p.SendMessage(readtext);
                    }
                    else {
                        p.SendMessage("%cYou cannot use 'check' as a report name! ");
                    }
                    break;
                case "delete":
                case "remove":
                    if (args.Split().Length == 2 && p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                        string msg = args.Split()[1];
                        if (!File.Exists("extra/reported/" + msg + ".txt")) {
                            p.SendMessage("%cThe player you specified has not been reported!");
                            return;
                        }
                        if (!Directory.Exists("extra/reportedbackups")) {
                            Directory.CreateDirectory("extra/reportedbackups");
                        }
                        if (File.Exists("extra/reportedbackups/" + msg + ".txt")) {
                            File.Delete("extra/reportedbackups/" + msg + ".txt");
                        }
                        File.Move("extra/reported/" + msg + ".txt", "extra/reportedbackups/" + msg + ".txt");
                        p.SendMessage("%a" + msg + "'s report has been deleted.");
                        _s.GlobalMessageOps(p.prefix + p.color + p.name + _s.props.DefaultColor + " deleted " + msg + "'s report.");
                        _s.logger.Log(msg + "'s report has been deleted by " + p.name);
                    }
                    else {
                        p.SendMessage("%cYou cannot use 'delete' as a report name! ");
                    }
                    break;
                case "clear":
                    if (length == 1 && p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                        if (!Directory.Exists("extra/reported")) {
                            Directory.CreateDirectory("extra/reported");
                        }
                        FileInfo[] fi = new DirectoryInfo("extra/reported").GetFiles("*.txt");
                        foreach (FileInfo file in fi) {
                            if (File.Exists("extra/reportedbackups/" + file.Name)) {
                                File.Delete("extra/reportedbackups/" + file.Name);
                            }
                            file.MoveTo("extra/reportedbackups/" + file.Name);
                        }
                        p.SendMessage("%aYou have cleared all reports!");
                        _s.GlobalMessageOps(p.prefix + p.name + "%c cleared ALL reports!");
                        _s.logger.Log(p.name + " cleared ALL reports!");
                    }
                    else {
                        p.SendMessage("%cYou cannot use 'clear' as a report name! ");
                    }
                    break;
                default:
                    string msg1 = String.Empty;
                    string msg2 = String.Empty;
                    try {
                        msg1 = args.Substring(0, args.IndexOf(' ')).ToLower();
                        msg2 = args.Substring(args.IndexOf(' ') + 1).ToLower();
                    }
                    catch {  // TODO: Find exact exception to catch
                        return;
                    }
                    if (File.Exists("extra/reported/" + msg1 + ".txt")) {
                        File.WriteAllText("extra/reported/" + msg1 + "(2).txt",
                                          msg2 + " - Reported by " + p.name + "." + " DateTime: " + DateTime.Now);
                        p.SendMessage("%aYour report has been sent, it should be viewed when an operator is online!");
                        break;
                    }
                    if (File.Exists("extra/reported/" + msg1 + "(2).txt")) {
                        p.SendMessage("%cThe player you've reported has already been reported 2 times! Please wait patiently untill an OP+ has reviewed the reports!");
                        break;
                    }
                    if (!Directory.Exists("extra/reported")) {
                        Directory.CreateDirectory("extra/reported");
                    }
                    File.WriteAllText("extra/reported/" + msg1 + ".txt",
                                      msg2 + " - Reported by " + p.name + "." + " DateTime: " + DateTime.Now);
                    p.SendMessage("%aYour report has been sent, it should be viewed when an operator is online!");
                    _s.GlobalMessageOps(p.prefix + p.name + _s.props.DefaultColor + " has made a report, view it with %f/report list ");
                    break;
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Called when /help is used on /report.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/report <player?> <reason?> - " +
                               "Reports a player for a given reason.");
            if (p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                p.SendMessage("/report check - Checks the list of reported players.");
                p.SendMessage("/report view <player?> - " +
                                   "View the report on the specified player.");
                p.SendMessage("/report delete <player?> - " +
                                   "Delete the report on the specified player.");
                p.SendMessage("/report delete all - Deletes all reports.");
            }
        }
    }
}

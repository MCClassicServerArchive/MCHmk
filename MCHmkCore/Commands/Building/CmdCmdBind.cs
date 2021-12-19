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

namespace MCHmk.Commands {
    public class CmdCmdBind : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"bind", "cmd", "command", "shortcut"});

        public override string Name {
            get {
                return "cmdbind";
            }
        }
        public override string Shortcut {
            get {
                return "cb";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdCmdBind(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }
            string foundcmd, foundmessage = String.Empty;
            int foundnum = 0;

            if (args.Length == 0) {
                bool OneFound = false;
                for (int i = 0; i < 10; i++) {
                    if (p.cmdBind[i] != null) {
                        p.SendMessage("&c/" + i + _s.props.DefaultColor + " bound to &b" + p.cmdBind[i] + " " + p.messageBind[i]);
                        OneFound = true;
                    }
                }
                if (!OneFound) {
                    p.SendMessage("You have no commands bound.");
                }
                return;
            }

            if (args.Split(' ').Length == 1) {
                try {
                    foundnum = Convert.ToInt16(args);
                    if (p.cmdBind[foundnum] == null) {
                        p.SendMessage("No command stored here yet.");
                        return;
                    }
                    foundcmd = "/" + p.cmdBind[foundnum] + " " + p.messageBind[foundnum];
                    p.SendMessage("Stored command: &b" + foundcmd);
                }
                catch {  // TODO: find exact exception to catch
                    Help(p);
                }
            }
            else if (args.Split(' ').Length > 1) {
                try {
                    foundnum = Convert.ToInt16(args.Split(' ')[args.Split(' ').Length - 1]);
                    foundcmd = args.Split(' ')[0];
                    if (args.Split(' ').Length > 2) {
                        foundmessage = args.Substring(args.IndexOf(' ') + 1);
                        foundmessage = foundmessage.Remove(foundmessage.LastIndexOf(' '));
                    }

                    p.cmdBind[foundnum] = foundcmd;
                    p.messageBind[foundnum] = foundmessage;

                    p.SendMessage("Binded &b/" + foundcmd + " " + foundmessage + " to &c/" + foundnum);
                }
                catch {  // TODO: find exact exception to catch
                    Help(p);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /cmdbind.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/cmdbind <command?> <num?> - Binds a command to a given number.");
            p.SendMessage("/cmdbind [num?] - Displays a list of bound commands.");
            p.SendMessage("The number must be between 0 and 9.");
            p.SendMessage("Use the binded command with '/<num?>'.");
            p.SendMessage("For example, /2 would use the command bound to 2.");
        }
    }
}

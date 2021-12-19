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

	Written by SebbiUltimate

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
    public class CmdSendCmd : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"send", "cmd", "command", "user", "player"});

        public override string Name {
            get {
                return "sendcmd";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Nobody;
            }
        }

        public CmdSendCmd(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            int length = args.Split().Length;
            Player player = null;
            if (length >= 1) {
                player = _s.players.Find(args.Split(' ')[0]);
            }
            else {
                return;
            }
            if (player == null) {
                p.SendMessage("Error: Player is not online.");
            }
            else {
                if (p.IsConsole) { }
                else {
                    if (player.rank.Permission >= p.rank.Permission) {
                        p.SendMessage("Cannot use this on someone of equal or greater rank.");
                        return;
                    }
                }
                string command;
                string cmdMsg = String.Empty;
                try {
                    command = args.Split(' ')[1];
                    for(int i = 2; i < length; i++) {
                        cmdMsg += args.Split(' ')[i] + " ";
                    }
                    cmdMsg.Remove(cmdMsg.Length - 1); //removing the space " " at the end of the msg
                    _s.commands.FindCommand(command).Use(player, cmdMsg);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Error: No parameter found");
                    command = args.Split(' ')[1];
                    _s.commands.FindCommand(command).Use(player, String.Empty);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /sendcmd.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/sendcmd <player?> <command?> <params?> - Make another player use a command.");
            p.SendMessage("Example: /sendcmd Jjp137 tp Legobricker");
        }
    }
}




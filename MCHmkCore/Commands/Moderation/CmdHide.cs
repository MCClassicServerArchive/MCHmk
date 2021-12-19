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
    public class CmdHide : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"hidden", "show", "invisible"});

        public override string Name {
            get {
                return "hide";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "mod";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdHide(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (args == "check") {
                if (p.hidden) {
                    p.SendMessage("You are currently hidden!");
                    return;
                }
                else {
                    p.SendMessage("You are not currently hidden!");
                    return;
                }
            }
            else if (args != String.Empty) {
                if (p.possess != String.Empty) {
                    p.SendMessage("Stop your current possession first.");
                    return;
                }
            }
            Command opchat = _s.commands.FindCommand("opchat");
            Command adminchat = _s.commands.FindCommand("adminchat");
            p.hidden = !p.hidden;
            if (p.hidden) {
                _s.GlobalDie(p, true);
                _s.GlobalMessageOps("To Ops -" + p.color + p.name + "-" + _s.props.DefaultColor + " is now &finvisible" +
                                        _s.props.DefaultColor + ".");
                _s.GlobalChat(p, "&c- " + p.color + p.prefix + p.name + _s.props.DefaultColor + " " +
                                  "disconnected.", false);
                _s.IRC.Say(p.name + " left the game (Disconnected.)");
                if (!p.opchat) {
                    opchat.Use(p, args);
                }
            }
            else {
                _s.GlobalSpawn(p, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1], false);
                _s.GlobalMessageOps("To Ops -" + p.color + p.name + "-" + _s.props.DefaultColor + " is now &8visible" +
                                        _s.props.DefaultColor + ".");
                _s.GlobalChat(p, "&a+ " + p.color + p.prefix + p.name + _s.props.DefaultColor + " " +
                                  "joined the game.", false);
                _s.IRC.Say(p.name + " joined the game");
                if (p.opchat) {
                    opchat.Use(p, args);
                }
                if (p.adminchat) {
                    adminchat.Use(p, args);
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /hide.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/hide - Toggles the visibility of yourself to other players.");
            p.SendMessage("This command also toggles opchat.");
            p.SendMessage("/hide check - Shows whether you are hidden or not.");
        }
    }
}

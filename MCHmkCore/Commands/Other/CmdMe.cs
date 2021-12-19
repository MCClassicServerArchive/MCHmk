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

namespace MCHmk.Commands {
    public class CmdMe : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"do", "action"});

        public override string Name {
            get {
                return "me";
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
        public CmdMe(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                p.SendMessage("You need to enter a message.");
                return;
            }
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }

            if (p.muted) {
                p.SendMessage("You are currently muted and cannot use this command.");
                return;
            }
            if (_s.chatmod && !p.voice) {
                p.SendMessage("Chat moderation is on, you cannot emote.");
                return;
            }

            if (_s.props.worldChat) {
                _s.GlobalChat(p, p.color + "*" + p.name + " " + args, false);
            }
            else {
                _s.GlobalChatLevel(p, p.color + "*" + p.name + " " + args, false);
            }
            _s.IRC.Say("*" + p.name + " " + args);
        }

        /// <summary>
        /// Called when /help is used on /me.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/me <message?> - Sends a message written from a third-person perspective.");
            p.SendMessage("Example: '/me ate cake' would be: *$name ate cake");
        }
    }
}

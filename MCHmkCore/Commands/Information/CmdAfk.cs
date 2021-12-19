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
    /// <summary>
    /// This is the command /afk
    /// use /help afk in-game for more info
    /// </summary>
    public class CmdAfk : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"away", "dnd"});

        public override string Name {
            get {
                return "afk";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "information";
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
        public static string keywords {
            get {
                return String.Empty;
            }
        }
        public CmdAfk(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!p.IsConsole) {
                if (_s.chatmod) {
                    p.SendMessage("You cannot use /afk while chat moderation is enabled");
                    return;
                }

                if (args != "list") {
                    if (p.joker) {
                        args = String.Empty;
                    }
                    if (!_s.afkset.Contains(p.name)) {
                        _s.afkset.Add(p.name);
                        if (p.muted) {
                            args = String.Empty;
                        }
                        _s.GlobalMessage("-" + p.color + p.name + _s.props.DefaultColor + "- is AFK " + args);
                        _s.IRC.Say(p.name + " is AFK " + args);
                        p.afkStart = DateTime.Now;
                        return;

                    }
                    else {
                        _s.afkset.Remove(p.name);
                        _s.GlobalMessage("-" + p.color + p.name + _s.props.DefaultColor + "- is no longer AFK");
                        _s.IRC.Say(p.name + " is no longer AFK");
                        return;
                    }
                }
                else {
                    foreach (string s in _s.afkset) {
                        p.SendMessage(s);
                    }
                    return;
                }
            }
            p.SendMessage("This command can only be used in-game");
        }

        /// <summary>
        /// Called when /help is used on /afk.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/afk [reason?] - Marks yourself as AFK.");
            p.SendMessage("Use again to mark yourself as back.");
            p.SendMessage("/afk list - Lists all players that are AFK.");
        }
    }
}

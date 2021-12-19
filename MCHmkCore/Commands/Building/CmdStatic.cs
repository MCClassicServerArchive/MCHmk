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
    public class CmdStatic : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"toggle", "mode"});

        public override string Name {
            get {
                return "static";
            }
        }
        public override string Shortcut {
            get {
                return "t";
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
        public CmdStatic(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            p.staticCommands = !p.staticCommands;
            p.ClearSelection();
            p.blockAction = 0;

            p.SendMessage("Static mode: &a" + p.staticCommands.ToString());

            try {
                if (args != String.Empty) {
                    if (args.IndexOf(' ') == -1) {
                        if (_s.commands.CanExecute(p, args)) {
                            _s.commands.FindCommand(args).Use(p, String.Empty);
                        }
                        else {
                            p.SendMessage("Cannot use that command.");
                        }
                    }
                    else {
                        if (_s.commands.CanExecute(p, args.Split(' ')[0])) {
                            _s.commands.FindCommand(args.Split(' ')[0]).Use(p, args.Substring(args.IndexOf(' ') + 1));
                        }
                        else {
                            p.SendMessage("Cannot use that command.");
                        }
                    }
                }
            }
            catch {  // TODO: would this be user error?
                p.SendMessage("Could not find specified command");
            }
        }

        /// <summary>
        /// Called when /help is used on /static.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/static [command?] - Makes every command a toggle.");
            p.SendMessage("If a command is specified, that command stays on until " +
                               "static mode is turned off.");
            p.SendMessage("Use /static again to turn off static mode.");
        }
    }
}

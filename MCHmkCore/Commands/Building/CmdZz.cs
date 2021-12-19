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
    public class CmdZz : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"static", "cuboid"});

        public override string Name {
            get {
                return "zz";
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
                return DefaultRankValue.Builder;
            }
        }

        public CmdZz(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (_s.commands.CanExecute(p, "cuboid") && _s.commands.CanExecute(p, "static")) {
                if ((!p.staticCommands == true) && (!p.megaBoid == true)) {
                    _s.commands.FindCommand("static").Use(p, String.Empty);
                    _s.commands.FindCommand("cuboid").Use(p, args);
                    p.SendMessage(p.color + p.name + _s.props.DefaultColor + " to stop this, use /zz again");
                }
                else {
                    p.ClearSelection();
                    p.staticCommands = false;
                    p.SendMessage("/zz has ended.");
                }
            }
            else {
                p.SendMessage("Sorry, your rank cannot use one of the commands this uses!");
            }

        }

        /// <summary>
        /// Called when /help is used on /zz.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/zz - Same as '/static cuboid'.");
        }
    }
}

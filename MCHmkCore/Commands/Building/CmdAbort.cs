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
    public class CmdAbort : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"command", "action", "mode"});

        public override string Name {
            get {
                return "abort";
            }
        }
        public override string Shortcut {
            get {
                return "a";
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

        public CmdAbort(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!p.IsConsole) {
                p.ClearSelection();

                p.painting = false;
                p.blockAction = 0;
                p.megaBoid = false;
                p.cmdTimer = false;
                p.staticCommands = false;
                p.deleteMode = false;
                p.ZoneCheck = false;
                p.modeType = 0;
                p.aiming = false;
                p.onTrain = false;
                p.isFlying = false;
                try {
                    p.level.blockqueue.RemoveAll((BlockQueue.block b) => {
                        if (b.p == p) {
                            return true;
                        }
                        return false;
                    });
                }
                finally {
                    _s.blockQueue.resume();
                }
                p.SendMessage("Every toggle or action was aborted.");
                return;
            }
            p.SendMessage("This command can only be used in-game!");
        }

        /// <summary>
        /// Called when /help is used on /abort.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/abort - Turns off all building modes and cancels any " +
                               "actions that are in progress.");
        }
    }
}

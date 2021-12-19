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
    public class CmdBlockSpeed : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "speed", "mod", "setting"});

        public override string Name {
            get {
                return "blockspeed";
            }
        }
        public override string Shortcut {
            get {
                return "bs";
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
        public CmdBlockSpeed(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                SendEstimation(p);
                return;
            }
            if (args == "clear") {
                _s.levels.ForEach((l) => {
                    l.blockqueue.Clear();
                });
                return;
            }
            if (args.StartsWith("bs")) {
                try {
                    _s.blockQueue.blockupdates = int.Parse(args.Split(' ')[1]);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Invalid number specified.");
                    return;
                }
                p.SendMessage(String.Format("Blocks per interval is now {0}.", _s.blockQueue.blockupdates));
                return;
            }
            if (args.StartsWith("ts")) {
                try {
                    _s.blockQueue.time = int.Parse(args.Split(' ')[1]);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Invalid number specified.");
                    return;
                }
                p.SendMessage(String.Format("Block interval is now {0}.", _s.blockQueue.time));
                return;
            }
            if (args.StartsWith("buf")) {
                if (p.level.bufferblocks) {
                    p.level.bufferblocks = false;
                    p.SendMessage(String.Format("Block buffering on {0} disabled.", p.level.name));
                }
                else {
                    p.level.bufferblocks = true;
                    p.SendMessage(String.Format("Block buffering on {0} enabled.", p.level.name));
                }
                return;
            }
            if (args.StartsWith("net")) {
                switch (int.Parse(args.Split(' ')[1])) {
                case 2:
                    _s.blockQueue.blockupdates = 25;
                    _s.blockQueue.time = 100;
                    break;
                case 4:
                    _s.blockQueue.blockupdates = 50;
                    _s.blockQueue.time = 100;
                    break;
                case 8:
                    _s.blockQueue.blockupdates = 100;
                    _s.blockQueue.time = 100;
                    break;
                case 12:
                    _s.blockQueue.blockupdates = 200;
                    _s.blockQueue.time = 100;
                    break;
                case 16:
                    _s.blockQueue.blockupdates = 200;
                    _s.blockQueue.time = 100;
                    break;
                case 161:
                    _s.blockQueue.blockupdates = 100;
                    _s.blockQueue.time = 50;
                    break;
                case 20:
                    _s.blockQueue.blockupdates = 125;
                    _s.blockQueue.time = 50;
                    break;
                case 24:
                    _s.blockQueue.blockupdates = 150;
                    _s.blockQueue.time = 50;
                    break;
                default:
                    _s.blockQueue.blockupdates = 200;
                    _s.blockQueue.time = 100;
                    break;
                }
                SendEstimation(p);
                return;
            }
        }
        private void SendEstimation(Player p) {
            p.SendMessage(String.Format("{0} blocks every {1} milliseconds = {2} blocks per second.",
                                                _s.blockQueue.blockupdates, _s.blockQueue.time, _s.blockQueue.blockupdates * (1000 / _s.blockQueue.time)));
            p.SendMessage(String.Format("Using ~{0}KB/s times {1} player(s) = ~{2}KB/s",
                                                (_s.blockQueue.blockupdates * (1000 / _s.blockQueue.time) * 8) / 1000, _s.players.Count,
                                                _s.players.Count * ((_s.blockQueue.blockupdates * (1000 / _s.blockQueue.time) * 8) / 1000)));
        }

        /// <summary>
        /// Called when /help is used on /blockspeed.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // I'm not going to touch this message right now -Jjp137
            p.SendMessage("/blockspeed <option?> <option value> - Changes the Block Speed.");
            p.SendMessage("Options are: bs (blocks per interval), ts (interval in milliseconds), buf (toggles buffering), clear, and net.");
            p.SendMessage("Option values: 2,4,8,12,16,20,24 - Presets, divide by 8 and times by 1000 to get blocks per second.");
        }
    }
}

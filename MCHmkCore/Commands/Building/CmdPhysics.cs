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
    public class CmdPhysics : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"block", "move"});

        public override string Name {
            get {
                return "physics";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdPhysics(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                foreach (Level l in _s.levels) {
                    if (l.physics > 0) {
                        p.SendMessage("&5" + l.name + _s.props.DefaultColor + " has physics at &b" + l.physics + _s.props.DefaultColor +
                                           ". &cChecks: " + l.lastCheck + "; Updates: " + l.lastUpdate);
                    }
                }
                return;
            }
            try {
                int temp = 0;
                Level level = null;
                if (args.Split(' ').Length == 1) {
                    temp = int.Parse(args);
                    if (!p.IsConsole) {
                        level = p.level;
                    }
                    else {
                        level = _s.mainLevel;
                    }
                }
                else {
                    temp = System.Convert.ToInt16(args.Split(' ')[1]);
                    string nameStore = args.Split(' ')[0];
                    level = _s.levels.FindExact(nameStore);
                }
                if (temp >= 0 && temp <= 5) {
                    level.setPhysics(temp);
                    switch (temp) {
                    case 0:
                        level.physic.ClearPhysics(level);
                        _s.GlobalMessage("Physics are now &cOFF" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now OFF on " + level.name + ".");
                        _s.IRC.Say("Physics are now OFF on " + level.name + ".");
                        break;

                    case 1:
                        _s.GlobalMessage("Physics are now &aNormal" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now Normal on " + level.name + ".");
                        _s.IRC.Say("Physics are now Normal on " + level.name + ".");
                        break;

                    case 2:
                        _s.GlobalMessage("Physics are now &aAdvanced" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now Advanced on " + level.name + ".");
                        _s.IRC.Say("Physics are now Advanced on " + level.name + ".");
                        break;

                    case 3:
                        _s.GlobalMessage("Physics are now &aHardcore" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now Hardcore on " + level.name + ".");
                        _s.IRC.Say("Physics are now Hardcore on " + level.name + ".");
                        break;

                    case 4:
                        _s.GlobalMessage("Physics are now &aInstant" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now Instant on " + level.name + ".");
                        _s.IRC.Say("Physics are now Instant on " + level.name + ".");
                        break;
                    case 5:
                        _s.GlobalMessage("Physics are now &4Doors-Only" + _s.props.DefaultColor + " on &b" + level.name + _s.props.DefaultColor + ".");
                        _s.logger.Log("Physics are now Doors-Only on " + level.name + ".");
                        _s.IRC.Say("Physics are now Doors-Only on " + level.name + ".");
                        break;
                    }

                    level.changed = true;
                }
                else {
                    p.SendMessage("Not a valid setting");
                }
            }
            catch {  // TODO: find exact exception to catch
                p.SendMessage("INVALID INPUT");
            }
        }

        /// <summary>
        /// Called when /help is used on /physics.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/physics [map?] <0/1/2/3/4/5> - Set the specified map's physics.");
            p.SendMessage("Physics settings: 0-Off 1-On 2-Advanced 3-Hardcore 4-Instant 5-Doors Only.");
            p.SendMessage("If a map is not specified, the current map is used.");
        }
    }
}

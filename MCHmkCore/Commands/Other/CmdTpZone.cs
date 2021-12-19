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
    public class CmdTpZone : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"tp", "zone", "teleport"});

        public override string Name {
            get {
                return "tpzone";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdTpZone(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                args = "list";
            }

            string[] parameters = args.Split(' ');

            if (parameters[0].ToLower() == "list") {
                if (parameters.Length > 1) {
                    int pageNum, currentNum;
                    try {
                        pageNum = int.Parse(parameters[1]) * 10;
                        currentNum = pageNum - 10;
                    }
                    catch {  // TODO: Find exact exception to catch
                        Help(p);
                        return;
                    }

                    if (currentNum < 0) {
                        p.SendMessage("Must be greater than 0");
                        return;
                    }
                    if (pageNum > p.level.ZoneList.Count) {
                        pageNum = p.level.ZoneList.Count;
                    }
                    if (currentNum > p.level.ZoneList.Count) {
                        p.SendMessage("No Zones beyond number " + (p.level.ZoneList.Count - 1));
                        return;
                    }

                    p.SendMessage("Zones (" + currentNum + " to " + (pageNum - 1) + "):");
                    for (int i = currentNum; i < pageNum; i++) {
                        Level.Zone zone = p.level.ZoneList[i];
                        p.SendMessage("&c" + i + " &b(" +
                                           zone.smallX + "-" + zone.bigX + ", " +
                                           zone.smallY + "-" + zone.bigY + ", " +
                                           zone.smallZ + "-" + zone.bigZ + ") &f" +
                                           zone.owner);
                    }
                }
                else {
                    for (int i = 0; i < p.level.ZoneList.Count; i++) {
                        Level.Zone zone = p.level.ZoneList[i];
                        p.SendMessage("&c" + i + " &b(" +
                                           zone.smallX + "-" + zone.bigX + ", " +
                                           zone.smallY + "-" + zone.bigY + ", " +
                                           zone.smallZ + "-" + zone.bigZ + ") &f" +
                                           zone.owner);
                    }
                    p.SendMessage("For a more structured list, use /tpzone list <1/2/3/..>");
                }
            }
            else {
                int zoneID;
                try {
                    zoneID = int.Parse(args);
                }
                catch {  // TODO: find exact exception to catch
                    Help(p);
                    return;
                }

                if (zoneID < 0 || zoneID > p.level.ZoneList.Count) {
                    p.SendMessage("This zone doesn't exist");
                    return;
                }

                Level.Zone zone = p.level.ZoneList[zoneID];
                unchecked {
                    p.SendPos((byte)-1, (ushort)(zone.bigX * 32 + 16), (ushort)(zone.bigY * 32 + 32), (ushort)(zone.bigZ * 32 + 16),
                              p.rot[0], p.rot[1]);
                }

                p.SendMessage("Teleported to zone &c" + zoneID + " &b(" +
                                   zone.bigX + ", " + zone.bigY + ", " + zone.bigZ + ") &f" +
                                   zone.owner);
            }
        }

        /// <summary>
        /// Called when /help is used on /tpzone.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tpzone <id?> - Teleports to the zone with the given ID.");
            p.SendMessage("/tpzone list - Lists all the zones in the current map.");
        }
    }
}

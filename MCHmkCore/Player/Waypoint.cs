/*
    Copyright 2016 Jjp137

    This file contains code from MCForge-Redux 6.x.

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

using System;
using System.Threading;
using System.IO;

namespace MCHmk {
    public class Waypoint {
        public class WP {
            public ushort x;
            public ushort y;
            public ushort z;
            public byte rotx;
            public byte roty;
            public string name;
            public string lvlname;
        }

        public static WP Find(string name, Player p) {
            WP wpfound = null;
            bool found = false;
            foreach (WP wp in p.Waypoints) {
                if (wp.name.ToLower() == name.ToLower()) {
                    wpfound = wp;
                    found = true;
                }
            }
            if (found) {
                return wpfound;
            }
            else {
                return null;
            }
        }

        public static void Goto(string waypoint, Player p) {
            if (!Exists(waypoint, p)) {
                return;
            }
            WP wp = Find(waypoint, p);
            Level lvl = p.Server.levels.Find(wp.lvlname);  // FIXME: crappy hack
            if (wp == null) {
                return;
            }
            if (lvl != null) {
                if (p.level != lvl) {
                    p.Server.commands.FindCommand("goto").Use(p, lvl.name);
                    while (p.Loading) {
                        Thread.Sleep(250);
                    }
                }
                unchecked {
                    p.SendPos((byte)-1, wp.x, wp.y, wp.z, wp.rotx, wp.roty);
                }
                p.SendMessage("Sent you to waypoint");
            }
            else {
                p.SendMessage("The map that that waypoint is on isn't loaded right now (" + wp.lvlname + ")");
                return;
            }
        }

        public static void Create(string waypoint, Player p) {
            WP wp = new WP();
            {
                wp.x = p.pos[0];
                wp.y = p.pos[1];
                wp.z = p.pos[2];
                wp.rotx = p.rot[0];
                wp.roty = p.rot[1];
                wp.name = waypoint;
                wp.lvlname = p.level.name;
            }
            p.Waypoints.Add(wp);
            Save(p);
        }

        public static void Update(string waypoint, Player p) {
            WP wp = Find(waypoint, p);
            p.Waypoints.Remove(wp);
            {
                wp.x = p.pos[0];
                wp.y = p.pos[1];
                wp.z = p.pos[2];
                wp.rotx = p.rot[0];
                wp.roty = p.rot[1];
                wp.name = waypoint;
                wp.lvlname = p.level.name;
            }
            p.Waypoints.Add(wp);
            Save(p);
        }

        public static void Remove(string waypoint, Player p) {
            WP wp = Find(waypoint, p);
            p.Waypoints.Remove(wp);
            Save(p);
        }

        public static bool Exists(string waypoint, Player p) {
            bool exists = false;
            foreach (WP wp in p.Waypoints) {
                if (wp.name.ToLower() == waypoint.ToLower()) {
                    exists = true;
                }
            }
            return exists;
        }

        private static readonly string waypointsPath = Path.Combine("extra", "Waypoints");

        public static void Load(Player p) {
            if (File.Exists(Path.Combine(waypointsPath, p.name + ".save"))) {
                using (StreamReader SR = new StreamReader(Path.Combine(waypointsPath, p.name + ".save"))) {
                    bool failed = false;
                    string line;
                    while (SR.EndOfStream == false) {
                        line = SR.ReadLine().ToLower().Trim();
                        if (!line.StartsWith("#") && line.Contains(":")) {
                            failed = false;
                            string[] LINE = line.ToLower().Split(':');
                            WP wp = new WP();
                            try {
                                wp.name = LINE[0];
                                wp.lvlname = LINE[1];
                                wp.x = ushort.Parse(LINE[2]);
                                wp.y = ushort.Parse(LINE[3]);
                                wp.z = ushort.Parse(LINE[4]);
                                wp.rotx = byte.Parse(LINE[5]);
                                wp.roty = byte.Parse(LINE[6]);
                            }
                            catch (Exception e) {
                                p.Logger.ErrorLog(e);
                                p.Logger.Log("Couldn't load a Waypoint!");
                                failed = true;
                            }
                            if (failed == false) {
                                p.Waypoints.Add(wp);
                            }
                        }
                    }
                    SR.Dispose();
                }
            }
        }

        public static void Save(Player p) {
            using (StreamWriter SW = new StreamWriter(Path.Combine(waypointsPath, p.name + ".save"))) {
                foreach (WP wp in p.Waypoints) {
                    SW.WriteLine(wp.name + ":" + wp.lvlname + ":" + wp.x + ":" + wp.y + ":" + wp.z + ":" + wp.rotx + ":" + wp.roty);
                }
                SW.Dispose();
            }
        }
    }
}


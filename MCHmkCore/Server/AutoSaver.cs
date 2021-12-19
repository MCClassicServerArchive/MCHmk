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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MCHmk {
    /// <summary>
    /// The AutoSaver class, which contains methods that are used to automatically save maps.
    /// </summary>
    internal class AutoSaver {
        /// <summary>
        /// The server that the levels are on.
        /// </summary>
        private Server _s;
        /// <summary>
        /// The number of milliseconds between autosaves.
        /// </summary>
        private int _interval;
        /// <summary>
        /// An integer that keeps track of when to save a backup of each level. If Run() causes it
        /// to reach 0, backups of each level will be saved.
        /// </summary>
        private int count = 1;

        /// <summary>
        /// Constructs an AutoSaver instance.
        /// </summary>
        /// <param name="interval"> The number of seconds between each autosave. </param>
        public AutoSaver(Server s, int interval) {
            _s = s;
            _interval = interval * 1000; // To convert from seconds into milliseconds

            new Thread(new ThreadStart(delegate {
                while (true) {
                    Thread.Sleep(_interval);

                    // Runs the autosave procedure.
                    new Thread(new ThreadStart(Run)).Start(); // I'll make it pretty later -Jjp137

                    // Go to the next iteration of the loop if there are no players online.
                    if (_s.players.Count <= 0) {
                        continue;
                    }

                    // These lines are responsible for those console messages...
                    string allCount = _s.players.Aggregate(String.Empty, (current, pl) => current + (", " + pl.name));
                    try {
                        _s.logger.Log("!PLAYERS ONLINE: " + allCount.Remove(0, 2), true);
                    }
                    catch (Exception e) { 
                        _s.logger.ErrorLog(e);
                    }

                    allCount = String.Empty;
                    foreach (Level lvl in _s.levels) {
                        allCount += ", " + lvl.name;
                    }
                    try {
                        _s.logger.Log("!LEVELS ONLINE: " + allCount.Remove(0, 2), true);
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                }
            })).Start();
        }

        /// <summary>
        /// Saves the levels to disk.
        /// </summary>
        public void Run() {
            try {
                count--; // One step closer to backing up all the levels...

                _s.levels.ForEach(delegate(Level l) {
                    try {
                        // Saves the level if it has changed.
                        if (!l.changed) {
                            return;
                        }
                        l.Save();

                        // Creates a backup of the level if enough time has passed.
                        if (count == 0) {
                            int backupNumber = l.Backup();

                            if (backupNumber != -1) {
                                l.ChatLevel("Backup " + backupNumber.ToString() + " saved.");
                                _s.logger.Log("Backup " + backupNumber.ToString() + " saved for " + l.name);
                            }
                        }
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                        _s.logger.Log("Backup for " + l.name + " has caused an error.");
                    }
                });

                if (count <= 0) {
                    // So every 15 * backupInterval, the levels are written to disk.
                    // If the server host specifies a backup interval of 300, it's actually
                    // 300 * 15 = 4500, or 75 minutes. Okay, what? -Jjp137
                    count = 15;
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }

            try {
                // Save all player data to the SQL table.
                if (_s.players.Count > 0) {
                    List<Player> tempList = new List<Player>();
                    tempList.AddRange(_s.players);
                    foreach (Player p in tempList) {
                        p.save();
                    }
                    tempList.Clear();
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }
        }
    }
}


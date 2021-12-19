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
using System.Diagnostics;

namespace MCHmk.Commands {
    public class CmdServerReport : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"report", "server"});

        public override string Name {
            get {
                return "serverreport";
            }
        }
        public override string Shortcut {
            get {
                return "sr";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdServerReport(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (_s.PCCounter == null) {
                p.SendMessage("Starting PCCounter...one second");
                _s.PCCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _s.PCCounter.BeginInit();
                _s.PCCounter.NextValue();
            }
            if (_s.ProcessCounter == null) {
                p.SendMessage("Starting ProcessCounter...one second");
                _s.ProcessCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
                _s.ProcessCounter.BeginInit();
                _s.ProcessCounter.NextValue();
            }

            //   TimeSpan tp = Process.GetCurrentProcess().TotalProcessorTime;
            TimeSpan up = (DateTime.Now - Process.GetCurrentProcess().StartTime);

            //To get actual CPU% is OS dependant
            string ProcessorUsage = "CPU Usage (Processes : All Processes):" + _s.ProcessCounter.NextValue() + " : " +
                                    _s.PCCounter.NextValue();
            //Alternative Average?
            //string ProcessorUsage = "CPU Usage is Not Implemented: So here is ProcessUsageTime/ProcessTotalTime:"+String.Format("00.00",(((tp.Ticks/up.Ticks))*100))+"%";
            //reports Private Bytes because it is what the process has reserved for itself and is unsharable
            string MemoryUsage = "Memory Usage: " + Math.Round((double)Process.GetCurrentProcess().PrivateMemorySize64 /
                                 1048576).ToString() + " Megabytes";
            string Uptime = "Uptime: " + up.Days + " Days " + up.Hours + " Hours " + up.Minutes + " Minutes " + up.Seconds +
                            " Seconds";
            string Threads = "Threads: " + Process.GetCurrentProcess().Threads.Count;
            p.SendMessage(Uptime);
            p.SendMessage(MemoryUsage);
            p.SendMessage(ProcessorUsage);
            p.SendMessage(Threads);

        }

        /// <summary>
        /// Called when /help is used on /serverreport.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/serverreport - Displays a report of the server's CPU usage, " +
                               "RAM usage, and uptime.");
        }
    }
}

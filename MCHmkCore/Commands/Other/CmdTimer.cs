/*
    Copyright 2016 Jjp137/LegoBricker

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
    public class CmdTimer : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"count", "down"});

        public override string Name {
            get {
                return "timer";
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdTimer(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // Ensures that a person only uses timer one at a time.
            if (p.cmdTimer == true) {
                p.SendMessage("Can only have one timer at a time. Use /abort to cancel your previous timer.");
                return;
            }

            // Initializes the timer.
            System.Timers.Timer messageTimer = new System.Timers.Timer(5000);

            // Checks to see that the command was given any parameters.
            if (args == String.Empty) {
                Help(p);
                return;
            }

            // Finds the time given and the message given and assigns them to their resepctive variables.
            // If no time is passed to the timer, it tells the user so, and exits the command.
            string[] messageArray = args.Split(" ".ToCharArray(), 2);
            int TotalTime = 0;
            try {
                TotalTime = int.Parse(messageArray[0]);
                args = String.Empty;
                if(messageArray.Length >= 2) {
                    args = messageArray[1];
                }
            }
            catch {  // TODO: Find exact exception to catch
                p.SendMessage("No time was passed!");
                return;
            }

            // Ensures the user doesn't hang the server up on unnecessary processes by ensuring that
            // the requested time is under 5 minutes.
            if (TotalTime > 300) {
                p.SendMessage("Cannot have more than 5 minutes in a timer.");
                return;
            }

            // Broadcasts that the timer has started.
            _s.GlobalChatLevel(p, _s.props.DefaultColor + "Timer lasting for " + TotalTime + " seconds has started.", false);
            TotalTime = (int)(TotalTime / 5);

            _s.GlobalChatLevel(p, _s.props.DefaultColor + args, false);

            // This block controls what happens as the timer runs.
            // Every 5 seconds, a message is broadcast to the server indicating how much time is left.
            p.cmdTimer = true;
            messageTimer.Elapsed += delegate {
                TotalTime--;
                // If the timer has run for as long as was requested, the timer stops. 
                // Also checks if the timer flag has been set to false, usually due to /abort.
                if (TotalTime < 1 || p.cmdTimer == false) {
                    p.SendMessage("Timer ended.");
                    messageTimer.Stop();
                    p.cmdTimer = false;
                }
                else {
                    _s.GlobalChatLevel(p, _s.props.DefaultColor + args, false);
                    _s.GlobalChatLevel(p, "Timer has " + (TotalTime * 5) + " seconds remaining.", false);
                }
            };

            messageTimer.Start();
        }

        /// <summary>
        /// Called when /help is used on /timer.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/timer <time?> [message?] - Starts a timer that repeats " +
                               "the given message every 5 seconds. This ends when the timer is up.");
        }
    }
}

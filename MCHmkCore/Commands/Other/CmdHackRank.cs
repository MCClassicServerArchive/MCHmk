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

    Made originally by 501st_commander, in something called SharpDevelop.
    Made into a safe and reasonabal command by EricKilla, in Visual Studio 2010.

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
    /// <summary>
    /// TODO: Description of CmdHackRank.
    /// </summary>
    public class CmdHackRank : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"fake", "rank", "color", "set", "mod"});

        public override string Name {
            get {
                return "hackrank";
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
                return DefaultRankValue.Banned;
            }
        }
        private string m_old_color;

        public CmdHackRank(Server s) : base(s) { }

        /// <summary>
        /// the use stub
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="args">Message</param>
        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            if (p.IsConsole) {
                p.SendMessage("Console can't use hackrank, that doesn't make any sense!");
                return;
            }

            string[] msg = args.Split(' ');
            if (_s.ranks.Exists(msg[0])) {
                Rank newRank = _s.ranks.Find(msg[0]);
                ranker(p, newRank);
            }
            else {
                p.SendMessage("Invalid Rank!");
                return;
            }
        }

        /// <summary>
        /// The hacer ranker
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="newRank">Group</param>
        public void ranker(Player p, Rank newRank) {
            p.color = newRank.color;

            //sent the trick text
            _s.GlobalMessage(p.color + p.name + _s.props.DefaultColor + "'s rank was set to " + newRank.color + newRank.name);
            _s.GlobalMessage("&6Congratulations!");
            p.SendMessage("You are now ranked " + newRank.color + newRank.name + _s.props.DefaultColor +
                          ", type /help for your new set of commands.");

            kick(p, newRank);
        }

        /// <summary>
        /// kicker
        /// </summary>
        /// <param name="p">Player</param>
        /// <param name="newRank">Group</param>
        private void kick(Player p, Rank newRank) {
            try {

                if (_s.props.hackrank_kick == true) {
                    int kicktime = (_s.props.hackrank_kick_time * 1000);

                    m_old_color = p.color;

                    //make the timer for the kick
                    System.Timers.Timer messageTimer = new System.Timers.Timer(kicktime);

                    //start the timer
                    messageTimer.Start();

                    //delegate the timer
                    messageTimer.Elapsed += delegate {
                        //kick him!
                        p.Kick("You have been kicked for hacking the rank " + newRank.color + newRank.name);
                        p.color = m_old_color;
                        killTimer(messageTimer);
                    };
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("An error has happend! It wont kick you now! :|");
            }
        }

        private void killTimer(System.Timers.Timer time) {
            time.Stop();
            time.Dispose();
        }
        
        /// <summary>
        /// Called when /help is used on /hackrank.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/hackrank <rank?> - Sets you to the specified rank by hacking.");
            p.SendMessage("Usable ranks:");
            p.SendMessage(_s.ranks.ConcatNames(_s.props.DefaultColor, true));
        }

    }
}

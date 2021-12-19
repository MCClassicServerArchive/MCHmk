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
    /// <summary>
    /// This is the command /vote
    /// </summary>
    public class CmdVote : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"yes", "no"});

        public override string Name {
            get {
                return "vote";
            }
        }
        public override string Shortcut {
            get {
                return "vo";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdVote(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args != String.Empty) {
                if (!_s.voting) {
                    string temp = args.Substring(0, 1) == "%" ? String.Empty : _s.props.DefaultColor;
                    _s.voting = true;
                    _s.NoVotes = 0;
                    _s.YesVotes = 0;
                    _s.GlobalMessage(" " + Colors.green + "VOTE: " + temp + args + "(" + Colors.green + "Yes " + _s.props.DefaultColor + "/" +
                                         Colors.red + "No" + _s.props.DefaultColor + ")");
                    System.Threading.Thread.Sleep(15000);
                    _s.voting = false;
                    _s.GlobalMessage("The vote is in! " + Colors.green + "Y: " + _s.YesVotes + Colors.red + " N: " + _s.NoVotes);
                    _s.players.ForEach(delegate(Player winners) {
                        winners.voted = false;
                    });
                }
                else {
                    p.SendMessage("A vote is in progress!");
                }
            }
            else {
                Help(p);
            }
        }

        /// <summary>
        /// Called when /help is used on /vote.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/vote <message?> - Starts a vote, using the given message " +
                          "as the prompt.");
        }
    }
}

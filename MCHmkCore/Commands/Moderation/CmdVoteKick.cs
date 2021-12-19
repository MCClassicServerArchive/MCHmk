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
    public class CmdVoteKick : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"vote", "kick"});

        public override string Name {
            get {
                return "votekick";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        public CmdVoteKick(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (args == String.Empty || args.IndexOf(' ') != -1) {
                Help(p);
                return;
            }

            if (_s.voteKickInProgress == true) {
                p.SendMessage("Please wait for the current vote to finish!");
                return;
            }

            Player who = _s.players.Find(args);
            if (who == null) {
                p.SendMessage("Could not find player specified!");
                return;
            }

            if (who.rank.Permission >= p.rank.Permission) {
                _s.GlobalChat(p, p.color + p.name + " " + _s.props.DefaultColor + "tried to votekick " + who.color + who.name + " " +
                                  _s.props.DefaultColor + "but failed!", false);
                return;
            }

            _s.GlobalMessageOps(p.color + p.name + _s.props.DefaultColor + " used &a/votekick");
            _s.GlobalMessage("&9A vote to kick " + who.color + who.name + " " + _s.props.DefaultColor + "has been called!");
            _s.GlobalMessage("&9Type &aY " + _s.props.DefaultColor + "or &cN " + _s.props.DefaultColor + "to vote.");

            // 1/3rd of the players must vote or nothing happens
            // Keep it at 0 to disable min number of votes
            _s.voteKickVotesNeeded = 3;
            _s.voteKickInProgress = true;

            System.Timers.Timer voteTimer = new System.Timers.Timer(30000);

            voteTimer.Elapsed += delegate {
                voteTimer.Stop();

                _s.voteKickInProgress = false;

                int votesYes = 0;
                int votesNo = 0;

                _s.players.ForEach(delegate(Player pl) {
                    // Tally the votes
                    if (pl.voteKickChoice == VoteKickChoice.Yes) {
                        votesYes++;
                    }
                    if (pl.voteKickChoice == VoteKickChoice.No) {
                        votesNo++;
                    }
                    // Reset their choice
                    pl.voteKickChoice = VoteKickChoice.HasntVoted;
                });

                int netVotesYes = votesYes - votesNo;

                // Should we also send this to players?
                _s.GlobalMessageOps("Vote Ended.  Results: &aY: " + votesYes + " &cN: " + votesNo);
                _s.logger.Log("VoteKick results for " + who.name + ": " + votesYes + " yes and " + votesNo + " no votes.");

                if (votesYes + votesNo < _s.voteKickVotesNeeded) {
                    _s.GlobalMessage("Not enough votes were made. " + who.color + who.name + " " + _s.props.DefaultColor +
                                         "shall remain!");
                }
                else if (netVotesYes > 0) {
                    _s.GlobalMessage("The people have spoken, " + who.color + who.name + " " + _s.props.DefaultColor + "is gone!");
                    who.Kick("Vote-Kick: The people have spoken!");
                }
                else {
                    _s.GlobalMessage(who.color + who.name + " " + _s.props.DefaultColor + "shall remain!");
                }

                voteTimer.Dispose();
            };

            voteTimer.Start();
        }

        /// <summary>
        /// Called when /help is used on /votekick.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/votekick <player?> - Calls a vote to kick a particular player.");
        }
    }
}

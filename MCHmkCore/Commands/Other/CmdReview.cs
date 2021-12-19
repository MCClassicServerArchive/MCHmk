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
    Written by BeMacized
    Assisted by RedNoodle

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
    public class CmdReview : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"view", "look", "judge"});

        public override string Name {
            get {
                return "review";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdReview(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (!p.IsConsole && args == String.Empty) {
                args = "enter";
            }
            switch (args.ToLower()) {
            case "enter":
                if (p.IsConsole) {
                    p.SendMessage("You can't execute this command as Console!");
                    return;
                }
                if (p.canusereview) {
                    Rank gre = _s.ranks.FindPerm(_s.props.reviewenter);
                    if (gre == null) {
                        p.SendMessage("There is something wrong with the system.  A message has been sent to the admin to fix");
                        _s.GlobalMessageAdmins(p.name +
                                                   " tryed to use /review, but a system error occurred. Make sure your groups are formatted correctly");
                        _s.GlobalMessageAdmins("The group permission that is messed up is: " + _s.props.reviewenter.ToString() + " (" +
                                                   _s.props.reviewenter + ")");
                        return;
                    }
                    int lpe = gre.Permission;
                    if (p.rank.Permission >= lpe) {
                        foreach (string testwho in _s.reviewlist) {
                            if (testwho == p.name) {
                                p.SendMessage("You already entered the review queue!");
                                return;
                            }
                        }

                        bool isopson = false;
                        try {
                            foreach (Player pl in _s.players) {
                                if (pl.rank.Permission >= _s.props.opchatperm && !pl.hidden) {
                                    isopson = true;
                                    break; // We're done, break out of this loop
                                }
                            }
                        }
                        catch (Exception e) {
                            _s.logger.ErrorLog(e);
                            isopson = true;
                        }
                        if (isopson == true) {
                            _s.reviewlist.Add(p.name);
                            int reviewlistpos = _s.reviewlist.IndexOf(p.name);
                            if (reviewlistpos > 1) {
                                p.SendMessage("You entered the &creview " + _s.props.DefaultColor + "queue. You have &c" +
                                                   reviewlistpos.ToString() + _s.props.DefaultColor + " people in front of you in the queue");
                            }
                            if (reviewlistpos == 1) {
                                p.SendMessage("You entered the &creview " + _s.props.DefaultColor + "queue. There is &c1 " + _s.props.DefaultColor +
                                                   "person in front of you in the queue");
                            }
                            if ((reviewlistpos + 1) == 1) {
                                p.SendMessage("You entered the &creview " + _s.props.DefaultColor + "queue. You are &cfirst " +
                                                   _s.props.DefaultColor + "in line!");
                            }
                            p.SendMessage("The Online Operators have been notified. Someone should be with you shortly.");
                            _s.GlobalMessageOps(p.color + " - " + p.name + " - " + _s.props.DefaultColor + "entered the review queue");
                            if ((reviewlistpos + 1) > 1) {
                                _s.GlobalMessageOps("There are now &c" + (reviewlistpos + 1) + _s.props.DefaultColor +
                                                        " people waiting for &creview!");
                            }
                            else {
                                _s.GlobalMessageOps("There is now &c1 " + _s.props.DefaultColor + "person waiting for &creview!");
                            }
                            p.ReviewTimer();
                        }
                        else {
                            p.SendMessage("&cThere are no operators on to review your build. Please wait for one to come on and try again.");
                        }
                    }
                }
                else {
                    p.SendMessage("You have to wait " + _s.props.reviewcooldown + " seconds everytime you use this command");
                }
                break;

            case "list":
            case "view":
                if (p.IsConsole) {
                    if (_s.reviewlist.Count != 0) {
                        p.SendMessage("Players in the review queue:");
                        int viewnumb = 1;
                        foreach (string golist in _s.reviewlist) {
                            Uuid uuid = Uuid.FindUuid(_s.database, golist);
                            string FoundRank = _s.ranks.FindPlayer(uuid);
                            p.SendMessage(viewnumb.ToString() + ". " + golist + " - Current Rank: " + FoundRank);
                            viewnumb++;
                        }
                    }
                    else {
                        p.SendMessage("There are no players in the review queue!");
                    }
                    return;
                }
                Rank grv = _s.ranks.FindPerm(_s.props.reviewview);

                if (grv == null) {
                    p.SendMessage("There is something wrong with the system.  A message has been sent to the admin to fix");
                    _s.GlobalMessageAdmins(p.name +
                                               " tryed to use /review, but a system error occurred. Make sure your groups are formatted correctly");
                    _s.GlobalMessageAdmins("The group permission that is messed up is: " + _s.props.reviewview.ToString() + " (" +
                                               _s.props.reviewview + ")");
                    return;
                }

                int lpv = grv.Permission;
                if (p.rank.Permission >= lpv && !p.IsConsole) {
                    if (_s.reviewlist.Count != 0) {
                        p.SendMessage("&9Players in the review queue:");
                        int viewnumb = 1;
                        foreach (string golist in _s.reviewlist) {
                            Uuid uuid = Uuid.FindUuid(_s.database, golist);
                            string FoundRank = _s.ranks.FindPlayer(uuid);
                            p.SendMessage("&a" + viewnumb.ToString() + ". &f" + golist + "&a - Current Rank: " + _s.ranks.Find(
                                                   FoundRank).color + FoundRank);
                            viewnumb++;
                        }
                    }
                    else {
                        p.SendMessage("There are no players in the review queue!");
                    }
                }
                break;

            case "leave":
                if (p.IsConsole) {
                    p.SendMessage("You can't execute this command as Console!");
                    return;
                }
                Rank grl = _s.ranks.FindPerm(_s.props.reviewleave);

                if (grl == null) {
                    p.SendMessage("There is something wrong with the system.  A message has been sent to the admin to fix");
                    _s.GlobalMessageAdmins(p.name +
                                               " tryed to use /review, but a system error occurred. Make sure your groups are formatted correctly");
                    _s.GlobalMessageAdmins("The group permission that is messed up is: " + _s.props.reviewleave.ToString() + " (" +
                                               _s.props.reviewleave + ")");
                    return;
                }

                int lpl = grl.Permission;
                if (p.rank.Permission >= lpl) {
                    bool leavetest = false;
                    foreach (string testwho2 in _s.reviewlist) {
                        if (testwho2 == p.name) {
                            leavetest = true;
                        }
                    }
                    if (!leavetest) {
                        p.SendMessage("You aren't in the review queue so you can't leave it!");
                        return;
                    }
                    _s.reviewlist.Remove(p.name);
                    int toallplayerscount = 1;
                    foreach (string toallplayers in _s.reviewlist) {
                        Player tosend = _s.players.Find(toallplayers);
                        tosend.SendMessage("The review queue has changed. Your now on spot " + toallplayerscount.ToString() + ".");
                        toallplayerscount++;
                    }
                    p.SendMessage("You have left the review queue!");
                    return;
                }
                break;

            case "next":
                if (p.IsConsole) {
                    p.SendMessage("You can't execute this command as Console!");
                    return;
                }
                Rank grn = _s.ranks.FindPerm(_s.props.reviewnext);

                if (grn == null) {
                    p.SendMessage("There is something wrong with the system.  A message has been sent to the admin to fix");
                    _s.GlobalMessageAdmins(p.name +
                                               " tryed to use /review, but a system error occurred. Make sure your groups are formatted correctly");
                    _s.GlobalMessageAdmins("The group permission that is messed up is: " + _s.props.reviewnext.ToString() + " (" +
                                               _s.props.reviewnext + ")");
                    return;
                }

                int lpn = grn.Permission;
                if (p.rank.Permission >= lpn) {
                    if (_s.reviewlist.Count == 0) {
                        p.SendMessage("There are no players in the review queue!");
                        return;
                    }
                    string[] user = _s.reviewlist.ToArray();
                    Player who = _s.players.Find(user[0]);
                    if (who == null) {
                        p.SendMessage("Player " + user[0] + " doesn't exist or is offline. " + user[0] +
                                           " has been removed from the review queue");
                        _s.reviewlist.Remove(user[0]);
                        return;
                    }
                    if (who == p) {
                        p.SendMessage("You can't teleport to yourself! You have been removed from the review queue.");
                        _s.reviewlist.Remove(user[0]);
                        return;
                    }
                    _s.reviewlist.Remove(user[0]);
                    _s.commands.FindCommand("tp").Use(p, who.name);
                    p.SendMessage("You have been teleported to " + user[0]);
                    who.SendMessage("Your request has been answered by " + p.name + ".");
                    int toallplayerscount = 0;
                    foreach (string toallplayers in _s.reviewlist) {
                        Player who2 = _s.players.Find(toallplayers);
                        who2.SendMessage("The review queue has been rotated. you now have " + toallplayerscount.ToString() +
                                           " players waiting in front of you");
                        toallplayerscount++;
                    }
                }
                else {
                    p.SendMessage("&cYou have no permission to use the review queue!");
                }
                break;

            case "clear":
                if (p.IsConsole) {
                    _s.reviewlist.Clear();
                    p.SendMessage("The review queue has been cleared");
                    return;
                }
                Rank grc = _s.ranks.FindPerm(_s.props.reviewclear);

                if (grc == null) {
                    p.SendMessage("There is something wrong with the system.  A message has been sent to the admin to fix");
                    _s.GlobalMessageAdmins(p.name +
                                               " tryed to use /review, but a system error occurred. Make sure your groups are formatted correctly");
                    _s.GlobalMessageAdmins("The group permission that is messed up is: " + _s.props.reviewclear.ToString() + " (" +
                                               _s.props.reviewclear + ")");
                    return;
                }

                int lpc = grc.Permission;
                if (p.rank.Permission >= lpc) {
                    _s.reviewlist.Clear();
                    p.SendMessage("The review queue has been cleared");
                    return;
                }
                else {
                    p.SendMessage("&cYou have no permission to clear the Review Queue!");
                }
                break;
            default:
                Help(p);
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /review.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/review <enter/view/leave/next/clear> - Lets you enter, " +
                               "view, leave, clear the review list, or teleport you to the next player " +
                               "in the review queue.");
        }
    }
}

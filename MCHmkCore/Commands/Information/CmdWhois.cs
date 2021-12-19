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
    /// <summary>
    /// Implementation of the /whois command, which displays information about an online player.
    /// </summary>
    public class CmdWhois : Command {
        /// <summary>
        /// The list of keywords that are associated with /whois.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"who", "player", "info"});

        /// <summary>
        /// Gets the name of /whois.
        /// </summary>
        public override string Name {
            get {
                return "whois";
            }
        }

        /// <summary>
        /// Gets the shortcut for /whois.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /whois belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /whois. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /whois can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /whois.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        /// <summary>
        /// Constructs an instance of the /whois command.
        /// </summary>
        /// <param name="s"> The server that this instance of /whois will belong to.
        /// <seealso cref="Server"/></param>
        public CmdWhois(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /whois.
        /// </summary>
        /// <param name="p"> The player that used /whois. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            Player target = null;

            // If a player name is given, assume that the user's information should be shown, if possible.
            if (args.Length == 0) {
                if (!p.IsConsole) {
                    target = p;
                    args = p.name;
                }
                else {  // The console isn't really a player.
                    p.SendMessage("A player's name must be given when using /whois from the console.");
                    return;
                }
            }
            else {  // Otherwise, try to find an online player with a name containing the given string.
                target = _s.players.Find(args);
            }

            // It's not enough for the target player to be online. If that player is hidden, make sure the
            // user's rank is the same or greater than the target's rank, which is what /players does.
            if (target != null && (!target.hidden || p.rank.Permission >= target.rank.Permission)) {
                // Convert the string representing the player's total time spent on the server to a TimeSpan.
                string[] splitTotal = target.time.Split(' ');
                TimeSpan totalTime = new TimeSpan(int.Parse(splitTotal[0]), int.Parse(splitTotal[1]),
                                                  int.Parse(splitTotal[2]), int.Parse(splitTotal[3]));

                // Figure out how long the player has been on the server and convert it to a string.
                string currentTime = DateTime.Now.Subtract(target.timeLogged).ToString(@"hh\:mm\:ss");

                // Display all the information to the player.
                p.SendMessage(target.color + target.prefix + target.name + "'s" + _s.props.DefaultColor + " information:");
                p.SendMessage("> Current rank: " + target.rank.color + target.rank.name + _s.props.DefaultColor +
                              " | Map: &b" + target.level.name);
                p.SendMessage("> Blocks modified: &a" + target.overallBlocks.ToString() + _s.props.DefaultColor +
                    " | Since login: &a" + target.loginBlocks.ToString());
                p.SendMessage("> First logged in at: &a" + target.firstLogin.ToString("yyyy-MM-dd") + " " +
                    target.firstLogin.ToString("HH:mm:ss"));
                p.SendMessage("> Total time spent: &a" + totalTime.ToString(@"dd\:hh\:mm\:ss") + _s.props.DefaultColor +
                              " | Since login: &a" + currentTime);
                p.SendMessage("> Logins: &a" + target.totalLogins.ToString() + _s.props.DefaultColor +
                              " | Times kicked: &c" + target.totalKicked.ToString());
                p.SendMessage("> Deaths: &c" + target.overallDeath.ToString() + _s.props.DefaultColor +
                              " | Awards: " + Awards.awardAmount(target.name));

                // If these commands can't be used, then money is irrelevant to this server, so don't mention
                // anything about how much money the player has.
                bool moneyEnabled = !_s.commands.NobodyCanUse("pay") && !_s.commands.NobodyCanUse("give") &&
                                    !_s.commands.NobodyCanUse("take");
                if (moneyEnabled) {
                    p.SendMessage("> Money: &a" + target.money.ToString() + " " + _s.props.moneys);
                }

                // Only those with a high enough permission level can see IP addresses and whitelist status.
                if (p.IsConsole || p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                    string output = _s.bannedIP.Contains(target.ip) ? "&8" + target.ip + " (banned)" : "&f" + target.ip;
                    p.SendMessage("> IP address: " + output);

                    if (_s.props.useWhitelist && _s.whiteList.Contains(target.uuid)) {
                        p.SendMessage("> This player is &fwhitelisted");
                    }
                }
            }
            else {
                // Automatically use /whowas if the player isn't online or if the target player is hidden and has a
                // greater rank than the player using the command.
                p.SendMessage("'" + args + "' is offline. Using /whowas instead.");
                _s.commands.FindCommand("whowas").Use(p, args);
            }
        }

        /// <summary>
        /// Called when /help is used on /whois.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/whois [player?] - Displays info about an online player.");
            p.SendMessage("By default, your information will be shown.");
            p.SendMessage("If the player is offline, /whowas is used instead.");
        }
    }
}

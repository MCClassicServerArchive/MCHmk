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
using System.Collections.ObjectModel;
using System.Data;

using MCHmk.SQL;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /whowas command, which displays information about an offline player.
    /// </summary>
    public class CmdWhowas : Command {
        /// <summary>
        /// The list of keywords that are associated with /whowas.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"who", "player", "info", "offline"});

        /// <summary>
        /// Gets the name of /whowas.
        /// </summary>
        public override string Name {
            get {
                return "whowas";
            }
        }

        /// <summary>
        /// Gets the shortcut for /whowas.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /whowas belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /whowas. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /whowas can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /whowas.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        /// <summary>
        /// Constructs an instance of the /whowas command.
        /// </summary>
        /// <param name="s"> The server that this instance of /whowas will belong to.
        /// <seealso cref="Server"/></param>
        public CmdWhowas(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /whowas.
        /// </summary>
        /// <param name="p"> The player that used /whowas. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            if (args.Length == 0) {
                Help(p);
                return;
            }

            // If /whois can be used, use that command instead. This allows the database to not be queried.
            Player target = _s.players.Find(args);
            if (EligibleForWhois(p, args, target)) {
                p.SendMessage(target.ColoredName + " is online. Using /whois instead.");
                _s.commands.FindCommand("whois").Use(p, args);
                return;
            }

            if (!Player.ValidName(args)) {
                p.SendMessage("Invalid player name.");
                return;
            }

            // Send a SQL query to the database.
            string name = args;
            string query = "SELECT Name, IP, FirstLogin, LastLogin, totalLogin, Title, TotalDeaths, Money, " +
                "totalBlocks, totalKicked, TimeSpent, color, title_color FROM Players WHERE Name=@name";

            using (PreparedStatement stmt = new PreparedStatement(_s.database, query)) {
                stmt.AddParam("@name", name);

                using (DataTable data = stmt.ObtainData()) {
                    if (data.Rows.Count == 0) {
                        p.SendMessage("'" + name + "' has never visited the server.");
                        return;
                    }
                    // Make the typing easier.
                    DataRow pRow = data.Rows[0];

                    // This assignment is done to obtain the proper capitalization of the name since an all-lowercase
                    // name might have been provided by the user.
                    name = pRow["Name"].ToString();

                    // Get the player's uuid and figure out their rank so that we can display it.
                    Uuid uuid = Uuid.FindUuid(_s.database, name);
                    if (!uuid.IsValid) {  // I hope this never happens.
                        p.SendMessage("An error occurred.");
                        return;
                    }
                    Rank targetRank = _s.ranks.FindPlayerRank(uuid);

                    // Put together the prefix that would have been displayed in-game from the information that
                    // was retrieved.
                    string title = pRow["Title"].ToString();
                    string tColor = Colors.Parse(pRow["title_color"].ToString());
                    string nameColor = Colors.Parse(pRow["color"].ToString());
                    if (nameColor.Length == 0) {  // If no custom color was given, use their rank's color.
                        nameColor = targetRank.color;
                    }

                    // TODO: taken from Player.SetPrefix(), should be its own function really
                    string prefix = (title == String.Empty) ? String.Empty : nameColor + "[" + tColor + title + nameColor + "] ";

                    // Display the information about the offline player.
                    p.SendMessage(prefix + nameColor + name + "'s" + _s.props.DefaultColor + " information:");
                    p.SendMessage("> Current rank: " + targetRank.color + targetRank.name);
                    p.SendMessage("> Blocks modified: &a" + pRow["totalBlocks"].ToString());
                    p.SendMessage("> First logged in at: &a" + pRow["FirstLogin"]);
                    p.SendMessage("> Last seen: &a" + pRow["LastLogin"]);

                    // MCLawl and old versions of MCForge didn't keep track of total time played, so the
                    // TimeSpent field is the empty string for players that haven't logged in since that
                    // column was added to the Players table. Thus, avoid an exception if that player hasn't
                    // logged on since that feature was added.
                    if (pRow["TimeSpent"].ToString().Length != 0) {
                        string[] splitTotal = pRow["TimeSpent"].ToString().Split(' ');
                        TimeSpan totalTime = new TimeSpan(int.Parse(splitTotal[0]), int.Parse(splitTotal[1]),
                                                          int.Parse(splitTotal[2]), int.Parse(splitTotal[3]));

                        p.SendMessage("> Total time spent: &a" + totalTime.ToString(@"dd\:hh\:mm\:ss"));
                    }

                    p.SendMessage("> Logins: &a" + pRow["totalLogin"].ToString() + _s.props.DefaultColor +
                                  " | Times kicked: &c" + pRow["totalKicked"].ToString());
                    p.SendMessage("> Deaths: &c" + pRow["TotalDeaths"].ToString() + _s.props.DefaultColor +
                                  " | Awards: " + Awards.awardAmount(name));

                    // If these commands can't be used, then money is irrelevant to this server, so don't mention
                    // anything about how much money the player has.
                    bool moneyEnabled = !_s.commands.NobodyCanUse("pay") && !_s.commands.NobodyCanUse("give") &&
                                        !_s.commands.NobodyCanUse("take");
                    if (moneyEnabled) {
                        p.SendMessage("> Money: &a" + pRow["Money"].ToString() + " " + _s.props.moneys);
                    }

                    // Only those with a high enough permission level can see IP addresses and whitelist status.
                    if (p.IsConsole || p.rank.Permission >= _s.commands.GetOtherPerm(this)) {
                        string ip = pRow["IP"].ToString();
                        string output = _s.bannedIP.Contains(ip) ? "&8" + ip + " (banned)" : "&f" + ip;
                        p.SendMessage("> IP address: " + output);

                        if (_s.props.useWhitelist && _s.whiteList.Contains(uuid)) {
                            p.SendMessage("> This player is &fwhitelisted");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if /whois should be used on the given player instead of /whowas.
        /// </summary>
        /// <param name="user"> The player using the command. </param>
        /// <param name="givenName"> The name given. </param>
        /// <param name="target"> The target player. Can be null if the player wasn't found. </param>
        /// <returns> Whether /whois should be used. </returns>
        private bool EligibleForWhois(Player user, string givenName, Player target) {
            // Only use /whois instead of /whowas if a player was found, if the name matches exactly,
            // and if the player can currently see the target when /players is used. For the last
            // condition, hidden players can't be seen in /players unless the user of the command is
            // of equal or greater rank to the target player.
            return target != null && givenName.ToLower() == target.name.ToLower() && 
                (!target.hidden || user.rank.Permission >= target.rank.Permission);
        }

        /// <summary>
        /// Called when /help is used on /whowas.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/whowas <player?> - Displays info about an offline player.");
            p.SendMessage("If an online player has the exact name, /whois is used instead.");
            p.SendMessage("Append a + to view info about ClassiCube accounts.");
            p.SendMessage("Append a ~ to view info about unpaid Minecraft accounts.");
        }
    }
}

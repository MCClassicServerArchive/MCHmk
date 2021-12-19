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
    /// Implementation of the /unban command, which unbans a player.
    /// </summary>
    public class CmdUnban : Command {
        /// <summary>
        /// The list of keywords that are associated with /unban.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"undo", "ban", "kick", "mod"});

        /// <summary>
        /// Gets the name of /unban.
        /// </summary>
        public override string Name {
            get {
                return "unban";
            }
        }

        /// <summary>
        /// Gets the shortcut for /unban.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /unban belongs to.
        /// </summary>
        public override string Type {
            get {
                return "mod";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /unban. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /unban can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /unban.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }
        /// <summary>
        /// Constructs an instance of the /unban command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /unban will belong to. <seealso cref="Server"/></param>
        public CmdUnban(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /unban.
        /// </summary>
        /// <param name="p"> The player that used /unban. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            if (args.Length == 0) {
                Help(p);
                return;
            }

            // Figure out if a total unban is desired.
            bool totalUnban = false;
            if (args[0] == '@') {
                totalUnban = true;
                args = args.Remove(0, 1).Trim();
            }

            string givenName = args;
            // Stop if the name isn't valid.
            if (!Player.ValidName(givenName)) {
                p.SendMessage("Invalid name '" + args + "'.");
                return;
            }

            // Save some typing and reduce the amount of if-else blocks by determining whether a player or the
            // console used the command.
            string cmdUser = !p.IsConsole ? p.color + p.name : "the Console";
            string cmdUserNoColor = !p.IsConsole ? p.name : "the Console";

            // Check if the player to be banned is online, and if so, get their Player object so that their
            // fields can be modified instantly. This returns null if the player is offline.
            Player target = _s.players.Find(givenName);
            bool online = (target != null);

            // If the player is online, a partial name might have been given, so ensure that the full
            // name is used. Otherwise, use the provided name.
            string targetName = online ? target.name : givenName;

            // Figure out which rank is the 'banned' rank.
            Uuid targetUuid = online ? target.uuid : Uuid.FindWithFallback(_s.database, givenName);
            Rank targetRank =  online ? target.rank : _s.ranks.FindPlayerRank(targetUuid);
            Rank bannedRank = _s.ranks.FindPerm(DefaultRankValue.Banned);

            // Tempbans don't assign the banned rank to the player, so check for those if a player is
            // not of the 'banned' rank before concluding that the player is indeed not banned.
            if (targetRank != bannedRank) {
                if (HandleTempBans(cmdUser, cmdUserNoColor, targetName)) {
                    return;
                }

                p.SendMessage("Player is not banned.");
                return;
            }

            // Remove the player from the banlist.
            bannedRank.playerList.Remove(targetUuid);
            bannedRank.playerList.Save();
            _s.logger.Log("SAVED: ranks/" + bannedRank.fileName);

            // Change an online player's color and rank, then respawn the player so that the name above
            // their character changes color for everyone.
            if (online) {
                target.rank = _s.ranks.DefaultRank;
                target.color = target.rank.color;
                _s.GlobalDie(target, false);
                _s.GlobalSpawn(target, target.pos[0], target.pos[1], target.pos[2],
                               target.rot[0], target.rot[1], false);
            }

            // Announce the unban.
            _s.GlobalMessage(targetName + _s.props.DefaultColor + " was &funbanned" +
                             _s.props.DefaultColor + " by " + cmdUser + _s.props.DefaultColor + ".");
            _s.logger.Log("UNBANNED: " + targetName + " by " + cmdUserNoColor);
            _s.IRC.Say(targetName + " was unbanned by " + cmdUserNoColor + ".");

            // Do a total unban if requested.
            if (totalUnban) {
                _s.commands.FindCommand("unbanip").Use(p, "@" + args);
            }
        }

        /// <summary>
        /// Removes a temporary ban given to a player if it exists.
        /// </summary>
        /// <param name="user"> The name of the player using the command. </param>
        /// <param name="userNoColor"> The uncolored name of the player using the command. </param>
        /// <param name="target"> The name of the player to be unbanned. </param>
        /// <returns> Whether a temporary ban was found and removed. </returns>
        private bool HandleTempBans(string user, string userNoColor, string target) {
            foreach (Server.TempBan tban in _s.tempBans) {
                if (tban.name.ToLower() == target.ToLower()) {
                    _s.tempBans.Remove(tban);
                    _s.GlobalMessage(target + " has had their temporary ban lifted by " + user + _s.props.DefaultColor + ".");
                    _s.logger.Log("UNBANNED: " + target + " by " + userNoColor);
                    _s.IRC.Say(target + " was unbanned by " + userNoColor + ".");

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when /help is used on /unban.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/unban <player?> - Unbans a player. Also removes temporary bans.");
            p.SendMessage("Add a @ before the name to perform a total unban.");
        }
    }
}

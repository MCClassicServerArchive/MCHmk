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

// TODO: move ban code to a centralized location so that other commands may use it without having to do
// the annoying find command -> use(null) idiom

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /ban command, which bans a player.
    /// </summary>
    public class CmdBan : Command {
        /// <summary>
        /// The list of keywords that are associated with /ban.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"kick", "mod", "punish"});

        /// <summary>
        /// Gets the name of /ban.
        /// </summary>
        public override string Name {
            get {
                return "ban";
            }
        }

        /// <summary>
        /// Gets the shortcut for /ban.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /ban belongs to.
        /// </summary>
        public override string Type {
            get {
                return "mod";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /ban. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /ban can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /ban.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }

        /// <summary>
        /// Constructs an instance of the /ban command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /ban will belong to. <seealso cref="Server"/></param>
        public CmdBan(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /ban.
        /// </summary>
        /// <param name="p"> The player that used /ban. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            if (args.Length == 0) {
                Help(p);
                return;
            }

            // Figure out if a stealth or total ban is desired.
            bool stealth = false;
            bool totalBan = false;

            if (args[0] == '#') {
                stealth = true;
                args = args.Remove(0, 1).Trim();
            }
            else if (args[0] == '@') {
                totalBan = true;
                args = args.Remove(0, 1).Trim();
            }

            string givenName = args;
            // Stop if the name isn't valid.
            if (!Player.ValidName(givenName)) {
                p.SendMessage("Invalid name '" + givenName + "'.");
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

            // Get the player's uuid, the player's current rank, and the 'banned' rank.
            Uuid targetUuid = online ? target.uuid : Uuid.FindWithFallback(_s.database, givenName);
            Rank targetRank =  online ? target.rank : _s.ranks.FindPlayerRank(targetUuid);
            Rank bannedRank = _s.ranks.FindPerm(DefaultRankValue.Banned);

            // Check that the user of this command can ban the target player.
            if (!RankCheck(p, targetName, targetRank)) {
                return;
            }

            // Switch the player's rank to the banned one.
            targetRank.playerList.Remove(targetUuid);
            targetRank.playerList.Save();
            _s.logger.Log("SAVED: ranks/" + targetRank.fileName);

            bannedRank.playerList.Add(targetUuid, targetName);
            bannedRank.playerList.Save();
            _s.logger.Log("SAVED: ranks/" + bannedRank.fileName);

            // Announce the ban to the server.
            if (!online) {
                _s.GlobalMessage(targetName + " &f(offline)" + _s.props.DefaultColor + " was &8banned" +
                                 _s.props.DefaultColor + " by " + cmdUser + _s.props.DefaultColor + ".");
            }
            else {
                // Change an online player's rank and color.
                string oldRankColor = target.rank.color;
                target.rank = bannedRank;
                target.color = bannedRank.color;

                // Only ops should know about stealth bans.
                if (stealth) {
                    _s.GlobalMessageOps(oldRankColor + target.name + _s.props.DefaultColor +
                                        " was stealth &8banned" + _s.props.DefaultColor + " by " + cmdUser +
                                        _s.props.DefaultColor + "!");
                }
                else {
                    _s.GlobalMessage(oldRankColor + target.name + _s.props.DefaultColor + " was &8banned" +
                                     _s.props.DefaultColor + " by " + cmdUser + _s.props.DefaultColor + "!");
                }

                // Respawn the player so that the color of the name above their head changes for everyone else.
                _s.GlobalDie(target, false);
                _s.GlobalSpawn(target, target.pos[0], target.pos[1], target.pos[2],
                               target.rot[0], target.rot[1], false);
            }

            // Announce the ban on IRC and in the console.
            if (!stealth) {
                _s.IRC.Say(targetName + " was banned by " + cmdUserNoColor + ".");
            }
            _s.logger.Log("BANNED: " + targetName + " by " + cmdUserNoColor);

            // Total bans also undo and ban a player's IP address.
            if (totalBan) {
                _s.commands.FindCommand("undo").Use(p, givenName + " 0");
                _s.commands.FindCommand("banip").Use(p, "@ " + givenName);
            }
        }

        /// <summary>
        /// Checks if a player has a high enough rank to ban another player, and sends a message to the player
        /// using the command if they cannot ban the target player.
        /// </summary>
        /// <param name="p"> The player using /ban. <seealso cref="Player"/></param>
        /// <param name="targetName"> The name of the player to be banned. </param>
        /// <param name="targetRank"> The rank of the player to be banned. <seealso cref="Rank"/></param>
        /// <returns> Whether the target player can be banned by the player using /ban. </returns>
        private bool RankCheck(Player p, string targetName, Rank targetRank) {
            // Save a bit of typing.
            int additionalPermValue = _s.commands.GetOtherPerm(this);

            // Don't ban players that meet or exceed the rank for ban immunity specified in the additional
            // permissions file.
            if (targetRank.Permission >= additionalPermValue) {
                // Display a rank name if possible, and the number otherwise.
                // TODO: These two lines could be a function maybe
                Rank temp = _s.ranks.FindPermInt(additionalPermValue);
                string display = temp != null ? temp.name : additionalPermValue.ToString();

                p.SendMessage("You can't ban players ranked " + display + " or higher!");
                return false;
            }
            // Don't ban players that are already banned.
            else if (targetRank.Permission == DefaultRankValue.Banned) {
                p.SendMessage(targetName + " is already banned.");
                return false;
            }
            // Don't ban players with a rank higher or equal to those using /ban. The console is exempt from this.
            else if (!p.IsConsole && targetRank.Permission >= p.rank.Permission) {
                p.SendMessage("You cannot ban a person ranked equal or higher than you.");
                return false;
            }

            // Otherwise, it is okay to ban the player.
            return true;
        }

        /// <summary>
        /// Called when /help is used on /ban.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/ban <player?> - Bans a player. Does not kick.");
            p.SendMessage("Add a # before the name to perform a stealth ban.");
            p.SendMessage("Add a @ before the name to perform a total ban.");
        }
    }
}

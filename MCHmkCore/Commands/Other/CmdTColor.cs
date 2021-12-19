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
	Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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

using MCHmk.SQL;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /tcolor command, which changes the color of a player's title.
    /// </summary>
    public class CmdTColor : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"title", "color", "set"});

        /// <summary>
        /// Gets the name of /tcolor.
        /// </summary>
        public override string Name {
            get {
                return "tcolor";
            }
        }

        /// <summary>
        /// Gets the shortcut for /tcolor.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /tcolor belongs to.
        /// </summary>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /tcolor. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /tcolor can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /tcolor.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }

        /// <summary>
        /// Constructs an instance of the /tcolor command.
        /// </summary>
        /// <param name="s"> The server that this instance of /tcolor will belong to.
        /// <seealso cref="Server"/></param>
        public CmdTColor(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /tcolor.
        /// </summary>
        /// <param name="p"> The player that used /tcolor. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // Check that the user provided an appropriate number of arguments, and obtain them if that's the case.
            if (args.Length == 0) {
                Help(p);
                return;
            }

            string[] splitArgs = args.Split(' ');
            if (splitArgs.Length > 2) {
                p.SendMessage("Too many options provided.");
                return;
            }

            // If only one argument is provided, attempt to change the title color of the player that used the command.
            if (splitArgs.Length == 1) {
                if (p.IsConsole) {
                    p.SendMessage("/tcolor requires a player name when used from the console.");
                    return;
                }

                string newColor = splitArgs[0];
                if (VerifyColor(p, p, newColor)) {  // The user and target are the same player.
                    ChangeTitleColor(p, newColor);
                }
            }
            else { // Both arguments are provided, so change the color of another player if possible.
                string targetName = splitArgs[0];
                string newColor = splitArgs[1];

                // /tcolor should only work on online players.
                Player target = _s.players.Find(targetName);
                if (target == null) {
                    p.SendMessage("The player named '" + targetName + "' isn't online.");
                    return;
                }

                // Perform the usual rank check.
                if (!p.IsConsole && target.rank.Permission > p.rank.Permission) {
                    p.SendMessage("You cannot change the title color of someone ranked higher than you.");
                    return;
                }

                if (VerifyColor(p, target, newColor)) {
                    ChangeTitleColor(target, newColor);

                    // The console won't see the message that is sent to players in-game, so send this message to
                    // let them know that the color change was successful.
                    if (p.IsConsole) {
                        p.SendMessage(PossessiveName(target.name) + " title color was changed to " + newColor + ".");
                    }
                }
            }
        }

        /// <summary>
        /// Given a color's name, ensure that it represents a valid color and that the target player's title
        /// doesn't already have that color.
        /// </summary>
        /// <param name="user"> The player using the command. <seealso cref="Player"/></param>
        /// <param name="target"> The player whose title color is being changed. Can be the same as the player
        /// using the command. <seealso cref="Player"/></param>
        /// <param name="colorName"> A string presenting the name of the color. </param>
        /// <returns> True if the color name is valid and can be applied to the target's title,
        /// false otherwise. </returns>
        private bool VerifyColor(Player user, Player target, string colorName) {
            string colorCode = Colors.Parse(colorName);  // %0, %1, etc.
            string pronoun = user == target ? "Your" : "Their";

            // Check if a valid color name is given and if the target player doesn't already
            // have that color for their title.
            if (colorCode.Length == 0 && colorName != "del") {
                user.SendMessage("Invalid color '" + colorName + "' given.");
                return false;
            }
            if (target.titlecolor == colorCode) {
                user.SendMessage(pronoun + " title is already that color.");
                return false;
            }
            // We're good to go.
            return true;
        }

        /// <summary>
        /// Changes the title color of a player's name.
        /// </summary>
        /// <param name="target"> The player whose title color should be changed. <seealso cref="Player"/></param>
        /// <param name="colorName"> A valid color name. </param>
        private void ChangeTitleColor(Player target, string colorName) {
            // Update the SQL database. Note that an empty string indicates that no custom title color is assigned.
            string query = "UPDATE Players SET title_color = @color WHERE name = @name";
            using (PreparedStatement stmt = _s.database.MakePreparedStatement(query)) {
                stmt.AddParam("@color", colorName == "del" ? String.Empty : colorName.ToLower());
                stmt.AddParam("@name", target.name);
                stmt.Execute();
            }

            // If the player's custom title color is to be removed, change their title color to an empty string.
            // This is different from /color, which uses their rank's color if "del" is provied.
            // Otherwise, use the given color.
            string colorCode = colorName == "del" ? String.Empty : Colors.Parse(colorName);
            if (colorName == "del") {
                _s.GlobalChat(target, target.color + "*" + PossessiveName(target.name) + _s.props.DefaultColor +
                                  " title color was removed.", false);
            }
            else {
                _s.GlobalChat(target, target.color + "*" + PossessiveName(target.name) + _s.props.DefaultColor +
                                  " title color changed to " + colorCode + colorName + _s.props.DefaultColor + ".", false);
            }
            target.titlecolor = colorCode;
            target.SetPrefix();

            // Respawn the player so that the changed color shows up for everyone else.
            _s.GlobalDie(target, false);
            _s.GlobalSpawn(target, target.pos[0], target.pos[1], target.pos[2], target.rot[0], target.rot[1], false);
        }

        /// <summary>
        /// Obtains the possessive form of the given name.
        /// </summary>
        /// <param name="name"> The name of a player. </param>
        /// <returns> The possessive form of the player's name. </returns>
        private string PossessiveName(string name) {
            char ch = name[name.Length - 1];
            if (ch == 's' || ch == 'x') {
                return name + "'";
            }
            else {
                return name + "'s";
            }
        }

        /// <summary>
        /// Called when /help is used on /tcolor.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/tcolor [player?] <color?/del>- Changes the color of a player's title " +
                               "or resets a title's color if the 'del' option is given.");
            p.SendMessage("If a player is not given, the default player is yourself.");
            p.SendMessage("Valid colors:");
            p.SendMessage("&0black &1navy &2green &3teal &4maroon &5purple &6gold &7silver");
            p.SendMessage("&8gray &9blue &alime &baqua &cred &dpink &eyellow &fwhite");
        }
    }
}

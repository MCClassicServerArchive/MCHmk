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

using MCHmk.SQL;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /color command, which changes the color of a player's name.
    /// </summary>
    public class CmdColor : Command {
        /// <summary>
        /// The list of keywords that are associated with /color.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"user", "name"});

        /// <summary>
        /// Gets the name of /color.
        /// </summary>
        public override string Name {
            get {
                return "color";
            }
        }

        /// <summary>
        /// Gets the shortcut for /color.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /color belongs to.
        /// </summary>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /color. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /color can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /color.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }

        /// <summary>
        /// Constructs an instance of the /color command.
        /// </summary>
        /// <param name="s"> The server that this instance of /color will belong to.
        /// <seealso cref="Server"/></param>
        public CmdColor(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /color.
        /// </summary>
        /// <param name="p"> The player that used /color. <seealso cref="Player"/></param>
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

            // If only one argument is provided, attempt to change the color of the player that used the command.
            if (splitArgs.Length == 1) {
                if (p.IsConsole) {
                    p.SendMessage("/color requires a player name when used from the console.");
                    return;
                }

                string newColor = splitArgs[0];
                if (VerifyColor(p, p, newColor)) {  // The user and target are the same player.
                    ChangeColor(p, newColor);
                }
            }
            else { // Both arguments are provided, so change the color of another player if possible.
                string targetName = splitArgs[0];
                string newColor = splitArgs[1];

                // /color should only work on online players.
                Player target = _s.players.Find(targetName);
                if (target == null) {
                    p.SendMessage("The player named '" + targetName + "' isn't online.");
                    return;
                }

                // Perform the usual rank check.
                if (!p.IsConsole && target.rank.Permission > p.rank.Permission) {
                    p.SendMessage("You cannot change the color of someone ranked higher than you.");
                    return;
                }

                if (VerifyColor(p, target, newColor)) {
                    ChangeColor(target, newColor);

                    // The console won't see the message that is sent to players in-game, so send this message to
                    // let them know that the color change was successful.
                    if (p.IsConsole) {
                        p.SendMessage(PossessiveName(target.name) + " color was changed to " + newColor + ".");
                    }
                }
            }
        }

        /// <summary>
        /// Given a color's name, ensure that it represents a valid color and that the target player's name
        /// doesn't already have that color.
        /// </summary>
        /// <param name="user"> The player using the command. <seealso cref="Player"/></param>
        /// <param name="target"> The player whose color is being changed. Can be the same as the player
        /// using the command. <seealso cref="Player"/></param>
        /// <param name="colorName"> A string presenting the name of the color. </param>
        /// <returns> True if the color name is valid and can be applied to the target, false otherwise. </returns>
        private bool VerifyColor(Player user, Player target, string colorName) {
            string colorCode = Colors.Parse(colorName);  // &0, &1, etc.
            string pronoun = user == target ? "Your" : "Their";

            // Check if a valid color name is given and if the target player doesn't already
            // have that color.
            if (colorCode.Length == 0 && colorName != "del") {
                user.SendMessage("Invalid color '" + colorName + "' given.");
                return false;
            }
            if (target.color == colorCode) {
                user.SendMessage(pronoun + " name is already that color.");
                return false;
            }
            // We're good to go.
            return true;
        }

        /// <summary>
        /// Changes the color of a player's name.
        /// </summary>
        /// <param name="target"> The player whose name's color should be changed. <seealso cref="Player"/></param>
        /// <param name="colorName"> A valid color name. </param>
        private void ChangeColor(Player target, string colorName) {
            // Update the SQL database. Note that an empty string indicates that no custom color is assigned.
            string query = "UPDATE Players SET color = @color WHERE name = @name";
            using (PreparedStatement stmt = _s.database.MakePreparedStatement(query)) {
                stmt.AddParam("@color", colorName == "del" ? String.Empty : colorName.ToLower());
                stmt.AddParam("@name", target.name);
                stmt.Execute();
            }

            // If the player's custom color is to be removed, change their color to their rank's color.
            // Otherwise, use the given color.
            string colorCode = colorName == "del" ? target.rank.color : Colors.Parse(colorName);
            if (colorName == "del") {
                _s.GlobalChat(target, target.color + "*" + PossessiveName(target.name) + _s.props.DefaultColor + 
                                  " color reverted to " + target.rank.color + "their group's default" +
                                  _s.props.DefaultColor + ".", false);
            }
            else {
                _s.GlobalChat(target, target.color + "*" + PossessiveName(target.name) + _s.props.DefaultColor +
                                  " color changed to " + colorCode + colorName + _s.props.DefaultColor + ".", false);
            }
            target.color = colorCode;
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
        /// Called when /help is used on /color.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/color [player?] <color?/del>- Changes the color of a player's name " +
                               "or resets a name's color if the 'del' option is given.");
            p.SendMessage("If a player is not given, the default player is yourself.");
            p.SendMessage("Valid colors:");
            p.SendMessage("&0black &1navy &2green &3teal &4maroon &5purple &6gold &7silver");
            p.SendMessage("&8gray &9blue &alime &baqua &cred &dpink &eyellow &fwhite");
        }
    }
}

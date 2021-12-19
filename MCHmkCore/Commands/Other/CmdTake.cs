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
    /// Implementation of the /take command, which takes money away from a player.
    /// </summary>
    public class CmdTake : Command {
        /// <summary>
        /// The list of keywords that are associated with /take.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"get", "money"});

        /// <summary>
        /// Gets the name of /take.
        /// </summary>
        public override string Name {
            get {
                return "take";
            }
        }

        /// <summary>
        /// Gets the shortcut for /take.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /take belongs to.
        /// </summary>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /take. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /take can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /take.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Admin;
            }
        }

        /// <summary>
        /// Constructs an instance of the /take command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /take will belong to. <seealso cref="Server"/></param>
        public CmdTake(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /take.
        /// </summary>
        /// <param name="p"> The player that used /take. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            string[] splitArgs = args.Split(' ');

            // Stop if not enough information was given.
            if (splitArgs.Length < 2) {
                Help(p);
                return;
            }

            // Find the target player.
            Player recipient = _s.players.Find(splitArgs[0]);

            // Make sure the target player is online.
            if (recipient == null) {
                p.SendMessage("The player you entered is either invalid or not online.");
            }
            // Prevent the player from using /take on themselves.
            else if (recipient == p) {
                p.SendMessage("You can't take " + _s.props.moneys + " from yourself.");
            }
            else {
                try {
                    int amountTaken = Int32.Parse(splitArgs[1]);

                    // Don't take negative amounts.
                    if (amountTaken < 0) {
                        p.SendMessage("Can't take a negative amount of " + _s.props.moneys + ".");
                    }
                    // Make sure that the target will have at least 0 moneys after this command.
                    else if (checked(recipient.money - amountTaken) < 0) {
                        p.SendMessage("Players can't have less than 0 " + _s.props.moneys + ".");
                    }
                    // Carry out the robbery; the "checked" keyword prevents overflow.
                    else {
                        recipient.money = checked(recipient.money - amountTaken);
                        _s.GlobalMessage(recipient.ColoredName + " was rattled down for " +
                                             amountTaken.ToString() + " " + _s.props.moneys + ".");
                    }
                }
                catch (OverflowException) { // This makes the limit basically 2^31.
                    p.SendMessage("That amount of " + _s.props.moneys + " is too high.");
                }
                catch (FormatException) {
                    p.SendMessage("That is not a number.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /take.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/take <player?> <amount?> - Takes some " + _s.props.moneys + " away from a player.");
        }
    }
}

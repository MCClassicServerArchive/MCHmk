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
using System.Globalization;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /pay command, which pays another player a certain amount of money.
    /// </summary>
    public class CmdPay : Command {
        /// <summary>
        /// The list of keywords that are associated with /pay.
        /// </summary>
        private string[] _keywords = new string[] {"money", "give", String.Empty};

        /// <summary>
        /// Gets the name of /pay.
        /// </summary>
        public override string Name {
            get {
                return "pay";
            }
        }

        /// <summary>
        /// Gets the shortcut for /pay.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /pay belongs to.
        /// </summary>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /pay. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                // Crappy hack to set the proper value at runtime -Jjp137
                _keywords[2] = _s.props.moneys;

                return Array.AsReadOnly<string>(_keywords);
            }
        }

        /// <summary>
        /// Gets whether /pay can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /pay.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        /// <summary>
        /// Constructs an instance of the /pay command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /pay will belong to. <seealso cref="Server"/></param>
        public CmdPay(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /pay.
        /// </summary>
        /// <param name="p"> The player that used /pay. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            string[] splitArgs = args.Split(' ');

            // Stop if not enough information was given.
            if (splitArgs.Length < 2) {
                Help(p);
                return;
            }
            // The console has no money so it should just use /give instead.
            else if (p.IsConsole) {
                p.SendMessage("Use /give instead. /pay is for in-game players only.");
                return;
            }

            // Find the target player.
            Player recipient = _s.players.Find(splitArgs[0]);

            // Make sure the target player is online.
            if (recipient == null) {
                p.SendMessage("The player you entered is either invalid or not online.");
            }
            // Prevent the player from using /pay on themselves.
            else if (recipient == p) {
                p.SendMessage("You can't pay " + _s.props.moneys + " to yourself.");
            }
            else {
                try {
                    int amountGiven = Int32.Parse(splitArgs[1]);

                    // Don't pay negative amounts.
                    if (amountGiven < 0) { 
                        p.SendMessage("Can't pay a negative amount of "
                                           + _s.props.moneys + ".");
                    }
                    // Don't pay if the player doesn't have enough money.
                    else if (checked(p.money - amountGiven) < 0) { 
                        p.SendMessage("You don't have enough " + _s.props.moneys + ".");
                    }
                    // Carry out the payment; the "checked" keyword prevent overflow.
                    else { 
                        recipient.money = checked(recipient.money + amountGiven);
                        p.money = checked(p.money - amountGiven);
                        _s.GlobalMessage(p.ColoredName + " paid " + recipient.ColoredName +
                                             " " + amountGiven.ToString() + " " + _s.props.moneys + ".");
                    }
                }
                catch (OverflowException) { // This makes the limit basically 2^31.
                    p.SendMessage("That would give that player too much " + _s.props.moneys + ".");
                }
                catch (FormatException) {
                    p.SendMessage("That is not a number.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /pay.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/pay <player?> <amount?> - Pays some " + _s.props.moneys + " to a player.");
        }
    }
}

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
    /// Implementation of the /fakepay command, which pretends to pay another player.
    /// </summary>
    public class CmdFakePay : Command {
        /// <summary>
        /// The list of keywords that are associated with /fakepay.
        /// </summary>
        private string[] _keywords = new string[] {"fake", "troll", "pay", String.Empty};

        /// <summary>
        /// Gets the name of /fakepay.
        /// </summary>
        public override string Name {
            get {
                return "fakepay";
            }
        }

        /// <summary>
        /// Gets the shortcut for /fakepay.
        /// </summary>
        public override string Shortcut {
            get {
                return "fpay";
            }
        }

        /// <summary>
        /// Gets the category that /fakepay belongs to.
        /// </summary>
        /// <value>The type.</value>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /fakepay. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                // Crappy hack to set the proper value at runtime -Jjp137
                _keywords[3] = _s.props.moneys;

                return Array.AsReadOnly<string>(_keywords);
            }
        }

        /// <summary>
        /// Gets whether /fakepay can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default permission value for /fakepay.
        /// </summary>
        /// <value>The default rank.</value>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Operator;
            }
        }

        /// <summary>
        /// Constructs an instance of the /fakepay command.
        /// </summary>
        /// <param name="s"> The server that this instance of /fakepay will belong to.
        /// <seealso cref="Server"/></param>
        public CmdFakePay(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /fakepay.
        /// </summary>
        /// <param name="p"> The player that used /fakepay. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // Check that the input is valid.
            string[] cmdParams = args.Split(' ');
            if (cmdParams.Length < 2) {
                Help(p);
                return;
            }

            // Offline players and the player using the command are not valid targets.
            Player target = _s.players.Find(cmdParams[0]);
            if (target == null) {
                p.SendMessage("The player you entered is either invalid or not online.");
                return;
            }
            if (target == p) { 
                p.SendMessage("You can't fakepay " + _s.props.moneys + " to yourself.");
                return;
            }

            // Pretend to give money if the number given is valid.
            try {
                int amountGiven = Int32.Parse(cmdParams[1]);
                if (amountGiven < 0) {
                    p.SendMessage("Can't fakepay a negative amount of " + _s.props.moneys + ".");
                }

                string giver = p.IsConsole ? "The Console" : p.ColoredName;
                _s.GlobalMessage(giver + " paid " + target.ColoredName + " " + amountGiven.ToString() + " " +
                                 _s.props.moneys + ".");
            }
            catch (OverflowException) { // This makes the limit basically 2^31
                p.SendMessage("That would give that player too much " + _s.props.moneys +
                              " if this was a real payment.");
            }
            catch (FormatException) {
                p.SendMessage("That is not a number.");
            }
        }

        /// <summary>
        /// Called when /help is used on /fakepay.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/fakepay <player?> <amount?> - Pretends to pay another player.");
        }
    }
}


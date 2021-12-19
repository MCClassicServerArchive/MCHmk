/*
    Copyright 2016 Jjp137/LegoBricker

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

using System;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    /// <summary>
    /// The code for the /khide command, which hides another player. To everyone else, it appears
    /// as if that player had been kicked.
    /// </summary>
    public class CmdKHide : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"kick", "hidden", "invisible"});

        public override string Name {
            get {
                return "khide";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "mod";
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
                return DefaultRankValue.Operator;
            }
        }

        public CmdKHide(Server s) : base(s) { }

        /// <summary>
        /// The code that runs when /khide is called.
        /// </summary>
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }
            args = args.Split(' ')[0];
            Player who = _s.players.Find(args);
            // If blocks for the input.
            if (who == null) {
                p.SendMessage("Could not find player.");
                return;
            }
            if (who == p) {
                p.SendMessage("Hmk. That's dumb. Use /hide instead.");
                return;
            }
            if (who.rank.Permission >= p.rank.Permission) {
                p.SendMessage("Cannot use this on someone of equal or greater rank.");
                return;
            }
            // If everything is all-ok, this happens.
            else {
                Command opchat = _s.commands.FindCommand("opchat");
                Command adminchat = _s.commands.FindCommand("adminchat");
                who.hidden = !who.hidden;
                //Now that the hidden state is inverted, it displays messages.
                if (who.hidden) {
                    _s.GlobalDie(who, true);
                    _s.GlobalMessageOps("To Ops -" + who.color + who.name + "-" + _s.props.DefaultColor + " is now &finvisible" + _s.props.DefaultColor + ".");
                    _s.GlobalChat(p, "&c- " + who.color + who.prefix + who.name + _s.props.DefaultColor + " left the game (You were kicked by " + p.name +"!)", false);
                    _s.IRC.Say(who.name + " left the game (You were kicked by " + p.name + "!)");
                    if (!who.opchat) {
                        opchat.Use(who, args);
                    }
                }
                // Of course if the guy isn't hidden, it unhides them!
                else {
                    _s.GlobalSpawn(who, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1], false);
                    _s.GlobalMessageOps("To Ops -" + who.color + who.name + "-" + _s.props.DefaultColor + " is now &fvisible" + _s.props.DefaultColor + ".");
                    _s.GlobalChat(p, "&a+ " + who.color + who.prefix + who.name + _s.props.DefaultColor + " " + "joined the game.", false);
                    _s.IRC.Say(who.name + " joined the game");
                    // Opchat yay or nay?
                    if (who.opchat) {
                        opchat.Use(who, args);
                    }
                    if (who.adminchat) {
                        adminchat.Use(who, args);
                    }
                }
            }
            // This tells you who you used it on.
            p.SendMessage("Used /khide on " + who.color + who.name + _s.props.DefaultColor + ".");
        }

        /// <summary>
        /// Called when /help is used on /khide.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/khide <player?> - Like /ohide, but displays the kick message.");
            p.SendMessage("Only works on players of lower rank.");
        }
    }
}


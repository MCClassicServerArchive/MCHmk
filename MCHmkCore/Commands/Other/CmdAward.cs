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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    /// <summary>
    /// The code for the /award command, which gives or takes away an award from a player.
    /// </summary>
    public class CmdAward : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"reward", "trophy", "price"});

        public override string Name {
            get {
                return "award";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "other";
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
        public CmdAward(Server s) : base(s) { }
        
        /// <summary>
        /// The code that runs when /award is called.
        /// </summary>
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }

            bool give = true; // The default is to give an award.
            if (args.Split(' ')[0].ToLower() == "give") {
                give = true;
                args = args.Substring(args.IndexOf(' ') + 1);
            }
            else if (args.Split(' ')[0].ToLower() == "take") {
                give = false;
                args = args.Substring(args.IndexOf(' ') + 1);
            }

            string foundPlayer = args.Split(' ')[0];
            Player who = _s.players.Find(args.Split (' ')[0]);
            if (who != null) {
                foundPlayer = who.name;
            }
            string awardName = args.Substring(args.IndexOf(' ') + 1);
            if (!Awards.awardExists(awardName)) {
                p.SendMessage("The award you entered doesn't exist");
                p.SendMessage("Use /awards for a list of awards");
                return;
            }

            Uuid uuid = Uuid.FindUuid(_s.database, foundPlayer);
            if (!uuid.IsValid) {
                p.SendMessage("That player is invalid or has never joined the server.");
                return;
            }

            string color = _s.ranks.FindPlayerColor(uuid);

            if (give) {
                if (Awards.giveAward(uuid, foundPlayer, awardName)) {
                    _s.GlobalMessage(color + foundPlayer + _s.props.DefaultColor + " was awarded: &b" + Awards.camelCase(awardName));
                }
                else {
                    p.SendMessage("The player already has that award!");
                }
            }
            else {
                if (Awards.takeAward(uuid, foundPlayer, awardName)) {
                    _s.GlobalMessage(color + foundPlayer + _s.props.DefaultColor + " had their &b" + Awards.camelCase(awardName) + _s.props.DefaultColor + " award removed");
                }
                else {
                    p.SendMessage("The player didn't have the award you tried to take");
                }
            }
            Awards.Save();
        }
        
        /// <summary>
        /// Called when /help is used on /award.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/award [give/take] <player?> <award?> - Awards a player " +
                               "the specified award.");
            p.SendMessage("If give or take is not specified, give is used.");
            p.SendMessage("The specified award needs to be the full award's name.");
        }
    }
}

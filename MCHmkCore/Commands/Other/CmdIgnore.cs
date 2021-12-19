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
    Written by Jack1312

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
using System.IO;

namespace MCHmk.Commands {
    public class CmdIgnore : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"abort", "chat", "player"});

        public override string Name {
            get {
                return "ignore";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdIgnore(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game!");
                return;
            }
            if (args.Split(' ')[0] == "all") {
                p.ignoreglobal = !p.ignoreglobal;
                if (p.ignoreglobal) {
                    Player.globalignores.Add(p.name);
                }
                else {
                    Player.globalignores.Remove(p.name);
                }
                p.SendMessage(p.ignoreglobal ? "&cAll chat is now ignored!" : "&aAll chat is no longer ignored!");
                return;
            }

            if (args.Split(' ')[0] == "list") {
                p.SendMessage("&cCurrently ignoring the following players:");
                foreach (string ignoring in p.listignored) {
                    p.SendMessage("- " + ignoring);
                }
                return;
            }
            Player who = _s.players.Find(args.Split(' ')[0]);
            if (who == null) {
                p.SendMessage("Could not find player specified!");
                return;
            }

            if (who.name == p.name) {
                p.SendMessage("You cannot ignore yourself!!");
                return;
            }

            if (who.rank.Permission >= _s.props.opchatperm) {
                if (p.rank.Permission <= who.rank.Permission) {
                    p.SendMessage("You cannot ignore an operator of higher rank!");
                    return;
                }
            }


            if (!Directory.Exists("ranks/ignore")) {
                Directory.CreateDirectory("ranks/ignore");
            }
            if (!File.Exists("ranks/ignore/" + p.name + ".txt")) {
                File.Create("ranks/ignore/" + p.name + ".txt").Dispose();
            }
            string chosenpath = "ranks/ignore/" + p.name + ".txt";
            if (!File.Exists(chosenpath) || !p.listignored.Contains(who.name)) {
                p.listignored.Add(who.name);
                p.SendMessage("Player now ignored: &c" + who.name + "!");
                return;
            }
            if (p.listignored.Contains(who.name)) {
                p.listignored.Remove(who.name);
                p.SendMessage("Player is no longer ignored: &a" + who.name + "!");
                return;
            }
            p.SendMessage("Something is stuffed.... Tell a MCHmk Developer!");
        }

        /// <summary>
        /// Called when /help is used on /ignore.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/ignore <player?> - Ignores the specified player.");
            p.SendMessage("/ignore global - Ignores everyone.");
            p.SendMessage("You cannot ignore players of higher rank.");
        }
    }
}

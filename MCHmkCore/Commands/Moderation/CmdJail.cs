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
using System.IO;

namespace MCHmk.Commands {
    public class CmdJail : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"prison", "punish"});

        public override string Name {
            get {
                return "jail";
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
        public CmdJail(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if ((args.ToLower() == "set") && !p.IsConsole) {
                p.level.jailx = p.pos[0];
                p.level.jaily = p.pos[1];
                p.level.jailz = p.pos[2];
                p.level.jailrotx = p.rot[0];
                p.level.jailroty = p.rot[1];
                p.SendMessage("Set Jail point.");
            }
            else {
                Player who = _s.players.Find(args);
                if (who != null) {
                    if (!who.jailed) {
                        if (!p.IsConsole) {
                            if (who.rank.Permission >= p.rank.Permission) {
                                p.SendMessage("Cannot jail someone of equal or greater rank.");
                                return;
                            }
                            p.SendMessage("You jailed " + who.name);
                        }
                        _s.GlobalDie(who, false);
                        who.jailed = true;
                        _s.GlobalSpawn(who, who.level.jailx, who.level.jaily, who.level.jailz, who.level.jailrotx, who.level.jailroty,
                                           true);
                        if (!File.Exists("ranks/jailed.txt")) {
                            File.Create("ranks/jailed.txt").Close();
                        }
                        Extensions.DeleteLineWord("ranks/jailed.txt", who.name);
                        using (StreamWriter writer = new StreamWriter("ranks/jailed.txt", true)) {
                            writer.WriteLine(who.name.ToLower() + " " + who.level.name);
                        }
                        _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " was &8jailed", false);
                    }
                    else {
                        if (!File.Exists("ranks/jailed.txt")) {
                            File.Create("ranks/jailed.txt").Close();
                        }
                        Extensions.DeleteLineWord("ranks/jailed.txt", who.name.ToLower());
                        who.jailed = false;
                        _s.commands.FindCommand("spawn").Use(who, String.Empty);
                        p.SendMessage("You freed " + who.name + " from jail");
                        _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " was &afreed" + _s.props.DefaultColor + " from jail",
                                          false);
                    }
                }
                else {
                    p.SendMessage("Could not find specified player.");
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /jail.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/jail <player?> - Places the specified player in jail.");
            p.SendMessage("A jailed player cannot use commands.");
            p.SendMessage("/jail [set] - Creates the jail point for the map.");
            p.SendMessage("This command has been deprecated in favor of /xjail.");
        }
    }
}

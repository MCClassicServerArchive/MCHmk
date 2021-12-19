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
    public class CmdReveal : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"show"});

        public override string Name {
            get {
                return "reveal";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdReveal(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                args = p.name;
            }
            Level lvl;
            string[] text = new string[2];
            text[0] = String.Empty;
            text[1] = String.Empty;
            try {
                text[0] = args.Split(' ')[0].ToLower();
                text[1] = args.Split(' ')[1].ToLower();
            }
            catch {  // TODO: find exact exception to catch or rewrite

            }

            if (!p.IsConsole && p.level != null) {
                lvl = p.level;
            }
            else {
                lvl = _s.levels.FindExact(text[1]);
                if (lvl == null) {
                    p.SendMessage("Level not found!");
                    return;
                }
            }

            if (text[0].ToLower() == "all") {
                if (!p.IsConsole && p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                    p.SendMessage("Reserved for " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name + "+");
                    return;
                }

                foreach (Player who in _s.players.ToArray()) {
                    if (who.level == lvl) {

                        who.Loading = true;
                        foreach (Player pl in _s.players.ToArray()) if (who.level == pl.level && who != pl) {
                                who.SendDie(pl.serverId);
                            }
                        foreach (PlayerBot b in PlayerBot.playerbots.ToArray()) if (who.level == b.level) {
                                who.SendDie(b.id);
                            }

                        _s.GlobalDie(who, true);
                        who.SendUserMOTD();
                        who.SendMap();

                        ushort x = (ushort)((0.5 + who.level.spawnx) * 32);
                        ushort y = (ushort)((1 + who.level.spawny) * 32);
                        ushort z = (ushort)((0.5 + who.level.spawnz) * 32);

                        if (!who.hidden) {
                            _s.GlobalSpawn(who, x, y, z, who.level.rotx, who.level.roty, true);
                        }
                        else unchecked {
                            who.SendPos((byte)-1, x, y, z, who.level.rotx, who.level.roty);
                        }

                        foreach (Player pl in _s.players.ToArray())
                            if (pl.level == who.level && who != pl && !pl.hidden) {
                                who.SendSpawn(pl.serverId, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]);
                            }

                        foreach (PlayerBot b in PlayerBot.playerbots.ToArray())
                            if (b.level == who.level) {
                                who.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);
                            }

                        who.Loading = false;

                        if (!p.IsConsole && !p.hidden) {
                            who.SendMessage("&bMap reloaded by " + p.name);
                        }
                        if (!p.IsConsole && p.hidden) {
                            who.SendMessage("&bMap reloaded");
                        }
                        p.SendMessage("&4Finished reloading for " + who.name);
                    }
                }
            }
            else {
                Player who = _s.players.Find(text[0]);
                if (who == null) {
                    p.SendMessage("Could not find player.");
                    return;
                }
                else if (who.rank.Permission > p.rank.Permission && p != who) {
                    p.SendMessage("Cannot reload the map of someone higher than you.");
                    return;
                }

                who.Loading = true;
                foreach (Player pl in _s.players.ToArray()) {
                    if (who.level == pl.level && who != pl) {
                        who.SendDie(pl.serverId);
                    }
                }
                foreach (PlayerBot b in PlayerBot.playerbots.ToArray()) {
                    if (who.level == b.level) {
                        who.SendDie(b.id);
                    }
                }

                _s.GlobalDie(who, true);
                who.SendUserMOTD();
                who.SendMap();

                ushort x = (ushort)((0.5 + who.level.spawnx) * 32);
                ushort y = (ushort)((1 + who.level.spawny) * 32);
                ushort z = (ushort)((0.5 + who.level.spawnz) * 32);

                if (!who.hidden) {
                    _s.GlobalSpawn(who, x, y, z, who.level.rotx, who.level.roty, true);
                }
                else unchecked {
                    who.SendPos((byte)-1, x, y, z, who.level.rotx, who.level.roty);
                }

                foreach (Player pl in _s.players.ToArray())
                    if (pl.level == who.level && who != pl && !pl.hidden) {
                        who.SendSpawn(pl.serverId, pl.color + pl.name, pl.pos[0], pl.pos[1], pl.pos[2], pl.rot[0], pl.rot[1]);
                    }

                foreach (PlayerBot b in PlayerBot.playerbots.ToArray())
                    if (b.level == who.level) {
                        who.SendSpawn(b.id, b.color + b.name, b.pos[0], b.pos[1], b.pos[2], b.rot[0], b.rot[1]);
                    }

                who.Loading = false;

                who.SendMessage("&bMap reloaded by " + p.name);
                p.SendMessage("&4Finished reloading for " + who.name);
            }
        }

        /// <summary>
        /// Called when /help is used on /reveal.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/reveal <player?> - Reveals the map for a player.");
            p.SendMessage("/reveal all - Reveals for all players in current map.");
            p.SendMessage("/reveal all <map?> - Reveals for all players who are in a particular map.");
            p.SendMessage("Use /reveal when the map is in instant mode or in other " +
                               "situations in which some blocks seems to be missing.");
            p.SendMessage("Also use this command to turn off the effects of /highlight.");
        }
    }
}

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

﻿using System;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdModel : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"change", "model"});

        public override string Name {
            get {
                return "model";
            }
        }
        public override string Shortcut {
            get {
                return "setmodel";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdModel(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                args = "normal";
            }

            Player target;
            string model;

            if (args.Split(' ').Length > 1) {
                target = _s.players.Find(args.Split(' ')[0].Trim());
                if (target == null) {
                    p.SendMessage("Player \"" + args.Split(' ')[0].Trim() + "\" does not exist");
                    return;
                }
                model = args.Split(' ')[1].Trim();
                target.model = model;
            }
            else {
                if (p.IsConsole) {
                    _s.logger.Log("Console can't use this command on itself.");
                    return;
                }
                target = p;
                model = args;
                p.model = model;
            }

            foreach (Player pl in _s.players) {
                if (pl.level == target.level && pl.HasExtension("ChangeModel")) {
                    pl.SendChangeModel(target.serverId, args);
                }
            }

            if (p.IsConsole) {
                target.SendMessage("You're now a &c" + model);
            }
            else {
                _s.GlobalMessage(target.color + target.name + "'s" + _s.props.DefaultColor + " model was changed to a &c" + model);
            }
        }

        /// <summary>
        /// Called when /help is used on /model.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/model <player?> [model?] - Changes a player's model.");
            p.SendMessage("Valid models: Chicken, Creeper, Croc, Humanoid, Pig, Printer," +  
                               "Sheep, Spider, Skeleton, Zombie.");
            p.SendMessage("Block ID's are also valid model types.");
        }
    }
}

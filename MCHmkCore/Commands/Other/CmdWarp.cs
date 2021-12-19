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
using System.Threading;

namespace MCHmk.Commands {
    public class CmdWarp : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"move", "teleport", "tp", "pos"});

        public override string Name {
            get {
                return "warp";
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
                return false;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        public CmdWarp(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game");
                return;
            }
            string[] command = args.ToLower().Split(' ');
            string par0 = String.Empty;
            string par1 = String.Empty;
            string par2 = String.Empty;
            try {
                par0 = command[0];
                par1 = command[1];
                par2 = command[2];
            }
            catch {  // TODO: find exact exception to catch (or rewrite the code)

            }

            if (par0 == "list" || par0 == "view" || par0 == "l" || par0 == "v") {
                p.SendMessage("Warps:");
                foreach (Warp.Wrp wr in Warp.Warps) {
                    if (_s.levels.Find(wr.lvlname) != null) {
                        p.SendMessage(wr.name + " : " + wr.lvlname);
                        Thread.Sleep(
                            300); // I feel this is needed so that if there are a lot of warps, they do not immediatly go off the screen!
                    }
                }
                return;
            }

            if (par0 == "create" || par0 == "add" || par0 == "c" || par0 == "a") {
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 1)) {
                    if (par1 == null) {
                        p.SendMessage("You didn't specify a name for the warp!");
                        return;
                    }
                    if (Warp.WarpExists(par1)) {
                        p.SendMessage("Warp has already been created!!");
                        return;
                    }
                    {
                        if (par2 == null) {
                            Warp.AddWarp(par1, p);
                        }
                        else {
                            Warp.AddWarp(par1, _s.players.Find(par2));
                        }
                    }
                    {
                        if (Warp.WarpExists(par1)) {
                            p.SendMessage("Warp created!");
                            return;
                        }
                        else {
                            p.SendMessage("Warp creation failed!!");
                            return;
                        }
                    }
                }
                else {
                    p.SendMessage("You can't use that because you aren't a" + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                       1)).name + "+");
                    return;
                }
            }

            if (par0 == "delete" || par0 == "remove" || par0 == "d" || par0 == "r") {
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 2)) {
                    if (par1 == null) {
                        p.SendMessage("You didn't specify a warp to delete!");
                        return;
                    }
                    if (!Warp.WarpExists(par1)) {
                        p.SendMessage("Warp doesn't exist!!");
                        return;
                    }
                    {
                        Warp.DeleteWarp(par1);
                    }
                    {
                        if (!Warp.WarpExists(par1)) {
                            p.SendMessage("Warp deleted!");
                            return;
                        }
                        else {
                            p.SendMessage("Warp deletion failed!!");
                            return;
                        }
                    }
                }
                else {
                    p.SendMessage("You can't use that because you aren't a" + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                       2)).name + "+");
                    return;
                }
            }

            if (par0 == "move" || par0 == "change" || par0 == "edit" || par0 == "m" || par0 == "e") {
                if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 3)) {
                    if (par1 == null) {
                        p.SendMessage("You didn't specify a warp to be moved!");
                        return;
                    }
                    if (!Warp.WarpExists(par1)) {
                        p.SendMessage("Warp doesn't exist!!");
                        return;
                    }
                    {
                        if (par2 == null) {
                            Warp.MoveWarp(par1, p);
                        }
                        else {
                            Warp.MoveWarp(par1, _s.players.Find(par2));
                        }
                    }
                    {
                        if (Warp.WarpExists(par1)) {
                            p.SendMessage("Warp moved!");
                            return;
                        }
                        else {
                            p.SendMessage("Warp moving failed!!");
                            return;
                        }
                    }
                }
                else {
                    p.SendMessage("You can't use that because you aren't a " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this,
                                       3)).name + "+");
                    return;
                }
            }

            else {
                if (Warp.WarpExists(par0) == true) {
                    Warp.Wrp w = new Warp.Wrp();
                    w = Warp.GetWarp(par0);
                    Level lvl = _s.levels.Find(w.lvlname);
                    if (lvl != null) {
                        if (p.level != lvl) {
                            if (lvl.permissionvisit > p.rank.Permission) {
                                p.SendMessage("Sorry, you aren't a high enough rank to visit the map that that warp is on.");
                                return;
                            }
                            _s.commands.FindCommand("goto").Use(p, lvl.name);
                            while (p.Loading) {
                                Thread.Sleep(250);
                            }
                        }
                        unchecked {
                            p.SendPos((byte)-1, w.x, w.y, w.z, w.rotx, w.roty);
                        }
                        return;
                    }
                    else {
                        p.SendMessage("The level that that warp is on (" + w.lvlname +
                                           ") either no longer exists or is currently unloaded");
                        return;
                    }
                }
                else {
                    p.SendMessage("That is not a command addition or a warp");
                    return;
                }
            }
        }

        /// <summary>
        /// Called when /help is used on /warp.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/warp <name?> - Warps you to a warp.");
            p.SendMessage("/warp list - Lists all the warps.");
            if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 1)) {
                p.SendMessage("/warp create <name?> [player?] - Creates a warp. " +
                                   "If a player is given, it will be created where he or she is.");
            }
            if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 2)) {
                p.SendMessage("/warp delete <name?> - Deletes a warp.");
            }
            if (p.rank.Permission >= _s.commands.GetOtherPerm(this, 3)) {
                p.SendMessage("/warp move <name?> [player?] - Moves a warp. " +
                                   "If a player is given, it will be created where he or she is.");
            }
        }
    }
}

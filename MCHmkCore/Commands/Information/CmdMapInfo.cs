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
    public class CmdMapInfo : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"map", "info"});

        public override string Name {
            get {
                return "mapinfo";
            }
        }
        public override string Shortcut {
            get {
                return "status";
            }
        }
        public override string Type {
            get {
                return "information";
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
                return DefaultRankValue.Banned;
            }
        }
        public CmdMapInfo(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            Level foundLevel;

            if (args == String.Empty) {
                foundLevel = p.level;
            }
            else {
                foundLevel = _s.levels.FindExact(args);
            }

            if (foundLevel == null) {
                p.SendMessage("Could not find specified level.");
                return;
            }

            p.SendMessage("&b" + foundLevel.name + _s.props.DefaultColor + ": Width=" + foundLevel.width.ToString() +
                               " Height=" + foundLevel.height.ToString() + " Depth=" + foundLevel.depth.ToString());

            switch (foundLevel.physics) {
            case 0:
                p.SendMessage("Physics are &cOFF" + _s.props.DefaultColor + " on &b" + foundLevel.name);
                break;
            case 1:
                p.SendMessage("Physics are &aNormal" + _s.props.DefaultColor + " on &b" + foundLevel.name);
                break;
            case 2:
                p.SendMessage("Physics are &aAdvanced" + _s.props.DefaultColor + " on &b" + foundLevel.name);
                break;
            case 3:
                p.SendMessage("Physics are &aHardcore" + _s.props.DefaultColor + " on &b" + foundLevel.name);
                break;
            case 4:
                p.SendMessage("Physics are &aInstant" + _s.props.DefaultColor + " on &b" + foundLevel.name);
                break;
            }

            try {
                p.SendMessage("Build rank = " + _s.ranks.FindPerm(foundLevel.permissionbuild).color + _s.ranks.FindPerm(
                                       foundLevel.permissionbuild).trueName + _s.props.DefaultColor + " : Visit rank = " + _s.ranks.FindPerm(
                                       foundLevel.permissionvisit).color + _s.ranks.FindPerm(foundLevel.permissionvisit).trueName);
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
            }

            p.SendMessage("BuildMax Rank = " + _s.ranks.FindPerm(foundLevel.perbuildmax).color + _s.ranks.FindPerm(
                                   foundLevel.perbuildmax).trueName + _s.props.DefaultColor + " : VisitMax Rank = " + _s.ranks.FindPerm(
                                   foundLevel.pervisitmax).color + _s.ranks.FindPerm(foundLevel.pervisitmax).trueName);

            if (foundLevel.guns == true) {
                p.SendMessage("&cGuns &eare &aonline &eon " + foundLevel.name + ".");
            }
            else {
                p.SendMessage("&cGuns &eare &coffline &eon " + foundLevel.name + ".");
            }

            if (Directory.Exists(@_s.props.backupLocation + "/" + foundLevel.name)) {
                int latestBackup = Directory.GetDirectories(@_s.props.backupLocation + "/" + foundLevel.name).Length;
                p.SendMessage("Latest backup: &a" + latestBackup + _s.props.DefaultColor + " at &a" + Directory.GetCreationTime(
                                       @_s.props.backupLocation + "/" + foundLevel.name + "/" +
                                       latestBackup).ToString("yyyy-MM-dd HH:mm:ss"));
            }
            else {
                p.SendMessage("No backups for this map exist yet.");
            }
        }

        /// <summary>
        /// Called when /help is used on /mapinfo.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/mapinfo <map?> - Display details about a map.");
        }
    }
}

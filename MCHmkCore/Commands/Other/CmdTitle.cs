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

using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdTitle : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"set", "user"});

        public override string Name {
            get {
                return "title";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdTitle(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            int pos = args.IndexOf(' ');
            Player who = _s.players.Find(args.Split(' ')[0]);
            if (who == null) {
                p.SendMessage("Could not find player.");
                return;
            }
            if (!p.IsConsole && who.rank.Permission > p.rank.Permission) {
                p.SendMessage("You can't change the title of someone ranked higher than you.");
                return;
            }

            string query;
            string newTitle = String.Empty;
            if (args.Split(' ').Length > 1) {
                newTitle = args.Substring(pos + 1);
            }
            else {
                who.title = String.Empty;
                who.SetPrefix();
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " had their title removed.", false);
                // FIXME: PreparedStatement
                query = "UPDATE Players SET Title = '' WHERE Name = '" + who.name + "'";
                _s.database.ExecuteStatement(query);
                return;
            }

            if (newTitle != String.Empty) {
                newTitle = newTitle.ToString().Trim().Replace("[", String.Empty);
                newTitle = newTitle.Replace("]", String.Empty);
            }

            if (newTitle.Length > 17) {
                p.SendMessage("Title must be under 17 letters.");
                return;
            }

            if (newTitle != String.Empty) {
                _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " was given the title of &b[" + newTitle + "]", false);
            }
            else {
                _s.GlobalChat(who, who.color + who.prefix + who.name + _s.props.DefaultColor + " had their title removed.", false);
            }

            if (newTitle == String.Empty) {
                query = "UPDATE Players SET Title = '' WHERE Name = '" + who.name + "'";
            }
            else {
                query = "UPDATE Players SET Title = '" + newTitle.Replace("'", _s.props.useMySQL ? "\\'" : "''") + "' WHERE Name = '" + who.name + "'";
            }
            // FIXME: PreparedStatement
            _s.database.ExecuteStatement(query);
            who.title = newTitle;
            who.SetPrefix();
        }

        /// <summary>
        /// Called when /help is used on /title.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/title <player?> <title?> - Gives a title to a player.");
            p.SendMessage("If no title is given, the player's title is removed.");
        }
    }
}

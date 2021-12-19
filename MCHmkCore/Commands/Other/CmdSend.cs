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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using MCHmk.SQL;

namespace MCHmk.Commands {
    public class CmdSend : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"message", "inbox", "send", "msg", "user", "player"});

        public override string Name {
            get {
                return "send";
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
                return DefaultRankValue.Builder;
            }
        }
        public CmdSend(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty || args.IndexOf(' ') == -1) {
                Help(p);
                return;
            }

            Player who = _s.players.Find(args.Split(' ')[0]);

            string whoTo, fromname;
            if (who != null) {
                whoTo = who.name;
            }
            else {
                whoTo = args.Split(' ')[0];
            }
            if (!p.IsConsole) {
                fromname = p.name;
            }
            else {
                fromname = "Console";
            }

            Uuid recipientUuid = Uuid.FindUuid(_s.database, whoTo);
            if (!recipientUuid.IsValid) {
                p.SendMessage("The player with that name is invalid or has never visited the server.");
                return;
            }

            args = args.Substring(args.IndexOf(' ') + 1);

            if (args.Length > 255 && _s.props.useMySQL) {
                p.SendMessage("Message was too long. The text below has been trimmed.");
                p.SendMessage(args.Substring(256));
                args = args.Remove(256);
            }
            _s.database.ExecuteStatement("CREATE TABLE if not exists inbox_" + recipientUuid + 
                                  " (PlayerFrom CHAR(20), TimeSent DATETIME, Contents VARCHAR(255));");
            if (!_s.props.useMySQL) {
                _s.logger.Log(args.Replace("'", "\\'"));
            }
            // FIXME: PreparedStatement
            _s.database.ExecuteStatement("INSERT INTO `inbox_" + recipientUuid + "` (PlayerFrom, TimeSent, Contents) VALUES ('" + 
                                  fromname + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + 
                                  args.Replace("'", (_s.props.useMySQL ? "\\'" : "''")) + "')");

            p.SendMessage("Message sent to &5" + whoTo + ".");
            if (who != null) {
                who.SendMessage("Message recieved from &5" + fromname + _s.props.DefaultColor + ".");
            }
        }

        /// <summary>
        /// Called when /help is used on /send.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/send <player?> <message?> - Sends an inbox message to a player.");
        }
    }
}

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
    public class CmdInbox : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"mail", "box"});

        public override string Name {
            get {
                return "inbox";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
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
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }
        public CmdInbox(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                _s.database.ExecuteStatement("CREATE TABLE if not exists inbox_" + p.uuid + 
                                             " (PlayerFrom CHAR(20), TimeSent DATETIME, Contents VARCHAR(255));");

                if (args == String.Empty) {
                    string query = "SELECT * FROM inbox_" + p.uuid + " ORDER BY TimeSent";
                    DataTable Inbox = _s.database.ObtainData(query);

                    if (Inbox.Rows.Count == 0) {
                        p.SendMessage("No messages found.");
                        Inbox.Dispose();
                        return;
                    }

                    for (int i = 0; i < Inbox.Rows.Count; ++i) {
                        p.SendMessage(i.ToString() + ": From &5" + Inbox.Rows[i]["PlayerFrom"].ToString() + 
                                           _s.props.DefaultColor + " at &a" + Inbox.Rows[i]["TimeSent"].ToString());
                    }
                    Inbox.Dispose();
                }
                else if (args.Split(' ')[0].ToLower() == "del" || args.Split(' ')[0].ToLower() == "delete") {
                    int FoundRecord = -1;

                    if (args.Split(' ')[1].ToLower() != "all") {
                        try {
                            FoundRecord = int.Parse(args.Split(' ')[1]);
                        }
                        catch {  // TODO: find exact exception to catch
                            p.SendMessage("Incorrect number given.");
                            return;
                        }

                        if (FoundRecord < 0) {
                            p.SendMessage("Cannot delete records below 0");
                            return;
                        }
                    }

                    string query = "SELECT * FROM inbox_" + p.uuid + " ORDER BY TimeSent";
                    DataTable Inbox = _s.database.ObtainData(query);

                    if (Inbox.Rows.Count - 1 < FoundRecord || Inbox.Rows.Count == 0) {
                        p.SendMessage("\"" + FoundRecord.ToString() + "\" does not exist.");
                        Inbox.Dispose();
                        return;
                    }

                    string queryString;
                    if (FoundRecord == -1) {
                        queryString = "DELETE FROM inbox_" + p.uuid;
                    }
                    else {
                        // FIXME: PreparedStatement
                        queryString = "DELETE FROM inbox_" + p.uuid + " WHERE PlayerFrom='" + Inbox.Rows[FoundRecord]["PlayerFrom"] 
                            + "' AND TimeSent='" + Convert.ToDateTime(Inbox.Rows[FoundRecord]["TimeSent"]).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                    }

                    _s.database.ExecuteStatement(queryString);

                    if (FoundRecord == -1) {
                        p.SendMessage("Deleted all messages.");
                    }
                    else {
                        p.SendMessage("Deleted message.");
                    }

                    Inbox.Dispose();
                }
                else {
                    int FoundRecord;

                    try {
                        FoundRecord = int.Parse(args);
                    }
                    catch {  // TODO: find exact exception to catch
                        p.SendMessage("Incorrect number given.");
                        return;
                    }

                    if (FoundRecord < 0) {
                        p.SendMessage("Cannot read records below 0");
                        return;
                    }

                    string query = "SELECT * FROM inbox_" + p.uuid + " ORDER BY TimeSent";
                    DataTable Inbox = _s.database.ObtainData(query);

                    if (Inbox.Rows.Count - 1 < FoundRecord || Inbox.Rows.Count == 0) {
                        p.SendMessage("\"" + FoundRecord.ToString() + "\" does not exist.");
                        Inbox.Dispose();
                        return;
                    }

                    p.SendMessage("Message from &5" + Inbox.Rows[FoundRecord]["PlayerFrom"] +
                                       _s.props.DefaultColor + " sent at &a" + Inbox.Rows[FoundRecord]["TimeSent"] + ":");
                    p.SendMessage(Inbox.Rows[FoundRecord]["Contents"].ToString());
                    Inbox.Dispose();
                }
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("Error accessing inbox. You may have no mail, try again.");
            }
        }

        /// <summary>
        /// Called when /help is used on /inbox.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/inbox - Displays your inbox.");
            p.SendMessage("/inbox <num> - Displays a particular message from your inbox.");
            p.SendMessage("/inbox del <num?/all> - Deletes a message or all of them from your inbox.");
        }
    }
}

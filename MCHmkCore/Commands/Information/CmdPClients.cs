/*
    Copyright 2015 MCGalaxy
        
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
using System.Text;

namespace MCHmk.Commands {
    
    public sealed class CmdPClients : Command {
        
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"players", "clients", "update", "outdated"});

        public override string Name {
            get {
                return "pclients";
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
        public CmdPClients(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            Dictionary<string, List<Player>> clients = new Dictionary<string, List<Player>>();
            foreach (Player pl in _s.players) {
                if (pl.hidden && p.rank.Permission < pl.rank.Permission) continue;
                
                string appName = pl.appName;
                if (String.IsNullOrEmpty(appName))
                    appName = "(unknown)";
                
                List<Player> usingClient;
                if (!clients.TryGetValue(appName, out usingClient)) {
                    usingClient = new List<Player>();
                    clients[appName] = usingClient;
                }
                usingClient.Add(pl);
            }
            
            p.SendMessage("Players using:");
            foreach (var kvp in clients) {
                StringBuilder builder = new StringBuilder();
                List<Player> players = kvp.Value;
                for (int i = 0; i < players.Count; i++) {
                    builder.Append(players[i].color).Append(players[i].DisplayName);
                    if (i < players.Count - 1)
                        builder.Append(_s.props.DefaultColor).Append(", ");
                }                
                p.SendMessage(String.Format("  {0}: &f{1}", kvp.Key, builder.ToString()));
            }
        }

        public override void Help(Player p) {
            p.SendMessage("/pclients - Lists the clients players are using, and who uses which client.");
        }
    }
}
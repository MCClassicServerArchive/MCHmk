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
    public class CmdBotSet : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"set", "bot", "ai"});

        public override string Name {
            get {
                return "botset";
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
                return DefaultRankValue.AdvBuilder;
            }
        }
        public CmdBotSet(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            try {
                if (args.Split(' ').Length == 1) {
                    PlayerBot pB = PlayerBot.Find(args);
                    try {
                        pB.Waypoints.Clear();
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                    pB.kill = false;
                    pB.hunt = false;
                    pB.AIName = String.Empty;
                    p.SendMessage(pB.color + pB.name + _s.props.DefaultColor + "'s AI was turned off.");
                    _s.logger.Log(pB.name + "'s AI was turned off.");
                    return;
                }
                else if (args.Split(' ').Length != 2) {
                    Help(p);
                    return;
                }

                PlayerBot Pb = PlayerBot.Find(args.Split(' ')[0]);
                if (Pb == null) {
                    p.SendMessage("Could not find specified Bot");
                    return;
                }
                string foundPath = args.Split(' ')[1].ToLower();

                if (foundPath == "hunt") {
                    Pb.hunt = !Pb.hunt;
                    try {
                        Pb.Waypoints.Clear();
                    }
                    catch (Exception e) {
                        _s.logger.ErrorLog(e);
                    }
                    Pb.AIName = String.Empty;
                    if (!p.IsConsole) {
                        _s.GlobalChatLevel(p, Pb.color + Pb.name + _s.props.DefaultColor + "'s hunt instinct: " + Pb.hunt, false);
                    }
                    _s.logger.Log(Pb.name + "'s hunt instinct: " + Pb.hunt);
                    return;
                }
                else if (foundPath == "kill") {
                    if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                        p.SendMessage("Only a " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(this)).name +
                                           "+ may toggle killer instinct.");
                        return;
                    }
                    Pb.kill = !Pb.kill;
                    if (!p.IsConsole) {
                        _s.GlobalChatLevel(p, Pb.color + Pb.name + _s.props.DefaultColor + "'s kill instinct: " + Pb.kill, false);
                    }
                    _s.logger.Log(Pb.name + "'s kill instinct: " + Pb.kill);
                    return;
                }

                if (!File.Exists("bots/" + foundPath)) {
                    p.SendMessage("Could not find specified AI.");
                    return;
                }

                string[] foundWay = File.ReadAllLines("bots/" + foundPath);

                if (foundWay[0] != "#Version 2") {
                    p.SendMessage("Invalid file version. Remake");
                    return;
                }

                PlayerBot.Pos newPos = new PlayerBot.Pos();
                try {
                    Pb.Waypoints.Clear();
                    Pb.currentPoint = 0;
                    Pb.countdown = 0;
                    Pb.movementSpeed = 12;
                }
                catch (Exception e) {
                    _s.logger.ErrorLog(e);
                }

                try {
                    foreach (string s in foundWay) {
                        if (s != String.Empty && s[0] != '#') {
                            bool skip = false;
                            newPos.type = s.Split(' ')[0];
                            switch (s.Split(' ')[0].ToLower()) {
                            case "walk":
                            case "teleport":
                                newPos.x = Convert.ToUInt16(s.Split(' ')[1]);
                                newPos.y = Convert.ToUInt16(s.Split(' ')[2]);
                                newPos.z = Convert.ToUInt16(s.Split(' ')[3]);
                                newPos.rotx = Convert.ToByte(s.Split(' ')[4]);
                                newPos.roty = Convert.ToByte(s.Split(' ')[5]);
                                break;
                            case "wait":
                            case "speed":
                                newPos.seconds = Convert.ToInt16(s.Split(' ')[1]);
                                break;
                            case "nod":
                            case "spin":
                                newPos.seconds = Convert.ToInt16(s.Split(' ')[1]);
                                newPos.rotspeed = Convert.ToInt16(s.Split(' ')[2]);
                                break;
                            case "linkscript":
                                newPos.newscript = s.Split(' ')[1];
                                break;
                            case "reset":
                            case "jump":
                            case "remove":
                                break;
                            default:
                                skip = true;
                                break;
                            }
                            if (!skip) {
                                Pb.Waypoints.Add(newPos);
                            }
                        }
                    }
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("AI file corrupt.");
                    return;
                }

                Pb.AIName = foundPath;
                if (!p.IsConsole) {
                    _s.GlobalChatLevel(p, Pb.color + Pb.name + _s.props.DefaultColor + "'s AI is now set to " + foundPath, false);
                }
                _s.logger.Log(Pb.name + "'s AI was set to " + foundPath);
            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("Error");
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /botset.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/botset <bot?> <script?> - Sets a bot to use the given AI script.");
            p.SendMessage("Special AI scripts: Kill and Hunt.");

        }
    }
}

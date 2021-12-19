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
    public class CmdMap : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"level", "lvl", "info", "edit"});

        public override string Name {
            get {
                return "map";
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
                return DefaultRankValue.Guest;
            }
        }
        public CmdMap(Server s) : base(s) { }
        public static bool gettinginfo = false;

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                args = p.level.name;
            }

            Level foundLevel;

            if (args.IndexOf(' ') == -1) {
                foundLevel = _s.levels.FindExact(args);
                if (foundLevel == null) {
                    if (!p.IsConsole) {
                        foundLevel = p.level;
                    }
                }
                else {
                    gettinginfo = true;
                    p.SendMessage("MOTD: &b" + foundLevel.motd);
                    p.SendMessage("Finite mode: " + FoundCheck(foundLevel, foundLevel.finite));
                    p.SendMessage("Random flow: " + FoundCheck(foundLevel, foundLevel.randomFlow));
                    p.SendMessage("Animal AI: " + FoundCheck(foundLevel, foundLevel.ai));
                    p.SendMessage("Edge water: " + FoundCheck(foundLevel, foundLevel.edgeWater));
                    p.SendMessage("Grass growing: " + FoundCheck(foundLevel, foundLevel.GrassGrow));
                    p.SendMessage("Tree growing: " + FoundCheck(foundLevel, foundLevel.growTrees));
                    p.SendMessage("Leaf decay: " + FoundCheck(foundLevel, foundLevel.leafDecay));
                    p.SendMessage("Physics speed: &b" + foundLevel.speedphysics);
                    p.SendMessage("Physics overload: &b" + foundLevel.overload);
                    p.SendMessage("Survival death: " + FoundCheck(foundLevel,
                                       foundLevel.Death) + "(Fall: " + foundLevel.fall + ", Drown: " + foundLevel.drown + ")");
                    p.SendMessage("Killer blocks: " + FoundCheck(foundLevel, foundLevel.Killer));
                    p.SendMessage("Unload: " + FoundCheck(foundLevel, foundLevel.unload));
                    p.SendMessage("Load on /goto: " + FoundCheck(foundLevel, foundLevel.loadOnGoto));
                    p.SendMessage("Auto physics: " + FoundCheck(foundLevel, foundLevel.rp));
                    p.SendMessage("Instant building: " + FoundCheck(foundLevel, foundLevel.Instant));
                    p.SendMessage("RP chat: " + FoundCheck(foundLevel, !foundLevel.worldChat));
                    p.SendMessage("Guns: " + FoundCheck(foundLevel, foundLevel.guns));
                    gettinginfo = false;
                    return;
                }
            }
            else {
                foundLevel = _s.levels.FindExact(args.Split(' ')[0]);

                if (foundLevel == null || args.Split(' ')[0].ToLower() == "ps" || args.Split(' ')[0].ToLower() == "rp") {
                    foundLevel = p.level;
                }
                else {
                    args = args.Substring(args.IndexOf(' ') + 1);
                }
            }

            if (!p.IsConsole)
                if (p.rank.Permission < _s.commands.GetOtherPerm(this)) {
                    p.SendMessage("Setting map options is reserved to " + _s.ranks.FindPermInt(_s.commands.GetOtherPerm(
                                           this)).name + "+");
                    return;
                }

            string foundStart;
            if (args.IndexOf(' ') == -1) {
                foundStart = args.ToLower();
            }
            else {
                foundStart = args.Split(' ')[0].ToLower();
            }

            try {
                if (foundLevel == null) {
                    p.SendMessage("derp");
                }
                switch (foundStart) {
                case "finite":
                    foundLevel.finite = !foundLevel.finite;
                    foundLevel.ChatLevel("Finite mode: " + FoundCheck(foundLevel, foundLevel.finite));
                    if (p.IsConsole) {
                        p.SendMessage("Finite mode: " + FoundCheck(foundLevel, foundLevel.finite, true));
                    }
                    break;
                case "ai":
                    foundLevel.ai = !foundLevel.ai;
                    foundLevel.ChatLevel("Animal AI: " + FoundCheck(foundLevel, foundLevel.ai));
                    if (p.IsConsole) {
                        p.SendMessage("Animal AI: " + FoundCheck(foundLevel, foundLevel.ai, true));
                    }
                    break;
                case "edge":
                    foundLevel.edgeWater = !foundLevel.edgeWater;
                    foundLevel.ChatLevel("Edge water: " + FoundCheck(foundLevel, foundLevel.edgeWater));
                    if (p.IsConsole) {
                        p.SendMessage("Edge water: " + FoundCheck(foundLevel, foundLevel.edgeWater, true));
                    }
                    break;
                case "grass":
                    foundLevel.GrassGrow = !foundLevel.GrassGrow;
                    foundLevel.ChatLevel("Growing grass: " + FoundCheck(foundLevel, foundLevel.GrassGrow));
                    if (p.IsConsole) {
                        p.SendMessage("Growing grass: " + FoundCheck(foundLevel, foundLevel.GrassGrow, true));
                    }
                    break;
                case "ps":
                case "physicspeed":
                    if (int.Parse(args.Split(' ')[1]) < 10) {
                        p.SendMessage("Cannot go below 10");
                        return;
                    }
                    foundLevel.speedphysics = int.Parse(args.Split(' ')[1]);
                    foundLevel.ChatLevel("Physics speed: &b" + foundLevel.speedphysics);
                    break;
                case "overload":
                    if (int.Parse(args.Split(' ')[1]) < 500) {
                        p.SendMessage("Cannot go below 500 (default is 1500)");
                        return;
                    }
                    if (!p.IsConsole && p.rank.Permission < DefaultRankValue.Admin && int.Parse(args.Split(' ')[1]) > 2500) {
                        p.SendMessage("Only SuperOPs may set higher than 2500");
                        return;
                    }
                    foundLevel.overload = int.Parse(args.Split(' ')[1]);
                    foundLevel.ChatLevel("Physics overload: &b" + foundLevel.overload);
                    break;
                case "motd":
                    if (args.Split(' ').Length == 1) {
                        foundLevel.motd = "ignore";
                    }
                    else {
                        foundLevel.motd = args.Substring(args.IndexOf(' ') + 1);
                    }
                    foundLevel.ChatLevel("Map MOTD: &b" + foundLevel.motd);
                    break;
                case "death":
                    foundLevel.Death = !foundLevel.Death;
                    foundLevel.ChatLevel("Survival death: " + FoundCheck(foundLevel, foundLevel.Death));
                    if (p.IsConsole) {
                        p.SendMessage("Survival death: " + FoundCheck(foundLevel, foundLevel.Death, true));
                    }
                    break;
                case "killer":
                    foundLevel.Killer = !foundLevel.Killer;
                    foundLevel.ChatLevel("Killer blocks: " + FoundCheck(foundLevel, foundLevel.Killer));
                    if (p.IsConsole) {
                        p.SendMessage("Killer blocks: " + FoundCheck(foundLevel, foundLevel.Killer, true));
                    }
                    break;
                case "fall":
                    foundLevel.fall = int.Parse(args.Split(' ')[1]);
                    foundLevel.ChatLevel("Fall distance: &b" + foundLevel.fall);
                    break;
                case "drown":
                    foundLevel.drown = int.Parse(args.Split(' ')[1]) * 10;
                    foundLevel.ChatLevel("Drown time: &b" + (foundLevel.drown / 10));
                    break;
                case "unload":
                    foundLevel.unload = !foundLevel.unload;
                    foundLevel.ChatLevel("Auto unload: " + FoundCheck(foundLevel, foundLevel.unload));
                    if (p.IsConsole) {
                        p.SendMessage("Auto unload: " + FoundCheck(foundLevel, foundLevel.unload, true));
                    }
                    break;
                case "rp":
                case "restartphysics":
                    foundLevel.rp = !foundLevel.rp;
                    foundLevel.ChatLevel("Auto physics: " + FoundCheck(foundLevel, foundLevel.rp));
                    if (p.IsConsole) {
                        p.SendMessage("Auto physics: " + FoundCheck(foundLevel, foundLevel.rp, true));
                    }
                    break;
                case "instant":
                    foundLevel.Instant = !foundLevel.Instant;
                    foundLevel.ChatLevel("Instant building: " + FoundCheck(foundLevel, foundLevel.Instant));
                    if(p.IsConsole) {
                        p.SendMessage("Instant building: " + FoundCheck(foundLevel, foundLevel.Instant, true));
                    }
                    break;
                case "chat":
                    foundLevel.worldChat = !foundLevel.worldChat;
                    foundLevel.ChatLevel("RP chat: " + FoundCheck(foundLevel, !foundLevel.worldChat));
                    if(p.IsConsole) {
                        p.SendMessage("RP chat: " + FoundCheck(foundLevel, !foundLevel.worldChat, true));
                    }
                    break;
                case "load":
                case "autoload":
                case "loadongoto":
                    foundLevel.loadOnGoto = !foundLevel.loadOnGoto;
                    foundLevel.ChatLevel("Load on /goto: " + FoundCheck(foundLevel, foundLevel.loadOnGoto));
                    if(p.IsConsole) {
                        p.SendMessage("Load on /goto: " + FoundCheck(foundLevel, foundLevel.loadOnGoto, true));
                    }
                    break;
                case "leaf":
                case "leafdecay":
                    foundLevel.leafDecay = !foundLevel.leafDecay;
                    foundLevel.ChatLevel("Leaf decay: " + FoundCheck(foundLevel, foundLevel.leafDecay));
                    if (p.IsConsole) {
                        p.SendMessage("Leaf decay: " + FoundCheck(foundLevel, foundLevel.leafDecay, true));
                    }
                    break;
                case "flow":
                case "randomflow":
                    foundLevel.randomFlow = !foundLevel.randomFlow;
                    foundLevel.ChatLevel("Random flow: " + FoundCheck(foundLevel, foundLevel.randomFlow));
                    if (p.IsConsole) {
                        p.SendMessage("Random flow: " + FoundCheck(foundLevel, foundLevel.randomFlow, true));
                    }
                    break;
                case "tree":
                case "growtrees":
                    foundLevel.growTrees = !foundLevel.growTrees;
                    foundLevel.ChatLevel("Tree growing: " + FoundCheck(foundLevel, foundLevel.growTrees));
                    if (p.IsConsole) {
                        p.SendMessage("Tree growing: " + FoundCheck(foundLevel, foundLevel.growTrees, true));
                    }
                    break;
                default:
                    p.SendMessage("Could not find option entered.");
                    return;
                }
                foundLevel.changed = true;
                if (!p.IsConsole && p.level != foundLevel) {
                    p.SendMessage("/map finished!");
                }
            }
            catch {  // TODO: find exact exception to catch
                p.SendMessage("INVALID INPUT");
            }
        }
        public string FoundCheck(Level level, bool check, bool console = false) {
            if (gettinginfo == false) {
                level.SaveSettings();
            }
            return console ? (check ? "ON" : "OFF") : (check ? "&aON" : "&cOFF");
        }

        /// <summary>
        /// Called when /help is used on /map.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            // I'm not going to touch this one much until I add a few things to /help -Jjp137
            p.SendMessage("/map <map?> <toggle?> [*toggle parameter*] - Sets the specified toggle on the specified level.");
            p.SendMessage("Possible toggles: finite, randomflow, ai, edge, grass, growtrees, leafdecay, ps, overload, motd, death, fall, drown, unload, loadongoto, rp, instant, killer, chat");
            p.SendMessage("finite - Causes all liquids to be finite.");
            p.SendMessage("randomflow - Makes mass flooding liquids flow less uniformly.");
            p.SendMessage("ai - Makes animals hunt or flee.");
            p.SendMessage("edge -  Causes edge water to flow.");
            p.SendMessage("grass - Makes grass not grow without physics.");
            p.SendMessage("growtrees - Makes saplings grow into trees after a while.");
            p.SendMessage("leafdecay - Makes leaves not connected to a log within 4 blocks disappear randomly.");
            p.SendMessage("ps - Sets the map's physics speed.");
            p.SendMessage("overload - will change how easy/hard it is to kill physics.");
            p.SendMessage("motd - Sets a custom motd for the map. (Leave blank to reset).");
            p.SendMessage("death - will allow survival-style dying (falling, drowning)");
            p.SendMessage("fall/drown - Sets the distance/time before dying from each.");
            p.SendMessage("unload/loadongoto - Sets whether to unload when noone is there or load on goto, respectively.");
            p.SendMessage("rp - Sets whether the physics auto-start for the map.");
            p.SendMessage("instant - Sets whether or not to update everyone's screens.");
            p.SendMessage("killer - Turns killer blocks on and off.");
            p.SendMessage("chat - Sets the map to recieve no messages from other maps.");
        }
    }
}

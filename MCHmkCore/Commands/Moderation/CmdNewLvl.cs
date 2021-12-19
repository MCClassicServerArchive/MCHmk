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

namespace MCHmk.Commands {
    public class CmdNewLvl : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"new", "add", "lvl", "level", "map"});

        public override string Name {
            get {
                return "newlvl";
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
                return DefaultRankValue.Admin;
            }
        }
        public CmdNewLvl(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            string[] parameters = args.Split(' '); // Grab the parameters from the player's message
            if (parameters.Length >= 5 && parameters.Length <= 6) { // make sure there are 5 or 6 params
                switch (parameters[4]) {
                case "flat":
                case "pixel":
                case "island":
                case "mountains":
                case "ocean":
                case "forest":
                case "desert":
                case "space":
                case "rainbow":
                case "hell":
                    break;

                default:
                    p.SendMessage("Valid types: island, mountains, forest, ocean, flat, pixel, desert, space, rainbow, and hell");
                    return;
                }

                string name = parameters[0].ToLower();
                ushort x = 1, y = 1, z = 1;
                int seed = 0;
                bool useSeed = false;
                try {
                    x = Convert.ToUInt16(parameters[1]);
                    y = Convert.ToUInt16(parameters[2]);
                    z = Convert.ToUInt16(parameters[3]);
                }
                catch {  // TODO: find exact exception to catch
                    p.SendMessage("Invalid dimensions.");
                    return;
                }
                if (parameters.Length == 6) {
                    try {
                        seed = Convert.ToInt32(parameters[5]);
                    }
                    catch {  // TODO: find exact exception to catch
                        seed = parameters[5].GetHashCode();
                    }
                    useSeed = true;
                }
                if (!isGood(x)) {
                    p.SendMessage(x + " is not a good dimension! Use a power of 2 next time.");
                    return;
                }
                if (!isGood(y)) {
                    p.SendMessage(y + " is not a good dimension! Use a power of 2 next time.");
                    return;
                }
                if (!isGood(z)) {
                    p.SendMessage(z + " is not a good dimension! Use a power of 2 next time.");
                    return;
                }

                if (!Level.ValidName(name)) {
                    p.SendMessage("Invalid name!");
                    return;
                }
                if (System.IO.File.Exists("levels/" + name + ".mcf") || System.IO.File.Exists("levels/byte/" + name + ".mcf")) {
                    p.SendMessage("Level \"" + name + "\" already exists!");
                    return;
                }

                // create a new level...
                using (Level lvl = new Level(_s, name, x, y, z, parameters[4], seed, useSeed)) {
                    lvl.Save(true); //... and save it.
                    lvl.Dispose(); // Then take out the garbage.
                }

                _s.GlobalMessage("Level \"" + name + "\" created" + (useSeed ? " with seed \"" + parameters[5] + "\"" :
                                     String.Empty)); // The player needs some form of confirmation.

            }
            else {
                Help(p);
            }
        }

        public bool isGood(ushort value) {
            switch (value) {
            //case 2:
            //case 4:
            //case 8:
            case 16: // below this is currently invalid.
            case 32:
            case 64:
            case 128:
            case 256:
            case 512:
            case 1024:
            case 2048:
            case 4096:
            case 8192:
                return true;
            }

            return false;
        }

        
        /// <summary>
        /// Called when /help is used on /newlvl.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/newlvl <mapname?> <x?> <y?> <z?> <type?> [seed?] - " +
                               "Creates a new level with the specified parameters.");
            // Update this to add more? (I'm guessing lego said this)
            // Who the heck wants a map with 8192^3 blocks? -Jjp137
            p.SendMessage("Valid x, y, z values: 16, 32, 64, 128, 256, 512, 1024");
            p.SendMessage("Valid level types: island, mountains, forest, ocean, flat, " +
                               "pixel, desert, space, rainbow, and hell.");
            p.SendMessage("The seed is optional, and controls how the level is generated.");
            p.SendMessage("If the seed is the same, the level will be the same.");
            p.SendMessage("The seed does not do anything on flat and pixel type maps.");
        }
    }
}

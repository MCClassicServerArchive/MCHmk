/*
    Copyright 2016 Jjp137/LegoBricker

    This file have been changed from the original source code by MCForge.

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

	Written by jordanneil23 with alot of help from TheMusiKid.

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
using System.Threading;

namespace MCHmk.Commands {
    public class CmdShutdown : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"shut", "down", "stop", "exit", "quit"});

        public override string Name {
            get {
                return "shutdown";
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

        public CmdShutdown(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            // Cancels a shutdown, if one is currently in progress.
            if (args == "cancel") {
                if (_s.isInShutDown) {
                    _s.isInShutDown = false;
                }
                else {
                    p.SendMessage("Server is not currently shutting down!");
                }
                return;
            }
            // If not canceled, and server is already shutting down, don't try and shutdown more.
            else if (_s.isInShutDown) {
                p.SendMessage("Server is already shutting down! Use /shutdown cancel to cancel.");
                return;
            }
            // The shutdown sequence
            else {
                int secTime = 10;

                //Tries to find a time passed by the player
                bool isNumber = int.TryParse(args, out secTime);

                if (!isNumber) {
                    p.SendMessage("Seconds value was invalid.");
                    return;
                }

                // Sets the shutdown property to true
                _s.isInShutDown = true;

                // Loops the shutdown message and then terminates it at the end of the loop.
                // Also stops giving shutdown messages if the shutdown was canceled.
                for (int i = secTime; i > 0; i--) {
                    if (_s.isInShutDown) {
                        _s.GlobalMessage("%4Server shutdown in " + i.ToString() + " seconds");
                        _s.logger.Log("Server shutdown in " + i.ToString() + " seconds");
                        Thread.Sleep(1000);
                    }
                    else {
                        // Cancel message if the shutdown was canceled.
                        _s.GlobalMessage("Shutdown canceled.");
                        _s.logger.Log("Shutdown canceled.");
                        return;
                    } 
                }
                // Terminates the server
                MCHmk.Gui.Program.ExitProgram();
            }
        }

        /// <summary>
        /// Called when /help is used on /shutdown.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/shutdown <seconds?> - Shuts the server down after the given number of seconds.");
            p.SendMessage("/shutdown cancel - Cancels an ongoing shutdown.");
        }
    }
}

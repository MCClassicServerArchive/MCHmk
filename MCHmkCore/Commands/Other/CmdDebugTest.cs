/*
    Copyright 2016 Jjp137

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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /debugtest command, which devs may use to test random methods. 
    /// It should only appear in the debug build.
    /// </summary>
    public class CmdDebugTest : Command {
        /// <summary>
        /// The list of keywords that are associated with /debugtest.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"debug", "devs"});

        /// <summary>
        /// Gets the name of /debugtest.
        /// </summary>
        public override string Name {
            get {
                return "debugtest";
            }
        }

        /// <summary>
        /// Gets the shortcut for /debugtest.
        /// </summary>
        public override string Shortcut {
            get {
                return "dbt";
            }
        }

        /// <summary>
        /// Gets the category that /debugtest belongs to.
        /// </summary>
        public override string Type {
            get {
                return "other";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /debugtest. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /debugtest can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /debugtest.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Guest;
            }
        }

        /// <summary>
        /// Constructs an instance of the /debugtest command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /debugtest will belong to.
        /// <seealso cref="Server"/></param>
        public CmdDebugTest(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /debugtest.
        /// </summary>
        /// <param name="p"> The player that used /debugtest. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            if (!p.IsConsole) {
                p.SendMessage(p.pos[0] + " " + p.pos[1] + " " + p.pos[2] + " " + p.rot[0] + " " + p.rot[1]);
            }
        }

        /// <summary>
        /// Called when /help is used on /debugtest.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/debugtest - A debug command that runs whatever the devs want.");
            p.SendMessage("Only in the debug build.");
            p.SendMessage("Useful for testing random methods.");
        }
    }
}

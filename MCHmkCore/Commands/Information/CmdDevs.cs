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
    Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)

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
    /// Implementation of the /devs command, which displays a message from the developers.
    /// </summary>
    public class CmdDevs : Command {
        /// <summary>
        /// The list of keywords that are associated with /devs.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"dev", "mchmk"});

        /// <summary>
        /// Gets the name of /devs.
        /// </summary>
        public override string Name {
            get {
                return "devs";
            }
        }

        /// <summary>
        /// Gets the shortcut for /devs.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /devs belongs to.
        /// </summary>
        public override string Type {
            get {
                return "information";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /devs. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /devs can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Gets the default rank of /devs.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        /// <summary>
        /// Constructs an instance of the /devs command.
        /// </summary>
        /// <param name="s"> The Server that this instance of /devs will belong to. <seealso cref="Server"/></param>
        public CmdDevs(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /devs.
        /// </summary>
        /// <param name="p"> The player that used /devs. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            p.SendMessage("&aPrimary MCHmk developer: &dJjp137");
            p.SendMessage("&aContributors: &6LegoBricker, &8Ultragamer1024");
            p.SendMessage("&fSpecial thanks to the &bMCForge team&f for the source code.");
            p.SendMessage("&fSome code and ideas were also taken from &bMCGalaxy&f.");
            p.SendMessage("&fAlso thank you to everyone who tested development releases. &aAnd poop is green!");
            p.SendMessage("&fGet the source code, report bugs, and read the wiki at: ");
            p.SendMessage("&bhttps://bitbucket.org/Jjp137/mchmk/");
        }

        /// <summary>
        /// Called when /help is used on /devs.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            p.SendMessage("/devs - Displays the list of MCHmk developers and MCHmk's website.");
        }
    }
}

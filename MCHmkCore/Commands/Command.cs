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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using MCHmk.Commands;

namespace MCHmk {
    /// <summary>
    /// The Command abstract class serves as a base for all command implementations.
    /// </summary>
    public abstract class Command {
        /// <summary>
        /// The Server instance that the command belongs to.
        /// </summary>
        protected Server _s;

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public abstract string Name {
            get;
        }

        /// <summary>
        /// Gets the command's shortcut, which is a shorter form of the command.
        /// </summary>
        public abstract string Shortcut {
            get;
        }

        /// <summary>
        /// Gets the category that the command belongs to.
        /// </summary>
        public abstract string Type {
            get;
        }

        /// <summary>
        /// Gets the keywords associated with this command. Used for /search.
        /// </summary>
        public abstract ReadOnlyCollection<string> Keywords {
            get;
        }

        /// <summary>
        /// Gets whether the command can be used in museums.
        /// </summary>
        public abstract bool MuseumUsable {
            get;
        }

        /// <summary>
        /// Gets the default permission value of this command.
        /// </summary>
        public abstract int DefaultRank {
            get;
        }

        /// <summary>
        /// Constructs a Command instance.
        /// </summary>
        /// <param name="s"> The server instance that the command will belong to. </param>
        protected Command(Server s) {
            _s = s;
        }

        /// <summary>
        /// Called when a player uses the command. This method should contain the command's logic.
        /// </summary>
        /// <param name="p"> The player that used the command. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public abstract void Use(Player p, string args);
        /// <summary>
        /// Called when a player uses /help on this command. This method should send explanatory messages
        /// to the user.
        /// </summary>
        /// <param name="p"> The player that used /help on this command. <seealso cref="Player"/></param>
        public abstract void Help(Player p);
    }
}

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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MCHmk.Commands;

namespace MCHmk {
    public class CommandList : IEnumerable<Command> {
        /// <summary>
        /// The CommandPerm subclass holds information about a command's permission values.
        /// </summary>
        public class CommandPerm {
            /// <summary>
            /// The name of the command.
            /// </summary>
            public string commandName;
            /// <summary>
            /// The lowest rank that can use the command.
            /// </summary>
            public int lowestRank;
            /// <summary>
            /// A list of specific permission values that cannot use the command.
            /// </summary>
            public List<int> disallow = new List<int>();
            /// <summary>
            /// A list of specific permission values that can use the command.
            /// </summary>
            public List<int> allow = new List<int>();
        }
        
        /// <summary>
        /// The OtherPerms class holds information about a single permission value and the feature that
        /// it affects access to.
        /// </summary>
        public class CommandOtherPerms {
            /// <summary>
            /// The command that uses the permission value.
            /// </summary>
            public string commandName;
            /// <summary>
            /// The permission value of the rank threshold associated with this feature. It can specify
            /// a minimum or maximum rank, depending on the feature.
            /// </summary>
            public int Permission;
            /// <summary>
            /// A description of what this permission value affects.
            /// </summary>
            public string Description = String.Empty;
            /// <summary>
            /// A number that a Command class can use in a call to GetPerm() to refer to the specific feature.
            /// </summary>
            public int number;
        }

        /// <summary>
        /// The list of commands.
        /// </summary>
        private List<Command> _commands = new List<Command>();
        /// <summary>
        /// The list of the permissions of every command.
        /// </summary>
        private List<CommandPerm> _perms = new List<CommandPerm>();
        /// <summary>
        /// The list of supplementary command permissions.
        /// </summary>
        private List<CommandOtherPerms> _otherPerms = new List<CommandOtherPerms>();
        /// <summary>
        /// A reference to the server's list of ranks.
        /// </summary>
        private RankList _ranks;
        /// <summary>
        /// A reference to the logger that will log any errors.
        /// </summary>
        private Logger _logger;

        /// <summary>
        /// Gets a shallow copy of the list of permission values for every command.
        /// </summary>
        public List<CommandPerm> Perms {
            get {
                return new List<CommandPerm>(_perms);
            }
        }

        /// <summary>
        /// Gets a shallow copy of the supplementary command permissions.
        /// </summary>
        public List<CommandOtherPerms> OtherPerms {
            get {
                return new List<CommandOtherPerms>(_otherPerms);
            }
        }

        /// <summary>
        /// Gets or sets the logger used to log errors related to the CommandList.
        /// </summary>
        public Logger Logger {
            get {
                return _logger;
            }
            set {
                _logger = value;
            }
        }

        /// <summary>
        /// Constructs a new CommandList object.
        /// </summary>
        /// <param name="ranks"> The list of ranks to use. <seealso cref="RankList"/></param>
        public CommandList(RankList ranks) { 
            _ranks = ranks;
        }

        /// <summary>
        /// Adds a command to the list.
        /// </summary>
        /// <param name="cmd"> The command to add. <seealso cref="Command"/></param>
        public void Add(Command cmd) {
            _commands.Add(cmd);
        }

        /// <summary>
        /// Obtains a list of the names of every command.
        /// </summary>
        /// <returns> A List containing the names of every command. </returns>
        public List<string> CommandNames() {
            var tempList = new List<string>();

            _commands.ForEach(cmd => tempList.Add(cmd.Name));

            return tempList;
        }

        /// <summary>
        /// Removes a command from the list.
        /// </summary>
        /// <param name="cmd"> The command to remove.  <seealso cref="Command"/></param>
        /// <returns> Whether the command was removed. </returns>
        public bool Remove(Command cmd) {
            return _commands.Remove(cmd);
        }

        /// <summary>
        /// Checks whether a command is in the list.
        /// </summary>
        /// <param name="cmd"> The command to check for. <seealso cref="Command"/></param>
        /// <returns> Whether the command is in the list. </returns>
        public bool Contains(Command cmd) {
            return _commands.Contains(cmd);
        }

        /// <summary>
        /// Given a command's name, finds that command.
        /// </summary>
        /// <param name="name"> The name of the command. </param>
        /// <returns> The corresponding command, or null if no command with that name exists.
        /// <seealso cref="Command"/></returns>
        public Command FindCommand(string name) {
            name = name.ToLower();
            return _commands.FirstOrDefault(cmd => cmd.Name == name || cmd.Shortcut == name);
        }

        /// <summary>
        /// Given a command's shortcut, finds the name of the command.
        /// </summary>
        /// <param name="shortcut"> The shortcut that was entered. </param>
        /// <returns> The name of the command that corresponds to that shortcut, or an empty string if
        /// no command with that shortcut was found. </returns>
        public string FindNameByShortcut(string shortcut) {
            if (shortcut == String.Empty) {
                return String.Empty;
            }

            shortcut = shortcut.ToLower();
            foreach (Command cmd in _commands.Where(cmd => cmd.Shortcut == shortcut)) {
                return cmd.Name;
            }
            return String.Empty;
        }

        /// <summary>
        /// Logs a CommandList-related error message.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        private void Log(string message) {
            Logger temp = _logger;
            if (temp != null) {
                temp.Log(message);
            }
        }

        /// <summary>
        /// Logs a CommandList-related exception.
        /// </summary>
        /// <param name="ex"> The exception to log. </param>
        private void ErrorLog(Exception ex) {
            Logger temp = _logger;
            if (temp != null) {
                temp.ErrorLog(ex);
            }
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of commands.
        /// </summary>
        /// <returns> An IEnumerator<Player> object for this list of commands. </returns>
        public IEnumerator<Command> GetEnumerator() {
            return _commands.GetEnumerator();
        }

        /// <summary>
        /// Obtains an enumerator that iterates through the list of commands.
        /// </summary>
        /// <returns> An IEnumerator object for this list of commands. </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Path to the file containing command permissions.
        /// </summary>
        private readonly string permsPath = Path.Combine("properties", "command.properties");

        /// <summary>
        /// Loads the permission values of each command if command.properties exists. Otherwise, initializes
        /// the default permissions of each command.
        /// </summary>
        public void LoadPerms() {
            // Obtain the names of every command in MCHmk and initialize the list of permissions.
            List<string> foundCommands = CommandNames();
            _perms = new List<CommandPerm>();

            // Temporary variable.
            CommandPerm allowVar;

            // Adds each command into the command permissions list with their default ranks.
            foreach (Command cmd in _commands) {
                allowVar = new CommandPerm();
                allowVar.commandName = cmd.Name;
                allowVar.lowestRank = cmd.DefaultRank;
                _perms.Add(allowVar);
            }

            // Read command.properties if it exists. Doing so will override the default permissions.
            if (File.Exists(permsPath)) {
                string[] lines = File.ReadAllLines(permsPath);

                // There were two versions of this file, apparently.
                if (lines[0] == "#Version 2") {
                    string[] colon = new[] { " : " };

                    // Go through each command entry in the file.
                    foreach (string line in lines) {
                        allowVar = new CommandPerm();

                        // Skip comments and blank lines.
                        if (line == String.Empty || line[0] == '#') { 
                            continue;
                        }

                        // Format of each line:
                        // Name : Lowest : Disallow : Allow
                        string[] command = line.Split(colon, StringSplitOptions.None);

                        // Check if the command name is valid before obtaining the permission values.
                        if (!foundCommands.Contains(command[0])) {
                            Log("Incorrect command name: " + command[0]);
                            continue;
                        }
                        allowVar.commandName = command[0];

                        // Obtain the disallowed and allowed permission values if there are any.
                        string[] disallow = new string[0];
                        if (command[2] != String.Empty) {
                            disallow = command[2].Split(',');
                        }
                        string[] allow = new string[0];
                        if (command[3] != String.Empty) {
                            allow = command[3].Split(',');
                        }

                        // Set the permission values for that command.
                        try {
                            allowVar.lowestRank = int.Parse(command[1]);
                            foreach (string s in disallow) {
                                allowVar.disallow.Add(int.Parse(s));
                            }
                            foreach (string s in allow) {
                                allowVar.allow.Add(int.Parse(s));
                            }
                        }
                        catch (Exception e) {
                            ErrorLog(e);
                            Log("Hit an error on the command " + line);
                            continue;
                        }

                        // Replace the corresponding entry in allowedCommands, which have default permissions,
                        // with the permissions loaded from command.properties.
                        int current = 0;
                        foreach (CommandPerm aV in _perms) {
                            if (command[0] == aV.commandName) {
                                _perms[current] = allowVar;
                                break;
                            }
                            current++;
                        }
                    }
                }
                else {  // Read version 1 of the command.properties file instead.
                    // Skip comments and blank lines.
                    foreach (string line in lines.Where(line => line != String.Empty && line[0] != '#')) {
                        allowVar = new CommandPerm();

                        // Split the string by the equals sign.
                        string key = line.Split('=')[0].Trim().ToLower();
                        string value = line.Split('=')[1].Trim().ToLower();

                        // Make sure it is an actual command with an actual permission value on the other side of
                        // the equals sign.
                        if (!foundCommands.Contains(key)) {
                            Log("Incorrect command name: " + key);
                        }
                        else if (_ranks.PermFromName(value) == DefaultRankValue.Null) {
                            Log("Incorrect value given for " + key + ", using default value.");
                        }
                        else {
                            allowVar.commandName = key;
                            allowVar.lowestRank = _ranks.PermFromName(value);

                            // Replace the corresponding entry in allowedCommands, which have default permissions,
                            // with the permissions loaded from command.properties.
                            int current = 0;
                            foreach (CommandPerm aV in _perms) {
                                if (key == aV.commandName) {
                                    _perms[current] = allowVar;
                                    break;
                                }
                                current++;
                            }
                        }
                    }
                }
                // Save blocks.properties.
                SavePerms();
            }
            // Create the default file with the default ranks.
            else {
                SavePerms();
            }
        }

        /// <summary>
        /// Saves command.properties.
        /// </summary>
        public void SavePerms() {
            try {
                // Create the file, overwriting the existing command.properties file if it exists.
                File.Create(permsPath).Dispose();
                using (StreamWriter w = File.CreateText(permsPath)) {
                    // Write the file's header.
                    w.WriteLine("#Version 2");
                    w.WriteLine("#   This file contains a reference to every command found in the server software");
                    w.WriteLine("#   Use this file to specify which ranks get which commands");
                    w.WriteLine("#   Current ranks: " + _ranks.ConcatPerms());
                    w.WriteLine("#   Disallow and allow can be left empty, just make sure there's 2 spaces between the colons");
                    w.WriteLine("#   This works entirely on permission values, not names. Do not enter a rank name. Use it's permission value");
                    w.WriteLine("#   CommandName : LowestRank : Disallow : Allow");
                    w.WriteLine("#   gun : 60 : 80,67 : 40,41,55");
                    w.WriteLine("");

                    // Write each command and its permission values on its own line.
                    foreach (CommandPerm aV in _perms) {
                        w.WriteLine(aV.commandName + " : " + aV.lowestRank + " : " +
                                    RankUtil.GetInts(aV.disallow) + " : " + RankUtil.GetInts(aV.allow));
                    }
                }
            }
            catch (Exception e) {
                ErrorLog(e);
                Log("SAVE FAILED! command.properties");
            }
        }

        /// <summary>
        /// Determines whether a player can perform the given command.
        /// </summary>
        /// <param name="rank"> The player using the command. <seealso cref="Player"/></param>
        /// <param name="cmd"> The name of the command to be performed. </param>
        /// <returns> Whether the player can perform the given command. </returns>
        /// <exception cref="ArgumentNullException"> Thrown when the player parameter is null. </exception>
        public bool CanExecute(Player player, string name) {
            if (player == null) {
                throw new ArgumentNullException("The player parameter cannot be null.");
            }

            return CanExecute(player.rank.Permission, FindCommand(name));
        }

        /// <summary>
        /// Determines whether those with the given permission value can execute the specified command.
        /// </summary>
        /// <param name="perm"> The permission value of those who want to use the command. </param>
        /// <param name="cmd"> The command to be performed. <seealso cref="Command"/></param>
        /// <returns> Whether players with the given permission value can perform the given command. </returns>
        public bool CanExecute(int perm, Command cmd) {
            CommandPerm foundPerm = FindPermByCommand(cmd);

            return foundPerm.lowestRank <= perm && !foundPerm.disallow.Contains(perm) || foundPerm.allow.Contains(perm);
        }

        /// <summary>
        /// Checks whether nobody can use the given command.
        /// </summary>
        /// <param name="cmdName"> The name of the command to check. </param>
        /// <returns> Whether nobody can use the command at all. </returns>
        public bool NobodyCanUse(string cmdName) {
            CommandPerm foundPerm = FindPermByCommand(FindCommand(cmdName));

            return foundPerm.lowestRank >= DefaultRankValue.Nobody;
        }

        /// <summary>
        /// Finds the CommandPerm object associated with the given command.
        /// </summary>
        /// <param name="cmd"> The command to find the permission values for. <seealso cref="Command"/></param>
        /// <returns> The CommandPerm object associated with the given command. <seealso cref="CommandPerm"/></returns>
        /// <exception cref="ArgumentException"> Thrown when the given command is not recognized by MCHmk. </exception>
        /// <exception cref="ArgumentNullException"> Thrown when the given command is null. </exception>
        public CommandPerm FindPermByCommand(Command cmd) {
            if (cmd == null) {
                throw new ArgumentNullException("The cmd parameter cannot be null.");
            }

            int index = _perms.FindIndex(perm => perm.commandName == cmd.Name);
            if (index == -1) {
                throw new ArgumentException("The given command is not recognized by MCHmk.");
            }

            return _perms[index];
        }

        /// <summary>
        /// Changes the minimum permission value needed to use a command.
        /// </summary>
        /// <param name="cmd"> The command to change the minimum permission value of. <seealso cref="Command"/></param>
        /// <param name="newRank"> The minimum permission value needed to use that command. </param>
        public void ChangePerm(Command cmd, int newRank) {
            CommandPerm foundPerm = FindPermByCommand(cmd);
            foundPerm.lowestRank = newRank;
        }

        /// <summary>
        /// Obtains a permission value from the list of supplementary permissions.
        /// </summary>
        /// <param name="cmd"> The command that uses the permission value. <seealso cref="Command"/></param>
        /// <param name="number"> A number that refers to a specific feature of the given command. Default is 1. </param>
        /// <returns> The permission value related to the given feature of the given command. </returns>
        public int GetOtherPerm(Command cmd, int number = 1) {
            CommandOtherPerms otpe = FindOtherPerm(cmd.Name, number);
            return otpe.Permission;
        }

        /// <summary>
        /// Finds the OtherPerms object associated with the provided command and feature.
        /// </summary>
        /// <param name="cmdName"> The command that uses the permission value. <seealso cref="Command"/></param>
        /// <param name="number"> A number that refers to a specific feature of the given command. Default is 1. </param>
        /// <returns> The OtherPerms object that contains the permission value for that specific feature.
        /// <seealso cref="CommandOtherPerms"/></returns>
        public CommandOtherPerms FindOtherPerm(string cmdName, int number = 1) {
            return _otherPerms.FirstOrDefault(OtPe => OtPe.commandName == cmdName && OtPe.number == number);
        }

        /// <summary>
        /// Adds information about a supplementary permission value to the list of permissions.
        /// </summary>
        /// <param name="cmdName"> The command that uses the permission value. <seealso cref="Command"/></param>
        /// <param name="Perm"> The permission value of the rank threshold associated with this feature. It can specify
        /// a minimum or maximum rank, depending on the feature. </param>
        /// <param name="desc"> A description of what the permission value affects. </param>
        /// <param name="number"> A number that a Command class can use in a call to GetPerm() to refer to the 
        /// specific feature. </param>
        public void AddOtherPerm(string cmdName, int Perm, string desc, int number = 1) {
            if (Perm > 120) {
                return;
            }
            CommandOtherPerms otpe = new CommandOtherPerms();
            otpe.commandName = cmdName;
            otpe.Permission = Perm;
            otpe.Description = desc;
            otpe.number = number;
            _otherPerms.Add(otpe);
        }

        /// <summary>
        /// Edits the permission value of a given feature.
        /// </summary>
        /// <param name="op"> The OtherPerms object that should be be edited. <seealso cref="CommandOtherPerms"/></param>
        /// <param name="perm"> The new permission value. </param>
        public void EditOtherPerm(CommandOtherPerms op, int perm) {
            if (perm > 120) {
                return;
            }
            CommandOtherPerms otpe = op;
            _otherPerms.Remove(op);
            otpe.Permission = perm;
            _otherPerms.Add(otpe);
        }

        /// <summary>
        /// Obtains the number of additional permissions that a given command uses.
        /// </summary>
        /// <param name="cmd"> The given command. </param>
        /// <returns> The number of additional permissions a given command uses. </returns>
        public int GetMaxOtherPerms(Command cmd) {
            int i = 1;
            bool stop = false;
            while (stop == false) {
                CommandOtherPerms op = FindOtherPerm(cmd.Name, i);
                if (op == null) {
                    stop = true;
                }
                else {
                    i++;
                }
            }
            return (i - 1);
        }

        /// <summary>
        /// Path to the file containing supplementary command permissions.
        /// </summary>
        private readonly string extraPermsPath = Path.Combine("properties", "ExtraCommandPermissions.properties");

        /// <summary>
        /// Saves the file that holds information about these supplementary command permissions.
        /// </summary>
        public void SaveOtherPerms() {
            using (StreamWriter SW = new StreamWriter(extraPermsPath)) {
                SW.WriteLine("#     This file is used for setting up additional permissions that are needed in commands!!");
                SW.WriteLine("#");
                SW.WriteLine("#     LAYOUT:");
                SW.WriteLine("#     [commandname]:[additionalpermissionnumber]:[permissionlevel]:[description]");
                SW.WriteLine("#     I.E:");
                SW.WriteLine("#     countdown:2:80:The lowest rank that can setup countdown (download, start, restart, enable, disable, cancel)");
                SW.WriteLine("#");
                SW.WriteLine("#     Please also note that descriptions cannot contain ':' and permissions cannot be above 120");
                SW.WriteLine("#");
                foreach (CommandOtherPerms otpe in _otherPerms) {
                    try {
                        SW.WriteLine(otpe.commandName + ":" + otpe.number + ":" + otpe.Permission + ":" + otpe.Description);
                    }
                    catch (Exception e) {
                        ErrorLog(e);
                        Log("Saving an additional command permission failed!!");
                    }
                }
                SW.Dispose();
            }
        }

        /// <summary>
        /// Loads the file that holds information about these supplementary command permissions, or
        /// creates a file containing default values if it does not exist.
        /// </summary>
        public void LoadOtherPerms() {
            if (_otherPerms.Count == 0) {
                AddDefaultPerms();
            }
            if (File.Exists(extraPermsPath)) {
                using (StreamReader SR = new StreamReader(extraPermsPath)) {
                    string line;
                    while (SR.EndOfStream == false) {
                        line = SR.ReadLine();
                        try {
                            if (!line.StartsWith("#") && line.Contains(':')) {
                                string[] LINE = line.ToLower().Split(':');
                                CommandOtherPerms OTPE = FindOtherPerm(LINE[0], int.Parse(LINE[1]));
                                EditOtherPerm(OTPE, int.Parse(LINE[2]));
                            }
                        }
                        catch (Exception e) {
                            ErrorLog(e);
                            Log("Loading an additional command permission failed!!");
                        }
                    }
                    SR.Dispose();
                }
            }
            else {
                SaveOtherPerms();
                LoadOtherPerms();
            }
        }

        /// <summary>
        /// Initializes the default permission values.
        /// </summary>
        public void AddDefaultPerms() {
            AddOtherPerm("ban", DefaultRankValue.AdvBuilder, "Minimum rank required to be immune from being banned");
            AddOtherPerm("zone", DefaultRankValue.Operator, "Minimum rank required to delete a zone", 1);
            AddOtherPerm("zone", DefaultRankValue.Operator, "Minimum rank required to delete all zones", 2);
            AddOtherPerm("zone", DefaultRankValue.Operator, "Minimum rank required to create zones", 3);
            AddOtherPerm("whowas", DefaultRankValue.AdvBuilder, "Minimum rank required to see a player's IP and whitelist status");
            AddOtherPerm("whois", DefaultRankValue.AdvBuilder, "Minimum rank required to see a player's IP and whitelist status");
            AddOtherPerm("warp", DefaultRankValue.Operator, "Minimum rank required to create warps", 1);
            AddOtherPerm("warp", DefaultRankValue.Operator, "Minimum rank required to delete warps", 2);
            AddOtherPerm("warp", DefaultRankValue.Operator, "Minimum rank required to move or edit warps", 3);
            AddOtherPerm("undo", DefaultRankValue.Operator, "Minimum rank required to undo another player's actions", 1);
            AddOtherPerm("undo", DefaultRankValue.AdvBuilder, "Minimum rank required to undo physics", 2);
            AddOtherPerm("tnt", DefaultRankValue.Operator, "Minimum rank required to use big TNT", 1);
            AddOtherPerm("tnt", DefaultRankValue.Operator, "Minimum rank required to allow or disallow TNT usage", 2);
            AddOtherPerm("tnt", DefaultRankValue.Operator, "Minimum rank required to use nuke TNT", 3);
            AddOtherPerm("store", DefaultRankValue.Operator, "Minimum rank required to delete another player's stored copy");
            AddOtherPerm("rules", DefaultRankValue.Builder, "Minimum rank required to send the rules to another player");
            AddOtherPerm("reveal", DefaultRankValue.Operator, "Minimum rank required to force everyone to reload the map");
            AddOtherPerm("report", DefaultRankValue.Operator, "Minimum rank required to check, view, and delete reports");
            AddOtherPerm("patrol", DefaultRankValue.Guest, "Maximum rank considered when selecting a player to patrol");
            AddOtherPerm("news", DefaultRankValue.Operator, "Minimum rank required to send the news to another player");
            AddOtherPerm("map", DefaultRankValue.Operator, "Minimum rank required to edit map options");
            AddOtherPerm("faq", DefaultRankValue.Builder, "Minimum rank required to send the FAQ to another player");
            AddOtherPerm("changelog", DefaultRankValue.Operator, "Minimum rank required to send the changelog to everybody");
            AddOtherPerm("botset", DefaultRankValue.Operator, "Minimum rank required to toggle killer status on a bot");
            AddOtherPerm("draw", DefaultRankValue.Builder, "Minimum rank required to make cones with /draw", 1);
            AddOtherPerm("draw", DefaultRankValue.Builder, "Minimum rank required to make pyramids with /draw", 2);
            AddOtherPerm("draw", DefaultRankValue.Builder, "Minimum rank required to make spheres with /draw", 3);
            AddOtherPerm("draw", DefaultRankValue.Builder, "Minimum rank required to make volcanoes with /draw", 4);
        }

        public void AddAllCommands(Server s) {
            _commands.Add(new CmdAbort(s));
            _commands.Add(new CmdAbout(s));
            _commands.Add(new CmdAdminChat(s));
            _commands.Add(new CmdAllowGuns(s));
            _commands.Add(new CmdAfk(s));
            _commands.Add(new CmdAgree(s));
            _commands.Add(new CmdAscend(s));
            _commands.Add(new CmdAward(s));
            _commands.Add(new CmdAwards(s));
            _commands.Add(new CmdAwardMod(s));
            _commands.Add(new CmdBan(s));
            _commands.Add(new CmdBanip(s));
            _commands.Add(new CmdBind(s));
            _commands.Add(new CmdBlocks(s));
            _commands.Add(new CmdBlockSet(s));
            _commands.Add(new CmdBlockSpeed(s));
            _commands.Add(new CmdBotAdd(s));
            _commands.Add(new CmdBotAI(s));
            _commands.Add(new CmdBotRemove(s));
            _commands.Add(new CmdBots(s));
            _commands.Add(new CmdBotSet(s));
            _commands.Add(new CmdBotSummon(s));
            _commands.Add(new CmdC4(s));
            _commands.Add(new CmdCalculate(s));
            _commands.Add(new CmdChangeLog(s));
            _commands.Add(new CmdClearBlockChanges(s));
            _commands.Add(new CmdClick(s));
            _commands.Add(new CmdClones(s));
            _commands.Add(new CmdCmdBind(s));
            _commands.Add(new CmdCmdSet(s));
            _commands.Add(new CmdColor(s));
            _commands.Add(new CmdCopy(s));
            _commands.Add(new CmdCopyLVL(s));
            _commands.Add(new CmdCrashServer(s));
            _commands.Add(new CmdCuboid(s));
#if DEBUG
            _commands.Add(new CmdDebugTest(s));
#endif
            _commands.Add(new CmdDelete(s));
            _commands.Add(new CmdDeleteLvl(s));
            _commands.Add(new CmdDemote(s));
            _commands.Add(new CmdDevs(s));
            _commands.Add(new CmdDisagree(s));
            _commands.Add(new CmdDescend(s));
            _commands.Add(new CmdDraw(s));
            _commands.Add(new CmdDrill(s));
            _commands.Add(new CmdDurl(s));
            _commands.Add(new CmdEllipse(s));
            _commands.Add(new CmdEmote(s));
            _commands.Add(new CmdExplode(s));
            _commands.Add(new CmdFakePay(s));
            _commands.Add(new CmdFakeRank(s));
            _commands.Add(new CmdFaq(s));
            _commands.Add(new CmdFetch(s));
            _commands.Add(new CmdFill(s));
            _commands.Add(new CmdFixGrass(s));
            _commands.Add(new CmdFlipHeads(s));
            _commands.Add(new CmdFly(s));
            _commands.Add(new CmdFollow(s));
            _commands.Add(new CmdFreeze(s));
            _commands.Add(new CmdGive(s));
            _commands.Add(new CmdGlobalCLS(s));
            _commands.Add(new CmdGoto(s));
            _commands.Add(new CmdGun(s));
            _commands.Add(new CmdHackRank(s));
            _commands.Add(new CmdHacks(s));
            _commands.Add(new CmdHasirc(s));
            _commands.Add(new CmdHelp(s));
            _commands.Add(new CmdHide(s));
            _commands.Add(new CmdHigh5(s));
            _commands.Add(new CmdHighlight(s));
            _commands.Add(new CmdHollow(s));
            _commands.Add(new CmdHost(s));
            _commands.Add(new CmdIgnore(s));
            _commands.Add(new CmdImpersonate(s));
            _commands.Add(new CmdImport(s));
            _commands.Add(new CmdInbox(s));
            _commands.Add(new CmdInfo(s));
            _commands.Add(new CmdInvincible(s));
            _commands.Add(new CmdJail(s));
            _commands.Add(new CmdJoker(s));
            _commands.Add(new CmdKHide(s));
            _commands.Add(new CmdKick(s));
            _commands.Add(new CmdKickban(s));
            _commands.Add(new CmdKill(s));
            _commands.Add(new CmdLastCmd(s));
            _commands.Add(new CmdLevels(s));
            _commands.Add(new CmdLimit(s));
            _commands.Add(new CmdLine(s));
            _commands.Add(new CmdLoad(s));
            _commands.Add(new CmdMain(s));
            _commands.Add(new CmdMap(s));
            _commands.Add(new CmdMapInfo(s));
            _commands.Add(new CmdMaze(s));
            _commands.Add(new CmdMe(s));
            _commands.Add(new CmdMeasure(s));
            _commands.Add(new CmdMegaboid(s));
            _commands.Add(new CmdMessageBlock(s));
            _commands.Add(new CmdMissile(s));
            _commands.Add(new CmdMode(s));
            _commands.Add(new CmdModel(s));
            _commands.Add(new CmdModerate(s));
            _commands.Add(new CmdMoney(s));
            _commands.Add(new CmdMove(s));
            _commands.Add(new CmdMoveAll(s));
            _commands.Add(new CmdMuseum(s));
            _commands.Add(new CmdMute(s));
            _commands.Add(new CmdNewLvl(s));
            _commands.Add(new CmdNews(s));
            _commands.Add(new CmdOHide(s));
            _commands.Add(new CmdOpChat(s));
            _commands.Add(new CmdOpRules(s));
            _commands.Add(new CmdOutline(s));
            _commands.Add(new CmdOZone(s));
            _commands.Add(new CmdP2P(s));
            _commands.Add(new CmdPaint(s));
            _commands.Add(new CmdPass(s));
            _commands.Add(new CmdPaste(s));
            _commands.Add(new CmdPatrol(s));
            _commands.Add(new CmdPause(s));
            _commands.Add(new CmdPay(s));
            _commands.Add(new CmdPClients(s));
            _commands.Add(new CmdPCount(s));
            _commands.Add(new CmdPerbuildMax(s));
            _commands.Add(new CmdPermissionBuild(s));
            _commands.Add(new CmdPermissionVisit(s));
            _commands.Add(new CmdPervisitMax(s));
            _commands.Add(new CmdPhysics(s));
            _commands.Add(new CmdPlace(s));
            _commands.Add(new CmdPlayerCLS(s));
            _commands.Add(new CmdPlayers(s));
            _commands.Add(new CmdPortal(s));
            _commands.Add(new CmdPossess(s));
            _commands.Add(new CmdPromote(s));
            _commands.Add(new CmdPyramid(s));
            _commands.Add(new CmdRagequit(s));
            _commands.Add(new CmdRainbow(s));
            _commands.Add(new CmdRankInfo(s));
            _commands.Add(new CmdRankMsg(s));
            _commands.Add(new CmdRedo(s));
            _commands.Add(new CmdReload(s));
            _commands.Add(new CmdRenameLvl(s));
            _commands.Add(new CmdRepeat(s));
            _commands.Add(new CmdReplace(s));
            _commands.Add(new CmdReplaceAll(s));
            _commands.Add(new CmdReplaceNot(s));
            _commands.Add(new CmdReport(s));
            _commands.Add(new CmdResetBot(s));
            _commands.Add(new CmdResetPass(s));
            _commands.Add(new CmdRestartPhysics(s));
            _commands.Add(new CmdRestore(s));
            _commands.Add(new CmdRestoreSelection(s));
            _commands.Add(new CmdRetrieve(s));
            _commands.Add(new CmdReveal(s));
            _commands.Add(new CmdReview(s));
            _commands.Add(new CmdRide(s));
            _commands.Add(new CmdRoll(s));
            _commands.Add(new CmdRules(s));
            _commands.Add(new CmdSave(s));
            _commands.Add(new CmdSay(s));
            _commands.Add(new CmdSearch(s));
            _commands.Add(new CmdSeen(s));
            _commands.Add(new CmdSend(s));
            _commands.Add(new CmdSendCmd(s));
            _commands.Add(new CmdServerReport(s));
            _commands.Add(new CmdSetPass(s));
            _commands.Add(new CmdSetRank(s));
            _commands.Add(new CmdSetspawn(s));
            _commands.Add(new CmdShutdown(s));
            _commands.Add(new CmdSlap(s));
            _commands.Add(new CmdSpawn(s));
            _commands.Add(new CmdSpheroid(s));
            _commands.Add(new CmdSpin(s));
            _commands.Add(new CmdSPlace(s));
            _commands.Add(new CmdStairs(s));
            _commands.Add(new CmdStatic(s));
            _commands.Add(new CmdStore(s));
            _commands.Add(new CmdSummon(s));
            _commands.Add(new CmdTake(s));
            _commands.Add(new CmdTColor(s));
            _commands.Add(new CmdTempBan(s));
            _commands.Add(new CmdText(s));
            _commands.Add(new CmdTime(s));
            _commands.Add(new CmdTimer(s));
            _commands.Add(new CmdTitle(s));
            _commands.Add(new CmdTopTen(s));
            _commands.Add(new CmdTnt(s));
            _commands.Add(new CmdTp(s));
            _commands.Add(new CmdTpZone(s));
            _commands.Add(new CmdTree(s));
            _commands.Add(new CmdTrust(s));
            _commands.Add(new CmdUnban(s));
            _commands.Add(new CmdUnbanip(s));
            _commands.Add(new CmdUndo(s));
            _commands.Add(new CmdUnflood(s));
            _commands.Add(new CmdUnload(s));
            _commands.Add(new CmdUnloaded(s));
            _commands.Add(new CmdView(s));
            _commands.Add(new CmdViewRanks(s));
            _commands.Add(new CmdVIP(s));
            _commands.Add(new CmdVoice(s));
            _commands.Add(new CmdVote(s));
            _commands.Add(new CmdVoteKick(s));
            _commands.Add(new CmdVoteResults(s));
            _commands.Add(new CmdWarn(s));
            _commands.Add(new CmdWarp(s));
            _commands.Add(new CmdWaypoint(s));
            _commands.Add(new CmdWeather(s));
            _commands.Add(new CmdWhisper(s));
            _commands.Add(new CmdWhitelist(s));
            _commands.Add(new CmdWhoip(s));
            _commands.Add(new CmdWhois(s));
            _commands.Add(new CmdWhowas(s));
            _commands.Add(new CmdWrite(s));
            _commands.Add(new CmdXban(s));
            _commands.Add(new CmdXhide(s));
            _commands.Add(new CmdXJail(s));
            _commands.Add(new CmdXspawn(s));
            _commands.Add(new CmdXundo(s));
            _commands.Add(new CmdZombieSpawn(s));
            _commands.Add(new CmdZone(s));
            _commands.Add(new CmdZz(s));
        }
    }
}

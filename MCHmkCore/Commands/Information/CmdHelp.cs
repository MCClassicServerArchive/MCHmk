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
    public class CmdHelp : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"info", "commands", "cmd", "list"});

        public override string Name {
            get {
                return "help";
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
                return DefaultRankValue.Banned;
            }
        }
        public CmdHelp(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            try {
                args = args.ToLower();
                switch (args) {
                case "":
                    p.SendMessage("Use &b/help ranks" + _s.props.DefaultColor + " for a list of ranks.");
                    p.SendMessage("Use &b/help build" + _s.props.DefaultColor + " for a list of building commands.");
                    p.SendMessage("Use &b/help mod" + _s.props.DefaultColor + " for a list of moderation commands.");
                    p.SendMessage("Use &b/help information" + _s.props.DefaultColor + " for a list of information commands.");
                    p.SendMessage("Use &b/help other" + _s.props.DefaultColor + " for a list of other commands.");
                    p.SendMessage("Use &b/help colors" + _s.props.DefaultColor + " to view the color codes.");
                    p.SendMessage("Use &b/help syntax" + _s.props.DefaultColor + " for a guide to /help message syntax.");
                    p.SendMessage("Use &b/help [command?] or /help [block?] " + _s.props.DefaultColor + "to view more info.");
                    break;
                case "syntax":
                    p.SendMessage("Help message syntax:");
                    p.SendMessage("<> is used to represent required parameters.");
                    p.SendMessage("[] is used to represent optional parameters.");
                    p.SendMessage("? is used to represent items that need to be filled in with appropriate input.");
                    p.SendMessage("Any parameters of the form <param1/param2> is a multiple choice parameter. Choose only one.");
                    p.SendMessage("Example: /kick <player?> [reason?] - Player is required, but reason is optional.");
                    break;
                case "ranks":
                    args = String.Empty;
                    foreach (Rank grp in _s.ranks) {
                        if (grp.name != "nobody") { // Note that -1 means max undo.  Undo anything and everything.
                            p.SendMessage(grp.color + grp.name + " - &bCmd: " + grp.maxBlocks.ToString() + " - &2Undo: " + ((grp.maxUndo != -1) ? grp.maxUndo.ToString() : "max") + " - &cPerm: " + grp.Permission);
                        }
                    }
                    break;
                case "build":
                    args = String.Empty;
                    foreach (Command comm in _s.commands) {
                        if (p.IsConsole || _s.commands.CanExecute(p.rank.Permission, comm)) {
                            if (comm.Type.Contains("build")) {
                                args += ", " + getColor(comm.Name) + comm.Name;
                            }
                        }
                    }

                    if (args == String.Empty) {
                        p.SendMessage("No commands of this type are available to you.");
                        break;
                    }
                    p.SendMessage("Building commands you may use:");
                    p.SendMessage(args.Remove(0, 2) + ".");
                    break;
                case "mod":
                case "moderation":
                    args = String.Empty;
                    foreach (Command comm in _s.commands) {
                        if (p.IsConsole || _s.commands.CanExecute(p.rank.Permission, comm)) {
                            if (comm.Type.Contains("mod")) {
                                args += ", " + getColor(comm.Name) + comm.Name;
                            }
                        }
                    }

                    if (args == String.Empty) {
                        p.SendMessage("No commands of this type are available to you.");
                        break;
                    }
                    p.SendMessage("Moderation commands you may use:");
                    p.SendMessage(args.Remove(0, 2) + ".");
                    break;
                case "information":
                    args = String.Empty;
                    foreach (Command comm in _s.commands) {
                        if (p.IsConsole || _s.commands.CanExecute(p.rank.Permission, comm)) {
                            if (comm.Type.Contains("info")) {
                                args += ", " + getColor(comm.Name) + comm.Name;
                            }
                        }
                    }

                    if (args == String.Empty) {
                        p.SendMessage("No commands of this type are available to you.");
                        break;
                    }
                    p.SendMessage("Information commands you may use:");
                    p.SendMessage(args.Remove(0, 2) + ".");
                    break;
                case "other":
                    args = String.Empty;
                    foreach (Command comm in _s.commands) {
                        if (p.IsConsole || _s.commands.CanExecute(p.rank.Permission, comm)) {
                            if (comm.Type.Contains("other")) {
                                args += ", " + getColor(comm.Name) + comm.Name;
                            }
                        }
                    }

                    if (args == String.Empty) {
                        p.SendMessage("No commands of this type are available to you.");
                        break;
                    }
                    p.SendMessage("Other commands you may use:");
                    p.SendMessage(args.Remove(0, 2) + ".");
                    break;
                case "colours":
                case "colors":
                    p.SendMessage("&fTo use a color simply put a '%' sign symbol before you put the color code.");
                    p.SendMessage("Colors Available:");
                    p.SendMessage("0 - &0Black " + _s.props.DefaultColor + "| 8 - &8Gray");
                    p.SendMessage("1 - &1Navy " + _s.props.DefaultColor + "| 9 - &9Blue");
                    p.SendMessage("2 - &2Green " + _s.props.DefaultColor + "| a - &aLime");
                    p.SendMessage("3 - &3Teal " + _s.props.DefaultColor + "| b - &bAqua");
                    p.SendMessage("4 - &4Maroon " + _s.props.DefaultColor + "| c - &cRed");
                    p.SendMessage("5 - &5Purple " + _s.props.DefaultColor + "| d - &dPink");
                    p.SendMessage("6 - &6Gold " + _s.props.DefaultColor + "| e - &eYellow");
                    p.SendMessage("7 - &7Silver " + _s.props.DefaultColor + "| f - &fWhite");
                    break;
                default:
                    Command cmd = _s.commands.FindCommand(args);
                    if (cmd != null) {
                        cmd.Help(p);
                        string foundRank = _s.ranks.PermToName(_s.commands.FindPermByCommand(cmd).lowestRank);
                        // If there is a shortcut for that command, print the shortcut;
                        // otherwise, tell the user that there is none -Jjp137
                        string shortcut = cmd.Shortcut != String.Empty ?
                                          "&b/" + cmd.Shortcut : "&8none";
                        p.SendMessage("Rank needed: " + getColor(cmd.Name) + foundRank
                                           + _s.props.DefaultColor + " | Shortcut: " + shortcut);
                        return;
                    }
                    BlockId b = BlockData.Ushort(args);
                    if (b != BlockId.Null) {
                        p.SendMessage("Block \"" + args + "\" appears as &b" + BlockData.Name(BlockData.Convert(b)));
                        Rank foundRank = _s.ranks.FindPerm(_s.blockPerms.FindById(b).lowestRank);
                        p.SendMessage("Rank needed: " + foundRank.color + foundRank.name);
                        return;
                    }
                    p.SendMessage("Could not find command or block specified.");
                    break;
                }

            }
            catch (Exception e) {
                _s.logger.ErrorLog(e);
                p.SendMessage("An error occured");
            }
        }

        private string getColor(string commName) {
            foreach (CommandList.CommandPerm aV in _s.commands.Perms) {
                if (aV.commandName == commName) {
                    if (_s.ranks.FindPerm(aV.lowestRank) != null) {
                        return _s.ranks.FindPerm(aV.lowestRank).color;
                    }
                }
            }
            return "&f";
        }

        /// <summary>
        /// Called when /help is used on /help.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("...really? Wow. Just...wow.");
        }
    }
}

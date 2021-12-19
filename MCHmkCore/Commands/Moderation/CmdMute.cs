/*
    Copyright 2016 Jjp137

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
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MCHmk.Commands {
    /// <summary>
    /// The code for the /mute command, which mutes another player.
    /// </summary>
    public class CmdMute : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"voice", "chat", "player"});

        public override string Name {
            get {
                return "mute";
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
                return DefaultRankValue.Operator;
            }
        }

        public CmdMute(Server s) : base(s) { }

        /// <summary>
        /// The code that runs when /mute is called.
        /// </summary>
        /// <param name="p"> The player that used the command. </param>
        /// <param name="args"> Any parameters that came after the command. </param>
        public override void Use(Player p, string args) {
            // If your message is too long, have some help!
            if (args == String.Empty || args.Split(' ').Length > 2) {
                Help(p);
                return;
            }
            // An array to keep the split message.
            string[] splitmessage = args.Split(' ');
            // Finds the player.
            Player who = _s.players.Find(splitmessage[0]);

            if (who == null) {
                bool found = false;
                String mutee = splitmessage[0].ToLower();
                Uuid uuid = Uuid.FindUuid(_s.database, mutee);
                if (!uuid.IsValid) {
                    found = false;
                }
                else if (_s.muted.Contains(uuid)) {
                    _s.muted.Remove(uuid);
                    _s.muted.Save("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");
                    _s.GlobalMessage(mutee + " &f(offline) " + _s.props.DefaultColor +
                            "is now unmuted!");
                    found = true;
                }
                else {
                    string path = "ranks/muted.txt";
                    foreach (string line in File.ReadAllLines(path)) {
                        string[] finder = line.Split (':');
                        if (finder.Length == 2 && finder[0] == mutee) {
                            _s.muted.Remove(uuid);
                            _s.muted.Save("muted.txt");
                            _s.logger.Log("SAVED: ranks/muted.txt");
                            found = true;
                            _s.GlobalMessage(mutee + " &f(offline) " + _s.props.DefaultColor +
                                    "is now unmuted!");
                        }
                    }
                }
                if (found == false) {
                    p.SendMessage("Invalid player! Either the player does not exist, or they are not muted.");
                }
                return;
            }
            // This is the time the person will be muted for.
            int time;
            if (splitmessage.Length == 1) {
                time = 0;
            }
            // Here it tries to get a time from the message.
            else {
                try {
                    time = Convert.ToInt32(splitmessage[1]);
                }
                catch (Exception) {  // TODO: find exact exception to catch
                    p.SendMessage("Invalid time given!");
                    Help(p);
                    //_s.logger.ErrorLog(e);
                    return;
                }
            }
            if (!p.IsConsole) {
                if (who.rank.Permission >= p.rank.Permission && who != p) {
                    p.SendMessage("Cannot mute somebody of an equal or higher rank!");
                    return;
                }
            }
            // Mutes yourself indefinitely
            if (who == p && time == 0) {
                if (p.muted) {
                    p.muted = false;
                    //p.SendMessage("You &bun-muted" + _s.props.DefaultColor + " yourself!");
                    _s.GlobalMessage(p.color + p.name + _s.props.DefaultColor + " has been &bun-muted!");
                    _s.muted.Remove (p.uuid);
                    _s.muted.Save("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");
                    string path = "ranks/muted.txt";
                    string name = who.name.ToLower();
                    foreach (string line in File.ReadAllLines(path)) {
                        string[] finder = line.Split (':');
                        if (finder.Length == 2 && finder[0] == name) {
                            _s.muted.Remove(p.uuid);
                            _s.muted.Save("muted.txt");
                            _s.logger.Log("SAVED: ranks/muted.txt");
                        }
                    }
                    return;
                }
                else {
                    p.muted = true;
                    //p.SendMessage("You &8muted " + _s.props.DefaultColor + "yourself!");
                    _s.GlobalMessage (p.color + p.name + _s.props.DefaultColor + " has been &8muted!");
                    _s.muted.Add (p.uuid, p.name);
                    _s.muted.Save("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");
                    return;
                }
            }
            // If you didn't enter a time, the person will be muted indefinitely.
            if (time == 0) {
                if (who.muted) {
                    who.muted = false;
                    _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " has been &bun-muted", false);
                    _s.muted.Remove(who.uuid);
                    _s.muted.Save("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");
                    string path = "ranks/muted.txt";
                    string name = who.name.ToLower();
                    foreach (string line in File.ReadAllLines(path)) {
                        string[] finder = line.Split (':');
                        if (finder.Length == 2 && finder[0] == name) {
                            _s.muted.Remove(who.uuid);
                            _s.muted.Save("muted.txt");
                            _s.logger.Log("SAVED: ranks/muted.txt");
                        }
                    }
                }
                else {
                    who.muted = true;
                    _s.GlobalChat(who, who.color + who.name + _s.props.DefaultColor + " has been &8muted", false);
                    _s.muted.Add(who.uuid, who.name);
                    _s.muted.Save("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");
                }
                return;
            }
            // If you entered a negative time, shame on you.
            if (time < 0) {
                p.SendMessage("Cannot use negative times!");
                return;
            }
            // Finally, if you entered a good time, the person is muted for that time.
            else {
                if (who.muted) {
                    p.SendMessage(who.name + " is already muted! Please wait until they are unmuted.");
                    return;
                }
                if (p.IsConsole) {
                    Thread t = new Thread(delegate() {
                        ConsoleMute (time, who);
                    });
                    t.Start();
                    return;
                }
                else {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    who.muted = true;
                    string mutedtime = who.name + ":" + time.ToString ();
                    _s.GlobalMessage(who.color + who.name + _s.props.DefaultColor + " was &8muted" + _s.props.DefaultColor + " for " + time.ToString() + " seconds.");
                    //Player.SendMessage (who, "You were &8muted" + _s.props.DefaultColor + " for " + time.ToString() + " seconds");
                    _s.muted.Add (who.uuid, mutedtime);
                    _s.muted.Save ("muted.txt");
                    _s.logger.Log("SAVED: ranks/muted.txt");

                    while (timer.ElapsedMilliseconds < time*1000) { }

                    if (_s.players.Contains(who) && _s.muted.Contains(who.uuid)) {
                        _s.muted.Remove(who.uuid);
                        _s.muted.Save ("muted.txt");
                        _s.logger.Log("SAVED: ranks/muted.txt");
                        who.muted = false;
                        _s.GlobalMessage(who.color + who.name + _s.props.DefaultColor + " was &bun-muted.");
                        timer.Reset();
                        return;
                    }
                }

            }
        }
        /// <summary>
        /// Method that mutes a player for a certain amount of time if
        /// /mute is used from the console.
        /// </summary>
        /// <param name="data"> The time the player is muted for. </param>
        /// <param name="who"> The player to be muted.</param>
        public void ConsoleMute(int data, Player who) {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            who.muted = true;
            string mutedtime = who.name + ":" + data.ToString ();
            _s.GlobalMessage(who.color + who.name + _s.props.DefaultColor + " was &8muted" + _s.props.DefaultColor + " for " + data.ToString() + " seconds.");
            //Player.SendMessage (who, "You were muted for " + data.ToString() + " seconds!");
            _s.muted.Add (who.uuid, mutedtime);
            _s.muted.Save ("muted.txt");
            _s.logger.Log("SAVED: ranks/muted.txt");
            while (timer.ElapsedMilliseconds < data*1000) { }

            if (_s.players.Contains(who) && _s.muted.Contains(who.uuid)) {
                _s.muted.Remove(who.uuid);
                _s.muted.Save ("muted.txt");
                _s.logger.Log("SAVED: ranks/muted.txt");
                who.muted = false;
                _s.GlobalMessage(who.color + who.name + _s.props.DefaultColor + " was &bun-muted.");
                timer.Reset();
                return;
            }
        }

        /// <summary>
        /// Called when /help is used on /mute.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/mute <player?> [seconds?] - Mutes or unmutes the specified player.");
            p.SendMessage("If a number of seconds is provided, mutes the specified player for that" +
                    " amount of time.");
        }
    }
}

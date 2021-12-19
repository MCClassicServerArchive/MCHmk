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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sharkbite.Irc;

namespace MCHmk {
    public class HmkBot {
        public static readonly string ColorSignal = "\x03";
        public static readonly string ResetSignal = "\x0F";
        private Connection connection;
        private List<string> banCmd;
        private string channel, opchannel;
        private string nick;
        private bool reset = false;
        private byte retries = 0;
        public string usedCmd = String.Empty;

        /// <summary>
        /// A reference to the server associated with the IRC bot.
        /// </summary>
        private Server _server;
        /// <summary>
        /// A reference to the server's logger.
        /// </summary>
        private Logger _logger;

        public HmkBot(Server svr, string channel, string opchannel, string nick, string ircServer) {
            // Store a reference to the server and logger.
            _server = svr;
            _logger = svr.logger;

            this.channel = channel.Trim();
            this.opchannel = opchannel.Trim();
            this.nick = nick.Replace(" ", String.Empty);
            banCmd = new List<string>();
            if (_server.props.irc) {

                ConnectionArgs con = new ConnectionArgs(nick, ircServer);
                con.Port = _server.props.ircPort;
                connection = new Connection(con, false, false);

                // Regster events for outgoing
                Player.PlayerChatted += Player_PlayerChat;
                Player.PlayerConnected += Player_PlayerConnect;
                Player.PlayerDisconnected += Player_PlayerDisconnect;

                // Regster events for incoming
                connection.Listener.OnNick += Listener_OnNick;
                connection.Listener.OnRegistered += Listener_OnRegistered;
                connection.Listener.OnPublic += Listener_OnPublic;
                connection.Listener.OnPrivate += Listener_OnPrivate;
                connection.Listener.OnAction += Listener_OnAction;
                connection.Listener.OnError += Listener_OnError;
                connection.Listener.OnQuit += Listener_OnQuit;
                connection.Listener.OnJoin += Listener_OnJoin;
                connection.Listener.OnPart += Listener_OnPart;
                connection.Listener.OnDisconnected += Listener_OnDisconnected;

                // Load banned commands list
                string oldIrcBlacklistPath = Path.Combine("text", "ircbancmd.txt");
                string ircBlacklistPath = Path.Combine("text", "irccmdblacklist.txt");

                if (File.Exists(oldIrcBlacklistPath)) { // Backwards compatibility
                    using (StreamWriter sw = File.CreateText(ircBlacklistPath)) {
                        sw.WriteLine("#Here you can put commands that cannot be used from the IRC bot.");
                        sw.WriteLine("#Lines starting with \"#\" are ignored.");
                        foreach (string line in File.ReadAllLines(oldIrcBlacklistPath)) {
                            sw.WriteLine(line);
                        }
                    }
                    File.Delete(oldIrcBlacklistPath);
                }
                else {
                    if (!File.Exists(ircBlacklistPath)) File.WriteAllLines(ircBlacklistPath, new String[] { "#Here you can put commands that cannot be used from the IRC bot.", "#Lines starting with \"#\" are ignored." });
                    foreach (string line in File.ReadAllLines(ircBlacklistPath))
                        if (line[0] != '#') {
                            banCmd.Add(line);
                        }
                }
            }
        }
        public void Say(string message, bool opchat = false, bool color = true) {
            if (!_server.props.irc || !IsConnected()) {
                return;
            }
            StringBuilder sb = new StringBuilder(message);

            if(String.IsNullOrEmpty(message.Trim())) {
                message = ".";
            }

            if (color) {
                for (int i = 0; i < 10; i++) {
                    sb.Replace("%" + i, ColorSignal + Colors.MCtoIRC("&" + i));
                    sb.Replace("&" + i, ColorSignal + Colors.MCtoIRC("&" + i));
                }
                for (char ch = 'a'; ch <= 'f'; ch++) {
                    sb.Replace("%" + ch, ColorSignal + Colors.MCtoIRC("&" + ch));
                    sb.Replace("&" + ch, ColorSignal + Colors.MCtoIRC("&" + ch));
                }
                for (char ch = 'A'; ch <= 'F'; ch++) {
                    sb.Replace("%" + ch, ColorSignal + Colors.MCtoIRC("&" + ch));
                    sb.Replace("&" + ch, ColorSignal + Colors.MCtoIRC("&" + ch));
                }
            }

            // Send the message accordingly
            if (opchat && !String.IsNullOrEmpty(opchannel)) { // Opchat
                connection.Sender.PublicMessage(opchannel, sb.ToString());
            }
            else if (!opchat && !String.IsNullOrEmpty(channel)) { // Regular chat
                connection.Sender.PublicMessage(channel, sb.ToString());
            }
        }

        public void Pm(string user, string message) {
            if (_server.props.irc && IsConnected()) {
                connection.Sender.PrivateMessage(user, message);
            }
        }

        public void Reset() {
            if (!_server.props.irc) {
                return;
            }
            reset = true;
            retries = 0;
            Disconnect("IRC Bot resetting...");
            Connect();
        }

        void Listener_OnJoin(UserInfo user, string channel) {
            doJoinLeaveMessage(user.Nick, "joined", channel);
        }

        void Listener_OnPart(UserInfo user, string channel, string reason) {
            if (user.Nick == nick) {
                return;
            }
            doJoinLeaveMessage(user.Nick, "left", channel);
        }

        private void doJoinLeaveMessage(string who, string verb, string channel) {
            _logger.Log(String.Format("{0} has {1} channel {2}", who, verb, channel));
            _server.GlobalMessage(String.Format("{0}[IRC] {1} has {2} the{3} channel", _server.props.IRCColour, who, verb,
                                               (channel == opchannel ? " operator" : String.Empty)));
        }

        void Player_PlayerDisconnect(object sender, PlayerDisconnectedEventArgs e) {
            Player p = (Player)sender;

            if (_server.props.irc && IsConnected() && !p.hidden) {
                connection.Sender.PublicMessage(channel, p.name + " left the game (" + e.Reason + ")");
            }
        }

        void Player_PlayerConnect(object sender, EventArgs e) {
            Player p = (Player)sender;

            if (_server.props.irc && IsConnected()) {
                connection.Sender.PublicMessage(channel, p.name + " joined the game");
            }
        }

        void Listener_OnQuit(UserInfo user, string reason) {
            if (user.Nick == nick) {
                return;
            }
            _logger.Log(user.Nick + " has left IRC");
            _server.GlobalMessage(_server.props.IRCColour + user.Nick + _server.props.DefaultColor + " has left IRC");
        }

        void Listener_OnError(ReplyCode code, string message) {
            _logger.Log("IRC Error: " + message);
        }

        /// <summary>
        /// This method is called when a private message is sent to the bot.
        /// </summary>
        /// <param name="user"> The IRC user that PM'd the bot. </param>
        /// <param name="message"> The message that the bot received. </param>
        void Listener_OnPrivate(UserInfo user, string message) {
            // Check if the user can actually use the IRC bot
            if (!_server.ircControllers.Contains(user.Nick)) {
                Pm(user.Nick, "You are not an IRC controller!");
                return;
            }

            string[] input = message.Split(' ');
            // If the command has parameters, set everything after the first space as the
            // parameters.
            string cmdParams = input.Length > 1 ?
                message.Substring(message.IndexOf(' ')).Trim() : String.Empty;

            // Check if the command is /resetbot or a banned IRC command.
            if (input[0] == "resetbot" || banCmd.Contains(input[0])) {
                Pm(user.Nick, "You cannot use this command from IRC!");
                return;
            }

            // Special case: make sure the correct kick reason is given if none is provided
            if (input[0] == "kick" && input.Length <= 2) {
                cmdParams += " You were kicked by an IRC controller!";
            }

            Command cmd = _server.commands.FindCommand(input[0]);
            if (cmd != null) { // If the command was found
                _logger.Log("IRC Command: /" + message);
                usedCmd = user.Nick; // So we can send a message in case the command fails
                try {
                    _server.ConsoleCommand(cmd, cmdParams);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                    Pm(user.Nick, "Failed command!");
                }
                usedCmd = String.Empty; // There are other places that use this variable
            }
            else {
                Pm(user.Nick, "Unknown command!");
            }
        }

        void Listener_OnPublic(UserInfo user, string channel, string message) {
            //string allowedchars = "1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./!@#$%^*()_+QWERTYUIOPASDFGHJKL:\"ZXCVBNM<>? ";
            // Allowed chars are any ASCII char between 20h/32 and 7Ah/122 inclusive, except for 26h/38 (&) and 60h/96 (`)

            for (byte i = 10; i < 16; i++) {
                message = message.Replace(ColorSignal + i, Colors.IRCtoMC(i).Replace('&', '%'));
            }
            for (byte i = 0; i < 10; i++) {
                message = message.Replace(ColorSignal + i, Colors.IRCtoMC(i).Replace('&', '%'));
            }

            message = message.MCCharFilter();

            if (channel == opchannel) {
                _logger.Log(String.Format("(OPs): [IRC] {0}: {1}", user.Nick, message));
                _server.GlobalMessageOps(String.Format("To Ops &f-{0}[IRC] {1}&f- {2}", _server.props.IRCColour, user.Nick,
                                                      _server.props.profanityFilter ? _server.profanityFilter.Parse(message) : message));
            }
            else {
                _logger.Log(String.Format("[IRC] {0}: {1}", user.Nick, message));
                _server.GlobalMessage(String.Format("{0}[IRC] {1}: &f{2}", _server.props.IRCColour, user.Nick,
                                                   _server.props.profanityFilter ? _server.profanityFilter.Parse(message) : message));
            }
        }

        /// <summary>
        /// This method is called when someone uses /me to illustrate an action in the IRC.
        /// </summary>
        /// <param name="user"> The user in IRC who used /me. </param>
        /// <param name="channel"> The channel where the message came from. </param>
        /// <param name="message"> The action performed. </param>
        void Listener_OnAction(UserInfo user, string channel, string message) {
            // Replace all &'s, which are illegal in the classic client, to %'s.
            // I'm very confused on why there has to be two for loops here though... -Jjp137
            for (byte i = 10; i < 16; i++) {
                message = message.Replace(ColorSignal + i, Colors.IRCtoMC(i).Replace('&', '%'));
            }
            for (byte i = 0; i < 10; i++) {
                message = message.Replace(ColorSignal + i, Colors.IRCtoMC(i).Replace('&', '%'));
            }

            message = message.MCCharFilter(); // Filter out all bad characters

            // Send the message to the appropriate place
            if (channel == opchannel) {
                _logger.Log(String.Format("(OPs): *[IRC] {0} {1}", user.Nick, message));
                _server.GlobalMessageOps(String.Format("To Ops &f-{0}[IRC] {1}&f- *{2}*", _server.props.IRCColour, user.Nick,
                                        _server.props.profanityFilter ? _server.profanityFilter.Parse(message) : message));
            }
            else {
                _logger.Log(String.Format("*[IRC] {0} {1}", user.Nick, message));
                _server.GlobalMessage(String.Format("{0}*[IRC] {1} {2}", _server.props.IRCColour, user.Nick,
                                     _server.props.profanityFilter ? _server.profanityFilter.Parse(message) : message));
            }
        }

        void Listener_OnRegistered() {
            _logger.Log("Connected to IRC!");
            reset = false;
            retries = 0;
            if (_server.props.ircIdentify && _server.props.ircPassword != String.Empty) {
                _logger.Log("Identifying with NickServ");
                connection.Sender.PrivateMessage("nickserv", "IDENTIFY " + _server.props.ircPassword);
            }

            _logger.Log("Joining channels...");

            if (!String.IsNullOrEmpty(channel)) {
                connection.Sender.Join(channel);
            }
            if (!String.IsNullOrEmpty(opchannel)) {
                connection.Sender.Join(opchannel);
            }
        }

        void Listener_OnDisconnected() {
            // This is being called twice, which is why the server kept crashing when it gets
            // disconnected from IRC. I have no idea why, so this will have to do for now. -Jjp137

            /*if (!reset && retries < 3) {
                retries++;
                Connect();
            }*/
        }

        void Listener_OnNick(UserInfo user, string newNick) {
            string key;
            if (newNick.Split('|').Length == 2) {
                key = newNick.Split('|')[1];
                if (key != null && key != String.Empty) {
                    switch (key) {
                    case "AFK":
                        _server.GlobalMessage("[IRC] " + _server.props.IRCColour + user.Nick + _server.props.DefaultColor + " is AFK");
                        _server.ircafkset.Add(user.Nick);
                        break;
                    case "Away":
                        _server.GlobalMessage("[IRC] " + _server.props.IRCColour + user.Nick + _server.props.DefaultColor + " is Away");
                        _server.ircafkset.Add(user.Nick);
                        break;
                    }
                }
            }
            else if (_server.ircafkset.Contains(newNick)) {
                _server.GlobalMessage("[IRC] " + _server.props.IRCColour + newNick + _server.props.DefaultColor + " is back");
                _server.ircafkset.Remove(newNick);
            }
            else {
                _server.GlobalMessage("[IRC] " + _server.props.IRCColour + user.Nick + _server.props.DefaultColor + " is now known as " + newNick);
            }
        }

        void Player_PlayerChat(object sender, PlayerChattedEventArgs e) {
            Player p = (Player)sender;

            if (_server.props.ircColorsEnable == true && _server.props.irc && IsConnected()) {
                Say(p.color + p.prefix + p.name + ": &0" + e.Message, p.opchat);
            }
            if (_server.props.ircColorsEnable == false && _server.props.irc && IsConnected()) {
                Say(p.name + ": " + e.Message, p.opchat);
            }
        }

        public void Connect() {
            if (!_server.props.irc || _server.shuttingDown) {
                return;
            }

            _logger.Log("Connecting to IRC...");

            try {
                connection.Connect();
            }
            catch (Exception e) {
                _logger.Log("Failed to connect to IRC!");
                _logger.ErrorLog(e);
            }
        }

        public void Disconnect(string reason) {
            if (IsConnected()) {
                connection.Disconnect(reason);
                _logger.Log("Disconnected from IRC!");
            }
        }

        public bool IsConnected() {
            if (!_server.props.irc) {
                return false;
            }
            try {
                return connection.Connected;
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                return false;
            }
        }
    }
}

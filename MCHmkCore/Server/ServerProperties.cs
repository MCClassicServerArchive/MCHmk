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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MCHmk {
    public class ServerProperties {
        /// <summary>
        /// The server's unique salt for minecraft.net clients.
        /// </summary>
        public string minecraftSalt = String.Empty;
        /// <summary>
        /// The server's unique salt for ClassiCube clients.
        /// </summary>
        public string classicubeSalt = String.Empty;

        /// <summary>
        /// The name of the server, as seen in the server list.
        /// </summary>
        public string name = "Default Server Name";
        /// <summary>
        /// The message that appears on the loading screen when a player joins the server.
        /// </summary>
        public string motd = "Welcome!";
        /// <summary>
        /// The texture URL for clients that support the SetEnvAppearance packet.
        /// </summary>
        public string textureUrl = String.Empty;
        /// <summary>
        /// The maximum amount of players that can be on the server at once.
        /// </summary>
        public byte maxPlayers = 12;
        /// <summary>
        /// The maximum amount of guests that can be on the server at once.
        /// </summary>
        public byte maxGuests = 10;

        /// <summary>
        /// The maximum amount of maps that can be loaded at once.
        /// </summary>
        public byte maps = 5;
        /// <summary>
        /// The port that the server uses.
        /// </summary>
        public int port = 25565;
        /// <summary>
        /// Whether the server is public or not.
        /// </summary>
        public bool pub = true;
        /// <summary>
        /// Whether player names should be verified with minecraft.net.
        /// </summary>
        public bool verify = true;
        /// <summary>
        /// Whether chat is sent to everyone on the server by default. If not, chat is sent to 
        /// a player's current level instead.
        /// </summary>
        public bool worldChat = true;

        /// <summary>
        /// Whether spam should be checked for.
        /// </summary>
        public bool checkspam = false;
        /// <summary>
        /// The number of messages that a player must send in a row before that player is 
        /// considered to be spamming.
        /// </summary>
        public int spamcounter = 8;
        /// <summary>
        /// The number of seconds that a player should be muted for when spam is detected.
        /// </summary>
        public int mutespamtime = 60;
        /// <summary>
        /// The number of seconds before a player's spam counter is reset.
        /// </summary>
        public int spamcountreset = 5;

        /// <summary>
        /// The mood of the host.
        /// </summary>
        public string ZallState = "Alive";

        /// <summary>
        /// The name of the main level.
        /// </summary>
        public string level = "main";
        /// <summary>
        /// The filename of the error log.
        /// </summary>
        public string errlog = "error.log";

        /// <summary>
        /// Whether the IRC bot is enabled for the server.
        /// </summary>
        public bool irc = false;
        /// <summary>
        /// Whether the IRC bot transmits colored messages to the IRC channel.
        /// </summary>
        public bool ircColorsEnable = false;
        /// <summary>
        /// The port that the IRC bot uses to connect to the server.
        /// </summary>
        public int ircPort = 6667;
        /// <summary>
        /// The nickname of the IRC bot.
        /// </summary>
        public string ircNick = "HmkBot";
        /// <summary>
        /// The IRC server that the bot should connect to.
        /// </summary>
        public string ircServer = "irc.change.me";
        /// <summary>
        /// The IRC channel that the bot should transmit normal chat to.
        /// </summary>
        public string ircChannel = "#changethis";
        /// <summary>
        /// The IRC channel that the bot should transmit opchat to.
        /// </summary>
        public string ircOpChannel = "#changethistoo";
        /// <summary>
        /// Whether the IRC bot should identify with NickServ.
        /// </summary>
        public bool ircIdentify = false;
        /// <summary>
        /// The NickServ password that the IRC bot uses to identify.
        /// </summary>
        public string ircPassword = String.Empty;
        /// <summary>
        /// Whether the admin verification system is enabled.
        /// </summary>
        public bool verifyadmins = true;
        /// <summary>
        /// The minimum permission level of those who need to verify with the admin verification
        /// system.
        /// </summary>
        public int verifyadminsrank = DefaultRankValue.Operator;

        /// <summary>
        /// Whether the anti-tunneling system is in effect.
        /// </summary>
        public bool antiTunnel = true;
        /// <summary>
        /// The maximum depth that guests can dig down to.
        /// </summary>
        public byte maxDepth = 4;
        /// <summary>
        /// The maximum number of blocks that can be modified by /restartphysics when special
        /// arguments are used.
        /// </summary>
        public int rpLimit = 500;
        /// <summary>
        /// The maximum number of blocks that can be modified by /restartphysics when the 'normal'
        /// argument is provided.
        /// </summary>
        public int rpNormLimit = 10000;

        /// <summary>
        /// The number of seconds between backups. (Not entirely true. -Jjp137)
        /// </summary>
        public int backupInterval = 300;
        /// <summary>
        /// The number of seconds between SQL table updates.
        /// </summary>
        public int blockInterval = 60;
        /// <summary>
        /// The location of map backups.
        /// </summary>
        public string backupLocation = Path.Combine(Application.StartupPath, "levels", "backups");

        /// <summary>
        /// Whether physics restart automatically by default.
        /// </summary>
        public bool physicsRestart = true;
        /// <summary>
        /// Whether messages displaying a person's death count should be displayed.
        /// </summary>
        public bool deathcount = true;
        /// <summary>
        /// Whether maps are automatically loaded when someone goes to an unloaded map.
        /// </summary>
        public bool AutoLoad = false;
        /// <summary>
        /// The maximum number of physics changes that can be undone.
        /// </summary>
        public int physUndo = 20000;
        /// <summary>
        /// The maximum number of players tracked for undo purposes.
        /// </summary>
        public int totalUndo = 200;
        /// <summary>
        /// Whether emoticons should be displayed by default.
        /// </summary>
        public bool parseSmiley = true;
        /// <summary>
        /// Whether the whitelist is being used on the server.
        /// </summary>
        public bool useWhitelist = false;
        /// <summary>
        /// Whether cuboid operations are performed even if the cuboid is too large. If true,
        /// cuboid operations are carried out up until a player's cuboid limit.
        /// </summary>
        public bool forceCuboid = false;
        /// <summary>
        /// Whether the profanity filter is enabled.
        /// </summary>
        public bool profanityFilter = false;
        /// <summary>
        /// Whether ops can be ignored.
        /// </summary>
        public bool globalignoreops = false;

        /// <summary>
        /// Whether MySQL should be used. If false, SQLite3 is used.
        /// </summary>
        public bool useMySQL = false;
        /// <summary>
        /// The IP address of the MySQL server.
        /// </summary>
        public string MySQLHost = "127.0.0.1";
        /// <summary>
        /// The port that is used to connect to the MySQL server.
        /// </summary>
        public string MySQLPort = "3306";
        /// <summary>
        /// The username to use to connect to the MySQL server.
        /// </summary>
        public string MySQLUsername = "root";
        /// <summary>
        /// The password to use to connect to the MySQL server.
        /// </summary>
        public string MySQLPassword = "password";
        /// <summary>
        /// The MySQL database where data is being kept.
        /// </summary>
        public string MySQLDatabaseName = "MCHmkDB";
        /// <summary>
        /// Whether database pooling is enabled.
        /// </summary>
        public bool DatabasePooling = true;

        /// <summary>
        /// The string representing the color used for most non-chat messages.
        /// </summary>
        public string DefaultColor = "&e";
        /// <summary>
        /// The string representing the color used for IRC nicknames.
        /// </summary>
        public string IRCColour = "&5";

        /// <summary>
        /// The number of minutes a player must be idle before that player could be considered AFK.
        /// </summary>
        public int afkminutes = 10;
        /// <summary>
        /// The number of minutes that a player must be idle for before that player is kicked for
        /// being AFK.
        /// </summary>
        public int afkkick = 45;
        /// <summary>
        /// The minimum permission level of those that are immune to AFK kicks.
        /// </summary>
        public int afkkickperm = DefaultRankValue.AdvBuilder;

        /// <summary>
        /// The default rank that a player entering a server for the first time has.
        /// </summary>
        public string defaultRank = "guest";

        /// <summary>
        /// Whether a message should appear when someone uses /invincible.
        /// </summary>
        public bool cheapMessage = true;
        /// <summary>
        /// The message that appears when someone uses /invincible.
        /// </summary>
        public string cheapMessageGiven = " is now being cheap and being immortal";
        /// <summary>
        /// Whether a custom ban message should be used instead of the default one.
        /// </summary>
        public bool customBan = false;
        /// <summary>
        /// The custom ban message.
        /// </summary>
        public string customBanMessage = "You're banned!";
        /// <summary>
        /// Whether a custom shutdown message should be used instead of the default one.
        /// </summary>
        public bool customShutdown = false;
        /// <summary>
        /// The custom shutdown message.
        /// </summary>
        public string customShutdownMessage = "Server shutdown. Rejoin in 10 seconds.";
        /// <summary>
        /// The default message that appears when /promote is used.
        /// </summary>
        public string customPromoteMessage = "&6Congratulations for working hard and getting &2PROMOTED!";
        /// <summary>
        /// The default message that appears when /demote is used.
        /// </summary>
        public string customDemoteMessage =
            "&4DEMOTED! &6We're sorry for your loss. Good luck on your future endeavors! &1:'(";
        /// <summary>
        /// The name of the server's currency.
        /// </summary>
        public string moneys = "moneys";
        /// <summary>
        /// The permission level required to view opchat.
        /// </summary>
        public int opchatperm = DefaultRankValue.Operator;
        /// <summary>
        /// The permission level required to view adminchat.
        /// </summary>
        public int adminchatperm = DefaultRankValue.Admin;
        /// <summary>
        /// Whether players are not notified if an admin joins.
        /// </summary>
        public bool adminsjoinsilent = false;

        /// <summary>
        /// The username of the server owner.
        /// </summary>
        public string server_owner = "Notch";

        /// <summary>
        /// Whether /hackrank kicks players that use it.
        /// </summary>
        public bool hackrank_kick = true;
        /// <summary>
        /// The number of seconds that pass before /hackrank kicks a player. It is in seconds.
        /// </summary>
        public int hackrank_kick_time = 5;

        /// <summary>
        /// Whether /players shows ranks with no players online.
        /// </summary>
        public bool showEmptyRanks = false;

        /// <summary>
        /// The number of seconds a player must wait before '/review enter' can be used again.
        /// </summary>
        public int reviewcooldown = 600;
        /// <summary>
        /// The minimum permission value needed to use '/review enter'.
        /// </summary>
        public int reviewenter = DefaultRankValue.Guest;
        /// <summary>
        /// The minimum permission value needed to use '/review leave'.
        /// </summary>
        public int reviewleave = DefaultRankValue.Guest;
        /// <summary>
        /// The minimum permission value needed to use '/review view'.
        /// </summary>
        public int reviewview = DefaultRankValue.Operator;
        /// <summary>
        /// The minimum permission value needed to use '/review next'.
        /// </summary>
        public int reviewnext = DefaultRankValue.Operator;
        /// <summary>
        /// The minimum permission value needed to use '/review clear'.
        /// </summary>
        public int reviewclear = DefaultRankValue.Operator;

        /// <summary>
        /// Whether those of lower rank can /tp to those of higher rank.
        /// </summary>
        public bool higherranktp = true;
        /// <summary>
        /// Whether a player must use /agree before he can build.
        /// </summary>
        public bool agreetorulesonentry = false;

        /// <summary>
        /// Whether block changes from certain commands should, by default, be queued instead of 
        /// sent immediately.
        /// </summary>
        public bool bufferblocks = true;

        /// <summary>
        /// A reference to the server's logger.
        /// </summary>
        private Logger _l;

        /// <summary>
        /// Generates a salt.
        /// </summary>
        /// <returns> The salt that was generated. </returns>
        private string GenerateSalt() {
            RandomNumberGenerator prng = RandomNumberGenerator.Create();
            StringBuilder sb = new StringBuilder();
            byte[] oneChar = new byte[1];
            while (sb.Length < 16) {
                prng.GetBytes(oneChar);
                if (Char.IsLetterOrDigit((char)oneChar[0])) {
                    sb.Append((char)oneChar[0]);
                }
            }
            return sb.ToString();
        }

        public void LoadProperties(Logger l, string givenPath) {
            // TODO: replace crappy dependency injection
            _l = l;

            this.minecraftSalt = GenerateSalt();
            this.classicubeSalt = GenerateSalt();

            if (File.Exists (givenPath)) {
                string[] lines = File.ReadAllLines (givenPath);

                // TODO: catch specific exceptions in catch blocks
                foreach (string line in lines) {
                    if (line != String.Empty && line [0] != '#') {
                        string key = line.Split('=')[0].Trim ();
                        string value = String.Empty;
                        if (line.IndexOf ('=') >= 0) {
                            value = line.Substring (line.IndexOf ('=') + 1).Trim ();    // allowing = in the values
                        }
                        string color = String.Empty;

                        switch (key.ToLower()) {
                        case "server-name":
                            if (ValidString(value, "![]:.,{}~-+()?_/\\' ")) {
                                this.name = value;
                            }
                            else {
                                _l.Log("server-name invalid! setting to default.");
                            }
                            break;
                        case "motd":
                            if (ValidString(value, "=![]&:.,{}~-+()?_/\\' ")) {
                                // allow = in the motd
                                this.motd = value;
                            }
                            else {
                                _l.Log("motd invalid! setting to default.");
                            }
                            break;
                        case "port":
                            try {
                                this.port = Convert.ToInt32(value);
                            }
                            catch {
                                _l.Log("port invalid! setting to default.");
                            }
                            break;
                        case "verify-names":
                            this.verify = (value.ToLower() == "true") ? true : false;
                            break;
                        case "public":
                            this.pub = (value.ToLower() == "true") ? true : false;
                            break;
                        case "world-chat":
                            this.worldChat = (value.ToLower() == "true") ? true : false;
                            break;
                        case "max-players":
                            try {
                                if (Convert.ToByte(value) > 128) {
                                    value = "128";
                                    _l.Log("Max players has been lowered to 128.");
                                }
                                else if (Convert.ToByte(value) < 1) {
                                    value = "1";
                                    _l.Log("Max players has been increased to 1.");
                                }
                                this.maxPlayers = Convert.ToByte(value);
                            }
                            catch {
                                _l.Log("max-players invalid! setting to default.");
                            }
                            break;
                        case "max-guests":
                            try {
                                if (Convert.ToByte(value) > this.maxPlayers) {
                                    value = this.maxPlayers.ToString();
                                    _l.Log("Max guests has been lowered to " + this.maxPlayers.ToString());
                                }
                                else if (Convert.ToByte(value) < 0) {
                                    value = "0";
                                    _l.Log("Max guests has been increased to 0.");
                                }
                                this.maxGuests = Convert.ToByte(value);
                            }
                            catch {
                                _l.Log("max-guests invalid! setting to default.");
                            }
                            break;
                        case "max-maps":
                            try {
                                if (Convert.ToByte(value) > 100) {
                                    value = "100";
                                    _l.Log("Max maps has been lowered to 100.");
                                }
                                else if (Convert.ToByte(value) < 1) {
                                    value = "1";
                                    _l.Log("Max maps has been increased to 1.");
                                }
                                this.maps = Convert.ToByte(value);
                            }
                            catch {
                                _l.Log("max-maps invalid! setting to default.");
                            }
                            break;
                        case "irc":
                            this.irc = (value.ToLower() == "true") ? true : false;
                            break;
                        case "irc-colorsenable":
                            this.ircColorsEnable = (value.ToLower() == "true") ? true : false;
                            break;
                        case "irc-server":
                            this.ircServer = value;
                            break;
                        case "irc-nick":
                            this.ircNick = value;
                            break;
                        case "irc-channel":
                            this.ircChannel = value;
                            break;
                        case "irc-opchannel":
                            this.ircOpChannel = value;
                            break;
                        case "irc-port":
                            try {
                                this.ircPort = Convert.ToInt32(value);
                            }
                            catch {
                                _l.Log("irc-port invalid! setting to default.");
                            }
                            break;
                        case "irc-identify":
                            try {
                                this.ircIdentify = Convert.ToBoolean(value);
                            }
                            catch {
                                _l.Log("irc-identify boolean value invalid! Setting to the default of: " + this.ircIdentify + ".");
                            }
                            break;
                        case "irc-password":
                            this.ircPassword = value;
                            break;
                        case "anti-tunnels":
                            this.antiTunnel = (value.ToLower() == "true") ? true : false;
                            break;
                        case "max-depth":
                            try {
                                this.maxDepth = Convert.ToByte(value);
                            }
                            catch {
                                _l.Log("maxDepth invalid! setting to default.");
                            }
                            break;

                        case "rplimit":
                            try {
                                this.rpLimit = Convert.ToInt16(value);
                            }
                            catch {
                                _l.Log("rpLimit invalid! setting to default.");
                            }
                            break;
                        case "rplimit-norm":
                            try {
                                this.rpNormLimit = Convert.ToInt16(value);
                            }
                            catch {
                                _l.Log("rpLimit-norm invalid! setting to default.");
                            }
                            break;
                        case "backup-time":
                            if (Convert.ToInt32(value) > 1) {
                                this.backupInterval = Convert.ToInt32(value);
                            }
                            break;
                        case "backup-location":
                            if (!value.Contains("System.Windows.Forms.TextBox, Text:")) {
                                this.backupLocation = value;
                            }
                            break;

                        case "physicsrestart":
                            this.physicsRestart = (value.ToLower() == "true") ? true : false;
                            break;
                        case "deathcount":
                            this.deathcount = (value.ToLower() == "true") ? true : false;
                            break;
                        case "usemysql":
                            this.useMySQL = (value.ToLower() == "true") ? true : false;
                            break;
                        case "host":
                            this.MySQLHost = value;
                            break;
                        case "sqlport":
                            this.MySQLPort = value;
                            break;
                        case "username":
                            this.MySQLUsername = value;
                            break;
                        case "password":
                            this.MySQLPassword = value;
                            break;
                        case "databasename":
                            this.MySQLDatabaseName = value;
                            break;
                        case "pooling":
                            try {
                                this.DatabasePooling = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "defaultcolor":
                            color = Colors.Parse(value);
                            if (color == String.Empty) {
                                color = Colors.Name(value);
                                if (color != String.Empty) {
                                    color = value;
                                }
                                else {
                                    _l.Log("Could not find " + value);
                                    return;
                                }
                            }
                            this.DefaultColor = color;
                            break;
                        case "irc-color":
                            color = Colors.Parse(value);
                            if (color == String.Empty) {
                                color = Colors.Name(value);
                                if (color != String.Empty) {
                                    color = value;
                                }
                                else {
                                    _l.Log("Could not find " + value);
                                    return;
                                }
                            }
                            this.IRCColour = color;
                            break;
                        case "opchat-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.opchatperm = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ".  Using default.");
                                break;
                            }
                            break;
                        case "adminchat-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.adminchatperm = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ".  Using default.");
                                break;
                            }
                            break;
                        case "force-cuboid":
                            try {
                                this.forceCuboid = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ".  Using default.");
                                break;
                            }
                            break;
                        case "profanity-filter":
                            try {
                                this.profanityFilter = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "cheapmessage":
                            try {
                                this.cheapMessage = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "cheap-message-given":
                            if (value != String.Empty) {
                                this.cheapMessageGiven = value;
                            }
                            break;
                        case "custom-ban":
                            try {
                                this.customBan = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "custom-ban-message":
                            if (value != String.Empty) {
                                this.customBanMessage = value;
                            }
                            break;
                        case "custom-shutdown":
                            try {
                                this.customShutdown = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "custom-shutdown-message":
                            if (value != String.Empty) {
                                this.customShutdownMessage = value;
                            }
                            break;
                        case "custom-promote-message":
                            if (value != String.Empty) {
                                this.customPromoteMessage = value;
                            }
                            break;
                        case "custom-demote-message":
                            if (value != String.Empty) {
                                this.customDemoteMessage = value;
                            }
                            break;
                        case "default-rank":
                            try {
                                this.defaultRank = value.ToLower();
                            }
                            catch {
                            }
                            break;
                        case "afk-minutes":
                            try {
                                this.afkminutes = Convert.ToInt32(value);
                            }
                            catch {
                                _l.Log("irc-port invalid! setting to default.");
                            }
                            break;
                        case "afk-kick":
                            try {
                                this.afkkick = Convert.ToInt32(value);
                            }
                            catch {
                                _l.Log("irc-port invalid! setting to default.");
                            }
                            break;
                        case "afk-kick-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.afkkickperm = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ".  Using default.");
                                break;
                            }
                            break;
                        case "autoload":
                            try {
                                this.AutoLoad = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "parse-emotes":
                            try {
                                this.parseSmiley = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                                break;
                            }
                            break;
                        case "use-whitelist":
                            this.useWhitelist = (value.ToLower() == "true") ? true : false;
                            break;
                        case "allow-tp-to-higher-ranks":
                            this.higherranktp = (value.ToLower() == "true") ? true : false;
                            break;
                        case "agree-to-rules-on-entry":
                            try {
                                this.agreetorulesonentry = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "admins-join-silent":
                            try {
                                this.adminsjoinsilent = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "main-name":
                            if (Player.ValidName(value)) {
                                this.level = value;
                            }
                            else {
                                _l.Log("Invalid main name");
                            }
                            break;
                        case "money-name":
                            if (value != String.Empty) {
                                this.moneys = value;
                            }
                            break;
                        case "host-state":
                            if (value != String.Empty) {
                                this.ZallState = value;
                            }
                            break;
                        case "kick-on-hackrank":
                            try {
                                this.hackrank_kick = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "hackrank-kick-time":
                            try {
                                this.hackrank_kick_time = int.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "server-owner":
                            if (value != String.Empty) {
                                this.server_owner = value;
                            }
                            break;
                        case "ignore-ops":
                            try {
                                this.globalignoreops = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "admin-verification":
                            try {
                                this.verifyadmins = bool.Parse(value);
                            }
                            catch {
                                _l.Log("invalid " + key + ". Using default");
                            }
                            break;
                        case "verify-admin-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.verifyadminsrank = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ".  Using default.");
                                break;
                            }
                            break;
                        case "mute-on-spam":
                            try {
                                this.checkspam = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "spam-messages":
                            try {
                                this.spamcounter = int.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "spam-mute-time":
                            try {
                                this.mutespamtime = int.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "spam-counter-reset-time":
                            try {
                                this.spamcountreset = int.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "show-empty-ranks":
                            try {
                                this.showEmptyRanks = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;
                        case "total-undo":
                            try {
                                this.totalUndo = int.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default");
                            }
                            break;

                        case "review-view-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.reviewview = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "review-enter-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.reviewenter = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "review-leave-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.reviewleave = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "review-cooldown":
                            try {
                                this.reviewcooldown = Convert.ToInt32(value.ToLower());
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "review-clear-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.reviewclear = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "review-next-perm":
                            try {
                                sbyte parsed = sbyte.Parse(value);
                                if (parsed < -50 || parsed > 120) {
                                    throw new FormatException();
                                }
                                this.reviewnext = parsed;
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        case "bufferblocks":
                            try {
                                this.bufferblocks = bool.Parse(value);
                            }
                            catch {
                                _l.Log("Invalid " + key + ". Using default.");
                            }
                            break;
                        }
                    }
                }
                SaveProperties(givenPath);
            }
            else {
                SaveProperties(givenPath);
            }
        }

        private bool ValidString(string str, string allowed) {
            string allowedchars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz01234567890" + allowed;
            foreach (char ch in str) {
                if (allowedchars.IndexOf(ch) == -1) {
                    return false;
                }
            }
            return true;
        }

        public void SaveProperties(string givenPath) {
            try {
                File.Create(givenPath).Dispose();
                using (StreamWriter w = File.CreateText(givenPath)) {
                    if (givenPath.IndexOf("server") != -1) {
                        GenerateText(w);
                    }
                }
            }
            catch (Exception e) {
                _l.ErrorLog(e);
                _l.Log("SAVE FAILED! " + givenPath);
            }
        }
        private void GenerateText(StreamWriter w) {
            w.WriteLine("# Edit the settings below to modify how your server operates.");
            w.WriteLine();
            w.WriteLine("# Essential options");
            w.WriteLine();
            w.WriteLine("# The server's name, which is displayed in the server list.");
            w.WriteLine("server-name = " + this.name);
            w.WriteLine();
            w.WriteLine("# The message that appears on the loading screen when a player connects.");
            w.WriteLine("motd = " + this.motd);
            w.WriteLine();
            w.WriteLine("# The port that the server should use. Default is 25565.");
            w.WriteLine("port = " + this.port.ToString());
            w.WriteLine();
            w.WriteLine("# Whether the server shows up on the server list. Defaults to true.");
            w.WriteLine("public = " + this.pub.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether usernames should be authenticated. Defaults to true.");
            w.WriteLine("verify-names = " + this.verify.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The username of the server's owner.");
            w.WriteLine("server-owner = " + this.server_owner.ToString());
            w.WriteLine();
            w.WriteLine("# The maximum amount of players allowed to be on the server. Default is 12.");
            w.WriteLine("max-players = " + this.maxPlayers.ToString());
            w.WriteLine();
            w.WriteLine("# The maximum amount of guests allowed to be on the server. Default is 10.");
            w.WriteLine("max-guests = " + this.maxGuests.ToString());
            w.WriteLine();
            w.WriteLine("# The name of the main map. Defaults to 'main'.");
            w.WriteLine("main-name = " + this.level);
            w.WriteLine();
            w.WriteLine("# The name of the server's currency. Defaults to 'moneys'.");
            w.WriteLine("money-name = " + this.moneys);
            w.WriteLine();
            w.WriteLine("# The texture URL to use for ClassiCube clients.");
            w.WriteLine("texture-url = " + this.textureUrl);
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Player options");
            w.WriteLine();
            w.WriteLine("# The rank that a player joining the server for the first time is set to.");
            try {
                w.WriteLine("default-rank = " + this.defaultRank);
            }
            catch {
                w.WriteLine("default-rank = guest");
            }
            w.WriteLine();
            w.WriteLine("# Whether a player must agree to the rules before building. Defaults to false.");
            w.WriteLine("agree-to-rules-on-entry = " + this.agreetorulesonentry.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether a whitelist should be used, which allows users to join even if" +
                        " their IP address is banned. Defaults to false.");
            w.WriteLine("use-whitelist = " + this.useWhitelist.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether guests are not allowed to build below a certain depth. Defaults to true.");
            w.WriteLine("anti-tunnels = " + this.antiTunnel.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# If anti-tunnels is true, the maximum depth that guests can build to. Default is 4.");
            w.WriteLine("max-depth = " + this.maxDepth.ToString().ToLower());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Admin verification");
            w.WriteLine();
            w.WriteLine("# Whether the admin verification system is turned on. Defaults to true.");
            w.WriteLine("admin-verification = " + this.verifyadmins.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The permission value of those required to verify. Applies to those who" +
                        " have that permission value and higher. Defaults to 80");
            w.WriteLine("verify-admin-perm = " + ((sbyte)this.verifyadminsrank).ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# AFK options");
            w.WriteLine();
            w.WriteLine("# How many minutes should pass before a player is considered AFK. Default is 10.");
            w.WriteLine("afk-minutes = " + this.afkminutes.ToString());
            w.WriteLine();
            w.WriteLine("# How many minutes should pass before a player is kicked for being AFK." +
                        " Default is 45.");
            w.WriteLine("afk-kick = " + this.afkkick.ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required in order to be immune from being" +
                        " kicked for being AFK.");
            w.WriteLine("afk-kick-perm = " + ((sbyte)this.afkkickperm).ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Messages and chat");
            w.WriteLine();
            w.WriteLine("# Whether chat is server-wide or not. If not, a player's chat can only be" +
                        " seen by those in the same level. Defaults to true");
            w.WriteLine("world-chat = " + this.worldChat.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The permission value required in order to see opchat. Default is 80.");
            w.WriteLine("opchat-perm = " + ((sbyte)this.opchatperm).ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required in order to see adminchat. Default is 100.");
            w.WriteLine("adminchat-perm = " + ((sbyte)this.adminchatperm).ToString());
            w.WriteLine();
            w.WriteLine("# Whether players can ignore ops' chat. Defaults to false.");
            w.WriteLine("ignore-ops = " + this.globalignoreops.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether the profanity filter should be enabled. Defaults to false.");
            w.WriteLine("profanity-filter = " + this.profanityFilter.ToString());
            w.WriteLine();
            w.WriteLine("# Whether emoticons should be displayed in chat. Defaults to true.");
            w.WriteLine("parse-emotes = " + this.parseSmiley.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether messages displaying a player's death count should appear. Defaults to true.");
            w.WriteLine("deathcount = " + this.deathcount.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether players are not notified if an admin joins. Defaults to false.");
            w.WriteLine("admins-join-silent = " + this.adminsjoinsilent.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The current mood of the host.");
            w.WriteLine("host-state = " + this.ZallState.ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Custom messages");
            w.WriteLine();
            w.WriteLine("# Whether the custom ban message should be used. Defaults to false.");
            w.WriteLine("custom-ban = " + this.customBan.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The custom ban message.");
            w.WriteLine("custom-ban-message = " + this.customBanMessage);
            w.WriteLine();
            w.WriteLine("# Whether the custom shutdown message should be used. Defaults to false.");
            w.WriteLine("custom-shutdown = " + this.customShutdown.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The custom shutdown message.");
            w.WriteLine("custom-shutdown-message = " + this.customShutdownMessage);
            w.WriteLine();
            w.WriteLine("# The custom default promote message.");
            w.WriteLine("custom-promote-message = " + this.customPromoteMessage);
            w.WriteLine();
            w.WriteLine("# The custom default demote message.");
            w.WriteLine("custom-demote-message = " + this.customDemoteMessage);
            w.WriteLine();
            w.WriteLine("# Whether a message appears when someone uses /invincible. Defaults to true.");
            w.WriteLine("cheapmessage = " + this.cheapMessage.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The message that is displayed when someone uses /invincible.");
            w.WriteLine("cheap-message-given = " + this.cheapMessageGiven);
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Colors");
            w.WriteLine();
            w.WriteLine("# The color for all server messages (not chat). Defaults to &e");
            w.WriteLine("defaultColor = " + this.DefaultColor);
            w.WriteLine();
            w.WriteLine("# The color for all IRC usernames. Defaults to &5");
            w.WriteLine("irc-color = " + this.IRCColour);
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Spam control");
            w.WriteLine();
            w.WriteLine("# Whether people are muted for spamming. Defaults to false.");
            w.WriteLine("mute-on-spam = " + this.checkspam.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The number of consecutive messages that are considered spam. Default is 8.");
            w.WriteLine("spam-messages = " + this.spamcounter.ToString());
            w.WriteLine();
            w.WriteLine("# The duration in seconds at which a person is muted for spamming. Default is 60.");
            w.WriteLine("spam-mute-time = " + this.mutespamtime.ToString());
            w.WriteLine();
            w.WriteLine("# The time in seconds before the spam counter is reset. Defaults is 6");
            w.WriteLine("spam-counter-reset-time = " + this.spamcountreset.ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Command behavior");
            w.WriteLine();
            w.WriteLine("# Whether a cuboid should be done anyways until it hits the player's block" +
                        " limit. If not, a player cannot start a cuboid that is too big. Defaults to false.");
            w.WriteLine("force-cuboid = " + this.forceCuboid.ToString());
            w.WriteLine();
            w.WriteLine("# The maximum number of blocks that can be affected by /restartphysics" +
                        " when special parameters are given. Default is 500.");
            w.WriteLine("rplimit = " + this.rpLimit.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The maximum number of blocks that can be affected by /restartphysics" +
                        " when no parameters are given. Default is 10000.");
            w.WriteLine("rplimit-norm = " + this.rpNormLimit.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The number of players to track for undo purposes. Default is 200.");
            w.WriteLine("total-undo = " + this.totalUndo.ToString());
            w.WriteLine();
            w.WriteLine("# Whether /players shows ranks with no online players in them. Defaults to false.");
            w.WriteLine("show-empty-ranks = " + this.showEmptyRanks.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether /hackrank kicks people. Defaults to true.");
            w.WriteLine("kick-on-hackrank = " + this.hackrank_kick.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# The time in seconds before someone gets kicked for using /hackrank." +
                        " Default is 5.");
            w.WriteLine("hackrank-kick-time = " + this.hackrank_kick_time.ToString());
            w.WriteLine();
            w.WriteLine("# Whether players can /tp to those of higher ranks. Defaults to true.");
            w.WriteLine("allow-tp-to-higher-ranks = " + this.higherranktp.ToString().ToLower());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Map behavior");
            w.WriteLine();
            w.WriteLine("# The maximum amount of maps allowed to be loaded at one time. Defaults is 5.");
            w.WriteLine("max-maps = " + this.maps.ToString());
            w.WriteLine();
            w.WriteLine("# Whether maps should be loaded automatically if they are unloaded when" +
                        " someone attempts to go a different map. Defaults to false.");
            w.WriteLine("autoload = " + this.AutoLoad.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether physics should remain the same after it shuts down. If not," +
                        " physics is set to 0 when a shutdown occurs. Defaults to true.");
            w.WriteLine("physicsrestart = " + this.physicsRestart.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether blocks should be buffered. Defaults to true.");
            w.WriteLine("bufferblocks = " + this.bufferblocks.ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# Backup options");
            w.WriteLine();
            w.WriteLine("# The amount of time in seconds between autosaves of all loaded maps." +
                        " Default is 300.");
            w.WriteLine("backup-time = " + this.backupInterval.ToString());
            w.WriteLine();
            w.WriteLine("# The location of map backups.");
            w.WriteLine("backup-location = " + this.backupLocation);
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# IRC bot options");
            w.WriteLine();
            w.WriteLine("# Whether the IRC bot is enabled. Defaults to false.");
            w.WriteLine("irc = " + this.irc.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# Whether the bot should use IRC colors. Defaults to false.");
            w.WriteLine("irc-colorsenable = " + this.ircColorsEnable.ToString().ToLower());
            w.WriteLine();
            w.WriteLine("# IRC nickname of the bot.");
            w.WriteLine("irc-nick = " + this.ircNick);
            w.WriteLine();
            w.WriteLine("# The IRC server that the bot should connect to.");
            w.WriteLine("irc-server = " + this.ircServer);
            w.WriteLine();
            w.WriteLine("# The regular IRC channel that the bot should connect to.");
            w.WriteLine("irc-channel = " + this.ircChannel);
            w.WriteLine();
            w.WriteLine("# The operator IRC channel that the bot should connect to." +
                        " Opchat can be seen by people in this channel.");
            w.WriteLine("irc-opchannel = " + this.ircOpChannel);
            w.WriteLine();
            w.WriteLine("# The IRC server's port.");
            w.WriteLine("irc-port = " + this.ircPort.ToString());
            w.WriteLine();
            w.WriteLine("# Whether the bot should identify with NickServ. You must register the" +
                        " bot yourself. Defaults to false.");
            w.WriteLine("irc-identify = " + this.ircIdentify.ToString());
            w.WriteLine();
            w.WriteLine("# The bot's NickServ password. Used if irc-identify is true.");
            w.WriteLine("irc-password = " + this.ircPassword);
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# MySQL options");
            w.WriteLine();
            w.WriteLine("# Whether MySQL should be used. If not, SQLite3 is used. Defaults to false.");
            w.WriteLine("UseMySQL = " + this.useMySQL.ToString());
            w.WriteLine();
            w.WriteLine("# The host name of the MySQL server. Defaults is 127.0.0.1, which is localhost.");
            w.WriteLine("Host = " + this.MySQLHost);
            w.WriteLine();
            w.WriteLine("# The port that the MySQL server uses. Default is 3306.");
            w.WriteLine("SQLPort = " + this.MySQLPort);
            w.WriteLine();
            w.WriteLine("# The username of the MySQL server account that you want to use.");
            w.WriteLine("Username = " + this.MySQLUsername);
            w.WriteLine();
            w.WriteLine("# The password of the MySQL server account.");
            w.WriteLine("Password = " + this.MySQLPassword);
            w.WriteLine();
            w.WriteLine("# The name of the MySQL server database that contains MCHmk data.");
            w.WriteLine("DatabaseName = " + this.MySQLDatabaseName);
            w.WriteLine();
            w.WriteLine("# Whether database pooling should be enabled. Defaults to true.");
            w.WriteLine("Pooling = " + this.DatabasePooling.ToString());
            w.WriteLine();
            w.WriteLine();
            w.WriteLine("# /review options");
            w.WriteLine();
            w.WriteLine("# The permission value required to view the review queue. Default is 80.");
            w.WriteLine("review-view-perm = " + ((sbyte)this.reviewview).ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required to enter the review queue. Default is 0.");
            w.WriteLine("review-enter-perm = " + ((sbyte)this.reviewenter).ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required to leave the review queue. Default is 0.");
            w.WriteLine("review-leave-perm = " + ((sbyte)this.reviewleave).ToString());
            w.WriteLine();
            w.WriteLine("# The period of time in seconds that must pass before /review can be" +
                        " used again. Default is 600.");
            w.WriteLine("review-cooldown = " + this.reviewcooldown.ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required to clear the review queue. Default is 80.");
            w.WriteLine("review-clear-perm = " + ((sbyte)this.reviewclear).ToString());
            w.WriteLine();
            w.WriteLine("# The permission value required to go to the next person in the review " +
                        "queue. Default is 80.");
            w.WriteLine("review-next-perm = " + ((sbyte)this.reviewnext).ToString());
            w.WriteLine();
        }
    }
}

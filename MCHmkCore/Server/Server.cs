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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using System.Threading;
using System.Windows.Forms;

using System.Data.SQLite;
using Newtonsoft.Json.Linq;

using MCHmk.SQL;

namespace MCHmk {
    /// <summary>
    /// The Server class represents the server's global state.
    /// </summary>
    public class Server {
        /// <summary>
        /// The file path of the .exe that started the server.
        /// </summary>
        public string apppath = Application.StartupPath;

        /// <summary>
        /// The IRC bot.
        /// </summary>
        public HmkBot IRC;
        /// <summary>
        /// The thread that modifies the location of players based on certain conditions.
        /// </summary>
        public Thread locationChecker;
        /// <summary>
        /// The thread that writes block changes to the SQL tables.
        /// </summary>
        public Thread blockThread;

        /// <summary>
        /// The version of MCHmk that the server is running.
        /// </summary>
        public Version Version {
            get {
                return System.Reflection.Assembly.GetAssembly(typeof(Server)).GetName().Version;
            }
        }

        /// <summary>
        /// The ClassiCube URL of the server.
        /// </summary>
        public string CCURL = String.Empty;
        /// <summary>
        /// The minecraft.net URL of the server.
        /// </summary>
        public string URL = String.Empty;

        /// <summary>
        /// The server's listening socket.
        /// </summary>
        public Socket listen;

        /// <summary>
        /// Timer that updates player and bot positions every 0.1 seconds.
        /// </summary>
        public System.Timers.Timer updateTimer = new System.Timers.Timer(100);
        /// <summary>
        /// Timer that displays a random message from messages.txt every five minutes.
        /// </summary>
        System.Timers.Timer messageTimer = new System.Timers.Timer(60000 * 5);

        /// <summary>
        /// Whether setting up the server has finished.
        /// </summary>
        public bool ServerSetupFinished = false;

        /// <summary>
        /// The list of banned IP addresses.
        /// </summary>
        public List<string> bannedIP;
        /// <summary>
        /// The list of players on the whitelist.
        /// </summary>
        public UuidList whiteList;
        /// <summary>
        /// The list of IRC users that can control the IRC bot.
        /// </summary>
        public List<string> ircControllers;
        /// <summary>
        /// The list of players that are muted.
        /// </summary>
        public UuidList muted;

        /// <summary>
        /// The list of temporary bans.
        /// </summary>
        public List<TempBan> tempBans = new List<TempBan>();
        /// <summary>
        /// The TempBan struct holds information about a temporary ban.
        /// </summary>
        public struct TempBan {
            /// <summary>
            /// The name of the player that was banned.
            /// </summary>
            public string name;
            /// <summary>
            /// When the player is allowed back into the server.
            /// </summary>
            public DateTime allowedJoin;
        }

        /// <summary>
        /// The map generator.
        /// </summary>
        public MapGenerator MapGen;

        /// <summary>
        /// The PerformanceCounter object that measures statistics about CPU usage.
        /// </summary>
        public PerformanceCounter PCCounter = null;
        /// <summary>
        /// The PerformanceCounter object that measures statistics about processes.
        /// </summary>
        public PerformanceCounter ProcessCounter = null;

        /// <summary>
        /// The main level itself.
        /// </summary>
        public Level mainLevel;
        /// <summary>
        /// The list of levels that are loaded on the server.
        /// </summary>
        public LevelList levels;

        /// <summary>
        /// The list of names of the players that are in the review queue.
        /// </summary>
        public List<string> reviewlist = new List<string>();
        /// <summary>
        /// The list of names of the players that are AFK in the server.
        /// </summary>
        public List<string> afkset = new List<string>();
        /// <summary>
        /// The list of IRC nicknames that are AFK in the IRC channel.
        /// </summary>
        public List<string> ircafkset = new List<string>();
        /// <summary>
        /// The list of global server announcements. A message is picked from this list and
        /// displayed to all players every five minutes.
        /// </summary>
        public List<string> messages = new List<string>();

        /// <summary>
        /// The time that the server went online.
        /// </summary>
        public DateTime timeOnline;
        /// <summary>
        /// The IP address of the server.
        /// </summary>
        public string IP;
        /// <summary>
        /// Whether chat moderation is in effect.
        /// </summary>
        public bool chatmod = false;

        /// <summary>
        /// Whether a vote-kick is in progress.
        /// </summary>
        public bool voteKickInProgress = false;
        /// <summary>
        /// The minimum number of votes needed for a vote-kick to be effective.
        /// </summary>
        public int voteKickVotesNeeded = 0;

        /// <summary>
        /// Dictionary that holds user-defined replacement variables.
        /// </summary>
        public Dictionary<string, string> customdollars = new Dictionary<string, string>();

        /// <summary>
        /// Array of color codes, but without the percent sign in front of them.
        /// </summary>
        public Char[] ColourCodesNoPercent = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        /// <summary>
        /// The number of 'yes' votes for a poll started by the /vote command. (Votekicks do not
        /// use this.)
        /// </summary>
        public int YesVotes = 0;
        /// <summary>
        /// The number of 'no' votes for a poll started by the /vote command. (Votekicks do not
        /// use this.)
        /// </summary>
        public int NoVotes = 0;
        /// <summary>
        /// Whether a regular vote is in progress.
        /// </summary>
        public bool voting = false;

        /// <summary>
        /// Whether the heads of everyone on the server are flipped.
        /// </summary>
        public bool flipHead = false;

        /// <summary>
        /// The version of the classic server protocol that the server uses.
        /// </summary>
        public const byte version = 7;

        /// <summary>
        /// Whether the server is shutting down.
        /// </summary>
        public bool shuttingDown = false;

        /// <summary>
        /// If a server is currently in shutdown mode. 
        /// </summary>
        public bool isInShutDown = false;

        /// <summary>
        /// The logger for this server.
        /// </summary>
        public Logger logger = new Logger();

        /// <summary>
        /// The object responsible for periodically sending the server's heartbeat.
        /// </summary>
        private HeartbeatPumper heart;

        /// <summary>
        /// The object responsible for autosaving levels.
        /// </summary>
        private AutoSaver levelSaver;

        /// <summary>
        /// The object responsible for limiting the rate at which block changes from certain commands get applied.
        /// </summary>
        public BlockQueue blockQueue;

        /// <summary>
        /// The server's profanity filter.
        /// </summary>
        public ProfanityFilter profanityFilter;

        /// <summary>
        /// The server's database.
        /// </summary>
        public Database database;

        /// <summary>
        /// The properties of this server.
        /// </summary>
        public ServerProperties props = new ServerProperties();

        /// <summary>
        /// The list of block permission values for this server.
        /// </summary>
        public BlockPermList blockPerms;

        /// <summary>
        /// The list of commands that are available on this server.
        /// </summary>
        public CommandList commands;

        /// <summary>
        /// The list of ranks on this server.
        /// </summary>
        public RankList ranks = new RankList();
        /// <summary>
        /// The list of all the players on the server that have logged in.
        /// </summary>
        public OnlinePlayerList players = new OnlinePlayerList();

        /// <summary>
        /// A Player object that represents the console.
        /// </summary>
        private Player _console = new Player();

        /// <summary>
        /// Gets the Player object that represents the console.
        /// </summary>
        internal Player ServerConsole {
            get {
                return _console;
            }
        }

        /// <summary>
        /// Constructor for the Server class. Should be called only once.
        /// </summary>
        public Server() { }

        /// <summary>
        /// Initializes and starts the server.
        /// </summary>
        public void Start() {
            // This could have been split into like 20 different methods instead of like
            // one huge blob of code... -Jjp137

            // TODO: move these initializers later to a saner place
            heart = new HeartbeatPumper(this);

            // Start the logger first.
            logger.Init();

            shuttingDown = false;
            logger.Log("Starting the server...");

            // Create random directories.
            if (!Directory.Exists("properties")) { // Properties folder
                Directory.CreateDirectory("properties");
            }
            if (!Directory.Exists("levels")) { // Levels folder
                Directory.CreateDirectory("levels");
            }
            if (!Directory.Exists("bots")) { // Bots folder
                Directory.CreateDirectory("bots");
            }
            if (!Directory.Exists("text")) { // Text folder (for /view and /text)
                Directory.CreateDirectory("text");
            }
            string rankinfoPath = Path.Combine("text", "rankinfo.txt");
            if (!File.Exists(rankinfoPath)) { // /rankinfo file
                File.CreateText(rankinfoPath).Dispose();
            }
            // The bans.txt requires a bit more handling.
            string baninfoPath = Path.Combine("text", "bans.txt");
            if (!File.Exists(baninfoPath)) {
                File.CreateText(baninfoPath).Dispose();
            }
            else {
                string bantext = File.ReadAllText(baninfoPath);
                // Ban.cs uses %20 to separate various parts of each entry instead of ~ and -, so
                // if an old bans.txt is detected, make sure it will work with /banedit and similar commands.
                if (!bantext.Contains("%20") && bantext != String.Empty) {
                    bantext = bantext.Replace("~", "%20");
                    bantext = bantext.Replace("-", "%20");
                    File.WriteAllText(baninfoPath, bantext);
                }
            }

            // Create the extra directory and various directories under it.
            string[] extraPaths = {"extra",  // Stores all the miscellaneous junk
                                   Path.Combine("extra", "undo"),  // Stores undo buffers
                                   Path.Combine("extra", "undoPrevious"),  // Stores even older undo buffers
                                   Path.Combine("extra", "copy"),  // Stores copy files (from /store)
                                   Path.Combine("extra", "copyBackup"),  // Stores deleted copy files
                                   Path.Combine("extra", "Waypoints")  // Stores waypoints
                                  };

            foreach (string path in extraPaths) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
            }

            // Backwards compatibility: if various files are found on the root folder, move them
            // to the proper place.
            try {
                if (File.Exists("server.properties")) {
                    File.Move("server.properties", Path.Combine("properties", "server.properties"));
                }
                if (File.Exists("rules.txt")) {
                    File.Move("rules.txt", Path.Combine("text", "rules.txt"));
                }
                if (File.Exists("welcome.txt")) {
                    File.Move("welcome.txt", Path.Combine("text", "welcome.txt"));
                }
                if (File.Exists("messages.txt")) {
                    File.Move("messages.txt", Path.Combine("text", "messages.txt"));
                }
                if (File.Exists("externalurl.txt")) {
                    File.Move("externalurl.txt", Path.Combine("text", "externalurl.txt"));
                }
                if (File.Exists("autoload.txt")) {
                    File.Move("autoload.txt", Path.Combine("text", "autoload.txt"));
                }
                if (File.Exists("IRC_Controllers.txt")) {
                    File.Move("IRC_Controllers.txt", Path.Combine("ranks", "IRC_Controllers.txt"));
                }
                if (props.useWhitelist) {
                    if (File.Exists("whitelist.txt")) {
                        File.Move("whitelist.txt", Path.Combine("ranks", "whitelist.txt"));
                    }
                }
            }
            catch (Exception e) {
                logger.ErrorLog(e);
            }

            // Initialize the user-defined replacement variables
            string customDollarsFile = Path.Combine("text", "custom$s.txt");
            if (File.Exists(customDollarsFile)) {
                using (StreamReader r = new StreamReader(customDollarsFile)) {
                    string line;
                    while ((line = r.ReadLine()) != null) {
                        if (line.StartsWith("//")) { // These denote comments
                            continue;
                        }
                        var split = line.Split(new[] { ':' }, 2);
                        if (split.Length == 2 && !String.IsNullOrEmpty(split[0])) {
                            customdollars.Add(split[0], split[1]);
                        }
                    }
                }
            }
            else { // Create a default file.
                logger.Log("custom$s.txt does not exist, creating");
                using (StreamWriter SW = File.CreateText(customDollarsFile)) {
                    SW.WriteLine("// This is used to create custom $s");
                    SW.WriteLine("// If you start the line with a // it wont be used");
                    SW.WriteLine("// It should be formatted like this:");
                    SW.WriteLine("// $website:google.com");
                    SW.WriteLine("// That would replace '$website' in any message to 'google.com'");
                    SW.WriteLine("// It must not start with a // and it must not have a space between the 2 sides and the colon (:)");
                    SW.Close();
                }
            }

            // Load the list of players that do not want to see emoticons.
            string emotesPath = Path.Combine("text", "emotelist.txt");
            if (File.Exists(emotesPath)) {
                // TODO: this is dumb
                Uuid.ParsePlayerList(Player.emoteList, emotesPath);
            }
            else { // If that list doesn't exist, create one.
                File.Create(emotesPath).Dispose();
            }

            // Load many other settings.
            LoadAllSettings();

            // This variable is used for uptime calculations, so set it.
            timeOnline = DateTime.Now;

            if (props.useMySQL) {
                try {
                    database = new MySQLDatabase(props.MySQLHost, props.MySQLPort, props.MySQLUsername,
                                                 props.MySQLPassword, props.MySQLDatabaseName, props.DatabasePooling);
                }
                catch (DbException e) {
                    logger.ErrorLog(e);
                    logger.Log("MySQL settings have not been set! Please setup by editing server.properties.");
                    return;
                }
            }
            else {
                database = new SQLiteDatabase(Path.Combine(apppath, "MCHmk.db"), props.DatabasePooling);
            }

            // If the Players table doesn't exist, create it.
            database.ExecuteStatement(string.Format("CREATE TABLE if not exists Players (ID INTEGER {0}AUTO{1}INCREMENT NOT NULL, Name VARCHAR(20), IP CHAR(15), FirstLogin DATETIME, LastLogin DATETIME, totalLogin MEDIUMINT, Title CHAR(20), TotalDeaths SMALLINT, Money BIGINT UNSIGNED, totalBlocks BIGINT, totalCuboided BIGINT, totalKicked MEDIUMINT, TimeSpent VARCHAR(20), color VARCHAR(6), title_color VARCHAR(6){2});",
                                                (props.useMySQL ? String.Empty : "PRIMARY KEY "), (props.useMySQL ? "_" : String.Empty), (props.useMySQL ? ", PRIMARY KEY (ID)" : String.Empty)));

            UpdateOldMySQLDatabase();

            // Create the uuids table if it does not exist.
            Uuid.CreateUuidTable(database);

            // Make sure no levels are loaded (maybe this is here for /server).
            if (levels != null) {
                foreach (Level l in levels) {
                    l.Unload();
                }
            }

            // Attempt to find a main level.
            try {
                levels = new LevelList(props.maps);
                MapGen = new MapGenerator();

                // If a level matching the main level's name exists, load it.
                string mcfPath = Path.Combine("levels", props.level + ".mcf");
                string lvlPath = Path.Combine("levels", props.level + ".lvl");

                if (File.Exists(mcfPath) || File.Exists(lvlPath)) {
                    // Prefer .mcf format first.
                    if (File.Exists(mcfPath)) {
                        mainLevel = Level.Load(this, props.level);
                    }
                    else {
                        mainLevel = Level.Load(this, props.level, 0, true);
                    }

                    // You can never unload the main level.
                    mainLevel.unload = false;

                    if (mainLevel == null) { // Attempt to load the backup if it exists.
                        string backupMcfPath = Path.Combine("levels", props.level + ".mcf.backup");

                        if (File.Exists(backupMcfPath)) {
                            logger.Log("Attempting to load backup of " + props.level + ".");
                            File.Copy(backupMcfPath, mcfPath, true);
                            mainLevel = Level.Load(this, props.level);
                            // If the backup cannot be loaded either, stop setting up the server.
                            if (mainLevel == null) {
                                logger.Log("BACKUP FAILED!");
                                Console.ReadLine();
                                return;
                            }
                        }
                        else { // If the backup can't be found, a main level must be made.
                            logger.Log("mainlevel not found");
                            mainLevel = new Level(this, props.level, 128, 64, 128, "flat") {
                                permissionvisit = DefaultRankValue.Guest, permissionbuild = DefaultRankValue.Guest
                            };
                            mainLevel.Save();
                            Level.CreateLeveldb(database, props.level);
                        }
                    }
                }
                else { // If the main level isn't found, create it and save it.
                    logger.Log("mainlevel not found");
                    mainLevel = new Level(this, props.level, 128, 64, 128, "flat") {
                        permissionvisit = DefaultRankValue.Guest, permissionbuild = DefaultRankValue.Guest
                    };
                    mainLevel.Save();
                    Level.CreateLeveldb(database, props.level);
                }

                // Add the level to the server's level list.
                addLevel(mainLevel);

                // Make sure the level has a physics thread.
                if (mainLevel.physic.physThread == null) {
                    mainLevel.physic.StartPhysics(mainLevel);
                }
            }
            catch (Exception e) {
                logger.ErrorLog(e);
            }

            bannedIP = new List<string>();
            ircControllers = new List<string>();

            // Load the list of banned IPs.
            // TODO: refactor this
            string bannedIpPath = Path.Combine("ranks", "banned-ip.txt");
            if (File.Exists(bannedIpPath)) {
                foreach (string line in File.ReadAllLines(bannedIpPath)) {
                    bannedIP.Add(line.Trim());
                }
            }
            else {
                File.Create(bannedIpPath).Close();
                logger.Log("CREATED NEW: ranks/banned-ip.txt");
            }

            // Load the list of IRC users that can communicate with the IRC bot.
            // TODO: refactor this
            string ircControllersPath = Path.Combine("ranks", "IRC_Controllers.txt");
            if (File.Exists(ircControllersPath)) {
                foreach (string line in File.ReadAllLines(ircControllersPath)) {
                    ircControllers.Add(line.Trim());
                }
            }
            else {
                File.Create(ircControllersPath).Close();
                logger.Log("CREATED NEW: ranks/IRC_Controllers.txt");
            }

            // Load all muted players.
            string mutedPath = Path.Combine("ranks", "muted.txt");
            if (!File.Exists(mutedPath)) {  // TODO: refactor this part
                logger.Log("CREATED NEW: ranks/muted.txt");
            }
            muted = UuidList.Load("muted.txt", null);

            // Load the whitelist if the server uses it.
            if (props.useWhitelist) {
                if (!File.Exists(Path.Combine("ranks", "whitelist.txt"))) {  // TODO: refactor this part
                    logger.Log("CREATED NEW: ranks/whitelist.txt");
                }
                whiteList = UuidList.Load("whitelist.txt", null);
            }

            // Load the list of players that are jailed.
            string jailedPath = Path.Combine("ranks", "jailed.txt");
            if (!File.Exists(jailedPath)) {
                File.Create(jailedPath).Close();
                logger.Log("CREATED NEW: ranks/jailed.txt");
            }

            // Lowercase the names of players in the banned and muted player lists.
            Extensions.UncapitalizeAll(Path.Combine("ranks", "banned.txt"));
            Extensions.UncapitalizeAll(mutedPath);

            // Load any levels in autoload.txt.
            string autoloadPath = Path.Combine("text", "autoload.txt");
            if (File.Exists(autoloadPath)) {
                try {
                    string[] lines = File.ReadAllLines(autoloadPath);
                    foreach (string _line in lines.Select(line => line.Trim())) {
                        try {
                            if (_line == String.Empty) { // Ignore blank lines
                                continue;
                            }
                            if (_line[0] == '#') { // Ignore commented lines
                                continue;
                            }

                            // Get the level name and the physics value.
                            string key = _line.Split('=')[0].Trim();
                            string value;
                            try {
                                value = _line.Split('=')[1].Trim();
                            }
                            catch {  // TODO: find exact exception to catch or rewrite logic
                                value = "0";
                            }

                            // Don't load the main level since that was already done earlier.
                            // Otherwise, load the level with the physics value.
                            if (!key.Equals(mainLevel.name)) {
                                commands.FindCommand("load").Use(_console, key + " " + value);
                            }
                            else { // If it is the main level, set its physics value.
                                try {
                                    int temp = int.Parse(value);
                                    if (temp >= 0 && temp <= 3) {
                                        mainLevel.setPhysics(temp);
                                    }
                                }
                                catch {  // TODO: find exact exception to catch
                                    logger.Log("Physics variable invalid");
                                }
                            }


                        }
                        catch (Exception) {
                            logger.Log(_line + " failed.");
                        }
                    }
                }
                catch (Exception e) {
                    logger.ErrorLog(e);
                    logger.Log("autoload.txt error");
                }
            }
            else {
                logger.Log("autoload.txt does not exist");
            }

            // Create the listening socket.
            logger.Log("Creating listening socket on port " + props.port + "... ");
            Setup();

            // Set this timer to update the positions of all players and bots every time the timer
            // expires. This occurs every 0.1 seconds.
            updateTimer.Elapsed += delegate {
                GlobalUpdate();
                PlayerBot.GlobalUpdatePosition();
            };
            // ...and start the timer.
            updateTimer.Start();

            // Set up the server's heartbeat.
            try {
                heart.Init();
            }
            catch (Exception e) {
                logger.ErrorLog(e);
            }

            // Set this timer to display a random message from messages.txt every 5 minutes to
            // all players.
            messageTimer.Elapsed += delegate {
                RandomMessage();
            };
            // ...and start the timer.
            messageTimer.Start();

            // Handle messages.txt by either reading from it or creating it if it doesn't exist.
            string messagesPath = Path.Combine("text", "messages.txt");
            if (File.Exists(messagesPath)) {
                using (StreamReader r = File.OpenText(messagesPath)) {
                    while (!r.EndOfStream) {
                        messages.Add(r.ReadLine());
                    }
                }
            }
            else {
                File.Create(messagesPath).Close();
            }

            // Start the auto-save thread.
            levelSaver = new AutoSaver(this, props.backupInterval);

            // Create a thread that saves block changes to the SQL table every 60 seconds.
            blockThread = new Thread(new ThreadStart(delegate {
                while (true) {
                    Thread.Sleep(props.blockInterval * 1000);
                    levels.ForEach(delegate(Level l) {
                        try {
                            l.saveChanges();
                        }
                        catch (Exception e) {
                            logger.ErrorLog(e);
                        }
                    });
                }
            }));
            // ...and start the thread.
            blockThread.Start();

            // Creates a thread that checks players' positions every 0.003 seconds.
            locationChecker = new Thread(new ThreadStart(delegate {
                Player p, who;
                ushort x, y, z;
                int i;
                while (true) {
                    Thread.Sleep(3);
                    for (i = 0; i < players.Count; i++) {
                        try {
                            p = players[i];

                            // The "frozen" effect is achieved client-side by teleporting the
                            // player to their current position over and over.
                            // HandleInput() already ignores the player's input when frozen, so the
                            // player's position is not updated server-side while frozen.
                            if (p.frozen) {
                                unchecked {
                                    p.SendPos((byte)-1, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1]);
                                }
                                continue;
                            }
                            // If the player is following another player...
                            else if (p.following != String.Empty) {
                                // Check if the player being followed did not disconnect and is
                                // actually on the same level as the player that is following.
                                // If not, reset everything to normal.
                                who = players.Find(p.following);
                                if (who == null || who.level != p.level) {
                                    p.following = String.Empty;
                                    if (!p.canBuild) {
                                        p.canBuild = true;
                                    }
                                    if (who != null && who.possess == p.name) {
                                        who.possess = String.Empty;
                                    }
                                    continue;
                                }
                                if (p.canBuild) {
                                    unchecked { // Why -16 of the y coordinate? -Jjp137
                                        p.SendPos((byte)-1, who.pos[0], (ushort)(who.pos[1] - 16), who.pos[2], who.rot[0], who.rot[1]);
                                    }
                                }
                                else {
                                    unchecked {
                                        p.SendPos((byte)-1, who.pos[0], who.pos[1], who.pos[2], who.rot[0], who.rot[1]);
                                    }
                                }
                            }
                            // Reset the possession state of both players if either the player
                            // being possessed is not on the server anymore or if they are both on
                            // different levels.
                            else if (p.possess != String.Empty) {
                                who = players.Find(p.possess);
                                if (who == null || who.level != p.level) {
                                    p.possess = String.Empty;
                                }
                            }

                            // Convert from fixed point to truncated floating-point values.
                            x = (ushort)(p.pos[0] / 32);
                            y = (ushort)(p.pos[1] / 32);
                            z = (ushort)(p.pos[2] / 32);

                            // If the 'survival death' option is on, check if the player has died.
                            if (p.level.Death) {
                                p.RealDeath(x, y, z);
                            }
                            // Check if the player is standing on any special blocks.
                            p.CheckBlock(x, y, z);

                            p.oldBlock = (ushort)(x + y + z); // Isn't there a flaw in this formula?
                        }
                        catch (Exception e) {
                            logger.ErrorLog(e);
                        }
                    }
                }
            }));
            // ...and start the thread.
            locationChecker.Start();

            // And some random leftovers...
            blockQueue = new BlockQueue(this);
            blockQueue.Start();

            // Construct the IRC bot. This must be done even if IRC is not being used because
            // of places in the code that assume that the bot exists.
            // TODO: properly fix this
            IRC = new HmkBot(this, props.ircChannel, props.ircOpChannel, props.ircNick, props.ircServer);

            // Establish the IRC bot's connection to the IRC channel if IRC is enabled.
            if (props.irc) {
                IRC.Connect();
            }

            // Retrieve the external IP address.
            try {
                IP = NetworkUtil.GetExternalIP();
                logger.Log("Your IP address is: " + IP);
            }
            catch (WebException) {
                logger.Log("Could not get the server's IP address.");
            }

            ServerSetupFinished = true;
        }

        /// <summary>
        /// Loads various settings and components of the server.
        /// </summary>
        public void LoadAllSettings() {
            // Read server.properties and load from it.
            props.LoadProperties(logger, "properties/server.properties");

            // Initialize all groups/ranks.
            ranks.Logger = logger;
            ranks.InitAll(props.defaultRank);

            // Initialize all commands.
            commands = new CommandList(ranks);
            commands.Logger = logger;
            commands.AddAllCommands(this);

            // Assign the commands and their features to the ranks that can use them.
            commands.LoadPerms();
            commands.LoadOtherPerms();

            // Set the permissions of all the blocks.
            blockPerms = new BlockPermList(ranks);
            blockPerms.Logger = logger;
            blockPerms.SetBlocks();

            // Loads all awards.
            Awards.Load();

            // Loads all warps.
            Warp.logger = logger;
            Warp.LOAD();

            // Initializes the profanity filter.
            profanityFilter = new ProfanityFilter(this);

            // Sets up properties for the Player that represents the console.
            _console.Server = this;
            _console.rank = ranks.FindPerm(DefaultRankValue.Nobody);
        }

        private void UpdateOldMySQLDatabase() {
            // Old comment: Here, since SQLite is a NEW thing from 5.3.0.0, we do not have
            // to check for existing tables in SQLite.

            // This is more backwards compatibility since very old versions of other software may
            // not have certain columns in them. -Jjp137
            if (props.useMySQL) {
                // Check if the color column exists.
                DataTable colorExists = database.ObtainData("SHOW COLUMNS FROM Players WHERE `Field`='color'");
                if (colorExists.Rows.Count == 0) {
                    database.ExecuteStatement("ALTER TABLE Players ADD COLUMN color VARCHAR(6) AFTER totalKicked");
                }
                colorExists.Dispose();

                // Check if the title color column exists.
                DataTable tcolorExists = database.ObtainData("SHOW COLUMNS FROM Players WHERE `Field`='title_color'");
                if (tcolorExists.Rows.Count == 0) {
                    database.ExecuteStatement("ALTER TABLE Players ADD COLUMN title_color VARCHAR(6) AFTER color");
                }
                tcolorExists.Dispose();

                DataTable timespent = database.ObtainData("SHOW COLUMNS FROM Players WHERE `Field`='TimeSpent'");
                // Check if the time spent column exists.
                if (timespent.Rows.Count == 0) {
                    database.ExecuteStatement("ALTER TABLE Players ADD COLUMN TimeSpent VARCHAR(20) AFTER totalKicked");
                }
                timespent.Dispose();

                // Er...a total cuboided column? That's not even used anywhere... -Jjp137
                DataTable totalCuboided = database.ObtainData("SHOW COLUMNS FROM Players WHERE `Field`='totalCuboided'");
                if (totalCuboided.Rows.Count == 0) {
                    database.ExecuteStatement("ALTER TABLE Players ADD COLUMN totalCuboided BIGINT AFTER totalBlocks");
                }
                totalCuboided.Dispose();
            }
        }

        /// <summary>
        /// Sets up a listening socket for the server.
        /// </summary>
        public void Setup() {
            try {
                // Form an endpoint with any IP address connecting on the given port.
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, props.port);
                // Create a new TCP, byte-stream socket using that endpoint's address family.
                listen = new Socket(endpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Associate the socket with the endpoint.
                listen.Bind(endpoint);
                // Put the Socket in a listening state.
                listen.Listen((int)SocketOptionName.MaxConnections);
                // Look for incoming connections in a separate thread, and call Accept() when a
                // connection arrives.
                listen.BeginAccept(Accept, null);
            }
            // But things always go wrong...
            catch (SocketException e) {
                logger.ErrorLog(e);
                logger.Log("Error Creating listener, socket shutting down");
            }
            catch (Exception e) {
                logger.ErrorLog(e);
                logger.Log("Error Creating listener, socket shutting down");
            }
        }

        /// <summary>
        /// Accepts and handles an incoming connection. This is the callback method used for listen.BeginAccept().
        /// </summary>
        /// <param name="result"> Information about the incoming connection. </param>
        void Accept(IAsyncResult result) {
            if (shuttingDown) {
                return;
            }

            Player p = null;
            bool begin = false;
            try {
                // EndAccept() returns a socket that can be used to create a new Player object
                p = new Player(this, listen.EndAccept(result));

                // Listen for more players...
                listen.BeginAccept(Accept, null);
                begin = true;
            }
            catch (SocketException) {
                if (p != null) {
                    p.Disconnect();
                }
                if (!begin) {
                    listen.BeginAccept(Accept, null);
                }
            }
            catch (Exception e) {
                logger.ErrorLog(e);
                if (p != null) {
                    p.Disconnect();
                }
                if (!begin) {
                    listen.BeginAccept(Accept, null);
                }
            }
        }

        /// <summary>
        /// Shuts down the server.
        /// </summary>
        public void Exit() {
            // Save all player data and kick them from the server.
            List<string> playerNames = new List<string>();
            foreach (Player p in players) {
                p.save();
                playerNames.Add(p.name);
            }
            foreach (string p in playerNames) {
                players.Find(p).Kick("Server shutdown occurred.");
            }

            // ...uh kick them again?
            Player.connections.ForEach(
                delegate(Player p) {
                    p.Kick("Server shutdown occurred.");
                }
            );

            // Close the listening socket.
            if (listen != null) {
                listen.Close();
            }

            // Disconnect the IRC bot.
            try {
                IRC.Disconnect("Server is shutting down.");
            }
            catch (Exception e) {
                logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Adds a level to the server's level list.
        /// </summary>
        /// <param name="level"> The level to add. <seealso cref="Level"/></param>
        public void addLevel(Level level) {
            levels.Add(level);
        }

        /// <summary>
        /// Displays a global message from messages.txt to all players on the server.
        /// </summary>
        public void RandomMessage() {
            if (players.Count != 0 && messages.Count > 0) {
                GlobalMessage(messages[new Random().Next(0, messages.Count)]);
            }
        }

        // TODO: refactor
        public void SaveBannedIP(bool console) {
            StreamWriter file = File.CreateText(Path.Combine("ranks", "banned-ip.txt"));
            foreach (string ip in bannedIP) {
                file.WriteLine(ip);
            }
            file.Close();
            if (console) {
                logger.Log("SAVED: ranks/banned-ip.txt");
            }
        }

        public void ConsoleCommand(Command cmd, string args) {
            cmd.Use(_console, args);
        }

        /// <summary>
        /// Sends a block change packet to everyone on the same level where the block change
        /// occurred. This method definition accepts an integer representing an index in a level's block array.
        /// </summary>
        /// <param name="level"> The level where the block change took place. <seealso cref="Level"/></param>
        /// <param name="b"> The index of the block array. </param>
        /// <param name="type"> The id of the block type that the block was changed to. <seealso cref="BlockId"/></param>
        public void GlobalBlockchange(Level level, int b, BlockId type) {
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);
            GlobalBlockchange(level, x, y, z, type);
        }

        /// <summary>
        /// Sends a block change packet to everyone on the same level where the block change
        /// occurred. This method definition accepts block coordinates.
        /// </summary>
        /// <param name="level"> The level where the block change took place. <seealso cref="Level"/></param>
        /// <param name="x"> The x-coordinate of the block. </param>
        /// <param name="y"> The y-coordinate of the block. </param>
        /// <param name="z"> The z-coordinate of the block. </param>
        /// <param name="type"> The id of the block type that the block was changed to. </param>
        public void GlobalBlockchange(Level level, ushort x, ushort y, ushort z, BlockId type) {
            players.ForEach(delegate(Player p) {
                if (p.level == level) {
                    p.SendBlockchange(x, y, z, type);
                }
            });
        }

        /// <summary>
        /// Sends a normal chat message to everyone.
        /// </summary>
        /// <param name="from"> The player that sent the message. <seealso cref="Player"/></param>
        /// <param name="message"> The message itself. </param>
        public void GlobalChat(Player from, string message) {
            GlobalChat(from, message, true);
        }

        /// <summary>
        /// Sends a message to everyone on the server. This is the default choice.
        /// </summary>
        /// <param name="from"> The player that sent the message. <seealso cref="Player"/></param>
        /// <param name="message"> The message itself. </param>
        /// <param name="showname"> Whether the sender's name is shown before his message. </param>
        public void GlobalChat(Player from, string message, bool showname) {
            // Avoid a possible NullReferenceException.
            if (from == null) {
                return;
            }

            // Show the player's title and name before his message unless it's a message that was
            // sent by some other means.
            if (showname) {
                message = from.color + from.voicestring + from.color + from.prefix + from.DisplayName + ": &f" + message;
            }
            players.ForEach(delegate(Player p) {
                // Send the message to players in levels that do not have level-only chat.
                if (p.level.worldChat) {
                    // If a player is not ignoring everyone and the sender is not in that player's
                    // ignore list, send the message to that person.
                    if (p.ignoreglobal == false) {
                        if (from != null) {
                            if (!p.listignored.Contains(from.name)) {
                                p.SendMessage(message);
                                return;
                            }
                            return;
                        }
                        p.SendMessage(message);
                        return;
                    }
                    // If no one is allowed to ignore ops and the player sending the message is able to
                    // use opchat and the recipent's rank is lower than the sender's, send the message.
                    if (props.globalignoreops == false) {
                        if (from.rank.Permission >= props.opchatperm) {
                            if (p.rank.Permission < from.rank.Permission) {
                                p.SendMessage(message);
                            }
                        }
                    }
                    // Send the message to the sender has well. This lets the sender see
                    // their own message in the chat history.
                    if (from != null) {
                        if (from == p) {
                            from.SendMessage(message);
                            return;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Sends a message to everyone in the level.
        /// </summary>
        /// <param name="from"> The player that sent the message. <seealso cref="Player"/></param>
        /// <param name="message"> The message itself. </param>
        /// <param name="showname"> Whether the sender's name is shown before his message. </param>
        public void GlobalChatLevel(Player from, string message, bool showname) {
            // Show the player's title and name before his message unless it's a message that was
            // sent by some other means.
            if (showname) {
                message = "<Level>" + from.color + from.voicestring + from.color + from.prefix + from.name + ": &f" + message;
            }
            players.ForEach(delegate(Player p) {
                // Send the message to players in the same level.
                if (p.level == from.level) {
                    // If a player is not ignoring everyone and the sender is not in that player's
                    // ignore list, send the message to that person.
                    if (p.ignoreglobal == false) {
                        if (from != null) {
                            if (!p.listignored.Contains(from.name)) {
                                p.SendMessage(props.DefaultColor + message);
                                return;
                            }
                            return;
                        }
                        p.SendMessage(props.DefaultColor + message);
                        return;
                    }
                    // If no one is allowed to ignore ops and the player sending the message is able to
                    // use opchat and the recipent's rank is lower than the sender's, send the message.
                    if (props.globalignoreops == false) {
                        if (from.rank.Permission >= props.opchatperm) {
                            if (p.rank.Permission < from.rank.Permission) {
                                p.SendMessage(props.DefaultColor + message);
                            }
                        }
                    }
                    // Send the message to the sender as well. This lets the sender see
                    // their own message in the chat history.
                    if (from != null) {
                        if (from == p) {
                            from.SendMessage(props.DefaultColor + message);
                            return;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Send a message to everyone on the server, overriding level-specific chat. Used when a player is on a
        /// level with level-specific chat but wants to send a message to everyone.
        /// </summary>
        /// <param name="from"> The person that sent the message. <seealso cref="Player"/></param>
        /// <param name="message"> The message itself. </param>
        /// <param name="showname"> Whether the sender's name is shown before his message. </param>
        public void GlobalChatWorld(Player from, string message, bool showname) {
            // Show the player's title and name before his message unless it's a message triggered
            // by some other means.
            if (showname) {
                message = "<World>" + from.color + from.voicestring + from.color + from.prefix + from.name + ": &f" + message;
            }
            players.ForEach(delegate(Player p) {
                // Send the message to other players who are in levels that are not restricted
                // to level-only chat.
                if (p.level.worldChat) {
                    // If a player is not ignoring everyone and the sender is not in that player's
                    // ignore list, send the message to that person.
                    if (p.ignoreglobal == false) {
                        if (from != null) {
                            if (!p.listignored.Contains(from.name)) {
                                p.SendMessage(props.DefaultColor + message);
                                return;
                            }
                            return;
                        }
                        p.SendMessage(props.DefaultColor + message);
                        return;
                    }

                    // If no one can ignore ops and the player sending the message is an op and
                    // a player receiving a message does not have a higher rank, send it.
                    if (props.globalignoreops == false) {
                        if (from.rank.Permission >= props.opchatperm) {
                            if (p.rank.Permission < from.rank.Permission) {
                                p.SendMessage(props.DefaultColor + message);
                            }
                        }
                    }

                    // Send the message to the sender himself as well. This lets the sender see
                    // their own message in the chat history.
                    if (from != null) {
                        if (from == p) {
                            from.SendMessage(props.DefaultColor + message);
                            return;
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Sends a chat message to everyone on the server.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void GlobalMessage(string message) {
            GlobalMessage(MessageType.Chat, message);
        }

        /// <summary>
        /// Send a message of the specified type to everyone on the server.
        /// </summary>
        /// <param name="type"> The type of message to send. <seealso cref="MessageType"/>
        /// <param name="message"> The message to send.</param>
        public void GlobalMessage(MessageType type, string message) {
            players.ForEach(delegate(Player p) {
                if (p.level.worldChat) {
                    p.SendMessage(type, message);
                }
            });
        }

        /// <summary>
        /// Sends a chat message to all players in a particular level.
        /// </summary>
        /// <param name="l"> The level to send the message to. <seealso cref="Level"/></param>
        /// <param name="message"> The message. </param>
        public void GlobalMessageLevel(Level l, string message) {
            players.ForEach(delegate(Player p) {
                if (p.level == l) {
                    p.SendMessage(MessageType.Chat, message);
                }
            });
        }

        /// <summary>
        /// Sends a message of the specified type to all players in a particular level.
        /// </summary>
        /// <param name="l"> The level to send the message to. <seealso cref="Level"/></param>
        /// <param name="type"> The type of message to send. <seealso cref="MessageType"/></param> 
        /// <param name="message"> The message. </param>
        public void GlobalMessageLevel(Level l, MessageType type, string message) {
            players.ForEach(delegate(Player p) {
                if (p.level == l) {
                    p.SendMessage(type, message);
                }
            });
        }

        /// <summary>
        /// Send a message to every op in the server.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void GlobalMessageOps(string message) {
            try {
                players.ForEach(delegate(Player p) {
                    if (p.rank.Permission >= props.opchatperm) {
                        p.SendMessage(message);
                    }
                });

            }
            catch (Exception e) {
                logger.ErrorLog(e);
                logger.Log("Error occured with Op Chat");
            }
        }

        /// <summary>
        /// Send a message to every admin in the server.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void GlobalMessageAdmins(string message) {
            try {
                players.ForEach(delegate(Player p) {
                    if (p.rank.Permission >= props.adminchatperm) {
                        p.SendMessage(message);
                    }
                });

            }
            catch (Exception e) {
                logger.ErrorLog(e);
                logger.Log("Error occured with Admin Chat");
            }
        }

        /// <summary>
        /// Spawns a player and makes that player visible to everyone else.
        /// </summary>
        /// <param name="from"> The player that is spawning. <seealso cref="Player"/></param>
        /// <param name="x"> The x-coordinate of the position that the player is spawning at. </param>
        /// <param name="y"> The y-coordinate of the position that the player is spawning at. </param>
        /// <param name="z"> The z-coordinate of the position that the player is spawning at. </param>
        /// <param name="rotx"> The angle on the x-axis of the player's head when spawning. </param>
        /// <param name="roty"> The angle on the y-axis of the player's head when spawning. </param>
        /// <param name="self"> Whether the position of the player being spawned needs to change. </param>
        /// <param name="possession"> The player that is possessing the person being spawned. </param>
        public void GlobalSpawn(Player from, ushort x, ushort y, ushort z, byte rotx, byte roty, bool self,
                                       string possession = "") {
            players.ForEach(delegate(Player p) {
                // If another player is currently loading a map, do not do anything.
                if (p.Loading && p != from) {
                    return;
                }
                // Do not do anything for players in a different level or if the player being
                // spawned is hidden and does not need to change their position.
                if (p.level != from.level || (from.hidden && !self)) {
                    return;
                }
                // Send the packet if the player is not the same as the player being spawned.
                if (p != from) {
                    p.SendSpawn(from.serverId, from.color + from.name + possession, x, y, z, rotx, roty);
                }
                // If the server is changing the player's position, tell their client to teleport the player.
                else if (self) { 
                    p.pos = new ushort[3] { x, y, z };
                    p.rot = new byte[2] { rotx, roty };
                    p.oldpos = p.pos;
                    p.oldrot = p.rot;
                    unchecked { // An id of -1 points to the player being spawned.
                        p.SendSpawn((byte)-1, from.color + from.name + possession, x, y, z, rotx, roty);
                    }
                }
            });
        }

        /// <summary>
        /// Sends a packet to all other players telling them that a player has despawned.
        /// </summary>
        /// <param name="from"> The player to despawn. <seealso cref="Player"/></param>
        /// <param name="self"> Whether to send a packet to the player that is despawning. </param>
        public void GlobalDie(Player from, bool self) {
            players.ForEach(delegate(Player p) {
                // Do not send a packet to those on different levels.
                if (p.level != from.level || (from.hidden && !self)) {
                    return;
                }
                // Send the packet with the despawning player's id to everyone else.
                if (p != from) {
                    p.SendDie(from.serverId);
                }
                // Send a packet to the despawning player, if needed. 255 is -1 signed, and
                // -1 indicates the despawning player.
                else if (self) { 
                    p.SendDie(255);
                }
            });
        }

        /// <summary>
        /// Updates the positions of all players to all clients.
        /// </summary>
        public void GlobalUpdate() {
            players.ForEach(delegate(Player p) {
                if (!p.hidden) { // Don't give away the locations of hidden players.
                    p.UpdatePosition();
                }
            });
        }
    }
}

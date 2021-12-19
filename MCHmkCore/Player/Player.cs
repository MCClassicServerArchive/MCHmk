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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading;

using Newtonsoft.Json.Linq;

using MCHmk.SQL;

namespace MCHmk {
    /// <summary>
    /// The Player class represents a player on the server.
    /// </summary>
    public class Player : IDisposable {
        /// <summary>
        /// A list of player names and the IP address that they are associated with. Used to
        /// check if two or more usernames share the same IP address. This list is appended
        /// to when a player leaves the server.
        /// </summary>
        public static Dictionary<string, string> left = new Dictionary<string, string>();
        /// <summary>
        /// The list of players that have connected to the server and are waiting to be logged in.
        /// </summary>
        public static List<Player> connections = new List<Player>();
        /// <summary>
        /// The Timer used to unmute players after a certain amount of time.
        /// </summary>
        System.Timers.Timer muteTimer = new System.Timers.Timer(1000);
        /// <summary>
        /// The list of players that do not want to see emoticons.
        /// </summary>
        public static List<UuidEntry> emoteList = new List<UuidEntry>();
        /// <summary>
        /// The list of players that the player is ignoring (not receiving chat from).
        /// </summary>
        public List<string> listignored = new List<string>();
        /// <summary>
        /// The list of players that are ignoring all chat.
        /// </summary>
        public static List<string> globalignores = new List<string>();

        /// <summary>
        /// Used to convert bits of data to the corresponding characters.
        /// </summary>
        static System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        /// <summary>
        /// Used to calculate md5 hashes.
        /// </summary>
        static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        /// <summary>
        /// The name of the player that sent the most recent chat message.
        /// </summary>
        public static string lastMSG = String.Empty;

        /// <summary>
        /// Occurs when a player places or removes a block.
        /// </summary>
        public event EventHandler<BlockChangedEventArgs> BlockChanged = null;
        /// <summary>
        /// Occurs when a player connects to the server.
        /// </summary>
        public static event EventHandler PlayerConnected = null;
        /// <summary>
        /// Occurs when a player disconnects from the server.
        /// </summary>
        public static event EventHandler<PlayerDisconnectedEventArgs> PlayerDisconnected = null;
        /// <summary>
        /// Occurs when a player sends a message to the chat.
        /// </summary>
        public static event EventHandler<PlayerChattedEventArgs> PlayerChatted = null;


        /// <summary>
        /// The method to call when a block for a command is selected.
        /// </summary>
        private Action<Player, CommandTempData> _blockSelected;
        /// <summary>
        /// The data that will be passed to the method when a block for a command is selected.
        /// </summary>
        private Dictionary<string, object> _commandData = null;

        /// <summary>
        /// Registers a method to be called when a block for a command is selected.
        /// Only one method, and therefore one command, can listen to a block selection at a time.
        /// </summary>
        /// <param name="callback"> The method that will be called when a block is selected. The method must
        /// accept a Player and a CommandTempData as its parameters. </param>
        /// <param name="data"> Additional data that will be passed to the given method in addition to the
        /// block coordinates and block type. Can be null if no other data is needed. </param>
        public void StartSelection(Action<Player, CommandTempData> callback, Dictionary<string, object> data) {
            _blockSelected = callback;
            _commandData = data;
        }

        /// <summary>
        /// Cancels the command that is waiting for a block to be selected.
        /// </summary>
        public void ClearSelection() {
            _blockSelected = null;
            _commandData = null;
        }

        /// <summary>
        /// The name that the client connected to the server with. If the client used
        /// a minecraft.net login, then this is the same as the client's name. For
        /// Mojang accounts, this is the e-mail address that the player logged in with.
        /// </summary>
        private string truename;
        /// <summary>
        /// A debug variable. If true, invalid client packets are tolerated.
        /// </summary>
        internal bool dontmindme = false;
        /// <summary>
        /// The network socket that the server uses to communicate with this player.
        /// </summary>
        public Socket socket;
        /// <summary>
        /// The Timer that updates the amount of time that the player has spent on the server.
        /// It does so every second.
        /// </summary>
        System.Timers.Timer timespent = new System.Timers.Timer(1000);
        /// <summary>
        /// The Timer that checks if the player finished loading the main map. If the player has
        /// loaded the main map, this Timer's function is to display login and welcome messages.
        /// </summary>
        System.Timers.Timer loginTimer = new System.Timers.Timer(1000);
        /// <summary>
        /// The Timer that pings a player every two seconds to see if the player's connection is
        /// alive.
        /// </summary>
        public System.Timers.Timer pingTimer = new System.Timers.Timer(2000);
        /// <summary>
        /// The Timer that displays additional login messages after a certain amount of time.
        /// </summary>
        System.Timers.Timer extraTimer = new System.Timers.Timer(22000);
        /// <summary>
        /// The Timer that checks if a player is idle. Has a two-second interval.
        /// </summary>
        public System.Timers.Timer afkTimer = new System.Timers.Timer(2000);
        /// <summary>
        /// The counter representing how long a player has been idle.
        /// </summary>
        public int afkCount = 0;
        /// <summary>
        /// The date and time of when the player became AFK.
        /// </summary>
        public DateTime afkStart;
        /// <summary>
        /// Whether a megaboid started by this player is in progress.
        /// </summary>
        public bool megaBoid = false;
        /// <summary>
        /// Whether the player has a timer running.
        /// </summary>
        public bool cmdTimer = false;

        /// <summary>
        /// The array that contains byte data that the player has sent. Used by HandleMessage().
        /// </summary>
        byte[] buffer = new byte[0];
        /// <summary>
        /// The array that contains byte data that the player has sent. Incoming data initially
        /// goes here and is later copied to the array named 'buffer'.
        /// </summary>
        byte[] tempbuffer = new byte[0xFF];
        /// <summary>
        /// Whether the player has disconnected.
        /// </summary>
        public bool disconnected = false;
        /// <summary>
        /// The string representing the total amount of time that the player has spent on the 
        /// server.
        /// </summary>
        public string time;
        /// <summary>
        /// The player's name. Contains a plus sign at the end if the player is using a
        /// ClassiCube account.
        /// </summary>
        public string name;
        /// <summary>
        /// The name that is displayed next to a player's lines in chat.
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// Whether a player's name has been verified.
        /// </summary>
        public bool identified = false;
        /// <summary>
        /// The uuid for this player.
        /// </summary>
        public Uuid uuid = Uuid.Empty;
        /// <summary>
        /// The number of times that the player was warned.
        /// </summary>
        public int warn = 0;
        /// <summary>
        /// The player ID used by the server. Sent with various packets.
        /// </summary>
        public byte serverId;
        /// <summary>
        /// The row number of the player's data in the SQL table.
        /// </summary>
        public int userID = -1;
        /// <summary>
        /// The IP address of the player.
        /// </summary>
        public string ip;
        /// <summary>
        /// The color of the player's name.
        /// </summary>
        public string color;
        /// <summary>
        /// The rank that the player is part of.
        /// </summary>
        public Rank rank;
        /// <summary>
        /// Whether the player is hidden.
        /// </summary>
        public bool hidden = false;
        /// <summary>
        /// Whether /paint mode is on.
        /// </summary>
        public bool painting = false;
        /// <summary>
        /// Whether the player has been muted.
        /// </summary>
        public bool muted = false;
        /// <summary>
        /// Whether the player is in jail.
        /// </summary>
        public bool jailed = false;
        /// <summary>
        /// Whether the player has agreed to the rules.
        /// </summary>
        public bool agreed = false;
        /// <summary>
        /// Whether the player is immune to death.
        /// </summary>
        public bool invincible = false;
        /// <summary>
        /// The string representing all text that comes before the username of the player.
        /// </summary>
        public string prefix = String.Empty;
        /// <summary>
        /// The player's title.
        /// </summary>
        public string title = String.Empty;
        /// <summary>
        /// The color of the player's title.
        /// </summary>
        public string titlecolor;
        /// <summary>
        /// The number of times the player entered their admin password.
        /// </summary>
        public int passtries = 0;
        /// <summary>
        /// The number of times that the player used /pony.
        /// </summary>
        public int ponycount = 0;
        /// <summary>
        /// The number of times that the player used /rainbowdashlikescoolthings.
        /// </summary>
        public int rdcount = 0;
        /// <summary>
        /// Whether the player has read the rules.
        /// </summary>
        public bool hasreadrules = false;
        /// <summary>
        /// Whether the player is able to use /review. It is true if the /review cooldown has elapsed.
        /// </summary>
        public bool canusereview = true;
        /// <summary>
        /// The model that is visible to other players using CPE clients.
        /// </summary>
        public string model = "humanoid";

        /// <summary>
        /// Whether the player is currently using /delete.
        /// </summary>
        public bool deleteMode = false;
        /// <summary>
        /// Whether the anti-digging system ignores this player.
        /// </summary>
        public bool ignoreGrief = false;
        /// <summary>
        /// Whether the players wants to see emoticons.
        /// </summary>
        public bool parseSmiley = true;
        /// <summary>
        /// Whether the emoticon preference has been saved to emotelist.txt yet.
        /// </summary>
        public bool smileySaved = true;
        /// <summary>
        /// Whether the player is using opchat.
        /// </summary>
        public bool opchat = false;
        /// <summary>
        /// Whether the player is using adminchat.
        /// </summary>
        public bool adminchat = false;
        /// <summary>
        /// Whether the player is on the server's whitelist.
        /// </summary>
        public bool onWhitelist = false;
        /// <summary>
        /// Whether the player is in whisper mode.
        /// </summary>
        public bool whisper = false;
        /// <summary>
        /// The name of the player to whisper to when /whisper is on.
        /// </summary>
        public string whisperTo = String.Empty;
        /// <summary>
        /// Whether all chat in the server is being ignored by the player.
        /// </summary>
        public bool ignoreglobal = false;
        /// <summary>
        /// The message that the player is appending to.
        /// </summary>
        public string storedMessage = String.Empty;

        /// <summary>
        /// Whether a player is on a train.
        /// </summary>
        public bool trainGrab = false;
        /// <summary>
        /// Whether /ride mode is on.
        /// </summary>
        public bool onTrain = false;
        /// <summary>
        /// Whether a player can use TNT.
        /// </summary>
        public bool allowTnt = true;

        /// <summary>
        /// Whether a player is frozen and can't move.
        /// </summary>
        public bool frozen = false;
        /// <summary>
        /// The name of the player that the player is following.
        /// </summary>
        public string following = String.Empty;
        /// <summary>
        /// The name of the player that the player is possessing.
        /// </summary>
        public string possess = String.Empty;

        /// <summary>
        /// Whether a player is allowed to build. This variable should be only used for possession.
        /// </summary>
        public bool canBuild = true;

        /// <summary>
        /// The amount of money that the player has.
        /// </summary>
        public int money = 0;
        /// <summary>
        /// The number of blocks that a player has modified across all sessions.
        /// </summary>
        public long overallBlocks = 0;
        /// <summary>
        /// The number of blocks that a player has modified during the current session.
        /// </summary>
        public int loginBlocks = 0;
        /// <summary>
        /// The date and time of the player's login for this session.
        /// </summary>
        public DateTime timeLogged;
        /// <summary>
        /// The date and time of the player's first login.
        /// </summary>
        public DateTime firstLogin;
        /// <summary>
        /// The number of times that a player logged into the server.
        /// </summary>
        public int totalLogins = 0;
        /// <summary>
        /// The number of times that a player was kicked.
        /// </summary>
        public int totalKicked = 0;
        /// <summary>
        /// The number of times that a player died.
        /// </summary>
        public int overallDeath = 0;

        /// <summary>
        /// Whether the player is in static mode.
        /// </summary>
        public bool staticCommands = false;
        /// <summary>
        /// The date and time of the last zone-related message sent to the player.
        /// </summary>
        public DateTime ZoneSpam;
        /// <summary>
        /// Whether a player is currently checking for zones.
        /// </summary>
        public bool ZoneCheck = false;
        /// <summary>
        /// Whether a player is using /zone del.
        /// </summary>
        public bool zoneDel = false;

        /// <summary>
        /// The thread that processes command input concurrently.
        /// </summary>
        public Thread commThread;

        /// <summary>
        /// Whether the player has /gun or /missile on.
        /// </summary>
        public bool aiming;
        /// <summary>
        /// Whether the player has fly mode on.
        /// </summary>
        public bool isFlying = false;
        /// <summary>
        /// Whether the player is a joker.
        /// </summary>
        public bool joker = false;
        /// <summary>
        /// Whether the player has to verify that he or she is an admin by typing their
        /// password.
        /// </summary>
        public bool adminpen = false;
        /// <summary>
        /// Whether the player has voice privileges.
        /// </summary>
        public bool voice = false;
        /// <summary>
        /// This is a + sign if the player has voice privileges.
        /// </summary>
        public string voicestring = String.Empty;

        /// <summary>
        /// Whether the player has voted during the current vote session.
        /// </summary>
        public bool voted = false;

        /// <summary>
        /// The list of blocks that can be pasted.
        /// </summary>
        public List<CopyPos> CopyBuffer = new List<CopyPos>();

        /// <summary>
        /// The CopyPos struct stores information about each block in the player's copy buffer.
        /// </summary>
        public struct CopyPos {
            /// <summary>
            /// The x coordinate of a copied block.
            /// </summary>
            public ushort x;
            /// <summary>
            /// The y coordinate of a copied block.
            /// </summary>
            public ushort y;
            /// <summary>
            /// The z coordinate of a copied block.
            /// </summary>
            public ushort z;
            /// <summary>
            /// The type of block that was copied.
            /// </summary>
            public BlockId type;
        }
        /// <summary>
        /// Whether the player is including the air block in the current copy operation.
        /// </summary>
        public bool copyAir = false;
        /// <summary>
        /// The position that the next paste operation will begin from.
        /// </summary>
        public int[] copyoffset = new int[3] { 0, 0, 0 };
        /// <summary>
        /// The position that the current copy operation will begin from.
        /// </summary>
        public ushort[] copystart = new ushort[3] { 0, 0, 0 };

        /// <summary>
        /// The UndoPos struct stores information about block changes that have occurred.
        /// </summary>
        public struct UndoPos {
            /// <summary>
            /// The x coordinate of the block that was changed.
            /// </summary>
            public ushort x;
            /// <summary>
            /// The y coordinate of the block that was changed.
            /// </summary>
            public ushort y;
            /// <summary>
            /// The z coordinate of the block that was changed.
            /// </summary>
            public ushort z;
            /// <summary>
            /// The former block type.
            /// </summary>
            public BlockId type;
            /// <summary>
            /// The block type that was placed (or being held?).
            /// </summary>
            public BlockId newtype;
            /// <summary>
            /// The map that the change took place on.
            /// </summary>
            public string mapName;
            /// <summary>
            /// The date and time when the block change occurred.
            /// </summary>
            public DateTime timePlaced;
        }

        /// <summary>
        /// Stores a player's block change actions so that they can be undone later.
        /// </summary>
        public List<UndoPos> UndoBuffer = new List<UndoPos>();
        /// <summary>
        /// Stores a player's most recent undo so that /redo can be used later.
        /// </summary>
        public List<UndoPos> RedoBuffer = new List<UndoPos>();

        /// <summary>
        /// Whether '/portal show' is enabled.
        /// </summary>
        public bool showPortals = false;
        /// <summary>
        /// Whether '/mb show' is enabled.
        /// </summary>
        public bool showMBs = false;

        /// <summary>
        /// The integer representing the last block that the player stepped on.
        /// </summary>
        public ushort oldBlock = 0;
        /// <summary>
        /// Used to keep track of whether a player should die from falling or drowning.
        /// </summary>
        public ushort deathCount = 0;
        /// <summary>
        /// The block that caused the player's death.
        /// </summary>
        public BlockId deathblock;

        /// <summary>
        /// The time and date of the player's last death.
        /// </summary>
        public DateTime lastDeath = DateTime.Now;

        /// <summary>
        /// This variable is 0 if the player is placing blocks normally, 6 if the player is using
        /// /mode, and 13, 14, and 15 for using small, big, and nuke TNT, respectively.
        /// </summary>
        public byte blockAction;
        /// <summary>
        /// The type of block that the player will place when he places a block. Used for /mode.
        /// </summary>
        public BlockId modeType;
        /// <summary>
        /// Used for /bind. Holds the actual block types that the player will place. When a player
        /// places a block, that block's id is used as an index for this array, and the value at
        /// that position is the block id that is actually placed.
        /// </summary>
        public BlockId[] bindings = new BlockId[128];
        /// <summary>
        /// The commands that the user has binded using /cmdbind.
        /// </summary>
        public string[] cmdBind = new string[10];
        /// <summary>
        /// The parameters for each binded command.
        /// </summary>
        public string[] messageBind = new string[10];
        /// <summary>
        /// The last command that the player used.
        /// </summary>
        public string lastCMD = String.Empty;
        /// <summary>
        /// The id of the C4 circuit that the player is currently placing.
        /// </summary>
        public sbyte c4circuitNumber = -1;

        /// <summary>
        /// The level that the player is on.
        /// </summary>
        public Level level;
        /// <summary>
        /// Whether the player is currently loading a map.
        /// </summary>
        public bool Loading = true;
        /// <summary>
        /// The position of the last mouse click by that player.
        /// </summary>
        public ushort[] lastClick = new ushort[] { 0, 0, 0 };

        /// <summary>
        /// The current position of the player in fixed-point.
        /// The entries are the x, y, and z coordinates, respectively.
        /// </summary>
        public ushort[] pos = new ushort[] { 0, 0, 0 };
        /// <summary>
        /// The former position of the player in fixed-point.
        /// The entries are the x, y, and z coordinates, respectively.
        /// </summary>
        public ushort[] oldpos = new ushort[] { 0, 0, 0 };
        /// <summary>
        /// The current rotation of the player's head in fixed-point.
        /// The entries are the angles on the x and y axes, respectively.
        /// </summary>
        public byte[] rot = new byte[] { 0, 0 };
        /// <summary>
        /// The former rotation of the player's head in fixed-point.
        /// The entries are the angles on the x and y axes, respectively.
        /// </summary>
        public byte[] oldrot = new byte[] { 0, 0 };

        /// <summary>
        /// The number of blocks that, if placed in a very short period of time, is said to be
        /// considered block spam.
        /// </summary>
        public static int spamBlockCount = 200;
        /// <summary>
        /// If a player places too many blocks in this set period of time, 
        /// he is kicked for block spam. The number is in seconds.
        /// </summary>
        public static int spamBlockTimer = 5;
        /// <summary>
        /// The Queue that contains the date and time of when the player's most recent block
        /// changes occurred.
        /// </summary>
        Queue<DateTime> spamBlockLog = new Queue<DateTime>(spamBlockCount);

        /// <summary>
        /// The number of consecutive messages that the player has sent.
        /// </summary>
        public int consecutivemessages;
        /// <summary>
        /// The Timer that resets a player's chat spam counter.
        /// </summary>
        private System.Timers.Timer resetSpamCount;

        /// <summary>
        /// The vote that the player sent during an ongoing votekick.
        /// </summary>
        public VoteKickChoice voteKickChoice = VoteKickChoice.HasntVoted;

        /// <summary>
        /// The list of waypoints that the player has saved.
        /// </summary>
        public List<Waypoint.WP> Waypoints = new List<Waypoint.WP>();

        /// <summary>
        /// The random number generator for this particular player.
        /// </summary>
        public Random random = new Random();
        /// <summary>
        /// Whether the player has logged into the server.
        /// </summary>
        public bool loggedIn;
        /// <summary>
        /// Whether the name has been verified.
        /// </summary>
        public bool verifiedName;

        /// <summary>
        /// The name of the client that the player is using.
        /// </summary>
        public string appName;
        /// <summary>
        /// The number of CPE extensions that the player supports.
        /// </summary>
        public int extensionCount;
        /// <summary>
        /// The level of support for the CustomBlocks extension.
        /// </summary>
        public int customBlockSupportLevel;
        /// <summary>
        /// Whether the player is using a CPE client.
        /// </summary>
        public bool extension;
        /// <summary>
        /// The list of CPE extensions that the client supports.
        /// </summary>
        public List<CPE> ExtEntry = new List<CPE>();

        /// <summary>
        /// The CPE struct represents a CPE extension that the client supports.
        /// </summary>
        public struct CPE {
            /// <summary>
            /// The name of the extension.
            /// </summary>
            public string name;
            /// <summary>
            /// The version of the extension that the client supports.
            /// </summary>
            public int version;
        }

        /// <summary>
        /// Gets the player's name with their color applied to it.
        /// </summary>
        public string ColoredName {
            get {
                return color + name + _server.props.DefaultColor;
            }
        }

        /// <summary>
        /// A reference to the server that the player is playing on.
        /// </summary>
        private Server _server;
        /// <summary>
        /// A reference to the server's logger.
        /// </summary>
        private Logger _logger;
        /// <summary>
        /// A reference to the server's database.
        /// </summary>
        private Database _db;

        /// <summary>
        /// Gets or internally sets the server the player is playing on.
        /// </summary>
        public Server Server {
            get {
                return _server;
            }
            internal set {
                _server = value;
            }
        }

        /// <summary>
        /// Gets the logger that logs events about the player.
        /// </summary>
        public Logger Logger {
            get {
                return _logger;
            }
        }

        /// <summary>
        /// Gets the database that the player uses.
        /// </summary>
        public Database Db {
            get {
                return _db;
            }
        }

        // TODO: is there a better way?

        /// <summary>
        /// Gets whether this Player object represents the server console.
        /// </summary>
        public bool IsConsole {
            get {
                return this == _server.ServerConsole;
            }
        }

        /// <summary>
        /// Constructs a new Player object for internal use.
        /// </summary>
        internal Player() {}

        /// <summary>
        /// Constructs a new Player object.
        /// </summary>
        /// <param name="svr"> The server that the player is connecting to. <seealso cref="Server"/></param>
        /// <param name="conn"> The socket associated with the player's connection. </param>
        public Player(Server svr, Socket conn) {
            try {
                // Obtain references to the server and logger.
                _server = svr;
                _logger = svr.logger;
                _db = svr.database;

                // Initialize some fields.
                level = svr.mainLevel;
                resetSpamCount = new System.Timers.Timer(_server.props.spamcountreset * 1000);

                // Associate the socket with the player and get the IP address.
                socket = conn;
                ip = socket.RemoteEndPoint.ToString().Split(':')[0];

                _logger.Log(ip + " connected to the server.");

                // Create default block bindings.
                for (byte i = 0; i < 128; ++i) {
                    bindings[i] = (BlockId)i;
                }

                // Start receiving data and put the data into the temporary buffer.
                socket.BeginReceive(tempbuffer, 0, tempbuffer.Length, SocketFlags.None, new AsyncCallback(Receive), this);

                // Start the thread that keeps track of how long a player has been on the server.
                timespent.Elapsed += delegate {
                    // Do not keep track of the time if the player has not logged in yet.
                    // This is to make sure that the player's amount of time spent has been
                    // retrieved from the SQL table first.
                    if (!Loading) {
                        try {
                            int Days = Convert.ToInt32(time.Split(' ')[0]);
                            int Hours = Convert.ToInt32(time.Split(' ')[1]);
                            int Minutes = Convert.ToInt32(time.Split(' ')[2]);
                            int Seconds = Convert.ToInt32(time.Split(' ')[3]);
                            Seconds++;
                            if (Seconds >= 60) {
                                Minutes++;
                                Seconds = 0;
                            }
                            if (Minutes >= 60) {
                                Hours++;
                                Minutes = 0;
                            }
                            if (Hours >= 24) {
                                Days++;
                                Hours = 0;
                            }
                            time = String.Empty + Days + " " + Hours + " " + Minutes + " " + Seconds;
                        }
                        catch {  // TODO: find exact exception to catch
                            time = "0 0 0 1";
                        }
                    }
                };
                timespent.Start();

                // Start the thread that displays welcome messages.
                loginTimer.Elapsed += delegate {
                    if (!Loading) {
                        loginTimer.Stop(); // Only display the welcome message once.

                        // Read welcome.txt, if present, and display each line of the
                        // welcome message to the user.
                        string welcomePath = Path.Combine("text", "welcome.txt");
                        if (File.Exists(welcomePath)) {
                            try {
                                using (StreamReader wm = File.OpenText(welcomePath)) {
                                    List<string> welcome = new List<string>();
                                    while (!wm.EndOfStream) {
                                        welcome.Add(wm.ReadLine());
                                    }
                                    foreach (string w in welcome) {
                                        SendMessage(w);
                                    }
                                }
                            }
                            catch (Exception e) {
                                _logger.ErrorLog(e);
                            }
                        }
                        // If the server is new, it does not have a welcome.txt file, so create one
                        // with a default message.
                        else {
                            _logger.Log("Could not find welcome.txt. Using default.");
                            File.WriteAllText(welcomePath, "Welcome to my server!");
                            SendMessage("Welcome to my server!");
                        }
                        extraTimer.Start();
                        loginTimer.Dispose();
                    }
                };
                loginTimer.Start();

                // Start the thread that checks if the player is alive by sending ping packets.
                pingTimer.Elapsed += delegate {
                    SendPing();
                };
                pingTimer.Start();

                // Starts the thread that sends additional login messages after a while.
                extraTimer.Elapsed += delegate {
                    extraTimer.Stop();

                    // FIXME: no guarantee that the inbox table exists and that the uuid has been fetched
                    try { // Tell the player how many inbox messages he or she has.
                        if (!_server.commands.NobodyCanUse("inbox") && !_server.commands.NobodyCanUse("send")) {
                            Int64 count = 0;
                            if (CheckIfInboxExists()) {
                                count = _db.ExecuteCount("SELECT COUNT(*) FROM inbox_" + uuid);
                            }

                            SendMessage("&cYou have &f" + count.ToString() + _server.props.DefaultColor + " &cmessages in /inbox");
                        }
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }

                    try { // Displays how much money the player has. Doesn't work though. -Jjp137
                        if (!_server.commands.NobodyCanUse("pay") && !_server.commands.NobodyCanUse("give")
                            && !_server.commands.NobodyCanUse("take")) {
                            SendMessage("You currently have &a" + money + _server.props.DefaultColor + " " + _server.props.moneys);
                        }
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }

                    // Tell the player how many players are on the server.
                    SendMessage("You have modified &a" + overallBlocks + _server.props.DefaultColor + " blocks!");
                    if (_server.players.Count == 1) {
                        SendMessage("There is currently &a" + _server.players.Count + " player online.");
                    }
                    else {
                        SendMessage("There are currently &a" + _server.players.Count + " players online.");
                    }
                    try { // Displays how many awards the player has. Doesn't work though. -Jjp137
                        if (!_server.commands.NobodyCanUse("award") && !_server.commands.NobodyCanUse("awards")
                            && !_server.commands.NobodyCanUse("awardmod")) {
                            SendMessage("You have " + Awards.awardAmount(name) + _server.props.DefaultColor + " awards.");
                        }
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }
                    extraTimer.Dispose();
                };

                // Starts the thread that checks the player's AFK status.
                afkTimer.Elapsed += delegate {
                    // Don't track AFK status for those that have not logged in fully.
                    if (name == String.Empty) {
                        return;
                    }

                    // If the player is considered to be AFK...
                    if (_server.afkset.Contains(name)) {
                        afkCount = 0;
                        // Check to see if the player should be kicked for being AFK for too long.
                        if (_server.props.afkkick > 0 && rank.Permission < _server.props.afkkickperm)
                            // There's a bug here, but I'm not sure what triggers it -Jjp137
                            if (afkStart.AddMinutes(_server.props.afkkick) < DateTime.Now) {
                                Kick("Auto-kick, AFK for " + _server.props.afkkick + " minutes");
                            }
                        // If the player has moved, use /afk to un-afk that person.
                        if ((oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2]) &&
                            (oldrot[0] != rot[0] || oldrot[1] != rot[1])) {
                            _server.commands.FindCommand("afk").Use(this, String.Empty);
                        }
                    }
                    // If the player hasn't been marked as AFK yet...
                    else { 
                        // Check if the player has been in the same place since the last time
                        // the player's position was checked, and increment the counter if this
                        // is the case.
                        if (oldpos[0] == pos[0] && oldpos[1] == pos[1] && oldpos[2] == pos[2] &&
                            oldrot[0] == rot[0] && oldrot[1] == rot[1]) {
                            afkCount++;
                        }
                        else {  // Otherwise, reset the counter because the player has moved.
                            afkCount = 0;
                        }

                        // If the player has been idle long enough, mark the person as AFK.
                        // Since the timer runs every two seconds, 30 is used instead of 60.
                        if (afkCount > _server.props.afkminutes * 30) {
                            if (name != null && !String.IsNullOrEmpty(name.Trim())) {
                                _server.commands.FindCommand("afk").Use(this, "auto: Not moved for " + _server.props.afkminutes + " minutes");

                                // Reset the AFK counter.
                                afkCount = 0;
                            }
                        }
                    }
                };

                // Reset the player's chat spam counter every few seconds.
                resetSpamCount.Elapsed += delegate {
                    if (consecutivemessages > 0) {
                        consecutivemessages = 0;
                    }
                };
                resetSpamCount.Start();

                // Only start the AFK thread if auto-AFK is enabled.
                if (_server.props.afkminutes > 0) {
                    afkTimer.Start();
                }

                // Add the player to the list of connected players.
                connections.Add(this);
            }
            catch (Exception e) {
                Kick("Login failed!");
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Checks if an inbox table exists for this player.
        /// </summary>
        /// <returns> Whether the inbox table exists for this player. </returns>
        private bool CheckIfInboxExists() {
            string query = _db.Engine == SQLEngine.MySQL ? "SHOW TABLES LIKE @name" :
                "SELECT name FROM sqlite_master WHERE type='table' AND name=@name";
            string name = "inbox_" + uuid;

            using (PreparedStatement stmt = new PreparedStatement(_db, query)) {
                stmt.AddParam("@name", name);
                using (DataTable data = stmt.ObtainData()) {
                    return data.Rows.Count > 0;
                }
            }
        }

        /// <summary>
        /// Determine whether the player's client supports the specified extension.
        /// </summary>
        /// <param name="extName"> The name of the extension.  </param>
        /// <param name="version"> The version of the extension.  </param>
        public bool HasExtension (string extName, byte version = 1) {
            // If the client is not a CPE client, it would not support any extensions.
            if (!extension) {
                return false;
            }

            // Return true while a CPE client is still logging in.
            if (!loggedIn && extension) {
                return true;
            }

            // Otherwise, find the extension's name and return true if it is found.
            // TODO: check version too
            return ExtEntry.FindAll(cpe => cpe.name.Contains(extName) == true).Count > 0;
        }

        /// <summary>
        /// Saves all of a player's data to the SQL table. Also, check if a player's personal
        /// preferences have been changed, and save that as well.
        /// </summary>
        public void save() {
            // Create the SQL query.
            // FIXME: PreparedStatement
            string commandString =
                "UPDATE Players SET IP='" + ip + "'" +
                ", LastLogin='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                ", totalLogin=" + totalLogins.ToString() +
                ", totalDeaths=" + overallDeath.ToString() +
                ", Money=" + money.ToString() +
                ", totalBlocks=" + overallBlocks.ToString() +
                ", totalKicked=" + totalKicked.ToString() +
                ", TimeSpent='" + time +
                "' WHERE Name='" + name + "'";

            // Execute the query.
            _db.ExecuteStatement(commandString);

            // Save the player's emoticon preference if it has changed.
            try {
                if (!smileySaved) {
                    if (parseSmiley) {
                        emoteList.RemoveAll(entry => entry.Uuid == uuid);
                    }
                    else {
                        emoteList.Add(new UuidEntry(uuid, name));
                    }

                    // TODO: refactor b/c this is stupid
                    string emotePath = Path.Combine("text", "emotelist.txt");
                    using (StreamWriter writer = new StreamWriter(File.Create(emotePath))) {
                        foreach (UuidEntry entry in emoteList) {
                            writer.WriteLine(entry.ToString());
                        }
                    }

                    smileySaved = true;
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }

            // Save the player's undo buffer.
            try {
                SaveUndo();
            }
            catch (Exception e) {
                _logger.Log("Error saving undo data.");
                _logger.ErrorLog(e);
            }
        }

        #region == INCOMING ==

        /// <summary>
        /// Called when a packet is received from the player's client.
        /// </summary>
        /// <param name="result"> Information about the incoming data. </param>
        static void Receive(IAsyncResult result) {
            // Extract the player's new state from the incoming data.
            Player p = (Player)result.AsyncState;

            // Do not do anything if a connection to the client no longer exists.
            if (p.disconnected || p.socket == null) {
                return;
            }

            try {
                // Stop receiving data. EndReceive() returns an int representing the number of
                // bytes received.
                int length = p.socket.EndReceive(result);

                // If there was nothing in the packet, disconnect the player.
                if (length == 0) {
                    p.Disconnect();
                    return;
                }

                // Combine the incoming data with any data that's still in the buffer.
                byte[] b = new byte[p.buffer.Length + length];
                Buffer.BlockCopy(p.buffer, 0, b, 0, p.buffer.Length);
                Buffer.BlockCopy(p.tempbuffer, 0, b, p.buffer.Length, length);

                // Interpret the data and then clean the buffer.
                p.buffer = p.HandleMessage(b);

                // If there was nothing in the packet, disconnect the player.
                if (p.dontmindme && p.buffer.Length == 0) {
                    p._logger.Log("Disconnected");
                    p.socket.Close();
                    p.disconnected = true;
                    return;
                }
                // If the player is still connected, listen for more data and put any received
                // data into tempbuffer.
                if (!p.disconnected)
                    p.socket.BeginReceive(p.tempbuffer, 0, p.tempbuffer.Length, SocketFlags.None,
                                          new AsyncCallback(Receive), p);
            }
            // Disconnect if accessing the socket causes an error for any reason.
            catch (SocketException) {
                p.Disconnect();
            }
            catch (ObjectDisposedException) {
                p.SaveUndo();
                if (connections.Contains(p)) {
                    connections.Remove(p);
                }
                p.disconnected = true;
            }
            catch (Exception e) {
                p._logger.ErrorLog(e);
                p.Kick("Error!");
            }
        }

        /// <summary>
        /// Read the data within a client's packet and act accordingly.
        /// </summary>
        /// <param name="buffer"> The array of bytes that are in the incoming packet.
        /// </param>
        /// <returns> The new state of the buffer. </returns>
        byte[] HandleMessage(byte[] buffer) {
            try {
                int length = 0;
                byte msg = buffer[0]; // This is the packet ID.

                // Get the length of the rest of the packet by checking the first byte.
                // The first byte indicates the type of packet that is being sent, and from
                // that, we can figure out the size of the packet since a given type of
                // packet will always have the same size.
                switch (msg) {
                    case 0: // Login packet
                        length = 130;
                        break;
                    case 5: // Block change packet
                        if (!loggedIn) {
                            goto default;
                        }
                        length = 8;
                        break;
                    case 8: // Input packet
                        if (!loggedIn) {
                            goto default;
                        }
                        length = 9;
                        break;
                    case 13: // Chat packet
                        if (!loggedIn) {
                            goto default;
                        }
                        length = 65;
                        break;
                    case 16: // CPE: ExtInfo packet
                        length = 66;
                        break;
                    case 17: // CPE: ExtEntry packet
                        length = 68;
                        break;
                    case 19: // CPE: CustomBlockSupportLevel packet
                        length = 1;
                        break;
                    default: // Unknown packet
                        if (!dontmindme) {
                            Kick("Unhandled message id \"" + msg + "\"!");
                        }
                        else {
                            _logger.Log(Encoding.UTF8.GetString(buffer, 0, buffer.Length));
                        }
                        return new byte[0];
                }

                // If there is more in the buffer than the length of the current message...
                // (There always is; the first byte in the buffer isn't counted in the length
                // variable, so... -Jjp137)
                if (buffer.Length > length) {
                    // Copy the bytes representing the current packet's payload into an array.
                    // Exclude the first byte since we already have read it.
                    byte[] message = new byte[length];
                    Buffer.BlockCopy(buffer, 1, message, 0, length);

                    // Copy the leftover bytes into a different array. Subtract length to exclude
                    // the current data being examined, and subtract 1 to exclude the first byte.
                    byte[] tempbuffer = new byte[buffer.Length - length - 1];
                    Buffer.BlockCopy(buffer, length + 1, tempbuffer, 0, buffer.Length - length - 1);

                    // Set the buffer variable to be those leftover bytes.
                    buffer = tempbuffer;

                    // Then, depending on the length of the byte array representing the data,
                    // which was determined earlier, call one of the following methods.
                    switch (msg) {
                        case 0:
                            HandleLogin(message);
                            break;
                        case 5:
                            if (!loggedIn) {
                                break;
                            }
                            HandleBlockchange(message);
                            break;
                        case 8:
                            if (!loggedIn) {
                                break;
                            }
                            HandleInput(message);
                            break;
                        case 13:
                            if (!loggedIn) {
                                break;
                            }
                            HandleChat(message);
                            break;
                        case 16:
                            HandleExtInfo(message);
                            break;
                        case 17:
                            HandleExtEntry(message);
                            break;
                        case 19:
                            HandleCustomBlockSupportLevel(message);
                            break;
                    }

                    // If there are leftover bytes to be read, use recursion to read more data.
                    if (buffer.Length > 0) {
                        buffer = HandleMessage(buffer);
                    }
                    else {
                        return new byte[0];
                    }
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
            return buffer;
        }

        /// <summary>
        /// Parses the ExtInfo packet that is sent by a CPE client joining the server.
        /// </summary>
        /// <param name="message"> The array of bytes reprsenting the ExtInfo packet. 
        /// </param>
        public void HandleExtInfo(byte[] message) {
            // Obtain the client program's name and the number of extensions the client supports.
            appName = enc.GetString(message, 0, 64).Trim();
            extensionCount = message[65];
        }

        /// <summary>
        /// Parses the ExtEntry packet that is sent by a CPE client joining the server.
        /// </summary>
        /// <param name="msg"> The array of bytes reprsenting the ExtEntry packet. 
        /// </param>
        void HandleExtEntry(byte[] msg) {
            // Obtain the extension name and the version of the extension that the client supports.
            CPE tmp;
            tmp.name = enc.GetString(msg, 0, 64);
            tmp.version = BitConverter.ToInt32(msg, 64);
            // Add this to the list of extensions that the client supports.
            ExtEntry.Add(tmp);
        }

        /// <summary>
        /// Parses the CustomBlockSupportLevel packet that is sent by a CPE client joining the server.
        /// </summary>
        /// <param name="message"> The array of bytes reprsenting the CustomBlockSupportLevel packet. 
        /// </param>
        public void HandleCustomBlockSupportLevel(byte[] message) {
            customBlockSupportLevel = message[0];
        }

        /// <summary>
        /// Parses the player identification packet that is sent by a player joining the server.
        /// </summary>
        /// <param name="message"> The array of bytes reprsenting the player identification packet. 
        /// </param>
        void HandleLogin(byte[] message) {
            try {
                // Ignore this packet if the player is already logged in.
                if (loggedIn) {
                    return;
                }

                // The first byte is the protocol version.
                byte version = message[0];
                // The next 64 bytes represent the player's name that is being used to
                // connect to the server.
                name = enc.GetString(message, 1, 64).Trim();

                // Set the other name variables to the name the client connected with.
                DisplayName = name;
                truename = name;

                // Handle e-mail logins by kicking them.
                if (name.Split('@').Length > 1) {
                    Kick("E-mail logins are not supported. Use another type of login.");
                    return;
                }

                // The next 32 bytes contains the verification key used to check if the player
                // is truly who he says he is.
                string verify = enc.GetString(message, 65, 32).Trim();
                // The last byte is unused, except for CPE clients.
                ushort type = message[129];

                // Note whether the name will be verified.
                verifiedName = _server.props.verify ? true : false;

                // Verify that user is who they claim to be by computing md5 hashes if verify-names is on.
                if (_server.props.verify) {
                    // This is for minecraft.net accounts.
                    if (verify == BitConverter.ToString(md5.ComputeHash(enc.GetBytes(_server.props.minecraftSalt + truename))).Replace("-",
                            String.Empty).ToLower().TrimStart('0')) {
                        uuid = Uuid.GetMojangUuid(name);
                        identified = uuid.IsValid;
                    }
                    // This is for ClassiCube accounts.
                    else if (verify == BitConverter.ToString(md5.ComputeHash(enc.GetBytes(_server.props.classicubeSalt + truename))).Replace("-",
                            String.Empty).ToLower()) {
                        // Attempt to obtain the player's ClassiCube ID if the name was verified.
                        uuid = Uuid.GetClassiCubeUuid(name);
                        identified = uuid.IsValid;
                        name += "+"; // The plus sign indicates a ClassiCube client.
                    }
                    // Private IPs are exempt.
                    /*if (IPInPrivateRange(ip)) {
                        identified = true;
                    }*/
                    if (!identified) {
                        Kick("Login failed! Try again.");
                        return;
                    }
                }

                // TODO: document
                if (!Uuid.UuidExists(_db, uuid)) {
                    Uuid.AddUuidToTable(_server.database, uuid, name);
                    Uuid.CleanupTemporaryUuid(_server, uuid, name);
                }
                else if (Uuid.FindName(_db, uuid) != name) {
                    Uuid.ChangeName(_server.database, uuid, name);
                }

                // Check if the user is still temporarily banned.
                try {
                    Server.TempBan tBan = _server.tempBans.Find(tB => tB.name.ToLower() == name.ToLower());

                    if (tBan.allowedJoin < DateTime.Now) {
                        _server.tempBans.Remove(tBan);
                    }
                    else {
                        Kick("You're still banned (temporary ban)!");
                    }
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }

                // Check if the player is on the whitelist.
                if (_server.props.useWhitelist) {
                    if (_server.props.verify) { // If verify names is on, then it's easy to be sure.
                        if (_server.whiteList.Contains(uuid)) {
                            onWhitelist = true;
                        }
                    }
                    else { // Otherwise, we have to do an SQL lookup to check if the IP is the same.
                        // FIXME: PreparedStatement
                        using (DataTable ipQuery = _db.ObtainData("SELECT Name FROM Players WHERE IP = '" + ip + "'")) {
                            if (ipQuery.Rows.Count > 0) {
                                if (ipQuery.Rows.Contains(name) && _server.whiteList.Contains(uuid)) {
                                    onWhitelist = true;
                                }
                            }
                        }
                    }
                }

                // Load that player's ignore list.
                string ignorePath = Path.Combine("ranks", "ignore", this.name + ".txt");
                if (File.Exists(ignorePath)) {
                    try {
                        string[] checklines = File.ReadAllLines(ignorePath);
                        foreach (string checkline in checklines) {
                            this.listignored.Add(checkline);
                        }
                        File.Delete(ignorePath);
                    }
                    catch (Exception e) {  // TODO: probably catch a specific exception
                        _logger.ErrorLog(e);
                        _logger.Log("Failed to load ignore list for: " + this.name);
                    }
                }

                // Probably for the online Global Chat... -Jjp137
                string globalIgnorePath = Path.Combine("ranks", "ignore", "GlobalIgnore.xml");
                if (File.Exists(globalIgnorePath)) {
                    try {
                        string[] searchxmls = File.ReadAllLines(globalIgnorePath);
                        foreach (string searchxml in searchxmls) {
                            globalignores.Add(searchxml);
                        }
                        foreach (string ignorer in globalignores) {
                            Player foundignore = _server.players.Find(ignorer);
                            foundignore.ignoreglobal = true;
                        }
                        File.Delete(globalIgnorePath);
                    }
                    catch (Exception e) {  // TODO: probably catch a specific exception
                        _logger.ErrorLog(e);
                        _logger.Log("Failed to load global ignore list!");
                    }
                }

                // If the player is IP-banned and not on the whitelist, kick him.
                if (_server.bannedIP.Contains(ip)) {
                    if (_server.props.useWhitelist) {
                        if (!onWhitelist) {
                            Kick(_server.props.customBanMessage);
                            return;
                        }
                    }
                    else {
                        Kick(_server.props.customBanMessage);
                        return;
                    }
                }

                // If the player is banned and not on the whitelist, kick him.
                if (_server.ranks.FindPlayerRank(uuid) == _server.ranks.FindPerm(DefaultRankValue.Banned)) {
                    if (_server.props.useWhitelist) {
                        if (!onWhitelist) {
                            Kick(_server.props.customBanMessage);
                            return;
                        }
                    }
                    else {
                        // Otherwise, just kick the player normally.
                        Kick(_server.props.customBanMessage);
                        return;
                    }
                }

                // If the player is not a VIP, that player is subject to the player limit.
                if (!VIP.Find(this)) {
                    // Check how many players the server has.
                    if (_server.players.Count >= _server.props.maxPlayers) {
                        Kick("Server full!");
                        return;
                    }

                    // If the connecting player is a guest, check if the guest limit has been reached.
                    // Kick the player if there are too many of them.
                    if (_server.ranks.FindPlayerRank(uuid) == _server.ranks.FindPerm(DefaultRankValue.Guest)) {
                        int currentNumOfGuests = _server.players.Count(pl => pl.rank.Permission <= DefaultRankValue.Guest);
                        if (currentNumOfGuests >= _server.props.maxGuests) {
                            _logger.Log("Guest " + this.name + " couldn't log in - too many guests.");
                            Kick("Server has reached max number of guests");
                            return;
                        }
                    }
                }

                // If the client is using the wrong version of the server protocol, kick the player.
                if (version != Server.version) {
                    Kick("Wrong version!");
                    return;
                }

                // If there is another instance of that player, handle it based on the verify-names setting.
                foreach (Player p in _server.players) {
                    if (p.name == name) {
                        // If verify-names is on, we are certain that whoever is currently logging in is the 
                        // real player, so kick the client with the same player name. The most likely case is
                        // that the player lost connection to the server.
                        if (_server.props.verify) {
                            p.Kick("Same player is reconnecting.");
                            break;
                        }
                        // Otherwise, assume that someone is trying to forge that player's identity.
                        else {
                            Kick("Someone else is logged in with this username!");
                            return;
                        }
                    }
                }

                // FIXME: this is just a temporary and crappy band-aid
                // Check the list of connecting players as well.
                foreach (Player p in connections) {
                    if (p == this) {
                        continue;
                    }

                    if (p.name == name) {
                        // If verify-names is on, we are certain that whoever is currently logging in is the
                        // real player, so kick the client with the same player name. The most likely case is
                        // that the player lost connection to the server.
                        if (_server.props.verify) {
                            p.Kick("Same player is reconnecting.");
                            break;
                        }
                        // Otherwise, assume that someone is trying to forge that player's identity.
                        else {
                            Kick("Someone else is logged in with this username!");
                            return;
                        }
                    }
                }

                // 0x42 is the number for the padding byte that CPE clients use, and the present of this
                // byte indicates that the server is using a CPE client.
                if (type == 0x42) {
                    extension = true;
                    SendExtInfo(14);
                    SendExtEntry("ClickDistance", 1);
                    SendExtEntry("CustomBlocks", 1);
                    SendExtEntry("HeldBlock", 1);
                    SendExtEntry("TextHotKey", 1);
                    SendExtEntry("ExtPlayerList", 1);
                    SendExtEntry("EnvColors", 1);
                    SendExtEntry("SelectionCuboid", 1);
                    SendExtEntry("BlockPermissions", 1);
                    SendExtEntry("ChangeModel", 1);
                    SendExtEntry("EnvMapAppearance", 1);
                    SendExtEntry("EnvWeatherType", 1);
                    SendExtEntry("HackControl", 1);
                    SendExtEntry("EmoteFix", 1);
                    SendExtEntry("MessageTypes", 1);
                    SendCustomBlockSupportLevel(1);
                }

                // Done to avoid erroneous "Lately known as" messages.
                try {
                    left.Remove(name.ToLower());
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }

                // Find out what rank the player is, and assign that rank.
                rank = _server.ranks.FindPlayerRank(uuid);

                // Send the motd and the map.
                SendMotd();
                SendMap();
                Loading = true;

                // If he left before the map loaded for him, do not continue any more.
                if (disconnected) {
                    return;
                }

                // At this point, the player has logged in. Find an available player id
                // for the client. This is not the same as a ClassiCube id.
                loggedIn = true;
                this.serverId = FreeId(_server.players);

                // Add the player to the list of players on the server.
                lock (_server.players)
                    _server.players.Add(this);

                // Remove the player from the list of players waiting to connect.
                connections.Remove(this);

                string temp = name + " is lately known as:";
                bool found = false;
                // Ignore localhost when determining if two or more players share the same IP address.
                if (!ip.StartsWith("127.0.0.")) {
                    // Check the list of players that have left the server to see if any IPs
                    // match the player that logged in.
                    foreach (KeyValuePair<string, string> prev in left) {
                        if (prev.Value == ip) {
                            found = true;
                            temp += " " + prev.Key;
                        }
                    }
                    // Notify the ops that a player with the same IP address logged in.
                    if (found) {
                        if (this.rank.Permission < _server.props.adminchatperm || _server.props.adminsjoinsilent == false) {
                            _server.GlobalMessageOps(temp);
                        }

                        _logger.Log(temp);
                    }
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                _server.GlobalMessage("An error occurred: " + e.Message);
            }

            // Load the player's SQL data.
            DataTable playerDb = null;

            using (PreparedStatement stmt = _db.MakePreparedStatement("SELECT * FROM Players WHERE Name=@name")) {
                stmt.AddParam("@name", name);
                playerDb = stmt.ObtainData();
            }

            // Generate default data if the player has not visited the server before.
            if (playerDb.Rows.Count == 0) {
                this.prefix = String.Empty;
                this.time = "0 0 0 1";
                this.title = String.Empty;
                this.titlecolor = String.Empty;
                this.color = rank.color;
                this.money = 0;
                this.firstLogin = DateTime.Now;
                this.totalLogins = 1;
                this.totalKicked = 0;
                this.overallDeath = 0;
                this.overallBlocks = 0;

                this.timeLogged = DateTime.Now;
                SendMessage("Welcome " + name + "! This is your first visit.");

                // Insert a new entry into the players table.
                // FIXME: PreparedStatement
                var query = "INSERT INTO Players (Name, IP, FirstLogin, LastLogin, totalLogin, Title, totalDeaths, Money, totalBlocks, totalKicked, TimeSpent)" +
                    " VALUES ('" + name + "', '" + ip + "', '" + firstLogin.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', " +
                    totalLogins.ToString() + ", '" + prefix + "', " + overallDeath.ToString() + ", " + money.ToString() + ", " + loginBlocks.ToString() + ", " + totalKicked.ToString() + ", '" + time + "')";
                _db.ExecuteStatement(query);
            }
            // Otherwise, retrieve the player's data.
            else {
                DataRow playerData = playerDb.Rows[0];

                totalLogins = int.Parse(playerData["totalLogin"].ToString()) + 1;
                time = playerData["TimeSpent"].ToString();
                userID = int.Parse(playerData["ID"].ToString());
                firstLogin = DateTime.Parse(playerData["firstLogin"].ToString());
                timeLogged = DateTime.Now;
                if (playerData["Title"].ToString().Trim() != String.Empty) {
                    string parse = playerData["Title"].ToString().Trim().Replace("[", String.Empty);
                    title = parse.Replace("]", String.Empty);
                }
                if (playerData["title_color"].ToString().Trim() != String.Empty) {
                    titlecolor = Colors.Parse(playerData["title_color"].ToString().Trim());
                }
                else {
                    titlecolor = String.Empty;
                }
                if (playerData["color"].ToString().Trim() != String.Empty) {
                    color = Colors.Parse(playerData["color"].ToString().Trim());
                }
                else {
                    color = rank.color;
                }
                overallDeath = int.Parse(playerData["TotalDeaths"].ToString());
                overallBlocks = long.Parse(playerData["totalBlocks"].ToString().Trim());
                money = int.Parse(playerData["Money"].ToString());
                totalKicked = int.Parse(playerData["totalKicked"].ToString());

                SendMessage("Welcome back " + color + prefix + name + _server.props.DefaultColor + "! You've been here " + totalLogins +
                            " times!");

                if (_server.muted.Contains(uuid)) {
                    muted = true;
                    _server.GlobalMessage(name + " is still muted from the last time they went offline.");
                }
            }

            // Set the text for the player's title.
            SetPrefix();

            // Call any methods listening for this event.
            EventHandler method = PlayerConnected;
            if (method != null) {
                method(this, EventArgs.Empty);
            }

            // Get rid of the connection to the database.
            playerDb.Dispose();

            try {
                // Send the spawn position to the client.
                ushort x = (ushort)((0.5 + level.spawnx) * 32);
                ushort y = (ushort)((1 + level.spawny) * 32);
                ushort z = (ushort)((0.5 + level.spawnz) * 32);
                pos = new ushort[3] { x, y, z };
                rot = new byte[2] { level.rotx, level.roty };

                // Tell everyone else that the player has spawned.
                _server.GlobalSpawn(this, x, y, z, rot[0], rot[1], true);
                // Display other visible players to the connecting client that are in the same level.
                foreach (Player p in _server.players) {
                    if (p.level == level && p != this && !p.hidden) {
                        SendSpawn(p.serverId, p.color + p.name, p.pos[0], p.pos[1], p.pos[2], p.rot[0], p.rot[1]);
                    }
                    // Change the player's model for everyone else.
                    if (HasExtension("ChangeModel")) {
                        SendChangeModel(p.serverId, p.model);
                    }
                }

                // Display all bots to the connecting client that are in the same level.
                foreach (PlayerBot pB in PlayerBot.playerbots) {
                    if (pB.level == level) {
                        SendSpawn(pB.id, pB.color + pB.name, pB.pos[0], pB.pos[1], pB.pos[2], pB.rot[0], pB.rot[1]);
                    }
                }
                // TODO: this is broken and not working right
                _server.players.ForEach(delegate(Player p) {
                    if (p != this && p.HasExtension("ExtPlayerList")) {
                        p.SendExtAddPlayerName(serverId, name, rank, color + name);
                    }
                    if (HasExtension("ExtPlayerList")) {
                        SendExtAddPlayerName(p.serverId, p.name, p.rank, p.color + p.name);
                    }
                });
                // TODO: this is probably broken too
                if (HasExtension("EnvMapAppearance")) {
                    SendSetMapAppearance(_server.props.textureUrl, 7, 8, (short)(level.height/2));
                }
            }
            catch ( Exception e ) {
                _logger.ErrorLog( e );
                _logger.Log( "Error spawning player \"" + name + "\"" );
            }

            // The player should be done loading by now.
            Loading = false;


            // Check if the player needs to verify that he or she is an admin.
            if (_server.props.verifyadmins == true) {
                if (this.rank.Permission >= _server.props.verifyadminsrank) {
                    adminpen = true;
                }
            }

            // Check if player doesn't want to see emoticons.
            // TODO: this is awkward so fix this
            if (emoteList.FindIndex(delegate(UuidEntry entry) { return entry.Uuid == uuid; }) != -1) {
                parseSmiley = false;
            }

            loggedIn = true;  // TODO: check if this is needed

            // Check if the player was still jailed from the last time they logged in.
            bool gotoJail = false;
            string gotoJailMap = String.Empty;
            string gotoJailName = String.Empty;
            try {
                string jailedPath = Path.Combine("ranks", "jailed.txt");

                if (File.Exists(jailedPath)) {
                    using (StreamReader read = new StreamReader(jailedPath)) {
                        string line;
                        while ((line = read.ReadLine()) != null) {
                            if (line.Split()[0].ToLower() == this.name.ToLower()) {
                                gotoJail = true;
                                gotoJailMap = line.Split()[1];
                                gotoJailName = line.Split()[0];
                                break;
                            }
                        }
                    }
                }
                else {
                    File.Create(jailedPath).Close();
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                gotoJail = false;
            }
            if (gotoJail) {
                try {
                    _server.commands.FindCommand("goto").Use(this, gotoJailMap);
                    _server.commands.FindCommand("jail").Use(_server.ServerConsole, gotoJailName);
                }
                catch (Exception e) {
                    Kick(e.ToString());
                }
            }

            // Check if the player needs to agree to the rules.
            if (_server.props.agreetorulesonentry) {
                string agreedPath = Path.Combine("ranks", "agreed.txt");

                if (!File.Exists(agreedPath)) {
                    File.WriteAllText(agreedPath, String.Empty);
                }
                var agreedFile = File.ReadAllText(agreedPath);
                if (this.rank.Permission == DefaultRankValue.Guest) {
                    if (!agreedFile.Contains(this.name.ToLower())) {
                        SendMessage("&9You must read the &c/rules&9 and &c/agree&9 to them before you can build and use commands!");
                    }
                    else {
                        agreed = true;
                    }
                }
                else {
                    agreed = true;
                }
            }
            else {
                agreed = true;
            }

            // Create the "player joined" message.
            string joinm = "&a+ " + this.color + this.prefix + this.name + _server.props.DefaultColor +
                           " joined the server.";

            // Display the "player joined" message if the relevant settings allow for it.
            if (this.rank.Permission < _server.props.adminchatperm || _server.props.adminsjoinsilent == false) {
                _server.players.ForEach(p1 => p1.SendMessage(joinm));
            }

            // If the player is an admin and admins-join-slient is on, don't announce that player's arrival.
            if (this.rank.Permission >= _server.props.adminchatperm && _server.props.adminsjoinsilent == true) {
                this.hidden = true;
                this.adminchat = true;
            }

            // Tell admins that they have to verify by entering their admin password.
            if (_server.props.verifyadmins) {
                if (this.rank.Permission >= _server.props.verifyadminsrank) {
                    string passwordsDir = Path.Combine("extra", "passwords");
                    string passwordPath = Path.Combine("extra", "passwords", this.uuid + ".dat");

                    if (!Directory.Exists(passwordsDir) || !File.Exists(passwordPath)) {
                        this.SendMessage("&cPlease set your admin verification password with &a/setpass [Password]!");
                    }
                    else {
                        this.SendMessage("&cPlease complete admin verification with &a/pass [Password]!");
                    }
                }
            }

            // Load the player's waypoints.
            try {
                Waypoint.Load(this);
            }
            catch (Exception ex) {
                SendMessage("Error loading waypoints!");
                _logger.ErrorLog(ex);
            }

            // Check if the player was still muted from the last time the player logged in.
            try {
                string mutedPath = Path.Combine("ranks", "muted.txt");
                if (File.Exists(mutedPath)) {
                    using (StreamReader read = new StreamReader(mutedPath)) {
                        string line;
                        while ((line = read.ReadLine()) != null) {
                            if (line.ToLower() == this.name.ToLower()) {
                                this.muted = true;
                                this.SendMessage("!%cYou are still %8muted%c since your last login.");
                                break;
                            }
                        }
                    }
                }
                else {
                    File.Create(mutedPath).Close();
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                muted = false;
            }

            // Announce that the player has joined the server in the console's log.
            _logger.Log(name + " [" + ip + "] + has joined the server.");
            _logger.Log(name + " has a uuid of: " + uuid);

            // Lego's mute code follows.
            ThreadStart ts = new ThreadStart(XMute);
            StartThread(ts);
        }

        /// <summary>
        /// Update the text that comes before the user's name.
        /// </summary>
        public void SetPrefix() {
            prefix = (title == String.Empty) ? String.Empty : color + "[" + titlecolor + title + color + "] ";
        }

        /// <summary>
        /// Parses the block change packet that is sent by the player.
        /// </summary>
        /// <param name="message"> The array of bytes representing the block change packet. 
        ///  </param>
        void HandleBlockchange(byte[] message) {
            try {
                // Make sure that the player can actually modify blocks.
                if (!loggedIn) {
                    return;
                }
                // Don't let players spam blocks.
                if (CheckBlockSpam()) {
                    return;
                }

                // Flip the bytes before reading them.
                ushort x = NetworkUtil.NetworkToHostOrder(message, 0);
                ushort y = NetworkUtil.NetworkToHostOrder(message, 2);
                ushort z = NetworkUtil.NetworkToHostOrder(message, 4);
                byte action = message[6]; // created = 1; destroyed = 0
                ushort type = message[7]; // type of block

                manualChange(x, y, z, action, (BlockId)type);
            }
            catch (Exception e) {
                // Don't ya just love it when the server tattles?
                // Jjp137's response: no.
                _server.GlobalMessageOps(name + " has triggered a block change error");
                _server.GlobalMessageOps(e.GetType().ToString() + ": " + e.Message);
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Handles a normal block change event, which is a block change that occurs when someone
        /// right-clicks to place a block.
        /// </summary>
        /// <param name="x"> The x coordinate of the block being changed. </param>
        /// <param name="y"> The y coordinate of the block being changed. </param>
        /// <param name="z"> The z coordinate of the block being changed. </param>
        /// <param name="action"> This is 1 if a block was created, and 0 if a block was destroyed. </param>
        /// <param name="type"> The block type that was involved.  <seealso cref="BlockId"/></param>
        public void manualChange(ushort x, ushort y, ushort z, byte action, BlockId type) {
            // Prevent hackers and such...
            if (Convert.ToInt32(type) > 65) {
                Kick("Unknown block type!");
                return;
            }

            // Get the original block type in that particular location and make sure the
            // coordinates are valid.
            BlockId b = level.GetTile(x, y, z);
            if (b == BlockId.Null) {
                return;
            }

            // Don't change the block if the player is jailed or if they haven't agreed to the rules.
            // Clients always assume that the block change was successful, so if it is not
            // successful on the server's side, a block change packet that changes the block on
            // the client's side to the original block must be sent from the server.
            if (jailed || !agreed) {
                SendBlockchange(x, y, z, b);
                return;
            }
            // Don't do anything if the player is in a museum and a cuboid isn't active.
            if (level.name.Contains("Museum " + _server.props.DefaultColor) && _blockSelected == null) {
                return;
            }

            // Do not do anything if physics effects are waiting to be applied to the block.
            if (!deleteMode) {
                string info = level.foundInfo(x, y, z);
                if (info.Contains("wait")) {
                    return;
                }
            }

            // Don't do anything if a player is possessed or can't build for some reason.
            // Resend the original block instead so that the client knows that the block has
            // not been modified.
            if (!canBuild) {
                SendBlockchange(x, y, z, b);
                return;
            }

            // Admin verification stuff.
            if (_server.props.verifyadmins == true) {
                if (this.adminpen == true) {
                    SendBlockchange(x, y, z, b);
                    this.SendMessage("&cYou must use &a/pass [Password]&c to verify!");
                    return;
                }
            }

            // Create a BlockPos struct to hold block history data.
            Level.BlockPos bP = new Level.BlockPos();
            bP.name = name;
            bP.TimePerformed = DateTime.Now;
            bP.x = x;
            bP.y = y;
            bP.z = z;
            bP.type = type;

            // Update the player's last click data.
            lastClick[0] = x;
            lastClick[1] = y;
            lastClick[2] = z;

            // Handle any commands, such as /cuboid or /about, that are waiting for a selected block.
            Action<Player, CommandTempData> selected = _blockSelected;
            Dictionary<string, object> dict = _commandData;
            if (selected != null) {
                CommandTempData data = new CommandTempData(x, y, z, type, dict);
                selected(this, data);

                return;
            }

            // Tell subscribers that a block was changed manually.
            EventHandler<BlockChangedEventArgs> changed = BlockChanged;
            if (changed != null) {
                changed(this, new BlockChangedEventArgs(x, y, z, type));
                return;
            }

            // Banned people can't build, and they may not realize that what they are building is
            // not seen by other players because the client always assumes that the block changes
            // always go through. The server won't update a client's data if the player is banned.
            if (rank.Permission == DefaultRankValue.Banned) {
                return;
            }

            // Impose some restrictions on guests.
            if (rank.Permission == DefaultRankValue.Guest) {
                int Diff = 0; // The distance between the block being modified and the player.

                // Remember that packets have coordinates in fixed point, so divide by 32.
                Diff = Math.Abs((int)(pos[0] / 32) - x);
                Diff += Math.Abs((int)(pos[1] / 32) - y);
                Diff += Math.Abs((int)(pos[2] / 32) - z);

                // If a player is trying to modify a block that is too far away, and he isn't using
                // the /click command, do not let the player do so.
                if (Diff > 12) {
                    if (lastCMD != "click") {
                        _logger.Log(name + " attempted to build with a " + Diff.ToString() + " distance offset");
                        _server.GlobalMessageOps("To Ops &f-" + color + name + "&f- attempted to build with a " + Diff.ToString() + " distance offset");
                        SendMessage("You can't build that far away.");
                        SendBlockchange(x, y, z, b);
                        return;
                    }
                }

                // If the guest depth limit is on, prevent a guest from digging too far down.
                if (_server.props.antiTunnel) {
                    if (!ignoreGrief) {
                        if (y < level.height / 2 - _server.props.maxDepth) {
                            SendMessage("You're not allowed to build this far down!");
                            SendBlockchange(x, y, z, b);
                            return;
                        }
                    }
                }
            }

            // This if-statement disallows building if three conditions are true:
            // - If the player does not have a high enough rank to (re)place the block he's breaking.
            // - If the player is not replacing liquids.
            // - If the player is not allowed to break the block at all, even at any rank.
            if (!_server.blockPerms.CanPlace(this, b) && !BlockData.BuildIn(b) && !BlockData.AllowBreak(b)) {
                SendMessage("Cannot build here!");
                SendBlockchange(x, y, z, b); // Resend the original block
                return;
            }

            // If it isn't one of the default block types in the Classic client...
            if (!_server.blockPerms.CanPlace(this, type)) {
                SendMessage("You can't place this block type!");
                SendBlockchange(x, y, z, b);
                return;
            }

            // Players can't place blocks on door_airs.
            if (Convert.ToInt32(b) >= 200 && Convert.ToInt32(b) < 220) {
                SendMessage("Block is active, you cant disturb it!");
                SendBlockchange(x, y, z, b);
                return;
            }

            // Another instance of preventing invalid packets; kick players that transmit packets
            // with unusual block change actions.
            if (action > 1) {
                Kick("Unknown block action!");
            }

            // Store the former block type that the player placed in case a block binding 
            // changes it.
            BlockId oldType = type;
            // If the player used /bind for that block type, then actually plack the block that is
            // binded to that block type.
            type = bindings[(int)type];
            // These two blocks of if statements are garbage...
            // - Do not waste packets if a player is in paint mode or creating a block, and the
            // block that he is creating is the same as the block that is there.
            // - ...or do not waste packets if a player is destroying an air block.
            // - In addition, resend the original block if the player is in paint mode or if a 
            // binding took place.
            if (b == ((painting || action == 1) ? type : BlockId.Air)) {
                if (painting || oldType != type) {
                    SendBlockchange(x, y, z, b);
                }
                return;
            }

            // If a player destroyed a block and is not in paint mode...
            if (!painting && action == 0) {
                // Handle portals and mbs unless the player wants to delete them.
                if (!deleteMode) {
                    if (BlockData.portal(b)) {
                        HandlePortal(this, x, y, z, b);
                        return;
                    }
                    if (BlockData.mb(b)) {
                        HandleMsgBlock(this, x, y, z, b);
                        return;
                    }
                }

                // Otherwise, delete the block, and record it.
                bP.deleted = true;
                level.blockCache.Add(bP);
                deleteBlock(b, type, x, y, z);
            }
            else { // Create/place the block otherwise.
                bP.deleted = false;
                level.blockCache.Add(bP);
                placeBlock(b, type, x, y, z);
            }
        }

        /// <summary>
        /// Method that contains portal behavior.
        /// </summary>
        /// <param name="p"> The player to be teleported. <seealso cref="Player"/></param>
        /// <param name="x"> The x coordinate of the portal block. </param>
        /// <param name="y"> The y coordinate of the portal block. </param>
        /// <param name="z"> The z coordinate of the portal block. </param>
        /// <param name="b"> The block type of the portal. <seealso cref="BlockId"/></param>
        public void HandlePortal(Player p, ushort x, ushort y, ushort z, BlockId b) {
            // Get the portal data from the SQL table.
            // TODO: foundPortals.Dispose() somewhere
            DataTable foundPortals = null;
            try {
                // FIXME: PreparedStatement
                string query = "SELECT * FROM `Portals" + level.name + "` WHERE EntryX=" + x.ToString() +
                    " AND EntryY=" + y.ToString() + " AND EntryZ=" + z.ToString();
                foundPortals = _db.ObtainData(query);

                int LastPortal = foundPortals.Rows.Count - 1;

                if (LastPortal > -1) { // This would mean that a portal at that location was found.
                    string destLevelName = foundPortals.Rows[LastPortal]["ExitMap"].ToString();

                    // Handle portals that lead to other levels.
                    if (level.name != destLevelName) {
                        Level thisLevel = level;
                        // TODO: possible bug here
                        _server.commands.FindCommand("goto").Use(this, foundPortals.Rows[LastPortal]["ExitMap"].ToString());

                        Level destLevel = _server.levels.FindExact(destLevelName);
                        // ugh this is what happens when the structure of your code is awful -Jjp137
                        // Check if a player is allowed to go to that level...
                        if (destLevel != null && destLevel.permissionvisit > this.rank.Permission) {
                            p.SendMessage("Your rank does not allow you to go there using a portal.");
                            foundPortals.Dispose();
                            return;
                        }
                        // Check if the destination level did not load for whatever reason.
                        else if (thisLevel == level) {
                            p.SendMessage("The map the portal goes to isn't loaded.");
                            foundPortals.Dispose();
                            return;
                        }
                    }
                    // The portal is on the same level, so resend the portal block since it isn't actually 
                    //  broken on the server's end.
                    else { 
                        SendBlockchange(x, y, z, b);
                    }

                    // Wait for the player to spawn in the map and then move the player. If the portal
                    // leads to somewhere on the same level, the while loop will not get executed.
                    while (p.Loading) { }
                    _server.commands.FindCommand("move").Use(this, this.name + " " + foundPortals.Rows[LastPortal]["ExitX"].ToString() + " " + foundPortals.Rows[LastPortal]["ExitY"].ToString() + " " + foundPortals.Rows[LastPortal]["ExitZ"].ToString());
                }
                else { // Delete the portal if no SQL entry actually exists.
                    level.Blockchange(this, x, y, z, (ushort)BlockId.Air);
                    p.SendMessage("Portal had no exit.");
                }
            }
            catch (Exception e) {  // TODO: find exact exception to catch
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Method that contains message block behavior.
        /// </summary>
        /// <param name="p"> The player that will read the message. <seealso cref="Player"/></param>
        /// <param name="x"> The x coordinate of the message block. </param>
        /// <param name="y"> The y coordinate of the message block. </param>
        /// <param name="z"> The z coordinate of the message block. </param>
        /// <param name="b"> The block type of the message block. <seealso cref="BlockId"/></param>
        public void HandleMsgBlock(Player p, ushort x, ushort y, ushort z, BlockId b) {
            try {
                // Get the data for the message block.
                // FIXME: PreparedStatement
                string query = "SELECT * FROM `Messages" + level.name + "` WHERE X=" + x.ToString() +
                    " AND Y=" + y.ToString() + " AND Z=" + z.ToString();
                DataTable Messages = _db.ObtainData(query);

                int LastMsg = Messages.Rows.Count - 1;
                if (LastMsg > -1) { // This would mean that a message block was found at that location.
                    string message = Messages.Rows[LastMsg]["Message"].ToString().Trim();
                    p.SendMessage(message);
                    SendBlockchange(x, y, z, b);
                }
                else { // Delete the message block if no message actually exists there.
                    level.Blockchange(this, x, y, z, (ushort)BlockId.Air);
                    p.SendMessage("No message was stored.");
                }
                Messages.Dispose();
            }
            catch (Exception e) {  // TODO: find exact exception to catch.
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Deletes a block at a given position.
        /// </summary>
        /// <param name="b"> The block type to be deleted/replaced. <seealso cref="BlockId"/></param>
        /// <param name="type"> The block type to place. (not used) <seealso cref="BlockId"/></param>
        /// <param name="x"> The x coordinate of the block that is affected. </param>
        /// <param name="y"> The y coordinate of the block that is affected. </param>
        /// <param name="z"> The z coordinate of the block that is affected. </param>
        private void deleteBlock(BlockId b, BlockId type, ushort x, ushort y, ushort z) {
            Random rand = new Random();
            int mx, mz;

            // If delete mode is on, the affected block always turns to air except in one case.
            if (deleteMode && b != BlockId.C4Detonator) {
                level.Blockchange(this, x, y, z, BlockId.Air);
                return;
            }

            // If the block deleted is a tDoor, send the original block back to the client.
            if (BlockData.tDoor(b)) {
                SendBlockchange(x, y, z, b);
                return;
            }

            // If the block is a door...
            if (BlockData.DoorAirs(b) != 0) {
                if (level.physics != 0) { // If physics is not zero, send the air_door.
                    level.Blockchange(x, y, z, BlockData.DoorAirs(b));
                }
                else { // Otherwise, change the block to what the player placed.
                    SendBlockchange(x, y, z, b);
                }
                return;
            }

            // If the block is an oDoor...
            if (BlockData.odoor(b) != BlockId.Null) {
                // If it is odoor_green or its air counterpart...
                if (b == BlockId.ODoorGreen || b == BlockId.ODoorGreenActive) {
                    level.Blockchange(this, x, y, z, BlockData.odoor(b));
                }
                else { // Otherwise, change the block to what the player placed.
                    SendBlockchange(x, y, z, b);
                }
                return;
            }

            switch (b) {
                // Don't do anything with door_air blocks.
                case BlockId.DoorTreeWoodActive:
                case BlockId.DoorObsidianActive:
                case BlockId.DoorGlassActive:
                case BlockId.DoorStoneActive:
                case BlockId.DoorLeavesActive:
                case BlockId.DoorSandActive:
                case BlockId.DoorWoodPlanksActive:
                case BlockId.DoorGreenActive:
                case BlockId.DoorTntActive:
                case BlockId.DoorSlabActive:
                case BlockId.DoorIronActive:
                case BlockId.DoorGoldActive:
                case BlockId.DoorCobblestoneActive:
                case BlockId.DoorRedActive:

                case BlockId.DoorOrangeActive:
                case BlockId.DoorYellowActive:
                case BlockId.DoorLimeActive:
                case BlockId.DoorAquaGreenActive:
                case BlockId.DoorCyanActive:
                case BlockId.DoorBlueActive:
                case BlockId.DoorIndigoActive:
                case BlockId.DoorVioletActive:
                case BlockId.DoorMagentaActive:
                case BlockId.DoorPinkActive:
                case BlockId.DoorBlackActive:
                case BlockId.DoorGrayActive:
                case BlockId.DoorWhiteActive:

                case BlockId.DoorDirtActive:
                case BlockId.DoorGrassActive:
                case BlockId.DoorPurpleActive:
                case BlockId.DoorBookshelfActive:
                    break;
                case BlockId.RocketStart:
                    if (level.physics < 2 || level.physics == 5) { // Physics must be 3+ for rockets
                        SendBlockchange(x, y, z, b);
                    }
                    else {
                        int newZ = 0, newX = 0, newY = 0;

                        SendBlockchange(x, y, z, BlockId.RocketStart);
                        // Determine the trajectory of the rocket based on the player's position.
                        if (rot[0] < 48 || rot[0] > (256 - 48)) {
                            newZ = -1;
                        }
                        else if (rot[0] > (128 - 48) && rot[0] < (128 + 48)) {
                            newZ = 1;
                        }

                        if (rot[0] > (64 - 48) && rot[0] < (64 + 48)) {
                            newX = 1;
                        }
                        else if (rot[0] > (192 - 48) && rot[0] < (192 + 48)) {
                            newX = -1;
                        }

                        if (rot[1] >= 192 && rot[1] <= (192 + 32)) {
                            newY = 1;
                        }
                        else if (rot[1] <= 64 && rot[1] >= 32) {
                            newY = -1;
                        }

                        if (192 <= rot[1] && rot[1] <= 196 || 60 <= rot[1] && rot[1] <= 64) {
                            newX = 0;
                            newZ = 0;
                        }

                        BlockId b1 = level.GetTile((ushort)(x + newX * 2), (ushort)(y + newY * 2), (ushort)(z + newZ * 2));
                        BlockId b2 = level.GetTile((ushort)(x + newX), (ushort)(y + newY), (ushort)(z + newZ));
                        // If the blocks where the rocket would spawn are air and have no physics
                        // activity going on, spawn the rocket.
                        if (b1 == BlockId.Air && b2 == BlockId.Air
                                && level.CheckClear((ushort)(x + newX * 2), (ushort)(y + newY * 2), (ushort)(z + newZ * 2))
                                && level.CheckClear((ushort)(x + newX), (ushort)(y + newY), (ushort)(z + newZ))) {
                            level.Blockchange((ushort)(x + newX * 2), (ushort)(y + newY * 2), (ushort)(z + newZ * 2), BlockId.RocketHead);
                            level.Blockchange((ushort)(x + newX), (ushort)(y + newY), (ushort)(z + newZ), BlockId.Embers);
                        }
                    }
                    break;
                case BlockId.Firework: // Fireworks-specific stuff
                    if (level.physics == 5) {
                        SendBlockchange(x, y, z, b);
                        return;
                    }
                    if (level.physics != 0) {
                        // The place where the firework spawns from is within a 3x3 area
                        mx = rand.Next(0, 2);
                        mz = rand.Next(0, 2);
                        BlockId b1 = level.GetTile((ushort)(x + mx - 1), (ushort)(y + 2), (ushort)(z + mz - 1));
                        BlockId b2 = level.GetTile((ushort)(x + mx - 1), (ushort)(y + 1), (ushort)(z + mz - 1));
                        // If the blocks where the firework would spawn are air and have no physics
                        // activity going on, spawn the firework.
                        if (b1 == BlockId.Air && b2 == BlockId.Air && level.CheckClear((ushort)(x + mx - 1), (ushort)(y + 2), (ushort)(z + mz - 1))
                                && level.CheckClear((ushort)(x + mx - 1), (ushort)(y + 1), (ushort)(z + mz - 1))) {
                            level.Blockchange((ushort)(x + mx - 1), (ushort)(y + 2), (ushort)(z + mz - 1), BlockId.Firework);
                            level.Blockchange((ushort)(x + mx - 1), (ushort)(y + 1), (ushort)(z + mz - 1), BlockId.StillLava, false,
                                              "wait 1 dissipate 100");
                        }
                    }
                    SendBlockchange(x, y, z, b);

                    break;

                case BlockId.C4Detonator: // C4 crap
                    C4.BlowUp(new ushort[] { x, y, z }, level);
                    level.Blockchange(x, y, z, BlockId.Air);
                    break;

                default: // The normal case: every other destroyed block turns into air
                    level.Blockchange(this, x, y, z, (ushort)BlockId.Air);
                    break;
            }
            // This turns dirt below the block that was destroyed into grass.
            if ((level.physics == 0 || level.physics == 5) && level.GetTile(x, (ushort)(y - 1), z) == BlockId.Dirt) {
                level.Blockchange(this, x, (ushort)(y - 1), z, BlockId.Grass);
            }
        }

        /// <summary>
        /// Creates a block.
        /// </summary>
        /// <param name="b"> The block type to be replaced. <seealso cref="BlockId"/></param>
        /// <param name="type"> The block type to place. <seealso cref="BlockId"/></param>
        /// <param name="x"> The x coordinate of the block that is affected. </param>
        /// <param name="y"> The y coordinate of the block that is affected. </param>
        /// <param name="z"> The z coordinate of the block that is affected. </param>
        public void placeBlock(BlockId b, BlockId type, ushort x, ushort y, ushort z) {
            // Prevent a player from placing blocks on active oDoors.
            if (BlockData.odoor(b) != BlockId.Null) {
                SendMessage("oDoor here!");
                return;
            }

            // Do different things based on the arbitrary blockAction variable.
            switch (blockAction) {
                case 0: // Normal
                    if (level.physics == 0 || level.physics == 5) {
                        switch (type) {
                        // Changes placed dirt to grass if light can reach it.
                        case BlockId.Dirt:
                            if (BlockData.LightPass(level.GetTile(x, (ushort)(y + 1), z))) {
                                level.Blockchange(this, x, y, z, BlockId.Grass);
                            }
                            else {
                                level.Blockchange(this, x, y, z, BlockId.Dirt);
                            }
                            break;
                        // Stack stairs if possible.
                        case BlockId.Slab:
                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Slab) {
                                // Only send the air block that results to the user.
                                SendBlockchange(x, y, z, BlockId.Air);

                                // Change the single stair to a double stair for everyone.
                                level.Blockchange(this, x, (ushort)(y - 1), z, BlockId.DoubleSlab);
                                break;
                            }
                            //else
                            level.Blockchange(this, x, y, z, type);
                            break;
                        default: // Otherwise, just place the block.
                            level.Blockchange(this, x, y, z, type);
                            break;
                        }
                    }
                    else {
                        level.Blockchange(this, x, y, z, type);
                    }
                    break;
                case 6: // When /mode is on
                    if (b == modeType) {
                        SendBlockchange(x, y, z, b);
                        return;
                    }
                    level.Blockchange(this, x, y, z, modeType);
                    break;
                case 13: // Small TNT
                    level.Blockchange(this, x, y, z, BlockId.SmallTnt);
                    break;
                case 14: // Big TNT
                    level.Blockchange(this, x, y, z, BlockId.BigTnt);
                    break;
                case 15: // Nuke TNT
                    level.Blockchange(this, x, y, z, BlockId.NukeTnt);
                    break;
                default: // The user broke something!
                    _logger.Log(name + " is breaking something");
                    blockAction = 0;
                    break;
            }
        }

        /// <summary>
        /// Parses the packet that contains position and orientation info.
        /// </summary>
        /// <param name="m"> The object representing the packet. </param>
        void HandleInput(object m) {
            // Ignore input if the player is not logged in, is on a train, is being followed, or is frozen.
            if (!loggedIn || trainGrab || following != String.Empty || frozen) {
                return;
            }

            // Most pointless typecast ever.
            byte[] message = (byte[])m;

            // Flip the bits before parsing the coordinates.
            ushort x = NetworkUtil.NetworkToHostOrder(message, 1);
            ushort y = NetworkUtil.NetworkToHostOrder(message, 3);
            ushort z = NetworkUtil.NetworkToHostOrder(message, 5);

            // Update position and rotation information for the player.
            byte rotx = message[7];
            byte roty = message[8];
            pos = new ushort[3] { x, y, z };
            rot = new byte[2] { rotx, roty };
        }

        /// <summary>
        /// Checks if the player is eligible to die by a falling death or drowning.
        /// </summary>
        /// <param name="x"> The current x block position of the player. </param>
        /// <param name="y"> The current y block position of the player. </param>
        /// <param name="z"> The current z block position of the player. </param>
        public void RealDeath(ushort x, ushort y, ushort z) {
            // Get the two blocks that the player occupies.
            BlockId b = level.GetTile(x, (ushort)(y - 2), z);
            BlockId b1 = level.GetTile(x, y, z);

            // This handles falling damage.
            if (oldBlock != (ushort)(x + y + z)) {
                // Increment the counter if the player is falling.
                if (BlockData.Convert(b) == BlockId.Air) {
                    deathCount++;
                    deathblock = BlockId.Air;
                    return;
                }
                else {
                    // If the player stopped falling and the player fell far enough, kill the player.
                    if (deathCount > level.fall && deathblock == BlockId.Air) {
                        HandleDeath(deathblock);
                        deathCount = 0;
                    }
                    // The player may still be drowning instead, so don't reset the counter in that case.
                    else if (deathblock != BlockId.ActiveWater) {
                        deathCount = 0;
                    }
                }
            }

            // This handles the blocks that can cause the player to drown.
            switch (BlockData.Convert(b1)) {
                case BlockId.ActiveWater:
                case BlockId.StillWater:
                case BlockId.ActiveLava:
                case BlockId.StillLava:
                    deathCount++;
                    deathblock = BlockId.ActiveWater;
                    if (deathCount > level.drown * 200) {  // 200 = 0.6 seconds
                        HandleDeath(deathblock);
                        deathCount = 0;
                    }
                    break;
                default:
                    deathCount = 0;
                    break;
                }
        }


        /// <summary>
        /// Checks if a player is standing on a special block, and calls the corresponding
        /// methods. The coordinates given must be block coordinates, not the fixed-point
        /// coordinates transmitted from clients.
        /// </summary>
        /// <param name="x"> The x coordinate of the block's position. </param>
        /// <param name="y"> The y coordinate of the block's position. </param>
        /// <param name="z"> The z coordinate of the block's position. </param>
        public void CheckBlock(ushort x, ushort y, ushort z) {
            // Get the two blocks that the player occupies.
            BlockId b = this.level.GetTile(x, y, z);
            BlockId b1 = this.level.GetTile(x, (ushort)((int)y - 1), z);

            // If a player moves onto a special block, activate its function.
            if (BlockData.Mover(b) || BlockData.Mover(b1)) {
                // This is the air_switch effect.
                if (BlockData.DoorAirs(b) != 0) {
                    level.Blockchange(x, y, z, BlockData.DoorAirs(b));
                }
                if (BlockData.DoorAirs(b1) != 0) {
                    level.Blockchange(x, (ushort)(y - 1), z, BlockData.DoorAirs(b1));
                }

                // This if condition is meant to make sure that while a player is on the same
                // message block, the message will appear only once. Otherwise, they could be
                // spammed with the same message every tenth of a second.
                if ((x + y + z) != oldBlock) {
                    // Check if the player is standing in a walkover portal.
                    if (b == BlockId.AirPortal || b == BlockId.WaterPortal || b == BlockId.LavaPortal) {
                        HandlePortal(this, x, y, z, b);
                    }
                    else if (b1 == BlockId.AirPortal || b1 == BlockId.WaterPortal || b1 == BlockId.LavaPortal) {
                        HandlePortal(this, x, (ushort)((int)y - 1), z, b1);
                    }
                    // Check if the player is standing in a walkover message block.
                    if (b == BlockId.AirMessage || b == BlockId.WaterMessage || b == BlockId.LavaMessage) {
                        HandleMsgBlock(this, x, y, z, b);
                    }
                    else if (b1 == BlockId.AirMessage || b1 == BlockId.WaterMessage || b1 == BlockId.LavaMessage) {
                        HandleMsgBlock(this, x, (ushort)((int)y - 1), z, b1);
                    }
                }
            }

            // Check if the player is standing in something that kills them.
            if (BlockData.Death(b)) {
                HandleDeath(b);
            }
            else if (BlockData.Death(b1)) {
                HandleDeath(b1);
            }
        }

        /// <summary>
        /// Causes the actual death of the player.
        /// </summary>
        /// <param name="b"> The block that caused the death. </param>
        /// <param name="customMessage"> A custom death message. Defaults to an empty string. </param>
        /// <param name="explode"> Whether the death is explosive in nature. Defaults to false. </param>
        public void HandleDeath(BlockId b, string customMessage = "", bool explode = false) {
            // Get the truncated floating point coordinates of the player.
            ushort x = (ushort)(pos[0] / (ushort)32);
            ushort y = (ushort)(pos[1] / 32);
            ushort z = (ushort)(pos[2] / 32);

            // If two seconds have passed since the last death and the player did not
            // use /invincible, kill the player with the correct message.
            if (lastDeath.AddSeconds(2) < DateTime.Now) {
                if (level.Killer && !invincible) {
                    switch (b) {
                    case BlockId.TntExplosion:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " &cblew into pieces.", false);
                        break;
                    case BlockId.NerveGas:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor +
                                        " walked into &cnerve gas and suffocated.", false);
                        break;
                    case BlockId.ColdWater:
                    case BlockId.ActiveColdWater:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor +
                                        " stepped in &dcold water and froze.", false);
                        break;
                    case BlockId.HotLava:
                    case BlockId.ActiveHotLava:
                    case BlockId.FastHotLava:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " stood in &cmagma and melted.",
                                        false);
                        break;
                    case BlockId.ActiveMagma:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor +
                                        " was hit by &cflowing magma and melted.", false);
                        break;
                    case BlockId.Geyser:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor +
                                        " was hit by &cboiling water and melted.", false);
                        break;
                    case BlockId.BirdKill:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was hit by a &cphoenix and burnt.",
                                        false);
                        break;
                    case BlockId.Train:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was hit by a &ctrain.", false);
                        break;
                    case BlockId.FishShark:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was eaten by a &cshark.", false);
                        break;
                    case BlockId.Embers:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " burnt to a &ccrisp.", false);
                        break;
                    case BlockId.RocketHead:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was &cin a fiery explosion.",
                                        false);
                        level.MakeExplosion(x, y, z, 0);
                        break;
                    case BlockId.ZombieBody:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " died due to lack of &5brain.",
                                        false);
                        break;
                    case BlockId.Creeper:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was killed &cb-SSSSSSSSSSSSSS",
                                        false);
                        level.MakeExplosion(x, y, z, 1);
                        break;
                    case BlockId.Air:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " hit the floor &chard.", false);
                        break;
                    case BlockId.ActiveWater:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " &cdrowned.", false);
                        break;
                    case BlockId.Null:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was &cterminated", false);
                        break;
                    case BlockId.FishLavaShark:
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " was eaten by a ... LAVA SHARK?!",
                                        false);
                        break;
                    case BlockId.Stone:
                        if (explode) {
                            level.MakeExplosion(x, y, z, 1);
                        }
                        _server.GlobalChat(this, this.color + this.prefix + this.name + _server.props.DefaultColor + customMessage, false);
                        break;
                    case BlockId.Cobblestone:
                        if (explode) {
                            level.MakeExplosion(x, y, z, 1);
                        }
                        _server.GlobalChatLevel(this, this.color + this.prefix + this.name + _server.props.DefaultColor + customMessage, false);
                        break;
                    }

                    // Respawn the player.
                    _server.commands.FindCommand("spawn").Use(this, String.Empty);
                    overallDeath++;

                    // Display the death count if the option is on.
                    if (_server.props.deathcount) {
                        if (overallDeath > 0 && overallDeath % 10 == 0) {
                            _server.GlobalChat(this, this.color + this.prefix + this.name + _server.props.DefaultColor + " has died &3" + overallDeath + " times",
                                       false);
                        }
                    }
                }
                // Update this so that the player doesn't get spawnkilled.
                lastDeath = DateTime.Now;

            }
        }

        /// <summary>
        /// Parses the message packet that is sent by the client.
        /// </summary>
        /// <param name="message"> The array of bytes that represent the message packet. 
        /// </param>
        void HandleChat(byte[] message) {
            try {
                // Don't accept message packets from those that haven't even logged in.
                if (!loggedIn) {
                    return;
                }

                // Translate the bits of the player's chat message to actual characters.
                string text = enc.GetString(message, 1, 64).Trim();

                if (storedMessage != String.Empty) {
                    // If the packet contains the last part of a multi-part message, set it up
                    // so that the entire message is sent.
                    if (!text.EndsWith(">") && !text.EndsWith("<")) {
                        text = storedMessage.Replace("|>|", " ").Replace("|<|", String.Empty) + text;
                        storedMessage = String.Empty;
                    }
                }

                // Do not accept text that starts with > or <, for some reason.
                if (text.StartsWith(">") || text.StartsWith("<")) {
                    return;
                }
                // If the player wants to add on to his message later, don't do anything else.
                if (text.EndsWith(">")) {
                    storedMessage += text.Replace(">", "|>|");
                    SendMessage(Colors.teal + "Partial message: " + Colors.white + storedMessage.Replace("|>|", " ").Replace("|<|", String.Empty));
                    return;
                }
                // Same case as above, but don't add a space to it.
                if (text.EndsWith("<")) {
                    storedMessage += text.Replace("<", "|<|");
                    SendMessage(Colors.teal + "Partial message: " + Colors.white + storedMessage.Replace("|<|", String.Empty).Replace("|>|", " "));
                    return;
                }
                // Do not tolerate illegal characters.
                text = Regex.Replace(text, @"\s\s+", " ");
                if (text.Any(ch => ch < 32 || ch >= 127 || ch == '&')) {
                    Kick("Illegal character in chat message!");
                    return;
                }
                // If the client actually didn't send anything, don't do anything.
                if (text.Length == 0) {
                    return;
                }

                // The player is obviously not away from his keyboard if he sent a message.
                afkCount = 0;

                // Prevent "no longer AFK; is AFK" incidents.
                if (text != "/afk") {
                    if (_server.afkset.Contains(this.name)) {
                        _server.afkset.Remove(this.name);
                        _server.GlobalMessage("-" + this.color + this.name + _server.props.DefaultColor + "- is no longer AFK");
                        _server.IRC.Say(this.name + " is no longer AFK");
                    }
                }

                // Allow a slash to begin a message if it preceded by another slash.
                if (text.StartsWith("//")) {
                    text = text.Remove(0, 1);
                    goto hello;
                }

                // Make a single slash an alias for /repeat.
                if (text == "/") {
                    HandleCommand("repeat", String.Empty);
                    return;
                }

                // Handle any commands that have been sent.
                if (text[0] == '/' || text[0] == '!') {
                    text = text.Remove(0, 1);

                    // If there is no space, assume that there are no arguments.
                    int pos = text.IndexOf(' ');
                    if (pos == -1) {
                        HandleCommand(text.ToLower(), String.Empty);
                        return;
                    }
                    // Otherwise, split the command and any provided arguments into two parts.
                    string cmd = text.Substring(0, pos).ToLower();
                    string msg = text.Substring(pos + 1);
                    HandleCommand(cmd, msg);
                    return;
                }
                hello:
                // People who are muted can't speak or vote.
                if (muted) {
                    this.SendMessage("You are muted.");
                    return;
                }

                // Record votes for votekicks.
                if (_server.voteKickInProgress && text.Length == 1) {
                    if (text.ToLower() == "y") {
                        this.voteKickChoice = VoteKickChoice.Yes;
                        SendMessage("Thanks for voting!");
                        return;
                    }
                    if (text.ToLower() == "n") {
                        this.voteKickChoice = VoteKickChoice.No;
                        SendMessage("Thanks for voting!");
                        return;
                    }
                }

                // This is after vote collection so that people can vote even when the chat is
                // being moderated. Anyway, if chat moderation is on, only allow voiced players
                // to speak.
                if (_server.chatmod && !this.voice) {
                    this.SendMessage("Chat moderation is on, you cannot speak.");
                    return;
                }

                // Filter out bad words.
                if (_server.props.profanityFilter == true) {
                    _server.profanityFilter.Parse(text);
                }

                // Check if the player is spamming chat by sending too many messages in a row.
                // If so, mute the player.
                if (_server.props.checkspam == true) {
                    if (Player.lastMSG == this.name) {
                        consecutivemessages++;
                    }
                    else {
                        consecutivemessages--;
                    }

                    if (this.consecutivemessages >= _server.props.spamcounter) {
                        int total = _server.props.mutespamtime;
                        _server.commands.FindCommand("mute").Use(_server.ServerConsole, this.name);
                        _server.GlobalMessage(this.name + " has been &0muted &efor spamming!");
                        muteTimer.Elapsed += delegate {
                            total--;
                            if (total <= 0) {
                                muteTimer.Stop();
                                if (this.muted == true) {
                                    _server.commands.FindCommand("mute").Use(_server.ServerConsole, this.name);
                                }
                                this.consecutivemessages = 0;
                                this.SendMessage("Remember, no &cspamming &e" + "next time!");
                            }
                        };
                        muteTimer.Start();
                        return;
                    }
                }
                // Keep track of who said the most recent message.
                Player.lastMSG = this.name;

                // Whisper to the console if the message begins with @@.
                if (text.Length >= 2 && text[0] == '@' && text[1] == '@') {
                    text = text.Remove(0, 2);
                    if (text.Length < 1) {
                        SendMessage("No message entered");
                        return;
                    }
                    SendMessage(_server.props.DefaultColor + "[<] Console: &f" + text);
                    _logger.Log("[>] " + this.name + ": " + text);
                    return;
                }

                // Whisper to another player if the message begins with @ or /whisper is on.
                if (text[0] == '@' || whisper) {
                    string newtext = text;
                    if (text[0] == '@') {
                        newtext = text.Remove(0, 1).Trim();
                    }

                    // If /whisper is not on, the name immediately after the @ is the player to
                    // send the private message to.
                    if (whisperTo == String.Empty) {
                        int pos = newtext.IndexOf(' ');
                        if (pos != -1) {
                            string to = newtext.Substring(0, pos);
                            string msg = newtext.Substring(pos + 1);
                            HandleQuery(to, msg);
                            return;
                        }
                        else {
                            SendMessage("No message entered");
                            return;
                        }
                    }
                    // Otherwise, /whisper is being used, so we know who to send the message to.
                    else {
                        HandleQuery(whisperTo, newtext);
                        return;
                    }
                }

                // Send the message to opchat if the message starts with # or if /opchat is on.
                if (text[0] == '#' || opchat) {
                    string newtext = text;
                    if (text[0] == '#') {
                        newtext = text.Remove(0, 1).Trim();
                    }

                    _server.GlobalMessageOps("To Ops &f-" + color + name + "&f- " + newtext);
                    if (rank.Permission < _server.props.opchatperm) {
                        SendMessage("To Ops &f-" + color + name + "&f- " + newtext);
                    }
                    _logger.Log("(OPs): " + name + ": " + newtext);
                    _server.IRC.Say(name + ": " + newtext, true);
                    return;
                }

                // Send the message to adminchat if the message starts with + or if /adminchat is on.
                if (text[0] == '+' || adminchat) {
                    string newtext = text;
                    if (text[0] == '+') {
                        newtext = text.Remove(0, 1).Trim();
                    }

                    _server.GlobalMessageAdmins("To Admins &f-" + color + name + "&f- " + newtext); //to make it easy on remote
                    if (rank.Permission < _server.props.adminchatperm) {
                        SendMessage("To Admins &f-" + color + name + "&f- " + newtext);
                    }
                    _logger.Log("(Admins): " + name + ": " + newtext);
                    _server.IRC.Say(name + ": " + newtext, true);
                    return;
                }

                // Joker stuff.
                if (this.joker) {
                    string jokerPath = Path.Combine("text", "joker.txt");

                    // So we open the file every time a joker says something? Seriously? -Jjp137
                    if (File.Exists(jokerPath)) {
                        _logger.Log("<JOKER>: " + this.name + ": " + text);
                        _server.GlobalMessageOps(_server.props.DefaultColor + "<&aJ&bO&cK&5E&9R" + _server.props.DefaultColor + ">: " + this.color + this.name
                                                + ":&f " + text);
                        FileInfo jokertxt = new FileInfo(jokerPath);
                        StreamReader stRead = jokertxt.OpenText();
                        List<string> lines = new List<string>();
                        Random rnd = new Random();
                        int i = 0;

                        while (!(stRead.Peek() == -1)) {
                            lines.Add(stRead.ReadLine());
                        }

                        stRead.Close();
                        stRead.Dispose();

                        if (lines.Count > 0) {
                            i = rnd.Next(lines.Count);
                            text = lines[i];
                        }

                    }
                    else {
                        File.Create(jokerPath).Dispose();
                    }

                }

                // If server-wide chat is off for a particular level, send the message only to that level.
                if (!level.worldChat) {
                    _logger.Log("<" + name + ">[level] " + text);
                    _server.GlobalChatLevel(this, text, true);
                    return;
                }

                // Force the message to be sent to the whole server if server-wide chat is off, and
                // vice versa. (Does this even work? -Jjp137)
                if (text[0] == '%') {
                    string newtext = text;
                    if (!_server.props.worldChat) {
                        newtext = text.Remove(0, 1).Trim();
                        _server.GlobalChatWorld(this, newtext, true);
                    }
                    else {
                        _server.GlobalChat(this, newtext);
                    }
                    _logger.Log("<" + name + "> " + newtext);

                    // Call any methods listening to this event.
                    EventHandler<PlayerChattedEventArgs> temp = PlayerChatted;
                    if (temp != null) {
                        temp(this, new PlayerChattedEventArgs(text));
                    }
                    return;
                }

                // Allow \ to be used to escape any first character that usually mean other things,
                // such as #, +, !, etc. -Jjp137
                if (text[0] == '\\') { // It's really \ but you have to escape it.
                    text = text.Remove(0, 1);
                }

                _logger.Log("<" + name + "> " + text);

                // Call any methods listening to this event.
                EventHandler<PlayerChattedEventArgs> method = PlayerChatted;
                if (method != null) {
                    method(this, new PlayerChattedEventArgs(text));
                }

                // Handle voting cases and chat moderation.
                if (_server.voting) {
                    if ((text == "y" || text == "yes") && !this.voted) {
                        _server.YesVotes++;
                        this.SendMessage(Colors.red + "Thanks For Voting!");
                        this.voted = true;
                        return;
                    }
                    else if ((text == "n" || text == "no") && !this.voted) {
                        _server.NoVotes++;
                        this.SendMessage(Colors.red + "Thanks For Voting!");
                        this.voted = true;
                        return;
                    }
                }

                // Otherwise, treat chat normally based on the server-wide chat setting.
                if (_server.props.worldChat) {
                    _server.GlobalChat(this, text);
                }
                else {
                    _server.GlobalChatLevel(this, text, true);
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                _server.GlobalMessage("An error occurred: " + e.Message);
            }
        }

        /// <summary>
        /// Handles command input.
        /// </summary>
        /// <param name="cmd"> The command itself. </param>
        /// <param name="message"> Any arguments passed to the command. </param>
        public void HandleCommand(string cmd, string message) {
            try {
                // Admins need their passwords to be private, so don't display the password in the console.
                if (_server.props.verifyadmins) {
                    if (cmd.ToLower() == "setpass") {
                        _server.commands.FindCommand(cmd).Use(this, message);
                        _logger.LogCommand(this.name + " used /setpass");
                        return;
                    }
                    if (cmd.ToLower() == "pass") {
                        _server.commands.FindCommand(cmd).Use(this, message);
                        _logger.LogCommand(this.name + " used /pass");
                        return;
                    }
                }

                // Anything that has to do with the /rules is a special case, for some reason.
                if (_server.props.agreetorulesonentry) {
                    if (cmd.ToLower() == "agree") {
                        _server.commands.FindCommand(cmd).Use(this, String.Empty);
                        _logger.LogCommand(this.name + " used /agree");
                        return;
                    }
                    if (cmd.ToLower() == "rules") {
                        _server.commands.FindCommand(cmd).Use(this, String.Empty);
                        _logger.LogCommand(this.name + " used /rules");
                        return;
                    }
                    if (cmd.ToLower() == "disagree") {
                        _server.commands.FindCommand(cmd).Use(this, String.Empty);
                        _logger.LogCommand(this.name + " used /disagree");
                        return;
                    }
                }

                // If no command was actually entered, don't do anything.
                if (cmd == String.Empty) {
                    SendMessage("No command entered.");
                    return;
                }

                // If the player has not agreed to the rules, he or she cannot use commands.
                if (_server.props.agreetorulesonentry && !agreed) {
                    SendMessage("You must read /rules then agree to them with /agree!");
                    return;
                }

                // Jailed players can't use commands.
                if (jailed) {
                    SendMessage("You cannot use any commands while jailed.");
                    return;
                }

                // Admins that have not verified can't use commands.
                if (_server.props.verifyadmins) {
                    if (this.adminpen) {
                        this.SendMessage("&cYou must use &a/pass [Password]&c to verify!");
                        return;
                    }
                }

                // These are easter egg commands that are in MCForge.
                if (cmd.ToLower() == "pony") {
                    _server.GlobalMessage(this.color + this.name + _server.props.DefaultColor + " just so happens to be a proud brony! Everyone give " +
                                  this.color + this.name + _server.props.DefaultColor + " a brohoof!");
                    ponycount += 1;

                    return;
                }
                if (cmd.ToLower() == "rainbowdashlikescoolthings") {
                    _server.GlobalMessage("&1T&2H&3I&4S &5S&6E&7R&8V&9E&aR &bJ&cU&dS&eT &fG&0O&1T &22&30 &4P&CE&7R&DC&EE&9N&1T &5C&6O&7O&8L&9E&aR&b!");
                    rdcount += 1;

                    return;
                }

                // Commands below this comment are easter egg commands added in MCHmk. -Lego
                if (cmd.ToLower() == "pulpfiction") {
                    String[] finder = message.Split(' ');
                    Player who = null;

                    // It's an easter egg command; don't give away the fact that you can
                    // smite other people if your permission allows it :p -Jjp137
                    if (finder.Length > 0 && _server.commands.CanExecute(this, "kill")) {
                        who = _server.players.Find(finder[0]);
                    }

                    // Use it on yourself if the target player is not found or if you don't have
                    // permission to use /kill on that player.
                    if (who == null || who.rank.Permission > this.rank.Permission) {
                        who = this;
                    }

                    _server.GlobalMessage("From Ezekiel 26:17: " +
                                  "&9" + "The path of the righteous man is beset on all sides" +
                                  " with the iniquities of the selfish and the tyranny of" +
                                  " evil men. Blessed is he who in the name of charity and" +
                                  " good will shepherds the weak through the valley of darkness," +
                                  " for he is truly his brother's keeper and the finder of lost" +
                                  " children." + "&c" + " And I will strike down upon those with great vengeance" +
                                  " and with furious anger those who attempt to poison and destroy my" +
                                  " brothers. And you will know that my name is the Lord when I" + "&4" +
                                  " LAY MY VENGENCE UPON THEE!.");

                    Thread t = new Thread(delegate() {
                        PulpKill (who);
                    });
                    t.Start();
                    return;
                }

                // Check if the given command matches a shortcut.
                string foundShortcut = _server.commands.FindNameByShortcut(cmd);
                if (foundShortcut != String.Empty) {
                    cmd = foundShortcut;
                }

                // Check if the given command matches a bind set using /cmdbind.
                int bindIndex = -1;
                bool isBind = int.TryParse(cmd, out bindIndex);

                if (isBind && bindIndex >= 0 && bindIndex <= 9) {
                    if (messageBind[bindIndex] == null) {
                        SendMessage("No command is stored on /" + bindIndex.ToString());
                        return;
                    }

                    message = messageBind[bindIndex] + " " + message;
                    message = message.TrimEnd(' ');
                    cmd = cmdBind[bindIndex];
                }

                // Find the corresponding command.
                Command command = _server.commands.FindCommand(cmd);

                if (command != null) {
                    if (_server.commands.CanExecute(rank.Permission, command)) { // The player must have a high enough rank.
                        if (cmd != "repeat") {
                            lastCMD = cmd + " " + message; // For use with /repeat later.
                        }

                        // Disallow commands that cannot be used in museums.
                        if (level.name.Contains("Museum " + _server.props.DefaultColor)) {
                            if (!command.MuseumUsable) {
                                SendMessage("Cannot use this command while in a museum!");
                                return;
                            }
                        }

                        // Disallow /me if the player is a joker or is muted.
                        if (this.joker == true || this.muted == true) {
                            if (cmd.ToLower() == "me") {
                                SendMessage("Cannot use /me while muted or jokered.");
                                return;
                            }
                        }

                        // Don't reveal passwords.
                        if (cmd.ToLower() != "setpass" || cmd.ToLower() != "pass") {
                            _logger.LogCommand(name + " used /" + cmd + " " + message);
                        }

                        // Handle the result of commands in a separate thread.
                        this.commThread = new Thread(new ThreadStart(delegate {
                            try {
                                command.Use(this, message);
                            }
                            catch (Exception e) {
                                _logger.ErrorLog(e);
                                this.SendMessage("An error occured when using the command!");
                                this.SendMessage(e.GetType().ToString() + ": " + e.Message);
                            }
                        }));
                        commThread.Start();
                    }
                    else {
                        SendMessage("You are not allowed to use \"" + cmd + "\"!");
                    }
                }

                // Check if /mode is implicitly being used (example: /water).
                else if (BlockData.Ushort(cmd.ToLower()) != BlockId.Null) {
                    HandleCommand("mode", cmd.ToLower());
                }
                else {
                    bool retry = true;
#if DEBUG
                    string oldCmd = cmd;
                    string oldMsg = message;
#endif
                    // Check for any aliases.
                    switch (cmd.ToLower()) {
                    case "guest":
                        message = message + " " + cmd.ToLower();
                        cmd = "setrank";
                        break;
                    case "builder":
                        message = message + " " + cmd.ToLower();
                        cmd = "setrank";
                        break;
                    case "advbuilder":
                    case "adv":
                        message = message + " " + cmd.ToLower();
                        cmd = "setrank";
                        break;
                    case "operator":
                    case "op":
                        message = message + " " + cmd.ToLower();
                        cmd = "setrank";
                        break;
                    case "super":
                    case "superop":
                        message = message + " " + cmd.ToLower();
                        cmd = "setrank";
                        break;
                    case "cut":
                        cmd = "copy";
                        message = "cut";
                        break;
                    case "admins":
                        message = "superop";
                        cmd = "viewranks";
                        break;
                    case "ops":
                        message = "op";
                        cmd = "viewranks";
                        break;
                    case "banned":
                        message = cmd;
                        cmd = "viewranks";
                        break;

                    case "ps":
                        message = "ps " + message;
                        cmd = "map";
                        break;

                    case "bhb":
                    case "hbox":
                        cmd = "cuboid";
                        message = "hollow";
                        break;
                    case "blb":
                    case "box":
                        cmd = "cuboid";
                        break;
                    case "sphere":
                        cmd = "spheroid";
                        break;
                    case "cmdlist":
                    case "commands":
                        cmd = "help";
                        break;
                    case "cmdhelp":
                        cmd = "help";
                        break;
                    case "worlds":
                    case "mapsave":
                        cmd = "save";
                        break;
                    case "mapload":
                        cmd = "load";
                        break;
                    case "colour":
                        cmd = "color";
                        break;
                    case "materials":
                        cmd = "blocks";
                        break;

                    default:
                        retry = false;
                        break; //Unknown command, then
                    }

                    if (retry) { // If the input was actually an alias for another command, try again.
#if DEBUG
                        this._logger.Log("Alias triggered: " + cmd + " " + message);
                        this._logger.Log("User entered: /" + oldCmd + " " + oldMsg);
#endif
                        HandleCommand(cmd, message);
                    }
                    else {
                        SendMessage("Unknown command \"" + cmd + "\"!");
                    }
                }
            }
            catch (Exception e) {
                _logger.ErrorLog(e);
                SendMessage("Command failed.");
            }
        }

        /// <summary>
        /// Handles private messages.
        /// </summary>
        /// <param name="to"> The name of the player to send it to. </param>
        /// <param name="message"> The message to send. </param>
        void HandleQuery(string to, string message) {
            // Find the player to send to.
            Player p = _server.players.Find(to);

            // A player cannot whisper to himself.
            if (p == this) {
                SendMessage("Trying to talk to yourself, huh?");
                return;
            }

            // Stop if the player was not found.
            if (p == null) {
                SendMessage("Could not find player.");
                return;
            }

            // Only hidden players can whisper to other hidden players.
            if (p.hidden) {
                if (this.hidden == false) {
                    p.SendMessage("Could not find player.");
                }
            }

            // If the recipient is ignoring everyone, check if the player sending the message can bypass this.
            if (p.ignoreglobal == true) {
                // If no one can ignore ops, the player sending the private message is an op, and
                // the recipient is of lower rank than the player, send the message.
                if (_server.props.globalignoreops == false) {
                    if (this.rank.Permission >= _server.props.opchatperm) {
                        if (p.rank.Permission < this.rank.Permission) {
                            _logger.Log(name + " @" + p.name + ": " + message);
                            this.SendMessage(_server.props.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
                            p.SendMessage("&9[>] " + this.color + this.prefix + this.name + ": &f" + message);
                            return;
                        }
                    }
                }  
                // Otherwise, pretend to send the message.
                _logger.Log(name + " @" + p.name + ": " + message);
                this.SendMessage(_server.props.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
                return;
            }
            // If the player sending the private message is in the recipient's ignore list, appear
            // to send the message but don't actually send it.
            foreach (string ignored2 in p.listignored) {
                if (ignored2 == this.name) {
                    _logger.Log(name + " @" + p.name + ": " + message);
                    this.SendMessage(_server.props.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
                    return;
                }
            }

            // If the recipient is not hidden or the recipient is hidden and the sender is the
            // same rank of the recipient or higher, send the message.
            if (p != null && !p.hidden || p.hidden && this.rank.Permission >= p.rank.Permission) {
                _logger.Log(name + " @" + p.name + ": " + message);
                this.SendMessage(_server.props.DefaultColor + "[<] " + p.color + p.prefix + p.name + ": &f" + message);
                p.SendMessage("&9[>] " + this.color + this.prefix + this.name + ": &f" + message);
            }
            else { // Otherwise, do not give away the fact that the recipient is hidden.
                SendMessage("Player \"" + to + "\" doesn't exist!");
            }
        }
        #endregion
        #region == OUTGOING ==
        /// <summary>
        /// Send a packet with no other data.
        /// </summary>
        /// <param name="id"> The packet id. <seealso cref="OpCode"/></param>
        public void SendRaw(OpCode id) {
            SendRaw(id, new byte[0]);
        }

        /// <summary>
        /// Send a packet with one byte.
        /// </summary>
        /// <param name="id"> The packet id. <seealso cref="OpCode"/></param>
        /// <param name="send"> The byte to send. </param>
        public void SendRaw(OpCode id, byte send) {
            SendRaw(id, new byte[] { send });
        }

        /// <summary>
        /// Sends a raw packet to the client.
        /// </summary>
        /// <param name="id"> The packet id. <seealso cref="OpCode"/></param>
        /// <param name="send"> The information to send to the client. </param>
        public void SendRaw(OpCode id, byte[] send) {
            // Abort if the socket has been closed.
            if (socket == null || !socket.Connected) {
                return;
            }

            // Combine the packet ID and the data to be sent into one buffer.
            byte[] buffer = new byte[send.Length + 1];
            buffer[0] = (byte)id;
            for (int i = 0; i < send.Length; i++) {
                buffer[i + 1] = send[i];
            }
            try {  // Send the entire buffer.
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, delegate(IAsyncResult result) { }, BlockId.Air);
                buffer = null;
            }
            catch (SocketException e) {
                buffer = null;
                Disconnect();  // Disconnect the player if his client can't be reached.
#if DEBUG
                _logger.ErrorLog(e);
#endif
            }
        }

        /// <summary>
        /// Sends a message to a player.
        /// </summary>
        /// <param name="p"> The player to send the message to. <seealso cref="Player"/></param>
        /// <param name="type"> The type of message to send. <seealso cref="MessageType"/></param>
        /// <param name="message"> The message to send. </param>
        public static void SendMessage(Player p, MessageType type, string message) {
            if (p == null) {
                throw new ArgumentNullException("p can't be null.");
            }

            if (p.IsConsole) {
                if (!p._server.props.irc || String.IsNullOrEmpty(p._server.IRC.usedCmd)) {
                    p._server.logger.Log(message);
                }
                else {
                    p._server.IRC.Pm(p._server.IRC.usedCmd, message);
                }
                return;
            }

            p.SendMessage(type, message);
        }

        /// <summary>
        /// Sends a message that originates from this player. This message will have color codes parsed.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        public void SendMessage(string message) {
            SendMessage(this, MessageType.Chat, message);
        }


        /// <summary>
        /// Sends a message packet to a player. This is the method that actually does all the work.
        /// </summary>
        /// <param name="type"> The type of message to send. <seealso cref="MessageType"/></param>
        /// <param name="message"> The message to send. </param>
        public void SendMessage (MessageType type, string message) {
            // Silently ignore if the console is the "player" being involved here.
            if (this.IsConsole) {
                Debug.WriteLine("[Debug] SendMessage(MessageType, string, bool) shouldn't be called on the console.");
                Debug.WriteLine("[Debug] Message was: " + message);
                return;
            }

            // Prevent users from being flooded with zone-related messages if they tried to build in
            // a zone repeatedly.
            if (ZoneSpam.AddSeconds (2) > DateTime.Now && message.Contains ("This zone belongs to ")) {
                return;
            }

            message = this._server.props.DefaultColor + message;

            byte[] buffer = new byte[65]; // Holds the packet that will be sent.
            unchecked {
                buffer [0] = (byte)type;  // Holds the player id or for CPE clients, the message type.
            }

            StringBuilder sb = new StringBuilder (message);

            // Parse color codes and strip out illegal color codes.
            for (int i = 0; i < 10; i++) {
                sb.Replace ("%" + i, "&" + i);
                sb.Replace ("&" + i + " &", " &");
            }

            for (char ch = 'a'; ch <= 'f'; ch++) {
                sb.Replace ("%" + ch, "&" + ch);
                sb.Replace ("&" + ch + " &", " &");
            }

            // Handle capital letters coupled with % as color codes.
            for (char ch = 'A'; ch <= 'F'; ch++) {
                sb.Replace("%" + ch, "&" + (char)(ch+32)); // 32 places between 'A' and 'a'
                sb.Replace("&" + (char)(ch+32) + " &", " &");
            }

            // Replace all invalid color codes with nothing.
            for (char ch = (char)0; ch <= (char)47; ch++) {
                sb.Replace ("&" + ch, String.Empty);
            }
            for (char ch = (char)58; ch <= (char)96; ch++) {
                sb.Replace ("&" + ch, String.Empty);
            }
            for (char ch = (char)103; ch <= (char)127; ch++) {
                sb.Replace ("&" + ch, String.Empty);
            }

            // Replace any chat variables with the actual content.
            sb.Replace("$name", name);
            sb.Replace ("$date", DateTime.Now.ToString ("yyyy-MM-dd"));
            sb.Replace ("$time", DateTime.Now.ToString ("HH:mm:ss"));
            sb.Replace ("$ip", ip);
            sb.Replace ("$color", color);
            sb.Replace ("$rank", rank.name);
            sb.Replace ("$level", level.name);
            sb.Replace ("$deaths", overallDeath.ToString ());
            sb.Replace ("$money", money.ToString ());
            sb.Replace ("$blocks", overallBlocks.ToString ());
            sb.Replace ("$first", firstLogin.ToString ());
            sb.Replace ("$kicked", totalKicked.ToString ());
            sb.Replace ("$server", _server.props.name);
            sb.Replace ("$motd", _server.props.motd);
            sb.Replace ("$banned", _server.ranks.FindPerm(DefaultRankValue.Banned).playerList.Count.ToString());
            sb.Replace ("$irc", _server.props.ircServer + " > " + _server.props.ircChannel);

            // Handle custom chat variables.
            foreach (var customReplacement in _server.customdollars) {
                if (!customReplacement.Key.StartsWith ("//")) {
                    try {
                        sb.Replace (customReplacement.Key, customReplacement.Value);
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }
                }
            }

            // Emoticon stuff.
            if (_server.props.parseSmiley && parseSmiley) {
                sb.Replace(":)", "(darksmile)");
                sb.Replace(":D", "(smile)");
                sb.Replace("<3", "(heart)");
            }

            message = Emotes.ReplaceKeywords(sb.ToString());

            int totalTries = 0;

            retryTag:
            try {
                foreach (string line in Wordwrap(message)) {
                    string newLine = line;
                   // If it's a control character? I have no idea - Jjp137
                    if (newLine.TrimEnd(' ')[newLine.TrimEnd(' ').Length - 1] < '!') {
                        //For some reason, this did the opposite
                        if (HasExtension("EmoteFix")) {
                            newLine += '\'';
                        }
                    }
                    // Copy the characters of each line to the packet's buffer.
                    StringFormat(newLine, 64).CopyTo(buffer, 1);
                    SendRaw(OpCode.Message, buffer);
                }
            }
            catch (Exception e) { // Try again if anything goes wrong.
                message = "&f" + message;
                totalTries++;
                if (totalTries < 10) {
                    goto retryTag;
                }
                else {
                    _logger.ErrorLog(e);
                }
            }
        }

        /// <summary>
        /// Send the server's MOTD to the client.
        /// </summary>
        public void SendMotd() {
            byte[] buffer = new byte[130];
            buffer[0] = (byte)8; // Isn't the protocol version 0x07 though? Or...
            StringFormat(_server.props.name, 64).CopyTo(buffer, 1); // The server's name takes up 64 bytes.

            // Prioritize the rank-specific MOTD, if that rank has one. Otherwise send the default
            // one. Also, the MOTD takes up 64 bytes.
            if (!String.IsNullOrEmpty(rank.MOTD)) {
                StringFormat(rank.MOTD, 64).CopyTo(buffer, 65);
            }
            else {
                StringFormat(_server.props.motd, 64).CopyTo(buffer, 65);
            }

            // Set the "player is an op" flag if they can break adminium. This is needed because
            // the default client does not recognize MCHmk's ranks. If the client is not told that
            // they are an op, they would not be able to break adminium manually on their end even if the
            // server knows that they can.
            if (_server.blockPerms.CanPlace(this, BlockId.Bedrock)) {
                buffer[129] = 100;
            }
            else {
                buffer[129] = 0;
            }

            // Send the packet.
            SendRaw(0, buffer);
        }

        /// <summary>
        /// Sends level-specific MOTDs to the client.
        /// </summary>
        public void SendUserMOTD() {
            // Local variables for the packet to send.
            byte[] buffer = new byte[130];
            buffer[0] = Server.version;

            // If there is no level-specific motd, send the server's motd.
            if (level.motd == "ignore") {
                StringFormat(_server.props.name, 64).CopyTo(buffer, 1);
                // Prioritize the rank-specific motd over the server's motd.
                if (!String.IsNullOrEmpty(rank.MOTD)) {
                    StringFormat(rank.MOTD, 64).CopyTo(buffer, 65);
                }
                else {
                    StringFormat(_server.props.motd, 64).CopyTo(buffer, 65);
                }
            }
            // Otherwise, send the level's motd.
            else {
                StringFormat(level.motd, 128).CopyTo(buffer, 1);
            }

            // Set the "player is an op" flag if they can break adminium. This is needed because
            // the default client does not recognize MCHmk's ranks. If the client is not told that
            // they are an op, they would not be able to break adminium manually on their end even
            // if the server knows that they can.
            if (_server.blockPerms.CanPlace(this.rank.Permission, BlockId.Bedrock)) {
                buffer[129] = 100;
            }
            else {
                buffer[129] = 0;
            }

            // Send the packet.
            SendRaw(0, buffer);
        }

        /// <summary>
        /// Sends map data to the client.
        /// </summary>
        public void SendMap() {
            if (level.blocks == null) {
                return;
            }
            try {
                using (MemoryStream ms = new MemoryStream()) {
                    // The GZipStream is wrapped in a BufferedStream because the GZipStream doesn't buffer,
                    // so if one byte is being written at a time to an unbuffered GZipStream, it can actually
                    // increase the size of the data. Thus, the GZipStream is wrapped to prevent that.
                    using (BufferedStream gzip = new BufferedStream(new GZipStream(ms, CompressionMode.Compress, true), 8192)) {
                        // The first four bytes of the level that is sent to the client is the number of blocks
                        // the level contains.
                        gzip.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(level.blocks.Length)), 0, 4);

                        // Read from the level's block array, one block at a time, get the equivalent block
                        // id to send to the client, and write it to the stream.
                        byte b;
                        for (int i = 0; i < level.blocks.Length; ++i) {
                            if (extension) {
                                b = (byte)BlockData.Convert(level.blocks[i]);
                            }
                            else {
                                b = (byte)BlockData.Convert(BlockData.ConvertCPE(level.blocks[i]));
                            }
                            gzip.WriteByte(b);
                        }
                    }

                    // Tell the client that the server will be sending level data.
                    SendRaw(OpCode.MapBegin);

                    // Since writing to the MemoryStream advances its position, and reading from the
                    // start is desired when the level is sent, set the position to the beginning of the stream.
                    ms.Position = 0;

                    // Send the map, one chunk at a time.
                    byte[] chunk = new byte[1027];
                    while (ms.Position < ms.Length - 1) {
                        // Copy the block data. Use an offset of two since the number of blocks being
                        // sent occupy the first two bytes.
                        short length = (short)ms.Read(chunk, 2, 1024);

                        // Copy the length of the chunk and the percentage complete into the
                        // appropriate places in the array.
                        NetworkUtil.HostToNetworkOrder(length).CopyTo(chunk, 0);
                        chunk[1026] = (byte)(ms.Position * 100.0 / ms.Length);

                        // Send the packet and clear the way for the next one.
                        SendRaw(OpCode.MapChunk, chunk);
                        Array.Clear(chunk, 0, chunk.Length);
                    }
                }

                // Send the level finalize packet, which contains the level's dimensions.
                byte[] size = new byte[6];
                // The bytes need to be in network order.
                NetworkUtil.HostToNetworkOrder((short)level.width).CopyTo(size, 0);
                NetworkUtil.HostToNetworkOrder((short)level.height).CopyTo(size, 2);
                NetworkUtil.HostToNetworkOrder((short)level.depth).CopyTo(size, 4);
                SendRaw(OpCode.MapEnd, size);
                Loading = false; // And we're done!
            }
            catch (Exception ex) {
                _server.commands.FindCommand("goto").Use(this, _server.mainLevel.name);
                SendMessage("There was an error sending the map data, you have been sent to the main level.");
                _logger.ErrorLog(ex);
            }

            if (HasExtension("EnvWeatherType")) {
                SendSetMapWeather(level.weather);
            }
        }

        /// <summary>
        /// Sends a packet that indicates where a player is spawning.
        /// </summary>
        /// <param name="id"> The ID of the player being spawned. </param>
        /// <param name="name"> The name of the player being spawned. </param>
        /// <param name="x"> The x-coordinate of the position that the player is spawning at. </param>
        /// <param name="y"> The y-coordinate of the position that the player is spawning at. </param>
        /// <param name="z"> The z-coordinate of the position that the player is spawning at. </param>
        /// <param name="rotx"> The angle on the x-axis of the player's head when spawning. </param>
        /// <param name="roty"> The angle on the y-axis of the player's head when spawning. </param>
        public void SendSpawn(byte id, string name, ushort x, ushort y, ushort z, byte rotx, byte roty) {
            // Do not include the + sign that CPE clients have since that isn't a part of their real name.
            name = name.TrimEnd('+');

            // Create the buffer.
            byte[] buffer = new byte[73];

            // The first byte is the player ID.
            buffer[0] = id;

            // The next 64 bytes consist of the player's name.
            StringFormat(name, 64).CopyTo(buffer, 1);

            // The final few bytes represents the player's spawn position. The bytes need to be
            // put in network order.
            NetworkUtil.HostToNetworkOrder(x).CopyTo(buffer, 65);
            NetworkUtil.HostToNetworkOrder(y).CopyTo(buffer, 67);
            NetworkUtil.HostToNetworkOrder(z).CopyTo(buffer, 69);
            buffer[71] = rotx;
            buffer[72] = roty;

            // Send the result.
            SendRaw(OpCode.AddEntity, buffer);
        }

        /// <summary>
        /// Sends a packet telling the player's client that the player's position and orientation
        /// changed, most likely due to a player teleport.
        /// </summary>
        /// <param name="id"> The player ID. Teleports the player if the ID is less than 0. </param>
        /// <param name="x"> The fixed-point x coordinate of the player. </param>
        /// <param name="y"> The fixed-point y coordinate of the player. </param>
        /// <param name="z"> The fixed-point z coordinate of the player. </param>
        /// <param name="rotx"> The angle on the x-axis that the head is facing. </param>
        /// <param name="roty"> The angle on the y-axis that the head is facing. </param>
        public void SendPos(byte id, ushort x, ushort y, ushort z, byte rotx, byte roty) {
            // Check if the position is within the level.
            if (x < 0) {
                x = 32;
            }
            if (y < 0) {
                y = 32;
            }
            if (z < 0) {
                z = 32;
            }
            if (x > level.width * 32) {
                x = (ushort)(level.width * 32 - 32);
            }
            if (z > level.depth * 32) {
                z = (ushort)(level.depth * 32 - 32);
            }
            if (x > 32767) {
                x = 32730;
            }
            if (y > 32767) {
                y = 32730;
            }
            if (z > 32767) {
                z = 32730;
            }

            // Updates the player's server-side coordinates.
            pos[0] = x;
            pos[1] = y;
            pos[2] = z;
            rot[0] = rotx;
            rot[1] = roty;

            // Create the packet.
            byte[] buffer = new byte[9];
            buffer[0] = id;

            // Swap the positional bytes so that they are in network order.
            NetworkUtil.HostToNetworkOrder(x).CopyTo(buffer, 1);
            NetworkUtil.HostToNetworkOrder(y).CopyTo(buffer, 3);
            NetworkUtil.HostToNetworkOrder(z).CopyTo(buffer, 5);
            buffer[7] = rotx;
            buffer[8] = roty;

            // Send the packet.
            SendRaw(OpCode.Teleport, buffer);
        }

        /// <summary>
        /// Sends a packet that sets the client's op flag.
        /// </summary>
        /// <param name="op"> Whether the client should be told that the user is an op. </param>
        public void SendUserType(bool op) {
            SendRaw(OpCode.SetPermission, op ? (byte)100 : (byte)0);
        }


        //TODO: Figure a way to SendPos without changing rotation
        /// <summary>
        /// Sends a packet telling the client that a player has despawned.
        /// </summary>
        /// <param name="id"> The id of the player that is despawning.
        /// </param>
        public void SendDie(byte id) {
            SendRaw(OpCode.RemoveEntity, new byte[1] { id });
        }

        /// <summary>
        /// Sends a packet that tells the client that a block has changed.
        /// </summary>
        /// <param name="x"> The x-coordinate of the block. </param>
        /// <param name="y"> The y-coordinate of the block. </param>
        /// <param name="z"> The z-coordinate of the block. </param>
        /// <param name="type"> The new block type. <seealso cref="BlockId"/></param>
        public void SendBlockchange(ushort x, ushort y, ushort z, BlockId type) {
            // Check if the coordinates are within the level and also handle some odd cases.
            if (type == BlockId.Air) {
                type = 0;
            }
            if (x < 0 || y < 0 || z < 0) {
                return;
            }
            if (Convert.ToInt32(type) > BlockData.maxblocks) {
                this.SendMessage("The server was not able to detect your held block, please try again!");
                return;
            }
            if (x >= level.width || y >= level.height || z >= level.depth) {
                return;
            }

            // Create the byte array to be sent.
            byte[] buffer = new byte[7];

            // Flip the positional bytes before sending them.
            NetworkUtil.HostToNetworkOrder(x).CopyTo(buffer, 0);
            NetworkUtil.HostToNetworkOrder(y).CopyTo(buffer, 2);
            NetworkUtil.HostToNetworkOrder(z).CopyTo(buffer, 4);

            // Convert the MCHmk block ids to those expected by the client. If the client
            // is not a CPE client, use the fallback block ids as well.
            if (extension == true) {
                buffer[6] = (byte)BlockData.Convert(type);
            }
            else {
                buffer[6] = (byte)BlockData.Convert(BlockData.ConvertCPE(type));
            }

            // 0x06 = block change packet
            SendRaw(OpCode.SetBlockServer, buffer);
        }

        /// <summary>
        /// Sends a packet telling the client that the player has been kicked/disconnected.
        /// </summary>
        /// <param name="message"> The kick message. </param>
        void SendKick(string message) {
            SendRaw(OpCode.Kick, StringFormat(message, 64));
        }

        /// <summary>
        /// Sends a ping packet to the client.
        /// </summary>
        void SendPing() {
            SendRaw(OpCode.Ping);
        }

        /// <summary>
        /// Sends an ExtInfo packet to the client.
        /// </summary>
        /// <param name="count"> The number of CPE extensions that the server supports. </param>
        public void SendExtInfo(short count) {
            byte[] buffer = new byte[66];
            // The string representing the server name is for logging purposes only.
            StringFormat("MCHmk Version: " + _server.Version, 64).CopyTo(buffer, 0);
            // Flip the bytes representing the number of extensions that the server supports.
            NetworkUtil.HostToNetworkOrder(count).CopyTo(buffer, 64);
            // Send the packet.
            SendRaw(OpCode.ExtInfo, buffer);
        }

        /// <summary>
        /// Sends an ExtEntry packet to the client.
        /// </summary>
        /// <param name="name"> The name of the CPE extension. </param>
        /// <param name="version"> The version of the extension that the server supports. </param>
        public void SendExtEntry(string name, int version) {
            // Flip the bytes representing the version number.
            byte[] version_ = BitConverter.GetBytes(version);
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(version_);
            }
            byte[] buffer = new byte[68];
            // Copy the string and version number to the buffer.
            StringFormat(name, 64).CopyTo(buffer, 0);
            version_.CopyTo(buffer, 64);
            // Send the packet.
            SendRaw(OpCode.ExtEntry, buffer);
        }

        /// <summary>
        /// Sends a SetClickDistance packet to the client.
        /// </summary>
        /// <param name="distance"> The maximum distance that a user can click a block from. </param>
        public void SendClickDistance(short distance) {
            byte[] buffer = new byte[2];
            // Flip the bytes representing the maximum distance before sending the packet.
            NetworkUtil.HostToNetworkOrder(distance).CopyTo(buffer, 0);
            SendRaw(OpCode.SetClickDistance, buffer);
        }

        /// <summary>
        /// Sends a CustomBlockSupportLevel packet to the client.
        /// </summary>
        /// <param name="level"> The highest tier of custom blocks that the server supports.
        /// </param>
        public void SendCustomBlockSupportLevel(byte level) {
            byte[] buffer = new byte[1];
            buffer[0] = level;
            SendRaw(OpCode.CustomBlockSupportLevel, buffer);
        }

        /// <summary>
        /// Sends a HoldThis packet to the client.
        /// </summary>
        /// <param name="type"> The id of the block that the client should hold. </param>
        /// <param name="locked"> This is 1 if the client cannot change the held block, and 0 otherwise. </param>
        public void SendHoldThis(byte type, byte locked) {
            byte[] buffer = new byte[2];
            buffer[0] = type;
            buffer[1] = locked;
            SendRaw(OpCode.HoldThis, buffer);
        }

        /// <summary>
        /// Sends a TextHotKey packet to the client.
        /// </summary>
        /// <param name="label"> A readable name for the hotkey. </param>
        /// <param name="command"> The command to automatically type in. </param>
        /// <param name="keycode"> The keycode representing the given key. </param>
        /// <param name="mods"> The value representing the additional keys that need to be held in addition to that 
        /// key. 0 = none, 1 = Ctrl, 2 = Shift, 4 = Alt. Can be combined. </param>
        public void SendTextHotKey(string label, string command, int keycode, byte mods) {
            byte[] buffer = new byte[133];
            // Copy the label and command to be used to the buffer.
            StringFormat(label, 64).CopyTo(buffer, 0);
            StringFormat(command, 64).CopyTo(buffer, 64);
            // Flip the bytes representing the keycode.
            BitConverter.GetBytes(keycode).CopyTo(buffer, 128);
            // The last byte represents the key modifier flags. After that, send the packet.
            buffer[132] = mods;
            SendRaw(OpCode.SetTextHotKey, buffer);
        }

        // TODO Use version 2 instead
        /// <summary>
        /// Sends version 1 of the ExtAddPlayerName packet to the client.
        /// </summary>
        /// <param name="id"> A unique id for this player. </param>
        /// <param name="name"> The name of the player that should appear in the player list. </param>
        /// <param name="grp"> The rank that the player belong to. <seealso cref="Rank"/></param>
        /// <param name="displayname"> The name of the player used for auto-completion. If left empty,
        /// it will be the same as the name parameter. </param>
        public void SendExtAddPlayerName(short id, string name, Rank grp, string displayname = "") {
            byte[] buffer = new byte[195];
            // Flip the bytes representing the player's id.
            NetworkUtil.HostToNetworkOrder(id).CopyTo(buffer, 0);
            // Copy the string representing the name of the player in the player list to the buffer.
            StringFormat(name, 64).CopyTo(buffer, 2);
            // By default, the given name is also used for auto-completion.
            if (displayname == String.Empty) {
                displayname = name;
            }
            // Copy the name that will be used for auto-completion purposes to the buffer.
            StringFormat(displayname, 64).CopyTo(buffer, 66);
            // Copy the name of the rank to the buffer.
            StringFormat(grp.color + grp.name.ToUpper() + "s:", 64).CopyTo(buffer, 130);
            // There is no concept of sub-ranks, so everyone in a given rank gets the same number.
            buffer[194] = (byte)grp.Permission.GetHashCode();
            // Send the packet.
            SendRaw(OpCode.ExtAddPlayerName, buffer);
        }

        // TODO Use version 2 instead
        /// <summary>
        /// Sends version 1 of the ExtAddEntity packet to the client.
        /// </summary>
        /// <param name="id"> A unique id for this player. </param>
        /// <param name="name"> The name that should be shown over the player's model. </param>
        /// <param name="displayname"> The name of the player whose skin to use. If left empty, the
        /// skin of the provided name in the previous parameter is used. </param>
        public void SendExtAddEntity(byte id, string name, string displayname = "") {
            byte[] buffer = new byte[129];
            // Copy over the player's id.
            buffer[0] = id;
            // Copy over the name that should be shown over the player's model to the buffer.
            StringFormat(name, 64).CopyTo(buffer, 1);
            // By default, use the player's own skin.
            if (displayname == String.Empty) {
                displayname = name;
            }
            // Copy the name of the player whose skin to use to the buffer.
            StringFormat(displayname, 64).CopyTo(buffer, 65);
            SendRaw(OpCode.ExtAddEntity, buffer);
        }

        // TODO Use version 2 instead
        /// <summary>
        /// Sends version 1 of the ExtRemovePlayerName packet to the client.
        /// </summary>
        /// <param name="id"> A unique id for this player. This should be the same as the id used for a previous 
        /// ExtAddEntity packet. </param>
        public void SendExtRemovePlayerName(short id) {
            byte[] buffer = new byte[2];
            // Flip the bytes representing the player's id before sending the packet.
            NetworkUtil.HostToNetworkOrder(id).CopyTo(buffer, 0);
            SendRaw(OpCode.ExtRemovePlayerName, buffer);
        }

        /// <summary>
        /// Send an EnvSetColor packet to the client.
        /// </summary>
        /// <remarks> 
        /// Possible environmental values to change: 0 = sky color, 1 = cloud color, 2 = fog color,
        /// 3 = ambient light (or shadow) color, 4 = diffuse light (sunlight) color
        /// 
        /// If -1 is used as a color value, that color component gets reset to its default value. 
        /// </remarks>
        /// <param name="type"> A number representing the environmental variable to change. </param>
        /// <param name="r"> The red component of the new color. </param>
        /// <param name="g"> The green component of the new color. </param>
        /// <param name="b"> The blue component of the new color. </param>
        public void SendEnvSetColor(byte type, short r, short g, short b) {
            byte[] buffer = new byte[7];
            buffer[0] = type;
            // Flip the bytes of the color components before sending the packet.
            NetworkUtil.HostToNetworkOrder(r).CopyTo(buffer, 1);
            NetworkUtil.HostToNetworkOrder(g).CopyTo(buffer, 3);
            NetworkUtil.HostToNetworkOrder(b).CopyTo(buffer, 5);
            SendRaw(OpCode.EnvSetColor, buffer);
        }

        /// <summary>
        /// Send a MakeSelection packet to the client.
        /// </summary>
        /// <param name="id"> A unique id for this cuboid selection. </param>
        /// <param name="label"> The label given to this cuboid selection. </param>
        /// <param name="smallx"> The x coordinate of the corner of the cuboid closest to the origin. </param>
        /// <param name="smally"> The y coordinate of the corner of the cuboid closest to the origin. </param>
        /// <param name="smallz"> The z coordinate of the corner of the cuboid closest to the origin. </param>
        /// <param name="bigx"> The x coordinate of the corner of the cuboid furthest from the origin. </param>
        /// <param name="bigy"> The y coordinate of the corner of the cuboid furthest from the origin. </param>
        /// <param name="bigz"> The z coordinate of the corner of the cuboid furthest from the origin. </param>
        /// <param name="r"> The red component of the cuboid's color. </param>
        /// <param name="g"> The green component of the cuboid's color. </param>
        /// <param name="b"> The blue component of the cuboid's color. </param>
        /// <param name="opacity"> The opacity of the cuboid. 0 means full transparency. 255 means fully opaque. </param>
        public void SendMakeSelection(byte id, string label, short smallx, short smally, short smallz, short bigx, short bigy,
                                      short bigz, short r, short g, short b, short opacity) {
            byte[] buffer = new byte[85];
            // Copy the id and label to the buffer.
            buffer[0] = id;
            StringFormat(label, 64).CopyTo(buffer, 1);
            // Flip the bytes of all the numerical paramters before sending the packet.
            NetworkUtil.HostToNetworkOrder(smallx).CopyTo(buffer, 65);
            NetworkUtil.HostToNetworkOrder(smally).CopyTo(buffer, 67);
            NetworkUtil.HostToNetworkOrder(smallz).CopyTo(buffer, 69);
            NetworkUtil.HostToNetworkOrder(bigx).CopyTo(buffer, 71);
            NetworkUtil.HostToNetworkOrder(bigy).CopyTo(buffer, 73);
            NetworkUtil.HostToNetworkOrder(bigz).CopyTo(buffer, 75);
            NetworkUtil.HostToNetworkOrder(r).CopyTo(buffer, 77);
            NetworkUtil.HostToNetworkOrder(g).CopyTo(buffer, 79);
            NetworkUtil.HostToNetworkOrder(b).CopyTo(buffer, 81);
            NetworkUtil.HostToNetworkOrder(opacity).CopyTo(buffer, 83);
            SendRaw(OpCode.MakeSelection, buffer);
        }

        /// <summary>
        /// Sends a DeleteSelection packet to the client.
        /// </summary>
        /// <param name="id"> A unique id for the cuboid selection. This id should be the same id as the one
        /// used for the MakeSelection packet.  </param>
        public void SendDeleteSelection(byte id) {
            byte[] buffer = new byte[1];
            buffer[0] = id;
            SendRaw(OpCode.RemoveSelection, buffer);
        }

        /// <summary>
        /// Sends a SetBlockPermission packet to the client.
        /// </summary>
        /// <param name="type"> The id of the block to set permissions for. If 0, the packet will apply for
        /// every block. </param>
        /// <param name="canplace"> If 0, the player cannot place this block. If 1 or higher, the player can
        /// place the block. </param>
        /// <param name="candelete"> If 0, the player cannot delete this block. If 1 or higher, the player can
        /// delete the block. </param>
        public void SendSetBlockPermission(byte type, byte canplace, byte candelete) {
            byte[] buffer = new byte[3];
            // Copy the bytes to the buffer and send the packet.
            buffer[0] = type;
            buffer[1] = canplace;
            buffer[2] = candelete;
            SendRaw(OpCode.SetBlockPermission, buffer);
        }

        /// <summary>
        /// Sends a ChangeModel packet to the client.
        /// </summary>
        /// <param name="id"> The id of the player whose model should be changed. This should be the same
        /// as the id used in a SpawnPlayer or ExtAddPlayer client. An id of -1 refers to the client the
        /// packet is being sent to. </param>
        /// <param name="model"> The name of the new model that should be used. An empty string indicates that
        /// the default model should be used. A block id converted to a string can also be used.
        /// </param>
        public void SendChangeModel(byte id, string model) {
            // Stop if the extension is not supported by the client.
            if (!HasExtension("ChangeModel")) {
                return;
            }
            byte[] buffer = new byte[65];
            // Copy the id and the model naem 
            buffer[0] = id;
            StringFormat(model, 64).CopyTo(buffer, 1);
            SendRaw(OpCode.ChangeModel, buffer);
        }

        /// <summary>
        /// Sends a EnvSetMapAppearance packet to the client.
        /// </summary>
        /// <param name="url"> The URL pointing to the textures being used by the server. </param>
        /// <param name="sideblock"> The id of the block that should be used on the surface surrounding the map.
        /// The adminium block is used by default. </param>
        /// <param name="edgeblock"> The id of the block that should be used on the layer above the surface
        /// that surrounds the map. The water block is used by default. </param>
        /// <param name="sidelevel"> The height of the outer surface of the map. </param>
        public void SendSetMapAppearance(string url, byte sideblock, byte edgeblock, short sidelevel) {
            byte[] buffer = new byte[68];
            // Copy the URL to the buffer.
            StringFormat(url, 64).CopyTo(buffer, 0);
            // Copy the block ids to the buffer.
            buffer[64] = sideblock;
            buffer[65] = edgeblock;
            // Flip the bytes of the value representing the height of the outer surface.
            NetworkUtil.HostToNetworkOrder(sidelevel).CopyTo(buffer, 66);
            // Send the packet.
            SendRaw(OpCode.EnvMapAppearance, buffer);
        }

        /// <summary>
        /// Sends a SetMapWeather packet to the client.
        /// </summary>
        /// <param name="weather"> 0 for sunny weather, 1 for rain, and 2 for snow. 
        /// </param>
        public void SendSetMapWeather(byte weather) {
            byte[] buffer = new byte[1];
            buffer[0] = weather;
            SendRaw(OpCode.EnvWeatherType, buffer);
        }

        /// <summary>
        /// Sends a HackControl packet to the client.
        /// </summary>
        /// <param name="allowflying"> 1 if flying is allowed, 0 otherwise. </param>
        /// <param name="allownoclip"> 1 if noclip is allowed, 0 otherwise. </param>
        /// <param name="allowspeeding"> 1 if speedhacks are allowed, 0 otherwise. </param>
        /// <param name="allowrespawning"> 1 if changing spawn and respawning is allowed, 0 otherwise. </param>
        /// <param name="allowthirdperson"> 1 if third-person view is allowed, 0 otherwise. </param>
        /// <param name="allowchangingweather"> 1 if changing the weather is allowed, 0 otherwise. </param>
        /// <param name="maxjumpheight"> The maximum height that the player can jump. </param>
        public void SendHackControl(byte allowflying, byte allownoclip, byte allowspeeding, byte allowrespawning,
                                    byte allowthirdperson, byte allowchangingweather, short maxjumpheight) {
            byte[] buffer = new byte[7];
            // Copy the allowance values to the buffer.
            buffer[0] = allowflying;
            buffer[1] = allownoclip;
            buffer[2] = allowspeeding;
            buffer[3] = allowrespawning;
            buffer[4] = allowthirdperson;
            buffer[5] = allowchangingweather;  // TODO: this isn't in the spec
            // Flip the bytes representing the maximum jump height before sending the packet.
            NetworkUtil.HostToNetworkOrder(maxjumpheight).CopyTo(buffer, 6);
            SendRaw(OpCode.HackControl, buffer);
        }

        /// <summary>
        /// Updates a player's position and sends a packet to everyone on the server.
        /// </summary>
        public void UpdatePosition() {
            // Shameless copy from JTE's Server
            byte changed = 0; //Denotes what has changed (x,y,z, rotation-x, rotation-y)
            // 0 = no change - never happens with this code.
            // 1 = position has changed
            // 2 = rotation has changed
            // 3 = position and rotation have changed
            // 4 = Teleport Required (maybe something to do with spawning)
            // 5 = Teleport Required + position has changed
            // 6 = Teleport Required + rotation has changed
            // 7 = Teleport Required + position and rotation has changed
            //NOTE: Players should NOT be teleporting this often. This is probably causing some problems.

            // Note: |= means bitwise OR.

            // For position changes.
            if (oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2]) {
                changed |= 1;
            }

            // For rotation changes.
            if (oldrot[0] != rot[0] || oldrot[1] != rot[1]) {
                changed |= 2;
            }

            // If the player's position is too far from the current position, assume that a
            // teleport is required. (The tolerance is one block.)
            if (Math.Abs(pos[0] - oldpos[0]) > 32 || Math.Abs(pos[1] - oldpos[1]) > 32 || Math.Abs(pos[2] - oldpos[2]) > 32) {
                changed |= 4;
            }

            // Check for a position change again.
            if (changed == 0) {
                if (oldpos[0] != pos[0] || oldpos[1] != pos[1] || oldpos[2] != pos[2]) {
                    changed |= 1;
                }
            }

            byte[] buffer = new byte[0]; // The data to send.
            OpCode msg = 0; // The packet id.

            // For teleporting the player. Used for spawning or moving too fast.
            // Packet id: 0x08
            if ((changed & 4) != 0) {
                msg = OpCode.Teleport;
                buffer = new byte[9];
                buffer[0] = serverId; // The player id.

                // Flip the bytes to network order.
                NetworkUtil.HostToNetworkOrder(pos[0]).CopyTo(buffer, 1);
                NetworkUtil.HostToNetworkOrder(pos[1]).CopyTo(buffer, 3);
                NetworkUtil.HostToNetworkOrder(pos[2]).CopyTo(buffer, 5);
                buffer[7] = rot[0];

                // Flip the head if someone in the server used /flipheads.
                if (_server.flipHead)
                    if (rot[1] > 64 && rot[1] < 192) {
                        buffer[8] = rot[1];
                    }
                    else {
                        buffer[8] = (byte)(rot[1] - (rot[1] - 128));
                    }
                else {
                    buffer[8] = rot[1];
                }
            }

            // Only the player's position needs to be updated.
            // Packet id: 0x0A
            else if (changed == 1) {
                try {
                    msg = OpCode.Move;
                    buffer = new byte[4];
                    buffer[0] = serverId; // The player id.

                    // Convert the difference between the old position and new position into
                    // a single, signed byte, and copy the result into the corresponding position
                    // in the packet data.
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
            }

            // Only the rotation of the player's head needs to be updated.
            // Packet id: 0x0B
            else if (changed == 2) {
                msg = OpCode.Rotate;
                buffer = new byte[3];
                buffer[0] = serverId; // The player id.
                buffer[1] = rot[0];

                // Flip the head if someone in the server used /flipheads.
                if (_server.flipHead)
                    if (rot[1] > 64 && rot[1] < 192) {
                        buffer[2] = rot[1];
                    }
                    else {
                        buffer[2] = (byte)(rot[1] - (rot[1] - 128));
                    }
                else {
                    buffer[2] = rot[1];
                }
            }

            // Both the position and head rotation of the player needs to be updated.
            // Packet id: 0x09
            else if (changed == 3) {
                try {
                    msg = OpCode.MoveRotate;
                    buffer = new byte[6];
                    buffer[0] = serverId; // The player id.

                    // Convert the difference between the old position and new position into
                    // a single, signed byte, and copy the result into the corresponding position
                    // in the packet data.
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[0] - oldpos[0])), 0, buffer, 1, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[1] - oldpos[1])), 0, buffer, 2, 1);
                    Buffer.BlockCopy(System.BitConverter.GetBytes((sbyte)(pos[2] - oldpos[2])), 0, buffer, 3, 1);
                    buffer[4] = rot[0];

                    // Flip the head if someone in the server used /flipheads.
                    if (_server.flipHead)
                        if (rot[1] > 64 && rot[1] < 192) {
                            buffer[5] = rot[1];
                        }
                        else {
                            buffer[5] = (byte)(rot[1] - (rot[1] - 128));
                        }
                    else {
                        buffer[5] = rot[1];
                    }
                }
                catch (Exception e) {
                    _logger.ErrorLog(e);
                }
            }

            // Store the old position and rotation information.
            oldpos = pos;
            oldrot = rot;

            // Send the updated position and head rotation to other players in the same level.
            if (changed != 0) {
                try {
                    foreach (Player p in _server.players) {
                        if (p != this && p.level == level) {
                            p.SendRaw(msg, buffer);
                        }
                    }
                }
                catch (Exception e) { 
                    _logger.ErrorLog(e);
                }
            }
        }
        #endregion

        #region == DISCONNECTING ==
        /// <summary>
        /// Disconnects a player from the server.
        /// </summary>
        public void Disconnect() {
            leftGame();
        }

        /// <summary>
        /// Kicks a player from the server with the given reason.
        /// </summary>
        /// <param name="kickString"> The reason for kicking the player. </param>
        public void Kick(string kickString) {
            leftGame(kickString);
        }

        /// <summary>
        /// Closes the player's Socket.
        /// </summary>
        internal void CloseSocket() {
            // Attempt to shutdown the socket. Errors may occur for various reasons.
            try {
                socket.Shutdown(SocketShutdown.Both);
#if DEBUG
                _logger.Log("Socket was shutdown for " + this.name ?? this.ip);
#endif
            }
            catch (Exception e) {
#if DEBUG
                Exception ex = new Exception("Failed to shutdown socket for " + this.name ?? this.ip, e);
                _logger.ErrorLog(ex);
#endif
            }

            // Attempt to close the socket. Errors may occur for various reasons.
            try {
                socket.Close();
#if DEBUG
                _logger.Log("Socket was closed for " + this.name ?? this.ip);
#endif
            }
            catch (Exception e) {
#if DEBUG
                Exception ex = new Exception("Failed to close socket for " + this.name ?? this.ip, e);
                _logger.ErrorLog(ex);
#endif
            }
        }

        /// <summary>
        /// Disconnects a player from the server and cleans up any resources they used.
        /// </summary>
        /// <param name="kickString"> The disconnect message. </param>
        public void leftGame(string kickString = "") {
            // This is probably for those clients that connect but immediately disconnect before
            // their name can be even retrieved.
            if (name == String.Empty) {
                if (socket != null) {
                    CloseSocket();
                }
                if (connections.Contains(this)) {
                    connections.Remove(this);
                }
                SaveUndo();
                disconnected = true;
                return;
            }
            // Remove the disconnecting player from the review list if they are in it.
            bool leavetest = false;
            foreach (string testwho2 in _server.reviewlist) {
                if (testwho2 == name) {
                    leavetest = true;
                }
            }
            if (leavetest) {
                _server.reviewlist.Remove(name);
            }
            try {
                // If he has been marked as disconnected for any other reason, 
                // close the socket, remove him from the list of players connected,
                /// and don't do anything else.
                if (disconnected) {
                    this.CloseSocket();
                    if (connections.Contains(this)) {
                        connections.Remove(this);
                    }
                    return;
                }
                // Otherwise, mark the player as disconnected.
                disconnected = true;
                // Stop the ping timer.
                pingTimer.Stop();
                pingTimer.Dispose();

                // Save the player's ignore lists.
                string ignorePath = Path.Combine("ranks", "ignore", this.name + ".txt");
                if (File.Exists(ignorePath)) {
                    try {
                        File.WriteAllLines(ignorePath, this.listignored.ToArray());
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                        _logger.Log("Failed to save ignored list for player: " + this.name);
                    }
                }
                string globalIgnorePath = Path.Combine("ranks", "ignore", "GlobalIgnore.xml");
                if (File.Exists(globalIgnorePath)) {
                    try {
                        File.WriteAllLines(globalIgnorePath, globalignores.ToArray());
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                        _logger.Log("failed to save global ignore list!");
                    }
                }

                // Stop other timers.
                afkTimer.Stop();
                afkTimer.Dispose();
                muteTimer.Stop();
                muteTimer.Dispose();
                timespent.Stop();
                timespent.Dispose();
                afkCount = 0;
                afkStart = DateTime.Now;

                // Removes a player from the AFK list if they are on it.
                if (_server.afkset.Contains(name)) {
                    _server.afkset.Remove(name);
                }

                // If no disconnect reason was provided, use the default one.
                if (kickString == String.Empty) {
                    kickString = "Disconnected.";
                }

                // Send a packet to the player's client informing him that he has been kicked.
                SendKick(kickString);

                // Handle other things if the player managed to successfully load the main level.
                if (loggedIn) {
                    // Reset flying and /gun status.
                    isFlying = false;
                    aiming = false;

                    // Despawn him on other people's clients.
                    _server.GlobalDie(this, false);

                    // If the disconnect reason indicates that it isn't a kick...
                    if (kickString == "Disconnected." || kickString.IndexOf("Server shutdown") != -1
                            || kickString == _server.props.customShutdownMessage) {

                        // Don't display logout messages for hidden players.
                        if (!hidden) {
                            string leavem = "&c- " + color + prefix + name + _server.props.DefaultColor + " disconnected.";
                            // Check the setting that indicates whether join/leave messages
                            // for guests are shown.
                            _server.players.ForEach(delegate(Player p1) {
                                p1.SendMessage(leavem);
                            });
                        }
                        _logger.Log(name + " disconnected.");
                    }

                    // If the disconnect reason is different than normal, it was an actual kick.
                    // Kick the player if this is the case.
                    else {
                        totalKicked++;
                        _server.GlobalChat(this, "&c- " + color + prefix + name + _server.props.DefaultColor + " kicked (" + kickString + _server.props.DefaultColor +
                                   ").", false);
                        _logger.Log(name + " kicked (" + kickString + ").");
                    }

                    // Save the player's SQL data.
                    try {
                        save();
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }

                    // Remove the player from the list of players.
                    _server.players.Remove(this);
                    _server.players.ForEach(delegate(Player p) {
                        if (p != this && p.extension) {
                            p.SendExtRemovePlayerName(this.serverId);
                        }
                    });

                    // Add them to the list of players that left so /clones and related other commands work.
                    try {
                        left.Add(this.name.ToLower(), this.ip);
                    }
                    catch (Exception e) {
                        _logger.ErrorLog(e);
                    }

                    // If needed, unload the level the player was on. This is usually the case if the player
                    // was the only one left on the server.
                    if (_server.props.AutoLoad && level.unload && !level.name.Contains("Museum " + _server.props.DefaultColor)
                            && IsAloneOnCurrentLevel()) {
                        level.Unload(true);
                    }

                    // Call any methods listening for this event.
                    EventHandler<PlayerDisconnectedEventArgs> method = PlayerDisconnected;
                    if (method != null) {
                        method(this, new PlayerDisconnectedEventArgs(kickString));
                    }

                    this.Dispose();
                }
                else { // Otherwise, just remove the player from the connection list.
                    connections.Remove(this);

                    _logger.Log(ip + " disconnected.");
                }

            }
            catch (Exception e) {
                _logger.ErrorLog(e);
            }
            finally {
                CloseSocket();
            }
        }

        /// <summary>
        /// Save the player's block history for undo purposes.
        /// </summary>
        public void SaveUndo() {
            // If there's nothing to save, stop early.
            if (this.UndoBuffer == null || this.UndoBuffer.Count < 1) {
                return;
            }
            try {
                // Create any needed folders.
                string undoPath = Path.Combine("extra", "undo");
                string undoPrevPath = Path.Combine("extra", "undoPrevious");
                string playerUndoPath = Path.Combine(undoPath, this.uuid.Value);

                if (!Directory.Exists(undoPath)) {
                    Directory.CreateDirectory(undoPath);
                }
                if (!Directory.Exists(undoPrevPath)) {
                    Directory.CreateDirectory(undoPrevPath);
                }

                // This is for the feature that limits the number of users whose block history
                // should be saved. Basically, the current undo folder becomes the previous
                // undo folder, and the previous undo folder is deleted.
                DirectoryInfo di = new DirectoryInfo(undoPath);
                if (di.GetDirectories("*").Length >= _server.props.totalUndo) {
                    Directory.Delete(undoPrevPath, true);
                    Directory.Move(undoPath, undoPrevPath);
                    Directory.CreateDirectory(undoPath);
                }

                // If there is no undo folder for that player, create it.
                if (!Directory.Exists(playerUndoPath)) {
                    Directory.CreateDirectory(playerUndoPath);
                }

                // Get the current number for the undo file, similar to level backups.
                di = new DirectoryInfo(playerUndoPath);
                int number = di.GetFiles("*.undo").Length;

                string curUndoPath = Path.Combine(playerUndoPath, number + ".undo");

                // Create the undo file.
                File.Create(curUndoPath).Dispose();

                // Copy each block in the undo buffer to the .undo file.
                using (StreamWriter w = File.CreateText(curUndoPath)) {
                    foreach (UndoPos uP in this.UndoBuffer.ToList()) {
                        w.Write(uP.mapName + " " +
                                uP.x + " " + uP.y + " " + uP.z + " " +
                                uP.timePlaced.ToString(CultureInfo.InvariantCulture).Replace(' ', '&') + " " +
                                Convert.ToInt32(uP.type) + " " + Convert.ToInt32(uP.newtype) + " ");
                    }
                }
            }
            catch (Exception e) {
                _logger.Log("Error saving undo data for " + this.name + "!");
                _logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Disposes of the Player object, releasing memory and handles to unmanaged objects.
        /// </summary>
        public void Dispose() {
            if (connections.Contains(this)) {
                connections.Remove(this);
            }
            CopyBuffer.Clear();
            RedoBuffer.Clear();
            UndoBuffer.Clear();
            spamBlockLog.Clear();
        }

        /// <summary>
        /// Checks if the player is the only player on their current level.
        /// </summary>
        /// <returns> Whether there is no one else on the level the player is on. </returns>
        public bool IsAloneOnCurrentLevel() {
            return _server.players.All(pl => pl.level != level || pl == this);
        }

        #endregion

        /// <summary>
        /// Finds the first free player ID.
        /// </summary>
        /// <param name="players"> The list of online players. <seealso cref="OnlinePlayerList"/></param>
        /// <returns> A player ID that is not being used by any player. </returns>
        static byte FreeId(OnlinePlayerList players) {
            for (ushort i = 0; i < 255; i++) {
                bool used = players.Any(p => p.serverId == i);

                if (!used) {
                    return (byte)i;
                }
            }
            return (byte)1;
        }

        /// <summary>
        /// Converts a string into a format suitable for sending to clients.
        /// </summary>
        /// <param name="str"> The string to be sent.  </param>
        /// <param name="size"> The size of the string buffer. </param>
        /// <returns> An array of bytes containing the formatted string. </returns>
        public static byte[] StringFormat(string str, int size) {
            byte[] bytes = new byte[size];
            // Any leftover space in the string becomes whitespace.
            bytes = enc.GetBytes(str.PadRight(size).Substring(0, size));
            return bytes;
        }

        // TODO: Optimize this using a StringBuilder
        /// <summary>
        /// Breaks up messages into chunks that are, at most, 64 characters long.
        /// </summary>
        /// <param name="message"> The message to send. </param>
        /// <returns> The chunks of the message to send. </returns>
        static List<string> Wordwrap(string message) {
            // Holds all the lines to send.
            List<string> lines = new List<string>();
            // Replace consecutive color codes.
            message = Regex.Replace(message, @"(&[0-9a-f])+(&[0-9a-f])", "$2");
            message = Regex.Replace(message, @"(&[0-9a-f])+$", String.Empty);

            int limit = 64; // The maximum number of characters in each packet
            string color = String.Empty;
            while (message.Length > 0) { // Keep parsing text until there is none.
                if (lines.Count > 0) { // Adjust for any previous lines.
                    // If a message begins with a color code, use it.
                    if (message[0].ToString() == "&") {
                        message = "> " + message.Trim();
                    }
                    else { // Otherwise, carry over the color that was in the previous line.
                        message = "> " + color + message.Trim();
                    }
                }

                // This removes two consecutive color codes...?
                if (message.IndexOf("&") == message.IndexOf("&", message.IndexOf("&") + 1) - 2) {
                    message = message.Remove(message.IndexOf("&"), 2);
                }

                // If the remaining message is shorter than the packet limit, just add the
                // remaining characters as the last line and break out of the loop.
                if (message.Length <= limit) {
                    lines.Add(message);
                    break;
                }

                // Find a space between two words where the message can be split.
                for (int i = limit - 1; i > limit - 20; --i)
                    if (message[i] == ' ') {
                    // Add the characters up until that space.
                    lines.Add(message.Substring(0, i));
                    goto Next;
                }

                retry:
                    if (message.Length == 0 || limit == 0) { // Stop if there's no more text to examine.
                    return lines;
                }

                // This is for "words" that are too long and can't be split up by finding a space.
                // Remove a color code or ampersand at the end of a chunk.
                if (message.Substring(limit - 2, 1) == "&" || message.Substring(limit - 1, 1) == "&") {
                    message = message.Remove(limit - 2, 1);
                    limit -= 2;
                    goto retry; // Look for more color codes.
                }
                // Find any illegal characters to remove at the end of a chunk.
                else if (message[limit - 1] < 32 || message[limit - 1] > 127) {
                    message = message.Remove(limit - 1, 1);
                    limit -= 1;
                }

                // Add the whole line.
                lines.Add(message.Substring(0, limit));

                Next: 
                    // Remove the line that was just added.
                message = message.Substring(lines[lines.Count - 1].Length);
                if (lines.Count == 1) { // Account for the > character that is added.
                    limit = 60;
                }

                // Attempt to find the last color code used in the line that was just added and
                // apply it to the next line.
                int index = lines[lines.Count - 1].LastIndexOf('&');
                if (index != -1) {
                    if (index < lines[lines.Count - 1].Length - 1) {
                        // Get the character next to the ampersand.
                        char next = lines[lines.Count - 1][index + 1];
                        if ("0123456789abcdef".IndexOf(next) != -1) {
                            color = "&" + next;
                        }
                        // If the last color code used happened to be at the edge of a line,
                        // take that color code out from that location.
                        // Although, is this ever true? -Jjp137
                        if (index == lines[lines.Count - 1].Length - 1) {
                            lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 2);
                        }
                    }
                    else if (message.Length != 0) {
                        // This is for if the ampersand and the character following it were split up.
                        char next = message[0];
                        if ("0123456789abcdef".IndexOf(next) != -1) {
                            color = "&" + next;
                        }
                        lines[lines.Count - 1] = lines[lines.Count - 1].Substring(0, lines[lines.Count - 1].Length - 1);
                        message = message.Substring(1);
                    }
                }
            }

            // Remove color codes that are at the end of each line so that clients will not crash.
            char[] temp;
            for (int i = 0; i < lines.Count; i++) {
                // This is dumb - Jjp137
                temp = lines[i].ToCharArray();

                StringBuilder message1 = new StringBuilder();
                message1.Append(temp);
                lines[i] = message1.ToString();

                // Crappy fix for rare instances of the percent bug
                // Here's a line that triggers it, for future reference:
                // (16:23:12) <plrplrplr1122334> Example: /me % ewent to %b [fake name]

                while (Regex.IsMatch(lines[i], @"&[0-9a-f][\s]*$")) {
                    lines[i] = Regex.Replace(lines[i], @"&[0-9a-f][\s]*$", String.Empty);
                }
            }
            return lines;
        }

        /// <summary>
        /// Checks whether a player's name consists of only valid characters.
        /// </summary>
        /// <param name="name"> The name to check. </param>
        /// <returns> Whether the name is valid. </returns>
        public static bool ValidName(string name) {
            // MCHmk has never allowed e-mail addresses to login, so don't include @.
            // The dot and underscore are allowed. A plus sign, which is added to the end of a ClassiCube name,
            // and a tilde, which indicates unpaid Minecraft accounts, are allowed only at the end.
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890._";

            // An empty name isn't really a name.
            if (String.IsNullOrEmpty(name)) {
                return false;
            }

            // Only one suffix is allowed.
            if (name.EndsWith("+~", StringComparison.Ordinal) || name.EndsWith("~+", StringComparison.Ordinal)) {
                return false;
            }

            return StringUtil.ContainsOnlyChars(name.TrimEnd('+', '~'), allowedChars);
        }

        /// <summary>
        /// For the easter egg command /pulpfiction. Delays the kill after the 
        /// ccommand's server wide message is send.
        /// </summary>
        private void PulpKill(Player who) {
            Thread.Sleep (10000); // 10 seconds
            _server.commands.FindCommand("kill").Use(this, who.name + " was smitten with vengence by " + this.color + this.name);
            return;
        }

        /// <summary>
        /// The method that mutes the player for a certain amount of seconds when they join the server.
        /// </summary>
        private void XMute() {
            string name = this.name;
            string path = Path.Combine("ranks", "muted.txt");

            if (File.Exists(path)) {
                foreach (string line in File.ReadAllLines(path)) {
                    string[] finder = line.Split (':');
                    if (finder.Length == 2 && finder[0] == name.ToLower()) {
                        _server.muted.Remove(this.uuid);
                        _server.muted.Save("muted.txt");
                        _logger.Log("SAVED: ranks/muted.txt");
                        _server.commands.FindCommand("mute").Use(_server.ServerConsole, name + " " + finder[1]);
                    }
                }
            }
        }

        /// <summary>
        /// The method that starts a thread to run a method.
        /// </summary>
        /// <param name="method"> The method to be run by a new thread. </param>
        public void StartThread(ThreadStart method) {
            Thread t = new Thread(method);
            t.Start();
        }

        /// <summary>
        /// Kicks the player if they placed too many blocks within a certain time period.
        /// </summary>
        /// <returns> Whether the player was kicked. </returns>
        internal bool CheckBlockSpam() {
            // Check if there are potentially too many block placements.
            if (spamBlockLog.Count >= spamBlockCount) {
                // Compare the date/time of the oldest block placement with the current time, and
                // if the gap is too small, kick the player.
                DateTime oldestTime = spamBlockLog.Dequeue();
                double spamTimer = DateTime.Now.Subtract(oldestTime).TotalSeconds;
                if (spamTimer < spamBlockTimer && !ignoreGrief) {
                    this.Kick("You were kicked by antigrief system. Slow down.");
                    SendMessage(Colors.red + name + " was kicked for suspected griefing.");
                    _logger.Log(name + " was kicked for block spam (" + spamBlockCount + " blocks in " + spamTimer + " seconds)");
                    return true;
                }
            }
            // Otherwise, just make a new entry in the block log with the current time.
            spamBlockLog.Enqueue(DateTime.Now);
            return false;
        }

        /// <summary>
        /// Marks a user as possessed.
        /// </summary>
        /// <param name="marker"> The name of the player that is possessing the player. </param>
        /// <returns> Whether the possession succeeded. </returns>
        public bool MarkPossessed(string marker = "") {
            // If given, append the possessor's name to the possessed player's name.
            if (marker != String.Empty) {
                Player controller = _server.players.Find(marker);
                if (controller == null) {
                    return false;
                }
                marker = " (" + controller.color + controller.name + color + ")";
            }
            // Despawn the player. This is needed so that the player can be respawned with
            // a different name.
            _server.GlobalDie(this, true);
            // The last parameter affects the name displayed to other clients.
            _server.GlobalSpawn(this, pos[0], pos[1], pos[2], rot[0], rot[1], true, marker);
            return true;
        }

        #region getters
        /// <summary>
        /// Gets the position of the player at his foot.
        /// </summary>
        public ushort[] footLocation {
            get {
                return getLoc(false);
            }
        }

        /// <summary>
        /// Gets the position of the player at his head.
        /// </summary>
        public ushort[] headLocation {
            get {
                return getLoc(true);
            }
        }

        /// <summary>
        /// Gets the position of the player.
        /// </summary>
        /// <param name="head"> Whether the position obtained should be at the player's head. </param>
        /// <returns> The position of the player as block coordinates. </returns>
        public ushort[] getLoc(bool head) {
            ushort[] myPos = pos;
            myPos[0] /= 32;
            if (head) {
                myPos[1] = (ushort)((myPos[1] + 4) / 32);
            }
            else {
                myPos[1] = (ushort)((myPos[1] + 4) / 32 - 1);  // One block below.
            }
            myPos[2] /= 32;
            return myPos;
        }

        /// <summary>
        /// Sets the position of the player.
        /// </summary>
        /// <param name="myPos"> The new position of the player as an array with three elements.
        /// They should be block coordinates. </param>
        public void setLoc(ushort[] myPos) {
            myPos[0] *= 32;
            myPos[1] *= 32;
            myPos[2] *= 32;
            unchecked {
                SendPos((byte)-1, myPos[0], myPos[1], myPos[2], rot[0], rot[1]);
            }
        }

        #endregion

        /// <summary>
        /// Checks if the player has at least a certain amount of money.
        /// </summary>
        /// <param name="amount"> The amount of money to check for. </param>
        /// <returns> Whether the player has at least that much money. </returns>
        public bool EnoughMoney(int amount) {
            return this.money >= amount;
        }

        /// <summary>
        /// Checks if the player can modify the level they are on.
        /// </summary>
        /// <returns> Whether the player can modify the level they are on. </returns>
        public bool CanModifyCurrentLevel() {
            return rank.Permission >= level.permissionbuild &&
                (rank.Permission <= level.perbuildmax || _server.commands.CanExecute(this, "perbuildmax"));
        }

        /// <summary>
        /// Enforces the /review cooldown.
        /// </summary>
        public void ReviewTimer() {
            this.canusereview = false;
            System.Timers.Timer Clock = new System.Timers.Timer(1000 * _server.props.reviewcooldown);
            Clock.Elapsed += delegate {
                this.canusereview = true;
                Clock.Dispose();
            };
            Clock.Start();
        }
    }
}

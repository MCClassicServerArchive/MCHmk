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
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MCHmk;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace MCHmk.Gui {
    /// <summary>
    /// The Program class is the starting point of MCHmk's code. It is responsible for providing
    /// a user interface for the server host, and it handles startup and shutdown operations.
    /// </summary>
    public static class Program {
        /// <summary>
        /// A reference to the Server.
        /// </summary>
        public static Server server;
        /// <summary>
        /// A reference to the Server's logger.
        /// </summary>
        public static Logger logger;

        /// <summary>
        /// Handles all exceptions that were not thrown in other places.
        /// </summary>
        /// <param name="sender"> The object that caused the event. </param>
        /// <param name="e"> Data about the unhandled exception. </param>
        public static void GlobalExHandler(object sender, UnhandledExceptionEventArgs e) {
            Exception ex = (Exception)e.ExceptionObject;
            logger.ErrorLog(ex);
        }

        /// <summary>
        /// Handles all exceptions that occur in threads that were not caught during execution.
        /// </summary>
        /// <param name="sender"> The object that caused the event. </param>
        /// <param name="e"> Data about the exception. </param>
        public static void ThreadExHandler(object sender, ThreadExceptionEventArgs e) {
            Exception ex = e.Exception;
            logger.ErrorLog(ex);
        }

        /// <summary>
        /// This does nothing. It is just there so that it has a Main method.
        /// </summary>
        /// <param name="args"> Not used. </param>
        static void Main(string[] args) {
            return;
        }

        /// <summary>
        /// Entry point of MCHmk. Call from the Console or GUI .exe to start the server.
        /// </summary>
        public static void Start() {
            // Time how long it takes to start the server.
            Stopwatch startupTime = Stopwatch.StartNew();

            // Assign methods that are called in the event that an unhandled exception or thread
            // exception occurs.
            AppDomain.CurrentDomain.UnhandledException += GlobalExHandler;
            Application.ThreadException += ThreadExHandler;

            // Print out debug statements to the console when a debug build is being used.
            Debug.Listeners.Remove("Default");
            Debug.Listeners.Add(new ConsoleTraceListener());

            try {
                // Construct a server instance and obtain a reference to the logger.
                server = new Server();
                logger = server.logger;

                // Write command usage, system messages, and general messages to the console.
                // This is done by attaching a method to Logger events.
                logger.MessageLogged += WriteToConsole;
                logger.CommandLogged += WriteToConsole;
                logger.SystemMessageLogged += WriteToConsole;

                // Start the server.
                server.Start();

                // Set the console window's title.
                Console.Title = server.props.name + " - MCHmk r" + server.Version.Revision;

                // Stop the stopwatch and print a message stating how long it took to start the server.
                startupTime.Stop();
                logger.Log("Server setup finished in: " + startupTime.ElapsedMilliseconds.ToString() + "ms");

                // Accept console input.
                HandleInput();
            }
            catch (Exception e) {
                logger.ErrorLog(e);
            }
        }

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message"> The message to write. <see cref="System.String"/></param>
        private static void WriteToConsole(string message) {
            // It's easy if there's no colors in the message.
            if (!message.Contains("&") && !message.Contains("%")) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message);
                return;
            }

            // Normal case: handle messages with color codes.
            string[] splitted = message.Split(new char[] {'&','%'});
            for (int i = 0; i < splitted.Length; i++) {
                string elString = splitted[i];
                // If the first character represents a character code, change the text color accordingly
                // and write that segment of the message to the console.
                if (!String.IsNullOrEmpty(elString) && ((elString[0] >= '0' && elString[0] <= '9')
                                                        || (elString[0] >= 'a' && elString[0] <= 'f'))) {
                    Console.ForegroundColor = GetColor(elString[0]);
                    Console.Write(elString.Substring(1));
                }
                // Otherwise, just write the string with the current color.
                else if (!String.IsNullOrEmpty(elString)) {
                    // Crappy hack alert: don't put in a % for the timestamp part
                    if (i == 0) {
                        Console.Write(elString);
                    }
                    else {
                        Console.Write("%" + elString);
                    }
                }
            }
            // Reset the color to white for the next line.
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Returns the command line's equivalent of a given color code.
        /// </summary>
        /// <param name="c"> A character representing the color code.
        /// <see cref="System.Char"/></param>
        /// <returns> The corresponding ConsoleColor constant. <see cref="ConsoleColor"/></returns>
        private static ConsoleColor GetColor(char c) {
            switch (c) {
                case 'e':
                    return ConsoleColor.Yellow;
                case '0':
                    return ConsoleColor.Black;
                case '1':
                    return ConsoleColor.DarkBlue;
                case '2':
                    return ConsoleColor.DarkGreen;
                case '3':
                    return ConsoleColor.DarkCyan;
                case '4':
                    return ConsoleColor.DarkMagenta;
                case '7':
                    return ConsoleColor.Gray;
                case '6':
                    return ConsoleColor.DarkYellow;
                case '8':
                    return ConsoleColor.DarkGray;
                case '9':
                    return ConsoleColor.Blue;
                case 'a':
                    return ConsoleColor.Green;
                case 'b':
                    return ConsoleColor.Cyan;
                case 'c':
                    return ConsoleColor.Red;
                case 'd':
                    return ConsoleColor.Magenta;
                default:  // If the color code is f
                    return ConsoleColor.White;
            }
        }

        /// <summary>
        /// Handles user input from the console.
        /// </summary>
        private static void HandleInput() {
            string input;

            while (true) {
                try {
                    // Wait for input from the user.
                    input = Console.ReadLine().Trim(); // Make sure we have no whitespace.

                    if (input.Length < 1) {  // Ignore empty input.
                        continue;
                    }
                    else if (input[0] == '/') {  // Interpret strings beginning with a forward slash as commands.
                        HandleCommand(input);
                    }
                    else {  // Everything else is chat.
                        HandleChat(input);
                    }
                }
                catch (Exception ex) {
                    logger.ErrorLog(ex);
                }
            }
        }

        /// <summary>
        /// Executes a command that was entered in the console.
        /// </summary>
        /// <param name="input"> Input from the console. </param>
        private static void HandleCommand(string input) {
            string sentCommand = String.Empty;
            string sentParameters = String.Empty;

            // Remove the slash in front of commands
            input = input.Remove(0, 1);

            // If there are parameters for that command, split the command and its
            // parameters apart.
            if (input.IndexOf(' ') != -1) {
                sentCommand = input.Substring(0, input.IndexOf(' '));
                sentParameters = input.Substring(input.IndexOf(' ') + 1);
            }
            // Otherwise, just send the command.
            else if (input != String.Empty) {
                sentCommand = input;
            }
            else {  // Edge case: the input is only a single slash.
                return;
            }

            try {
                // Find the command and use it if it is a valid command.
                Command cmd = server.commands.FindCommand(sentCommand);
                if (cmd != null) {
                    server.ConsoleCommand(cmd, sentParameters);
                    Console.WriteLine(GetTimestamp() + "CONSOLE: USED /" + sentCommand + " " + sentParameters);
                    logger.LogMessage(GetTimestamp() + "CONSOLE: USED /" + sentCommand + " " + sentParameters + Environment.NewLine);
                }
                else {
                    Console.WriteLine("CONSOLE: Unknown command.");
                }
            }
            catch (Exception e) {
                logger.ErrorLog(e);
                Console.WriteLine("CONSOLE: Failed command.");
            }
        }

        /// <summary>
        /// Sends a message that was entered in the console.
        /// </summary>
        /// <param name="input"> Input from the console. </param>
        private static void HandleChat(string input) {
            string msg;

            // If Console whispers to a person...
            if (input[0] == '@') {
                input = input.Remove(0, 1);
                int spacePos = input.IndexOf(' ');
                if (spacePos == -1) {
                    Console.WriteLine("No message entered.");
                    return;
                }

                Player recipient = server.players.Find(input.Substring(0, spacePos));
                if (recipient == null) {
                    Console.WriteLine("Player not found.");
                    return;
                }

                msg = String.Format("&9[>] {0}Console [&a{1}{0}]: &f{2}", server.props.DefaultColor, server.props.ZallState, 
                                    input.Substring(spacePos + 1));
                recipient.SendMessage(msg);
                logger.LogMessage(GetTimestamp() + "Console [" + server.props.ZallState + "] @" + recipient.name + ": " + 
                             input + Environment.NewLine);
            }
            // If Console speaks in opchat...
            else if (input[0] == '#') {
                input = input.Remove(0, 1);
                msg = String.Format("To Ops -{0}Console [&a{1}{0}]&f- {2}", server.props.DefaultColor, server.props.ZallState, input);
                server.GlobalMessageOps(msg);
                server.IRC.Say(msg, true);
                logger.LogMessage(GetTimestamp() + "(OPs): Console [" + server.props.ZallState + "]: " + input + Environment.NewLine);
            }
            // If Console speaks in adminchat...
            else if (input[0] == '+') {
                input = input.Remove(0, 1);
                msg = String.Format("To Admins -{0}Console [&a{1}{0}]&f- {2}", server.props.DefaultColor, server.props.ZallState, input);
                server.GlobalMessageAdmins(msg);
                server.IRC.Say(msg, true);
                logger.LogMessage(GetTimestamp() + "(Admins): Console [" + server.props.ZallState + "]: " + input + Environment.NewLine);
            }
            // Otherwise, Console speaks normally to the server.
            else {
                msg = String.Format("{0}Console [&a{1}{0}]: &f{2}", server.props.DefaultColor, server.props.ZallState, input);
                server.GlobalMessage(msg);
                server.IRC.Say(msg);
                logger.LogMessage(GetTimestamp() + "Console [" + server.props.ZallState + "]: " + input + Environment.NewLine);
            }
            // Write a copy of what the console said back to the console itself.
            WriteToConsole(GetTimestamp() + msg);
        }

        /// <summary>
        /// Generates the text of the timestamp at the current time.
        /// </summary>
        /// <returns> The timestamp as a string. </returns>
        private static string GetTimestamp() {
            return DateTime.Now.ToString("(HH:mm:ss) ");
        }

        /// <summary>
        /// Exits MCHmk.
        /// </summary>
        public static void ExitProgram() {
            server.shuttingDown = true;

            server.Exit();

            new Thread(new ThreadStart(delegate {
                SaveAll();
                Application.Exit();
                Environment.Exit(0);
            })).Start();
        }

        /// <summary>
        /// Kicks all players and save all levels.
        /// </summary>
        public static void SaveAll() {
            // Kick all players from the server if they have not been kicked already.
            try {
                List<Player> kickList = new List<Player>();
                kickList.AddRange(server.players);
                foreach (Player p in kickList) {
                    p.Kick("Server is shutting down.");
                }
            }
            catch (Exception exc) {
                logger.ErrorLog(exc);
            }

            try {
                string level = null;
                // Save every level and record the current physics setting for each.
                foreach (Level l in server.levels) {
                    level = level + l.name + "=" + l.physics + System.Environment.NewLine;
                    l.Save(false, true);
                    l.saveChanges();
                }
                // If level autoloading is off, save the names of the currently loaded
                // levels to autoload.txt along with the physics settings that they had.
                // This information was gathered in a string generated from the foreach
                // loop above.
                if (server.ServerSetupFinished && !server.props.AutoLoad) {
                    File.WriteAllText(Path.Combine("text", "autoload.txt"), level);
                }
            }
            catch (Exception exc) {
                logger.ErrorLog(exc);
            }
        }
    }
}


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
using System.IO;
using System.Text;
using System.Threading;

namespace MCHmk {
    /// <summary>
    /// The Logger class logs what happens in the server.
    /// </summary>
    public class Logger {
        /// <summary>
        /// Whether the logger has been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Path to the normal log file.
        /// </summary>
        private string _messagePath = "logs/" + DateTime.Now.ToString("yyyy-MM-dd").Replace("/", "-") + ".txt";
        /// <summary>
        /// Path to the error log file.
        /// </summary>
        private string _errorPath = "logs/errors/" + DateTime.Now.ToString("yyyy-MM-dd").Replace("/", "-") + "error.log";

        /// <summary>
        /// Object that is locked when queues are being interacted with in some way.
        /// </summary>
        private object _lockObject = new object();
        /// <summary>
        /// Object that is locked when something is being written to the log file.
        /// </summary>
        private object _fileLockObject = new object();
        /// <summary>
        /// Background thread where the logging takes place.
        /// </summary>
        private Thread _workingThread;

        /// <summary>
        /// Queue containing messages that need to be logged.
        /// </summary>
        private Queue<string> _messageCache = new Queue<string>();
        /// <summary>
        /// Queue containing exceptions that need to be logged. Should be handled first.
        /// </summary>
        private Queue<string> _errorCache = new Queue<string>();

        /// <summary>
        /// This delegate is assigned to methods that manipulate log messages.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        public delegate void LogHandler(string message);
        /// <summary>
        /// This event is triggered when something log-related that isn't associated with the
        /// other LogHandler events occurs.
        /// </summary>
        public event LogHandler MessageLogged;
        /// <summary>
        /// This event is triggered when a system message is triggered.
        /// </summary>
        public event LogHandler SystemMessageLogged;
        /// <summary>
        /// This event is triggered when a command is performed.
        /// </summary>
        public event LogHandler CommandLogged;

        /// <summary>
        /// Path to the normal log file.
        /// </summary>
        public string MessageLogPath {
            get {
                return _messagePath;
            }
            set {
                _messagePath = value;
            }
        }

        /// <summary>
        /// Path to the error log file.
        /// </summary>
        public string ErrorLogPath {
            get {
                return _errorPath;
            }
            set {
                _errorPath = value;
            }
        }

        /// <summary>
        /// Constructs a Logger object.
        /// </summary>
        public Logger() { }

        /// <summary>
        /// Initializes the logger.
        /// </summary>
        public void Init() {
            // Create the log folders if they don't exist.
            if (!Directory.Exists("logs")) {
                Directory.CreateDirectory("logs");
            }
            if (!Directory.Exists("logs/errors")) {
                Directory.CreateDirectory("logs/errors");
            }

            // Create the logger thread and set it to be in the background.
            _workingThread = new Thread(new ThreadStart(WorkerThread));
            _workingThread.IsBackground = true;
            _workingThread.Start();

            // Then return to Program.Main(); the thread will run continually.
        }

        /// <summary>
        /// Logs a message to the log file.
        /// </summary>
        /// <param name="message"> The message to log. <see cref="System.String"/></param>
        /// <param name="systemMsg"> Whether the message is a system message. 
        /// <see cref="System.Boolean"/></param>
        public void Log(string message, bool systemMsg = false) {
            // Handle messages that did not come from opchat or adminchat.
            if (MessageLogged != null) {
                if (!systemMsg) {
                    MessageLogged(DateTime.Now.ToString("(HH:mm:ss) ") + message);
                }
                else {
                    SystemMessageLogged(DateTime.Now.ToString("(HH:mm:ss) ") + message);
                }
            }

            // Log the message to the text file.
            LogMessage(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
        }

        /// <summary>
        /// Logs a player's usage of a command.
        /// </summary>
        /// <param name="message"> The message to log. <see cref="System.String"/></param>
        public void LogCommand(string message) {
            // Plugin stuff.
            if (CommandLogged != null) {
                CommandLogged(DateTime.Now.ToString("(HH:mm:ss) ") + message);
            }
            LogMessage(DateTime.Now.ToString("(HH:mm:ss) ") + message + Environment.NewLine);
        }

        /// <summary>
        /// Logs an error to an error log file.
        /// </summary>
        /// <param name="ex"> The exception to log. <see cref="Exception"/></param>
        public void ErrorLog(Exception ex) {
            LogError(ex);
            try {
                Log("!!!Error! See " + ErrorLogPath + " for more information.");
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Logs a normal message.
        /// </summary>
        /// <param name="message"> The message to log. </param>
        public void LogMessage(string message) {
            try {
                if (!string.IsNullOrEmpty(message)) {
                    lock (_lockObject) {
                        // Add the message to the queue and notify a waiting thread that the state
                        // of the lock object has changed, unblocking it.
                        _messageCache.Enqueue(message);
                        Monitor.Pulse(_lockObject);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            // Old comment: Should it error or passed null or zero string?
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="ex"> The exception to log. </param>
        public void LogError(Exception ex) {
            try {
                StringBuilder sb = new StringBuilder();
                Exception e = ex;

                // Create the entry to be written to the error log file by recursively printing
                // out information about each exception that is involved.
                sb.AppendLine("----" + DateTime.Now + " ----");
                while (e != null) {
                    sb.AppendLine(getErrorText(e));
                    e = e.InnerException;
                }

                sb.AppendLine(new string('-', 25));

                lock (_lockObject) {
                    // Add the message to the queue and notify a waiting thread that the state
                    // of the lock object has changed, unblocking it.
                    _errorCache.Enqueue(sb.ToString());
                    Monitor.Pulse(_lockObject);
                }
            }
            // If an exception occurs, log the error about logging the error to another file.
            catch (Exception e) {
                try {
                    File.AppendAllText("ErrorLogError.log", getErrorText(e));
                }
                catch (Exception _ex) {  // I find this hilarious. -Jjp137
                    Console.WriteLine("ErrorLogError Error:\n Could not log the error logs error. This is a big error. \n" + _ex.Message);
                }
            }
        }

        /// <summary>
        /// Method for the worker thread that does the logging.
        /// </summary>
        private void WorkerThread() {
            while (!_disposed) {
                lock (_lockObject) {  // Prevent the logger from being used by other methods.
                    if (_errorCache.Count > 0) {  // Log any exceptions.
                        FlushCache(_errorPath, _errorCache);
                    }

                    if (_messageCache.Count > 0) {  // Log any other methods.
                        FlushCache(_messagePath, _messageCache);
                    }
                }
                Thread.Sleep(500);  // Allow other methods to access the logger.
            }
        }

        /// <summary>
        /// Flushes a queue, writing its contents to the appropriate log file.
        /// </summary>
        /// <param name="path"> Path of the log file to log to. </param>
        /// <param name="cache"> Queue that contains the messages to log. </param>
        private void FlushCache(string path, Queue<string> cache) {
            // Prevent other writing operations to the log file until this method is over.
            lock (_fileLockObject) {
                FileStream fs = null;

                try {
                    //TODO: not happy about constantly opening and closing a stream like this but I suppose its ok (Pidgeon)
                    fs = new FileStream(path, FileMode.Append, FileAccess.Write);

                    while (cache.Count > 0) { // Write everything that's in the queue at once.
                        byte[] tmp = Encoding.Default.GetBytes(cache.Dequeue());
                        fs.Write(tmp, 0, tmp.Length);
                    }
                    fs.Close();
                }
                finally {
                    fs.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets information about an exception.
        /// </summary>
        /// <param name="e"> The exception to gather information about. </param>
        /// <returns> A string to be written to the error log. </returns>
        private string getErrorText(Exception e) {
            if (e == null) {
                return String.Empty;
            }

            StringBuilder sb = new StringBuilder();

            // Gather information about the exception.
            sb.AppendLine("Type: " + e.GetType().Name);
            sb.AppendLine("Source: " + e.Source);
            sb.AppendLine("Message: " + e.Message);
            if (e.TargetSite != null) {
                sb.AppendLine("Target: " + e.TargetSite.Name);
            }
            else {
                sb.AppendLine("Target:");
            }
            if (e.StackTrace != null) {
                sb.AppendLine("Trace: " + e.StackTrace);
            }
            else {
                sb.AppendLine("Trace:");
            }

            return sb.ToString();
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the logger.
        /// </summary>
        public void Dispose() {
            if (_disposed) {
                return;
            }
            _disposed = true;
            lock (_lockObject) {
                if (_errorCache.Count > 0) {  // Write all remaining errors.
                    FlushCache(_errorPath, _errorCache);
                }

                _messageCache.Clear();  // Clear all normal messages.
                // Signal any waiting threads.
                Monitor.Pulse(_lockObject);
            }
        }

        #endregion
    }
}

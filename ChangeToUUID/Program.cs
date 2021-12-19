/*
    Copyright 2015 Jjp137

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
using System.Data;
using System.Data.Common;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SQLite;

namespace ChangeToUUID {
    class Program {
        /// <summary>
        /// The executable's current path.
        /// </summary>
        private static string workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        /// <summary>
        /// Whether the program should drop the uuids table and fetch the uuids again.
        /// </summary>
        private static bool deleteUUIDtable = false;
        /// <summary>
        /// Whether the program should skip the step where uuids are fetched.
        /// </summary>
        private static bool skipFetching = false;
        /// <summary>
        /// Whether the program should skip the step where the inbox tables are altered.
        /// </summary>
        private static bool skipInbox = false;
        /// <summary>
        /// Whether the program should drop inbox tables that are not associated with any player.
        /// </summary>
        private static bool dropInvalidInboxes = false;
        /// <summary>
        /// Whether the program should skip the step where files in the extra folder are altered.
        /// </summary>
        private static bool skipExtra = false;
        /// <summary>
        /// Whether the program should skip updating the files in the ranks folder.
        /// </summary>
        private static bool skipRanks = false;
        /// <summary>
        /// Whether the program should remove manually ranked names instead of assigning a uuid to them.
        /// Manually ranked names are defined as usernames that are ranked without ever joining the server.
        /// These are usually typos.
        /// </summary>
        private static bool purgeManualRanks = false;
        /// <summary>
        /// Whether the program should skip updating the players award list and the rank list.
        /// </summary>
        private static bool skipText = false;

        /// <summary>
        /// Whether the server uses MySQL. If false, SQLite is being used.
        /// </summary>
        private static bool usingMySQL = true;
        /// <summary>
        /// The IP address of the MySQL server.
        /// </summary>
        private static string myHost = String.Empty;
        /// <summary>
        /// The port that the MySQL server is using.
        /// </summary>
        private static string myPort = String.Empty;
        /// <summary>
        /// The username used to connect to the MySQL server.
        /// </summary>
        private static string myUsername = String.Empty;
        /// <summary>
        /// The password used to connect to the MySQL server.
        /// </summary>
        private static string myPassword = String.Empty;
        /// <summary>
        /// The name of the database that MCHmk uses on the MySQL server.
        /// </summary>
        private static string databaseName = String.Empty;
        /// <summary>
        /// Whether database pooling should be used.
        /// </summary>
        private static bool usePooling = true;

        /// <summary>
        /// The parameters to send when connecting to the SQL database.
        /// </summary>
        private static string connString = String.Empty;

        /// <summary>
        /// A cache containing names and their uuids.
        /// </summary>
        private static Dictionary<string, string> uuidCache = new Dictionary<string, string>();

        /// <summary>
        /// Parse any command line arguments.
        /// </summary>
        /// <param name="args"> The command line arguments. <seealso cref="System.String"/></param>
        private static void ParseArgs(string[] args) {
            for (int i = 0; i < args.Length; i++) {
                string arg = args[i];

                if (arg == "--delete-uuids") {
                    deleteUUIDtable = true;
                }
                else if (arg == "--skip-fetching") {
                    skipFetching = true;
                }
                else if (arg == "--skip-inbox") {
                    skipInbox = true;
                }
                else if (arg == "--drop-bad-inboxes") {
                    dropInvalidInboxes = true;
                }
                else if (arg == "--skip-extra") {
                    skipExtra = true;
                }
                else if (arg == "--skip-ranks") {
                    skipRanks = true;
                }
                else if (arg == "--purge-manual-ranks") {
                    purgeManualRanks = true;
                }
                else if (arg == "--skip-text") {
                    skipText = true;
                }
                else {
                    Console.WriteLine("Invalid argument: " + arg);
                }
            }
        }

        private static void ContinuePrompt() {
            Console.Write("Press Enter to continue, or Ctrl+C to quit: ");
            Console.ReadLine();
        }

        /// <summary>
        /// Prints the help messages that appear when you execute the program.
        /// </summary>
        private static void PrintIntro() {
            Console.Clear();
            Console.WriteLine("This program will convert your entire server folder and your SQL");
            Console.WriteLine("database in order to add support for Minecraft's uuids.");
            Console.WriteLine("Running this program is necessary before using MCHmk r17.");
            Console.WriteLine("Please make sure that you have backed up your server's data before");
            Console.WriteLine("continuing further. MCHmk's developers are not responsible for data loss.");
            Console.WriteLine("For more information about this process, consult the MCHmk wiki at");
            Console.WriteLine("https://bitbucket.org/Jjp137/mchmk/wiki");
            Console.WriteLine();
            Console.WriteLine("Below is the directory that this program is running from:");
            Console.WriteLine(workingDir);
            Console.WriteLine();
            Console.WriteLine("If this directory is not your server's directory, then please quit");
            Console.WriteLine("this program by pressing Ctrl+C and move the program to your server's");
            Console.WriteLine("directory. Otherwise, you may continue.");
            Console.WriteLine();
            Console.Write("Press Enter to continue, or Ctrl+C to quit: ");
            Console.ReadLine();

            Console.Clear();
            Console.WriteLine("The process requires an internet connection in order to make requests to");
            Console.WriteLine("Mojang's API. Please make sure that your connection is stable.");
            Console.WriteLine("Due to the API's rate limits, this process will take a long time if your");
            Console.WriteLine("database contains many players.");
            Console.WriteLine();
            Console.WriteLine("Also, if admin verification is turned on for your server, all of your admins");
            Console.WriteLine("will need to set their passwords again.");
            Console.WriteLine();
            Console.WriteLine("The conversion process will start after this screen.");
            Console.WriteLine();
            ContinuePrompt();

            Console.Clear();
        }

        /// <summary>
        /// Attempts to figure out whether the server is using MySQL or SQLite.
        /// </summary>
        /// <returns> Whether the SQL settings were successfully discovered. <seealso cref="System.Boolean"/></returns>
        private static bool FindSQLSettings() {
            string serverProperties = Path.Combine(workingDir, "properties", "server.properties");
            // The other settings do not need these variables because they are declared as String.Empty.
            bool foundMySQLSetting = false;
            bool foundPoolingSetting = false;

            // Read server.properties.
            try {
                using (StreamReader fin = File.OpenText(serverProperties)) {
                    while (!fin.EndOfStream) {
                        string line = fin.ReadLine();

                        if (line == String.Empty || line[0] == '#') {  // Ignore blank lines and comments
                            continue;
                        }

                        string[] split = line.Split(new char[] {'='}, 2);
                        if (split.Length < 2) {
                            continue;
                        }

                        string key = split[0].Trim().ToLower();
                        string value = split[1].Trim();

                        if (key == "usemysql") {
                            usingMySQL = Boolean.Parse(value);
                            foundMySQLSetting = true;
                        }
                        else if (key == "host") {
                            myHost = value;
                        }
                        else if (key == "sqlport") {
                            myPort = value;
                        }
                        else if (key == "username") {
                            myUsername = value;
                        }
                        else if (key == "password") {
                            myPassword = value;
                        }
                        else if (key == "databasename") {
                            databaseName = value;
                        }
                        else if (key == "pooling") {
                            usePooling = Boolean.Parse(value);
                            foundPoolingSetting = true;
                        }
                    }

                    fin.Close();
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException ||
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while opening or reading server.properties.");
                    Console.WriteLine("Please check if the file is readable, then run this program again.");
                    return false;
                }
                else if (e is FormatException) {
                    Console.WriteLine("A setting in your server.properties is invalid.");
                    Console.WriteLine("Please double-check your SQL settings, then run this program again.");
                    return false;
                }
                else {
                    throw;
                }
            }

            // Check if all the needed settings are obtained.
            // At a minimum, the database engine and pooling settings are required. If MySQL is being
            // used, check that none of the details needed to establish a connection are missing.
            if (foundMySQLSetting && foundPoolingSetting && 
               (!usingMySQL || (usingMySQL && myHost != String.Empty && myPort != String.Empty &&
                                myUsername != String.Empty && myPassword != String.Empty &&
                                databaseName != String.Empty))) {
                Console.WriteLine("Successfully obtained SQL settings.");
                Console.WriteLine("Using " + (usingMySQL ? "MySQL" : "SQLite") + " to connect to the database.");
                return true;
            }
            else {
                Console.WriteLine("The obtained SQL settings are incomplete.");
                Console.WriteLine("Please double-check your SQL settings, then run this program again.");
                return false;
            }
        }

        /// <summary>
        /// Sets the connection string to use when connecting to the SQL database.
        /// </summary>
        private static void SetConnectionString() {
            // The connection strings are taken from the MCHmk source code.
            if (usingMySQL) {
                connString = String.Format("Data Source={0};Port={1};User ID={2};Password={3};Pooling={4}",
                                           myHost, myPort, myUsername, myPassword, usePooling);
            }
            else { // SQLite
                connString = String.Format("Data Source ={0}/MCHmk.db; Version =3; Pooling ={1}; Max Pool Size =1000", 
                                           workingDir, usePooling);
            }
        }

        /// <summary>
        /// Gets an open DbConnection based on whether MySQL is being used or not.
        /// </summary>
        /// <param name="usingMySQL"> Whether MySQL is being used. If false, SQLite is being used.
        /// <seealso cref="System.Boolean"/></param>
        /// <param name="connString"> The parameters to use when connecting to the database.
        /// <seealso cref="System.String"/></param>
        /// <returns> An appropriate DbConnection object that is open. 
        /// <seealso cref="System.Data.Common.DbConnection"/></returns>
        private static DbConnection OpenSQLConnection(bool usingMySQL, string connString) {
            DbConnection conn = null;

            if (usingMySQL) {
                conn = new MySqlConnection(connString);
                conn.Open();
                // A NotImplementedException is thrown if you call this for the SQLiteConnection class, since
                // it doesn't use multiple databases. Thus, we have to make sure to call ChangeDatabase() 
                // only for MySQL.
                conn.ChangeDatabase(databaseName);
            }
            else {
                conn = new SQLiteConnection(connString);
                conn.Open();
            }

            return conn;
        }

        /// <summary>
        /// Reads the players table and attempts to fetch a UUID for each player.
        /// </summary>
        /// <returns> Whether the process completed without major errors. <seealso cref="System.Boolean"/></returns>
        private static bool FetchAllUUIDs() {
            try {
                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    bool uuidsTableExists = false;

                    // Check if the uuids table already exists.
                    using (DbCommand cmd = conn.CreateCommand()) {
                        Console.WriteLine("Checking if the uuids table already exists...");

                        // MySQL and SQLite have different ways of figuring out whether a table exists.
                        if (usingMySQL) {
                            cmd.CommandText = "SHOW TABLES LIKE 'uuids'";
                            using (DbDataReader reader = cmd.ExecuteReader()) {
                                uuidsTableExists = reader.HasRows;  // crappy hack
                            }
                        }
                        else {
                            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='uuids'";
                            Int64 result = (Int64)cmd.ExecuteScalar();
                            uuidsTableExists = (result == 1);
                        }
                    }

                    // Wipe the uuids table if the command line parameter was given and if the table
                    // exists. The table must exist because dropping a table that does not exist
                    // results in an error.
                    if (deleteUUIDtable && uuidsTableExists) {
                        using (DbCommand cmd = conn.CreateCommand()) {
                            cmd.CommandText = "DROP TABLE uuids";
                            cmd.ExecuteNonQuery();
                            uuidsTableExists = false;

                            Console.WriteLine("--delete-uuids parameter given. Table dropped.");
                        }
                    }

                    // Create the uuids table if it does not exist.
                    if (!uuidsTableExists) {
                        using (DbCommand cmd = conn.CreateCommand()) {
                            string autoInc = usingMySQL ? "AUTO_INCREMENT" : "AUTOINCREMENT";  // Yes, really.
                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS uuids (" +
                                              "id INTEGER NOT NULL PRIMARY KEY " + autoInc + ", " +
                                              "uuid VARCHAR(64) NOT NULL UNIQUE, " +
                                              "name VARCHAR(20) NOT NULL)";
                            cmd.ExecuteNonQuery();
                            Console.WriteLine("uuids table created.");
                        }
                    }
                    // Stop execution if the table already exists and let the user decide what to do.
                    else {
                        Console.WriteLine("Remnants of a previous attempt have been detected.");
                        Console.WriteLine("If the previous attempt failed, run this program again with");
                        Console.WriteLine("the --delete-uuids parameter.");
                        ContinuePrompt();
                        return false;
                    }

                    // Count the number of rows in the players table.
                    Int64 numberOfRecords = 0;
                    using (DbCommand cmd = conn.CreateCommand()) {
                        cmd.CommandText = "SELECT COUNT(*) FROM players";
                        numberOfRecords = (Int64)cmd.ExecuteScalar();  // Return type is object, so cast it
                    }
                    Console.WriteLine("Number of rows in the players table: " + numberOfRecords);

                    // Store all the uuids we obtain in this Dictionary.
                    Dictionary<string, string> allIDs = new Dictionary<string, string>();

                    // Read all the names from the players table.
                    using (DbCommand cmd = conn.CreateCommand()) {
                        cmd.CommandText = "SELECT id, name FROM players ORDER BY id";

                        // We use a DbDataReader in order to minimize memory usage.
                        using (DbDataReader reader = cmd.ExecuteReader()) {
                            int rowsRead = 0;
                            // This dictionary will hold up to 100 entries since the Mojang API
                            // only allows 100 uuids to be fetched at a time.
                            Dictionary<string, string> mojangNames = new Dictionary<string, string>();

                            while (reader.Read()) {
                                rowsRead++;

                                if (rowsRead % 100 == 0) {
                                    Console.WriteLine("Read " + rowsRead + "/" + numberOfRecords + " rows.");
                                }

                                string currentName = reader.GetString(1);
                                // Handle ClassiCube names, which have a plus sign at the end, by contacting
                                // ClassiCube's API for the id of that name.
                                if (currentName.EndsWith("+") && !allIDs.ContainsKey(currentName)) {
                                    allIDs.Add(currentName, GetClassiCubeID(currentName));

                                    Thread.Sleep(1000 * 3);  // Delay due to rate limit
                                }
                                // minecraft.net names are added to a seperate dictionary, and their uuids will be 
                                // fetched later. Be careful of duplicates, however.
                                else if (!mojangNames.ContainsKey(currentName) && !allIDs.ContainsKey(currentName)) {
                                    mojangNames.Add(currentName, String.Empty);
                                }

                                // If 100 minecraft.net names have been accumulated or if this is the last row,
                                // contact Mojang's API, and then create uuids for unpaid users.
                                if (mojangNames.Count == 100 || rowsRead == numberOfRecords) {
                                    FetchMojangUUIDs(mojangNames);
                                    HandleUnpaidUsers(mojangNames);

                                    // Transfer the uuids that were fetched or created to the other dictionary,
                                    // and then clear this dictionary so that another 100 names can be stored.
                                    foreach (KeyValuePair<string, string> uuidInfo in mojangNames) {
                                        allIDs.Add(uuidInfo.Key, uuidInfo.Value);
                                    }
                                    mojangNames.Clear();

                                    Thread.Sleep(1000 * 5);  // Delay due to rate limit
                                }
                            }
                        }
                    }

                    conn.Close();

                    // Write to the uuids table.
                    Console.WriteLine("Writing all uuids to the table. This may take a few minutes.");
                    WriteAllUUIDs(allIDs);
                    Console.WriteLine("Done writing to the uuids table.");
                }
            }
            catch (MySqlException) {
                throw;
            }
            catch (SQLiteException) {
                throw;
            }
            return true;
        }

        /// <summary>
        /// Gets the uuid for a ClassiCube user.
        /// </summary>
        /// <param name="name"> The name whose id to fetch. <seealso cref="System.String"/></param>
        /// <returns> A uuid associated with a ClassiCube user. <seealso cref="System.String"/></returns>
        private static string GetClassiCubeID(string name) {
            string response = String.Empty;
            bool success = false;

            while (!success) {
                try {
                    using (WebClient client = new WebClient()) {
                        // The plus sign is what MCHmk does to distinguish ClassiCube users from minecraft.net
                        // users, but we have to remove it since the ClassiCube API will not recognize it.
                        string trimmed = name.TrimEnd(new char[] { '+' });
                        response = client.DownloadString("http://www.classicube.net/api/player/" + trimmed.ToLower());
                        success = true;
                    }
                }
                catch (WebException) {
                    Console.WriteLine("Something went wrong when contacting ClassiCube's API.");
                    Console.WriteLine("Trying again in 5 seconds.");
                    Thread.Sleep(1000 * 5);
                }
            }

            string id = JObject.Parse(response).Value<string>("id");
            string registered = JObject.Parse(response).Value<string>("registered");
            string uuid = String.Empty;

            // ClassiCube accounts can be deleted somehow, so if one is found to be deleted, mark it as such.
            // This should not be an issue if the server is using MCHmk r17+, as uuids will generate for
            // ClassiCube users as they enter the server for the first time. If a user is deleted after this time 
            // and a different person claims the name, the timestamp will uniquely identify the new user and will 
            // result in a different uuid.
            if (String.IsNullOrEmpty(id)) {
                uuid = "classicube_deleted_" + name.TrimEnd('+').ToLower();
            }
            // A normal "uuid" for a ClassiCube user is the below prefix combined with the id and the Unix
            // timestamp of the time that the user registered.
            else {
                uuid = "classicube_" + id + "_" + registered;
            }

            return uuid;
        }

        /// <summary>
        /// Fetches UUIDs for the names in the dictionary.
        /// </summary>
        /// <param name="dict"> The dictionary containing the names of the players to fetch as its keys. 
        /// Its values should be placeholders for uuids. </param>
        private static void FetchMojangUUIDs(Dictionary<string, string> dict) {
            // Copy the names to a string array, and then serialize that array into a JSON object.
            string[] namesToSend = new string[dict.Count];
            dict.Keys.CopyTo(namesToSend, 0);
            string requestText = JsonConvert.SerializeObject(namesToSend);

            string response = String.Empty;
            bool success = false;

            while (!success) {
                try {
                    using (WebClient client = new WebClient()) {
                        client.Headers.Add("Content-Type", "application/json");  // This header is required.
                        response = client.UploadString("https://api.mojang.com/profiles/minecraft", requestText);
                        success = true;
                    }
                }
                catch (WebException) {
                    Console.WriteLine("Something went wrong when contacting Mojang's API.");
                    Console.WriteLine("Trying again in 5 seconds.");
                    Thread.Sleep(1000 * 5);
                }
            }
           
            // Parse the response and assign uuids to the players that have paid for the game.
            JEnumerable<JToken> playerData = JArray.Parse(response).Children();
            foreach (JToken player in playerData) {
                string givenName = player.SelectToken("name").Value<string>();
                string uuid = player.SelectToken("id").Value<string>();

                if (dict.ContainsKey(givenName)) {
                    dict[givenName] = uuid;
                }
            }
        }

        /// <summary>
        /// Assigns uuids to unpaid users.
        /// </summary>
        /// <param name="dict"> A dictionary with usernames as its keys and uuids as its values.
        /// The uuids for players that have paid for the game should have already been assigned. </param>
        private static void HandleUnpaidUsers(Dictionary<string, string> dict) {
            SHA1Managed sha1 = new SHA1Managed();
            // A collection that's being iterated through cannot be altered, so copy the names
            // to an array and iterate through that instead.
            string[] namesCopy = new string[dict.Count];
            dict.Keys.CopyTo(namesCopy, 0);

            // Assign a generated uuid for those who have not paid for the game.
            foreach (string name in namesCopy) {
                if (dict[name] == String.Empty) {  // Paid players have a uuid already.
                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(name));
                    StringBuilder uuid = new StringBuilder();

                    // A "uuid" for unpaid users is the below prefix combined with a sha1 hash of their name.
                    uuid.Append("unpaid_");
                    for (int i = 0; i < hash.Length; i++) {
                        uuid.Append(hash[i].ToString("x2"));
                    }

                    dict[name] = uuid.ToString();
                }
            }
        }

        /// <summary>
        /// Write all names and their uuids to the table.
        /// </summary>
        /// <param name="uuids"> A dictionary mapping usernames to uuids. The uuids should be
        /// populated already. </param>
        private static void WriteAllUUIDs(Dictionary<string, string> uuids) {
            try {
                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    using (DbCommand cmd = conn.CreateCommand()) {
                        // Do this in one transaction for much better performance.
                        using (DbTransaction trans = conn.BeginTransaction()) {
                            // Create a prepared statement.
                            cmd.CommandText = "INSERT INTO uuids (name, uuid) VALUES (@name, @uuid)";

                            DbParameter nameParam = cmd.CreateParameter();
                            nameParam.ParameterName = "@name";
                            nameParam.DbType = System.Data.DbType.String;
                            nameParam.Direction = System.Data.ParameterDirection.Input;

                            DbParameter uuidParam = cmd.CreateParameter();
                            uuidParam.ParameterName = "@uuid";
                            uuidParam.DbType = System.Data.DbType.String;
                            uuidParam.Direction = System.Data.ParameterDirection.Input;

                            cmd.Parameters.Add(nameParam);
                            cmd.Parameters.Add(uuidParam);

                            cmd.Prepare();

                            // Write every name-uuid pair to the table.
                            int recordsWritten = 0;
                            foreach (string name in uuids.Keys) {
                                cmd.Parameters["@name"].Value = name;
                                cmd.Parameters["@uuid"].Value = uuids[name];
                                cmd.ExecuteNonQuery();

                                recordsWritten++;
                                if (recordsWritten % 1000 == 0) {
                                    Console.WriteLine("Written " + recordsWritten + "/" + uuids.Count + " records.");
                                }
                            }

                            // Commit the transaction once every record has been written.
                            trans.Commit();
                        }
                    }

                    conn.Close();
                }
            }
            catch (MySqlException) {
                throw;
            }
            catch (SQLiteException) {
                throw;
            }
        }

        /// <summary>
        /// Fetch a uuid from the uuids table.
        /// </summary>
        /// <param name="name"> The name of the player whose uuid is requested. <seealso cref="Systme.String"/></param>
        /// <param name="conn"> A database connection to use. <seealso cref="System.Data.Common.DbConnection"/></param>
        /// <returns> The uuid, or String.Empty if a uuid cannot be found for that player.
        /// <seealso cref="System.String"/></returns>
        private static string FetchUUIDFromTable(string name, DbConnection conn) {
            // Don't contact the database if we have already obtained the user's uuid earlier.
            if (uuidCache.ContainsKey(name)) {
                return uuidCache[name];
            }

            string uuid = String.Empty;

            // Otherwise, attempt to find the player's uuid.
            try {
                using (DbCommand cmd = conn.CreateCommand()) {
                    // By default, the parameter after the LIKE keyword is a case-insensitive pattern.
                    // It's a crappy hack, but the original database structure is crappy, too. *sighs*
                    cmd.CommandText = "SELECT uuid FROM uuids WHERE name LIKE @name";

                    DbParameter nameParam = cmd.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.DbType = System.Data.DbType.String;
                    nameParam.Direction = System.Data.ParameterDirection.Input;
                    nameParam.Value = name;

                    cmd.Parameters.Add(nameParam);
                    cmd.Prepare();

                    using (DbDataReader reader = cmd.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();  // Advance the reader to the first row.
                            uuid = reader.GetString(0);
                        }
                    }

                    // Add the result to the cache.
                    uuidCache.Add(name, uuid);
                }
            }
            catch (MySqlException) {
                throw;
            }
            catch (SQLiteException) {
                throw;
            }
            return uuid;
        }

        /// <summary>
        /// Renames the inbox tables.
        /// </summary>
        /// <returns> Whether the process completed without major errors. <seealso cref="System.Boolean"/></returns>
        private static bool RenameInboxes() {
            try {
                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    // This dictionary maps inbox table names to the uuids that they belong to.
                    Dictionary<string, string> inboxToUUIDs = new Dictionary<string, string>();

                    // Obtain a list of tables that serve as player inboxes.
                    using (DbCommand cmd = conn.CreateCommand()) {
                        Console.WriteLine("Generating list of inbox tables...");
                        cmd.CommandText = usingMySQL ? "SHOW TABLES LIKE 'inbox%'" :
                                          "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'inbox%'";

                        using (DbDataReader reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                string inboxTable = reader.GetString(0);
                                if (!inboxToUUIDs.ContainsKey(inboxTable)) {
                                    inboxToUUIDs.Add(inboxTable, String.Empty);
                                }
                            }
                        }
                    }

                    // Start a transaction since a lot of changes to the database will be done in a row.
                    using (DbTransaction trans = conn.BeginTransaction()) {
                        Console.WriteLine("Obtaining uuids for each user with an inbox...");
                        // A collection that's being iterated through cannot be altered, so copy the names
                        // to an array and iterate through that instead.
                        string[] inboxTables = new string[inboxToUUIDs.Count];
                        inboxToUUIDs.Keys.CopyTo(inboxTables, 0);

                        foreach (string inbox in inboxTables) {
                            // The call to Substring removes the "inbox" prefix.
                            inboxToUUIDs[inbox] = FetchUUIDFromTable(inbox.Substring(5), conn);
                        }

                        Console.WriteLine("Altering inbox tables...");

                        if (dropInvalidInboxes) {
                            Console.WriteLine("--drop-bad-inboxes parameter was given. Dropping inboxes with no uuids.");
                        }

                        foreach (string table in inboxToUUIDs.Keys) {
                            string oldName;
                            // Escape the player name since it might not even be valid due to lack of
                            // sanity checking back in the MCLawl and MCForge days. We can't use prepared 
                            // statements for table names, so it has to be done this way. This sucks.
                            if (usingMySQL) {
                                oldName = MySqlHelper.EscapeString(table);
                            }
                            else {  // I don't see an easy way to do it for SQLite.
                                oldName = table.Replace("\"", "\"\"");
                            }

                            // Rename the table if it represents a valid player name.
                            if (inboxToUUIDs[table] != String.Empty) {
                                // The new table will include the player's uuid.
                                string newName = "inbox_" + inboxToUUIDs[table];

                                using (DbCommand cmd = conn.CreateCommand()) {
                                    // MySQL doesn't support double quotes for some reason, so use backticks instead.
                                    cmd.CommandText = usingMySQL ? "ALTER TABLE `" + oldName + "` RENAME TO `" + newName + "`" :
                                                                   "ALTER TABLE \"" + oldName + "\" RENAME TO \"" + newName + "\"";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else if (dropInvalidInboxes) {
                                using (DbCommand cmd = conn.CreateCommand()) {
                                    cmd.CommandText = "DROP TABLE \"" + oldName + "\"";
                                    // MySQL doesn't like double quotes for some reason.
                                    cmd.CommandText = usingMySQL ? "DROP TABLE `" + oldName + "`" :
                                                                   "DROP TABLE \"" + oldName + "\"";
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        // Finalize our changes.
                        trans.Commit();
                    }

                    conn.Close();
                }
            }
            catch (MySqlException) {
                throw;
            }
            catch (SQLiteException) {
                throw;
            }
            return true;
        }

        /// <summary>
        /// Converts the copy database used by /store and /retrieve to use uuids.
        /// </summary>
        private static void ConvertCopyDatabase() {
            string copyDb = Path.Combine(workingDir, "extra", "copy", "index.copydb");

            try {
                if (!File.Exists(copyDb)) {
                    Console.WriteLine("The copy database does not exist. Skipping.");
                    return;
                }

                // Back up the file in case something goes wrong.
                File.Copy(copyDb, copyDb + ".backup");

                string[] lines = File.ReadAllLines(copyDb);
                // Create() overwrites the old file if it exists.
                using (StreamWriter newFile = new StreamWriter(File.Create(copyDb))) {
                    using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                        // Replace the username with the uuid on each line.
                        foreach (string line in lines) {
                            string[] split = line.Split(' ');
                            string file = split[0].Trim();
                            string username = split[1].Trim();

                            string uuid = FetchUUIDFromTable(username, conn);

                            if (uuid != String.Empty) {
                                // Unpaid users have a tilde appended to their name. This is to distinguish
                                // them from users that may change their name to this one after the name
                                // changes take effect.
                                if (uuid.StartsWith("unpaid")) {
                                    username = username + "~";
                                }

                                // Add the username for human readability purposes.
                                newFile.WriteLine(file + " " + uuid + " " + username);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while opening or reading the copy database. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Given a directory, renames all of its subdirectories from player names to the corresponding uuids.
        /// </summary>
        /// <param name="basePath"> The full path of the directory where the subdirectories to be renamed
        /// are located. <seealso cref="System.String"/></param>
        private static void ConvertFolderNames(string basePath) {
            try {
                string[] folderPaths = Directory.GetDirectories(basePath);

                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    foreach (string path in folderPaths) {
                        // Crappy hack to get the rightmost folder name.
                        DirectoryInfo di = new DirectoryInfo(path);
                        string username = di.Name;

                        string uuid = FetchUUIDFromTable(username, conn);

                        if (uuid != String.Empty) {
                            // The new folder's name is simply the uuid.
                            string newPath = Path.Combine(basePath, uuid);

                            Directory.Move(path, newPath);
                            // Write a file that contains only the player's name. This is so anyone that has
                            // to look in the folder for some reason will know the username that the files
                            // within the folder belong to.
                            string nameFilePath = Path.Combine(newPath, "name.txt");
                            using (StreamWriter nameFile = new StreamWriter(File.Create(nameFilePath))) {
                                // Unpaid users have a tilde appended to their name. This is to distinguish
                                // them from users that may change their name to this one after the name
                                // changes take effect.
                                if (uuid.StartsWith("unpaid")) {
                                    username = username + "~";
                                }

                                nameFile.WriteLine(username);
                            }
                        }
                    }
                }
            }
            catch (Exception) {  // The caller should handle it.
                throw;
            }
        }

        /// <summary>
        /// Given a directory, renames all of its files from player names to the corresponding uuids.
        /// </summary>
        /// <param name="basePath"> The full path of the directory where the files to be renamed
        /// are located. <seealso cref="System.String"/></param>
        private static void ConvertFileNames(string basePath) {
            try {
                string[] filePaths = Directory.GetFiles(basePath);

                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    foreach (string path in filePaths) { 
                        // Get the directory, the filename, and its extension separately.
                        string directory = Path.GetDirectoryName(path);
                        string username = Path.GetFileNameWithoutExtension(path);
                        string extension = Path.GetExtension(path);

                        string uuid = FetchUUIDFromTable(username, conn);

                        if (uuid != String.Empty) {
                            // Unpaid users have a tilde appended to their name. This is to distinguish
                            // them from users that may change their name to this one after the name
                            // changes take effect.
                            if (uuid.StartsWith("unpaid")) {
                                username = username + "~";
                            }

                            // Back up the file, just in case.
                            File.Copy(path, path + ".backup");
                            // Rename the file afterwards.
                            File.Move(path, Path.Combine(directory, uuid + extension));
                        }
                    }
                }
            }
            catch (Exception) {  // The caller should handle it.
                throw;
            }
        }

        /// <summary>
        /// Rename the folders used by /copy load and /copy save.
        /// </summary>
        private static void RenameSaveCopyFolders() {
            string copyFolder = Path.Combine(workingDir, "extra", "savecopy");

            try {
                if (!Directory.Exists(copyFolder)) {
                    Console.WriteLine("The extra/savecopy/ folder does not exist. Skipping.");
                }
                else {
                    ConvertFolderNames(copyFolder);
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working in the savecopy folder. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames the folders in the extra/undo and extra/undoPrevious directories.
        /// </summary>
        private static void RenameUndoFolders() {
            string undoFolder = Path.Combine(workingDir, "extra", "undo");
            string undoPreviousFolder = Path.Combine(workingDir, "extra", "undoPrevious");

            try {
                if (!Directory.Exists(undoFolder)) {
                    Console.WriteLine("The extra/undo/ directory does not exist. Skipping.");
                }
                else {
                    ConvertFolderNames(undoFolder);
                }

                if (!Directory.Exists(undoPreviousFolder)) {
                    Console.WriteLine("The extra/undoPrevious/ directory does not exist. Skipping.");
                }
                else {
                    ConvertFolderNames(undoPreviousFolder);
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working in the undo folders. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames files in extra/passwords.
        /// </summary>
        private static void RenamePasswordFiles() {
            string passwordsFolder = Path.Combine(workingDir, "extra", "passwords");

            try {
                if (!Directory.Exists(passwordsFolder)) {
                    Console.WriteLine("The extra/passwords directory does not exist. Skipping.");
                }
                else {
                    // Rename the files so that they are no longer recognized by MCHmk.
                    // MCHmk r17+ now uses the uuid as the salt, so these passwords files no longer work.
                    string[] filePaths = Directory.GetFiles(passwordsFolder);
                    foreach (string path in filePaths) {
                        File.Move(path, path + ".old");
                    }
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working in the password folder. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Renames files in extra/Waypoints.
        /// </summary>
        private static void RenameWaypointFiles() {
            string waypointsFolder = Path.Combine(workingDir, "extra", "Waypoints");

            try {
                if (!Directory.Exists(waypointsFolder)) {
                    Console.WriteLine("The extra/passwords directory does not exist. Skipping.");
                }
                else {
                    ConvertFileNames(waypointsFolder);
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working in the waypoints folder. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Converts the files that store the names of ranked users to use uuids instead.
        /// </summary>
        private static void ConvertRankFiles() {
            // These are files in the rank folder that do not have rank-related information.
            HashSet<string> excluded = new HashSet<string>() 
                {"bots.txt", "banned-ip.txt", "IRC_Controllers.txt", "known-ips.txt"};
            string ranksFolder = Path.Combine(workingDir, "ranks");
            SHA1Managed sha1 = new SHA1Managed();

            try {
                while (!Directory.Exists(ranksFolder)) {
                    Console.WriteLine("The ranks directory does not exist.");
                    Console.WriteLine("Please make sure that it exists, then press Enter to try again.");
                    Console.WriteLine();
                    ContinuePrompt();
                }
               
                // Get all the files in the folder.
                string[] filePaths = Directory.GetFiles(ranksFolder);

                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    // Don't work on the files that have been excluded.
                    foreach (string path in filePaths) {
                        string filename = Path.GetFileName(path);

                        if (excluded.Contains(filename)) {
                            continue;
                        }

                        // Back up the file, just in case.
                        File.Copy(path, path + ".backup");

                        // Read all the lines of the file.
                        Console.WriteLine("Updating " + filename + "...");
                        string[] lines = File.ReadAllLines(path);

                        // Create() overwrites the old file if it exists.
                        using (StreamWriter writer = new StreamWriter(File.Create(path))) {
                            foreach (string line in lines) {
                                string username = line.Trim();
                                string uuid = FetchUUIDFromTable(username, conn);

                                // If there is no uuid, then it must be a manually ranked or banned name for an offline
                                // player that has never entered the server. In this case, create a uuid which consists
                                // of the prefix "manual_" combined with the SHA-1 hash of the player's name.
                                if (uuid == String.Empty && !purgeManualRanks) {
                                    byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(username));
                                    StringBuilder temp = new StringBuilder();

                                    temp.Append("manual_"); 
                                    for (int i = 0; i < hash.Length; i++) {
                                        temp.Append(hash[i].ToString("x2"));
                                    }

                                    uuid = temp.ToString();
                                }
                                // Unpaid users have a tilde appended to their name. This is to distinguish
                                // them from users that may change their name to this one after the name
                                // changes take effect.
                                else if (uuid.StartsWith("unpaid")) {
                                    username = username + "~";
                                }

                                // Add the username for human readability purposes.
                                if (uuid != String.Empty) {
                                    writer.WriteLine(uuid + " " + username);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working in the ranks folder. Skipping.");
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Converts the text/playerAwards.txt file to use uuids instead. 
        /// </summary>
        private static void ConvertPlayerAwards() {
            string playerAwardsFile = Path.Combine(workingDir, "text", "playerAwards.txt");

            try {
                if (!File.Exists(playerAwardsFile)) {
                    Console.WriteLine("The player awards file does not exist. Skipping.");
                    return;
                }

                // Read and back up the file.
                string[] lines = File.ReadAllLines(playerAwardsFile);
                File.Copy(playerAwardsFile, playerAwardsFile + ".backup");

                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    // Create() overwrites the old file if it exists.
                    using (StreamWriter writer = new StreamWriter(File.Create(playerAwardsFile))) {
                        foreach (string line in lines) {
                            // Replace the username with the uuid on each line.
                            string[] split = line.Split(new string[] { " : " }, StringSplitOptions.None);

                            if (split.Length != 2) {
                                continue;
                            }

                            string username = split[0].Trim();
                            string awards = split[1].Trim();
                            string uuid = FetchUUIDFromTable(username, conn);

                            if (uuid != String.Empty) {
                                // Unpaid users have a tilde appended to their name. This is to distinguish
                                // them from users that may change their name to this one after the name
                                // changes take effect.
                                if (uuid.StartsWith("unpaid")) {
                                    username = username + "~";
                                }

                                // Add the username for human readability purposes.
                                writer.WriteLine(uuid + " : " + username + " : " + awards);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while working with the player awards file. Skipping.");
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Converts the text/emotelist.txt file to use uuids instead.
        /// </summary>
        private static void ConvertEmoteList() {
            string emoteListFile = Path.Combine(workingDir, "text", "emotelist.txt");

            try {
                if (!File.Exists(emoteListFile)) {
                    Console.WriteLine("The emotelist.txt file does not exist. Skipping.");
                    return;
                }

                // Back up the file in case something goes wrong.
                File.Copy(emoteListFile, emoteListFile + ".backup");

                string[] lines = File.ReadAllLines(emoteListFile);
                // Create() overwrites the old file if it exists.
                using (StreamWriter newFile = new StreamWriter(File.Create(emoteListFile))) {
                    using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                        // Replace the username with the uuid on each line.
                        foreach (string line in lines) {
                            string username = line.Trim();
                            string uuid = FetchUUIDFromTable(username, conn);

                            if (uuid != String.Empty) {
                                // Unpaid users have a tilde appended to their name. This is to distinguish
                                // them from users that may change their name to this one after the name
                                // changes take effect.
                                if (uuid.StartsWith("unpaid")) {
                                    username = username + "~";
                                }

                                // Add the username for human readability purposes.
                                newFile.WriteLine(uuid + " " + username);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                if (e is PathTooLongException || e is UnauthorizedAccessException || 
                    e is FileNotFoundException || e is IOException) {
                    Console.WriteLine("An error occurred while reading the emotelist.txt file. Skipping.");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Appends a tilde to each user in the database that has not paid for Minecraft in both
        /// the players and uuids table.
        /// </summary>
        /// <remarks>
        /// This is done so that players who claim those names in the future will not been seen as
        /// the same player that may have played in a server a long time ago. A tilde is used since
        /// it is not a valid character in a Minecraft username. This step is done last because the
        /// files that were converted earlier do not use this convention, so the files should be
        /// converted to use it before the database does.
        /// </remarks>
        /// <returns> Whether the operation completed without any major errors.
        /// <seealso cref="System.Boolean"/></returns>
        private static bool MarkUnpaidUsersInDatabase() {
            try {
                using (DbConnection conn = OpenSQLConnection(usingMySQL, connString)) {
                    // Start a transaction since a lot of changes to the database will be done in a row.
                    using (DbTransaction trans = conn.BeginTransaction()) {
                        List<string> unpaidUsers = new List<string>();

                        // Get a list of users whose uuids start with the "unpaid_" prefix.
                        Console.WriteLine("Generating list of unpaid users...");
                        using (DbCommand cmd = conn.CreateCommand()) {
                            cmd.CommandText = "SELECT name FROM uuids WHERE uuid LIKE 'unpaid%'";

                            using (DbDataReader reader = cmd.ExecuteReader()) {
                                while (reader.Read()) {
                                    unpaidUsers.Add(reader.GetString(0));
                                }
                            }
                        }

                        // Add a tilde to each unpaid user's name in the uuids table.
                        Console.WriteLine("Updating the uuids table...");
                        using (DbCommand cmd = conn.CreateCommand()) {
                            cmd.CommandText = "UPDATE uuids SET name = @newname WHERE name = @oldname";

                            DbParameter oldParam = cmd.CreateParameter();
                            oldParam.ParameterName = "@oldname";
                            oldParam.DbType = System.Data.DbType.String;
                            oldParam.Direction = System.Data.ParameterDirection.Input;

                            DbParameter newParam = cmd.CreateParameter();
                            newParam.ParameterName = "@newname";
                            newParam.DbType = System.Data.DbType.String;
                            newParam.Direction = System.Data.ParameterDirection.Input;

                            cmd.Parameters.Add(oldParam);
                            cmd.Parameters.Add(newParam);
                            cmd.Prepare();

                            for (int i = 0; i < unpaidUsers.Count; i++) {
                                string oldName = unpaidUsers[i];
                                string newName = oldName + "~";

                                cmd.Parameters["@oldname"].Value = oldName;
                                cmd.Parameters["@newname"].Value = newName;
                                cmd.ExecuteNonQuery();

                                if ((i + 1) % 1000 == 0) {
                                    Console.WriteLine((i + 1) + "/" + unpaidUsers.Count + " records updated.");
                                }
                            }
                        }

                        // Do the same for the players table.
                        Console.WriteLine("Updating the players table...");
                        using (DbCommand cmd = conn.CreateCommand()) {
                            cmd.CommandText = "UPDATE players SET name = @newname WHERE name = @oldname";

                            DbParameter oldParam = cmd.CreateParameter();
                            oldParam.ParameterName = "@oldname";
                            oldParam.DbType = System.Data.DbType.String;
                            oldParam.Direction = System.Data.ParameterDirection.Input;

                            DbParameter newParam = cmd.CreateParameter();
                            newParam.ParameterName = "@newname";
                            newParam.DbType = System.Data.DbType.String;
                            newParam.Direction = System.Data.ParameterDirection.Input;

                            cmd.Parameters.Add(oldParam);
                            cmd.Parameters.Add(newParam);
                            cmd.Prepare();

                            for (int i = 0; i < unpaidUsers.Count; i++) {
                                string oldName = unpaidUsers[i];
                                string newName = oldName + "~";

                                cmd.Parameters["@oldname"].Value = oldName;
                                cmd.Parameters["@newname"].Value = newName;
                                cmd.ExecuteNonQuery();

                                if ((i + 1) % 1000 == 0) {
                                    Console.WriteLine((i + 1) + "/" + unpaidUsers.Count + " records updated.");
                                }
                            }
                        }

                        Console.WriteLine("Finalizing changes...");
                        trans.Commit();
                    }
                }
            }
            catch (MySqlException) {
                throw;
            }
            catch (SQLiteException) {
                throw;
            }
            return true;
        }

        /// <summary>
        /// The entry point of the program.
        /// </summary>
        /// <param name="args"> The command line arguments. <seealso cref="System.String"/></param>
        public static void Main(string[] args) {
            ParseArgs(args);
            PrintIntro();

            Console.WriteLine("Determining SQL settings...");
            if (!FindSQLSettings()) {
                return;
            }
            SetConnectionString();

            Console.WriteLine("Connecting to the SQL database...");

            if (skipFetching) {
                Console.WriteLine("--skip-fetching parameter given. Assuming that uuids table is valid.");
            }
            else if (!FetchAllUUIDs()) {
                return;
            }

            if (skipInbox) {
                Console.WriteLine("--skip-inbox parameter given. Skipping inbox tables.");
            }
            else if (!RenameInboxes()) {
                return;
            }

            if (!skipExtra) {
                Console.WriteLine("Updating files and folders in the extra directory...");

                Console.WriteLine("Converting extra/copy/index.copydb...");
                ConvertCopyDatabase();
                Console.WriteLine("Renaming folder names in extra/savecopy...");
                RenameSaveCopyFolders();
                Console.WriteLine("Renaming folder names in extra/undo and extra/undoPrevious directories...");
                RenameUndoFolders();
                Console.WriteLine("Renaming files in extra/passwords...");
                RenamePasswordFiles();
                Console.WriteLine("Renaming files in extra/Waypoints...");
                RenameWaypointFiles();
            }
            else {
                Console.WriteLine("--skip-extra parameter given. Skipping the extra directory.");
            }

            if (!skipRanks) {
                Console.WriteLine("Updating files in the ranks directory...");
                if (purgeManualRanks) {
                    Console.WriteLine("--purge-manual-ranks parameter given. Deleting users with no uuids.");
                }
                ConvertRankFiles();
            }
            else {
                Console.WriteLine("--skip-ranks parameter given. Skipping the ranks directory.");
            }

            if (!skipText) {
                Console.WriteLine("Updating text/emotelist.txt...");
                ConvertEmoteList();
                Console.WriteLine("Updating text/playerAwards.txt...");
                ConvertPlayerAwards();
            }
            else {
                Console.WriteLine("--skip-text parameter given. Skipping the text directory.");
            }

            Console.WriteLine("Marking unpaid users in database...");
            if (!MarkUnpaidUsersInDatabase()) {
                return;
            }

            Console.WriteLine("Everything is done! You may now update to MCHmk r17.");
            Console.WriteLine("Please note that admin passwords must be set again.");
        }
    }
}

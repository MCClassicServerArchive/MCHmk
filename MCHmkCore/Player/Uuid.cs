/*
    Copyright 2016 Jjp137

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
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

using MCHmk.SQL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCHmk {
    /// <summary>
    /// The Uuid class represents a player's unique identifier, and it also contains methods for obtaining
    /// a player's uuid.
    /// </summary>
    public class Uuid : IEquatable<Uuid> {
        /// <summary>
        /// A cache of usernames and their uuids. This cache helps avoid excessive requests to the SQL
        /// database and online servers.
        /// </summary>
        private static Dictionary<string, Uuid> cache = new Dictionary<string, Uuid>();
        /// <summary>
        /// A cache of usernames and manually generated uuids. This cache helps save time generating
        /// uuids for the same names over and over.
        /// </summary>
        private static Dictionary<string, Uuid> manualCache = new Dictionary<string, Uuid>();

        /// <summary>
        /// The Uuid object that contains an empty uuid, which doesn't have a valid value.
        /// </summary>
        private static Uuid _empty = new Uuid(String.Empty);

        /// <summary>
        /// Gets the Uuid object that contains an empty uuid, which doesn't have a valid value.
        /// </summary>
        public static Uuid Empty {
            get {
                return _empty;
            }
        }

        /// <summary>
        /// The uuid itself.
        /// </summary>
        private string _value;

        /// <summary>
        /// Gets the uuid itself.
        /// </summary>
        public string Value {
            get {
                return _value;
            }
        }

        /// <summary>
        /// Gets whether this uuid represents a valid value.
        /// </summary>
        public bool IsValid {
            get {
                return this != Uuid.Empty;
            }
        }

        /// <summary>
        /// Constructs a new Uuid object.
        /// </summary>
        /// <param name="uuid"> The uuid. </param>
        /// <exception cref="ArgumentException"> Thrown if neither a valid uuid nor the empty string is
        /// given as an argument. </exception>
        private Uuid(string uuid) {
            if (uuid.Length != 0 && !Uuid.IsLikelyUuid(uuid)) {
                throw new ArgumentException("uuid doesn't seem to be a valid uuid and isn't the empty string.");
            }

            this._value = uuid;
        }

        /// <summary>
        /// Returns a string that represents this Uuid object.
        /// </summary>
        /// <returns> The string that represents this Uuid object.</returns>
        public override string ToString() {
            return this._value;
        }

        /// <summary>
        /// Determines whether the given Uuid object is equal to this one.
        /// </summary>
        /// <param name="other"> The Uuid object to compare to. <seealso cref="Uuid"/></param>
        /// <returns> Whether both Uuid objects are equal. </returns>
        public bool Equals(Uuid other) {
            if (other == null) {
                return false;
            }

            return this._value == other._value;
        }

        /// <summary>
        /// Determines whether the given object is equal to this Uuid object.
        /// </summary>
        /// <param name="obj"> The object to compare to. </param>
        /// <returns> Whether both objects are equal. </returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }

            Uuid uuid = obj as Uuid;
            if (uuid == null) {
                return false;
            }
            else {
                return Equals(uuid);
            }
        }

        /// <summary>
        /// Calculates a hash code for this Uuid object and returns it.
        /// </summary>
        /// <returns> A hash code representing this Uuid object. </returns>
        public override int GetHashCode() {
            return this._value.GetHashCode();
        }

        /// <summary>
        /// Checks two Uuid objects for equality.
        /// </summary>
        /// <param name="first"> The first Uuid object. </param>
        /// <param name="second"> The second Uuid object. </param>
        /// <returns> Whether both Uuid objects are equal. </returns> 
        public static bool operator == (Uuid first, Uuid second) {
            if ((object)first == null || (object)second == null) {
                return Object.Equals(first, second);
            }
            return first.Equals(second);
        }

        /// <summary>
        /// Checks two Uuid objects for inequality.
        /// </summary>
        /// <param name="first"> The first Uuid object. </param>
        /// <param name="second"> The second Uuid object. </param>
        /// <returns> Whether both Uuid objects are unequal. </returns> 
        public static bool operator != (Uuid first, Uuid second) {
            if ((object)first == null || (object)second == null) {
                return !Object.Equals(first, second);
            }
            return !(first.Equals(second));
        }

        /// <summary>
        /// Finds the uuid of the player with the given name.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="username"> The name of a player. </param>
        /// <returns> The uuid of the player, or an empty string if no uuid exists for that player. 
        /// <seealso cref="Uuid"/></returns>
        public static Uuid FindUuid(Database db, string username) {
            if (String.IsNullOrEmpty(username)) {
                throw new ArgumentException("username can't be empty in FindUuid().");
            }

            // Obtain the uuid from the cache if the uuid for this player has already been
            // looked up once before.
            if (cache.ContainsKey(username)) {
                return cache[username];
            }

            // The pattern after the LIKE keyword is case-insensitive, at least on SQLite.
            // It's a gross hack to force case-insensitive matches, and there's several ways
            // that it could fall apart.
            string query = "SELECT uuid FROM uuids WHERE name LIKE @name";

            // Query the SQL database and find the uuid for that player, if it exists.
            using (PreparedStatement stmt = db.MakePreparedStatement(query)) {
                stmt.AddParam("@name", username);

                using (DataTable result = stmt.ObtainData()) {
                    if (result.Rows.Count > 0) {
                        string value = result.Rows[0]["uuid"].ToString();
                        Uuid uuid = new Uuid(value);

                        // Don't cache uuids of minecraft.net usernames fetched from the database since they might
                        // change while those players are offline. It is safe to cache unpaid and ClassiCube players,
                        // though, so cache their uuids.
                        if (username.EndsWith("~") || username.EndsWith("+")) {
                            cache.Add(username, uuid);
                        }

                        return uuid;
                    }
                    else {
                        return Uuid.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the name of the player with the given uuid.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="uuid"> The uuid of a player. <seealso cref="Uuid"/></param>
        /// <returns> The name of the player with the uuid, or an empty string if there is no
        /// player associated with that uuid. </returns>
        public static string FindName(Database db, Uuid uuid) {
            if (!uuid.IsValid) {
                throw new ArgumentException("uuid can't be invalid in FindName().");
            }

            // Query the SQL database and find the name of the player with the given uuid,
            // if it exists.
            string query = "SELECT name FROM uuids WHERE uuid = @uuid";

            using (PreparedStatement stmt = db.MakePreparedStatement(query)) {
                stmt.AddParam("@uuid", uuid.Value);

                using (DataTable result = stmt.ObtainData()) {
                    if (result.Rows.Count > 0) {
                        return result.Rows[0]["name"].ToString();
                    }
                    else {
                        return String.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to find a uuid for the player with given name, and if no uuid exists for
        /// that player, generate a temporary uuid for that player instead.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="username"> The name of the player. </param>
        /// <returns> A uuid associated with the player. <seealso cref="Uuid"/></returns>
        public static Uuid FindWithFallback(Database db, string username) {
            Uuid uuid = FindUuid(db, username);

            if (!uuid.IsValid) {
                uuid = GenerateTemporaryUuid(username);
            }
            return uuid;
        }

        /// <summary>
        /// Determine whether the given uuid is present in the datbase.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="uuid"> The uuid to check for. <seealso cref="Uuid"/></param>
        /// <returns> Whether the uuid exists in the database. </returns>
        public static bool UuidExists(Database db, Uuid uuid) {
            if (!uuid.IsValid) {
                throw new ArgumentException("uuid can't be invalid in UuidExists().");
            }

            return !String.IsNullOrEmpty(FindName(db, uuid));
        }

        /// <summary>
        /// Changes the name associated with the given uuid.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <param name="username"> The new username of the player. </param>
        internal static void ChangeName(Database db, Uuid uuid, string username) {
            if (!uuid.IsValid) {
                throw new ArgumentException("uuid can't be invalid in ChangeName().");
            }
            else if (String.IsNullOrEmpty(username)) {
                throw new ArgumentException("username can't be empty in ChangeName().");
            }

            // Make sure a name with that uuid exists in the database.
            string oldName = Uuid.FindName(db, uuid);
            if (String.IsNullOrEmpty(oldName)) {
                throw new ArgumentException("old name was not found.");
            }

            // Update the uuids table.
            string query = "UPDATE uuids SET name = @newname WHERE uuid = @uuid";
            using (PreparedStatement stmt = db.MakePreparedStatement(query)) {
                stmt.AddParam("@uuid", uuid.Value);
                stmt.AddParam("@name", username);

                using (TransactionHelper trans = stmt.BeginTransaction()) {
                    stmt.Execute();
                    trans.CommitOrRollback();
                }
            }

            // Update the players table.
            query = "UPDATE players SET name = @newname WHERE name = @oldname";
            using (PreparedStatement stmt = db.MakePreparedStatement(query)) {
                stmt.AddParam("@oldname", oldName);
                stmt.AddParam("@newname", username);

                using (TransactionHelper trans = stmt.BeginTransaction()) {
                    stmt.Execute();
                    trans.CommitOrRollback();
                }
            }
        }

        /// <summary>
        /// Obtain the uuid of a player logged into the server on a minecraft.net account.
        /// </summary>
        /// <param name="username"> The name of a player logged into their minecraft.net account. </param>
        /// <returns> The uuid obtained from Mojang's API, or an empty string if an error occurred.
        /// <seealso cref="Uuid"/></returns>
        internal static Uuid GetMojangUuid(string username) {
            if (String.IsNullOrEmpty(username)) {
                throw new ArgumentException("username can't be empty in GetMojangUuid().");
            }

            // Obtain the uuid from the cache if it has already been looked up before.
            // This saves time waiting for a response from Mojang's servers.
            if (cache.ContainsKey(username)) {
                return cache[username];
            }

            // Make an HTTP request to Mojang's API.
            string response = String.Empty;
            try {
                using (WebClient client = new WebClient()) {
                    client.Headers.Add("Content-Type", "application/json");  // This header is required.
                    response = client.DownloadString("https://api.mojang.com/users/profiles/minecraft/" + username);
                }
            }
            catch (WebException) {
                return Uuid.Empty;
            }

            if (String.IsNullOrEmpty(response)) {
                return Uuid.Empty;
            }
            // Parse the JSON object and obtain the uuid.
            else {
                string value = JObject.Parse(response).Value<string>("id");
                Uuid uuid = new Uuid(value);

                // The lifetime of a minecraft.net player's uuid being the same is 37 days.
                cache.Add(username, uuid);
                return uuid;
            }
        }

        /// <summary>
        /// Obtain the uuid of a user logged in with their ClassiCube account.
        /// </summary>
        /// <param name="username"> The name of a player logged into their ClassiCube account. </param>
        /// <returns> The uuid formed using information from ClassiCube's API, or an empty string 
        /// if an error occurred. <seealso cref="Uuid"/></returns>
        internal static Uuid GetClassiCubeUuid(string username) {
            if (String.IsNullOrEmpty(username)) {
                throw new ArgumentException("username can't be empty in GetClassiCubeUuid().");
            }

            // MCHmk separates ClassiCube usernames from minecraft.net ones by adding a plus sign to
            // the end of ClassiCube usernames.
            string internalName = username + "+";

            // Obtain the uuid from the cache if it has already been looked up before.
            // This saves time waiting for a response from ClassiCube's servers.
            if (cache.ContainsKey(internalName)) {
                return cache[internalName];
            }

            // Make an HTTP request to ClassiCube's API.
            string response = String.Empty;
            try {
                using (WebClient client = new WebClient()) {
                    response = client.DownloadString("http://www.classicube.net/api/player/" + username.ToLower());
                }
            }
            catch (WebException) {
                return Uuid.Empty;
            }

            // Obtain the needed information from the given response.
            string id = JObject.Parse(response).Value<string>("id");
            string registered = JObject.Parse(response).Value<string>("registered");

            if (String.IsNullOrEmpty(id)) {
                return Uuid.Empty;
            }
            // A normal "uuid" for a ClassiCube user is the below prefix combined with the id and the Unix
            // timestamp of the time that the user registered.
            else {
                string value = "classicube_" + id + "_" + registered;
                Uuid uuid = new Uuid(value);

                // ClassiCube usernames will never be changed, only deleted, so cache the result.
                cache.Add(internalName, uuid);
                return uuid;
            }
        }

        /// <summary>
        /// Generate a temporary uuid for a player with the given name. This uuid should only be
        /// used until an actual uuid is generated using information from an API.
        /// </summary>
        /// <param name="username"> The name of the player. </param>
        /// <returns> A temporary uuid for that player. <seealso cref="Uuid"/></returns>
        internal static Uuid GenerateTemporaryUuid(string username) {
            if (String.IsNullOrEmpty(username)) {
                throw new ArgumentException("username can't be empty in GenerateManualUuid().");
            }

            // Make the name lowercase to ensure case-insensitivity.
            string nameToHash = username.ToLower();

            // Don't waste time calculating a hash if a hash for this player exists.
            if (manualCache.ContainsKey(nameToHash)) {
                return manualCache[nameToHash];
            }

            // Otherwise, generate the SHA-1 hash using the player's name.
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(nameToHash));
            StringBuilder temp = new StringBuilder();

            // Temporary uuids are prefixed with "manual_" to indicate that they were
            // manually assigned by MCHmk.
            temp.Append("manual_"); 
            for (int i = 0; i < hash.Length; i++) {
                temp.Append(hash[i].ToString("x2"));
            }

            // Cache the uuid so that time is not spent hashing the player's name again.
            Uuid uuid = new Uuid(temp.ToString());
            manualCache.Add(nameToHash, uuid);

            // Dispose the managed SHA-1 hasher and return the uuid.
            sha1.Dispose();
            return uuid;
        }

        /// <summary>
        /// Create the uuids table in the SQL database.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <returns> Whether the uuids table was created successfully. </returns>
        internal static bool CreateUuidTable(Database db) {
            string autoInc = (db.Engine == SQLEngine.MySQL) ? "AUTO_INCREMENT" : "AUTOINCREMENT";  // Yes, really.
            db.ExecuteStatement("CREATE TABLE IF NOT EXISTS uuids (" +
                                "id INTEGER NOT NULL PRIMARY KEY " + autoInc + ", " +
                                "uuid VARCHAR(64) NOT NULL UNIQUE, " +
                                "name VARCHAR(20) NOT NULL)");
            return true;
        }

        /// <summary>
        /// Adds a username and the uuid associated with it to the uuids table.
        /// </summary>
        /// <param name="db"> The database to use. <seealso cref="Database"/></param>
        /// <param name="uuid"> The uuid of the player. <seealso cref="Uuid"/></param>
        /// <param name="username"> The name of the player. </param>
        /// <returns> Whether the operation was successful. </returns>
        internal static bool AddUuidToTable(Database db, Uuid uuid, string username) {
            // Add the username and the uuid to the uuids table.
            string query = "INSERT INTO uuids (name, uuid) VALUES (@name, @uuid)";

            using (PreparedStatement stmt = db.MakePreparedStatement(query)) {
                stmt.AddParam("@name", username);
                stmt.AddParam("@uuid", uuid.Value);

                using (TransactionHelper trans = stmt.BeginTransaction()) {
                    stmt.Execute();
                    trans.CommitOrRollback();
                }
            }

            // The cache entry is outdated now, so remove the entry from the list.
            if (cache.ContainsKey(username)) {
                cache.Remove(username);
            }

            return true;
        }


        /// <summary>
        /// Replaces all instances of a player's temporary generated uuid within the server's data with
        /// their real uuid.
        /// </summary>
        /// <param name="s"> The server that the player is playing on. <seealso cref="Server"/></param>
        /// <param name="realUuid"> The player's actual uuid. <seealso cref="Uuid"/></param>
        /// <param name="username"> The player's username. </param>
        internal static void CleanupTemporaryUuid(Server s, Uuid realUuid, string username) {
            // Get the uuid that would be generated for this player.
            Uuid manualUuid = Uuid.GenerateTemporaryUuid(username);

            // If the player was ranked before they ever joined the server, remove the placeholder
            // uuid and insert the real one instead.
            foreach (Rank rank in s.ranks) {
                if (rank.playerList.Contains(manualUuid)) {
                    rank.playerList.Remove(manualUuid);
                    rank.playerList.Add(realUuid, username);
                    rank.playerList.Save();
                    break;  // A player should only belong to one rank.
                }
            }

            // If the player was added to the whitelist manually, remove their placeholder uuid and
            // insert the real one instead.
            if (s.props.useWhitelist && s.whiteList.Contains(manualUuid)) {
                s.whiteList.Remove(manualUuid);
                s.whiteList.Add(realUuid, username);
                s.whiteList.Save();
            }
        }

        /// <summary>
        /// Checks if a string is likely to be a uuid.
        /// </summary>
        /// <remarks>
        /// This method is not foolproof. If a player has a username entirely composed of hexadecimal digits, for
        /// instance, that can pass as a uuid even though it is not. Thus, while this method can be useful for
        /// catching bugs and throwing exceptions, it can't replace due diligence in making sure that a uuid is
        /// being passed instead of a username.
        /// </remarks>
        /// <param name="uuid"> The string to check. </param>
        /// <returns> Whether the string seems like a uuid. </returns>
        private static bool IsLikelyUuid(string uuid) {
            // Uuids should be lengthy.
            if (uuid.Length < 10) {
                return false;
            }

            // A uuid will never have a plus sign in it, so if one is found, it's probably a ClassiCube username.
            if (uuid.Contains("+")) {
                return false;
            }

            // If a uuid has some sort of prefix, like "classicube_", "unpaid_", and "manual_", handle those cases.
            if (uuid.Contains("_")) {
                // If the uuid is for a deleted ClassiCube user, it's probably valid.
                if (uuid.Contains("classicube_deleted_")) {
                    return true;
                }

                // Otherwise, split using the underscore as a delimiter.
                string[] temp = uuid.Split(new char[] {'_'}, 2);
                string prefix = temp[0];
                string rest = temp[1];

                // If the prefix isn't a valid one, then it's not a uuid.
                if (prefix != "classicube" && prefix != "unpaid" && prefix != "manual") {
                    return false;
                }

                // Otherwise, check that the rest of the characters are valid.
                for (int i = 0; i < rest.Length; i++) {
                    // For ClassiCube uuids, valid characters are 0-9 and the underscore. Other uuids
                    // with prefixes can have 0-9, a-f, and the underscore.
                    if (rest[i] <= '0' && rest[i] >= '9' && rest[i] != '_' && 
                       (prefix == "classicube" || (rest[i] <= 'a' && rest[i] >= 'f'))) {
                        return false;
                    }
                }
                // The string seems valid if it reaches this point.
                return true;
            }

            // Otherwise, it might be a minecraft.net uuid. Iterate through the string and make sure that it has only 
            // hexadecimal digits.
            foreach (char c in uuid) {
                if (c <= '0' && c >= '9' && c <= 'a' && c >= 'f') {
                    return false;
                }
            }
            // The string seems like a minecraft.net uuid, so return true.
            return true;
        }

        // TODO: gross hack
        internal static void ParsePlayerList(UuidList list, string path) {
            foreach (string line in File.ReadAllLines(path)) {
                string[] split = line.Split(' ');
                list.Add(new Uuid(split[0]), split[1]);
            }
        }

        // TODO: ugh
        internal static void ParsePlayerList(List<UuidEntry> list, string path) {
            foreach (string line in File.ReadAllLines(path)) {
                string[] split = line.Split(' ');
                list.Add(new UuidEntry(new Uuid(split[0]), split[1]));
            }
        }
    }
}

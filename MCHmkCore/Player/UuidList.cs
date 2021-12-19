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
using System.IO;
using System.Collections.Generic;

namespace MCHmk {
    public class UuidList {
        public string filename;

        // first: uuid, second: player
        private List<UuidEntry> entries = new List<UuidEntry>();

        public int Count {
            get {
                return entries.Count;
            }
        }

        public UuidList() { }

        public void Add(Uuid uuid, string p) {
            entries.Add(new UuidEntry(uuid, p));
        }

        private int FindIndex(Uuid uuid) {
            return entries.FindIndex(delegate(UuidEntry entry) {
                return entry.Uuid == uuid;
            });
        }

        public bool Remove(Uuid uuid) {
            if (!uuid.IsValid) {
                return false;
            }

            int index = FindIndex(uuid);

            if (index != -1) { 
                entries.RemoveAt(index);
                return true;
            }
            else {
                return false;
            }
        }

        public bool Contains(Uuid uuid) {
            if (!uuid.IsValid) {
                return false;
            }

            return FindIndex(uuid) != -1;
        }

        public List<UuidEntry> All() {
            return new List<UuidEntry>(entries);
        }

        public void Save() {
            Save(filename);
        }

        public void Save(string path) {
            StreamWriter file = File.CreateText(Path.Combine("ranks", path));
            entries.ForEach(delegate(UuidEntry entry) {
                file.WriteLine(entry.Uuid + " " + entry.Name);
            });
            file.Close();
        }

        public static UuidList Load(string file, Rank groupName) {
            if (!Directory.Exists("ranks")) {
                Directory.CreateDirectory("ranks");
            }
            string path = Path.Combine("ranks", file);
            UuidList list = new UuidList();
            list.filename = file;
            if (File.Exists(path)) {
                Uuid.ParsePlayerList(list, path);
            }
            else {
                File.Create(path).Close();
            }
            return list;
        }
    }

    // TODO: Implement IEquatable later
    public class UuidEntry {
        private Tuple<Uuid, string> _entry;

        public Uuid Uuid {
            get { return _entry.Item1; }
        }

        public string Name {
            get { return _entry.Item2; }
        }

        public UuidEntry(Uuid uuid, string name) {
            this._entry = new Tuple<Uuid, string>(uuid, name.ToLower());  // Store names in lowercase
        }

        public override string ToString() {
            return _entry.Item1 + " " + _entry.Item2;
        }
    }
}
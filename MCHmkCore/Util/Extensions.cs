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

	Author: fenderrock87

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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MCHmk {
    public static class Extensions {
        public static byte[] GZip(this byte[] bytes) {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
                gs.Write(bytes, 0, bytes.Length);
                gs.Close();
                ms.Position = 0;
                bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);
                ms.Close();
                ms.Dispose();
            }
            return bytes;
        }

        public static byte[] Decompress(this byte[] gzip) {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress)) {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream()) {
                    int count = 0;
                    do {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0) {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        public static string MCCharFilter(this string str) {
            // Allowed chars are any ASCII char between 20h/32 and 7Dh/125 inclusive, except for 26h/38 (&) and 60h/96 (`)
            str = Regex.Replace(str, @"[^\u0000-\u007F]", "");

            if (String.IsNullOrEmpty(str.Trim())) {
                return str;
            }

            StringBuilder sb = new StringBuilder();

            foreach (char b in Encoding.ASCII.GetBytes(str)) {
                if (b != 38 && b != 96 && b >= 32 && b <= 125) {
                    sb.Append(b);
                }
                else {
                    sb.Append("*");
                }
            }

            return sb.ToString();
        }

        public static void DeleteLineWord(string file, string word) {
            var complete = from selectLine in File.ReadAllLines(file) where !selectLine.Contains(word) select selectLine;
            File.WriteAllLines(file, complete.ToArray());
        }

        public static void UncapitalizeAll(string file) {
            string[] complete = File.ReadAllLines(file);
            for (int i = 0; i < complete.Length; i++) {
                complete[i] = complete[i].ToLower();
            }
            File.WriteAllLines(file, complete);
        }
    }
}

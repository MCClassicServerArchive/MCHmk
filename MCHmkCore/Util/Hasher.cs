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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MCHmk.Util {
    internal sealed class PasswordHasher {
        internal static readonly string passwordsPath = Path.Combine("extra", "passwords");

        internal static byte[] Compute(string salt, string plainText) {
            if (string.IsNullOrEmpty(salt)) {
                throw new ArgumentNullException("salt", "fileName is null or empty");
            }

            if (string.IsNullOrEmpty(plainText)) {
                throw new ArgumentNullException("plainText", "plainText is null or empty");
            }

            salt = salt.Replace("<", "(");
            salt = salt.Replace(">", ")");
            plainText = plainText.Replace("<", "(");
            plainText = plainText.Replace(">", ")");

            MD5 hash = MD5.Create();

            byte[] textBuffer = Encoding.ASCII.GetBytes(plainText);
            byte[] saltBuffer = Encoding.ASCII.GetBytes(salt);

            byte[] hashedTextBuffer = hash.ComputeHash(textBuffer);
            byte[] hashedSaltBuffer = hash.ComputeHash(saltBuffer);
            return hash.ComputeHash(hashedSaltBuffer.Concat(hashedTextBuffer).ToArray());
        }

        internal static void StoreHash(string salt, string plainText) {
            byte[] doubleHashedSaltBuffer = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(Compute(salt, plainText)));
            
            string curPath = Path.Combine(passwordsPath, salt + ".dat");

            if (!File.Exists(curPath)) {
                File.Create(curPath).Dispose();
            }

            using (FileStream Writer = File.OpenWrite(curPath)) {
                Writer.Write(doubleHashedSaltBuffer, 0, doubleHashedSaltBuffer.Length);
            }
        }

        internal static bool MatchesPass(string salt, string plainText) {
            string curPath = Path.Combine(passwordsPath, salt + ".dat");
            
            if (!File.Exists(curPath)) {
                return false;
            }

            string hashes = File.ReadAllText(curPath);

            return hashes.Equals(Encoding.UTF8.GetString(Compute(salt, plainText)));
        }
    }
}

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
using System.IO;

namespace MCHmk {
    /// <summary>
    /// The FileUtil static class contains helper functions for working with files.
    /// </summary>
    // TODO: better name than FileUtil
    public static class FileUtil {
        /// <summary>
        /// Given two paths, combine both of them, and check whether the resulting path is within the
        /// location specified by the first path.
        /// </summary>
        /// <remarks>
        /// The first path can be absolute or relative. It must not end in a file name. The second path
        /// is intended to be relative to the first path when programming, although user input can cause
        /// it to be an absolute path.
        /// 
        /// In most cases, use this method when dealing with file paths that contain user input. A usual
        /// scenario is when a player provides input that is used to find a file with the same name as the
        /// input, and the file is expected to be within a given directory.
        /// 
        /// The implementation of this method does involve accessing the filesystem, so exceptions may
        /// occur if the given paths exist and the current user does not have permission to access them.
        /// </remarks>
        /// <param name="first"> The first path, which is the base for the second path. </param>
        /// <param name="second"> The second path. </param>
        /// <returns> Whether the second path points to a file within the first path. </returns>
        public static bool BasePathCheck(string first, string second) {
            string absBasePath = Path.GetFullPath(first);
            string filePath = Path.GetFullPath(Path.Combine(first, second));

            // If the provided path doesn't end with a slash, add one. Otherwise, a malicious user can
            // access files in the parent directory that begin with the parent directory's name.
            // For example, if the first path is "/home/poop/heh", and the second path is "../hehe",
            // that would still be considered valid if this if-block is not here.
            if (!first.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                    !first.EndsWith(Path.AltDirectorySeparatorChar.ToString())) {
                absBasePath += Path.DirectorySeparatorChar.ToString();
            }

            // In MCHmk, file names should be used as if they were case sensitive, no matter the operating
            // system. Thus, don't use the IgnoreCase comparison.
            return filePath.StartsWith(absBasePath, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if a name of a file or folder is valid for MCHmk's purposes.
        /// </summary>
        /// <remarks>
        /// The allowed characters in files created by MCHmk is different and more restrictive than what
        /// the file system allows. Alphanumeric characters are allowed as well as the period, single quote,
        /// comma, plus sign, hyphen, and underscore. No spaces are allowed. More characters may be added
        /// over time, provided that they are legal to use on Windows as well.
        ///
        /// This is to ensure common behavior across Windows and Unix filesystems. The latter is especially a
        /// problem because it allows control characters in the names and other characters that can be
        /// rather unexpected, and it also allows characters that the Windows kernel would reject.
        ///
        /// There are several commands in MCHmk that directly uses the user's input as a filename, however
        /// unwise that may be. These include, but are not limited to, /save <map?> <name?>, /view <name?>,
        /// and /store <name?>, so this method is best for those cases. However, if the input given is a
        /// level or player name, it is strongly recommended to use Level.ValidName or Player.ValidName
        /// instead, as they follow different rules that are more suitable for level or player names.
        /// </remarks>
        /// <param name="filename"> The name to examine. Do not pass in a path. </param>
        /// <returns> Whether the filename is valid. </returns>
        public static bool ValidName(string filename) {
            // It's best to use a whitelist and not a blacklist. With a blacklist, it is easy to forget some
            // characters that should be disallowed.
            const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.',-_+";
            return StringUtil.ContainsOnlyChars(filename, allowedChars);
        }
    }
}

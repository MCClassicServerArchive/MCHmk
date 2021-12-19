/*
    Copyright 2016 Jjp137

    This file includes source code from MCForge-Redux.

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
using System.Text;

namespace MCHmk {
    /// <summary>
    /// The Emotes static class contains methods for handling emoticons in messages.
    /// </summary>
    public static class Emotes {
        /// <summary>
        /// A dictionary of emote keywords and the symbols that they are replaced with.
        /// </summary>
        public static readonly Dictionary<string, char> EmoteKeywords = new Dictionary<string, char> {
            { "darksmile", '\u0001' },

            { "smile", '\u0002' }, // ☻

            { "heart", '\u0003' }, // ♥
            { "hearts", '\u0003' },

            { "diamond", '\u0004' }, // ♦
            { "diamonds", '\u0004' },
            { "rhombus", '\u0004' },

            { "club", '\u0005' }, // ♣
            { "clubs", '\u0005' },
            { "clover", '\u0005' },
            { "shamrock", '\u0005' },

            { "spade", '\u0006' }, // ♠
            { "spades", '\u0006' },

            { "*", '\u0007' }, // •
            { "bullet", '\u0007' },
            { "dot", '\u0007' },
            { "point", '\u0007' },

            { "hole", '\u0008' }, // ◘

            { "circle", '\u0009' }, // ○
            { "o", '\u0009' },

            { "male", '\u000B' }, // ♂
            { "mars", '\u000B' },

            { "female", '\u000C' }, // ♀
            { "venus", '\u000C' },

            { "8", '\u000D' }, // ♪
            { "note", '\u000D' },
            { "quaver", '\u000D' },

            { "notes", '\u000E' }, // ♫
            { "music", '\u000E' },

            { "sun", '\u000F' }, // ☼
            { "celestia", '\u000F' },

            { ">>", '\u0010' }, // ►
            { "right2", '\u0010' },

            { "<<", '\u0011' }, // ◄
            { "left2", '\u0011' },

            { "updown", '\u0012' }, // ↕
            { "^v", '\u0012' },

            { "!!", '\u0013' }, // ‼

            { "p", '\u0014' }, // ¶
            { "para", '\u0014' },
            { "pilcrow", '\u0014' },
            { "paragraph", '\u0014' },

            { "s", '\u0015' }, // §
            { "sect", '\u0015' },
            { "section", '\u0015' },

            { "-", '\u0016' }, // ▬
            { "_", '\u0016' },
            { "bar", '\u0016' },
            { "half", '\u0016' },

            { "updown2", '\u0017' }, // ↨
            { "^v_", '\u0017' },

            { "^", '\u0018' }, // ↑
            { "up", '\u0018' },

            { "v", '\u0019' }, // ↓
            { "down", '\u0019' },

            { ">", '\u001A' }, // →
            { "->", '\u001A' },
            { "right", '\u001A' },

            { "<", '\u001B' }, // ←
            { "<-", '\u001B' },
            { "left", '\u001B' },

            { "l", '\u001C' }, // ∟
            { "angle", '\u001C' },
            { "corner", '\u001C' },

            { "<>", '\u001D' }, // ↔
            { "<->", '\u001D' },
            { "leftright", '\u001D' },

            { "^^", '\u001E' }, // ▲
            { "up2", '\u001E' },

            { "vv", '\u001F' }, // ▼
            { "down2", '\u001F' },

            { "house", '\u007F' } // ⌂
        };

        /// <summary>
        /// Replaces the emoticon keywords in a message with the actual emoticon characters.
        /// </summary>
        /// <param name="message"> The message that is being sent. </param>
        /// <returns> The message with the equivalent Unicode emoticon characters. </returns>
        public static string ReplaceKeywords(string message) {
            if (message == null) {
                throw new ArgumentNullException("message");
            }

            // Check if there are any left parentheses, which may indicate the beginning
            // of an emoticon keyword.
            int startIndex = message.IndexOf('(');
            if (startIndex == -1) {
                return message; // Return the original message if there are no left parentheses at all.
            }

            StringBuilder output = new StringBuilder(message.Length);
            int lastAppendedIndex = 0;
            while (startIndex != -1) {
                // Try to find the end of the keyword.
                int endIndex = message.IndexOf(')', startIndex + 1);
                if (endIndex == -1) {
                    break;  // Stop if there are no right parentheses.
                }

                // If there are an odd number of backslashes, the emoticon is being escaped.
                bool escaped = false;
                for (int i = startIndex - 1; i >= 0 && message[i] == '\\'; i--) {
                    escaped = !escaped;
                }
                // Extract the potential keyword between the parentheses.
                string keyword = message.Substring(startIndex + 1, endIndex - startIndex - 1);
                char substitute;
                // Check if the keyword is in the emoticon dictionary.
                if (EmoteKeywords.TryGetValue(keyword.ToLowerInvariant(), out substitute)) {
                    if (escaped) {
                        // If the emoticon was escaped, just remove the escape character.
                        startIndex++;
                        output.Append( message, lastAppendedIndex, startIndex - lastAppendedIndex - 2 );
                        lastAppendedIndex = startIndex - 1;
                    }
                    else {
                        // Otherwise, replace the keyword, including the parentheses, with the emoticon.
                        output.Append(message, lastAppendedIndex, startIndex - lastAppendedIndex);
                        output.Append(substitute);
                        startIndex = endIndex + 1;
                        lastAppendedIndex = startIndex;
                    }
                }
                else { // The keyword was not in the dictionary, so just move on.
                    startIndex++;
                }
                // Try to find the start of another emoticon keyword. The loop ends if none remain.
                startIndex = message.IndexOf('(', startIndex);
            }
            // Append the rest of the message and return the modified string.
            output.Append(message, lastAppendedIndex, message.Length - lastAppendedIndex);
            return output.ToString();
        }
    }
}

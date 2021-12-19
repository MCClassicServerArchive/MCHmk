/*
    Copyright 2016 Jjp137

    This file have been changed from the original source code by MCForge.

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
    Copyright © 2011-2014 MCForge-Redux.

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
using System.Text;
using System.Text.RegularExpressions;

// TODO: add support for putting 2+ words on one line

namespace MCHmk {
    /// <summary>
    /// The class containing static methods that are related to filtering out profane content
    /// from chat.
    /// </summary>
    public class ProfanityFilter {
        /// <summary>
        /// The list of words that should be filtered out of chat.
        /// </summary>
        private List<string> BadWords = new List<string>();
        /// <summary>
        /// The path to the badwords.txt file. This string is read-only.
        /// </summary>
        private readonly string BadWordsPath = Path.Combine("text", "badwords.txt");

        /// <summary>
        /// Initializes the profanity filter.
        /// </summary>
        public ProfanityFilter(Server s) {
            if (!CreateBadWordsFile(s) || !AddWords(s)) { // If one of them doesn't succeed
                s.logger.Log("Warning: profanity filter could not be initialized.");
                s.logger.Log("Disabling the filter. Re-enable in server.properties.");
                s.props.profanityFilter = false;
                return;
            }
        }

        /// <summary>
        /// Checks for badwords.txt, and creates it if it doesn't exist.
        /// </summary>
        /// <param name="s"> The Server that will be using the profanity filter. <seealso cref="Server"/></param>
        /// <returns> True if the file exists or is created succcesfully, false otherwise. </returns>
        private bool CreateBadWordsFile(Server s) {
            try {
                if (File.Exists(BadWordsPath)) {
                    return true;
                }
                else { // Create the default file
                    StringBuilder newFile = new StringBuilder();
                    newFile.AppendLine("# This file contains a list of bad words that the profanity filter should remove.");
                    newFile.AppendLine("# Each bad word that should be removed needs to have its own line.");
                    File.WriteAllText(BadWordsPath, newFile.ToString());
                    return true;
                }
            }
            catch (IOException) {
                s.logger.Log("Warning: badwords.txt cannot be created for some reason.");
                return false;
            }
            catch (UnauthorizedAccessException) {
                s.logger.Log("Warning: badwords.txt cannot be created because MCHmk lacks permission to write it!");
                return false;
            }
        }

        /// <summary>
        /// Reads badwords.txt and adds each word to the filter.
        /// </summary>
        /// <param name="s"> The Server that will be using the profanity filter. <seealso cref="Server"/></param>
        /// <returns> True if badwords.txt is parsed successfully, false otherwise. </returns>
        private bool AddWords(Server s) {
            try {
                string[] wordsToAdd = File.ReadAllLines(BadWordsPath);
                foreach (string line in wordsToAdd) {
                    string word = line.Trim(); // Remove trailing whitespace

                    // Don't add commented lines or empty strings
                    if (!line.StartsWith("#") && !String.IsNullOrEmpty(word)) {
                        BadWords.Add(word.ToLower());
                    }
                }
                return true;
            }
            catch (IOException) {
                s.logger.Log("Warning: badwords.txt cannot be parsed for some reason.");
                return false;
            }
            catch (UnauthorizedAccessException) {
                s.logger.Log("Warning: badwords.txt cannot be parsed because MCHmk lacks permission to parse it!");
                return false;
            }
        }

        /// <summary>
        /// Replaces a word with the appropriate number of asterisks, censoring it.
        /// </summary>
        /// <param name="word"> The word to be censored. </param>
        /// <returns> </returns>
        private string Asterisk(string word) {
            string asterisks = String.Empty;
            for (int i = 1; i <= word.Length; i++) {
                asterisks += "*";
            }
            return asterisks;
        }

        /// <summary>
        /// Checks the word and if it is a bad word, filters it.
        /// </summary>
        /// <param name="word"> The word to be filtered. </param>
        /// <returns> The filtered word, or the original word if it is not a bad word. </returns>
        private string Filter(string word) {
            // Get rid of color codes for comparison purposes
            string noColorWord = Regex.Replace(word, @"(%[0-9a-f])*", String.Empty);
            // Keep color if it is not a bad word; return the asterisks otherwise
            return BadWords.Contains(noColorWord.ToLower()) ? Asterisk(noColorWord) : word;
        }

        /// <summary>
        /// Parses a message and replaces any bad words with asterisks.
        /// </summary>
        /// <param name="message"> The message to be filtered. </param>
        /// <returns> The filtered message. </returns>
        public string Parse(string message) {
            if (FilterHasNoWords()) { // No point in trying if that's true...
                return message;
            }

            // This is quite possibly the worst string parsing code ever written.
            // I'm very worried about performance here. -Jjp137
            string newMessage = String.Empty;
            string currentWord = String.Empty;

            foreach (char c in message) {
                // If we see the space, it's usually the end of the word, but sometimes there can
                // be 2+ spaces in a row
                if (Char.IsWhiteSpace(c)) {
                    // Check if the space marked the end of a word, and if so, filter it
                    if (!String.IsNullOrEmpty(currentWord)) {
                        newMessage += Filter(currentWord);
                        currentWord = String.Empty; // Prepare for the next word
                    }

                    // Add the space too; do this even if the current "word" is blank since that
                    // could mean that there are many spaces in a row
                    newMessage += " ";
                }
                else { // Otherwise, add the character to the current "word" being processed
                    currentWord += c.ToString();
                }
            }

            // There is usually no space at the end of a message, so we need to check the last word
            // here. Otherwise, it will get lost since words are normally added to the string when
            // a space is encountered. People, however, don't put an extra space at the end of their
            // messages.
            if (!String.IsNullOrEmpty(currentWord)) {
                newMessage += Filter(currentWord);
            }
            // Finally! :)
            return newMessage;
        }

        /// <summary>
        /// Checks if the filter has no words.
        /// </summary>
        /// <returns> Whether the filter has no words at all. </returns>
        public bool FilterHasNoWords() {
            return BadWords.Count == 0;
        }
    }
}

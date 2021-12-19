/*
    Copyright 2016 Jjp137/LegoBricker

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
using System.Collections.Generic;

namespace MCHmk.Drawing {
    /// <summary>
    /// The CharacterWriter class, which writes characters with blocks in game.
    /// </summary>
    internal static class CharacterWriter {

        // These arrays hold booleans which are true if a block is 
        // at that location in space, and false if not.
        static bool[,] A = {{false, true, false},
            {true, false, true},
            {true, true, true},
            {true, false, true},
            {true, false, true}
        };
        static bool[,] B = {{true, true, false},
            {true, false, true},
            {true, true, false},
            {true, false, true},
            {true, true, false}
        };
        static bool[,] C = {{false, true, true},
            {true, false, false},
            {true, false, false},
            {true, false, false},
            {false, true, true}
        };
        static bool[,] D = {{true, true, false},
            {true, false, true},
            {true, false, true},
            {true, false, true},
            {true, true, false}
        };
        static bool[,] E = {{true, true, true},
            {true, false, false},
            {true, true, true},
            {true, false, false},
            {true, true, true}
        };
        static bool[,] F = {{true, true, true},
            {true, false, false},
            {true, true, true},
            {true, false, false},
            {true, false, false}
        };
        static bool[,] G = {{false, true, true},
            {true, false, false},
            {true, false, true},
            {true, false, true},
            {false, true, true}
        };
        static bool[,] H = {{true, false, true},
            {true, false, true},
            {true, true, true},
            {true, false, true},
            {true, false, true}
        };
        static bool[,] I = {{true, true, true},
            {false, true, false},
            {false, true, false},
            {false, true, false},
            {true, true, true}
        };
        static bool[,] J = {{true, true, true},
            {false, false, true},
            {false, false, true},
            {false, false, true},
            {true, true, false}
        };
        static bool[,] K = {{true, false, true},
            {true, false, true},
            {true, true, false},
            {true, false, true},
            {true, false, true}
        };
        static bool[,] L = {{true, false, false},
            {true, false, false},
            {true, false, false},
            {true, false, false},
            {true, true, true}
        };
        static bool[,] M = {{true, false, false, false, true},
            {true, true, false, true, true},
            {true, false, true, false, true},
            {true, false, false, false, true},
            {true, false, false, false, true}
        };
        static bool[,] N = {{true, false, false, false, true},
            {true, true, false, false, true},
            {true, false, true, false, true},
            {true, false, false, true, true},
            {true, false, false, false, true}
        };
        static bool[,] O = {{false, true, false},
            {true, false, true},
            {true, false, true},
            {true, false, true},
            {false, true, false}
        };
        static bool[,] P = {{true, true, false},
            {true, false, true},
            {true, true, false},
            {true, false, false},
            {true, false, false}
        };
        static bool[,] Q = {{false, true, true, false},
            {true, false, false, true},
            {true, false, false, true},
            {true, false, true, true},
            {false, true, true, true}
        };
        static bool[,] R = {{true, true, false},
            {true, false, true},
            {true, true, false},
            {true, false, true},
            {true, false, true}
        };
        static bool[,] S = {{false, true, true},
            {true, false, false},
            {false, true, false},
            {false, false, true},
            {true, true, false}
        };
        static bool[,] T = {{true, true, true},
            {false, true, false},
            {false, true, false},
            {false, true, false},
            {false, true, false}
        };
        static bool[,] U = {{true, false, true},
            {true, false, true},
            {true, false, true},
            {true, false, true},
            {false, true, false}
        };
        static bool[,] V = {{true, false, false, false, true},
            {true, false, false, false, true},
            {false, true, false, true, false},
            {false, true, false, true, false},
            {false, false, true, false, false}
        };
        static bool[,] W = {{true, false, false, false, true},
            {true, false, false, false, true},
            {true, false, true, false, true},
            {true, true, false, true, true},
            {true, false, false, false, true}
        };
        static bool[,] X = {{true, false, true},
            {true, false, true},
            {false, true, false},
            {true, false, true},
            {true, false, true}
        };
        static bool[,] Y = {{true, false, false, false, true},
            {false, true, false, true, false},
            {false, false, true, false, false},
            {false, false, true, false, false},
            {false, false, true, false, false}
        };
        static bool[,] Z = {{true, true, true, true, true},
            {false, false, false, true, false},
            {false, false, true, false, false},
            {false, true, false, false, false},
            {true, true, true, true, true}
        };
        static bool[,] ZERO = {{false, true, true, true, false},
            {true, false, false, true, true},
            {true, false, true, false, true},
            {true, true, false, false, true},
            {false, true, true, true, false}
        };
        static bool[,] ONE = {{false, true, false},
            {true, true, false},
            {false, true, false},
            {false, true, false},
            {true, true, true}
        };
        static bool[,] TWO = {{true, true, true},
            {false, false, true},
            {true, true, true},
            {true, false, false},
            {true, true, true}
        };
        static bool[,] THREE = {{true, true, true},
            {false, false, true},
            {true, true, true},
            {false, false, true},
            {true, true, true}
        };
        static bool[,] FOUR = {{true, false, false, false},
            {true, false, false, false},
            {true, false, true, false},
            {true, true, true, true},
            {false, false, true, false}
        };
        static bool[,] FIVE = {{true, true, true},
            {true, false, false},
            {true, true, true},
            {false, false, true},
            {true, true, true}
        };
        static bool[,] SIX = {{true, true, true},
            {true, false, false},
            {true, true, true},
            {true, false, true},
            {true, true, true}
        };
        static bool[,] SEVEN = {{true, true, true},
            {false, false, true},
            {false, false, true},
            {false, false, true},
            {false, false, true}
        };
        static bool[,] EIGHT = {{true, true, true},
            {true, false, true},
            {true, true, true},
            {true, false, true},
            {true, true, true}
        };
        static bool[,] NINE = {{true, true, true},
            {true, false, true},
            {true, true, true},
            {false, false, true},
            {true, true, true}
        };
        static bool[,] COLON = {{false},
            {true},
            {false},
            {true},
            {false}
        };
        static bool[,] BACKSLASH = {{true, false, false, false, false},
            {false, true, false, false, false},
            {false, false, true, false, false},
            {false, false, false, true, false},
            {false, false, false, false, true}
        };
        static bool[,] FORWARDSLASH = {{false, false, false, false, true},
            {false, false, false, true, false},
            {false, false, true, false, false},
            {false, true, false, false, false},
            {true, false, false, false, false}
        };
        static bool[,] PERIOD = {{false},
            {false},
            {false},
            {false},
            {true}
        };
        static bool[,] EXCLAMATIONPOINT = {{true},
            {true},
            {true},
            {false},
            {true}
        };
        static bool[,] COMMA = {{false, false},
            {false, false},
            {false, false},
            {false, true},
            {true, true}
        };
        static bool[,] DOUBLEQUOTE = {{true, false, true},
            {true, false, true},
            {false, false, false},
            {false, false, false},
            {false, false, false}
        };
        static bool[,] SINGLEQUOTE = {{true},
            {true},
            {false},
            {false},
            {false}
        };
        static bool[,] PLUS = {{false, false, false},
            {false, true, false},
            {true, true, true},
            {false, true, false},
            {false, false, false}
        };
        static bool[,] MINUS = {{false, false, false},
            {false, false, false},
            {true, true, true},
            {false, false, false},
            {false, false, false}
        };
        static bool[,] UNDERSCORE = {{false, false, false, false},
            {false, false, false, false},
            {false, false, false, false},
            {false, false, false, false},
            {true, true, true, true}
        };
        static bool[,] EQUALS = {{false, false, false},
            {true, true, true},
            {false, false, false},
            {true, true, true},
            {false, false, false}
        };
        static bool[,] OPENPARENTHESIS = {{false, true},
            {true, false},
            {true, false},
            {true, false},
            {false, true}
        };
        static bool[,] CLOSEPARENTHESIS = {{true, false},
            {false, true},
            {false, true},
            {false, true},
            {true, false}
        };
        static bool[,] OPENBRACE = {{false, true, true},
            {false, true, false},
            {true, false, false},
            {false, true, false},
            {false, true, true}
        };
        static bool[,] CLOSEBRACE = {{true, true, false},
            {false, true, false},
            {false, false, true},
            {false, true, false},
            {true, true, false}
        };
        static bool[,] LEFTCHEVRON = {{false, false, true},
            {false, true, false},
            {true, false, false},
            {false, true, false},
            {false, false, true}
        };
        static bool[,] RIGHTCHEVRON = {{true, false, false},
            {false, true, false},
            {false, false, true},
            {false, true, false},
            {true, false, false}
        };
        static bool[,] VERTICALBAR = {{true},
            {true},
            {true},
            {true},
            {true}
        };
        static bool[,] GRAVE = {{true, false},
            {false, true},
            {false, false},
            {false, false},
            {false, false}
        };
        static bool[,] OPENBRACKET = {{true, true},
            {true, false},
            {true, false},
            {true, false},
            {true, true}
        };
        static bool[,] CLOSEBRACKET = {{true, true},
            {false, true},
            {false, true},
            {false, true},
            {true, true}
        };
        static bool[,] TILDE = {{false, false, false, false},
            {false, true, false, true},
            {true, false, true, false},
            {false, false, false, false},
            {false, false, false, false}
        };
        static bool[,] SEMICOLON = {{false, false},
            {false, true},
            {false, false},
            {false, true},
            {true, false}
        };
        static bool[,] QUESTIONMARK = {{true, true, false},
            {false, false, true},
            {false, true, false},
            {false, false, false},
            {false, true, false}
        };
        static bool[,] SPACE = {{false},
            {false},
            {false},
            {false},
            {false}
        };
        static bool[,] UNKNOWN = {{false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false},
            {false, false, false, false, false}
        };
      
        /// <summary>
        /// The dictionary linking a char to its array of booleans.
        /// </summary>
        static Dictionary<char, bool[,]> charDictionary = new Dictionary<char, bool[,]>()
        {
            {'A', A},
            {'B', B},
            {'C', C},
            {'D', D},
            {'E', E},
            {'F', F},
            {'G', G},
            {'H', H},
            {'I', I},
            {'J', J},
            {'K', K},
            {'L', L},
            {'M', M},
            {'N', N},
            {'O', O},
            {'P', P},
            {'Q', Q},
            {'R', R},
            {'S', S},
            {'T', T},
            {'U', U},
            {'V', V},
            {'W', W},
            {'X', X},
            {'Y', Y},
            {'Z', Z},
            {'0', ZERO},
            {'1', ONE},
            {'2', TWO},
            {'3', THREE},
            {'4', FOUR},
            {'5', FIVE},
            {'6', SIX},
            {'7', SEVEN},
            {'8', EIGHT},
            {'9', NINE},
            {':', COLON},
            {'\\', BACKSLASH},
            {'/', FORWARDSLASH},
            {'.', PERIOD},
            {'!', EXCLAMATIONPOINT},
            {',', COMMA},
            {'\"', DOUBLEQUOTE},
            {'\'', SINGLEQUOTE},
            {'+', PLUS},
            {'-', MINUS},
            {'_', UNDERSCORE},
            {'=', EQUALS},
            {'(', OPENPARENTHESIS},
            {')', CLOSEPARENTHESIS},
            {'{', OPENBRACE},
            {'}', CLOSEBRACE},
            {'<', LEFTCHEVRON},
            {'>', RIGHTCHEVRON},
            {'|', VERTICALBAR},
            {'`', GRAVE},
            {'[', OPENBRACKET},
            {']', CLOSEBRACKET},
            {'~', TILDE},
            {';', SEMICOLON},
            {'?', QUESTIONMARK},
            {' ', SPACE},
        };

        /// <summary>
        /// Writes the given character.
        /// </summary>
        /// <remarks>Direction to go: 0 - Positive X, 1 - Negative X,
        /// 2 - Positive Z, 3 - Negative Z</remarks>
        /// <returns>The start position for the next character.</returns>
        /// <param name="p">The player who used /write.</param>
        /// <param name="c">The character to write.</param>
        /// <param name="x">The x coordinate of the lower left corner.</param>
        /// <param name="y">The y coordinate of the lower left corner.</param>
        /// <param name="z">The z coordinate of the lower left corner.</param>
        /// <param name="blockID">The ID of the block to write with.</param>
        /// <param name="directionToGo">The direction to write in.</param>
        public static ushort writeCharacter(Player p, char c, ushort x, ushort y, ushort z, BlockId blockID, int directionToGo) {
            return writeCharacter(p.level, p, c, x, y, z, blockID, directionToGo);
        }

        /// <summary>
        /// Writes the given character.
        /// </summary>
        /// <remarks>Direction to go: 0 - Positive X, 1 - Negative X,
        /// 2 - Positive Z, 3 - Negative Z</remarks>
        /// <returns>The start position for the next character.</returns>
        /// <param name="l">The level to write the character on.</param>
        /// <param name="p">The player who used /write.</param>
        /// <param name="c">The character to write.</param>
        /// <param name="x">The x coordinate of the lower left corner.</param>
        /// <param name="y">The y coordinate of the lower left corner.</param>
        /// <param name="z">The z coordinate of the lower left corner.</param>
        /// <param name="blockID">The ID of the block to write with.</param>
        /// <param name="directionToGo">The direction to write in.</param>
        public static ushort writeCharacter(Level l, Player p, char c, ushort x, ushort y, ushort z, BlockId blockID, int directionToGo) {

            // Intialize the character block placement array.
            bool[,] character = new bool[1,1];

            // Set the character array to the correct array.
            if (charDictionary.ContainsKey(c)){
                character = charDictionary[c];
            }
            else { // If the character is unsupported leave a very large space.
                p.SendMessage("\"" + c + "\" is currently not supported. Space left");
                character = UNKNOWN;
            }

            // Determine the height and width of the character passed.
            int height = character.GetUpperBound(0);
            int width = character.GetUpperBound(1);

            // Loop through the character's array and place blocks in respective positions.
            for (int i = 0; i <= height; i++){
                for (int j = 0; j <= width; j++){
                    if (character[i,j]){
                        if (directionToGo == 0) {
                            placeBlock(l, p, (ushort)(x + j), (ushort)(y + (height - i)), (ushort)z, blockID);
                        }
                        else if (directionToGo == 1) {
                            placeBlock(l, p, (ushort) (x - j), (ushort) (y + (height - i)), (ushort) z, blockID);
                        }
                        else if (directionToGo == 2) {
                            placeBlock(l, p, (ushort) x, (ushort) (y + (height - i)), (ushort) (z + j), blockID);
                        }
                        else {
                            placeBlock(l, p, (ushort) x, (ushort) (y + (height - i)), (ushort) (z - j), blockID);
                        }
                    }
                }
            }

            // Return the position to start the next character,
            // A position 2 blocks after the end of the current character.
            // This leaves a one block space when the next character is written.
            if (directionToGo == 0) {
                return (ushort)(x + width + 2);
            }
            else if (directionToGo == 1) {
                return (ushort)(x - width - 2);
            }
            else if (directionToGo == 2) {
                return (ushort)(z + width + 2);
            }
            else {
                return (ushort)(z - width - 2);
            }

        }

        /// <summary>
        /// Places a block at a given position on a given level.
        /// </summary>
        /// <param name="l">The level to place the block on.</param>
        /// <param name="p">The player who used /write.</param>
        /// <param name="x">The x coordinate to place at.</param>
        /// <param name="y">The y coordinate to place at.</param>
        /// <param name="z">The z coordinate to place at.</param>
        /// <param name="blockID">The ID of the block to place.</param>
        public static void placeBlock(Level l, Player p, ushort x, ushort y, ushort z, BlockId blockID) {
            if (p == null) {
                l.Blockchange(x, y, z, blockID);
            }
            else {
                l.Blockchange(p, x, y, z, blockID);
            }
        }
    }
}

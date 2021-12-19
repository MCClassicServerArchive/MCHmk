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
using System.Collections.ObjectModel;

namespace MCHmk.Commands {
    public class CmdCalculate : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"calc", "number"});

        public override string Name {
            get {
                return "calculate";
            }
        }
        public override string Shortcut {
            get {
                return "calc";
            }
        }
        public override string Type {
            get {
                return "information";
            }
        }
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }
        public override bool MuseumUsable {
            get {
                return true;
            }
        }
        public override int DefaultRank {
            get {
                return DefaultRankValue.Banned;
            }
        }

        public CmdCalculate(Server s) : base(s) { }

        public override void Use(Player p, string args) {
            if (args == String.Empty) {
                Help(p);
                return;
            }

            var split = args.Split(' ');
            if (split.Length < 2) {
                Help(p);
                return;
            }

            if (!ValidChar(split[0])) {
                p.SendMessage("Invalid number given");
                return;
            }

            double result = 0;
            float num1 = float.Parse(split[0]);
            String operation = split[1];

            // All 2-parameter operations go here

            if (split.Length == 2) {
                switch(operation) {
                case "square":
                    result = num1 * num1;
                    p.SendMessage("The answer: %aThe square of " + split[0] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "root":
                    result = Math.Sqrt(num1);
                    p.SendMessage("The answer: %aThe root of " + split[0] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "cube":
                    result = num1 * num1 * num1;
                    p.SendMessage("The answer: %aThe cube of " + split[0] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "pi":
                    result = num1 * Math.PI;
                    p.SendMessage("The answer: %a" + split[0] + " x PI" + _s.props.DefaultColor + " = %c" + result);
                    return;
                default:
                    p.SendMessage("There is no such method");
                    return;
                }
            }

            // Now we try 3-parameter methods

            if (split.Length == 3) {
                if (!ValidChar(split[2])) {
                    p.SendMessage("Invalid number given");
                    return;
                }

                float num2 = float.Parse(split[2]);

                switch (operation) {
                case "x":
                case "*":
                    result = num1 * num2;
                    p.SendMessage("The answer: %a" + split[0] + " x " + split[2] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "+":
                    result = num1 + num2;
                    p.SendMessage("The answer: %a" + split[0] + " + " + split[2] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "-":
                    result = num1 - num2;
                    p.SendMessage("The answer: %a" + split[0] + " - " + split[2] + _s.props.DefaultColor + " = %c" + result);
                    return;
                case "/":
                    if(num2 == 0) {
                        p.SendMessage("Cannot divide by 0");
                        return;
                    }

                    result = num1 / num2;
                    p.SendMessage("The answer: %a" + split[0] + " / " + split[2] + _s.props.DefaultColor + " = %c" + result);
                    return;
                default:
                    p.SendMessage("There is no such method");
                    return;
                }
            }

            // If we get here, the player did something wrong

            Help(p);

        }


        public static bool ValidChar(string chr) {
            string allowedchars = "01234567890.,";
            foreach (char ch in chr) {
                if (allowedchars.IndexOf(ch) == -1) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Called when /help is used on /calculate.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/calculate <num1?> <operation?> <num2?> - " +
                               "Performs the given calculation and displays the result.");
            p.SendMessage("Depending on the calculation, one or two numbers are required.");
            p.SendMessage("Valid two-number operations: /, x, -, +");
            p.SendMessage("Valid one-number operations: square, root, pi, cube");
        }
    }
}

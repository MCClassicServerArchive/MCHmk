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

﻿using System;
using System.Collections.ObjectModel;
using System.IO;

namespace MCHmk.Commands {
    public class CmdWeather : Command {
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"snow", "sunny", "rain"});

        public override string Name {
            get {
                return "weather";
            }
        }
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }
        public override string Type {
            get {
                return "other";
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
                return DefaultRankValue.Operator;
            }
        }
        public CmdWeather(Server s) : base(s) { }

        /// <summary>
        /// What happens when /weather is used.
        /// </summary>
        /// <param name="p">The player who used /weather.</param>
        /// <param name="args">The parameters passed to /weather by the player.</param>
        public override void Use(Player p, string args) {
            // If the player passes no parameters.
            if (args == String.Empty) {
                Help(p);
                return;
            }

            // Setting up variables with default values.
            Level targetLevel = p.level;
            string forecast = "sunny";
            byte weatherType;
            string[] parameters = args.Split(' ');

            // If the player passed only a forecast.
            if (parameters.Length == 1) {
                forecast = parameters[0].Trim().ToLower();
            }
            // If the player passed a map and a forecast.
            else if (parameters.Length >= 2) {
                Level levelFound = _s.levels.FindExact(parameters[0]);
                if (levelFound != null) {
                    targetLevel = levelFound;
                }
                else {
                    p.SendMessage("The provided map name was invalid or not currently loaded.");
                    return;
                }
                forecast = parameters[1].Trim().ToLower();
            }
            
            // Forecast if block, setting the weatherType variable for the foreach later.
            if (forecast == "sunny") {
                weatherType = 0;
            }
            else if (forecast == "raining") {
                weatherType = 1;
            }
            else if (forecast == "snowing") {
                weatherType = 2;
            }
            else {
                p.SendMessage("The provided forecast was invalid.");
                return;
            }

            // Changes the weather for each player using the classicube client
            // or another client that supports weather.
            _s.players.ForEach(delegate(Player pl) {
                if (pl.level == targetLevel && pl.HasExtension("EnvWeatherType")) {
                    pl.SendSetMapWeather(weatherType);
                }
            });

            _s.GlobalMessage("Weather was set to &c" + forecast + _s.props.DefaultColor + " on &b" + targetLevel.name);
            targetLevel.weather = weatherType;
        }

        /// <summary>
        /// Called when /help is used on /weather.
        /// </summary>
        /// <param name="p"> The player that used the /help command. </param>
        public override void Help(Player p) {
            p.SendMessage("/weather [map?] <forecast?> - Changes the specified map's weather.");
            p.SendMessage("Available forecasts: Sunny, Raining, Snowing.");
            p.SendMessage("If no map is given, the player's current map is used.");
        }
    }
}

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using MCHmk.Drawing;

namespace MCHmk.Commands {
    /// <summary>
    /// Implementation of the /draw command, which draws various shapes into the player's current level.
    /// </summary>
    public class CmdDraw : Command {
        /// <summary>
        /// Name of the key used to store and retrieve the shape being drawn.
        /// </summary>
        private readonly string _shapeKey = "draw_shape";
        /// <summary>
        /// Name of the key used to store and retrieve the height of the shape being drawn.
        /// </summary>
        private readonly string _heightKey = "draw_height";
        /// <summary>
        /// Name of the key used to store and retrieve the radius of the shape being drawn.
        /// </summary>
        private readonly string _radiusKey = "draw_radius";

        /// <summary>
        /// The list of keywords that are associated with /draw.
        /// </summary>
        private ReadOnlyCollection<string> _keywords = Array.AsReadOnly<string>(
            new string[] {"cone", "sphere", "pyramid", "create"});

        /// <summary>
        /// The set of strings that represent shapes that /draw recognizes.
        /// </summary>
        private readonly HashSet<string> _validShapes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {"cone", "hcone", "icone", "hicone", "pyramid", "hpyramid", "ipyramid", "hipyramid",
            "sphere", "hsphere", "volcano"};

        /// <summary>
        /// The mapping of shapes to the index within the additional permissions list that contains the
        /// permission value for that particular shape.
        /// </summary>
        private readonly Dictionary<string, int> _permMapping =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {
                {"cone", 1}, {"hcone", 1}, {"icone", 1}, {"hicone", 1},
                {"pyramid", 2}, {"hpyramid", 2}, {"ipyramid", 2}, {"hipyramid", 2},
                {"sphere", 3}, {"hsphere", 3},
                {"volcano", 4}
            };

        /// <summary>
        /// Gets the name of /draw.
        /// </summary>
        public override string Name {
            get {
                return "draw";
            }
        }

        /// <summary>
        /// Gets the shortcut for /draw.
        /// </summary>
        public override string Shortcut {
            get {
                return String.Empty;
            }
        }

        /// <summary>
        /// Gets the category that /draw belongs to.
        /// </summary>
        public override string Type {
            get {
                return "build";
            }
        }

        /// <summary>
        /// Gets the keywords associated with /draw. Used for /search.
        /// </summary>
        public override ReadOnlyCollection<string> Keywords {
            get {
                return _keywords;
            }
        }

        /// <summary>
        /// Gets whether /draw can be used in museums.
        /// </summary>
        public override bool MuseumUsable {
            get {
                return false;
            }
        }

        /// <summary>
        /// Gets the default permission value for /draw.
        /// </summary>
        public override int DefaultRank {
            get {
                return DefaultRankValue.Builder;
            }
        }

        /// <summary>
        /// Constructs an instance of the /draw command.
        /// </summary>
        /// <param name="s"> The server that this instance of /draw will belong to. <seealso cref="Server"/></param>
        public CmdDraw(Server s) : base(s) { }

        /// <summary>
        /// Called when a player uses /draw.
        /// </summary>
        /// <param name="p"> The player that used /draw. <seealso cref="Player"/></param>
        /// <param name="args"> The arguments given by the user, if any. </param>
        public override void Use(Player p, string args) {
            // The console can't use this command.
            if (p.IsConsole) {
                p.SendMessage("This command can only be used in-game.");
                return;
            }

            // The player has to be able to build on the level they are on.
            if (!p.CanModifyCurrentLevel()) {
                p.SendMessage("Your rank prohibits you from modifying this map.");
                return;
            }

            int height = 0;
            int radius = 0;
            string shape = String.Empty;
            bool parsed = true;

            // Obtain the individual parameters.
            string[] split = args.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

            // The user clearly doesn't know how to use the command if no parameters or
            // too many parameters are given.
            if (split.Length == 0 || split.Length > 3) {
                Help(p);
                return;
            }
            // If at least one parameter is given, check if the given shape is valid and usable by
            // the player. This is done before everything else is checked because it makes more sense
            // to check the parameters in order.
            else {
                shape = split[0];

                if (!_validShapes.Contains(shape)) {
                    p.SendMessage("The provided shape is invalid.");
                    return;
                }
                if (!HasRequiredPerm(p, shape)) {
                    return;
                }
            }

            // Nothing can really be done if only the shape was given.
            if (split.Length == 1) {
                string errorMsg = AcceptsRadiusOnly(shape) ? "The radius was not provided." :
                                                             "The height and radius were not provided.";
                p.SendMessage(errorMsg);
                return;
            }
            // Spheres and hollow spheres only need the radius, so for those two shapes, only the radius is
            // needed to create those shapes. For other shapes, that is not enough, so tell the user that
            // and don't go any further.
            else if (split.Length == 2) {
                if (AcceptsRadiusOnly(shape)) {
                    parsed = Int32.TryParse(split[1], out radius);
                    if (!parsed || radius <= 0) {  // Negative lengths make no sense.
                        p.SendMessage("Invalid radius.");
                        return;
                    }
                }
                else {
                    p.SendMessage("The height or radius was not provided.");
                    return;
                }
            }
            // For most shapes, a height and a radius are both needed. Spheres and hollow spheres don't need
            // the extra parameter, though.
            else if (split.Length == 3) {
                if (AcceptsRadiusOnly(shape)) {
                    p.SendMessage("Spheres and hspheres need only a radius.");
                    return;
                }
                else {
                    parsed = Int32.TryParse(split[1], out height);
                    if (!parsed || height <= 0) {  // Negative lengths make no sense.
                        p.SendMessage("Invalid height.");
                        return;
                    }

                    parsed = Int32.TryParse(split[2], out radius);
                    if (!parsed || radius <= 0) {  // Negative lengths make no sense.
                        p.SendMessage("Invalid radius.");
                        return;
                    }
                }
            }

            // If we get here, the player's input is valid.
            PromptForBlockPos(p, shape, height, radius);
        }

        /// <summary>
        /// Sets up the command to receive a block position from the player.
        /// </summary>
        /// <param name="p"> The player who used the command. <seealso cref="Player"/></param>
        /// <param name="shape"> The shape to be drawn. </param>
        /// <param name="height"> The height of the shape to be drawn. </param>
        /// <param name="radius"> The radius of the shape to be drawn. </param>
        private void PromptForBlockPos(Player p, string shape, int height, int radius) {
            // Abort any conflicting commands, such as /cuboid.
            p.ClearSelection();

            // Store information that will be passed to the next step of the command.
            Dictionary<string, object> data = new Dictionary<string, object>();
            data[_shapeKey] = shape;
            data[_heightKey] = height;
            data[_radiusKey] = radius;

            // Listen for a block change by the player.
            p.StartSelection(BlockSelected, data);
            p.SendMessage("Place a block where you want the shape to be centered at.");
        }

        /// <summary>
        /// Called when a player selects a block as the base for the /draw command.
        /// </summary>
        /// <param name="p"> The player that has selected the block. <seealso cref="Player"/></param>
        /// <param name="c"> Data associated with the block selection. <seealso cref="CommandTempData"/></param>
        private void BlockSelected(Player p, CommandTempData c) {
            // The player has selected a block, so stop listening for a block change.
            p.ClearSelection();

            // Obtain the data that was passed previously.
            string shape = c.GetData<string>(_shapeKey);
            int height = c.GetData<int>(_heightKey);
            int radius = c.GetData<int>(_radiusKey);

            // Revert the block to what it used to be on the client's end since the block change that the player had
            // to do to select the block should not count.
            p.SendBlockchange(c.X, c.Y, c.Z, p.level.GetTile(c.X, c.Y, c.Z));

            // FIXME: A mapping would be better, but I want to wait until the "shapes" are refactored, so
            // use a switch-case block for now.

            // Depending on the shape, create it on the spot that the player placed a block on.
            switch (shape) {
                case "cone":
                    SCOGenerator.Cone(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "hcone":
                    SCOGenerator.HCone(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "icone":
                    SCOGenerator.ICone(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "hicone":
                    SCOGenerator.HICone(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "pyramid":
                    SCOGenerator.Pyramid(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "hpyramid":
                    SCOGenerator.HPyramid(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "ipyramid":
                    SCOGenerator.IPyramid(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "hipyramid":
                    SCOGenerator.HIPyramid(p, c.X, c.Y, c.Z, height, radius, c.BlockType);
                    break;
                case "sphere":
                    SCOGenerator.Sphere(p, c.X, c.Y, c.Z, radius, c.BlockType);
                    break;
                case "hsphere":
                    SCOGenerator.HSphere(p, c.X, c.Y, c.Z, radius, c.BlockType);
                    break;
                case "volcano":
                    SCOGenerator.Volcano(p, c.X, c.Y, c.Z, height, radius);
                    break;
                default:
                    p.SendMessage("Something went wrong with /draw.");
                    break;
            }
        }

        /// <summary>
        /// Check whether a given shape ignores the height parameter and only needs the radius.
        /// </summary>
        /// <param name="shape"> The shape to be drawn. </param>
        /// <returns> Whether the shape only needs the radius. If false, the shape requires the
        /// height as well. </returns>
        private bool AcceptsRadiusOnly(string shape) {
            return shape.Equals("sphere", StringComparison.OrdinalIgnoreCase) ||
                   shape.Equals("hsphere", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check whether the player has permission to draw the given shape.
        /// </summary>
        /// <param name="p"> The player that is drawing the shape. <seealso cref="Player"/></param>
        /// <param name="shape"> The shape to be drawn. </param>
        /// <returns> Whether the player can draw the given shape. </returns>
        private bool HasRequiredPerm(Player p, string shape) {
            int reqPerm = _s.commands.GetOtherPerm(this, _permMapping[shape]);

            if (p.rank.Permission < reqPerm) {
                string permRankName = _s.ranks.FindPerm(reqPerm).name;
                string plural = shape + "s";  // Incorrect for "volcano", but oh well.

                p.SendMessage("You must be at least a " + permRankName + " to draw " + plural + ".");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called when /help is used on /draw.
        /// </summary>
        /// <param name="p"> The player that used the /help command. <seealso cref="Player"/></param>
        public override void Help(Player p) {
            // TODO: display only shapes that the player can actually use
            p.SendMessage("/draw <shape?> <height?> <radius?> - Draws a centered shape with " +
                          "the given height and radius.");
            p.SendMessage("Available shapes: cone, hcone, icone, hicone, pyramid, hpyramid, ipyramid, hipyramid, " +
                          "sphere, hsphere, volcano");
            p.SendMessage("Shapes prefixed with h are hollow shapes, and shapes prefixed with i are inverted.");
            p.SendMessage("Omit the height for sphere and hspheres.");
        }
    }
}

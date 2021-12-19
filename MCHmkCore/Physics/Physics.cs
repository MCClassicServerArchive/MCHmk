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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;

namespace MCHmk {
    /// <summary>
    /// The Physics class represents the physics loop for a level.
    /// </summary>
    public class Physics {
        /// <summary>
        /// Holds information about blocks that are relevant to leaf decay. Used by PhysLeaf().
        /// </summary>
        public readonly Dictionary<int, sbyte> leaves = new Dictionary<int, sbyte>();

        /// <summary>
        /// Whether physics is currently paused for the level.
        /// </summary>
        public bool physPause;
        /// <summary>
        /// The scheduled time to unpause the physics loop. Used by the /pause command.
        /// </summary>
        public DateTime physResume;
        /// <summary>
        /// The thread that runs the physics loop.
        /// </summary>
        public Thread physThread;
        /// <summary>
        /// The Timer object that periodically checks if the physics loop should be unpaused.
        /// Used by the /pause command.
        /// </summary>
        public System.Timers.Timer physTimer = new System.Timers.Timer(1000);
        /// <summary>
        /// Holds information about active liquid blocks that flow randomly. The value for each block
        /// is a boolean array that keeps track of whether adjacent blocks are already an active liquid
        /// or a block that water cannot interact with.
        /// </summary>
        public readonly Dictionary<int, bool[]> liquids = new Dictionary<int, bool[]>();
        /// <summary>
        /// Whether physics is currently enabled.
        /// </summary>
        public bool physicssate = false;
        /// <summary>
        /// The lock object that controls access to the physics loop thread.
        /// </summary>
        private readonly object physThreadLock = new object();

        /// <summary>
        /// Runs the level's physics.
        /// </summary>
        public void PhysicsLoop(Level level) {
            int wait = level.speedphysics;  // Therefore, higher is slower.
            while (true) {

                // Don't do anything if physics is off.
                if (!level.PhysicsEnabled) {
                    Thread.Sleep(500);
                    continue;
                }

                try {
                    // If there is some wait time left over, wait for that amount of time.
                    if (wait > 0) {
                        Thread.Sleep(wait);
                    }

                    // If there are no blocks to update or if physics is 0, reset
                    // the wait time and note that no blocks have been checked.
                    if (level.physics == 0 || level.ListCheck.Count == 0) {
                        level.lastCheck = 0;
                        wait = level.speedphysics;

                        // Additionally, if physics is off, exit the loop.
                        if (level.physics == 0) {
                            break;
                        }
                        continue;  // Otherwise, wait again and don't process the physics.
                    }

                    // This is used to determine how long it takes to process the physics.
                    DateTime Start = DateTime.Now;

                    // Calculate one physics update.
                    if (level.physics > 0) {
                        CalcPhysics(level);
                    }

                    // Calculate the time it took for one physics update to occur and set
                    // the wait time accordingly. The wait time can be negative if the update
                    // took too long.
                    TimeSpan Took = DateTime.Now - Start;
                    wait = level.speedphysics - (int)Took.TotalMilliseconds;

                    // The threshold for sending a physics warning is if it lags behind 75% of
                    // the set overload time (in milliseconds).
                    if (wait < (int)(-level.overload * 0.75f)) {
                        Level Cause = level;

                        // Stop the physics if it has lagged behind for at least the amount of
                        // milliseconds specified in the physics overload setting.
                        if (wait < -level.overload) {
                            // If physics is not set to automatically restart, physics are turned off.
                            if (!level.Server.props.physicsRestart) {
                                Cause.setPhysics(0);
                            }

                            // Clear the physics.
                            Cause.physic.ClearPhysics(level);

                            // Tell everyone in the server what happened.
                            level.Server.GlobalMessage("level.physics shutdown on &b" + Cause.name);
                            level.Logger.Log("level.physics shutdown on " + level.name);

                            // Reset the wait time.
                            wait = level.speedphysics;
                        }

                        // Otherwise, just warn players in that level that physics is lagging a lot.
                        else {
                            foreach (Player p in level.Server.players.Where(p => p.level == level)) {
                                p.SendMessage("Physics warning!");
                            }
                            level.Logger.Log("level.physics warning on " + level.name);
                        }
                    }
                }
                catch (Exception e) {  // If something breaks, wait again.
                    level.Logger.ErrorLog(e);
                    wait = level.speedphysics;
                }
            }

            // Once the loop is aborted, stop the physics thread.
            physicssate = false;
            physThread.Abort();
        }

        /// <summary>
        /// Starts the level's physics thread.
        /// </summary>
        public void StartPhysics(Level level) {
            // Make sure that there won't be colliding attempts to start the physics thread.
            lock (physThreadLock) {
                if (physThread != null) { // Don't create another thread if one is already running
                    if (physThread.ThreadState == System.Threading.ThreadState.Running) {
                        return;
                    }
                }

                // TODO: figure out why the first check is there
                if (level.ListCheck.Count == 0 || physicssate) {
                    return;
                }

                // Create the physics thread and start it.
                physThread = new Thread(() => PhysicsLoop(level));
                level.PhysicsEnabled = true;
                physThread.Start();
                physicssate = true;
            }
        }

        /// <summary>
        /// Calculates and applies block changes caused by physics interactions.
        /// </summary>
        /// <param name="level"> The level associated with these calculations. <seealso cref="Level"/></param>
        public void CalcPhysics(Level level) {
            try {
                // Handle doors-only physics.
                if (level.physics == 5) {
                    #region == INCOMING ==

                    ushort x, y, z;

                    // Record the number of blocks that are about to be examined.
                    level.lastCheck = level.ListCheck.Count;

                    // Calculate what needs to be changed for each block.
                    for (int i = 0; i < level.ListCheck.Count; i++) {
                        Check C = level.ListCheck[i];

                        try {
                            // Obtain the coordinates and any extra physics-related information for this block.
                            level.IntToPos(C.b, out x, out y, out z);
                            string foundInfo = C.extraInfo;

                            newPhysic_p5:
                            // Examine any physics-related information first. This means that any information
                            // specified in extraInfo has first priority.
                            if (foundInfo != String.Empty) {
                                int currentLoop = 0;

                                // Air blocks that are not currently waiting for another effect to be
                                // applied to them lose all physics-related information.
                                if (!foundInfo.Contains("wait"))
                                    if (level.blocks[C.b] == BlockId.Air) {
                                        C.extraInfo = String.Empty;
                                    }

                                // Store the information gleaned from extraInfo in these variables.
                                bool wait = false;
                                int waitnum = 0;
                                bool door = false;

                                // Parse extraInfo. Any changes to C.extraInfo after this point are not
                                // reflected during the current iteration (unless the wait time has expired).
                                foreach (string s in C.extraInfo.Split(' ')) {
                                    // The format of extraInfo is: param1 value1 param2 value2 ...
                                    // Thus, the keywords are in the even indices, and their parameters are
                                    // in the odd indices.
                                    if (currentLoop % 2 == 0) {
                                        switch (s) {
                                        case "wait":
                                            wait = true;
                                            waitnum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;
                                        case "door":
                                            door = true;
                                            break;
                                        }
                                    }
                                    currentLoop++;
                                }

                                startCheck_p5:
                                // Examine the wait parameter if it is present. The wait parameter causes all
                                // other extraInfo parameters to be delayed for the given number of physics
                                // loop iterations. However, "door 1" is an exception to this rule.
                                if (wait) {
                                    int storedInt = 0;
                                    // If the "door 1" string is part of the extraInfo, this block is actually
                                    // a tdoor that is waiting to be reverted back, so check if the immediately
                                    // adjacent blocks are all tdoors and change them to air as well.
                                    // ---
                                    // Why C.time < 2? That makes this run twice, since C.time starts at 0, but
                                    // AddUpdate prevents two Update objects from having the same block location,
                                    // so I suppose this is okay. - Jjp137
                                    if (door && C.time < 2) {
                                        storedInt = level.IntOffset(C.b, -1, 0, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 1, 0, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 1, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, -1, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 0, 1);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 0, -1);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                    }

                                    // Once the number of physics loop iterations specified by the wait parameter has
                                    // passed, we are done waiting.
                                    if (waitnum <= C.time) {
                                        // We are not waiting anymore, so remove the "wait x" parameter from extraInfo.
                                        wait = false;
                                        C.extraInfo = C.extraInfo.Substring(0, C.extraInfo.IndexOf("wait ")) +
                                            C.extraInfo.Substring(C.extraInfo.IndexOf(' ', C.extraInfo.IndexOf("wait ") + 5) + 1);
                                        // This will jump back to the if statement that checks for the wait parameter.
                                        // The condition will be false, so all the other parameters would be examined
                                        // except that we are in Doors-Only mode. Nothing really happens because
                                        // there is no "else" or "else if" blocks that check the other parameters.
                                        goto startCheck_p5;
                                    }

                                    // Otherwise, that's one physics loop iteration done.
                                    C.time++;

                                    // The foundInfo variable is set to the empty string so that the extraInfo is
                                    // not processed again. This is done because the wait delay has not passed yet
                                    // if we reached this point, so we should not examine the extraInfo again.
                                    // Instead, the block-specific behavior should be looked at. Remember that
                                    // foundInfo and C.extraInfo are not the same variable, so the block still has
                                    // its information for the next iteration.
                                    foundInfo = String.Empty;
                                    // Since we jump back to right before the if statement, the condition will
                                    // evaluate to false since foundInfo is the empty string, so the else block
                                    // is executed instead.
                                    goto newPhysic_p5;
                                }
                            }
                            // If there is no extraInfo to read (or if we read it already and "wait x" is present),
                            // perform any physics-related changes based on the type of block being checked.
                            else {
                                switch (level.blocks[C.b]) {
                                case BlockId.DoorTreeWoodActive:
                                case BlockId.DoorObsidianActive:
                                case BlockId.DoorGlassActive:
                                case BlockId.DoorStoneActive:
                                case BlockId.DoorLeavesActive:
                                case BlockId.DoorSandActive:
                                case BlockId.DoorWoodPlanksActive:
                                case BlockId.DoorGreenActive:
                                case BlockId.DoorSlabActive:
                                case BlockId.DoorWaterActive:
                                case BlockId.DoorLavaActive:
                                case BlockId.DoorIronActive:
                                case BlockId.DoorGoldActive:
                                case BlockId.DoorCobblestoneActive:
                                case BlockId.DoorRedActive:

                                case BlockId.DoorOrangeActive:
                                case BlockId.DoorYellowActive:
                                case BlockId.DoorLimeActive:
                                case BlockId.DoorAquaGreenActive:
                                case BlockId.DoorCyanActive:
                                case BlockId.DoorBlueActive:
                                case BlockId.DoorIndigoActive:
                                case BlockId.DoorVioletActive:
                                case BlockId.DoorMagentaActive:
                                case BlockId.DoorPinkActive:
                                case BlockId.DoorBlackActive:
                                case BlockId.DoorGrayActive:
                                case BlockId.DoorWhiteActive:

                                case BlockId.DoorDirtActive:
                                case BlockId.DoorGrassActive:
                                case BlockId.DoorPurpleActive:
                                case BlockId.DoorBookshelfActive:
                                    // Activate adjacent doors if C.time == 0. Otherwise, the method checks
                                    // if 16 iterations have already passed, and if so, reverts the door_air
                                    // back to the equivalent door block.
                                    level.AnyDoor(C, x, y, z, 16);
                                    break;
                                case BlockId.DoorAirSwitchActive:
                                case BlockId.DoorAirActive:
                                    // The air_switch and air_door air blocks revert back in only 4 iterations.
                                    level.AnyDoor(C, x, y, z, 4, true);
                                    break;
                                case BlockId.DoorTntActive:
                                    // The door_tnt's air block reverts back in only 4 iterations.
                                    level.AnyDoor(C, x, y, z, 4);
                                    break;

                                case BlockId.ODoorTreeWoodActive:
                                case BlockId.ODoorObsidianActive:
                                case BlockId.ODoorGlassActive:
                                case BlockId.ODoorStoneActive:
                                case BlockId.ODoorLeavesActive:
                                case BlockId.ODoorSandActive:
                                case BlockId.ODoorWoodPlanksActive:
                                case BlockId.ODoorGreenActive:
                                case BlockId.ODoorTntActive:
                                case BlockId.ODoorSlabActive:
                                case BlockId.ODoorAirActive:
                                case BlockId.ODoorWaterActive:

                                case BlockId.ODoorTreeWood:
                                case BlockId.ODoorObsidian:
                                case BlockId.ODoorGlass:
                                case BlockId.ODoorStone:
                                case BlockId.ODoorLeaves:
                                case BlockId.ODoorSand:
                                case BlockId.ODoorWoodPlanks:
                                case BlockId.ODoorGreen:
                                case BlockId.ODoorTnt:
                                case BlockId.ODoorSlab:
                                case BlockId.ODoorAir:
                                case BlockId.ODoorWater:
                                    // Toggle the state of the odoor. It's really, really buggy though.
                                    level.odoor(C);
                                    break;
                                default:
                                    // Otherwise, this block does not affect physics in any way, at least in
                                    // Doors-Only mode. If the "wait x" parameter is not given, then the Check
                                    // object has expired and should be deleted from the list of blocks to check.
                                    if (!C.extraInfo.Contains("wait")) {
                                        C.time = 255;
                                    }
                                    break;
                                }
                            }
                        }
                        catch (Exception e){
                            level.Logger.ErrorLog(e);
                            level.ListCheck.Remove(C);
                        }
                    }

                    // All Check objects that have expired are deleted.
                    level.ListCheck.RemoveAll(delegate(Check Check) {
                        return Check.time == 255;
                    });

                    // Record the number of physics updates that are about to be applied.
                    level.lastUpdate = level.ListUpdate.Count;

                    // Go through each Update object and change the blocks that need to be changed.
                    level.ListUpdate.ForEach(delegate(Update C) {
                        try {
                            level.IntToPos(C.b, out x, out y, out z);
                            level.Blockchange(x, y, z, C.type, false, C.extraInfo);
                        }
                        catch (Exception e) {
                            level.Logger.ErrorLog(e);
                            level.Logger.Log("Phys update issue");
                        }
                    });

                    // Clear the list of physics updates afterwards since they have been applied.
                    level.ListUpdate.Clear();

                    #endregion

                    return;
                }
                // Handle physics settings from 1 to 4.
                if (level.physics > 0) {
                    #region == INCOMING ==

                    ushort x, y, z;
                    int mx, my, mz;

                    // Initialize the random number generator all the way up here.
                    var rand = new Random();

                    // Record the number of blocks that are about to be examined.
                    level.lastCheck = level.ListCheck.Count;

                    // Calculate what needs to be changed for each block. A regular for-loop is used
                    // here instead of ForEach() because modifying the collection raises an exception in
                    // .NET 4.5. It's best if the collection wasn't modified at all during iteration, but
                    // changing the code to accomplish that would probably break some things at this point.
                    for (int m = 0; m < level.ListCheck.Count; m++) {
                        // Obtain the Check object to be examined.
                        Check C = level.ListCheck[m];

                        try {
                            // Obtain the coordinates of this block.
                            level.IntToPos(C.b, out x, out y, out z);
                            // Used to keep track whether a block has been changed or not changed, depending on
                            // the block. It is best to figure out the context of this variable within a particular
                            // case. This variable is an example of why reusing variables can be so confusing.
                            bool InnerChange = false;
                            // Used for zombies in order to denote that a zombie cannot go in a certain direction.
                            bool skip = false;
                            // Used to store a random number. Seems to only be used for fire.
                            int storedRand = 0;
                            // Keeps track of a player that is closest to an AI block.
                            Player foundPlayer = null;
                            // Used as a temporary variable to store the distance between the closest player and
                            // the AI block. Also used to keep track of how many directions an AI block attempted
                            // to move on.
                            int foundNum = 75;
                            // Used as a temporary variable to store the distance between a player and an AI block.
                            // Also used to hold random numbers that determine the direction that an AI block should go.
                            int currentNum;
                            // Used to keep track of the former position of an AI block. Only used for snakes.
                            int oldNum;

                            // Obtain any extra physics-related information for this block.
                            string foundInfo = C.extraInfo;

                            newPhysic:
                            // Examine any physics-related information first. This means that any information
                            // specified in extraInfo has first priority.
                            if (foundInfo != String.Empty) {
                                int currentLoop = 0;

                                // Air blocks that are not currently waiting for another effect to be
                                // applied to them lose all physics-related information.
                                if (!foundInfo.Contains("wait"))
                                    if (level.blocks[C.b] == BlockId.Air) {
                                        C.extraInfo = String.Empty;
                                    }

                                // Store the information gleaned from extraInfo in these variables.
                                bool drop = false;
                                int dropnum = 0;
                                bool wait = false;
                                int waitnum = 0;
                                bool dissipate = false;
                                int dissipatenum = 0;
                                bool revert = false;
                                byte reverttype = 0;
                                bool explode = false;
                                int explodenum = 0;
                                bool finiteWater = false;
                                bool rainbow = false;
                                int rainbownum = 0;
                                bool door = false;

                                // Parse extraInfo. Any changes to C.extraInfo after this point are not
                                // reflected during the current iteration (unless the wait time has expired).
                                foreach (string s in C.extraInfo.Split(' ')) {
                                    // The format of extraInfo is: param1 value1 param2 value2 ...
                                    // Thus, the keywords are in the even indices, and their parameters are
                                    // in the odd indices.
                                    if (currentLoop % 2 == 0) {
                                        switch (s) {
                                        case "wait":
                                            wait = true;
                                            waitnum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;
                                        case "drop":
                                            drop = true;
                                            dropnum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;
                                        case "dissipate":
                                            dissipate = true;
                                            dissipatenum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;
                                        case "revert":
                                            revert = true;
                                            reverttype = byte.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;
                                        case "explode":
                                            explode = true;
                                            explodenum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;

                                        case "finite":
                                            finiteWater = true;
                                            break;

                                        case "rainbow":
                                            rainbow = true;
                                            rainbownum = int.Parse(C.extraInfo.Split(' ')[currentLoop + 1]);
                                            break;

                                        case "door":
                                            door = true;
                                            break;
                                        }
                                    }
                                    currentLoop++;
                                }

                                startCheck:
                                // Examine the wait parameter if it is present. The wait parameter causes all
                                // other extraInfo parameters to be delayed for the given number of physics
                                // loop iterations. However, "door 1" is an exception to this rule.
                                if (wait) {
                                    int storedInt = 0;
                                    // If the "door 1" string is part of the extraInfo, this block is actually
                                    // a tdoor that is waiting to be reverted back, so check if the immediately
                                    // adjacent blocks are all tdoors and change them to air as well.
                                    // ---
                                    // Why C.time < 2? That makes this run twice, since C.time starts at 0, but
                                    // AddUpdate prevents two Update objects from having the same block location,
                                    // so I suppose this is okay. - Jjp137
                                    if (door && C.time < 2) {
                                        storedInt = level.IntOffset(C.b, -1, 0, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 1, 0, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 1, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, -1, 0);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 0, 1);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                        storedInt = level.IntOffset(C.b, 0, 0, -1);
                                        if (BlockData.tDoor(level.blocks[storedInt])) {
                                            level.AddUpdate(storedInt, BlockId.Air, false, "wait 10 door 1 revert " +
                                                            Convert.ToInt32(level.blocks[storedInt]).ToString());
                                        }
                                    }

                                    // Once the number of physics loop iterations specified by the wait parameter has
                                    // passed, we are done waiting.
                                    if (waitnum <= C.time) {
                                        // We are not waiting anymore, so remove the "wait x" parameter from extraInfo.
                                        wait = false;
                                        C.extraInfo = C.extraInfo.Substring(0, C.extraInfo.IndexOf("wait ")) +
                                            C.extraInfo.Substring(C.extraInfo.IndexOf(' ', C.extraInfo.IndexOf("wait ") + 5) + 1);
                                        // This will jump back to the if statement that checks for the wait parameter.
                                        // The condition will be false, so all the other parameters will be examined.
                                        goto startCheck;
                                    }

                                    // Otherwise, that's one physics loop iteration done.
                                    C.time++;

                                    // The foundInfo variable is set to the empty string so that the extraInfo is
                                    // not processed again. This is done because the wait delay has not passed yet
                                    // if we reached this point, so we should not examine the extraInfo again.
                                    // Instead, the block-specific behavior should be looked at. Remember that
                                    // foundInfo and C.extraInfo are not the same variable, so the block still has
                                    // its information for the next iteration.
                                    foundInfo = String.Empty;
                                    // Since we jump back to right before the if statement, the condition will
                                    // evaluate to false since foundInfo is the empty string, so the else block
                                    // is executed instead.
                                    goto newPhysic;
                                }

                                // Due to the structure of the if blocks here, a block that has finite movement
                                // will have its other parameters ignored.
                                if (finiteWater) {
                                    // If the block is acting like a finite block, then process its movement. At the
                                    // end of the method, this block's extraInfo is cleared.
                                    level.finiteMovement(C, x, y, z);
                                }
                                // A block that has constantly changing colors of the rainbow will have its other
                                // parameters ignored.
                                else if (rainbow)
                                    // There is a small delay before the colors start changing.
                                    if (C.time < 4) {
                                        C.time++;
                                    }
                                    else {
                                        // If the parameter given is above 2, the colors change in the same order.
                                        if (rainbownum > 2) {
                                            // Force non-cloth blocks to turn to the red cloth block immediately
                                            // without waiting for the rest of the Check objects to be examined.
                                            if (level.blocks[C.b] < BlockId.Red || level.blocks[C.b] > BlockId.Pink) {
                                                level.AddUpdate(C.b, BlockId.Red, true);
                                            }
                                            // Otherwise, change a cloth block to the next one in the sequence,
                                            // wrapping around when necessary.
                                            else {
                                                if (level.blocks[C.b] == BlockId.Pink) {
                                                    level.AddUpdate(C.b, BlockId.Red);
                                                }
                                                else {
                                                    level.AddUpdate(C.b, (BlockId)(level.blocks[C.b] + 1));
                                                }
                                            }
                                        }
                                        // Otherwise, the colors change randomly.
                                        else {
                                            level.AddUpdate((int)C.b, (BlockId)rand.Next(21, 33));
                                        }
                                    }
                                // The below parameters can all be combined.
                                else {
                                    // Revert the block to the given block type, and clear the block's extraInfo.
                                    // This will not affect the current iteration of the physics loop.
                                    if (revert) {
                                        level.AddUpdate(C.b, (BlockId)reverttype);
                                        C.extraInfo = String.Empty;
                                    }
                                    // Check if the block should turn into air.
                                    if (dissipate)
                                        if (rand.Next(1, 100) <= dissipatenum) {
                                            // If a pending physics update does not already exist for this block,
                                            // which may happen if the revert parameter is passed, turn the block
                                            // into air.
                                            if (!level.ListUpdate.Exists(Update => Update.b == C.b)) {
                                                level.AddUpdate(C.b, BlockId.Air);
                                                // Clear the block's extraInfo. This affects what happens during the
                                                // next iteration, not the current one.
                                                C.extraInfo = String.Empty;
                                                // Drop is set to false here since otherwise a block may spawn below
                                                // the block that just disappeared, and these blocks would not have
                                                // any extraInfo associated with it, so they would just be there until
                                                // they are broken by a player.
                                                drop = false;
                                            }
                                            // Otherwise, make sure that the block stays the same. That is the intent,
                                            // at least, but nothing happens unless the block is sand or gravel since
                                            // the override parameter is false.
                                            else {
                                                level.AddUpdate(C.b, level.blocks[C.b], false, C.extraInfo);
                                            }
                                        }
                                    // Check if the block should explode.
                                    if (explode)
                                        if (rand.Next(1, 100) <= explodenum) {
                                            // Make the block explode.
                                            level.MakeExplosion(x, y, z, 0);
                                            // Clear the block's extraInfo since it is now air. This affects what
                                            // happens during the next iteration, not the current one.
                                            C.extraInfo = String.Empty;
                                            // Drop is set to false here since otherwise a block may spawn below
                                            // the block that just exploded, and these blocks would not have
                                            // any extraInfo associated with it, so they would just be there until
                                            // they are broken.
                                            drop = false;
                                        }
                                    // Check if the block should "drop" down by one.
                                    if (drop)
                                        if (rand.Next(1, 100) <= dropnum)
                                            // There needs to be space for the block to drop down to.
                                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air ||
                                                level.GetTile(x, (ushort)(y - 1), z) == BlockId.ActiveLava ||
                                                level.GetTile(x, (ushort)(y - 1), z) == BlockId.ActiveWater) {
                                                // I'm not sure why the first parameter is being checked... - Jjp137
                                                if (rand.Next(1, 100) < int.Parse(C.extraInfo.Split(' ')[1])) {
                                                    // Change the block below the current block being checked to
                                                    // the same block type, and change the current block to air if
                                                    // that block change is valid. That is how the "falling" effect
                                                    // is achieved.
                                                    if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z),
                                                        level.blocks[C.b], false, C.extraInfo)) {
                                                        level.AddUpdate(C.b, BlockId.Air);
                                                        // Clear the block's extraInfo since it is now air. This affects
                                                        // what happens during the next iteration, not the current one.
                                                        C.extraInfo = String.Empty;
                                                    }
                                                }
                                            }
                                }
                            }
                            else {
                                // Used to keep track of an AI block's potential new position.
                                int newNum;

                                // Each block behaves differently.
                                switch (level.blocks[C.b]) {
                                    case BlockId.Air:
                                        // Attempt to activate the physics of adjacent blocks if they are not
                                        // activated already.
                                        Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                        Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                        Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                        Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                        Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                        Physair(level, level.PosToInt(x, (ushort)(y - 1), z));

                                        // If the block being broken has allowed water to come from outside the map
                                        // and the "edge water" option is on, let the water flow into the map.
                                        if (level.edgeWater) {
                                            if (y < level.height / 2 && y >= (level.height / 2) - 2) {
                                                if (x == 0 || x == level.width - 1 || z == 0 || z == level.depth - 1) {
                                                    level.AddUpdate(C.b, BlockId.ActiveWater);
                                                }
                                            }
                                        }

                                        // If there are no other physics effects waiting to be applied, this block does
                                        // not need to be checked anymore.
                                        if (!C.extraInfo.Contains("wait")) {
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.Dirt:
                                        // If grass cannot grow in the level, then this block does not need to be
                                        // checked anymore.
                                        if (!level.GrassGrow) {
                                            C.time = 255;
                                            break;
                                        }

                                        // If enough time has passed and sunlight can reach the dirt block, change
                                        // it to a grass block.
                                        if (C.time > 20) {
                                            if (BlockData.LightPass(level.GetTile(x, (ushort)(y + 1), z))) {
                                                level.AddUpdate(C.b, BlockId.Grass);
                                            }
                                            C.time = 255;
                                        }
                                        // Otherwise, wait some more.
                                        else {
                                            C.time++;
                                        }
                                        break;

                                    case BlockId.Leaves:
                                        // Since some active liquid blocks can destroy leaves, reactivate the physics of
                                        // adjacent blocks since doing may cause inactive liquid blocks to flow again.
                                        // This only happens when physics is set to Advanced or higher.
                                        if (level.physics > 1)
                                        {
                                            Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                            Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                            Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                            // The block below is not checked since active liquids do not flow upwards.
                                        }

                                        // If leaves cannot decay, then this block does not need to be checked anymore.
                                        if (!level.leafDecay) {
                                            C.time = 255;
                                            leaves.Clear();
                                            break;
                                        }
                                        // Leaves do not start decaying immediately.
                                        if (C.time < 5) {
                                            if (rand.Next(10) == 0) {
                                                C.time++;
                                            }
                                            break;
                                        }

                                        // If the leaves are not close enough to tree wood, turn the leaves to air.
                                        if (PhysLeaf(level, C.b)) {
                                            level.AddUpdate(C.b, 0);
                                        }
                                        // Either way, we do not need to check this block again until an adjacent block
                                        // is modified in some way.
                                        C.time = 255;
                                        break;

                                    case BlockId.Sapling:
                                        // Since some active liquid blocks can destroy leaves, reactivate the physics of
                                        // adjacent blocks since doing may cause inactive liquid blocks to flow again.
                                        // This only happens when physics is set to Advanced or higher.
                                        if (level.physics > 1)
                                        {
                                            Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                            Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                            Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                            // The block below is not checked since active liquids do not flow upwards.
                                        }

                                        // If trees cannot grow, then this block does not need to be checked anymore.
                                        if (!level.growTrees) {
                                            C.time = 255;
                                            break;
                                        }
                                        // Trees do not grow immediately.
                                        if (C.time < 20) {
                                            if (rand.Next(20) == 0) {
                                                C.time++;
                                            }
                                            break;
                                        }
                                        // Grow the tree once enough time has passed.
                                        level.Server.MapGen.AddTree(level, x, y, z, rand, true, false);
                                        // The tree grew, so this block does not need to be checked anymore.
                                        C.time = 255;
                                        break;

                                    // Both active water and active death water share the same behavior.
                                    case BlockId.ActiveWater:
                                    case BlockId.ActiveColdWater:
                                        // Check if the finite liquid option is off.
                                        if (!level.finite) {
                                            // Check if water is set to flow randomly.
                                            if (level.randomFlow) {
                                                // Check if there are no sponges nearby.
                                                if (!PhysSpongeCheck(level, C.b)) {
                                                    // Add this block to the dictionary if it does not exist.
                                                    // The value is a boolean array that represents the directions
                                                    // that the water cannot go to anymore. This may be because another
                                                    // active water block exists there or because another block that
                                                    // water cannot interact with is on that spot. These directions do not
                                                    // include the block above the water block since water does not flow upwards.
                                                    if (!liquids.ContainsKey(C.b)) {
                                                        liquids.Add(C.b, new bool[5]);
                                                    }

                                                    // Check for any sand or gravel above the block.
                                                    if (level.GetTile(x, (ushort)(y + 1), z) != BlockId.Null) {
                                                        PhysSandCheck(level, level.PosToInt(x, (ushort)(y + 1), z));
                                                    }

                                                    // Randomly determine whether the water should go in a specific
                                                    // direction, and change the block in that direction if it will go
                                                    // that way. Record that the block in that direction is now water.
                                                    if (!liquids[C.b][0] && rand.Next(4) == 0) {
                                                        PhysWater(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                        liquids[C.b][0] = true;
                                                    }
                                                    if (!liquids[C.b][1] && rand.Next(4) == 0) {
                                                        PhysWater(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                        liquids[C.b][1] = true;
                                                    }
                                                    if (!liquids[C.b][2] && rand.Next(4) == 0) {
                                                        PhysWater(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                        liquids[C.b][2] = true;
                                                    }
                                                    if (!liquids[C.b][3] && rand.Next(4) == 0) {
                                                        PhysWater(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                        liquids[C.b][3] = true;
                                                    }
                                                    if (!liquids[C.b][4] && rand.Next(4) == 0) {
                                                        PhysWater(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                        liquids[C.b][4] = true;
                                                    }

                                                    // Check each direction (except above the water block) for blocks that
                                                    // are blocking the water's path and record these directions as blocked.
                                                    if (!liquids[C.b][0] && !PhysWaterCheck(level, level.PosToInt((ushort)(x + 1), y, z))) {
                                                        liquids[C.b][0] = true;
                                                    }
                                                    if (!liquids[C.b][1] && !PhysWaterCheck(level, level.PosToInt((ushort)(x - 1), y, z))) {
                                                        liquids[C.b][1] = true;
                                                    }
                                                    if (!liquids[C.b][2] && !PhysWaterCheck(level, level.PosToInt(x, y, (ushort)(z + 1)))) {
                                                        liquids[C.b][2] = true;
                                                    }
                                                    if (!liquids[C.b][3] && !PhysWaterCheck(level, level.PosToInt(x, y, (ushort)(z - 1)))) {
                                                        liquids[C.b][3] = true;
                                                    }
                                                    if (!liquids[C.b][4] && !PhysWaterCheck(level, level.PosToInt(x, (ushort)(y - 1), z))) {
                                                        liquids[C.b][4] = true;
                                                    }
                                                }
                                                // Otherwise, the active water is too close to a sponge, so turn it back
                                                // into air.
                                                else {
                                                    level.AddUpdate(C.b, BlockId.Air);

                                                    // If there are no other physics effects waiting to be applied, this block does
                                                    // not need to be checked anymore.
                                                    if (C.extraInfo.IndexOf("wait") == -1) {
                                                        C.time = 255;
                                                    }
                                                }

                                                // If this water block is completely surrounded by other blocks and
                                                // if there are no other physics effects waiting to be applied, this
                                                // block does not need to be looked at anymore. Remove the block from
                                                // the dictionary and mark the Check object as expired.
                                                if (C.extraInfo.IndexOf("wait") == -1 && liquids.ContainsKey(C.b))
                                                    if (liquids[C.b][0] && liquids[C.b][1] && liquids[C.b][2] &&
                                                        liquids[C.b][3] && liquids[C.b][4]) {
                                                        liquids.Remove(C.b);
                                                        C.time = 255;
                                                    }
                                            }
                                            // Otherwise, random flow is off, so spread out the water in as many directions
                                            // as possible.
                                            else {
                                                // Remove this block from the dictionary that is used for randomly
                                                // flowing water to avoid any weird behavior.
                                                if (liquids.ContainsKey(C.b)) {
                                                    liquids.Remove(C.b);
                                                }
                                                // Check if there are no sponges nearby.
                                                if (!PhysSpongeCheck(level, C.b)) {
                                                    // Check for any sand or gravel above the block.
                                                    if (level.GetTile(x, (ushort)(y + 1), z) != BlockId.Null) {
                                                        PhysSandCheck(level, level.PosToInt(x, (ushort)(y + 1), z));
                                                    }
                                                    // Let the water spread. The only direction missing is the block
                                                    // above the water since water does not flow upwards.
                                                    PhysWater(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                }
                                                // Otherwise, a sponge is too close to the water, so turn the water block
                                                // into air.
                                                else {
                                                    level.AddUpdate(C.b, BlockId.Air);
                                                }
                                                // If there are no other physics effects waiting to be applied, this block does
                                                // not need to be checked anymore.
                                                if (C.extraInfo.IndexOf("wait") == -1) {
                                                    C.time = 255;
                                                }
                                            }
                                        }
                                        // If finite mode is on, then this block will act like finite water.
                                        else {
                                            // Remove this block from the dictionary that is used for randomly
                                            // flowing water to avoid any weird behavior.
                                            if (liquids.ContainsKey(C.b)) {
                                                liquids.Remove(C.b);
                                            }
                                            // Jump to the finite water case. :( -Jjp137
                                            goto case BlockId.FiniteWater;
                                        }
                                        break;

                                    // This is the waterfall block.
                                    case BlockId.Waterfall:
                                        rand = new Random();

                                        // Figure out what to do based on the block below the waterfall block.
                                        switch (level.GetTile(x, (ushort)(y - 1), z)) {
                                            // If the block below is air, simply drop down by one block.
                                            case BlockId.Air:
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Waterfall);
                                                // If there are no other physics effects waiting to be applied, this block does
                                                // not need to be checked anymore.
                                                if (C.extraInfo.IndexOf("wait") == -1) {
                                                    C.time = 255;
                                                }
                                                break;
                                            // If the block below is an air flood block, then water cannot enter that block.
                                            case BlockId.AirFloodDown:
                                                break;
                                            // If the block below is a still liquid, then the waterfall stops there.
                                            case BlockId.StillLava:
                                            case BlockId.StillWater:
                                                break;
                                            default:
                                                // Otherwise, if the block below is a solid block and is not a waterfall
                                                // block, it should spread itself on that layer.
                                                if (level.GetTile(x, (ushort)(y - 1), z) != BlockId.Waterfall) {
                                                    PhysWater(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                    PhysWater(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                    // If there are no other physics effects waiting to be applied, this block does
                                                    // not need to be checked anymore.
                                                    if (C.extraInfo.IndexOf("wait") == -1) {
                                                        C.time = 255;
                                                    }
                                                }
                                                break;
                                        }
                                        break;

                                    // This is the lavafall block.
                                    case BlockId.Lavafall:
                                        rand = new Random();

                                        // Figure out what to do based on the block below the waterfall block.
                                        switch (level.GetTile(x, (ushort)(y - 1), z)) {
                                            // If the block below is air, simply drop down by one block.
                                            case BlockId.Air:
                                                // If there are no other physics effects waiting to be applied, this block does
                                                // not need to be checked anymore.
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Lavafall);
                                                if (C.extraInfo.IndexOf("wait") == -1) {
                                                    C.time = 255;
                                                }
                                                break;
                                            // If the block below is an air flood block, then lava cannot enter that block.
                                            case BlockId.AirFloodDown:
                                                break;
                                            // If the block below is a still liquid, then the lavafall stops there.
                                            case BlockId.StillLava:
                                            case BlockId.StillWater:
                                                break;
                                            default:
                                                // Otherwise, if the block below is a solid block and is not a lavafall
                                                // block, it should spread itself on that layer.
                                                if (level.GetTile(x, (ushort)(y - 1), z) != BlockId.Lavafall) {
                                                    PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                    // If there are no other physics effects waiting to be applied, this block does
                                                    // not need to be checked anymore.
                                                    if (C.extraInfo.IndexOf("wait") == -1) {
                                                        C.time = 255;
                                                    }
                                                }
                                                break;
                                        }
                                        break;

                                    case BlockId.WaterFaucet:
                                        // There is a small delay every time the faucet turns on and off.
                                        if (C.time < 2) {
                                            break;
                                        }

                                        // Reset the counter so that the faucet stays at the same state for at
                                        // least a little while due to the delay above.
                                        C.time = 0;

                                        // Determine whether to turn on or off the faucet based on the block below it.
                                        switch (level.GetTile(x, (ushort)(y - 1), z)) {
                                            // Turn off the faucet if the waterfall block is currently below it,
                                            // or keep the faucet off if air is below it.
                                            case BlockId.Waterfall:
                                            case BlockId.Air:
                                                if (rand.Next(1, 10) > 7)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.AirFloodDown);
                                                break;
                                            // Turn on the faucet if an air flood block is below it.
                                            case BlockId.AirFloodDown:
                                                if (rand.Next(1, 10) > 4)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Waterfall);
                                                break;
                                        }
                                        break;

                                    case BlockId.LavaFaucet:
                                        // There is a small delay every time the faucet turns on and off.
                                        C.time++;
                                        if (C.time < 2) {
                                            break;
                                        }

                                        // Reset the counter so that the faucet stays at the same state for at
                                        // least a little while due to the delay above.
                                        C.time = 0;

                                        // Determine whether to turn on or off the faucet based on the block below it.
                                        switch (level.GetTile(x, (ushort)(y - 1), z)) {
                                            // Turn off the faucet if the lavafall block is currently below it,
                                            // or keep the faucet off if air is below it.
                                            case BlockId.Lavafall:
                                            case BlockId.Air:
                                                if (rand.Next(1, 10) > 7)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.AirFloodDown);
                                                break;
                                           // Turn on the faucet if an air flood block is below it.
                                            case BlockId.AirFloodDown:
                                                if (rand.Next(1, 10) > 4)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Lavafall);
                                                break;
                                        }
                                        break;

                                    // Both active lava and active death lava share the same behavior.
                                    case BlockId.ActiveLava:
                                    case BlockId.ActiveHotLava:
                                        // There is a small delay before active lava starts spreading.
                                        if (C.time < 4) {
                                            C.time++;
                                            break;
                                        }
                                        // Check if the finite liquid option is off.
                                        if (!level.finite) {
                                            // Check if lava is set to flow randomly.
                                            if (level.randomFlow) {
                                                // Check if there are no lava sponges nearby.
                                                if (!PhysSpongeCheck(level, C.b, true)) {
                                                    // Force the next step to delay by a little bit.
                                                    C.time = (byte)rand.Next(3);

                                                    // Add this block to the dictionary if it does not exist.
                                                    // The value is a boolean array that represents the directions
                                                    // that the lava cannot go to anymore. This may be because another
                                                    // active lava block exists there or because another block that
                                                    // lava cannot interact with is on that spot. These directions do not
                                                    // include the block above the lava block since water does not flow upwards.
                                                    if (!liquids.ContainsKey(C.b)) {
                                                        liquids.Add(C.b, new bool[5]);
                                                    }

                                                    // Randomly determine whether the lava should go in a specific
                                                    // direction, and change the block in that direction if it will go
                                                    // that way. Record that the block in that direction is now lava.
                                                    if (!liquids[C.b][0] && rand.Next(4) == 0) {
                                                        PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                        liquids[C.b][0] = true;
                                                    }
                                                    if (!liquids[C.b][1] && rand.Next(4) == 0) {
                                                        PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                        liquids[C.b][1] = true;
                                                    }
                                                    if (!liquids[C.b][2] && rand.Next(4) == 0) {
                                                        PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                        liquids[C.b][2] = true;
                                                    }
                                                    if (!liquids[C.b][3] && rand.Next(4) == 0) {
                                                        PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                        liquids[C.b][3] = true;
                                                    }
                                                    if (!liquids[C.b][4] && rand.Next(4) == 0) {
                                                        PhysLava(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                        liquids[C.b][4] = true;
                                                    }

                                                    // Check each direction (except above the lava block) for blocks that
                                                    // are blocking the lava's path and record these directions as blocked.
                                                    if (!liquids[C.b][0] && !PhysLavaCheck(level, level.PosToInt((ushort)(x + 1), y, z))) {
                                                        liquids[C.b][0] = true;
                                                    }
                                                    if (!liquids[C.b][1] && !PhysLavaCheck(level, level.PosToInt((ushort)(x - 1), y, z))) {
                                                        liquids[C.b][1] = true;
                                                    }
                                                    if (!liquids[C.b][2] && !PhysLavaCheck(level, level.PosToInt(x, y, (ushort)(z + 1)))) {
                                                        liquids[C.b][2] = true;
                                                    }
                                                    if (!liquids[C.b][3] && !PhysLavaCheck(level, level.PosToInt(x, y, (ushort)(z - 1)))) {
                                                        liquids[C.b][3] = true;
                                                    }
                                                    if (!liquids[C.b][4] && !PhysLavaCheck(level, level.PosToInt(x, (ushort)(y - 1), z))) {
                                                        liquids[C.b][4] = true;
                                                    }
                                                }
                                                // Otherwise, the active lava is too close to a lava sponge, so turn it back
                                                // into air.
                                                else {
                                                    level.AddUpdate(C.b, BlockId.Air);

                                                    // If there are no other physics effects waiting to be applied, this block does
                                                    // not need to be checked anymore.
                                                    if (C.extraInfo.IndexOf("wait") == -1) {
                                                        C.time = 255;
                                                    }
                                                }

                                                // If this lava block is completely surrounded by other blocks and
                                                // if there are no other physics effects waiting to be applied, this
                                                // block does not need to be looked at anymore. Remove the block from
                                                // the dictionary and mark the Check object as expired.
                                                if (C.extraInfo.IndexOf("wait") == -1 && liquids.ContainsKey(C.b))
                                                    if (liquids[C.b][0] && liquids[C.b][1] && liquids[C.b][2] &&
                                                        liquids[C.b][3] && liquids[C.b][4]) {
                                                        liquids.Remove(C.b);
                                                        C.time = 255;
                                                    }
                                            }
                                            // Otherwise, random flow is off, so spread out the lava in as many directions
                                            // as possible.
                                            else {
                                                // Remove this block from the dictionary that is used for randomly
                                                // flowing water to avoid any weird behavior.
                                                if (liquids.ContainsKey(C.b)) {
                                                    liquids.Remove(C.b);
                                                }
                                                // Check if there are no lava sponges nearby.
                                                if (!PhysSpongeCheck(level, C.b, true)) {
                                                    // Let the lava spread. The only direction missing is the block
                                                    // above the water since lava does not flow upwards.
                                                    PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                    PhysLava(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                }
                                                // Otherwise, a lava sponge is too close to the lava, so turn the water block
                                                // into air.
                                                else {
                                                    level.AddUpdate(C.b, BlockId.Air);
                                                }

                                                // If there are no other physics effects waiting to be applied, this block does
                                                // not need to be checked anymore.
                                                if (C.extraInfo.IndexOf("wait") == -1) {
                                                    C.time = 255;
                                                }
                                            }
                                        }
                                        // If finite mode is on, then this block will act like finite lava.
                                        else {
                                            if (liquids.ContainsKey(C.b)) {
                                                liquids.Remove(C.b);
                                            }
                                            goto case BlockId.FiniteWater;
                                        }
                                        break;

                                        #region fire

                                    // This is the non-CPE fire block.
                                    case BlockId.Embers:
                                        // A short delay occurs before fire starts spreading.
                                        if (C.time < 2) {
                                            C.time++;
                                            break;
                                        }

                                        // If there is nothing flammable that fire can spread to, there is still a
                                        // very small chance that fire will spread on its own.
                                        storedRand = rand.Next(1, 20);
                                        if (storedRand < 2 && C.time % 2 == 0) {
                                            storedRand = rand.Next(1, 18);

                                            if (storedRand <= 3 && level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                            else if (storedRand <= 6 && level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                            else if (storedRand <= 9 && level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            else if (storedRand <= 12 && level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            else if (storedRand <= 15 && level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                            else if (storedRand <= 18 && level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                        }

                                        // Check for any air blocks that are adjacent to both a flammable block
                                        // and the fire block itself, and spread the fire to those blocks.
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), y, (ushort)(z - 1)))) {
                                            if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), y, (ushort)(z - 1)))) {
                                            if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), y, (ushort)(z + 1)))) {
                                            if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), y, (ushort)(z + 1)))) {
                                            if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile(x, (ushort)(y - 1), (ushort)(z - 1)))) {
                                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                        }
                                        // Burn any grass below the fire.
                                        else if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Grass) {
                                            level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Dirt);
                                        }

                                        // Continue checking for eligible blocks to spread to.
                                        if (BlockData.LavaKill(level.GetTile(x, (ushort)(y + 1), (ushort)(z - 1)))) {
                                            if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile(x, (ushort)(y - 1), (ushort)(z + 1)))) {
                                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile(x, (ushort)(y + 1), (ushort)(z + 1)))) {
                                            if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), (ushort)(y - 1), z))) {
                                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), (ushort)(y + 1), z))) {
                                            if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), (ushort)(y - 1), z))) {
                                            if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                        }
                                        if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), (ushort)(y + 1), z))) {
                                            if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                        }

                                        // Additional behavior occurs if physics is set to Advanced or higher.
                                        if (level.physics >= 2) {
                                            // Some time must pass until flammable blocks start burning.
                                            if (C.time < 4) {
                                                C.time++;
                                                break;
                                            }

                                            // Turn any adjacent flammable block into the fire block, or activate
                                            // any adjacent TNT blocks.
                                            if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), y, z)))
                                                level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Embers);
                                            else if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Tnt) {
                                                level.MakeExplosion((ushort)(x - 1), y, z, -1);
                                            }

                                            if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), y, z)))
                                                level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Embers);
                                            else if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Tnt) {
                                                level.MakeExplosion((ushort)(x + 1), y, z, -1);
                                            }

                                            if (BlockData.LavaKill(level.GetTile(x, (ushort)(y - 1), z)))
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Embers);
                                            else if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Tnt) {
                                                level.MakeExplosion(x, (ushort)(y - 1), z, -1);
                                            }

                                            if (BlockData.LavaKill(level.GetTile(x, (ushort)(y + 1), z)))
                                                level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Embers);
                                            else if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Tnt) {
                                                level.MakeExplosion(x, (ushort)(y + 1), z, -1);
                                            }

                                            if (BlockData.LavaKill(level.GetTile(x, y, (ushort)(z - 1))))
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Embers);
                                            else if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Tnt) {
                                                level.MakeExplosion(x, y, (ushort)(z - 1), -1);
                                            }

                                            if (BlockData.LavaKill(level.GetTile(x, y, (ushort)(z + 1))))
                                                level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Embers);
                                            else if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Tnt) {
                                                level.MakeExplosion(x, y, (ushort)(z + 1), -1);
                                            }
                                        }

                                        // Some time has passed.
                                        C.time++;

                                        // Once enough time has passed, there is a chance that the fire will
                                        // burn out. It may turn into a coal or obsidian block for a while,
                                        // or it may just disappear completely.
                                        if (C.time > 5) {
                                            storedRand = (rand.Next(1, 10));
                                            if (storedRand <= 2) {
                                                level.AddUpdate(C.b, BlockId.CoalOre);
                                                C.extraInfo = "drop 63 dissipate 10";
                                            }
                                            else if (storedRand <= 4) {
                                                level.AddUpdate(C.b, BlockId.Obsidian);
                                                C.extraInfo = "drop 63 dissipate 10";
                                            }
                                            else if (storedRand <= 8) {
                                                level.AddUpdate(C.b, BlockId.Air);
                                            }
                                            else {
                                                C.time = 3;
                                            }
                                        }

                                        break;

                                        #endregion

                                    case BlockId.FiniteWater:
                                    case BlockId.FiniteLava:
                                        // Process the movement of finite liquids. At the end of the method, this
                                        // block gets turned into air if it moved.
                                        level.finiteMovement(C, x, y, z);
                                        break;
                                    case BlockId.FiniteLavaFaucet:
                                        var bufferfinitefaucet1 = new List<ushort>();

                                        // Make a list of numbers from 0 to 5.
                                        for (ushort i = 0; i < 6; ++i) {
                                            bufferfinitefaucet1.Add(i);
                                        }
                                        // Shuffle the list.
                                        for (int k = bufferfinitefaucet1.Count - 1; k > 1; --k) {
                                            int randIndx = rand.Next(k);
                                            ushort temp = bufferfinitefaucet1[k];
                                            bufferfinitefaucet1[k] = bufferfinitefaucet1[randIndx];
                                            bufferfinitefaucet1[randIndx] = temp;
                                        }

                                        // Pick numbers from the list, one at a time. Each number corresponds to one
                                        // of the six adjacent blocks. If that adjacent block is an air block,
                                        // change it to a finite lava block. Otherwise, pick another number unless
                                        // we went through all the numbers already, which would mean that there are
                                        // no air blocks around the finite lava faucet.
                                        foreach (ushort i in bufferfinitefaucet1) {
                                            switch (i) {
                                                case 0:
                                                    if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                                case 1:
                                                    if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                                case 2:
                                                    if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                                case 3:
                                                    if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                                case 4:
                                                    if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                                case 5:
                                                    if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air) {
                                                        if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.FiniteLava)) {
                                                            InnerChange = true;
                                                        }
                                                    }
                                                    break;
                                            }

                                            // Break out of the loop if we changed an air block to a finite lava block.
                                            if (InnerChange) {
                                                break;
                                            }
                                        }

                                        break;
                                    case BlockId.FiniteWaterFaucet:
                                        var bufferfinitefaucet = new List<ushort>();

                                        // Make a list of numbers from 0 to 5.
                                        for (ushort i = 0; i < 6; ++i) {
                                            bufferfinitefaucet.Add(i);
                                        }
                                        // Shuffle the list.
                                        for (int k = bufferfinitefaucet.Count - 1; k > 1; --k) {
                                            int randIndx = rand.Next(k);
                                            ushort temp = bufferfinitefaucet[k];
                                            bufferfinitefaucet[k] = bufferfinitefaucet[randIndx];
                                            bufferfinitefaucet[randIndx] = temp;
                                        }

                                        // Pick numbers from the list, one at a time. Each number corresponds to one
                                        // of the six adjacent blocks. If that adjacent block is an air block,
                                        // change it to a finite water block. Otherwise, pick another number unless
                                        // we went through all the numbers already, which would mean that there are
                                        // no air blocks around the finite water faucet.
                                        foreach (ushort i in bufferfinitefaucet) {
                                            switch (i) {
                                            case 0:
                                                if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            case 1:
                                                if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            case 2:
                                                if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            case 3:
                                                if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            case 4:
                                                if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            case 5:
                                                if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air) {
                                                    if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.FiniteWater)) {
                                                        InnerChange = true;
                                                    }
                                                }
                                                break;
                                            }

                                            // Break out of the loop if we changed an air block to a finite water block.
                                            if (InnerChange) {
                                                break;
                                            }
                                        }

                                        break;

                                    case BlockId.Sand:
                                        // Make the sand fall down if it can.
                                        if (PhysSand(level, C.b, BlockId.Sand)) {
                                            // If it did fall down, activate the physics of blocks that were
                                            // adjacent to the sand block's former position.
                                            Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                            Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                            Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                            // The new position of the sand block is directly below the former
                                            // position, so do not check that block.
                                        }
                                        // Either way, the sand is in the correct place, so this block does not
                                        // need to be checked anymore.
                                        C.time = 255;
                                        break;

                                    case BlockId.Gravel:
                                        // Make the gravel fall down if it can.
                                        if (PhysSand(level, C.b, BlockId.Gravel)) {
                                            // If it did fall down, activate the physics of blocks that were
                                            // adjacent to the gravel block's former position.
                                            Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                            Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                            Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                            // The new position of the gravel block is directly below the former
                                            // position, so do not check that block.
                                        }
                                        // Either way, the gravel is in the correct place, so this block does not
                                        // need to be checked anymore.
                                        C.time = 255;
                                        break;

                                    case BlockId.Sponge:
                                        // Clear out any water near the sponge.
                                        PhysSponge(level, C.b);
                                        // After the water is cleared out, this block does not need to be checked anymore.
                                        C.time = 255;
                                        break;

                                    case BlockId.LavaSponge:
                                        // Clear out any lava near the sponge.
                                        PhysSponge(level, C.b, true);
                                        // After the lava is cleared out, this block does not need to be checked anymore.
                                        C.time = 255;
                                        break;

                                    // These blocks can be destroyed by active liquids or fire, which is why they
                                    // are grouped together.
                                    case BlockId.WoodPlanks:
                                    case BlockId.TreeWood:
                                    case BlockId.YellowFlower:
                                    case BlockId.RedFlower:
                                    case BlockId.BrownMushroom:
                                    case BlockId.RedMushroom:
                                    case BlockId.Bookshelf:
                                    case BlockId.Red:
                                    case BlockId.Orange:
                                    case BlockId.Yellow:
                                    case BlockId.Lime:
                                    case BlockId.Green:
                                    case BlockId.AquaGreen:
                                    case BlockId.Cyan:
                                    case BlockId.Blue:
                                    case BlockId.Purple:
                                    case BlockId.Indigo:
                                    case BlockId.Violet:
                                    case BlockId.Magenta:
                                    case BlockId.Pink:
                                    case BlockId.Black:
                                    case BlockId.Gray:
                                    case BlockId.White:
                                        // Attempt to activate the physics of adjacent blocks if they are not
                                        // activated already. This is only done if physics is set to Advanced or
                                        // higher since active liquids do not destroy blocks when physics is set
                                        // to Normal.
                                        if (level.physics > 1)
                                        {
                                            Physair(level, level.PosToInt((ushort)(x + 1), y, z));
                                            Physair(level, level.PosToInt((ushort)(x - 1), y, z));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z + 1)));
                                            Physair(level, level.PosToInt(x, y, (ushort)(z - 1)));
                                            Physair(level, level.PosToInt(x, (ushort)(y + 1), z));
                                            // The block below is not checked since active liquids do not flow upwards.
                                        }
                                        // This block does not need to be checked anymore after that.
                                        C.time = 255;
                                        break;

                                    case BlockId.Slab:
                                        // Check if another single stair block is below the current block, and if
                                        // so, combine the two blocks into a double stair block.
                                        PhysStair(level, C.b);
                                        // This block does not need to be checked anymore after that.
                                        C.time = 255;
                                        break;

                                    case BlockId.WoodFloat:
                                        // Calculate if the wood_float block should move, and move it if needed.
                                        PhysFloatwood(level, C.b);
                                        // This block does not need to be checked anymore after that.
                                        C.time = 255;
                                        break;

                                    // Both lava_fast and fast death lava share the same behavior.
                                    // Also, note that there are no delays.
                                    case BlockId.FastLava:
                                    case BlockId.FastHotLava:
                                        // Check if lava is set to flow randomly.
                                        if (level.randomFlow) {
                                            // Check if there are no lava sponges nearby.
                                            if (!PhysSpongeCheck(level, C.b, true)) {
                                                // Add this block to the dictionary if it does not exist.
                                                // The value is a boolean array that represents the directions
                                                // that the lava cannot go to anymore. This may be because another
                                                // active lava block exists there or because another block that
                                                // lava cannot interact with is on that spot. These directions do not
                                                // include the block above the lava block since water does not flow upwards.
                                                if (!liquids.ContainsKey(C.b)) {
                                                    liquids.Add(C.b, new bool[5]);
                                                }

                                                // Randomly determine whether the lava should go in a specific
                                                // direction, and change the block in that direction if it will go
                                                // that way. Record that the block in that direction is now lava.
                                                if (!liquids[C.b][0] && rand.Next(4) == 0) {
                                                    PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                    liquids[C.b][0] = true;
                                                }
                                                if (!liquids[C.b][1] && rand.Next(4) == 0) {
                                                    PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                    liquids[C.b][1] = true;
                                                }
                                                if (!liquids[C.b][2] && rand.Next(4) == 0) {
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                    liquids[C.b][2] = true;
                                                }
                                                if (!liquids[C.b][3] && rand.Next(4) == 0) {
                                                    PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                    liquids[C.b][3] = true;
                                                }
                                                if (!liquids[C.b][4] && rand.Next(4) == 0) {
                                                    PhysLava(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                    liquids[C.b][4] = true;
                                                }

                                                // Check each direction (except above the lava block) for blocks that
                                                // are blocking the lava's path and record these directions as blocked.
                                                if (!liquids[C.b][0] && !PhysLavaCheck(level, level.PosToInt((ushort)(x + 1), y, z))) {
                                                    liquids[C.b][0] = true;
                                                }
                                                if (!liquids[C.b][1] && !PhysLavaCheck(level, level.PosToInt((ushort)(x - 1), y, z))) {
                                                    liquids[C.b][1] = true;
                                                }
                                                if (!liquids[C.b][2] && !PhysLavaCheck(level, level.PosToInt(x, y, (ushort)(z + 1)))) {
                                                    liquids[C.b][2] = true;
                                                }
                                                if (!liquids[C.b][3] && !PhysLavaCheck(level, level.PosToInt(x, y, (ushort)(z - 1)))) {
                                                    liquids[C.b][3] = true;
                                                }
                                                if (!liquids[C.b][4] && !PhysLavaCheck(level, level.PosToInt(x, (ushort)(y - 1), z))) {
                                                    liquids[C.b][4] = true;
                                                }
                                            }
                                            // Otherwise, the active lava is too close to a lava sponge, so turn it back
                                            // into air.
                                            else {
                                                level.AddUpdate(C.b, BlockId.Air);

                                                // Since the active lava block turned into air, we do not need to
                                                // check this block anymore.
                                                C.time = 255;
                                            }

                                            // If this lava block is completely surrounded by other blocks, this
                                            // block does not need to be looked at anymore. Remove the block from
                                            // the dictionary and mark the Check object as expired.
                                            if (liquids.ContainsKey(C.b))
                                                if (liquids[C.b][0] && liquids[C.b][1] && liquids[C.b][2] &&
                                                    liquids[C.b][3] && liquids[C.b][4]) {
                                                    liquids.Remove(C.b);
                                                    C.time = 255;
                                                }
                                        }
                                        // Otherwise, random flow is off, so spread out the water in as many directions
                                        // as possible.
                                        else {
                                            // Remove this block from the dictionary that is used for randomly
                                            // flowing water to avoid any weird behavior.
                                            if (liquids.ContainsKey(C.b)) {
                                                liquids.Remove(C.b);
                                            }
                                            // Check if there are no lava sponges nearby.
                                            if (!PhysSpongeCheck(level, C.b, true)) {
                                                // Let the lava spread. The only direction missing is the block
                                                // above the water since lava does not flow upwards.
                                                PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                PhysLava(level, level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                            }
                                            // Otherwise, a lava sponge is too close to the lava, so turn the water block
                                            // into air.
                                            else {
                                                level.AddUpdate(C.b, BlockId.Air);
                                            }

                                            // The lava block has spread, so we do not need to check this block anymore.
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.AirFlood:
                                        // When the block is first checked, all adjacent active liquid blocks turn
                                        // into air_flood blocks.
                                        if (C.time < 1) {
                                            PhysairFlood(level, level.PosToInt((ushort)(x + 1), y, z), BlockId.AirFlood);
                                            PhysairFlood(level, level.PosToInt((ushort)(x - 1), y, z), BlockId.AirFlood);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z + 1)), BlockId.AirFlood);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z - 1)), BlockId.AirFlood);
                                            PhysairFlood(level, level.PosToInt(x, (ushort)(y - 1), z), BlockId.AirFlood);
                                            PhysairFlood(level, level.PosToInt(x, (ushort)(y + 1), z), BlockId.AirFlood);

                                            C.time++;
                                        }
                                        // When the block is checked afterwards, it turns into normal air. Due to this,
                                        // the block does not need to be checked anymore.
                                        else {
                                            level.AddUpdate(C.b, 0);
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.DoorTreeWoodActive:
                                    case BlockId.DoorObsidianActive:
                                    case BlockId.DoorGlassActive:
                                    case BlockId.DoorStoneActive:
                                    case BlockId.DoorLeavesActive:
                                    case BlockId.DoorSandActive:
                                    case BlockId.DoorWoodPlanksActive:
                                    case BlockId.DoorGreenActive:
                                    case BlockId.DoorSlabActive:
                                    case BlockId.DoorWaterActive:
                                    case BlockId.DoorLavaActive:
                                    case BlockId.DoorIronActive:
                                    case BlockId.DoorGoldActive:
                                    case BlockId.DoorCobblestoneActive:
                                    case BlockId.DoorRedActive:

                                    case BlockId.DoorOrangeActive:
                                    case BlockId.DoorYellowActive:
                                    case BlockId.DoorLimeActive:
                                    case BlockId.DoorAquaGreenActive:
                                    case BlockId.DoorCyanActive:
                                    case BlockId.DoorBlueActive:
                                    case BlockId.DoorIndigoActive:
                                    case BlockId.DoorVioletActive:
                                    case BlockId.DoorMagentaActive:
                                    case BlockId.DoorPinkActive:
                                    case BlockId.DoorBlackActive:
                                    case BlockId.DoorGrayActive:
                                    case BlockId.DoorWhiteActive:

                                    case BlockId.DoorDirtActive:
                                    case BlockId.DoorGrassActive:
                                    case BlockId.DoorPurpleActive:
                                    case BlockId.DoorBookshelfActive:
                                        // Activate adjacent doors if C.time == 0. Otherwise, the method checks
                                        // if 16 iterations have already passed, and if so, reverts the door_air
                                        // back to the equivalent door block.
                                        level.AnyDoor(C, x, y, z, 16);
                                        break;
                                    case BlockId.DoorAirSwitchActive:
                                    case BlockId.DoorAirActive:
                                        // The air_switch and air_door air blocks revert back in only 4 iterations.
                                        level.AnyDoor(C, x, y, z, 4, true);
                                        break;
                                    case BlockId.DoorTntActive:
                                        // The door_tnt's air block reverts back in only 4 iterations.
                                        level.AnyDoor(C, x, y, z, 4);
                                        break;

                                    case BlockId.ODoorTreeWoodActive:
                                    case BlockId.ODoorObsidianActive:
                                    case BlockId.ODoorGlassActive:
                                    case BlockId.ODoorStoneActive:
                                    case BlockId.ODoorLeavesActive:
                                    case BlockId.ODoorSandActive:
                                    case BlockId.ODoorWoodPlanksActive:
                                    case BlockId.ODoorGreenActive:
                                    case BlockId.ODoorTntActive:
                                    case BlockId.ODoorSlabActive:
                                    case BlockId.ODoorAirActive:
                                    case BlockId.ODoorWaterActive:

                                    case BlockId.ODoorTreeWood:
                                    case BlockId.ODoorObsidian:
                                    case BlockId.ODoorGlass:
                                    case BlockId.ODoorStone:
                                    case BlockId.ODoorLeaves:
                                    case BlockId.ODoorSand:
                                    case BlockId.ODoorWoodPlanks:
                                    case BlockId.ODoorGreen:
                                    case BlockId.ODoorTnt:
                                    case BlockId.ODoorSlab:
                                    case BlockId.ODoorAir:
                                    case BlockId.ODoorWater:
                                        // Toggle the state of the odoor. It's really, really buggy though.
                                        level.odoor(C);
                                        break;

                                    case BlockId.AirFloodLayer:
                                        // When the block is first checked, all adjacent active liquid blocks on the
                                        // same y coordinate turn into air_flood_layer blocks.
                                        if (C.time < 1) {
                                            PhysairFlood(level, level.PosToInt((ushort)(x + 1), y, z), BlockId.AirFloodLayer);
                                            PhysairFlood(level, level.PosToInt((ushort)(x - 1), y, z), BlockId.AirFloodLayer);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z + 1)), BlockId.AirFloodLayer);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z - 1)), BlockId.AirFloodLayer);

                                            C.time++;
                                        }
                                        // When the block is checked afterwards, it turns into normal air. Due to this,
                                        // the block does not need to be checked anymore.
                                        else {
                                            level.AddUpdate(C.b, 0);
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.AirFloodDown: //air_flood_down
                                        // When the block is first checked, all adjacent active liquid blocks on the
                                        // same y coordinate or below it turn into air_flood_down blocks.
                                        if (C.time < 1) {
                                            PhysairFlood(level, level.PosToInt((ushort)(x + 1), y, z), BlockId.AirFloodDown);
                                            PhysairFlood(level, level.PosToInt((ushort)(x - 1), y, z), BlockId.AirFloodDown);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z + 1)), BlockId.AirFloodDown);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z - 1)), BlockId.AirFloodDown);
                                            PhysairFlood(level, level.PosToInt(x, (ushort)(y - 1), z), BlockId.AirFloodDown);

                                            C.time++;
                                        }
                                        // When the block is checked afterwards, it turns into normal air. Due to this,
                                        // the block does not need to be checked anymore.
                                        else {
                                            level.AddUpdate(C.b, 0);
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.AirFloodUp:
                                        // When the block is first checked, all adjacent active liquid blocks on the
                                        // same y coordinate or above it turn into air_flood_down blocks.
                                        if (C.time < 1) {
                                            PhysairFlood(level, level.PosToInt((ushort)(x + 1), y, z), BlockId.AirFloodUp);
                                            PhysairFlood(level, level.PosToInt((ushort)(x - 1), y, z), BlockId.AirFloodUp);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z + 1)), BlockId.AirFloodUp);
                                            PhysairFlood(level, level.PosToInt(x, y, (ushort)(z - 1)), BlockId.AirFloodUp);
                                            PhysairFlood(level, level.PosToInt(x, (ushort)(y + 1), z), BlockId.AirFloodUp);

                                            C.time++;
                                        }
                                        // When the block is checked afterwards, it turns into normal air. Due to this,
                                        // the block does not need to be checked anymore.
                                        else {
                                            level.AddUpdate(C.b, 0);
                                            C.time = 255;
                                        }
                                        break;

                                    case BlockId.SmallTnt:
                                        // If physics is not Hardcore or Instant, just turn the TNT block into air.
                                        if (level.physics < 3) {
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }

                                        // Otherwise, blow up the TNT.
                                        if (level.physics >= 3) {
                                            rand = new Random();

                                            // If physics is not Instant, there is a short countdown before the TNT
                                            // blows up. This countdown is indicated by a block above the TNT block
                                            // that alternates between lava and air.
                                            if (C.time < 5 && level.physics == 3) {
                                                C.time += 1;
                                                level.Blockchange(x, (ushort)(y + 1), z, level.GetTile(x, (ushort)(y + 1), z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                break;
                                            }

                                            // The explosion for a small TNT block has a size of 0.
                                            level.MakeExplosion(x, y, z, 0);
                                        }
                                        else {  // What is this case for? - Jjp137
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }
                                        break;

                                    case BlockId.BigTnt:
                                        // If physics is not Hardcore or Instant, just turn the TNT block into air.
                                        if (level.physics < 3) {
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }

                                        // Otherwise, blow up the TNT.
                                        if (level.physics >= 3) {
                                            rand = new Random();

                                            // If physics is not Instant, there is a short countdown before the TNT
                                            // blows up. This countdown is indicated by blocks around the TNT block
                                            // that alternates between lava and air.
                                            if (C.time < 5 && level.physics == 3) {
                                                C.time += 1;
                                                level.Blockchange(x, (ushort)(y + 1), z, level.GetTile(x, (ushort)(y + 1), z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, (ushort)(y - 1), z, level.GetTile(x, (ushort)(y - 1), z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange((ushort)(x + 1), y, z, level.GetTile((ushort)(x + 1), y, z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange((ushort)(x - 1), y, z, level.GetTile((ushort)(x - 1), y, z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, y, (ushort)(z + 1), level.GetTile(x, y, (ushort)(z + 1)) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, y, (ushort)(z - 1), level.GetTile(x, y, (ushort)(z - 1)) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                break;
                                            }

                                            // The explosion for a small TNT block has a size of 1.
                                            level.MakeExplosion(x, y, z, 1);
                                        }
                                        else {  // What is this case for? - Jjp137
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }
                                        break;

                                    case BlockId.NukeTnt:
                                        // If physics is not Hardcore or Instant, just turn the TNT block into air.
                                        if (level.physics < 3) {
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }

                                        // Otherwise, blow up the TNT.
                                        if (level.physics >= 3) {
                                            rand = new Random();

                                            // If physics is not Instant, there is a short countdown before the TNT
                                            // blows up. This countdown is indicated by blocks around the TNT block
                                            // that alternates between lava and air.
                                            if (C.time < 5 && level.physics == 3) {
                                                C.time += 1;
                                                level.Blockchange(x, (ushort)(y + 2), z, level.GetTile(x, (ushort)(y + 2), z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, (ushort)(y - 2), z, level.GetTile(x, (ushort)(y - 2), z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange((ushort)(x + 1), y, z, level.GetTile((ushort)(x + 1), y, z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange((ushort)(x - 1), y, z, level.GetTile((ushort)(x - 1), y, z) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, y, (ushort)(z + 1), level.GetTile(x, y, (ushort)(z + 1)) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                level.Blockchange(x, y, (ushort)(z - 1), level.GetTile(x, y, (ushort)(z - 1)) == BlockId.StillLava ?
                                                                  (ushort)BlockId.Air : BlockId.StillLava);
                                                break;
                                            }

                                            // The explosion for a small TNT block has a size of 4.
                                            level.MakeExplosion(x, y, z, 4);
                                        }
                                        else {  // What is this case for? - Jjp137
                                            level.Blockchange(x, y, z, BlockId.Air);
                                        }
                                        break;

                                    case BlockId.TntExplosion:
                                        // The residue from TNT explosions has a 70% chance of disappearing.
                                        if (rand.Next(1, 11) <= 7) {
                                            level.AddUpdate(C.b, BlockId.Air);
                                        }
                                        break;

                                    case BlockId.Train:
                                        // These variables determine the order in which the blocks surrounding
                                        // the train get checked.
                                        if (rand.Next(1, 10) <= 5) {
                                            mx = 1;
                                        }
                                        else {
                                            mx = -1;
                                        }
                                        if (rand.Next(1, 10) <= 5) {
                                            my = 1;
                                        }
                                        else {
                                            my = -1;
                                        }
                                        if (rand.Next(1, 10) <= 5) {
                                            mz = 1;
                                        }
                                        else {
                                            mz = -1;
                                        }

                                        // Depending on the variables above, each for loop either goes through the
                                        // values [-1, 0, 1] or [1, 0, -1].
                                        for (int cx = (-1 * mx); cx != ((1 * mx) + mx); cx = cx + (1 * mx))
                                            for (int cy = (-1 * my); cy != ((1 * my) + my); cy = cy + (1 * my))
                                                for (int cz = (-1 * mz); cz != ((1 * mz) + mz); cz = cz + (1 * mz)) {
                                                    // If the block being checked is above red cloth and is air or water, the train can
                                                    // move there.
                                                    if (level.GetTile((ushort)(x + cx), (ushort)(y + cy - 1), (ushort)(z + cz)) == BlockId.Red &&
                                                        (level.GetTile((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)) == BlockId.Air ||
                                                         level.GetTile((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)) == BlockId.ActiveWater) &&
                                                        !InnerChange) {
                                                        level.AddUpdate(level.PosToInt((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)), BlockId.Train);
                                                        level.AddUpdate(level.PosToInt(x, y, z), BlockId.Air);
                                                        level.AddUpdate(level.IntOffset(C.b, 0, -1, 0), BlockId.Obsidian, true, 
                                                                        "wait 5 revert " + Convert.ToInt32(BlockId.Red).ToString());

                                                        InnerChange = true;
                                                        break;
                                                    }
                                                    // If the block being checked is above op_air and is air or water, the train can
                                                    // move there if it has not moved already.
                                                    if (level.GetTile((ushort)(x + cx), (ushort)(y + cy - 1), (ushort)(z + cz)) == BlockId.OpAir &&
                                                        (level.GetTile((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)) == BlockId.Air ||
                                                         level.GetTile((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)) == BlockId.ActiveWater) &&
                                                        !InnerChange) {
                                                        level.AddUpdate(level.PosToInt((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)), BlockId.Train);
                                                        level.AddUpdate(level.PosToInt(x, y, z), BlockId.Air);
                                                        level.AddUpdate(level.IntOffset(C.b, 0, -1, 0), BlockId.Glass, true, 
                                                                        "wait 5 revert " + Convert.ToInt32(BlockId.OpAir).ToString());

                                                        InnerChange = true;
                                                        break;
                                                    }
                                                }
                                        break;

                                    case BlockId.ActiveMagma:
                                        // Some time has passed.
                                        C.time++;
                                        // There is a small delay before active_magma starts moving.
                                        if (C.time < 3) {
                                            break;
                                        }

                                        // Active_magma acts like lavafall. It spreads downward until it hits a
                                        // solid block, then it spreads outward on the same layer. Note that it does
                                        // not destroy a flammable block that is below it. This is handled later.
                                        // It does immediately destroy most flammable blocks that are on the same
                                        // layer, though. The destruction occurs only on Advanced physics or higher.
                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                            level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.ActiveMagma);
                                        else if (level.GetTile(x, (ushort)(y - 1), z) != BlockId.ActiveMagma) {
                                            PhysLava(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                            PhysLava(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                            PhysLava(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                            PhysLava(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                        }

                                        // Additional behavior occurs if physics is set to Advanced or higher.
                                        if (level.physics > 1) {
                                            // There is a longer delay before this additional behavior occurs.
                                            if (C.time > 10) {
                                                C.time = 0;

                                                // Check for any adjacent blocks that can be destroyed by active_magma,
                                                // and destroy them. This differs from the previous step in many ways:
                                                // - This step, as stated above, has a longer delay.
                                                // - Flammable blocks below the active_magma block are destroyed.
                                                // This does not happen initially.
                                                // - Some blocks are not included in PhysLava() but are included in
                                                // LavaKill(), such as the bookshelf block.
                                                // - If the block below is also active_magma, then the previous step
                                                // would have not destroyed flammable blocks that are not on the same
                                                // layer since the condition in the if statement would be false. This
                                                // step ignores whether the block below is active_magma.
                                                if (BlockData.LavaKill(level.GetTile((ushort)(x + 1), y, z))) {
                                                    level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.ActiveMagma);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.LavaKill(level.GetTile((ushort)(x - 1), y, z))) {
                                                    level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.ActiveMagma);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.LavaKill(level.GetTile(x, y, (ushort)(z + 1)))) {
                                                    level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.ActiveMagma);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.LavaKill(level.GetTile(x, y, (ushort)(z - 1)))) {
                                                    level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.ActiveMagma);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.LavaKill(level.GetTile(x, (ushort)(y - 1), z))) {
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.ActiveMagma);
                                                    InnerChange = true;
                                                }

                                                // If the active_magma block destroyed an adjacent block, it can spread
                                                // upwards too if another flammable block is above it.
                                                if (InnerChange) {
                                                    if (BlockData.LavaKill(level.GetTile(x, (ushort)(y + 1), z)))
                                                        level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.ActiveMagma);
                                                }
                                            }
                                        }
                                        // Note that C.time is not set to 255 here, which means that these blocks
                                        // will be continuously checked forever if physics is Advanced or higher.

                                        break;
                                    case BlockId.Geyser:
                                        // Some time has passed.
                                        C.time++;

                                        // Geyser blocks acts like waterfall. It spreads downward until it hits a
                                        // solid block, then it spreads outward on the same layer. Note that it does
                                        // not destroy any eligible blocks that are below it. This is handled later.
                                        // It does immediately destroy most eligible blocks that are on the same layer,
                                        // though. The destruction occurs only on Advanced physics or higher.
                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                            level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Geyser);
                                        else if (level.GetTile(x, (ushort)(y - 1), z) != BlockId.Geyser) {
                                            PhysWater(level, level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                            PhysWater(level, level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                            PhysWater(level, level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                            PhysWater(level, level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                        }

                                        // Additional behavior occurs if physics is set to Advanced or higher.
                                        if (level.physics > 1) {
                                            // There is a longer delay before this additional behavior occurs.
                                            if (C.time > 10) {
                                                C.time = 0;

                                                // Check for any adjacent blocks that can be destroyed by active_magma,
                                                // and destroy them. This differs from the previous step in many ways:
                                                // - This step, as stated above, has a longer delay.
                                                // - Eligible blocks below the geyser block are destroyed.
                                                // This does not happen initially.
                                                // - Some blocks are not included in PhysWater() but are included in
                                                // WaterKill(), such as the leaves block.
                                                // - If the block below is also a geyser block, then the previous step
                                                // would have not destroyed eligible blocks that are not on the same
                                                // layer since the condition in the if statement would be false. This
                                                // step ignores whether the block below is a geyser block.
                                                if (BlockData.WaterKill(level.GetTile((ushort)(x + 1), y, z))) {
                                                    level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), BlockId.Geyser);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.WaterKill(level.GetTile((ushort)(x - 1), y, z))) {
                                                    level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), BlockId.Geyser);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.WaterKill(level.GetTile(x, y, (ushort)(z + 1)))) {
                                                    level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), BlockId.Geyser);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.WaterKill(level.GetTile(x, y, (ushort)(z - 1)))) {
                                                    level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), BlockId.Geyser);
                                                    InnerChange = true;
                                                }
                                                if (BlockData.WaterKill(level.GetTile(x, (ushort)(y - 1), z))) {
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), BlockId.Geyser);
                                                    InnerChange = true;
                                                }

                                                // If the active_magma block destroyed an adjacent block, it can spread
                                                // upwards too if another flammable block is above it.
                                                if (InnerChange) {
                                                    if (BlockData.WaterKill(level.GetTile(x, (ushort)(y + 1), z)))
                                                        level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Geyser);
                                                }
                                            }
                                        }
                                        // Note that C.time is not set to 255 here, which means that these blocks
                                        // will be continuously checked forever if physics is Advanced or higher.

                                        break;

                                    // These are the randomly moving birds.
                                    case BlockId.BirdBlack:
                                    case BlockId.BirdWhite:
                                    case BlockId.BirdLava:
                                    case BlockId.BirdWater:
                                        // Randomly decide where it should move.
                                        switch (rand.Next(1, 15)) {
                                            // These birds have a lower chance of changing their y coordinate. If
                                            // moving there is not possible because there is a non-air block there,
                                            // go to one of the cases where the bird attempts to move within the same
                                            // horizontal plane.
                                            case 1:  // Move down on the y axis.
                                                if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b]);
                                                else {
                                                    goto case 3;  // Left on the x axis
                                                }
                                                break;
                                            case 2:  // Move up on the y axis.
                                                if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                    level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), level.blocks[C.b]);
                                                else {
                                                    goto case 6;  // Right on the x axis
                                                }
                                                break;
                                            case 3:
                                            case 4:
                                            case 5:  // Move left on the x axis.
                                                // For each of these cases, move if the adjacent block being checked
                                                // is air, and don't move if the adjacent block is op_air, as dying
                                                // due to collisions with op_air would look weird. If it ran into a
                                                // solid block, kill the bird by turning it into a red cloth block
                                                // that eventually disappears.
                                                switch (level.GetTile((ushort)(x - 1), y, z)) {
                                                    case BlockId.Air:
                                                        level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b]);
                                                        break;
                                                    case BlockId.OpAir:
                                                        break;
                                                    default:
                                                        level.AddUpdate(C.b, BlockId.Red, false, "dissipate 25");
                                                        break;
                                                }
                                                break;
                                            case 6:
                                            case 7:
                                            case 8:  // Move right on the x axis.
                                                switch (level.GetTile((ushort)(x + 1), y, z)) {
                                                    case BlockId.Air:
                                                        level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b]);
                                                        break;
                                                    case BlockId.OpAir:
                                                        break;
                                                    default:
                                                        level.AddUpdate(C.b, BlockId.Red, false, "dissipate 25");
                                                        break;
                                                }
                                                break;
                                            case 9:
                                            case 10:
                                            case 11:  // Move down on the z axis.
                                                switch (level.GetTile(x, y, (ushort)(z - 1))) {
                                                    case BlockId.Air:
                                                        level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b]);
                                                        break;
                                                    case BlockId.OpAir:
                                                        break;
                                                    default:
                                                        level.AddUpdate(C.b, BlockId.Red, false, "dissipate 25");
                                                        break;
                                                }
                                                break;
                                            default:  // If the number is 12 to 14; move up on the z axis.
                                                switch (level.GetTile(x, y, (ushort)(z + 1))) {
                                                    case BlockId.Air:
                                                        level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b]);
                                                        break;
                                                    case BlockId.OpAir:
                                                        break;
                                                    default:
                                                        level.AddUpdate(C.b, BlockId.Red, false, "dissipate 25");
                                                        break;
                                                }
                                                break;
                                        }

                                        // Turn the block at the former position of the bird into air. Note that
                                        // if the bird did not do anything due to bumping into op_air, this would
                                        // actually cause the bird to disappear, which might be a bug. If the bird
                                        // turned into a red cloth block, this update would not be added since another
                                        // update exists at that position already.
                                        level.AddUpdate(C.b, BlockId.Air);
                                        // Since the block where the bird was at changed in some way, this block does
                                        // not need to be checked anymore.
                                        C.time = 255;

                                        break;

                                    case BlockId.SnakeTail:
                                        // If a snake tail is not next to a snake, revert it into air.
                                        // TODO: this is probably buggy and not working as intended
                                        if (level.GetTile(level.IntOffset(C.b, -1, 0, 0)) != BlockId.Snake ||
                                            level.GetTile(level.IntOffset(C.b, 1, 0, 0)) != BlockId.Snake ||
                                            level.GetTile(level.IntOffset(C.b, 0, 0, 1)) != BlockId.Snake ||
                                            level.GetTile(level.IntOffset(C.b, 0, 0, -1)) != BlockId.Snake) {
                                            C.extraInfo = "revert 0";
                                        }
                                        break;

                                    // Note: The snake code is really buggy.
                                    case BlockId.Snake:

                                        #region SNAKE

                                        // If the AI option for the level is turned on, find the nearest player in the
                                        // level that is not invincible.
                                        if (level.ai)
                                            level.Server.players.ForEach(delegate(Player p) {
                                                if (p.level == level && !p.invincible) {
                                                    // This is the distance between the snake block and the player. The
                                                    // player's coordinates are divided by 32 to convert them into block
                                                    // coordinates.
                                                    currentNum = Math.Abs((p.pos[0] / 32) - x) + Math.Abs((p.pos[1] / 32) - y) + Math.Abs((p.pos[2] / 32) - z);
                                                    // This is basically Math.Min().
                                                    if (currentNum < foundNum) {
                                                        foundNum = currentNum;
                                                        foundPlayer = p;
                                                    }
                                                }
                                            });

                                        bool inElse = false; // If it reached the else block
                                        while (!inElse) { // randomMovement_Snake
                                            // Follow the player if a player is found. There is a small chance that
                                            // the snake will move randomly even if a player was found.
                                            if (foundPlayer != null && rand.Next(1, 20) < 19) {
                                                // Pick a random number to determine the axis to follow the player along.
                                                currentNum = rand.Next(1, 10);  // 10 is "goto default", although 10 isn't a possible result
                                                foundNum = 0;

                                                // Attempt to move along the x axis.
                                                if (currentNum >= 1 && currentNum <= 3) { // case 1, case 2, case 3
                                                    // Don't move along the x axis if both the snake and the player
                                                    // are on the same x coordinate.
                                                    if ((foundPlayer.pos[0] / 32) - x != 0) {
                                                        // Determine which direction on the x axis to go to based on where the player is,
                                                        // and obtain the block at that position.
                                                        newNum = level.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        // Make sure that there is no block at that position.
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            // FIXME: This doesn't work properly because GetTile is not called.
                                                            // TODO: Figure out the intent of this condition.
                                                            if (level.IntOffset(newNum, -1, 0, 0) == Convert.ToInt32(BlockId.Grass) ||
                                                                level.IntOffset(newNum, -1, 0, 0) == Convert.ToInt32(BlockId.Dirt)) {
                                                                // If moving to that block is successful, go to the part of the code
                                                                // where the snake block is removed from the former position.
                                                                if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                    break;  // goto removeSelf_snake
                                                                }
                                                            }
                                                        }
                                                    }
                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the y axis.
                                                    else {
                                                        currentNum = 4; // goto case 4
                                                    }
                                                }

                                                // Attempt to move along the y axis.
                                                if (currentNum >= 4 && currentNum <= 6) { // case 4, case 5, case 6
                                                    // Don't move along the y axis if both the snake and the player
                                                    // are on the same y coordinate.
                                                    if ((foundPlayer.pos[1] / 32) - y != 0) {
                                                        // Determine which direction on the y axis to go to based on where the player is,
                                                        // and obtain the block at that position.
                                                        newNum = level.PosToInt(x, (ushort)(y + Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        // Make sure that there is no block at that position.
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            // This always happens since array indices are always positive.
                                                            if (newNum > 0) {
                                                                // FIXME: This doesn't work properly because GetTile is not called.
                                                                // TODO: Figure out the intent of this condition.
                                                                if (level.IntOffset(newNum, 0, 1, 0) == Convert.ToInt32(BlockId.Grass) ||
                                                                    level.IntOffset(newNum, 0, 1, 0) == Convert.ToInt32(BlockId.Dirt) &&
                                                                    level.IntOffset(newNum, 0, 2, 0) == Convert.ToInt32(BlockId.Air)) {
                                                                    // If moving to that block is successful, go to the part of the code
                                                                    // where the snake block is removed from the former position.
                                                                    if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                        break; // goto removeSelf_snake
                                                                    }
                                                                }
                                                            }
                                                            // FIXME: This never happens since array indices are never negative.
                                                            else if (newNum < 0) {
                                                                // FIXME: This doesn't work properly because GetTile is not called.
                                                                // TODO: Figure out the intent of this condition.
                                                                if (level.IntOffset(newNum, 0, -2, 0) == Convert.ToInt32(BlockId.Grass) ||
                                                                    level.IntOffset(newNum, 0, -2, 0) == Convert.ToInt32(BlockId.Dirt) &&
                                                                    level.IntOffset(newNum, 0, -1, 0) == Convert.ToInt32(BlockId.Air)) {
                                                                    // If moving to that block is successful, go to the part of the code
                                                                    // where the snake block is removed from the former position.
                                                                    if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                        break; // goto removeSelf_snake
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the z axis.
                                                    else {
                                                        currentNum = 7; // goto case 7
                                                    }
                                                }

                                                // Attempt to move along the z axis.
                                                if (currentNum >= 7 && currentNum <= 9) {
                                                    // Don't move along the z axis if both the snake and the player
                                                    // are on the same z coordinate.
                                                    if ((foundPlayer.pos[2] / 32) - z != 0) {
                                                        // Determine which direction on the z axis to go to based on where the player is,
                                                        // and obtain the block at that position.
                                                        newNum = level.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            // FIXME: This doesn't work properly because GetTile is not called.
                                                            // TODO: Figure out the intent of this condition.
                                                            if (level.IntOffset(newNum, 0, 0, -1) == Convert.ToInt32(BlockId.Grass) ||
                                                                level.IntOffset(newNum, 0, 0, -1) == Convert.ToInt32(BlockId.Dirt)) {
                                                                // If moving to that block is successful, go to the part of the code
                                                                // where the snake block is removed from the former position.
                                                                if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                    break; // goto removeSelf_snake
                                                                }
                                                            }
                                                        }
                                                    }
                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the x axis.
                                                    else {
                                                        currentNum = 1; // goto case 1
                                                    }
                                                }
                                                // This if block is executed if the snake could not move on any of the three axes.
                                                if (currentNum == 10) { // default
                                                    // Set this variable to null so that the condition within the if statement
                                                    // that will be jumped to is false. Since that if statement is false, the
                                                    // else block is executed instead, and that block simply picks a random
                                                    // direction for the snake to go to.
                                                    foundPlayer = null;
                                                    // Go back to the line right before the if statement.
                                                    continue;  // goto randomMovement_snake
                                                }
                                            }

                                            else {
                                                inElse = true;
                                                // Determine the direction that a snake will travel towards by
                                                // picking a random number between 1 and 12.
                                                switch (rand.Next(1, 13)) {
                                                    // This case is for going left on the x axis. Only this case is fully
                                                    // commented because the other cases are basically the same.
                                                    case 1:
                                                    case 2:
                                                    case 3:
                                                        // Store integers that correspond to the adjacent block being examined
                                                        // and the current position.
                                                        newNum = level.IntOffset(C.b, -1, 0, 0);
                                                        oldNum = level.PosToInt(x, y, z);

                                                        // If the adjacent block and the block below it are both air, the
                                                        // snake may be able to take a step down.
                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        // If the adjacent block and the block above it is air, just move
                                                        // to the adjacent block.
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        // If the adjacent block is solid, check if the two blocks above it are
                                                        // both air. If so, the snake may be able to take a step up.
                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        // Otherwise, do not do anything.
                                                        else {
                                                            skip = true;  // This variable is not used.
                                                        }

                                                        if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                            // If moving to that block is successful, place a tail block where the
                                                            // snake used to be and go to the part of the code where the snake
                                                            // block is removed from the former position.
                                                            // FIXME: The air block overrides the tail block. Also, this may cause
                                                            // the snake itself to die after 5 iterations if it ends up in the same
                                                            // place, but this is unconfirmed.
                                                            level.AddUpdate(level.IntOffset(oldNum, 0, 0, 0), BlockId.SnakeTail, true,
                                                                            "wait 5 revert 0");
                                                            break; // goto removeSelf_snake
                                                        }

                                                        // Note that the snake attempted to move in a direction.
                                                        foundNum++;
                                                        // If the snake attempted to move four times, note that the snake
                                                        // has not moved and stop attempting to move the snake for this iteration.
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                            break;
                                                        }
                                                        // Otherwise, attempt to move right on the x axis.
                                                        else {
                                                            goto case 4;
                                                        }

                                                    // This case is for going right on the x axis.
                                                    case 4:
                                                    case 5:
                                                    case 6:
                                                        newNum = level.IntOffset(C.b, 1, 0, 0);
                                                        oldNum = level.PosToInt(x, y, z);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                            level.AddUpdate(level.IntOffset(oldNum, 0, 0, 0), BlockId.SnakeTail, true,
                                                                            "wait 5 revert 0");
                                                            break; // goto removeSelf_snake
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                            break;
                                                        }
                                                        else {
                                                            goto case 7;  // Up on the z axis
                                                        }

                                                    // This case is for going up on the z axis.
                                                    case 7:
                                                    case 8:
                                                    case 9:
                                                        newNum = level.IntOffset(C.b, 0, 0, 1);
                                                        oldNum = level.PosToInt(x, y, z);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                            level.AddUpdate(level.IntOffset(oldNum, 0, 0, 0),
                                                                            BlockId.SnakeTail, true, "wait 5 revert 0");
                                                            break; // goto removeSelf_snake
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                            break;
                                                        }
                                                        else {
                                                            goto case 10;  // Down on the z axis.
                                                        }

                                                    // This case is for going down on the z axis.
                                                    case 10:
                                                    case 11:
                                                    case 12:
                                                    default:
                                                        newNum = level.IntOffset(C.b, 0, 0, -1);
                                                        oldNum = level.PosToInt(x, y, z);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                            level.AddUpdate(level.IntOffset(oldNum, 0, 0, 0),
                                                                            BlockId.SnakeTail, true, "wait 5 revert 0");
                                                            break; // goto removeSelf_snake
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                            break;
                                                        }
                                                        else {
                                                            goto case 1;  // Left on the x axis
                                                        }
                                                    // end cases
                                                }
                                            }
                                        }

                                        // removeSelf_snake
                                        // If the snake moved, change the block at its former position to air.
                                        if (!InnerChange) {
                                            level.AddUpdate(C.b, BlockId.Air);
                                        }
                                        break;

                                        #endregion

                                    // These birds actively go towards players.
                                    case BlockId.BirdRed:
                                    case BlockId.BirdBlue:
                                    case BlockId.BirdKill:

                                        #region HUNTER BIRDS

                                        // If the AI option for the level is turned on, find the nearest player in the
                                        // level that is not invincible.
                                        if (level.ai)
                                            level.Server.players.ForEach(delegate(Player p) {
                                                if (p.level == level && !p.invincible) {
                                                    // This is the distance between the bird block and the player. The
                                                    // player's coordinates are divided by 32 to convert them into block
                                                    // coordinates.
                                                    currentNum = Math.Abs((p.pos[0] / 32) - x) + Math.Abs((p.pos[1] / 32) - y) + Math.Abs((p.pos[2] / 32) - z);
                                                    // This is basically Math.Min().
                                                    if (currentNum < foundNum) {
                                                        foundNum = currentNum;
                                                        foundPlayer = p;
                                                    }
                                                }
                                            });

                                        bool inElseBird = false; // If it reached the else block
                                        while (!inElseBird) { //randomMovement
                                            // Follow the player if a player is found. There is a small chance that
                                            // the bird will move randomly even if a player was found.
                                            if (foundPlayer != null && rand.Next(1, 20) < 19) {
                                                // Pick a random number to determine the axis to follow the player along.
                                                currentNum = rand.Next(1, 10);  // 10 is "goto default", although 10 isn't a possible result
                                                foundNum = 0;

                                                // Attempt to move along the x axis. The cases are basically the same so only this case
                                                // is fully commented.
                                                if (currentNum >= 1 && currentNum <= 3) { // case 1, case 2, case 3
                                                    // Don't move along the x axis if both the bird and the player
                                                    // are on the same x coordinate.
                                                    if ((foundPlayer.pos[0] / 32) - x != 0) {
                                                        // Determine which direction on the x axis to go to based on where the player is,
                                                        // and obtain the block at that position.
                                                        newNum = level.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        // Make sure that there is no block at that position.
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            // If moving to that block is successful, go to the part of the code
                                                            // where the bird block is removed from the former position.
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf
                                                            }
                                                        }
                                                    }

                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the y axis.
                                                    else {
                                                        currentNum = 4; // goto case 4
                                                    }
                                                }
                                                // Attempt to move along the y axis in a similar manner.
                                                if (currentNum >= 4 && currentNum <= 6) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[1] / 32) - y != 0) {
                                                        newNum = level.PosToInt(x, (ushort)(y + Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the z axis.
                                                        currentNum = 7; // goto case 7
                                                    }
                                                }
                                                // Attempt to move along the z axis in a similar manner.
                                                if (currentNum >= 7 && currentNum <= 9) { // case 7, case 8, case 9
                                                    if ((foundPlayer.pos[2] / 32) - z != 0) {
                                                        newNum = level.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        if (level.GetTile(newNum) == BlockId.Air) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the x axis.
                                                        currentNum = 1; // goto case 1
                                                    }
                                                }
                                                // This if block is executed if the bird could not move on any of the three axes.
                                                if (currentNum == 10) { // default
                                                    // Set this variable to null so that the condition within the if statement
                                                    // that will be jumped to is false. Since that if statement is false, the
                                                    // else block is executed instead, and that block simply picks a random
                                                    // direction for the bird to go to.
                                                    foundPlayer = null;
                                                    // Go back to the line right before the if statement.
                                                    continue;  // goto randomMovement
                                                }
                                            }
                                            else {
                                                inElseBird = true;
                                                // Determine the direction that a bird will travel towards by
                                                // picking a random number between 1 and 14.
                                                switch (rand.Next(1, 15)) {
                                                    // This case is for going down on the y axis.
                                                    // For each case, move to the adjacent block if it is air.
                                                    // If moving to that block is successful, go to the part of the code
                                                    // where the bird block is removed from the former position.
                                                    case 1:
                                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air) {
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 3;  // Left on the x axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 3;  // Left on the x axis
                                                        }
                                                    // This case is for going up on the y axis.
                                                    case 2:
                                                        if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air)
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 6;  // Right on the x axis
                                                            }
                                                        else {
                                                            goto case 6;  // Right on the x axis
                                                        }
                                                    // This case is for going left on the x axis.
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                        if (level.GetTile((ushort)(x - 1), y, z) == BlockId.Air)
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 9;  // Down on the z axis
                                                            }
                                                        else {
                                                            goto case 9;  // Down on the z axis
                                                        }
                                                    // This case is for going right on the x axis.
                                                    case 6:
                                                    case 7:
                                                    case 8:
                                                        if (level.GetTile((ushort)(x + 1), y, z) == BlockId.Air)
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 12;  // Up on the z axis
                                                            }
                                                        else {
                                                            goto case 12;  // Up on the z axis
                                                        }
                                                    // This case is for going down on the z axis.
                                                    case 9:
                                                    case 10:
                                                    case 11:
                                                        if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.Air)
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    // This case is for going up on the z axis.
                                                    case 12:
                                                    case 13:
                                                    case 14:
                                                    default:
                                                        if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.Air)
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    }
                                                // end cases
                                            }
                                        }

                                        // removeSelf
                                        // If the bird moved, change the block at its former position to air.
                                        if (!InnerChange) {
                                            level.AddUpdate(C.b, BlockId.Air);
                                        }
                                        break;

                                        #endregion

                                    case BlockId.FishBetta:
                                    case BlockId.FishGold:
                                    case BlockId.FishSalmon:
                                    case BlockId.FishShark:
                                    case BlockId.FishSponge:

                                        #region FISH

                                        // If the AI option for the level is turned on, find the nearest player in the
                                        // level that is not invincible.
                                        if (level.ai)
                                            level.Server.players.ForEach(delegate(Player p) {
                                                if (p.level == level && !p.invincible) {
                                                    // This is the distance between the fish block and the player. The
                                                    // player's coordinates are divided by 32 to convert them into block
                                                    // coordinates.
                                                    currentNum = Math.Abs((p.pos[0] / 32) - x) + Math.Abs((p.pos[1] / 32) - y) + Math.Abs((p.pos[2] / 32) - z);
                                                    // This is basically Math.Min().
                                                    if (currentNum < foundNum) {
                                                        foundNum = currentNum;
                                                        foundPlayer = p;
                                                    }
                                                }
                                            });

                                        bool inElseFish = false; // If it reached the else block
                                        while (!inElseFish) { //randomMovement_fish
                                            // Follow the player if a player is found. There is a small chance that
                                            // the fish will move randomly even if a player was found.
                                            if (foundPlayer != null && rand.Next(1, 20) < 19) {
                                                // Pick a random number to determine the axis to follow the player along.
                                                currentNum = rand.Next(1, 10);  // 10 is "goto default", although 10 isn't a possible result
                                                foundNum = 0;

                                                // Attempt to move along the x axis. The cases are basically the same so only this case
                                                // is fully commented.
                                                if (currentNum >= 1 && currentNum <= 3) { // case 1, case 2, case 3
                                                    // Don't move along the x axis if both the fish and the player
                                                    // are on the same x coordinate.
                                                    if ((foundPlayer.pos[0] / 32) - x != 0) {
                                                        // Determine which direction on the x axis to go to based on where the player is,
                                                        // and obtain the block at that position. Some fish go towards the player, and
                                                        // other fish go away from the player.
                                                        if (level.blocks[C.b] == BlockId.FishBetta ||
                                                            level.blocks[C.b] == BlockId.FishShark) {
                                                            newNum = level.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        }
                                                        else {
                                                            newNum = level.PosToInt((ushort)(x - Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        }

                                                        // Make sure that there is active water at that position.
                                                        if (level.GetTile(newNum) == BlockId.ActiveWater) {
                                                            // If moving to that block is successful, go to the part of the code
                                                            // where the fish block is removed from the former position.
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_fish
                                                            }
                                                        }
                                                    }

                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the y axis.
                                                    else {
                                                        currentNum = 4; // goto case 4
                                                    }
                                                }
                                                // Attempt to move along the y axis in a similar manner.
                                                if (currentNum >= 4 && currentNum <= 6) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[1] / 32) - y != 0) {
                                                        if (level.blocks[C.b] == BlockId.FishBetta ||
                                                            level.blocks[C.b] == BlockId.FishShark) {
                                                            newNum = level.PosToInt(x, (ushort)(y + Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        }
                                                        else {
                                                            newNum = level.PosToInt(x, (ushort)(y - Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        }

                                                        if (level.GetTile(newNum) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_fish
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the z axis.
                                                        currentNum = 7; // goto case 7
                                                    }
                                                }
                                                // Attempt to move along the z axis in a similar manner.
                                                if (currentNum >= 7 && currentNum <= 9) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[2] / 32) - z != 0) {
                                                        if (level.blocks[C.b] == BlockId.FishBetta ||
                                                            level.blocks[C.b] == BlockId.FishShark) {
                                                            newNum = level.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        }
                                                        else {
                                                            newNum = level.PosToInt(x, y, (ushort)(z - Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        }

                                                        if (level.GetTile(newNum) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_fish
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the x axis.
                                                        currentNum = 1; // goto case 1
                                                    }
                                                }
                                                // This if block is executed if the fish could not move on any of the three axes.
                                                if (currentNum == 10) { // default
                                                    // Set this variable to null so that the condition within the if statement
                                                    // that will be jumped to is false. Since that if statement is false, the
                                                    // else block is executed instead, and that block simply picks a random
                                                    // direction for the fish to go to.
                                                    foundPlayer = null;
                                                    // Go back to the line right before the if statement.
                                                    continue; // goto randomMovement_fish
                                                }
                                            }
                                            else {
                                                inElseFish = true;
                                                // Determine the direction that a fish will travel towards by
                                                // picking a random number between 1 and 14.
                                                switch (rand.Next(1, 15)) {
                                                    // This case is for going down on the y axis.
                                                    // For each case, move to the adjacent block if it is active water.
                                                    // If moving to that block is successful, go to the part of the code
                                                    // where the fish block is removed from the former position.
                                                    case 1:
                                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 3;  // Left on the x axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 3;  // Left on the x axis
                                                        }
                                                    // This case is for going up on the y axis.
                                                    case 2:
                                                        if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 6;  // Right on the x axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 6;  // Right on the x axis
                                                        }
                                                    // This case is for going left on the x axis.
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                        if (level.GetTile((ushort)(x - 1), y, z) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 9;  // Down on the z axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 9;  // Down on the z axis
                                                        }
                                                    // This case is for going right on the x axis.
                                                    case 6:
                                                    case 7:
                                                    case 8:
                                                        if (level.GetTile((ushort)(x + 1), y, z) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 12;  // Up on the z axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 12;  // Up on the z axis
                                                        }
                                                    // This case is for going down on the z axis.
                                                    case 9:
                                                    case 10:
                                                    case 11:
                                                        if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    // This case is for going up on the z axis.
                                                    case 12:
                                                    case 13:
                                                    case 14:
                                                    default:
                                                        if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.ActiveWater) {
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    }
                                                // end cases
                                            }
                                        }

                                        // removeSelf_fish
                                        // If the fish moved, change the block at its former position to active water.
                                        if (!InnerChange) {
                                            level.AddUpdate(C.b, BlockId.ActiveWater);
                                        }
                                        break;

                                        #endregion

                                    case BlockId.FishLavaShark:

                                        #region lavafish

                                        // If the AI option for the level is turned on, find the nearest player in the
                                        // level that is not invincible.
                                        if (level.ai)
                                            level.Server.players.ForEach(delegate(Player p) {
                                                if (p.level == level && !p.invincible) {
                                                    // This is the distance between the lava shark block and the player. The
                                                    // player's coordinates are divided by 32 to convert them into block
                                                    // coordinates.
                                                    currentNum = Math.Abs((p.pos[0] / 32) - x) + Math.Abs((p.pos[1] / 32) - y) + Math.Abs((p.pos[2] / 32) - z);
                                                    // This is basically Math.Min().
                                                    if (currentNum < foundNum) {
                                                        foundNum = currentNum;
                                                        foundPlayer = p;
                                                    }
                                                }
                                            });

                                        bool inElseLavaShark = false; // If it reached the else block
                                        while (!inElseLavaShark) { //randomMovement_lavafish
                                            // Follow the player if a player is found. There is a small chance that
                                            // the lava shark will move randomly even if a player was found.
                                            if (foundPlayer != null && rand.Next(1, 20) < 19) {
                                                // Pick a random number to determine the axis to follow the player along.
                                                currentNum = rand.Next(1, 10); // 10 is "goto default"
                                                foundNum = 0;

                                                // Attempt to move along the x axis. The cases are basically the same so only this case
                                                // is fully commented.
                                                if (currentNum >= 1 && currentNum <= 3) { // case 1, case 2, case 3
                                                    // Don't move along the x axis if both the lava shark and the player
                                                    // are on the same x coordinate.
                                                    if ((foundPlayer.pos[0] / 32) - x != 0) {
                                                        // Determine which direction on the x axis to go to based on where the player is,
                                                        // and obtain the block at that position. The if condition is always true, so the
                                                        // lava shark always go towards the player if possible.
                                                        if (level.blocks[C.b] == BlockId.FishLavaShark) {
                                                            newNum = level.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        }
                                                        else {
                                                            newNum = level.PosToInt((ushort)(x - Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);
                                                        }

                                                        // Make sure that there is active lava at that position.
                                                        if (level.GetTile(newNum) == BlockId.ActiveLava) {
                                                            // If moving to that block is successful, go to the part of the code
                                                            // where the lava shark block is removed from the former position.
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_lavafish
                                                            }
                                                        }
                                                    }

                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the block has tried to follow the player on all three axes but
                                                    // could not, resort to random movement.
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the y axis.
                                                    else {
                                                        currentNum = 4; // goto case 4
                                                    }
                                                }
                                                // Attempt to move along the y axis in a similar manner.
                                                if (currentNum >= 4 && currentNum <= 6) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[1] / 32) - y != 0) {
                                                        if (level.blocks[C.b] == BlockId.FishLavaShark) {
                                                            newNum = level.PosToInt(x, (ushort)(y + Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        }
                                                        else {
                                                            newNum = level.PosToInt(x, (ushort)(y - Math.Sign((foundPlayer.pos[1] / 32) - y)), z);
                                                        }

                                                        if (level.GetTile(newNum) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_lavafish
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the z axis.
                                                        currentNum = 7; // goto case 7
                                                    }
                                                }
                                                // Attempt to move along the z axis in a similar manner.
                                                if (currentNum >= 7 && currentNum <= 9) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[2] / 32) - z != 0) {
                                                        if (level.blocks[C.b] == BlockId.FishLavaShark) {
                                                            newNum = level.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        }
                                                        else {
                                                            newNum = level.PosToInt(x, y, (ushort)(z - Math.Sign((foundPlayer.pos[2] / 32) - z)));
                                                        }

                                                        if (level.GetTile(newNum) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                break; // goto removeSelf_lavafish
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 3) {
                                                        currentNum = 10; // goto default
                                                    }
                                                    else {  // Try the x axis.
                                                        currentNum = 1; // goto case 1
                                                    }
                                                }
                                                // This if block is executed if the lava shark could not move on any of the three axes.
                                                if (currentNum == 10) { // default
                                                    // Set this variable to null so that the condition within the if statement
                                                    // that will be jumped to is false. Since that if statement is false, the
                                                    // else block is executed instead, and that block simply picks a random
                                                    // direction for the lava shark to go to.
                                                    foundPlayer = null;
                                                    // Go back to the line right before the if statement.
                                                    continue; // goto randomMovement_lavafish
                                                }
                                            }
                                            else {
                                                inElseLavaShark = true;
                                                // Determine the direction that a lava shark will travel towards by
                                                // picking a random number between 1 and 14.
                                                switch (rand.Next(1, 15)) {
                                                    // This case is for going down on the y axis.
                                                    // For each case, move to the adjacent block if it is active lava.
                                                    // If moving to that block is successful, go to the part of the code
                                                    // where the lava shark block is removed from the former position.
                                                    case 1:
                                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y - 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 3;  // Left on the x axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 3;  // Left on the x axis
                                                        }
                                                    // This case is for going up on the y axis.
                                                    case 2:
                                                        if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 6;  // Right on the x axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 6;  // Right on the x axis
                                                        }
                                                    // This case is for going left on the x axis.
                                                    case 3:
                                                    case 4:
                                                    case 5:
                                                        if (level.GetTile((ushort)(x - 1), y, z) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x - 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 9;  // Down on the z axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 9;  // Down on the z axis
                                                        }
                                                    // This case is for going right on the x axis.
                                                    case 6:
                                                    case 7:
                                                    case 8:
                                                        if (level.GetTile((ushort)(x + 1), y, z) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt((ushort)(x + 1), y, z), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            else {
                                                                goto case 12;  // Up on the z axis
                                                            }
                                                        }
                                                        else {
                                                            goto case 12;  // Up on the z axis
                                                        }
                                                    // This case is for going down on the z axis.
                                                    case 9:
                                                    case 10:
                                                    case 11:
                                                        if (level.GetTile(x, y, (ushort)(z - 1)) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z - 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    // This case is for going up on the z axis.
                                                    case 12:
                                                    case 13:
                                                    case 14:
                                                    default:
                                                        if (level.GetTile(x, y, (ushort)(z + 1)) == BlockId.ActiveLava) {
                                                            if (level.AddUpdate(level.PosToInt(x, y, (ushort)(z + 1)), level.blocks[C.b])) {
                                                                break;
                                                            }
                                                            // Mark that the block has not moved if this fails.
                                                            else {
                                                                InnerChange = true;
                                                            }
                                                        }
                                                        else {
                                                            InnerChange = true;
                                                        }
                                                        break;
                                                    }
                                                // end cases
                                            }
                                        }

                                        // removeSelf_lavafish
                                        // If the lava shark moved, change the block at its former position to active lava.
                                        if (!InnerChange) {
                                            level.AddUpdate(C.b, BlockId.ActiveLava);
                                        }
                                        break;

                                        #endregion

                                    case BlockId.RocketHead:
                                        // These variables determine the order in which the blocks surrounding
                                        // the train get checked.
                                        if (rand.Next(1, 10) <= 5) {
                                            mx = 1;
                                        }
                                        else {
                                            mx = -1;
                                        }
                                        if (rand.Next(1, 10) <= 5) {
                                            my = 1;
                                        }
                                        else {
                                            my = -1;
                                        }
                                        if (rand.Next(1, 10) <= 5) {
                                            mz = 1;
                                        }
                                        else {
                                            mz = -1;
                                        }

                                        // Depending on the variables above, each for loop either goes through the
                                        // values [-1, 0, 1] or [1, 0, -1].
                                        for (int cx = (-1 * mx); cx != ((1 * mx) + mx) && InnerChange == false; cx = cx + (1 * mx))
                                            for (int cy = (-1 * my); cy != ((1 * my) + my) && InnerChange == false; cy = cy + (1 * my))
                                                for (int cz = (-1 * mz); cz != ((1 * mz) + mz) && InnerChange == false; cz = cz + (1 * mz)) {
                                                    // Check if the block is the rocket's fire trail, which indicates the direction of
                                                    // the rocket. In reality, more than one block around the rocket can be an embers block,
                                                    // which would make the behavior more complicated. Oh well.
                                                    if (level.GetTile((ushort)(x + cx), (ushort)(y + cy), (ushort)(z + cz)) == BlockId.Embers) {
                                                        // Check if the block being checked and the block on the opposite side of the fire
                                                        // trail has no pending physics updates, as that would prevent the rocket from moving
                                                        // forward.
                                                        int bp1 = level.PosToInt((ushort)(x - cx), (ushort)(y - cy), (ushort)(z - cz));
                                                        int bp2 = level.PosToInt(x, y, z);
                                                        bool unblocked = !level.ListUpdate.Exists(Update => Update.b == bp1) &&
                                                                         !level.ListUpdate.Exists(Update => Update.b == bp2);
                                                        // If the above is true and the block ahead of the rocket is air, change the air block to
                                                        // the rocket head and the rocket's former position to the embers block. The if condition
                                                        // is also true if the block ahead is rocketstart, but that just results in the rocket
                                                        // disappearing since BlockId.OPBlocks() returns true for a rocketstart block, which results
                                                        // in the physics-related overload of Level.Blockchange() not doing anything.
                                                        if (unblocked && level.GetTile((ushort)(x - cx), (ushort)(y - cy), (ushort)(z - cz)) == BlockId.Air ||
                                                            level.GetTile((ushort)(x - cx), (ushort)(y - cy), (ushort)(z - cz)) == BlockId.RocketStart) {
                                                            level.AddUpdate(level.PosToInt((ushort)(x - cx), (ushort)(y - cy), (ushort)(z - cz)), BlockId.RocketHead);
                                                            level.AddUpdate(level.PosToInt(x, y, z), BlockId.Embers);
                                                        }
                                                        // Do not do anything if the block opposite of the fire trail is also an embers block.
                                                        else if (level.GetTile((ushort)(x - cx), (ushort)(y - cy), (ushort)(z - cz)) == BlockId.Embers) { }
                                                        // If the rocket hits a solid wall, make it explode or turn it into an embers block,
                                                        // depending on the physics setting.
                                                        else {
                                                            if (level.physics > 2) {
                                                                level.MakeExplosion(x, y, z, 2);
                                                            }
                                                            else
                                                                level.AddUpdate(level.PosToInt(x, y, z), BlockId.Embers);
                                                        }
                                                        // Since we found the fire trail, do not do anything else and break
                                                        // out of the loop.
                                                        InnerChange = true;
                                                    }
                                                }
                                        break;

                                    case BlockId.Firework:
                                        // Do not do anything if the block below the firework is not lava.
                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.StillLava) {
                                            // If the block above the firework is air, it can still go higher.
                                            if (level.GetTile(x, (ushort)(y + 1), z) == BlockId.Air) {
                                                // The firework is guaranteed to go up if it is too low on the y axis.
                                                // The threshold is determined by the level's height.
                                                if ((level.height / 100) * 80 < y) {
                                                    mx = rand.Next(1, 20);
                                                }
                                                else {
                                                    mx = 5;
                                                }

                                                // There is a 1/19 chance if the firework is high enough that this
                                                // code does not execute, which will cause the firework block to explode.
                                                if (mx > 1) {
                                                    // Check if the above block does not have a pending physics update
                                                    // associated with it.
                                                    int bp = level.PosToInt(x, (ushort)(y + 1), z);
                                                    bool unblocked = !level.ListUpdate.Exists(Update => Update.b == bp);
                                                    // If there is no physics update associated with the above block,
                                                    // have the firework and its lava trail go up by one block.
                                                    if (unblocked) {
                                                        level.AddUpdate(level.PosToInt(x, (ushort)(y + 1), z), BlockId.Firework, false);
                                                        level.AddUpdate(level.PosToInt(x, y, z), BlockId.StillLava, false, "wait 1 dissipate 100");
                                                        C.extraInfo = "wait 1 dissipate 100";
                                                        // Break out of this case so that the firework block does not
                                                        // explode into cloth blocks.
                                                        break;
                                                    }
                                                }
                                            }
                                            // If the switch-case loop was not broken out of, then the firework
                                            // did not go up. Thus, make it explode into cloth blocks.
                                            level.Firework(x, y, z, 4);
                                            break;
                                        }
                                        break;

                                    case BlockId.ZombieHead:
                                        // Remove zombie head blocks that are by themselves.
                                        if (level.GetTile(level.IntOffset(C.b, 0, -1, 0)) != BlockId.ZombieBody &&
                                            level.GetTile(level.IntOffset(C.b, 0, -1, 0)) != BlockId.Creeper) {
                                            C.extraInfo = "revert 0";
                                        }
                                        break;

                                    // This code is for zombies and creepers.
                                    case BlockId.ZombieBody:
                                    case BlockId.Creeper:

                                        #region ZOMBIE

                                        // If the block below the zombie is air, then the zombie should fall downwards.
                                        // Shift the zombie's position by one block downwards.
                                        if (level.GetTile(x, (ushort)(y - 1), z) == BlockId.Air) {
                                            level.AddUpdate(C.b, BlockId.ZombieHead);
                                            level.AddUpdate(level.IntOffset(C.b, 0, -1, 0), level.blocks[C.b]);
                                            level.AddUpdate(level.IntOffset(C.b, 0, 1, 0), BlockId.Air);
                                            // Do not do anything else during this iteration.
                                            break;
                                        }

                                        // If the AI option for the level is turned on, find the nearest player in the
                                        // level that is not invincible.
                                        if (level.ai)
                                                level.Server.players.ForEach(delegate(Player p) {
                                                if (p.level == level && !p.invincible) {
                                                    // This is the distance between the zombie block and the player. The
                                                    // player's coordinates are divided by 32 to convert them into block
                                                    // coordinates.
                                                    currentNum = Math.Abs((p.pos[0] / 32) - x) + Math.Abs((p.pos[1] / 32) - y) + Math.Abs((p.pos[2] / 32) - z);
                                                    // This is basically Math.Min().
                                                    if (currentNum < foundNum) {
                                                        foundNum = currentNum;
                                                        foundPlayer = p;
                                                    }
                                                }
                                            });

                                        bool addedTime = false; // For the if block with C.time++ in it
                                        bool inElseZombie = false; // If the else block has been reached
                                        while (!inElseZombie) { // randomMovement_zomb:
                                            // Follow the player if a player is found. There is a small chance that
                                            // the zombie will move randomly even if a player was found.
                                            if (foundPlayer != null && rand.Next(1, 20) < 18) {
                                                // Pick a random number to determine the axis to follow the player along.
                                                currentNum = rand.Next(1, 7); // 7 is "goto default"
                                                foundNum = 0;

                                                // Attempt to move along the x axis. The cases are basically the same so only this case
                                                // is fully commented.
                                                if (currentNum >= 1 && currentNum <= 3) { // case 1, case 2, case 3
                                                    // Don't move along the x axis if both the zombie and the player
                                                    // are on the same x coordinate.
                                                    if ((foundPlayer.pos[0] / 32) - x != 0) {
                                                        skip = false;
                                                        // Determine which direction on the x axis to go to based on where the player is,
                                                        // and obtain the block at that position.
                                                        newNum = level.PosToInt((ushort)(x + Math.Sign((foundPlayer.pos[0] / 32) - x)), y, z);

                                                        // If the adjacent block and the block below it are both air, the
                                                        // zombie may be able to take a step down.
                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        // If the adjacent block and the block above it is air, just move
                                                        // to the adjacent block.
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        // If the adjacent block is solid, check if the two blocks above it are
                                                        // both air. If so, the zombie may be able to take a step up.
                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        // Otherwise, the zombie cannot move on this axis.
                                                        else {
                                                            skip = true;
                                                        }

                                                        // If the zombie can move, attempt to move the zombie's body to the new
                                                        // position.
                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                // If successful, place the head on the top of the body's new
                                                                // position. Then go to the part of the code where the zombie
                                                                // blocks are removed from their former positions.
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }
                                                    }

                                                    // Record that the block has attempted to follow the player on an axis.
                                                    foundNum++;
                                                    // If the zombie has tried to follow the player on both the x and z axes
                                                    // but could not, resort to random movement.
                                                    if (foundNum >= 2) {
                                                        currentNum = 7; // goto default
                                                    }
                                                    // Otherwise, attempt to follow the player on the z axis.
                                                    else {
                                                        currentNum = 4; // goto case 4
                                                    }
                                                }
                                                // Attempt to move along the z axis in a similar manner.
                                                if (currentNum >= 4 && currentNum <= 6) { // case 4, case 5, case 6
                                                    if ((foundPlayer.pos[2] / 32) - z != 0) {
                                                        skip = false;
                                                        newNum = level.PosToInt(x, y, (ushort)(z + Math.Sign((foundPlayer.pos[2] / 32) - z)));

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }
                                                    }

                                                    foundNum++;
                                                    if (foundNum >= 2) {
                                                        currentNum = 7; // goto default
                                                    }
                                                    else {
                                                        currentNum = 1; // goto case 1
                                                    }
                                                }
                                                // This if block is executed if the zombie could not move on both axes.
                                                if (currentNum == 7) { // default
                                                    // Set this variable to null so that the condition within the if statement
                                                    // that will be jumped to is false. Since that if statement is false, the
                                                    // else block is executed instead, and that block simply picks a random
                                                    // direction for the zombie to go to.
                                                    foundPlayer = null;
                                                    skip = true;
                                                    continue; // goto randomMovement_zomb
                                                }
                                            }
                                            else {
                                                inElseZombie = true;
                                                // If AI movement was not just skipped, then there is a delay between
                                                // a zombie's random movement. Increment the Check object's time and do
                                                // not do anything else.
                                                if (!skip) {
                                                    if (C.time < 3) {
                                                        C.time++;
                                                        addedTime = true;
                                                        break; // Only breaks out of the while loop, not the outer switch
                                                    }
                                                }

                                                foundNum = 0;
                                                // Determine the direction that a zombie will travel towards by
                                                // picking a random number between 1 and 12.
                                                switch (rand.Next(1, 13)) {
                                                    // This case is for going left on the x axis. Only this case is fully
                                                    // commented because the other cases are basically the same.
                                                    case 1:
                                                    case 2:
                                                    case 3:
                                                        skip = false;
                                                        // Store the index of the adjacent block.
                                                        newNum = level.IntOffset(C.b, -1, 0, 0);

                                                        // If the adjacent block and the block below it are both air, the
                                                        // zombie may be able to take a step down.
                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        // If the adjacent block and the block above it is air, just move
                                                        // to the adjacent block.
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }
                                                        // If the adjacent block is solid, check if the two blocks above it are
                                                        // both air. If so, the zombie may be able to take a step up.
                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        // Otherwise, the zombie cannot move in this direction.
                                                        else {
                                                            skip = true;
                                                        }

                                                        // If the zombie can move, attempt to move the zombie's body to the new
                                                        // position.
                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                // If successful, place the head on the top of the body's new
                                                                // position. Then go to the part of the code where the zombie
                                                                // blocks are removed from their former positions.
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }

                                                        // Note that the zombie attempted to move in a direction.
                                                        foundNum++;
                                                        // If the zombie attempted to move four times, note that the zombie
                                                        // has not moved, and stop attempting to move it any more for this iteration.
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                        }
                                                        // Otherwise, attempt to move right on the x axis.
                                                        else {
                                                            goto case 4;
                                                        }
                                                        break;

                                                    // This case is for moving right on the x axis.
                                                    case 4:
                                                    case 5:
                                                    case 6:
                                                        skip = false;
                                                        newNum = level.IntOffset(C.b, 1, 0, 0);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                        }
                                                        else {
                                                            goto case 7;  // Up on the z axis.
                                                        }
                                                        break;

                                                    // This case is for moving up on the z axis.
                                                    case 7:
                                                    case 8:
                                                    case 9:
                                                        skip = false;
                                                        newNum = level.IntOffset(C.b, 0, 0, 1);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                        }
                                                        else {
                                                            goto case 10;  // Down on the z axis.
                                                        }
                                                        break;

                                                    // This case is for moving down on the z axis.
                                                    case 10:
                                                    case 11:
                                                    case 12:
                                                    default:
                                                        skip = false;
                                                        newNum = level.IntOffset(C.b, 0, 0, -1);

                                                        if (level.GetTile(level.IntOffset(newNum, 0, -1, 0)) == BlockId.Air &&
                                                            level.GetTile(newNum) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, -1, 0);
                                                        }
                                                        else if (level.GetTile(newNum) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) { }

                                                        else if (level.GetTile(level.IntOffset(newNum, 0, 2, 0)) == BlockId.Air &&
                                                                 level.GetTile(level.IntOffset(newNum, 0, 1, 0)) == BlockId.Air) {
                                                            newNum = level.IntOffset(newNum, 0, 1, 0);
                                                        }
                                                        else {
                                                            skip = true;
                                                        }

                                                        if (!skip) {
                                                            if (level.AddUpdate(newNum, level.blocks[C.b])) {
                                                                level.AddUpdate(level.IntOffset(newNum, 0, 1, 0), BlockId.ZombieHead);
                                                                break; // goto removeSelf_zomb
                                                            }
                                                        }

                                                        foundNum++;
                                                        if (foundNum >= 4) {
                                                            InnerChange = true;
                                                        }
                                                        else {
                                                            goto case 1;
                                                        }
                                                        break;
                                                    // end cases
                                                }
                                            }
                                        }

                                        // If the delay before a random move takes place is still in effect, do
                                        // not do anything else.
                                        if (addedTime) {
                                            break; // Breaks out of the outer switch
                                        }

                                        // removeSelf_zomb
                                        // If the zombie moved, change the blocks at its former position to air.
                                        if (!InnerChange) {
                                            level.AddUpdate(C.b, BlockId.Air);
                                            level.AddUpdate(level.IntOffset(C.b, 0, 1, 0), BlockId.Air);
                                        }
                                        break;

                                        #endregion

                                    case BlockId.C4:
                                        // Find the C4 circuit that has the same number of the circuit that the
                                        // player is currently placing, and add the newly placed C4 block to the list
                                        // if the circuit was found.
                                        C4.C4s c4 = C4.Find(level, C.p.c4circuitNumber);
                                        if (c4 != null) {
                                            C4.C4s.OneC4 one = new C4.C4s.OneC4(x, y, z);
                                            c4.list.Add(one);
                                        }
                                        // The C4 block does not need to be checked after that.
                                        C.time = 255;
                                        break;

                                    case BlockId.C4Detonator:
                                        // Find the C4 circuit that has the same number of the circuit that the
                                        // player has just placed the detonator for, and record the detonator's
                                        // coordinates if the circuit was found.
                                        C4.C4s c = C4.Find(level, C.p.c4circuitNumber);
                                        if (c != null) {
                                            c.detenator[0] = x;
                                            c.detenator[1] = y;
                                            c.detenator[2] = z;
                                        }
                                        // Since the player is no longer placing C4 anymore, change their id back
                                        // to an invalid one.
                                        C.p.c4circuitNumber = -1;
                                        // The detonator block does not need to be checked anymore after that.
                                        C.time = 255;
                                        break;

                                    default:
                                        //non special blocks are then ignored, maybe it would be better to avoid getting here and cutting down the list
                                        if (!C.extraInfo.Contains("wait")) {
                                            C.time = 255;
                                        }
                                        break;
                                }
                            }
                        }
                        catch (Exception e) {
                            level.Logger.ErrorLog(e);
                            level.ListCheck.Remove(C);
                        }
                    }

                    // All Check objects that have expired are deleted.
                    level.ListCheck.RemoveAll(Check => Check.time == 255); //Remove all that are finished with 255 time

                    // Record the number of physics updates that are about to be applied.
                    level.lastUpdate = level.ListUpdate.Count;

                    // Go through each Update object and change the blocks that need to be changed.
                    level.ListUpdate.ForEach(delegate(Update C) {
                        try {
                            level.Blockchange(C.b, C.type, false, C.extraInfo);
                        }
                        catch (Exception e) {
                            level.Logger.ErrorLog(e);
                            level.Logger.Log("Phys update issue");
                        }
                    });

                    level.ListUpdate.Clear();

                    #endregion
                }
            }
            catch (Exception e) {
                level.Logger.Log("Level level.physics error");
                level.Logger.ErrorLog(e);
            }
        }

        public void ClearPhysics(Level level) {
            ushort x, y, z;
            level.ListCheck.ForEach(delegate(Check C) {
                level.IntToPos(C.b, out x, out y, out z);
                //attemps on shutdown to change blocks back into normal selves that are active, hopefully without needing to send into to clients.
                switch (level.blocks[C.b]) {
                    case BlockId.AirFlood:
                    case BlockId.AirFloodLayer:
                    case BlockId.AirFloodDown:
                        level.blocks[C.b] = BlockId.Air;
                        break;
                    case BlockId.DoorTreeWoodActive:
                        level.Blockchange(x, y, z, BlockId.DoorTreeWood);
                        break;
                    case BlockId.DoorObsidianActive:
                        level.Blockchange(x, y, z, BlockId.DoorObsidian);
                        break;
                    case BlockId.DoorGlassActive:
                        level.Blockchange(x, y, z, BlockId.DoorGlass);
                        break;
                    case BlockId.DoorStoneActive:
                        level.Blockchange(x, y, z, BlockId.DoorStone);
                        break;
                }

                try {
                    if (C.extraInfo.Contains("revert")) {
                        int i = 0;
                        foreach (string s in C.extraInfo.Split(' ')) {
                            if (s == "revert") {
                                level.Blockchange(x, y, z, (BlockId)Byte.Parse(C.extraInfo.Split(' ')[i + 1]), true);
                                break;
                            }
                            i++;
                        }
                    }
                }
                catch (Exception e) {
                    level.Logger.ErrorLog(e);
                }
            });

            level.ListCheck.Clear();
            level.ListUpdate.Clear();
        }

        /// <summary>
        /// Given a block that is next to active water, determine the block's new type.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <param name="type"> The id of a type that acts like active water. <seealso cref="BlockId"/></param>
        private void PhysWater(Level level, int b, BlockId type) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return;
            }

            // Obtain the coordinates of the block being checked.
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);

            switch (level.blocks[b]) {
                // Turn an air block into water.
                case BlockId.Air:
                    // Do not turn air into water if a sponge is close enough to the air block.
                    if (!PhysSpongeCheck(level, b)) {
                        level.AddUpdate(b, type);
                    }
                    break;

                // Turn adjacent lava into stone.
                case BlockId.ActiveLava: //hit active_lava
                case BlockId.FastLava: //hit lava_fast
                case BlockId.ActiveHotLava:
                    // Do not turn the block into stone if a sponge is close enough to it.
                    if (!PhysSpongeCheck(level, b)) {
                        level.AddUpdate(b, BlockId.Stone);
                    }
                    break;

                // Destroy adjacent plants and mushrooms.
                case BlockId.Sapling:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                    // This only occurs if physics is Advanced or higher.
                    if (level.physics > 1) {
                        if (level.physics != 5) {
                            // Do not destroy the block if a sponge is close enough to it.
                            if (!PhysSpongeCheck(level, b)) {
                                level.AddUpdate(b, BlockId.Air);
                            }
                        }
                    }
                    break;

                // Make sure that sand, gravel, and wood_float next to the water (usually above it) gets
                // updated as well.
                case BlockId.Sand: //sand
                case BlockId.Gravel: //gravel
                case BlockId.WoodFloat: //woodfloat
                    level.AddCheck(b);
                    break;

                // All other blocks are not affected.
                default:
                    break;
            }
        }

        /// <summary>
        /// Given a block that is next to active water, determine whether that block interacts with active
        /// water in some way.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <returns> Whether that block can interact with active water. </returns>
        private bool PhysWaterCheck(Level level, int b) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return false;
            }

            // Obtain the coordinates of the block being checked.
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);

            switch (level.blocks[b]) {
                // Air blocks can turn into water if a sponge is not close enough to the air block.
                case BlockId.Air:
                    // Make sure that a sponge is not too close to the air block.
                    if (!PhysSpongeCheck(level, b)) {
                        return true;
                    }
                    break;

                // Nearby lava can be turned into stone.
                case BlockId.ActiveLava: //hit active_lava
                case BlockId.FastLava: //hit lava_fast
                case BlockId.ActiveHotLava:
                    // Make sure that a sponge is not too close to the lava block.
                    if (!PhysSpongeCheck(level, b)) {
                        return true;
                    }
                    break;

                // Adjacent plants and mushrooms can get destroyed.
                case BlockId.Sapling:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                    // This only occurs if physics is Advanced or higher.
                    if (level.physics > 1) {
                        if (level.physics != 5) {
                            // Make sure that a sponge is not too close to the plant or mushroom.
                            if (!PhysSpongeCheck(level, b)) {
                                return true;
                            }
                        }
                    }
                    break;

                // Sand, gravel, and wood_float can interact with water.
                case BlockId.Sand: //sand
                case BlockId.Gravel: //gravel
                case BlockId.WoodFloat: //woodfloat
                    return true;
            }
            // All other blocks cannot interact with water, so they effectively block water.
            return false;
        }

        /// <summary>
        /// Given a block that is next to active lava, determine the block's new type.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <param name="type"> The id of a type that acts like active lava. <seealso cref="BlockId"/></param>
        private void PhysLava(Level level, int b, BlockId type) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return;
            }

            // Obtain the coordinates of the block being checked.
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);

            // Destroy cloth if advanced physics are on and a lava sponge is not too close to the cloth block.
            if (level.physics > 1 && level.physics != 5 && !PhysSpongeCheck(level, b, true) &&
                Convert.ToInt32(level.blocks[b]) >= 21 && Convert.ToInt32(level.blocks[b]) <= 36) {
                level.AddUpdate(b, 0);
                return;
            }

            switch (level.blocks[b]) {
                // Turn an air block into lava.
                case BlockId.Air:
                    // Do not turn air into lava if a lava sponge is close enough to the air block.
                    if (!PhysSpongeCheck(level, b, true)) {
                        level.AddUpdate(b, type);
                    }
                    break;

                // Turn adjacent water into stone.
                case BlockId.ActiveWater:
                case BlockId.ActiveColdWater:
                    // Do not turn water into stone if a lava sponge is close enough to the water block.
                    if (!PhysSpongeCheck(level, b, true)) {
                        level.AddUpdate(b, BlockId.Stone);
                    }
                    break;

                // Interact with nearby sand.
                case BlockId.Sand:
                    // If physics is Advanced or higher, turn the sand into glass.
                    if (level.physics > 1) {
                        if (level.physics != 5) {
                            level.AddUpdate(b, BlockId.Glass);
                        }
                    }
                    // Otherwise, make sure that the sand block's location gets updated.
                    else {
                        level.AddCheck(b);
                    }
                    break;

                // Gravel can fall into lava, so make sure that any nearby gravel has had its physics activated.
                case BlockId.Gravel:
                    level.AddCheck(b);
                    break;

                // Destroy adjacent flowers, mushrooms, tree wood, and leaves.
                case BlockId.WoodPlanks:
                case BlockId.Sapling:
                case BlockId.TreeWood:
                case BlockId.Leaves:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                    // This only occurs if physics is Advanced or higher.
                    if (level.physics > 1 && level.physics != 5)
                        // Make sure that a lava sponge is not too close to the given block.
                        if (!PhysSpongeCheck(level, b, true)) {
                            level.AddUpdate(b, BlockId.Air);
                        }
                    break;
                // All other blocks are not affected.
                default:
                    break;
            }
        }

        /// <summary>
        /// Given a block that is next to active lava, determine whether that block interacts with active
        /// lava in some way.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <returns> Whether that block can interact with active lava. </returns>
        private bool PhysLavaCheck(Level level, int b) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return false;
            }

            // Obtain the coordinates of the block being checked.
            ushort x, y, z;
            level.IntToPos(b, out x, out y, out z);

            // Cloth can be destroyed if advanced physics is on and if there are no lava sponges too close to
            // the cloth block.
            if (level.physics > 1 && level.physics != 5 && !PhysSpongeCheck(level, b, true) &&
                Convert.ToInt32(level.blocks[b]) >= 21 && Convert.ToInt32(level.blocks[b]) <= 36) {
                return true;
            }
            switch (level.blocks[b]) {
                // Air blocks turn into lava.
                case BlockId.Air:
                    // TODO: lava sponge check missing?
                    return true;

                // Nearby water can be turned into stone.
                case BlockId.ActiveWater:
                case BlockId.ActiveColdWater:
                    // Make sure that a lava sponge is not too close to the water block.
                    if (!PhysSpongeCheck(level, b, true)) {
                        return true;
                    }
                    break;

                // Sand can fall into lava or turn into glass.
                case BlockId.Sand:
                    if (level.physics > 1) {
                        if (level.physics != 5) {
                            return true;
                        }
                    }
                    else {
                        return true;
                    }
                    break;

                // Gravel can fall into lava.
                case BlockId.Gravel:
                    return true;

                // Lava can destroy adjacent wood, leaves, plants, and mushrooms.
                case BlockId.WoodPlanks:
                case BlockId.Sapling:
                case BlockId.TreeWood:
                case BlockId.Leaves:
                case BlockId.YellowFlower:
                case BlockId.RedFlower:
                case BlockId.BrownMushroom:
                case BlockId.RedMushroom:
                    // This only occurs if physics is set to Advanced or higher.
                    if (level.physics > 1 && level.physics != 5)
                        // Make sure that a lava sponge is not too close to the given block.
                        if (!PhysSpongeCheck(level, b, true)) {
                            return true;
                        }
                    break;
            }
            // All other blocks cannot interact with lava, so they effectively block lava.
            return false;
        }

        /// <summary>
        /// Reactivates the physics behavior of any blocks next to a destroyed block or blocks that turn into
        /// air under certain circumstances.
        /// </summary>
        /// <remarks>
        /// This method is called on adjacent blocks when a block is deleted, when sand or gravel is placed,
        /// and when anything that can get destroyed by active liquids is placed. One purpose of this method
        /// is to activate adjacent blocks that may have been deactivated because of a physics shutdown or
        /// because physics was turned off. Since examining some of these blocks also causes PhysAir() to be
        /// called, one call to PhysAir() can become many, causing entire ranges of blocks to have their
        /// physics reactivated. Another purpose of this method is specifically to fill in newly created
        /// pockets of air that are surrounded with active liquids that have been deactivated since they
        /// previously had no other blocks to spread to.
        ///
        /// The blocks that are added to the list of blocks to be checked are looked at during the same
        /// physics iteration. Prior to .NET 4.5, this was allowed to be done within a ForEach() call, but not
        /// anymore.
        /// </remarks>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        private void Physair(Level level, int b) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return;
            }

            // Reactivate blocks that are derived from the water or lava blocks. Also check the physics of cloth
            // blocks because lava blocks, which destroy cloth, might be next to them.
            if (BlockData.Convert(level.blocks[b]) == BlockId.ActiveWater || BlockData.Convert(level.blocks[b]) == BlockId.ActiveLava ||
                (Convert.ToInt32(level.blocks[b]) >= 21 && Convert.ToInt32(level.blocks[b]) <= 36)) {
                level.AddCheck(b);
                return;
            }

            // Reactivate blocks that also call PhysAir() when they are being checked. This has the effect of
            // reactivating as many blocks as possible. Leaves and saplings are included since tree growth and
            // leaf decay gets interrupted when physics is turned off.
            switch (level.blocks[b]) {
                case BlockId.Sapling: //shrub
                case BlockId.Sand: //sand
                case BlockId.Gravel: //gravel
                case BlockId.Leaves: //leaf
                case BlockId.WoodFloat: //wood_float
                    level.AddCheck(b);
                break;

            default:
                break;
            }
        }

        /// <summary>
        /// Given a sand or gravel block, determine whether it can fall and move the block if it will.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a sand or gravel block within the block array. </param>
        /// <param name="type"> The id of the given block's type. <seealso cref="BlockId"/></param>
        /// <returns> Whether the sand or gravel block fell down. </returns>
        private bool PhysSand(Level level, int b, BlockId type) { //also does gravel
            // Do simplistic bounds checking and check if physics is on.
            if (b == -1 || level.physics == 0) {
                return false;
            }
            if (level.physics == 5) {
                return false;
            }

            int tempb = b;
            bool blocked = false;
            bool moved = false;

            do {
                // Check the block below the sand or gravel block, and then keep checking the blocks below that
                // block until the falling block cannot go downward anymore.
                tempb = level.IntOffset(tempb, 0, -1, 0);

                if (level.GetTile(tempb) != BlockId.Null) {
                    switch (level.blocks[tempb]) {
                        // If the block below is air, water, or lava, the sand or gravel block can move there.
                        case BlockId.Air:
                        case BlockId.ActiveWater:
                        case BlockId.ActiveLava:
                            moved = true;
                            break;

                        // Sand and gravel blocks can potentially crush plants and mushrooms.
                        case BlockId.Sapling:
                        case BlockId.YellowFlower:
                        case BlockId.RedFlower:
                        case BlockId.BrownMushroom:
                        case BlockId.RedMushroom:
                            // If physics is set to Advanced or higher, the falling block crushes the plant
                            // or mushroom.
                            if (level.physics > 1 && level.physics != 5) {
                                moved = true;
                            }
                            // Otherwise, the falling block gets stopped by the plant or mushroom.
                            else {
                                blocked = true;
                            }
                            break;

                        // All other blocks are solid and block the falling block's path.
                        default:
                            blocked = true;
                            break;
                    }
                    // If physics is set to Advanced or higher, the falling block is marked as blocked because
                    // sand and gravel falls one block per iteration. Breaking out of the loop now allows the block
                    // to fall by exactly one block. Otherwise, physics is set to Normal, and on that setting, sand
                    // and gravel falls as far as possible within the same iteration.
                    if (level.physics > 1) {
                        if (level.physics != 5) {
                            blocked = true;
                        }
                    }
                }
                // GetTile() can return BlockId.Zero if the y coordinate ends up negative, which means that
                // bedrock has been reached. Thus, the falling block cannot fall anymore.
                else {
                    blocked = true;
                }
            }
            while (!blocked);

            // Change the block types of the involved blocks if the falling block moved.
            if (moved) {
                // Change the former sand or gravel block into air.
                level.AddUpdate(b, BlockId.Air);

                // If physics was set to Advanced or higher, the block that was just checked is the block that
                // the sand or gravel block will fall to, so replace that block with sand or gravel.
                if (level.physics > 1) {
                    level.AddUpdate(tempb, type);
                }
                // Otherwise, the block that was last checked is the block that is preventing the sand or
                // gravel from falling further, so the falling block should be one block above the solid block.
                else {
                    level.AddUpdate(level.IntOffset(tempb, 0, 1, 0), type);
                }
            }

            // And we are done here.
            return moved;
        }

        /// <summary>
        /// Reactivates the physics of sand and gravel blocks.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        private void PhysSandCheck(Level level, int b) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return;
            }

            // Check if the block is sand, gravel, or wood_float, and if so, reactivate the physics of that block.
            switch (level.blocks[b]) {
                case BlockId.Sand: //sand
                case BlockId.Gravel: //gravel
                case BlockId.WoodFloat: //wood_float
                    level.AddCheck(b);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Given a single stair block, check if it another single stair block is below it, and if so, combine
        /// the two blocks into a double stair block.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a stair block within the block array. </param>
        private void PhysStair(Level level, int b) {
            // Get the block below the current block.
            int tempb = level.IntOffset(b, 0, -1, 0);
            if (level.GetTile(tempb) != BlockId.Null) {
                // If the block below it is a single stair block, change the block below to a double stair block
                // and change the current block to air.
                if (level.GetTile(tempb) == BlockId.Slab) {
                    level.AddUpdate(b, BlockId.Air);
                    level.AddUpdate(tempb, BlockId.DoubleSlab);
                }
            }
        }

        /// <summary>
        /// Checks if a block is close enough to a sponge block. Called for active liquids.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <param name="lava"> Whether the active liquid is derived from the lava block. Defaults to false. </param>
        /// <returns> Whether the leaf should decay.  </returns>
        private bool PhysSpongeCheck(Level level, int b, bool lava = false) { //return true if sponge is near
            int temp = 0;
            // Sponges affect blocks that are 2 blocks or less away from them, so check for a sponge that is within
            // that distance.
            for (int x = -2; x <= +2; ++x) {
                for (int y = -2; y <= +2; ++y) {
                    for (int z = -2; z <= +2; ++z) {
                        temp = level.IntOffset(b, x, y, z);
                        if (level.GetTile(temp) != BlockId.Null) {
                            // If the liquid being checked is lava, check for a lava_sponge block, not a regular
                            // one. Otherwise, check for a regular sponge block.
                            if ((!lava && level.GetTile(temp) == BlockId.Sponge) || 
                                (lava && level.GetTile(temp) == BlockId.LavaSponge)) {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Turns the active liquid blocks around a sponge into air.
        /// </summary>
        /// <param name="level"> The level that the sponge block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a sponge block within the block array. </param>
        /// <param name="lava"> Whether the sponge block is a lava sponge. Defaults to false. </param>
        private void PhysSponge(Level level, int b, bool lava = false) { //turn near water into air when placed
            int temp = 0;
            // Sponges affect blocks that are 2 blocks or less away from them, so check for active liquids within
            // that distance.
            for (int x = -2; x <= +2; ++x) {
                for (int y = -2; y <= +2; ++y) {
                    for (int z = -2; z <= +2; ++z) {
                        temp = level.IntOffset(b, x, y, z);
                        if (level.GetTile(temp) != BlockId.Null) {
                            // If the sponge is a lava sponge, check for lava. Otherwise, check for water.
                            // Turn any matching blocks into air.
                            if ((!lava && BlockData.Convert(level.GetTile(temp)) == BlockId.ActiveWater) ||
                                    (lava && BlockData.Convert(level.GetTile(temp)) == BlockId.ActiveLava)) {
                                level.AddUpdate(temp, 0);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates the physics of active liquid blocks that are near a removed sponge block
        /// </summary>
        /// <param name="level"> The level that the sponge block was on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a removed sponge block within the block array. </param>
        /// <param name="lava"> Whether the removed sponge block is a lava sponge. Defaults to false. </param>
        public void PhysSpongeRemoved(Level level, int b, bool lava = false) {
            int temp = 0;
            // Sponges affect blocks that are 2 blocks or less away from them, so the nearest that an active
            // liquid can be before a sponge is removed is 3 blocks away.
            for (int x = -3; x <= +3; ++x) {
                for (int y = -3; y <= +3; ++y) {
                    for (int z = -3; z <= +3; ++z) {
                        // Only check blocks that were just outside a sponge's range.
                        if (Math.Abs(x) == 3 || Math.Abs(y) == 3 || Math.Abs(z) == 3) {
                            temp = level.IntOffset(b, x, y, z);
                            if (level.GetTile(temp) != BlockId.Null) {
                                // If the removed sponge was a lava sponge, reactivate the physics of lava blocks.
                                // Otherwise, reactivate the physics of water blocks.
                                if ((!lava && BlockData.Convert(level.GetTile(temp)) == BlockId.ActiveWater) ||
                                        (lava && BlockData.Convert(level.GetTile(temp)) == BlockId.ActiveLava)) {
                                    level.AddCheck(temp);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle physics for a wood_float block.
        /// </summary>
        /// <param name="level"> The level that the wood_float block was on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a wood_float block within the block array. </param>
        private void PhysFloatwood(Level level, int b) {
            // Fall down similarly to sand or gravel if air is below it. It stops one block above the water, though.
            int tempb = level.IntOffset(b, 0, -1, 0);
            if (level.GetTile(tempb) != BlockId.Null) {
                if (level.GetTile(tempb) == BlockId.Air) {
                    level.AddUpdate(b, BlockId.Air);  // The block at the former position becomes air.
                    level.AddUpdate(tempb, BlockId.WoodFloat);  // The block at the new position becomes wood_float.
                    return;
                }
            }

            // Float upwards if it is currently in active water. It stops at the surface.
            tempb = level.IntOffset(b, 0, 1, 0);
            if (level.GetTile(tempb) != BlockId.Null) {
                if (BlockData.Convert(level.GetTile(tempb)) == BlockId.ActiveWater) {
                    level.AddUpdate(b, BlockId.ActiveWater);  // The block at the former position becomes active water.
                    level.AddUpdate(tempb, BlockId.WoodFloat);  // The block at the new position becomes wood_float.
                    return;
                }
            }
        }

        /// <summary>
        /// Given an active liquid block, change it to an air_flood block.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of an active liquid block within the block array. </param>
        /// <param name="type"> The id of the new block type, which should be a type of air_flood.
        /// <seealso cref="BlockId"/></param>
        private void PhysairFlood(Level level, int b, BlockId type) {
            // Do simplistic bounds checking.
            if (b == -1) {
                return;
            }
            // If the given block is an active liquid block, change it to an air_flood block.
            if (BlockData.Convert(level.blocks[b]) == BlockId.ActiveWater || BlockData.Convert(level.blocks[b]) == BlockId.ActiveLava) {
                level.AddUpdate(b, type);
            }
        }

        /// <summary>
        /// Checks whether the leaves at the given block should decay.
        /// </summary>
        /// <param name="level"> The level that the block is on. <seealso cref="Level"/></param>
        /// <param name="b"> The index of a block within the block array. </param>
        /// <returns> Whether the leaf should decay.  </returns>
        private bool PhysLeaf(Level level, int b) {
            BlockId type;
            // All blocks that are 4 or less blocks away from the given block are checked.
            ushort dist = 4;
            int? i, xx, yy, zz;
            ushort x, y, z;

            // Obtain the coordinates of the given block.
            level.IntToPos(b, out x, out y, out z);

            // Mark each block that is 4 blocks or less away from the given block as tree wood, leaves, or
            // neither. 0 = tree wood, -2 = leaves, -1 = other.
            for (xx = -dist; xx <= dist; xx++) {
                for (yy = -dist; yy <= dist; yy++) {
                    for (zz = -dist; zz <= dist; zz++) {
                        type = level.GetTile((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                        if (type == BlockId.TreeWood) {
                            leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz))] = 0;
                        }
                        else if (type == BlockId.Leaves) {
                            leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz))] = -2;
                        }
                        else {
                            leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz))] = -1;
                        }
                    }
                }
            }

            // Figure out which leaves are 4 blocks away from a tree wood block. This algorithm, which is
            // inefficient, works like this:
            // - Change the value of any leaves adjacent to the tree wood blocks, which were given the value of 0,
            // to 1. (Since they do not have the value of -2 anymore, they will not decay.)
            // - Change the value of any leaves adjacent to the leaves with the value of 1 to 2 if they still have
            // the value of -2. Then, change the value of leaves adjacent to those with the value 2 to 3, and those
            // adjacent to those with the value 3 to 4.
            // - Essentially, it maps out the distance of blocks from the nearest tree wood block, or -2 if they are
            // not close enough to one.
            for (i = 1; i <= dist; i++) {
                for (xx = -dist; xx <= dist; xx++) {
                    for (yy = -dist; yy <= dist; yy++) {
                        for (zz = -dist; zz <= dist; zz++) {
                            try {
                                if (leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz))] == i - 1) {
                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx - 1), (ushort)(y + yy), (ushort)(z + zz))) &&
                                        leaves[level.PosToInt((ushort)(x + xx - 1), (ushort)(y + yy), (ushort)(z + zz))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx - 1), (ushort)(y + yy), (ushort)(z + zz))] = (sbyte)i;

                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx + 1), (ushort)(y + yy), (ushort)(z + zz))) &&
                                        leaves[level.PosToInt((ushort)(x + xx + 1), (ushort)(y + yy), (ushort)(z + zz))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx + 1), (ushort)(y + yy), (ushort)(z + zz))] = (sbyte)i;

                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx), (ushort)(y + yy - 1), (ushort)(z + zz))) &&
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy - 1), (ushort)(z + zz))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy - 1), (ushort)(z + zz))] = (sbyte)i;

                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx), (ushort)(y + yy + 1), (ushort)(z + zz))) &&
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy + 1), (ushort)(z + zz))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy + 1), (ushort)(z + zz))] = (sbyte)i;

                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz - 1))) &&
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz - 1))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz - 1))] = (sbyte)i;

                                    if (leaves.ContainsKey(level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz + 1))) &&
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz + 1))] == -2)
                                        leaves[level.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz + 1))] = (sbyte)i;
                                }
                            }
                            catch (Exception e) {
                                level.Logger.ErrorLog(e);
                                level.Logger.Log("Leaf decay error!");
                            }
                        }
                    }
                }
            }

            // If the given block was not marked, it should decay.
            return leaves[b] < 0;
        }
    }

    /// <summary>
    /// The C4 class provides methods for managing C4 blocks.
    /// </summary>
    public static class C4 {
        /// <summary>
        /// Destroys all C4 blocks associated with a particular detonator block.
        /// </summary>
        /// <param name="detenator"> The coordinates of the detonator as an array. </param>
        /// <param name="lvl"> The level that the detonator is on. <seealso cref="Level"/></param>
        public static void BlowUp(ushort[] detenator, Level lvl) {
            try {
                List<C4s> toDelete = new List<C4s>(); 

                foreach (C4s c4 in lvl.C4list) {
                    // Find the C4 circuit that the detonator block is linked to.
                    if (c4.detenator[0] == detenator[0] && c4.detenator[1] == detenator[1] && c4.detenator[2] == detenator[2]) {
                        // Cause an explosion for every C4 block associated with the detonator.
                        foreach (C4s.OneC4 c in c4.list) {
                            lvl.MakeExplosion(c.pos[0], c.pos[1], c.pos[2], 0);
                        }

                        toDelete.Add(c4);
                    }
                }

                foreach (C4s c4 in toDelete) {
                    lvl.C4list.Remove(c4);
                }
            }
            catch (Exception e) {
                lvl.Logger.ErrorLog(e);
            }
        }
        /// <summary>
        /// Obtains an unused C4 circuit number for the given level.
        /// </summary>
        /// <param name="lvl"> The level that the player is on. <seealso cref="Level"/></param>
        /// <returns> An unused circuit number. </returns>
        public static sbyte NextCircuit(Level lvl) {
            // The number returned is based on the number of C4 circuits already in the level.
            // FIXME: Making two C4 circuits, then detonating the first one, then creating another C4 circuit
            // causes that and the other circuit to have the same number.
            sbyte number = 1;
            foreach (C4s c4 in lvl.C4list) {
                number++;
            }
            return number;
        }

        /// <summary>
        /// Finds the C4 circuit on the given level with the given circuit number.
        /// </summary>
        /// <param name="lvl"> The level that the player is on. <seealso cref="Level"/></param>
        /// <param name="CircuitNumber"> The circuit number. </param>
        /// <returns> The C4s object with the given circuit number, or null if none is found.
        /// <seealso cref="C4.C4s"/></returns>
        public static C4s Find(Level lvl, sbyte CircuitNumber) {
            foreach (C4s c4 in lvl.C4list) {
                if (c4.CircuitNumb == CircuitNumber) {
                    return c4;
                }
            }
            return null;
        }

        /// <summary>
        /// The C4s subclass represents a single C4 circuit.
        /// </summary>
        public class C4s {
            /// <summary>
            /// The number of the circuit.
            /// </summary>
            public sbyte CircuitNumb;
            /// <summary>
            /// The coordinates of the detonator as an array.
            /// </summary>
            public ushort[] detenator;
            /// <summary>
            /// The list of C4 blocks associated with the circuit.
            /// </summary>
            public List<OneC4> list;

            /// <summary>
            /// The OneC4 class represents an individual C4 block.
            /// </summary>
            public class OneC4 {
                /// <summary>
                /// The coordinates of the C4 block as an array.
                /// </summary>
                public ushort[] pos = new ushort[3];
                /// <summary>
                /// Constructs a OneC4 object.
                /// </summary>
                /// <param name="x"> The x coordinate of the C4 block. </param>
                /// <param name="y"> The y coordinate of the C4 block. </param>
                /// <param name="z"> The z coordinate of the C4 block. </param>
                public OneC4(ushort x, ushort y, ushort z) {
                    pos[0] = x;
                    pos[1] = y;
                    pos[2] = z;
                }
            }

            /// <summary>
            /// Constructs a C4s object.
            /// </summary>
            /// <param name="num"> The circuit number to use. </param>
            public C4s(sbyte num) {
                CircuitNumb = num;
                list = new List<OneC4>();
                detenator = new ushort[3];
            }
        }
    }
}

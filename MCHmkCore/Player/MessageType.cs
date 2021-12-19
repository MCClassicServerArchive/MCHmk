using System;

namespace MCHmk {
    /// <summary>
    /// The MessageType enumeration describes the various types of messages that can be
    /// sent to CPE clients.
    /// </summary>
    public enum MessageType {
        /// <summary>
        /// Identifies a regular chat message. This is the only type of message that non-CPE
        /// clients can receive.
        /// </summary>
        Chat = (byte)0,
        /// <summary>
        /// Identifies a status message, which appears on the top-right corner persistently.
        /// This is the first line on the top-right corner.
        /// </summary>
        Status1 = (byte)1,
        /// <summary>
        /// Identifies a status message, which appears on the top-right corner persistently.
        /// This is the second line on the top-right corner.
        /// </summary>
        Status2 = (byte)2,
        /// <summary>
        /// Identifies a status message, which appears on the top-right corner persistently.
        /// This is the third line on the top-right corner.
        /// </summary>
        Status3 = (byte)3,
        /// <summary>
        /// Identifies a message that appears on the bottom-right corner. Appears on the bottom line.
        /// </summary>
        BottomRight1 = (byte)11,
        /// <summary>
        /// Identifies a message that appears on the bottom-right corner. Appears on the middle line.
        /// </summary>
        BottomRight2 = (byte)12,
        /// <summary>
        /// Identifies a message that appears on the bottom-right corner. Appears on the top line.
        /// </summary>
        BottomRight3 = (byte)13,
        /// <summary>
        /// Identifies an announcement, which appears on the center of the screen for a few seconds.
        /// </summary>
        Announcement = (byte)100
    }
}

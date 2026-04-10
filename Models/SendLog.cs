using System;

namespace Jellyfin.Plugin.Kindle.Models
{
    /// <summary>
    /// Logs information about book sends to e-readers
    /// </summary>
    public class SendLog
    {
        /// <summary>
        /// Unique identifier (GUID)
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Jellyfin User ID who sent the book
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Jellyfin Item ID of the book
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Name of the file sent
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// E-reader email address it was sent to
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Device ID (reference to UserDevice)
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Send status
        /// </summary>
        public SendStatus Status { get; set; } = SendStatus.Success;

        /// <summary>
        /// Timestamp when the send was attempted
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Error message if send failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Book title for display
        /// </summary>
        public string BookTitle { get; set; }

        /// <summary>
        /// File format sent (e.g., epub, pdf, mobi)
        /// </summary>
        public string Format { get; set; }
    }

    /// <summary>
    /// Status of a send operation
    /// </summary>
    public enum SendStatus
    {
        /// <summary>
        /// Successfully sent
        /// </summary>
        Success = 0,

        /// <summary>
        /// Send failed with error
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Send is pending/in progress
        /// </summary>
        Pending = 2
    }
}

using System;
using System.Text.Json.Serialization;

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
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Jellyfin User ID who sent the book
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Jellyfin Item ID of the book
        /// </summary>
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; }

        /// <summary>
        /// Name of the file sent
        /// </summary>
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        [JsonPropertyName("fileSizeBytes")]
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// E-reader email address it was sent to
        /// </summary>
        [JsonPropertyName("recipientEmail")]
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Device ID (reference to UserDevice)
        /// </summary>
        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// Send status
        /// </summary>
        [JsonPropertyName("status")]
        public SendStatus Status { get; set; } = SendStatus.Success;

        /// <summary>
        /// Timestamp when the send was attempted
        /// </summary>
        [JsonPropertyName("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Error message if send failed
        /// </summary>
        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Book title for display
        /// </summary>
        [JsonPropertyName("bookTitle")]
        public string BookTitle { get; set; }

        /// <summary>
        /// File format sent (e.g., epub, pdf, mobi)
        /// </summary>
        [JsonPropertyName("format")]
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

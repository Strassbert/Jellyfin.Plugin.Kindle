using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Kindle.Models
{
    /// <summary>
    /// Represents an e-reader device for a Jellyfin user
    /// </summary>
    public class UserDevice
    {
        /// <summary>
        /// Unique identifier (GUID)
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Jellyfin User ID this device belongs to
        /// </summary>
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        /// <summary>
        /// Device name (e.g., "Kindle Paperwhite", "Kobo Clara")
        /// </summary>
        [JsonPropertyName("deviceName")]
        public string? DeviceName { get; set; }

        /// <summary>
        /// E-reader email address (e.g., user@kindle.com)
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Preferred file format for this device
        /// (e.g., "epub", "pdf", "mobi")
        /// </summary>
        [JsonPropertyName("preferredFormat")]
        public string PreferredFormat { get; set; } = "epub";

        /// <summary>
        /// Whether this is the primary/default device
        /// </summary>
        [JsonPropertyName("isDefault")]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Device creation timestamp
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this device is active
        /// </summary>
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; } = true;
    }
}

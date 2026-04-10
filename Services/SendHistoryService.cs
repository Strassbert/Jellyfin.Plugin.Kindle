using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.Kindle.Configuration;
using Jellyfin.Plugin.Kindle.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Kindle.Services
{
    /// <summary>
    /// Service for managing send history and statistics
    /// </summary>
    public class SendHistoryService
    {
        private readonly PluginConfiguration _config;
        private readonly ILogger<SendHistoryService> _logger;

        public SendHistoryService(
            PluginConfiguration config,
            ILogger<SendHistoryService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Add a send log entry
        /// </summary>
        public void LogSend(SendLog log)
        {
            if (!_config.EnableSendHistory)
                return;

            var logs = _config.SendLogs;
            logs.Add(log);

            // Apply retention policy
            if (_config.MaxSendLogs > 0 && logs.Count > _config.MaxSendLogs)
            {
                // Remove oldest entries
                var toRemove = logs.Count - _config.MaxSendLogs;
                logs.RemoveRange(0, toRemove);
            }

            _config.SendLogs = logs;
            Plugin.Instance.SaveConfiguration();

            _logger.LogDebug("Send log recorded - ItemId: {ItemId}, UserId: {UserId}", log.ItemId, log.UserId);
        }

        /// <summary>
        /// Get send history for a user
        /// </summary>
        public List<SendLog> GetUserHistory(string userId, int limit = 50)
        {
            return _config.SendLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.SentAt)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Get send history for a specific device
        /// </summary>
        public List<SendLog> GetDeviceHistory(string deviceId, int limit = 50)
        {
            return _config.SendLogs
                .Where(l => l.DeviceId == deviceId)
                .OrderByDescending(l => l.SentAt)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// Get statistics for the entire system (Admin only)
        /// </summary>
        public SystemStatistics GetSystemStatistics()
        {
            var logs = _config.SendLogs;

            return new SystemStatistics
            {
                TotalSends = logs.Count,
                SuccessfulSends = logs.Count(l => l.Status == SendStatus.Success),
                FailedSends = logs.Count(l => l.Status == SendStatus.Failed),
                TotalFilesSize = logs.Sum(l => l.FileSizeBytes),
                UniqueUsers = logs.Select(l => l.UserId).Distinct().Count(),
                MostActiveUser = GetMostActiveUser(logs),
                MostCommonFormat = GetMostCommonFormat(logs),
                AverageDailyActivity = GetAverageDailyActivity(logs),
                SuccessRate = logs.Count > 0 ? (double)logs.Count(l => l.Status == SendStatus.Success) / logs.Count * 100 : 0
            };
        }

        /// <summary>
        /// Get statistics for a specific user
        /// </summary>
        public UserStatistics GetUserStatistics(string userId)
        {
            var userLogs = _config.SendLogs.Where(l => l.UserId == userId).ToList();

            var successCount = userLogs.Count(l => l.Status == SendStatus.Success);
            var failCount = userLogs.Count(l => l.Status == SendStatus.Failed);
            var lastSend = userLogs.OrderByDescending(l => l.SentAt).FirstOrDefault();

            return new UserStatistics
            {
                UserId = userId,
                TotalSends = userLogs.Count,
                SuccessfulSends = successCount,
                FailedSends = failCount,
                TotalFilesSize = userLogs.Sum(l => l.FileSizeBytes),
                LastSendAt = lastSend?.SentAt,
                LastSentTo = lastSend?.RecipientEmail,
                SuccessRate = userLogs.Count > 0 ? (double)successCount / userLogs.Count * 100 : 0,
                FavoriteFormat = userLogs.GroupBy(l => l.Format)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "unknown"
            };
        }

        /// <summary>
        /// Clear history for a user (Admin action)
        /// </summary>
        public void ClearUserHistory(string userId)
        {
            var logs = _config.SendLogs;
            var initialCount = logs.Count;
            logs.RemoveAll(l => l.UserId == userId);
            _config.SendLogs = logs;
            Plugin.Instance.SaveConfiguration();

            _logger.LogWarning(
                "User history cleared - UserId: {UserId}, RemovedLogs: {Count}, Timestamp: {Timestamp}",
                userId, initialCount - logs.Count, DateTime.UtcNow);
        }

        /// <summary>
        /// Clear all history (Admin action)
        /// </summary>
        public void ClearAllHistory()
        {
            var count = _config.SendLogs.Count;
            _config.SendLogs = new();
            Plugin.Instance.SaveConfiguration();

            _logger.LogWarning(
                "All history cleared - RemovedLogs: {Count}, Timestamp: {Timestamp}",
                count, DateTime.UtcNow);
        }

        private static string GetMostActiveUser(List<SendLog> logs)
        {
            return logs.GroupBy(l => l.UserId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A";
        }

        private static string GetMostCommonFormat(List<SendLog> logs)
        {
            return logs.GroupBy(l => l.Format ?? "unknown")
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "unknown";
        }

        private static double GetAverageDailyActivity(List<SendLog> logs)
        {
            if (logs.Count == 0) return 0;

            var daySpan = (logs.Max(l => l.SentAt) - logs.Min(l => l.SentAt)).TotalDays;
            return daySpan > 0 ? logs.Count / daySpan : 0;
        }
    }

    /// <summary>
    /// System-wide statistics
    /// </summary>
    public class SystemStatistics
    {
        [JsonPropertyName("totalSends")]
        public int TotalSends { get; set; }

        [JsonPropertyName("successfulSends")]
        public int SuccessfulSends { get; set; }

        [JsonPropertyName("failedSends")]
        public int FailedSends { get; set; }

        [JsonPropertyName("totalFilesSize")]
        public long TotalFilesSize { get; set; }

        [JsonPropertyName("uniqueUsers")]
        public int UniqueUsers { get; set; }

        [JsonPropertyName("mostActiveUser")]
        public string MostActiveUser { get; set; }

        [JsonPropertyName("mostCommonFormat")]
        public string MostCommonFormat { get; set; }

        [JsonPropertyName("averageDailyActivity")]
        public double AverageDailyActivity { get; set; }

        [JsonPropertyName("successRate")]
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// Per-user statistics
    /// </summary>
    public class UserStatistics
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("totalSends")]
        public int TotalSends { get; set; }

        [JsonPropertyName("successfulSends")]
        public int SuccessfulSends { get; set; }

        [JsonPropertyName("failedSends")]
        public int FailedSends { get; set; }

        [JsonPropertyName("totalFilesSize")]
        public long TotalFilesSize { get; set; }

        [JsonPropertyName("lastSendAt")]
        public DateTime? LastSendAt { get; set; }

        [JsonPropertyName("lastSentTo")]
        public string LastSentTo { get; set; }

        [JsonPropertyName("successRate")]
        public double SuccessRate { get; set; }

        [JsonPropertyName("favoriteFormat")]
        public string FavoriteFormat { get; set; }
    }
}

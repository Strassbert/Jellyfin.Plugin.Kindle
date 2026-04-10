using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.Kindle.Services
{
    /// <summary>
    /// Service for rate limiting API requests to prevent abuse
    /// </summary>
    public class RateLimitingService
    {
        private const int MaxAttemptsPerMinute = 5;
        private const int MaxAttemptsPerHour = 50;

        private readonly Dictionary<string, List<DateTime>> _userAttempts = new();
        private readonly object _lockObject = new();

        /// <summary>
        /// Check if a user is allowed to make a request based on rate limits
        /// </summary>
        /// <returns>True if allowed, false if rate limited</returns>
        public bool IsAllowed(string userId, string action = "send")
        {
            if (string.IsNullOrWhiteSpace(userId))
                return false;

            var key = $"{userId}:{action}";

            lock (_lockObject)
            {
                if (!_userAttempts.TryGetValue(key, out var attempts))
                {
                    _userAttempts[key] = new List<DateTime> { DateTime.UtcNow };
                    return true;
                }

                var now = DateTime.UtcNow;

                // Clean up old attempts (older than 1 hour)
                attempts.RemoveAll(d => now.Subtract(d).TotalHours >= 1);

                // Check per-minute limit (5 attempts per minute)
                var recentAttempts = attempts.Where(d => now.Subtract(d).TotalMinutes < 1).Count();
                if (recentAttempts >= MaxAttemptsPerMinute)
                {
                    return false;
                }

                // Check per-hour limit (50 attempts per hour)
                if (attempts.Count >= MaxAttemptsPerHour)
                {
                    return false;
                }

                attempts.Add(now);
                return true;
            }
        }

        /// <summary>
        /// Get remaining attempts for a user
        /// </summary>
        public int GetRemainingAttempts(string userId, string action = "send")
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            var key = $"{userId}:{action}";

            lock (_lockObject)
            {
                if (!_userAttempts.TryGetValue(key, out var attempts))
                {
                    return MaxAttemptsPerMinute;
                }

                var now = DateTime.UtcNow;
                var recentAttempts = attempts.Where(d => now.Subtract(d).TotalMinutes < 1).Count();
                return Math.Max(0, MaxAttemptsPerMinute - recentAttempts);
            }
        }

        /// <summary>
        /// Reset rate limit for a user (useful for testing)
        /// </summary>
        public void Reset(string userId)
        {
            lock (_lockObject)
            {
                var keysToRemove = _userAttempts.Keys
                    .Where(k => k.StartsWith($"{userId}:"))
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _userAttempts.Remove(key);
                }
            }
        }
    }
}

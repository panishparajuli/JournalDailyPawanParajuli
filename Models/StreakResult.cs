using System;

namespace JournalDaily.Models
{
    public class StreakResult
    {
        /// <summary>
        /// Number of consecutive days with entries from today backwards.
        /// </summary>
        public int CurrentStreak { get; set; }

        /// <summary>
        /// Longest consecutive streak of entries ever recorded.
        /// </summary>
        public int LongestStreak { get; set; }

        /// <summary>
        /// The start date of the longest streak.
        /// </summary>
        public DateTime? LongestStreakStartDate { get; set; }

        /// <summary>
        /// The end date of the longest streak.
        /// </summary>
        public DateTime? LongestStreakEndDate { get; set; }

        /// <summary>
        /// The start date of the current streak.
        /// </summary>
        public DateTime? CurrentStreakStartDate { get; set; }

        /// <summary>
        /// Total number of days with entries.
        /// </summary>
        public int TotalDaysWithEntries { get; set; }

        /// <summary>
        /// Number of unique days that should have had entries but didn't (gaps in the timeline).
        /// </summary>
        public int MissedDays { get; set; }

        /// <summary>
        /// List of dates that are part of missed days (gaps).
        /// </summary>
        public List<DateTime> MissedDaysList { get; set; } = new();

        /// <summary>
        /// The date of the most recent entry.
        /// </summary>
        public DateTime? MostRecentEntryDate { get; set; }

        /// <summary>
        /// The date of the oldest entry.
        /// </summary>
        public DateTime? OldestEntryDate { get; set; }

        /// <summary>
        /// Whether the streak is still active (has entry today or yesterday).
        /// </summary>
        public bool IsStreakActive { get; set; }

        /// <summary>
        /// Total days span from oldest to most recent entry.
        /// </summary>
        public int TotalDaysSpan { get; set; }

        /// <summary>
        /// Completion percentage (entries / total span * 100).
        /// </summary>
        public double CompletionPercentage { get; set; }

        /// <summary>
        /// Average streak length (total entries / number of streaks).
        /// </summary>
        public double AverageStreakLength { get; set; }

        /// <summary>
        /// Number of distinct streaks (chains).
        /// </summary>
        public int TotalStreaks { get; set; }

        /// <summary>
        /// Estimated days until streak is lost (if current streak is active).
        /// </summary>
        public int DaysUntilStreakLost { get; set; }

        /// <summary>
        /// Streak status message for UI display.
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;
    }
}

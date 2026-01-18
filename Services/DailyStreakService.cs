using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JournalDaily.Data;
using JournalDaily.Models;

namespace JournalDaily.Services
{
    public class DailyStreakService
    {
        private readonly AppDbContext _db;

        public DailyStreakService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Calculate all streak statistics based on journal entry dates.
        /// Assumes only one entry per day.
        /// </summary>
        public async Task<StreakResult> CalculateStreakAsync()
        {
            try
            {
                // Get all unique entry dates, ordered ascending
                var entryDates = await _db.JournalEntries
                    .Select(e => e.EntryDate.Date)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                if (entryDates.Count == 0)
                {
                    return new StreakResult
                    {
                        CurrentStreak = 0,
                        LongestStreak = 0,
                        TotalDaysWithEntries = 0,
                        MissedDays = 0,
                        IsStreakActive = false
                    };
                }

                var today = DateTime.Today;
                var result = new StreakResult
                {
                    TotalDaysWithEntries = entryDates.Count,
                    MostRecentEntryDate = entryDates.Last(),
                    OldestEntryDate = entryDates.First(),
                    TotalDaysSpan = (int)(entryDates.Last() - entryDates.First()).TotalDays + 1
                };

                // Calculate current streak
                var currentStreakInfo = CalculateCurrentStreak(entryDates, today);
                result.CurrentStreak = currentStreakInfo.Length;
                result.CurrentStreakStartDate = currentStreakInfo.StartDate;
                result.IsStreakActive = currentStreakInfo.IsActive;

                // Calculate longest streak
                var longestStreakInfo = CalculateLongestStreak(entryDates);
                result.LongestStreak = longestStreakInfo.Length;
                result.LongestStreakStartDate = longestStreakInfo.StartDate;
                result.LongestStreakEndDate = longestStreakInfo.EndDate;

                // Calculate missed days
                var missedDaysInfo = CalculateMissedDays(entryDates);
                result.MissedDays = missedDaysInfo.Count;
                result.MissedDaysList = missedDaysInfo;

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculating streak: {ex}");
                throw;
            }
        }

        /// <summary>
        /// Get only the current streak count and status.
        /// </summary>
        public async Task<(int Count, bool IsActive, DateTime? StartDate)> GetCurrentStreakAsync()
        {
            var entryDates = await _db.JournalEntries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            if (entryDates.Count == 0)
                return (0, false, null);

            var streak = CalculateCurrentStreak(entryDates, DateTime.Today);
            return (streak.Length, streak.IsActive, streak.StartDate);
        }

        /// <summary>
        /// Get only the longest streak information.
        /// </summary>
        public async Task<(int Count, DateTime? StartDate, DateTime? EndDate)> GetLongestStreakAsync()
        {
            var entryDates = await _db.JournalEntries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            if (entryDates.Count == 0)
                return (0, null, null);

            var streak = CalculateLongestStreak(entryDates);
            return (streak.Length, streak.StartDate, streak.EndDate);
        }

        /// <summary>
        /// Get all missed days between oldest and most recent entry.
        /// </summary>
        public async Task<List<DateTime>> GetMissedDaysAsync()
        {
            var entryDates = await _db.JournalEntries
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            if (entryDates.Count == 0)
                return new List<DateTime>();

            return CalculateMissedDays(entryDates);
        }

        /// <summary>
        /// Check if there's an entry for a specific date.
        /// </summary>
        public async Task<bool> HasEntryAsync(DateTime date)
        {
            var d = date.Date;
            return await _db.JournalEntries
                .AnyAsync(e => e.EntryDate == d);
        }

        /// <summary>
        /// Get entries for a date range.
        /// </summary>
        public async Task<List<DateTime>> GetEntryDatesInRangeAsync(DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date;

            return await _db.JournalEntries
                .Where(e => e.EntryDate >= fromDate && e.EntryDate <= toDate)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }

        /// <summary>
        /// Calculate the percentage of days with entries over a given period.
        /// </summary>
        public async Task<double> GetCompletionPercentageAsync(DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date;
            var totalDaysInRange = (int)(toDate - fromDate).TotalDays + 1;

            var entriesInRange = await _db.JournalEntries
                .Where(e => e.EntryDate >= fromDate && e.EntryDate <= toDate)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .CountAsync();

            if (totalDaysInRange == 0)
                return 0;

            return (entriesInRange / (double)totalDaysInRange) * 100;
        }

        // Private helper methods

        private (int Length, DateTime? StartDate, bool IsActive) CalculateCurrentStreak(List<DateTime> entryDates, DateTime today)
        {
            if (entryDates.Count == 0)
                return (0, null, false);

            int streakLength = 0;
            DateTime? streakStartDate = null;
            DateTime checkDate = today;

            // Count backwards from today
            for (int i = entryDates.Count - 1; i >= 0; i--)
            {
                if (entryDates[i] == checkDate)
                {
                    streakLength++;
                    streakStartDate = entryDates[i];
                    checkDate = checkDate.AddDays(-1);
                }
                else if (entryDates[i] < checkDate)
                {
                    // Gap found, streak is broken
                    break;
                }
            }

            // Determine if streak is active (has entry today or yesterday)
            bool isActive = entryDates.Any(d => d == today || d == today.AddDays(-1));

            return (streakLength, streakStartDate, isActive);
        }

        private (int Length, DateTime? StartDate, DateTime? EndDate) CalculateLongestStreak(List<DateTime> entryDates)
        {
            if (entryDates.Count == 0)
                return (0, null, null);

            if (entryDates.Count == 1)
                return (1, entryDates[0], entryDates[0]);

            int longestLength = 1;
            int currentLength = 1;
            DateTime? longestStart = entryDates[0];
            DateTime? longestEnd = entryDates[0];
            DateTime? currentStart = entryDates[0];

            for (int i = 1; i < entryDates.Count; i++)
            {
                // Check if consecutive (1 day apart)
                if ((entryDates[i] - entryDates[i - 1]).TotalDays == 1)
                {
                    currentLength++;
                }
                else
                {
                    // Streak broken
                    if (currentLength > longestLength)
                    {
                        longestLength = currentLength;
                        longestStart = currentStart;
                        longestEnd = entryDates[i - 1];
                    }

                    currentLength = 1;
                    currentStart = entryDates[i];
                }
            }

            // Check the last streak
            if (currentLength > longestLength)
            {
                longestLength = currentLength;
                longestStart = currentStart;
                longestEnd = entryDates[entryDates.Count - 1];
            }

            return (longestLength, longestStart, longestEnd);
        }

        private List<DateTime> CalculateMissedDays(List<DateTime> entryDates)
        {
            if (entryDates.Count <= 1)
                return new List<DateTime>();

            var missedDays = new List<DateTime>();
            DateTime firstDate = entryDates[0];
            DateTime lastDate = entryDates[entryDates.Count - 1];

            // Check each day in the range
            for (DateTime date = firstDate; date <= lastDate; date = date.AddDays(1))
            {
                if (!entryDates.Contains(date))
                {
                    missedDays.Add(date);
                }
            }

            return missedDays;
        }
    }
}

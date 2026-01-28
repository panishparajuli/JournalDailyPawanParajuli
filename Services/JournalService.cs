using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using JournalDaily.Data;
using JournalDaily.Models;

namespace JournalDaily.Services
{
    public class JournalService
    {
        private readonly AppDbContext _db;

        public JournalService(AppDbContext db)
        {
            _db = db;
        }

        private static DateTime DateOnly(DateTime dt) => dt.Date;

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            var d = DateOnly(date);
            return await _db.JournalEntries
                .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
                .Include(e => e.EntryMoods).ThenInclude(em => em.Mood)
                .FirstOrDefaultAsync(e => e.EntryDate == d);
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await _db.JournalEntries
                .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
                .Include(e => e.EntryMoods).ThenInclude(em => em.Mood)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public async Task<List<JournalEntry>> GetPaginatedEntriesAsync(int pageIndex, int pageSize)
        {
            return await _db.JournalEntries
                .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
                .Include(e => e.EntryMoods).ThenInclude(em => em.Mood)
                .OrderByDescending(e => e.EntryDate)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpsertEntryAsync(JournalEntry entry, IEnumerable<string>? tagNames = null, IEnumerable<string>? moodNames = null, string? primaryMood = null)
        {
            var d = DateOnly(entry.EntryDate);
            var existing = await _db.JournalEntries
                .Include(e => e.EntryTags)
                .Include(e => e.EntryMoods)
                .FirstOrDefaultAsync(e => e.EntryDate == d);

            if (existing == null)
            {
                entry.Id = Guid.NewGuid();
                entry.EntryDate = d;
                entry.CreatedAt = DateTime.UtcNow;
                entry.UpdatedAt = DateTime.UtcNow;
                _db.JournalEntries.Add(entry);

                existing = entry;
            }
            else
            {
                existing.Title = entry.Title;
                existing.Content = entry.Content;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // tags
            if (tagNames != null)
            {
                // remove existing tags
                _db.EntryTags.RemoveRange(existing.EntryTags);

                foreach (var tn in tagNames.Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == tn);
                    if (tag == null)
                    {
                        tag = new Tag { Id = Guid.NewGuid(), Name = tn, Prebuilt = false };
                        _db.Tags.Add(tag);
                    }
                    existing.EntryTags.Add(new EntryTag { Id = Guid.NewGuid(), JournalEntryId = existing.Id, TagId = tag.Id, Tag = tag });
                }
            }

            // moods
            if (moodNames != null || !string.IsNullOrWhiteSpace(primaryMood))
            {
                _db.EntryMoods.RemoveRange(existing.EntryMoods);

                var added = new List<string>();
                if (!string.IsNullOrWhiteSpace(primaryMood)) added.Add(primaryMood.Trim());
                if (moodNames != null) added.AddRange(moodNames.Select(m => m.Trim()));

                foreach (var mn in added.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var mood = await _db.Moods.FirstOrDefaultAsync(m => m.Name == mn);
                    if (mood == null)
                    {
                        mood = new Mood { Id = Guid.NewGuid(), Name = mn, Category = "Neutral" };
                        _db.Moods.Add(mood);
                    }
                    existing.EntryMoods.Add(new EntryMood { Id = Guid.NewGuid(), JournalEntryId = existing.Id, MoodId = mood.Id, Mood = mood, IsPrimary = string.Equals(mn, primaryMood, StringComparison.OrdinalIgnoreCase) });
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteEntryAsync(DateTime date)
        {
            var d = DateOnly(date);
            var existing = await _db.JournalEntries
                .Include(e => e.EntryTags)
                .Include(e => e.EntryMoods)
                .FirstOrDefaultAsync(e => e.EntryDate == d);
            if (existing == null) return false;

            _db.EntryTags.RemoveRange(existing.EntryTags);
            _db.EntryMoods.RemoveRange(existing.EntryMoods);
            _db.JournalEntries.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<JournalEntry>> GetEntriesByMonthAsync(int year, int month)
        {
            var from = new DateTime(year, month, 1).Date;
            var to = from.AddMonths(1).AddDays(-1).Date;

            return await _db.JournalEntries
                .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
                .Include(e => e.EntryMoods).ThenInclude(em => em.Mood)
                .Where(e => e.EntryDate >= from && e.EntryDate <= to)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string? query, DateTime? from, DateTime? to, IEnumerable<string>? moods, IEnumerable<string>? tags)
        {
            var q = _db.JournalEntries
                .Include(e => e.EntryMoods).ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags).ThenInclude(et => et.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.ToLower();
                q = q.Where(e => (e.Title ?? "").ToLower().Contains(lowerQuery) || (e.Content ?? "").ToLower().Contains(lowerQuery));
            }
            if (from.HasValue) q = q.Where(e => e.EntryDate >= from.Value.Date);
            if (to.HasValue) q = q.Where(e => e.EntryDate <= to.Value.Date);
            if (moods != null && moods.Any())
            {
                var moodList = moods.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
                if (moodList.Any())
                    q = q.Where(e => e.EntryMoods.Any(em => moodList.Contains(em.Mood!.Name)));
            }
            if (tags != null && tags.Any())
            {
                var tagList = tags.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
                if (tagList.Any())
                    q = q.Where(e => e.EntryTags.Any(et => tagList.Contains(et.Tag!.Name)));
            }

            return await q.OrderByDescending(e => e.EntryDate).ToListAsync();
        }

        public async Task<List<string>> GetAllMoodsAsync()
        {
            return await _db.Moods
                .Select(m => m.Name)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllTagsAsync()
        {
            return await _db.Tags
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}

using System;

namespace JournalDaily.Models
{
    public class EntryMood
    {
        public Guid Id { get; set; }
        public Guid JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        public Guid MoodId { get; set; }
        public Mood? Mood { get; set; }
        public bool IsPrimary { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace JournalDaily.Models
{
    public class JournalEntry
    {
        public Guid Id { get; set; }
        public DateTime EntryDate { get; set; } // date-only semantics: time set to 00:00:00
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<EntryTag> EntryTags { get; set; } = new();
        public List<EntryMood> EntryMoods { get; set; } = new();
    }
}

using System;

namespace JournalDaily.Models
{
    public class EntryTag
    {
        public Guid Id { get; set; }
        public Guid JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }
        public Guid TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}

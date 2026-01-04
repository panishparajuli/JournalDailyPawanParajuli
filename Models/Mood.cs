using System;

namespace JournalDaily.Models
{
    public class Mood
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        // Category: Positive / Neutral / Negative
        public string Category { get; set; } = "Neutral";
    }
}

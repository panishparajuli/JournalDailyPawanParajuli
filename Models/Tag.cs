using System;

namespace JournalDaily.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Prebuilt { get; set; }
    }
}

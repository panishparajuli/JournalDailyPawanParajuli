using Microsoft.EntityFrameworkCore;
using JournalDaily.Models;

namespace JournalDaily.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
        public DbSet<Mood> Moods { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<EntryTag> EntryTags { get; set; } = null!;
        public DbSet<EntryMood> EntryMoods { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<JournalEntry>()
                .HasIndex(e => e.EntryDate)
                .IsUnique();

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.JournalEntry)
                .WithMany(e => e.EntryTags)
                .HasForeignKey(et => et.JournalEntryId);

            modelBuilder.Entity<EntryTag>()
                .HasOne(et => et.Tag)
                .WithMany()
                .HasForeignKey(et => et.TagId);

            modelBuilder.Entity<EntryMood>()
                .HasOne(em => em.JournalEntry)
                .WithMany(e => e.EntryMoods)
                .HasForeignKey(em => em.JournalEntryId);

            modelBuilder.Entity<EntryMood>()
                .HasOne(em => em.Mood)
                .WithMany()
                .HasForeignKey(em => em.MoodId);
        }
    }
}

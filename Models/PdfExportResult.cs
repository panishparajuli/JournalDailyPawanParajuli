namespace JournalDaily.Models
{
    /// <summary>
    /// Result of a PDF export operation.
    /// </summary>
    public class PdfExportResult
    {
        /// <summary>
        /// Whether export was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Full path to the exported PDF file.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Human-readable file name (e.g., "JournalEntries_Jan01-Jan31.pdf").
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Number of entries included in the PDF.
        /// </summary>
        public int EntriesCount { get; set; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Error message if export failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Date range exported (for display).
        /// </summary>
        public string DateRangeDisplay { get; set; }
    }
}

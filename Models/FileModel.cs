using Microsoft.EntityFrameworkCore;
using System;

namespace File_Management1.Models
{
    // The AppDbContext class, which is the main entry point for interacting with the database.
    public class AppDbContext : DbContext
    {
        // DbSet represents the "Files" table in the database.
        public DbSet<FileMetadata> Files { get; set; }

        // Configure the DbContext to use SQL Server.
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Use your actual connection string here
            optionsBuilder.UseSqlServer("YourConnectionString");
        }
    }

    // This class represents metadata related to each file in the database.
    public class FileMetadata
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; }
    }
}
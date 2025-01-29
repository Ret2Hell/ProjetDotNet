using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Models;

namespace ProjetDotNet.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileModel> Files { get; set; }
    public DbSet<FileCollectionModel> FileCollections { get; set; }
}
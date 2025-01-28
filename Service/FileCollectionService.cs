namespace ProjetDotNet.Service;
using ProjetDotNet.Data;
using ProjetDotNet.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



public interface IFileCollectionService
{
    Task<List<FileCollectionModel>> GetAllFileCollectionsAsync();
    Task<FileCollectionModel> GetFileCollectionByIdAsync(int id);
    Task<FileCollectionModel> CreateFileCollectionAsync(FileCollectionModel fileCollection);
    Task UpdateFileCollectionAsync(int id, FileCollectionModel fileCollection);
    Task DeleteFileCollectionAsync(int id);
    Task<bool> FileCollectionExistsAsync(int id);
    Task AddFileToCollectionAsync(int collectionId, int fileId);
    Task RemoveFileFromCollectionAsync(int collectionId, int fileId);

}

public class FileCollectionService : IFileCollectionService
{
    private readonly ApplicationDbContext _context;
    public FileCollectionService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<FileCollectionModel>> GetAllFileCollectionsAsync()
    {
        return await _context.FileCollections
            .Include(fc => fc.SubCollections)
            .Include(fc => fc.Files)
            .ToListAsync();
    }
    public async Task<FileCollectionModel?> GetFileCollectionByIdAsync(int id)
    {
        return await _context.FileCollections  
            .Include(fc => fc.SubCollections)
            .Include(fc => fc.Files)
            .FirstOrDefaultAsync(fc => fc.Id == id);
    }

    public async Task<FileCollectionModel> CreateFileCollectionAsync(FileCollectionModel fileCollection)
    {
        _context.FileCollections.Add(fileCollection);
        await _context.SaveChangesAsync();
        return fileCollection;
    }

    public async Task UpdateFileCollectionAsync(int id, FileCollectionModel fileCollection)
    {
        var existingFileCollection = await _context.FileCollections.FindAsync(id);
        if (existingFileCollection != null)
        {
            existingFileCollection.Name = fileCollection.Name;
            existingFileCollection.Description = fileCollection.Description;
            existingFileCollection.Files = fileCollection.Files;
            existingFileCollection.SubCollections = fileCollection.SubCollections;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteFileCollectionAsync(int id)
    {
        var fileCollection = await _context.FileCollections.FindAsync(id);
        if (fileCollection != null)
        {
            _context.FileCollections.Remove(fileCollection);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> FileCollectionExistsAsync(int id)
    {
        return await _context.FileCollections.AnyAsync(fc => fc.Id == id);
    }
    
    public async Task AddFileToCollectionAsync(int collectionId, int fileId)
    {
        var fileCollection = await _context.FileCollections.FindAsync(collectionId);
        var file = await _context.Files.FindAsync(fileId);
        if (fileCollection != null && file != null)
        {
            fileCollection.Files.Add(file);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveFileFromCollectionAsync(int collectionId, int fileId)
    {
        var fileCollection = await _context.FileCollections
            .Include(fc => fc.Files)
            .FirstOrDefaultAsync(fc => fc.Id == collectionId);

        if (fileCollection != null)
        {
            var file = fileCollection.Files.FirstOrDefault(f => f.Id == fileId);
            if (file != null)
            {
                fileCollection.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
        }
    }
}
namespace ProjetDotNet.Service;
using ProjetDotNet.Data;
using ProjetDotNet.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



public interface IFileCollectionService
{
    Task<object> GetAllFileCollectionsAsync(int parentId);
    Task<FileCollectionModel> GetFileCollectionByIdAsync(int id);
    Task<FileCollectionModel> CreateFileCollectionAsync(FileCollectionModel fileCollection);
    Task UpdateFileCollectionAsync(int id, String name);
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
    
    public async Task<object> GetAllFileCollectionsAsync(int parentId = -1)
    {
        if (parentId == -1)
        {
            
            var collections = await _context.FileCollections
                .Where(fc => fc.ParentCollection == null)
                .Include(fc => fc.Files)
                .Include(fc => fc.SubCollections)
                .Select(fc => new 
                { 
                    Id = fc.Id, 
                    Name = fc.Name, 
                    SubCollections = fc.SubCollections.Select(sub => new { sub.Id, sub.Name }),
                    Files = fc.Files.Select(f => new { f.Id, f.FileName }) 
                })
                .ToListAsync();

            return collections;
        }

        
        var parentCollection = await _context.FileCollections
            .Where(fc => fc.Id == parentId)
            .Include(fc => fc.Files)
            .FirstOrDefaultAsync();

        if (parentCollection == null)
        {
            return null; 
        }

        
        var subCollections = await _context.FileCollections
            .Where(fc => fc.ParentCollection.Id == parentId)
            .Include(fc => fc.Files)
            .Include(fc => fc.SubCollections)
            .Select(fc => new 
            { 
                Id = fc.Id, 
                Name = fc.Name, 
                SubCollections = fc.SubCollections.Select(sub => new { sub.Id, sub.Name }),
                Files = fc.Files.Select(f => new { f.Id, f.FileName }) 
            })
            .ToListAsync();

        
        return new 
        {
            ParentCollection = new 
            {
                Id = parentCollection.Id,
                Name = parentCollection.Name,
                Files = parentCollection.Files.Select(f => new { f.Id, f.FileName })
            },
            SubCollections = subCollections
        };
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
        if (fileCollection.ParentCollectionId.HasValue)
        {
            fileCollection.ParentCollection = await _context.FileCollections
                .FindAsync(fileCollection.ParentCollectionId);
        }

        return fileCollection;
    }

    public async Task UpdateFileCollectionAsync(int id, String name)
    {
        var existingFileCollection = await _context.FileCollections.FindAsync(id);
        if (existingFileCollection != null)
        {
            existingFileCollection.Name = name;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteFileCollectionAsync(int id)
    {
        var fileCollection = await _context.FileCollections
            .Include(fc => fc.Files)
            .Include(fc => fc.SubCollections)
            .FirstOrDefaultAsync(fc => fc.Id == id);

        if (fileCollection != null)
        {
            foreach (var subCollection in fileCollection.SubCollections.ToList())
            {
                await DeleteFileCollectionAsync(subCollection.Id);
            }
            _context.Files.RemoveRange(fileCollection.Files);
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
        var collection = await _context.FileCollections
            .Include(fc => fc.Files)
            .FirstOrDefaultAsync(fc => fc.Id == collectionId);

        var file = await _context.Files.FindAsync(fileId);

        // ✅ Add null checks
        if (collection == null)
            throw new NullReferenceException($"Collection with ID {collectionId} not found.");

        if (file == null)
            throw new NullReferenceException($"File with ID {fileId} not found.");

        collection.Files.Add(file);
        await _context.SaveChangesAsync();
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
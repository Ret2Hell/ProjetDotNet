using ProjetDotNet.Data;
using ProjetDotNet.Models;
using ProjetDotNet.Service;

public interface IFileService
{
    Task<FileModel> UploadFileAsync(IFormFile file, int collectionId);
    Task<FileModel> GetFileAsync(int id);
    Task<byte[]> DownloadFileAsync(int id);
    Task DeleteFileAsync(int id);
    Task UpdateFileAsync(int id, String name);
}

public class FileService : IFileService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadDirectory;
    private readonly IFileCollectionService _fileCollectionService;

    public FileService(ApplicationDbContext context, IWebHostEnvironment environment, IFileCollectionService fileCollectionService)
    {
        _context = context;
        _uploadDirectory = Path.Combine(environment.ContentRootPath, "Uploads");
        Directory.CreateDirectory(_uploadDirectory);
        _fileCollectionService = fileCollectionService;
    }

    public async Task<FileModel> UploadFileAsync(IFormFile file, int collectionId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file");

        string fileName = Path.GetFileName(file.FileName);
        string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        string filePath = Path.Combine(_uploadDirectory, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileModel = new FileModel
        {
            FileName = fileName,
            FilePath = filePath,
            UploadedBy = "User", // Constant for now, change it to username
            UploadedOn = DateTime.UtcNow
        };

        _context.Files.Add(fileModel);
        await _context.SaveChangesAsync();
        await _fileCollectionService.AddFileToCollectionAsync(collectionId, fileModel.Id);
        return fileModel;
    }

    public async Task<FileModel> GetFileAsync(int id)
    {
        return await _context.Files.FindAsync(id);
    }

    public async Task<byte[]> DownloadFileAsync(int id)
    {
        var file = await GetFileAsync(id);
        if (file == null) throw new FileNotFoundException();

        return await File.ReadAllBytesAsync(file.FilePath);
    }

    public async Task DeleteFileAsync(int id)
    {
        var file = await GetFileAsync(id);
        if (file == null) throw new FileNotFoundException();

        if (File.Exists(file.FilePath))
            File.Delete(file.FilePath);

        _context.Files.Remove(file);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFileAsync(int id, String name)
    {
        var file = await GetFileAsync(id);
        if (file == null) throw new FileNotFoundException();
        file.FileName = name;
        await _context.SaveChangesAsync();
    }
}
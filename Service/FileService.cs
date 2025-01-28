using ProjetDotNet.Data;
using ProjetDotNet.Models;

public interface IFileService
{
    Task<FileModel> UploadFileAsync(IFormFile file);
    Task<FileModel> GetFileAsync(int id);
    Task<byte[]> DownloadFileAsync(int id);
    Task DeleteFileAsync(int id);
}

public class FileService : IFileService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadDirectory;

    public FileService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _uploadDirectory = Path.Combine(environment.ContentRootPath, "Uploads");
        Directory.CreateDirectory(_uploadDirectory);
    }

    public async Task<FileModel> UploadFileAsync(IFormFile file)
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
}
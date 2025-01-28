using File_Management1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;

[Route("api/[controller]")] // Base URL for this controller: /api/files
[ApiController]
public class FilesController : Controller
{
    private readonly AppDbContext _context;

    public FilesController(AppDbContext context)
    {
        _context = context; // Inject the database context for saving file metadata
    }

    // Endpoint: POST /api/files/upload

    //Upload a file

    // Step 1: Display the Upload Page
    [HttpGet]
    [Authorize(Roles = "Student, Admin")] // Only students and admins can upload
    public IActionResult Upload()
    {
        return View(); // Show the Upload.cshtml view
    }

    // Step 2: Handle File Upload Request
    [HttpPost]
    [Authorize(Roles = "Student, Admin")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        // 1️⃣ Check if a file was selected
        if (file == null || file.Length == 0)
        {
            ViewBag.Message = "No file selected or file is empty!";
            return View("Upload");
        }

        // 2️⃣ Define the folder to store uploaded files
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        // Create the folder if it does not exist
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // 3️⃣ Generate a unique file name to avoid overwriting existing files
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // 4️⃣ Save the file to the server
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // 5️⃣ Save file metadata to the database
        var fileMetadata = new FileMetadata
        {
            FileName = file.FileName, // Store original file name
            FilePath = "/uploads/" + uniqueFileName, // Store relative path
            UploadedBy = User.Identity.Name, // Store uploader's username
            UploadDate = DateTime.Now
        };

        _context.Files.Add(fileMetadata);
        await _context.SaveChangesAsync();

        // 6️⃣ Show a success message
        ViewBag.Message = "File uploaded successfully!";
        return View("Upload"); // Reload the Upload page with a success message
    }

    // List all files 

    [HttpGet("list")]
    [Authorize]
    public async Task<IActionResult> GetFiles()
    {
        var files = await _context.Files.ToListAsync();
        return View("List", files); // Pass the list of files to the "List" View
    }

    //Download a file by ID
    [HttpGet("download/{id}")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null)
            return NotFound();

        var memory = new MemoryStream();
        using (var stream = new FileStream(file.FilePath, FileMode.Open))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        return File(memory, "application/octet-stream", file.FileName);
    }

    // Delete a file by ID
    [HttpDelete("delete/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        var file = await _context.Files.FindAsync(id);
        if (file == null)
            return NotFound();

        if (System.IO.File.Exists(file.FilePath))
            System.IO.File.Delete(file.FilePath);

        _context.Files.Remove(file);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "File deleted successfully!" });
    }

}
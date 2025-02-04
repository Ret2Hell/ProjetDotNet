using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Data;
using ProjetDotNet.Service;

namespace ProjetDotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IPdfParserService _pdfParserService;
    private readonly ApplicationDbContext _context;

    public FileController(ApplicationDbContext context, IFileService fileService, IPdfParserService pdfParserService)
    {
        _context = context;
        _fileService = fileService;
        _pdfParserService = pdfParserService;
    }

    // POST: api/file/upload
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var result = await _fileService.UploadFileAsync(file);
        return Ok(result);
    }

    // GET: api/file/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var file = await _fileService.GetFileAsync(id);
            var fileBytes = await _fileService.DownloadFileAsync(id);
            return File(fileBytes, "application/pdf", file.FileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    // GET: api/file/parse/{id}
    [HttpGet("parse/{id}")]
    public async Task<IActionResult> ParsePdf(int id)
    {
        try
        {
            var file = await _fileService.GetFileAsync(id);
            var text = await _pdfParserService.ParsePdfToText(file.FilePath);
            return Ok(new { Text = text });
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE: api/file/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _fileService.DeleteFileAsync(id);
            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    // GET: api/file
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var files = await _context.Files.ToListAsync();
        return Ok(files);
    }
}

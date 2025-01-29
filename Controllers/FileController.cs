using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetDotNet.Data;
using ProjetDotNet.Service;

namespace ProjetDotNet.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : Controller
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

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var result = await _fileService.UploadFileAsync(file);
        return Ok(result);
    }

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
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _fileService.DeleteFileAsync(id);
            return Ok();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }
    
    [HttpGet("index")]
    public async Task<IActionResult> Index()
    {
        var files = await _context.Files.ToListAsync();
        return View(files);
    }

    [HttpGet("upload")]
    public IActionResult Upload()
    {
        return View();
    }
}
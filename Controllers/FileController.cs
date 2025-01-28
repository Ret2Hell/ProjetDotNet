// Controllers/FileController.cs

using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Service;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly IPdfParserService _pdfParserService;

    public FileController(IFileService fileService, IPdfParserService pdfParserService)
    {
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
}
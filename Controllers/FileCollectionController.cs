using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Models;
using ProjetDotNet.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileCollectionController : ControllerBase
    {
        private readonly IFileCollectionService _fileCollectionService;

        public FileCollectionController(IFileCollectionService fileCollectionService)
        {
            _fileCollectionService = fileCollectionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileCollectionModel>>> GetAllFileCollections()
        {
            var fileCollections = await _fileCollectionService.GetAllFileCollectionsAsync();
            return Ok(fileCollections);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileCollectionModel>> GetFileCollectionById(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }
            return Ok(fileCollection);
        }

        [HttpPost]
        public async Task<ActionResult<FileCollectionModel>> CreateFileCollection(FileCollectionModel fileCollection)
        {
            var createdFileCollection = await _fileCollectionService.CreateFileCollectionAsync(fileCollection);
            return CreatedAtAction(nameof(GetFileCollectionById), new { id = createdFileCollection.Id }, createdFileCollection);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFileCollection(int id, FileCollectionModel fileCollection)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(id))
            {
                return NotFound();
            }

            await _fileCollectionService.UpdateFileCollectionAsync(id, fileCollection);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFileCollection(int id)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(id))
            {
                return NotFound();
            }

            await _fileCollectionService.DeleteFileCollectionAsync(id);
            return NoContent();
        }

        [HttpPost("{collectionId}/files/{fileId}")]
        public async Task<IActionResult> AddFileToCollection(int collectionId, int fileId)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(collectionId))
            {
                return NotFound();
            }

            await _fileCollectionService.AddFileToCollectionAsync(collectionId, fileId);
            return NoContent();
        }

        [HttpDelete("{collectionId}/files/{fileId}")]
        public async Task<IActionResult> RemoveFileFromCollection(int collectionId, int fileId)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(collectionId))
            {
                return NotFound();
            }

            await _fileCollectionService.RemoveFileFromCollectionAsync(collectionId, fileId);
            return NoContent();
        }
    }
}

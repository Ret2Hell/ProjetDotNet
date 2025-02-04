using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Models;
using ProjetDotNet.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetDotNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileCollectionController : ControllerBase
    {
        private readonly IFileCollectionService _fileCollectionService;

        public FileCollectionController(IFileCollectionService fileCollectionService)
        {
            _fileCollectionService = fileCollectionService;
        }

        // GET: api/filecollection/parent/{parentid}
        [HttpGet("parent/{parentid}")]
        public async Task<ActionResult<IEnumerable<FileCollectionModel>>> GetAll(int? parentId)
        {
            var fileCollections = await _fileCollectionService.GetAllFileCollectionsAsync(parentId ?? -1);
            return Ok(fileCollections);
        }
        
        // GET: api/filecollection/parent
        [HttpGet("parent")]
        public async Task<ActionResult<IEnumerable<FileCollectionModel>>> GetAll()
        {
            var fileCollections = await _fileCollectionService.GetAllFileCollectionsAsync(-1);
            return Ok(fileCollections);
        }

        // GET: api/filecollection/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FileCollectionModel>> Get(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }
            return Ok(fileCollection);
        }

        // POST: api/filecollection
        [HttpPost]
        public async Task<ActionResult<FileCollectionModel>> Create([FromBody] FileCollectionModel fileCollection)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdFileCollection = await _fileCollectionService.CreateFileCollectionAsync(fileCollection);

            return Ok(createdFileCollection);
        }


        // PUT: api/filecollection/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FileCollectionModel fileCollection)
        {
            if (id != fileCollection.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _fileCollectionService.UpdateFileCollectionAsync(id, fileCollection);
            return NoContent();
        }

        // DELETE: api/filecollection/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }

            await _fileCollectionService.DeleteFileCollectionAsync(id);
            return NoContent();
        }

        // POST: api/filecollection/{collectionId}/files/{fileId}/add
        [HttpPost("{collectionId}/files/{fileId}/add")]
        public async Task<IActionResult> AddFileToCollection(int collectionId, int fileId)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(collectionId))
            {
                return NotFound();
            }

            await _fileCollectionService.AddFileToCollectionAsync(collectionId, fileId);
            return NoContent();
        }

        // DELETE: api/filecollection/{collectionId}/files/{fileId}/remove
        [HttpDelete("{collectionId}/files/{fileId}/remove")]
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
using Microsoft.AspNetCore.Mvc;
using ProjetDotNet.Models;
using ProjetDotNet.Service;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetDotNet.Controllers
{
    public class FileCollectionController : Controller
    {
        private readonly IFileCollectionService _fileCollectionService;

        public FileCollectionController(IFileCollectionService fileCollectionService)
        {
            _fileCollectionService = fileCollectionService;
        }

        // GET: /file-collection/
        public async Task<IActionResult> Index()
        {
            var fileCollections = await _fileCollectionService.GetAllFileCollectionsAsync();
            return View(fileCollections);  // Return view with fileCollections data
        }

        // GET: /file-collection/{id}
        public async Task<IActionResult> Details(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }
            return View(fileCollection);  // Return view with single fileCollection data
        }

        // GET: /file-collection/create
        public IActionResult Create()
        {
            return View();  // Return view for creating a file collection
        }

        // POST: /file-collection/create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FileCollectionModel fileCollection)
        {
            if (ModelState.IsValid)
            {
                var createdFileCollection = await _fileCollectionService.CreateFileCollectionAsync(fileCollection);
                return RedirectToAction(nameof(Details), new { id = createdFileCollection.Id });  // Redirect to the created file collection details
            }
            return View(fileCollection);  // If the model state is invalid, return to the same view
        }

        // GET: /file-collection/edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }
            return View(fileCollection);  // Return view with file collection data to edit
        }

        // POST: /file-collection/edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FileCollectionModel fileCollection)
        {
            if (id != fileCollection.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _fileCollectionService.UpdateFileCollectionAsync(id, fileCollection);
                return RedirectToAction(nameof(Details), new { id = fileCollection.Id });  // Redirect to updated file collection details
            }
            return View(fileCollection);  // Return to the same view if model state is invalid
        }

        // GET: /file-collection/delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var fileCollection = await _fileCollectionService.GetFileCollectionByIdAsync(id);
            if (fileCollection == null)
            {
                return NotFound();
            }
            return View(fileCollection);  // Return view for confirmation to delete
        }

        // POST: /file-collection/delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fileCollectionService.DeleteFileCollectionAsync(id);
            return RedirectToAction(nameof(Index));  // Redirect to the index page after deletion
        }

        // GET: /file-collection/{collectionId}/files/{fileId}/add
        public async Task<IActionResult> AddFileToCollection(int collectionId, int fileId)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(collectionId))
            {
                return NotFound();
            }
            await _fileCollectionService.AddFileToCollectionAsync(collectionId, fileId);
            return RedirectToAction(nameof(Details), new { id = collectionId });  // Redirect back to the file collection details
        }

        // GET: /file-collection/{collectionId}/files/{fileId}/remove
        public async Task<IActionResult> RemoveFileFromCollection(int collectionId, int fileId)
        {
            if (!await _fileCollectionService.FileCollectionExistsAsync(collectionId))
            {
                return NotFound();
            }
            await _fileCollectionService.RemoveFileFromCollectionAsync(collectionId, fileId);
            return RedirectToAction(nameof(Details), new { id = collectionId });  // Redirect back to the file collection details
        }
    }
}

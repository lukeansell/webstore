using Microsoft.AspNetCore.Mvc;
using cldv_poe.Services;
using cldv_poe.Models;

namespace cldv_poe.Controllers
{
    public class FilesController : Controller
    {
        private readonly AzureFileShareService _azureFileShareService;
        private List<string> dirs = ["uploads", "contracts"];

        public FilesController(AzureFileShareService azureFileShareService)
        {
            _azureFileShareService = azureFileShareService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Dirs"] = dirs;
            List<FileModel> files;
            try
            {
                files = await _azureFileShareService.ListFilesAsync(dirs);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Failed to load files: {ex.Message}";
                files = new List<FileModel>();
            }
            return View(files);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(string dirName, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please select a file to upload");
                return RedirectToAction("Index");
            }
            if (string.IsNullOrEmpty(dirName))
            {
                ModelState.AddModelError("Directory", "Please select a directory");
                return RedirectToAction("Index");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    //string dirName = "uploads";
                    string fName = file.FileName;
                    await _azureFileShareService.UploadFileAsync(dirName, fName, stream);
                }
                TempData["Message"] = $"File '{file.FileName}' uploaded successfully";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"File ({file.FileName}) upload failed: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string dirName, string fName)
        {
            if (string.IsNullOrEmpty(fName))
            {
                return BadRequest("File name cannot be null or empty");
            }

            try
            {

                var fStream = await _azureFileShareService.DownloadFileAsync(dirName, fName);
                if (fStream == null)
                {
                    return BadRequest($"File '{fName}' not found");
                }
                return File(fStream, "application/octet-stream", fName);
            }
            catch (Exception ex)
            {
                throw new Exception($"DownloadFile error ({fName}): {ex.Message}", ex);
            }
        }
    }
}

using cldv_poe.Services;
using Microsoft.AspNetCore.Mvc;
using cldv_poe.Models;
using System.Runtime.InteropServices;

namespace cldv_poe.Controllers
{
    public class ProductController(BlobService blobService, TableStorageService tableStorageService, QueueService queueService) : Controller
    {
        private readonly BlobService _blobService = blobService;
        private readonly TableStorageService _tableStorageService = tableStorageService;
        private readonly QueueService _queueService = queueService;

        public async Task<IActionResult> Index()
        {
            var products = await _tableStorageService.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, IFormFile file)
        {
            product.ProductID = _tableStorageService.GetNextProductID();
            if (file != null)
            {
                // rename the file to the product id
                var fileName = $"product_{product.ProductID}{Path.GetExtension(file.FileName)}";
                using var stream = file.OpenReadStream();
                var imageUrl = await _blobService.UploadAsync(stream, fileName);
                product.ImageUrl = imageUrl;
            }

            if (ModelState.IsValid)
            {

                product.RowKey = Guid.NewGuid().ToString();
                await _tableStorageService.AddProductAsync(product);
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var modelState in ViewData.ModelState.Values)
                    foreach (var error in modelState.Errors)
                        Console.WriteLine(error.ErrorMessage);
            }
            return View(product);
        }

        public async Task<IActionResult> DeleteProduct(string rowKey, Product product)
        {
            if (product != null && !string.IsNullOrEmpty(product.ImageUrl))
            {
                await _blobService.DeleteBlobAsync(product.ImageUrl);
            }
            await _tableStorageService.DeleteProductAsync(rowKey);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string rowKey)
        {
            Product? product = await _tableStorageService.GetProductAsync(rowKey);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductAsync(Product product)
        {
            try
            {
                await _tableStorageService.UpdateProductAsync(product);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating product", ex);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewProduct(string rowKey)
        {
            if (string.IsNullOrEmpty(rowKey))
                throw new ArgumentException("No rowKey provided");
            var product = await _tableStorageService.GetProductAsync(rowKey);
            if (product == null)
                return RedirectToAction("Index");
            ViewData["ImgUrl"] = product.ImageUrl;
            return View(product);
        }

    }
}

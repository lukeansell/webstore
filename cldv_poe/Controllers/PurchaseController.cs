using cldv_poe.Services;
using cldv_poe.Models;
using Microsoft.AspNetCore.Mvc;

namespace cldv_poe.Controllers
{
    public class PurchaseController : Controller
    {

        private readonly TableStorageService _tableStorageService;
        private readonly QueueService _queueService;

        public PurchaseController(TableStorageService tableStorageService, QueueService queueService)
        {
            _tableStorageService = tableStorageService;
            _queueService = queueService;
        }
        public async Task<IActionResult> Index()
        {
            var purchases = await _tableStorageService.GetAllPurchasesAsync();
            return View(purchases);
        }

        public IActionResult Details(Purchase purchase)
        {
            return View(purchase);
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPurchase()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            var products = await _tableStorageService.GetAllProductsAsync();
            if (customers == null || customers.Count == 0)
            {
                ModelState.AddModelError("", "No customers found.");
                return View();
            }
            if (products == null || products.Count == 0)
            {
                ModelState.AddModelError("", "No products found.");
                return View();
            }

            ViewData["Customers"] = customers;
            ViewData["Products"] = products;
            ViewData["TableStorageService"] = _tableStorageService;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPurchase(Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                purchase.PurchaseID = _tableStorageService.GetNextPurchaseID();
                purchase.RowKey = Guid.NewGuid().ToString();
                purchase.PurchaseDate = DateTime.SpecifyKind(purchase.PurchaseDate, DateTimeKind.Utc);
                // check exsists
                if (!_tableStorageService.HasProduct(purchase.ProductID))
                    throw new Exception("Product does not exist");
                var res = await _tableStorageService.GetProductAsync(purchase.ProductID)
                    ?? throw new Exception("Product is null");
                Product product = res;
                // check stock
                if (!product.HasStock)
                    throw new Exception("Product is out of stuck");
                if (product.Stock < purchase.Quantity)
                    throw new Exception($"Less stock than requested there are {product.Stock} items left");
                purchase.TotalPrice = product.Price * purchase.Quantity;

                await _tableStorageService.AddPurchaseAsync(purchase);
                // reduce stock
                product.Stock = product.Stock - purchase.Quantity;
                await _tableStorageService.UpdateProductAsync(product);
                await _queueService.LogProcessPurchaseAsync(purchase);
                return RedirectToAction("Index");
            }
            return View(purchase);
        }


        public async Task<double> CalculatePrice(int productID, int quantity)
        {
            var product = await _tableStorageService.GetProductAsync(productID);
            if (product == null)
                return 0.0;
            var price = product.Price * quantity;
            return price;
        }

        public async Task<Product?> Product(int productID)
        {
            return await _tableStorageService.GetProductAsync(productID);
        }

        public async Task<int> ProductStock(int productID)
        {
            var product = await _tableStorageService.GetProductAsync(productID);
            if (product == null)
                return 0;
            return product.Stock;
        }

        [HttpGet]
        public async Task<IActionResult> DetailPurchase(string rowKey)
        {
            Purchase? purchase = await _tableStorageService.GetPurchaseAsync(rowKey);
            if (purchase == null)
                return NotFound("Purchase not found");
            int customerID = purchase.CustomerID;
            var customer = await _tableStorageService.GetCustomerAsync(customerID);
            //if (customer != null)
                ViewData["Customer"] = customer as Customer;
            var product = await _tableStorageService.GetProductAsync(purchase.ProductID);
            //if (product != null)
                ViewData["Product"] = product as Product;
          

            return View(purchase);
        }




    }
}

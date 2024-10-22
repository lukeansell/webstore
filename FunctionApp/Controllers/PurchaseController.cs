using FunctionApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FunctionApp.Controllers
{
    public class PurchaseController(TableStorageService tableStorageService, QueueService queueService)
    {
        private readonly TableStorageService _tableStorageService = tableStorageService;
        private readonly QueueService _queueService = queueService;

        public async Task<IActionResult> AddPurchaseAsync(int customerID, int productID, int quantity)
        {
            Customer? customer = await _tableStorageService.GetCustomerAsync(customerID);
            if (customer == null) return new BadRequestObjectResult("Customer not found");
            Product? product = await _tableStorageService.GetProductAsync(productID);
            if (product == null) return new BadRequestObjectResult("Product not found");
            if (quantity < 1) return new BadRequestObjectResult("Quantity must be greater than 0");

            if (product.Stock < quantity)
                return new BadRequestObjectResult("Not enough stock");

            Purchase purchase = new()
            {
                PurchaseID = _tableStorageService.GetNextPurchaseID(),
                ProductID = productID,
                CustomerID = customerID,
                Quantity = quantity,
                TotalPrice = product.Price * quantity,
                PurchaseDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                PartitionKey = Purchase.PKey,
                RowKey = Guid.NewGuid().ToString()
            };

            await _tableStorageService.AddPurchaseAsync(purchase);
            product.Stock -= quantity;
            await _tableStorageService.UpdateProductAsync(product);
            await _queueService.LogProcessPurchaseAsync(purchase);

            return new OkObjectResult(JsonConvert.SerializeObject(purchase));
        }

        public async Task<IActionResult> GetPurchaseAsync(int purchaseID)
        {
            if (purchaseID < 1)
                return new BadRequestObjectResult("Invalid Purchase ID");

            Purchase? purchase = await _tableStorageService.GetPurchaseAsync(purchaseID);
            if (purchase == null)
                return new NotFoundObjectResult("Purchase not found");
            return new OkObjectResult(JsonConvert.SerializeObject(purchase));
        }

        public async Task<IActionResult> GetAllPurchasesAsync()
        {
            List<Purchase> purchases = await _tableStorageService.GetAllPurchasesAsync();
            return new OkObjectResult(JsonConvert.SerializeObject(purchases));
        }
    }
}

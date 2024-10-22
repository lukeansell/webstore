using Azure.Data.Tables;
using FunctionApp.Models;
using FunctionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp.Controllers
{
    public class ProductController(TableStorageService tableStorageService)
    {
        private readonly TableStorageService _tableStorageService = tableStorageService;

        public async Task<IActionResult> GetProductAsync(int productID)
        {
            if (productID < 1)
                return new BadRequestObjectResult("Invalid Product ID");

            Product? product = await _tableStorageService.GetProductAsync(productID);
            if (product == null)
                return new NotFoundObjectResult("Product not found");

            return new OkObjectResult(JsonConvert.SerializeObject(product));
        }
    }
}

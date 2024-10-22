using FunctionApp.Services;
using FunctionApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunctionApp
{
    public class Function1(ILogger<Function1> logger, CustomerController customerController, PurchaseController purchaseController, ProductController productController, AzureFileShareService azureFileShareService, BlobService blobService)
    {
        private readonly ILogger<Function1> _logger = logger;
        private readonly CustomerController _customerController = customerController;
        private readonly PurchaseController _purchaseController = purchaseController;
        private readonly ProductController _productController = productController;
        private readonly AzureFileShareService _azureFileShareService = azureFileShareService;
        private readonly BlobService _blobService = blobService;

        //-------------------- CUSTOMER --------------------

        [Function("AddCustomerJSON")]
        public async Task<IActionResult> AddCustomerJSON([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger AddCustomerJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");

            string? name = data.CustomerName;
            string? email = data.CustomerEmail;
            string? phone = data.PhoneNumber;
            string? address = data.Address;

            return await _customerController.AddCustomerAsync(name, email, phone, address);
        }

        [Function("AddCustomerForm")]
        public async Task<IActionResult> AddCustomerForm([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger AddCustomerForm");
            string? name = req.Form["CustomerName"];
            string? email = req.Form["CustomerEmail"];
            string? phone = req.Form["PhoneNumber"];
            string? address = req.Form["Address"];

            return await _customerController.AddCustomerAsync(name, email, phone, address);
        }

        [Function("GetCustomerJSON")]
        public async Task<IActionResult> GetCustomerJSON([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger GetCustomerJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");

            if (!int.TryParse(data.CustomerID.ToString(), out int id))
                return new BadRequestObjectResult("Invalid or missing 'CustomerID' parameter.");

            return await _customerController.GetCustomerAsync(id);
        }

        [Function("GetCustomerForm")]
        public async Task<IActionResult> GetCustomerForm([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger GetCustomerForm");
            if (!int.TryParse(req.Form["CustomerID"], out int id))
                return new BadRequestObjectResult("Invalid or missing 'CustomerID' parameter.");

            return await _customerController.GetCustomerAsync(id);
        }

        [Function("DeleteCustomerJSON")]
        public async Task<IActionResult> DeleteCustomerJSON([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger DeleteCustomerJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");

            if (!int.TryParse(data.CustomerID.ToString(), out int id))
                return new BadRequestObjectResult("Invalid or missing 'CustomerID' parameter.");

            return await _customerController.DeleteCustomerAsync(id);
        }

        [Function("DeleteCustomerForm")]
        public async Task<IActionResult> DeleteCustomerForm([HttpTrigger(AuthorizationLevel.Anonymous, "delete")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger DeleteCustomerForm");
            if (!int.TryParse(req.Form["CustomerID"], out int id))
                return new BadRequestObjectResult("Invalid or missing 'CustomerID' parameter.");

            return await _customerController.DeleteCustomerAsync(id);
        }

        //-------------------- PURCHASE --------------------

        [Function("ProcessPurchaseJSON")]
        public async Task<IActionResult> ProcessPurchaseJSON([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger ProcessPurchaseJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            int customerID = 0, productID = 0, quantity = 0;
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");

            if (data.CustomerID == null || !int.TryParse(data.CustomerID.ToString(), out customerID))
                return new BadRequestObjectResult("Invalid or missing 'CustomerID' parameter.");

            if (data.ProductID == null || !int.TryParse(data.ProductID.ToString(), out productID))
                return new BadRequestObjectResult("Invalid or missing 'ProductID' parameter.");

            if (data.Quantity == null || !int.TryParse(data.Quantity.ToString(), out quantity))
                return new BadRequestObjectResult("Invalid or missing 'Quantity' parameter.");

            return await _purchaseController.AddPurchaseAsync(customerID, productID, quantity);
        }

        [Function("GetPurchaseJSON")]
        public async Task<IActionResult> GetPurchaseJSON([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger GetPurchaseJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");
            if (!int.TryParse(data.PurchaseID.ToString(), out int purchaseID))
                return new BadRequestObjectResult("Invalid or missing 'PurchaseID' parameter.");
            return await _purchaseController.GetPurchaseAsync(purchaseID);
        }

        [Function("GetAllPurchasesJSON")]
        public async Task<IActionResult> GetAllPurchasesJSON([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger GetAllPurchases");
            return await _purchaseController.GetAllPurchasesAsync();
        }

        //-------------------- PRODUCT --------------------

        [Function("GetProductJSON")]
        public async Task<IActionResult> GetProductJSON([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger GetProductJSON");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
                return new BadRequestObjectResult("Invalid JSON");
            if (!int.TryParse(data.ProductID.ToString(), out int productID))
                return new BadRequestObjectResult("Invalid or missing 'ProductID' parameter.");

            return await _productController.GetProductAsync(productID);
        }

        //-------------------- UPLOAD --------------------

        [Function("UploadFile")]
        public async Task<IActionResult> UploadFile([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger UploadFile");
            try
            {
                if (!req.Form.Files.Any())
                    return new BadRequestObjectResult("No file uploaded.");

                var file = req.Form.Files[0];
                string? dirName = req.Form["DirectoryName"];
                if (string.IsNullOrEmpty(dirName))
                    return new BadRequestObjectResult("DirectoryName is required.");
                var fileName = file.FileName;

                if (string.IsNullOrEmpty(dirName))
                    return new BadRequestObjectResult("Directory name is required.");

                using (var stream = file.OpenReadStream())
                    await _azureFileShareService.UploadFileAsync(dirName, fileName, stream);

                return new OkObjectResult($"File '{fileName}' uploaded to directory '{dirName}' successfully.");
            }
            catch (Exception)
            {
                return new BadRequestObjectResult("Request form data missing");
            }
        }

        [Function("UploadBlobStorage")]
        public async Task<IActionResult> UploadBlobStorage([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
        {
            _logger.LogInformation("HTTP trigger UploadBlobStorage");

            try
            {
                if (!req.Form.Files.Any())
                    return new BadRequestObjectResult("No file uploaded.");

                var file = req.Form.Files[0];

                var fileName = file.FileName;

                using var stream = file.OpenReadStream();
                var blobUri = await _blobService.UploadAsync(stream, fileName);
                return new OkObjectResult($"File '{fileName}' uploaded to Blob Storage successfully. Blob URI: {blobUri}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error uploading file to Blob Storage");
                return new BadRequestObjectResult("Error uploading file to Blob Storage");
            }
        }

    }
}

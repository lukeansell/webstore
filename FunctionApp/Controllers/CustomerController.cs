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
    public class CustomerController(TableStorageService tableStorageService)
    {
        private readonly TableStorageService _tableStorageService = tableStorageService;

        public async Task<IActionResult> AddCustomerAsync(string? name, string? email, string? phone, string? address)
        {
            StringBuilder missing = new();
            if (string.IsNullOrEmpty(name)) missing.AppendLine("CustomerName");
            if (string.IsNullOrEmpty(email)) missing.AppendLine("CustomerEmail");
            if (string.IsNullOrEmpty(phone)) missing.AppendLine("PhoneNumber");
            if (string.IsNullOrEmpty(address)) missing.AppendLine("Address");
            if (missing.Length > 0)
                return new BadRequestObjectResult("Invalid or missing:\n" + missing.ToString());

            Customer customer = new()
            {
                CustomerID = _tableStorageService.GetNextCustomerID(),
                CustomerName = name,
                CustomerEmail = email,
                PhoneNumber = phone,
                Address = address,
                PartitionKey = Customer.PKey,
                RowKey = Guid.NewGuid().ToString()
            };

            await _tableStorageService.AddCustomerAsync(customer);

            return new OkObjectResult($"Customer added with CustomerID: {customer.CustomerID}");
        }

        public async Task<IActionResult> GetCustomerAsync(int id)
        {
            Customer? cust = await _tableStorageService.GetCustomerAsync(id);
            if (cust == null)
                return new BadRequestObjectResult($"Customer with CustomerID: {id} not found");

            return new OkObjectResult(JsonConvert.SerializeObject(cust));
        }

        public async Task<IActionResult> GetAllCustomersAsync()
        {
            List<Customer> customers = await _tableStorageService.GetAllCustomersAsync();
            return new OkObjectResult(JsonConvert.SerializeObject(customers));
        }

        public async Task<IActionResult> DeleteCustomerAsync(int id)
        {
            Customer? cust = await _tableStorageService.GetCustomerAsync(id);
            if (cust == null)
                return new BadRequestObjectResult($"Customer with CustomerID: {id} not found");
            if (string.IsNullOrEmpty(cust.RowKey))
                return new BadRequestObjectResult($"Customer with CustomerID: {id} has no rowkey");
            if (await _tableStorageService.CustomerHasPurchases(id))
                return new BadRequestObjectResult($"Customer with CustomerID: {id} has purchases. " +
                    $"Cannot delete customer with purchases.");
            await _tableStorageService.DeleteCustomerAsync(cust.RowKey);

            return new OkObjectResult($"Customer with CustomerID: {id} deleted");
        }

        public async Task<IActionResult> UpdateCustomerAsync(int id, string? name, string? email, string? phone, string? address)
        {
            Customer? cust = await _tableStorageService.GetCustomerAsync(id);
            if (cust == null)
                return new BadRequestObjectResult($"Customer with CustomerID: {id} not found");

            if (!string.IsNullOrEmpty(name)) cust.CustomerName = name;
            if (!string.IsNullOrEmpty(email)) cust.CustomerEmail = email;
            if (!string.IsNullOrEmpty(phone)) cust.PhoneNumber = phone;
            if (!string.IsNullOrEmpty(address)) cust.Address = address;

            await _tableStorageService.UpdateCustomerAsync(cust);

            return new OkObjectResult($"Customer with CustomerID: {id} updated");

        }
    }
}

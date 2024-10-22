using cldv_poe.Services;
using Microsoft.AspNetCore.Mvc;
using cldv_poe.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace cldv_poe.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableStorageService _tableStorageService;

        public CustomerController(TableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _tableStorageService.GetAllCustomersAsync();
            return View(customers);
        }

        [HttpGet]
        public IActionResult AddCustomer()
        {
            Console.WriteLine("Add Customer");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCustomer(Customer customer)
        {

            customer.RowKey = Guid.NewGuid().ToString();
            customer.CustomerID = _tableStorageService.GetNextCustomerID();
            if (ModelState.IsValid)
            {
                await _tableStorageService.AddCustomerAsync(customer);
                return RedirectToAction("Index");
            }
            return View(customer);
        }

        public async Task<IActionResult> DeleteCustomer(string rowKey)
        {
            if (!ModelState.IsValid)
                return View();
            await _tableStorageService.DeleteCustomerAsync(rowKey);
            return RedirectToAction("Index");
        }
    }
}

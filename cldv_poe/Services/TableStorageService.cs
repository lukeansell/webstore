using Azure;
using Azure.Data.Tables;
using cldv_poe.Models;
using Microsoft.Build.Construction;
using System.Collections.Concurrent;

namespace cldv_poe.Services
{
    public class TableStorageService(string connStr)
    {
        private readonly TableClient _customerTableClient = new(connStr, "Customer");
        private readonly TableClient _productTableClient = new(connStr, "Product");
        private readonly TableClient _purchaseTableClient = new(connStr, "Purchase");

        // -------------------- PRODUCT --------------------

        public Boolean HasProduct(int productID)
        {
            return GetProductAsync(productID) != null;
        }
        public int GetNextProductID()
        {
            var products = GetAllProductsAsync().Result;
            return products.Count == 0 ? 1 : products.Max(p => p.ProductID) + 1;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _productTableClient.QueryAsync<Product>())
                products.Add(product);
            return products;
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrEmpty(product.PartitionKey) || string.IsNullOrEmpty(product.RowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set");
            if (product.ProductID == 0)
                throw new ArgumentException("ProductID must be set");
            if (GetProductAsync(product.ProductID).Result != null)
                throw new ArgumentException("ProductID already exists");

            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to table storage", ex);
            }
        }

        public async Task DeleteProductAsync(String rowkey)
        {
            await DeleteProductAsync(Product.PKey, rowkey);

        }

        public async Task DeleteProductAsync(string partitionKey, string rowkey)
        {
            try
            {
                var response = await _productTableClient.DeleteEntityAsync(partitionKey, rowkey);
                if (response.IsError)
                    Console.WriteLine($"DeleteProductAsync: partitionKey: {partitionKey} rowKey: {rowkey}, Status: {response.Status}\n{response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public async Task<Product?> GetProductAsync(string rowKey) => await GetProductAsync(Product.PKey, rowKey);

        public async Task<Product?> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<Product>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<Product?> GetProductAsync(int productID)
        {
            await foreach (var product in _productTableClient.QueryAsync<Product>())
                if (product.ProductID == productID)
                    return product;
            return null;
        }
        public async Task UpdateProductAsync(Product product)
        {
            await _productTableClient.UpsertEntityAsync(product);
        }


        // -------------------- CUSTOMER --------------------
        public int GetNextCustomerID()
        {
            var customers = GetAllCustomersAsync().Result;
            return customers.Count == 0 ? 1 : customers.Max(c => c.CustomerID) + 1;
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.PartitionKey) || string.IsNullOrEmpty(customer.RowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set");
            if (customer.CustomerID == 0)
                throw new ArgumentException("CustomerID must be set");
            if (GetCustomerAsync(customer.CustomerID).Result != null)
                throw new ArgumentException("CustomerID already exists");

            try
            {
                await _customerTableClient.AddEntityAsync(customer);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to table storage", ex);
            }
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();
            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
                customers.Add(customer);
            return customers;
        }

        public async Task DeleteProduct(string rowkey)
        {
            await DeleteProductAsync(Product.PKey, rowkey);
        }

        public async Task DeleteCustomerAsync(string rowKey)
        {
            await DeleteCustomerAsync(Customer.PKey, rowKey);
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowkey)
        {
            await _customerTableClient.DeleteEntityAsync(partitionKey, rowkey);
        }

        public async Task<Customer?> GetCustomerAsync(string rowKey)
        {
            return await GetCustomerAsync(Customer.PKey, rowKey);
        }

        public async Task<Customer?> GetCustomerAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _customerTableClient.GetEntityAsync<Customer>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<Customer?> GetCustomerAsync(int customerID)
        {
            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
                if (customer.CustomerID == customerID)
                    return customer;
            return null;
        }

        public async Task<string?> GetCustomerRowKeyAsync(int customerID)
        {
            await foreach (var customer in _customerTableClient.QueryAsync<Customer>())
                if (customer.CustomerID == customerID)
                    return customer.RowKey;
            return null;
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _customerTableClient.UpsertEntityAsync(customer);
        }

        public async Task<int> GetCustomerPurchaseCountAsync(int customerID)
        {
            int count = 0;
            await foreach (var purchase in _purchaseTableClient.QueryAsync<Purchase>())
                if (purchase.CustomerID == customerID)
                    count++;
            return count;
        }

        public async Task<Boolean> CustomerHasPurchases(int customerID)
        {
            return await GetCustomerPurchaseCountAsync(customerID) > 0;
        }

        // -------------------- PURCHASE --------------------
        public int GetNextPurchaseID()
        {
            var purchases = GetAllPurchasesAsync().Result;
            return purchases.Count == 0 ? 1 : purchases.Max(p => p.PurchaseID) + 1;
        }
        public async Task<List<Purchase>> GetAllPurchasesAsync()
        {
            var purchases = new List<Purchase>();
            await foreach (var purchase in _purchaseTableClient.QueryAsync<Purchase>())
                purchases.Add(purchase);
            return purchases;
        }

        public async Task AddPurchaseAsync(Purchase purchase)
        {
            if (string.IsNullOrEmpty(purchase.PartitionKey) || string.IsNullOrEmpty(purchase.RowKey))
                throw new ArgumentException("PartitionKey and RowKey must be set");

            try
            {
                await _purchaseTableClient.AddEntityAsync(purchase);
            }
            catch (RequestFailedException ex)
            {
                throw new InvalidOperationException("Error adding entity to table storage", ex);
            }
        }

        public async Task DeletePurchaseAsync(string rowkey)
        {
            await DeletePurchaseAsync(Purchase.PKey, rowkey);
        }

        public async Task DeletePurchaseAsync(string partitionKey, string rowkey)
        {
            await _purchaseTableClient.DeleteEntityAsync(partitionKey, rowkey);
        }

        public async Task<Purchase?> GetPurchaseAsync(string rowKey)
        {
            return await GetPurchaseAsync(Purchase.PKey, rowKey);
        }

        public async Task<Purchase?> GetPurchaseAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _purchaseTableClient.GetEntityAsync<Purchase>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<Purchase?> GetPurchaseAsync(int purchaseID)
        {
            await foreach (var purchase in _purchaseTableClient.QueryAsync<Purchase>())
                if (purchase.PurchaseID == purchaseID)
                    return purchase;
            return null;
        }

        public async Task<List<Purchase>> GetPurchasesByCustomerAsync(int customerID)
        {
            var purchases = new List<Purchase>();
            await foreach (var purchase in _purchaseTableClient.QueryAsync<Purchase>())
                if (purchase.CustomerID == customerID)
                    purchases.Add(purchase);
            return purchases;
        }

        public async Task<List<Purchase>> GetPurchasesByProduct(int productID)
        {
            var purchases = new List<Purchase>();
            await foreach (var purchase in _purchaseTableClient.QueryAsync<Purchase>())
                if (purchase.ProductID == productID)
                    purchases.Add(purchase);
            return purchases;
        }
    }
}

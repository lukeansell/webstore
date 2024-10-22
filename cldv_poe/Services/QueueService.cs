using Azure.Storage.Queues;
using cldv_poe.Models;

namespace cldv_poe.Services
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
        }

        public async Task SendMessage(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        public async Task LogBlobUploadAsync(string uri)
        {
            await _queueClient.SendMessageAsync($"{DateTime.Now} Blob Upload: uri {uri}");
        }

        public async Task LogAddProductAsync(Product product)
        {
            await _queueClient.SendMessageAsync($"{DateTime.Now} Product added, ProductID: {product.ProductID}, ProductName: {product.ProductName}, Stock: {product.Stock}");
        }

        public async Task LogUpdateProductAsync(Product product)
        {
            await _queueClient.SendMessageAsync($"{DateTime.Now} Product updated, ProductID: {product.ProductID}, ProductName: {product.ProductName}, Stock: {product.Stock}");
        }

        public async Task LogProcessPurchaseAsync(Purchase purchase)
        {
            await _queueClient.SendMessageAsync($"{DateTime.Now} Purchase Proccessed PurchaseID: {purchase.PurchaseID}, CustomerID: {purchase.CustomerID}, ProductID: {purchase.ProductID}, Quantity: {purchase.Quantity}, Price: {purchase.TotalPrice} ");
        }

        public async Task LogUploadFileShare(string dirName, string fName)
        {
            await _queueClient.SendMessageAsync($"{DateTime.Now} File uploaded to {dirName} with name {fName}");
        }
    }
}

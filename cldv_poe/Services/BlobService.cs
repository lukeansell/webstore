using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
namespace cldv_poe.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "images";
        private readonly QueueService _queueService;

        public BlobService(string connString, QueueService queueService)
        {
            _blobServiceClient = new BlobServiceClient(connString);
            _queueService = queueService;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            try
            {
                await blobClient.UploadAsync(fileStream);
                await _queueService.LogBlobUploadAsync(blobClient.Uri.ToString());
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload the file.", ex);
            }
        }

        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1]; // end of the array
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            try
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete the blob.", ex);
            }
        }
    }
}

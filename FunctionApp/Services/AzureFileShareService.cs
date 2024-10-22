using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using FunctionApp.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using System.IO;

namespace FunctionApp.Services
{
    public class AzureFileShareService(string connstr, string fileShareName, QueueService queueService)
    {
        private readonly string _connStr = connstr ?? throw new ArgumentNullException(nameof(connstr));
        private readonly string _fileShareName = fileShareName ?? throw new ArgumentNullException(nameof(fileShareName));
        private readonly QueueService _queueService = queueService;

        public async Task UploadFileAsync(string dirName, string fName, Stream fStream)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connStr);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var dirClient = shareClient.GetDirectoryClient(dirName);
                await dirClient.CreateIfNotExistsAsync();

                var fileClient = dirClient.GetFileClient(fName);

                await fileClient.CreateAsync(fStream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, fStream.Length), fStream);
                await _queueService.LogUploadFileShare(dirName, fName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file ({fName}): {ex.Message}", ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string dirName, string fName)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connStr);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var dirClient = shareClient.GetDirectoryClient(dirName);
                await dirClient.CreateIfNotExistsAsync();

                var fileClient = dirClient.GetFileClient(fName);
                var downloadInfo = await fileClient.DownloadAsync();
                return downloadInfo.Value.Content;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file ({fName}): {ex.Message}", ex);
            }
        }

        public async Task<List<FileModel>> ListFilesAsync(string dirName)
        {
            var fileModels = new List<FileModel>();
            try
            {
                var serviceClient = new ShareServiceClient(_connStr);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var dirClient = shareClient.GetDirectoryClient(dirName);
                await foreach (ShareFileItem item in dirClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = dirClient.GetFileClient(item.Name);
                        var prop = await fileClient.GetPropertiesAsync();
                        fileModels.Add(new FileModel
                        {
                            Name = item.Name,
                            Size = prop.Value.ContentLength,
                            LastModified = prop.Value.LastModified,
                            Dir = dirName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing files: {ex.Message}", ex);
            }
            return fileModels;
        }


        public async Task<List<FileModel>> ListFilesAsync(List<string> dirNames)
        {
            var fileModels = new List<FileModel>();
            try
            {
                var serviceClient = new ShareServiceClient(_connStr);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                foreach (var dirName in dirNames)
                {
                    var dirClient = shareClient.GetDirectoryClient(dirName);
                    await foreach (ShareFileItem item in dirClient.GetFilesAndDirectoriesAsync())
                    {
                        if (!item.IsDirectory)
                        {
                            var fileClient = dirClient.GetFileClient(item.Name);
                            var prop = await fileClient.GetPropertiesAsync();
                            fileModels.Add(new FileModel
                            {
                                Name = item.Name,
                                Size = prop.Value.ContentLength,
                                LastModified = prop.Value.LastModified,
                                Dir = dirName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing files: {ex.Message}", ex);
            }
            return fileModels;
        }
    }
}

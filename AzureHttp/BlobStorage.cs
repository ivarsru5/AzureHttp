using System;
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace AzureHttp
{
	public class BlobStorage
	{
        private readonly ILogger _logger;
        private readonly CloudStorageAccount _account;
        private string _container = Environment.GetEnvironmentVariable("Container")!;
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public BlobStorage(ILogger logger)
		{
            _logger = logger;
            _account = CloudStorageAccount.Parse(_azureStorageValue);
		}

        public async Task<String> UploadPayload(string? responseBody)
        {
            var containerClient = new BlobContainerClient(_azureStorageValue, _container);

            var uinqID = Guid.NewGuid().ToString();

            try
            {
                var blobClient = containerClient.GetBlobClient(uinqID);
                var contentBytes = Encoding.UTF8.GetBytes(responseBody!);

                using (var stream = new MemoryStream(contentBytes))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }
            }
            catch (Exception error)
            {
                _logger.LogInformation("Error uploading body: {0}", error);
            }
            return uinqID;
        }
    }
}


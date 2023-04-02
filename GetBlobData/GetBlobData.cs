using System.Collections.Generic;
using System.IO;
using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;

namespace GetBlobData
{
    public class GetBlobData
    {
        private readonly ILogger _logger;
        private readonly CloudStorageAccount _account;
        private readonly BlobContainerClient _containerClient;
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public GetBlobData(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetBlobData>();
            _account = CloudStorageAccount.Parse(_azureStorageValue);
            _containerClient = new BlobContainerClient(_azureStorageValue, "payload");
        }

        [Function("GetBlobData")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function,
            "get", Route = "{rowKey}")]
            HttpRequestData req,
            string rowKey)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var blob = _containerClient.GetBlobClient(rowKey);
            if (await blob.ExistsAsync())
            {
                BlobDownloadInfo downloadInfo = await blob.DownloadAsync();
                using (var reader = new StreamReader(downloadInfo.Content))
                {
                    var data = await reader.ReadToEndAsync();
                    await response.WriteStringAsync(data);
                }
            }
            else
            {
                response.StatusCode = HttpStatusCode.NotFound;
            }

            return response;
        }
    }
}
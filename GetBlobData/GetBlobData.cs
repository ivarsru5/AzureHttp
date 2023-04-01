using System.Collections.Generic;
using System.IO;
using System.Net;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace GetBlobData
{
    public class GetBlobData
    {
        private readonly ILogger _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public GetBlobData(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
        {
            _logger = loggerFactory.CreateLogger<GetBlobData>();
            _blobServiceClient = blobServiceClient;
            _containerClient = new BlobContainerClient(_azureStorageValue ,"payload");
        }

        [Function("GetBlobData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var blobs = new List<string>();

            var options = new BlobRequestConditions
            {
                IfModifiedSince = DateTimeOffset.UtcNow.AddDays(-1)
            };

            await foreach (var blobItem in _containerClient.GetBlobsAsync(BlobTraits.Metadata)
                .WithCancellation(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token)
                .ConfigureAwait(false))
            {
                var blobClient = _containerClient.GetBlobClient(blobItem.Name);
                var responseStream = await blobClient.OpenReadAsync();
                using (var streamReader = new StreamReader(responseStream))
                {
                    var content = await streamReader.ReadToEndAsync();
                    blobs.Add(content);
                }
            }

            await response.WriteAsJsonAsync(blobs);

            return response;
        }
    }
}

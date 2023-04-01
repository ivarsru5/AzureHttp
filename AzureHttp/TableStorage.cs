using System;
using Azure;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace AzureHttp
{
	public class TableStorage
	{
		private readonly ILogger _logger;
		private readonly CloudStorageAccount _account;
        private readonly string _tableName = "resultlogs";
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public TableStorage(ILogger logger)
		{
            _logger = logger;
            _account = CloudStorageAccount.Parse(_azureStorageValue);
		}

        public async Task UploadResponse(HttpResponseMessage _response, string blobId)
        {
            CloudTableClient tableClient = _account.CreateCloudTableClient(new TableClientConfiguration());
            CloudTable table = tableClient.GetTableReference(_tableName);

            CustomTableEntity newEntity;

            if (_response.IsSuccessStatusCode)
            {
                newEntity = new CustomTableEntity("Success", blobId, ((int)_response.StatusCode));
            }
            else
            {
                newEntity = new CustomTableEntity("Failure", blobId, ((int)_response.StatusCode));
            }

            await InsertRow(newEntity, table);
        }

        public async Task InsertRow(CustomTableEntity log, CloudTable table)
        {
            TableOperation insert = TableOperation.InsertOrMerge(log);
            try
            {
                TableResult result = await table.ExecuteAsync(insert);
            }
            catch (Exception error)
            {
                _logger.LogInformation("Error inserting row: {0}", error);
            }
        }
    }
}


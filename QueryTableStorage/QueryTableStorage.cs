using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs.Extensions.Tables;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;

namespace QueryTableStorage
{
    public class QueryTableStorage
    {
        private readonly ILogger _logger;
        private readonly CloudStorageAccount _account;
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public QueryTableStorage(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<QueryTableStorage>();
            _account = CloudStorageAccount.Parse(_azureStorageValue);
        }

        [Function("QueryTableStorage")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/{fromDate}/{toDate}/{rowKey}")]
            HttpRequestData req,
            string fromDate,
            string toDate,
            string rowKey)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var fromDateTime = DateTime.ParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var toDateTime = DateTime.ParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var client = _account.CreateCloudTableClient();
            var table = client.GetTableReference("resultlogs");

            var filter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, fromDateTime),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, toDateTime)
                );

            var query = new TableQuery<TableQueryEntity>().Where(filter);
            var entities = await table.ExecuteQuerySegmentedAsync(query, null);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            if (entities.Any())
            {
                response.WriteString($"Entities found between {fromDate} and {toDate}:");
                foreach (var entity in entities)
                {
                    response.WriteString($" PartitionKey: {entity.PartitionKey}, RowKey: {entity.RowKey}, Timestamp: {entity.Timestamp}, Response code: {entity.Response}");
                }
            }
            else
            {
                response.WriteString($"No entities found between {fromDate} and {toDate}.");
            }

            return response;
        }
    }
}
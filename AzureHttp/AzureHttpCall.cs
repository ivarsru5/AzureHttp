using System;
using System.Reflection.Metadata;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;

namespace AzureHttp
{
    public class AzureHttpCall
    {
        private readonly ILogger _logger;
        private readonly CloudStorageAccount _account;
        private string _azureStorageValue = Environment.GetEnvironmentVariable("AzureStorage")!;

        public AzureHttpCall(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AzureHttpCall>();
            _account = CloudStorageAccount.Parse(_azureStorageValue);
        }

        [Function("AzureHttpCall")]
        public void Run([TimerTrigger("0 */1 * * * *")] MyInfo myTimer)
        {
            var request = new HttpRequest(_logger);
            request.MakeRequestAsync().Wait();

            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus!.Next}");

        }
    }

    public class MyInfo
    {
        public MyScheduleStatus? ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        private DateTime _next;

        public DateTime Last { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value.AddMinutes(1);
            }
        }
    }
}
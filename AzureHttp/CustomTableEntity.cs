using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureHttp
{
	public class CustomTableEntity: TableEntity
	{
        public int Response { get; set; }

        public CustomTableEntity(string result, int response)
		{
			PartitionKey = result;
			RowKey = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
			Response = response;
        }
	}
}


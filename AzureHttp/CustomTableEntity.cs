using System;
using Microsoft.Azure.Cosmos.Table;

namespace AzureHttp
{
	public class CustomTableEntity: TableEntity
	{
        public int Response { get; set; }

        public CustomTableEntity(string result, string blobId, int response)
		{
			PartitionKey = result;
			RowKey = blobId;
			Response = response;
        }
	}
}


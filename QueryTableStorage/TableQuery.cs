using System;
using Microsoft.Azure.Cosmos.Table;

namespace QueryTableStorage
{
	public class TableQueryEntity : TableEntity
	{
		public int Response { get; set; }

		public TableQueryEntity()
		{
			this.PartitionKey = PartitionKey;
			this.RowKey = RowKey;
			this.Response = Response;
		}
	}
}


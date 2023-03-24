using System;
using Microsoft.Extensions.Logging;

namespace AzureHttp
{
	public class HttpRequest
	{
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private BlobStorage _storage;
        private TableStorage _table;

        public HttpRequest(ILogger logger)
		{
            _httpClient = new HttpClient();
            _logger = logger;
            _storage = new BlobStorage(_logger);
            _table = new TableStorage(_logger);
        }

        public async Task MakeRequestAsync()
        {
            var response = await _httpClient.GetAsync("https://api.publicapis.org/random?auth=null");
            await _table.UploadResponse(response);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"API response content: {body}");
                 await _storage.UploadPayload(body);
            }
            else
            {
                _logger.LogError($"API request failed with status code: {response.StatusCode}");
            }
        }
	}
}


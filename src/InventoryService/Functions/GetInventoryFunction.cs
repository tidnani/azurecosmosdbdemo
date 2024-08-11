using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using InventoryService.Repositories;
using System.Net;
using System.Threading.Tasks;

namespace InventoryService.Functions
{
    public class GetInventoryFunction
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ILogger _logger;

        public GetInventoryFunction(InventoryRepository inventoryRepository, ILoggerFactory loggerFactory)
        {
            _inventoryRepository = inventoryRepository;
            _logger = loggerFactory.CreateLogger<GetInventoryFunction>();
        }

        [Function("GetInventory")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "inventory/{productId}")] HttpRequestData req,
            string productId)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request to get inventory for product {productId}.");

            var inventoryItem = await _inventoryRepository.GetInventoryAsync(productId);

            if (inventoryItem == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Inventory for product {productId} not found.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(inventoryItem);

            return response;
        }
    }
}
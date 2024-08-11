using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using InventoryService.Models;
using InventoryService.Repositories;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace InventoryService.Functions
{
    public class UpdateInventoryFunction
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ILogger _logger;

        public UpdateInventoryFunction(InventoryRepository inventoryRepository, ILoggerFactory loggerFactory)
        {
            _inventoryRepository = inventoryRepository;
            _logger = loggerFactory.CreateLogger<UpdateInventoryFunction>();
        }

        [Function("UpdateInventory")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to update inventory.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var inventoryUpdate = JsonConvert.DeserializeObject<InventoryUpdate>(requestBody);

            if (inventoryUpdate == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid inventory update data");
                return badRequestResponse;
            }

            var updatedInventory = await _inventoryRepository.UpdateInventoryAsync(inventoryUpdate.ProductId, inventoryUpdate.QuantityChange);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updatedInventory);

            return response;
        }
    }

    public class InventoryUpdate
    {
        public string ProductId { get; set; }
        public int QuantityChange { get; set; }
    }
}
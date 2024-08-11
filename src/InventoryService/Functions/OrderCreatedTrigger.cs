using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using InventoryService.Repositories;
using Common.Models;

namespace InventoryService.Functions
{
    public class OrderCreatedTrigger
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly ILogger _logger;

        public OrderCreatedTrigger(InventoryRepository inventoryRepository, ILoggerFactory loggerFactory)
        {
            _inventoryRepository = inventoryRepository;
            _logger = loggerFactory.CreateLogger<OrderCreatedTrigger>();
        }

        [Function("OrderCreatedTrigger")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "MicroservicesDB",
            containerName: "OrdersContainer",
            Connection = "CosmosDBConnectionString",
            LeaseContainerName = "leases",
            CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Order> input)
        {
            foreach (var order in input)
            {
                _logger.LogInformation($"Processing order: {order.Id}");
                foreach (var item in order.Items)
                {
                    await _inventoryRepository.UpdateInventoryAsync(item.ProductId, -item.Quantity);
                    _logger.LogInformation($"Updated inventory for product {item.ProductId}");
                }
            }
        }
    }
}
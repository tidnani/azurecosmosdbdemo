using Microsoft.Azure.Cosmos;
using InventoryService.Models;
using System;
using System.Threading.Tasks;
using System.IO;

namespace InventoryService.Repositories
{
    public class InventoryRepository
{
    private readonly Container _container;

    public InventoryRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<InventoryItem> UpdateInventoryAsync(string productId, int quantityChange)
    {
        try
        {
            var response = await _container.ReadItemAsync<InventoryItem>(productId, new PartitionKey(productId));
            var inventoryItem = response.Resource;

            inventoryItem.Quantity += quantityChange;
            inventoryItem.LastUpdated = DateTime.UtcNow;

            response = await _container.ReplaceItemAsync(inventoryItem, productId, new PartitionKey(productId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Item not found, create a new one
            var newItem = new InventoryItem
            {
                Id = productId,
                ProductId = productId,
                Quantity = quantityChange > 0 ? quantityChange : 0,
                LastUpdated = DateTime.UtcNow
            };
            var createResponse = await _container.CreateItemAsync(newItem, new PartitionKey(productId));
            return createResponse.Resource;
        }
    }

    public async Task<InventoryItem> GetInventoryAsync(string productId)
    {
        try
        {
            var response = await _container.ReadItemAsync<InventoryItem>(productId, new PartitionKey(productId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null; // Or throw a custom exception
        }
    }
}
}
using Microsoft.Azure.Cosmos;
using OrderService.Models;
using System;
using System.Threading.Tasks;

namespace OrderService.Repositories
{
    public class OrderRepository
    {
        private readonly Container _container;

        public OrderRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            try
            {
                order.Id = Guid.NewGuid().ToString();
                order.CreatedAt = DateTime.UtcNow;
                order.LastUpdated = DateTime.UtcNow;

                order.Status = "Pending";

                var response = await _container.CreateItemAsync(order, new PartitionKey(order.CustomerId));
                
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                Console.WriteLine($"Conflict detected for order {order.Id}. Handling conflict...");
                return await HandleConflictAsync(order);
            }
        }

        private async Task<Order> HandleConflictAsync(Order order)
        {
            // Implement your conflict resolution strategy here
            // For this example, we'll use a simple "last writer wins" approach
            var existingOrder = await _container.ReadItemAsync<Order>(order.Id, new PartitionKey(order.Id));

            if (order.LastUpdated > existingOrder.Resource.LastUpdated)
            {
                var response = await _container.ReplaceItemAsync(order, order.Id, new PartitionKey(order.Id));
                return response.Resource;
            }
            else
            {
                return existingOrder.Resource;
            }
        }


        public async Task<Order> GetOrderAsync(string orderId, string customerId)
        {
            var response = await _container.ReadItemAsync<Order>(orderId,
            new PartitionKey(customerId),
            new ItemRequestOptions
            {
                ConsistencyLevel = ConsistencyLevel.Eventual
            });
            return response.Resource;
        }
    }
}
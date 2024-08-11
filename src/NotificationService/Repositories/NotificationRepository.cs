using Microsoft.Azure.Cosmos;
using NotificationService.Models;
using System;
using System.Threading.Tasks;

namespace NotificationService.Repositories
{
    public class NotificationRepository
    {
        private readonly Container _container;

        public NotificationRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedAt = DateTime.UtcNow;
            notification.Status = "Pending";
            //notification.TimeToLive = 7 * 24 * 60 * 60; // 7 days in seconds
            notification.TimeToLive = 60; // 7 days in seconds

            var response = await _container.CreateItemAsync(notification, new PartitionKey(notification.RecipientId));
            return response.Resource;
        }

        public async Task<Notification> GetNotificationAsync(string notificationId, string recipientId)
        {
            try
            {
                ItemResponse<Notification> response = await _container.ReadItemAsync<Notification>(
                    notificationId, new PartitionKey(recipientId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;//could be due to TTL
            }
        }

        public async Task<Notification> UpdateNotificationStatusAsync(string notificationId, string recipientId, string newStatus)
        {
            try
            {
                ItemResponse<Notification> response = await _container.ReadItemAsync<Notification>(
                    notificationId, new PartitionKey(recipientId));
                var notification = response.Resource;
                
                notification.Status = newStatus;
                
                response = await _container.ReplaceItemAsync(notification, notificationId, new PartitionKey(recipientId));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NotificationService.Models;
using NotificationService.Repositories;
using System.Net;
using System.Threading.Tasks;
using System.IO;


namespace NotificationService.Functions
{
    public class SendNotificationFunction
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly ILogger _logger;

        public SendNotificationFunction(NotificationRepository notificationRepository, ILoggerFactory loggerFactory)
        {
            _notificationRepository = notificationRepository;
            _logger = loggerFactory.CreateLogger<SendNotificationFunction>();
        }

        [Function("SendNotification")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to send a notification.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var notification = JsonConvert.DeserializeObject<Notification>(requestBody);

            if (notification == null || string.IsNullOrEmpty(notification.RecipientId) || string.IsNullOrEmpty(notification.Message))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid notification data. RecipientId and Message are required.");
                return badRequestResponse;
            }

            var createdNotification = await _notificationRepository.CreateNotificationAsync(notification);

            // Here you would typically add logic to actually send the notification (e.g., via email, SMS, push notification)
            // For this example, we'll just log it
            _logger.LogInformation($"Notification sent to {createdNotification.RecipientId}: {createdNotification.Message}");

            // Update the notification status to "Sent"
            var updatedNotification = await _notificationRepository.UpdateNotificationStatusAsync(createdNotification.Id, createdNotification.RecipientId, "Sent");

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(updatedNotification);

            return response;
        }
    }
}
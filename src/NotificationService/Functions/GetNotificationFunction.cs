// NotificationService/Functions/GetNotificationFunction.cs
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NotificationService.Repositories;
using System.Net;
using System.Threading.Tasks;

namespace NotificationService.Functions
{
    public class GetNotificationFunction
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly ILogger _logger;

        public GetNotificationFunction(NotificationRepository notificationRepository, ILoggerFactory loggerFactory)
        {
            _notificationRepository = notificationRepository;
            _logger = loggerFactory.CreateLogger<GetNotificationFunction>();
        }

        [Function("GetNotification")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "notifications/{notificationId}/{recipientId}")] HttpRequestData req,
            string notificationId,
            string recipientId)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request to get notification {notificationId}.");

            var notification = await _notificationRepository.GetNotificationAsync(notificationId, recipientId);

            if (notification == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Notification with ID {notificationId} not found for recipient {recipientId}.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(notification);

            return response;
        }
    }
}
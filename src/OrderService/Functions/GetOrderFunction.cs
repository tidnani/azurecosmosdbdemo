using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderService.Repositories;
using System.Net;
using System.Threading.Tasks;

namespace OrderService.Functions
{
    public class GetOrderFunction
    {
        private readonly OrderRepository _orderRepository;
        private readonly ILogger _logger;

        public GetOrderFunction(OrderRepository orderRepository, ILoggerFactory loggerFactory)
        {
            _orderRepository = orderRepository;
            _logger = loggerFactory.CreateLogger<GetOrderFunction>();
        }

        [Function("GetOrder")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{orderId}/{customerId}")] HttpRequestData req,
            string orderId,
            string customerId)
        {
            _logger.LogInformation($"C# HTTP trigger function processed a request to get order {orderId}.");

            var order = await _orderRepository.GetOrderAsync(orderId, customerId);

            if (order == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Order with ID {orderId} not found.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(order);

            return response;
        }
    }
}
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderService.Models;
using OrderService.Repositories;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OrderService.Functions
{
    public class CreateOrderFunction
    {
        private readonly OrderRepository _orderRepository;
        private readonly ILogger _logger;

        public CreateOrderFunction(OrderRepository orderRepository, ILoggerFactory loggerFactory)
        {
            _orderRepository = orderRepository;
            _logger = loggerFactory.CreateLogger<CreateOrderFunction>();
        }

        [Function("CreateOrder")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to create an order.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);

            if (order == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid order data");
                return badRequestResponse;
            }

            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(createdOrder);

            return response;
        }
    }
}
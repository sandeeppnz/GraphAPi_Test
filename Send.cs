using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSLServiceBus
{
    public static class Send
    {
        const string _serviceBusConnectionString = "";
        const string _queueName = "ssl-events-queue";
        static IQueueClient _queueClient;

        [FunctionName("Send")]
        public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            QueueAsync(requestBody).GetAwaiter().GetResult();

            var responseMessage = string.IsNullOrEmpty(requestBody)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"{requestBody}";

            return new OkObjectResult(responseMessage);
        }


        private static async Task QueueAsync(string data)
        {
            _queueClient = new QueueClient(_serviceBusConnectionString, _queueName);
            await SendMessage(data);
            await _queueClient.CloseAsync();
        }

        private static async Task SendMessage(string data)
        {
            var busMessage = new Message(Encoding.UTF8.GetBytes(data));
            await _queueClient.SendAsync(busMessage);
        }


    }
}

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using System.Text;

namespace SSLServiceBus
{
    public static class SendToTopic
    {
        static ITopicClient _topicClient;
        const string _serviceBusConnectionString = "";
        const string _topicName = "ssl-events-topic";
        const string _subscriptionName = "ssl-events-subscription";


        [FunctionName("SendToTopic")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            TopicAsync(requestBody).GetAwaiter().GetResult();

            var responseMessage = string.IsNullOrEmpty(requestBody)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"{requestBody}";

            return new OkObjectResult(responseMessage);
        }

        private static async Task TopicAsync(string data)
        {
            _topicClient = new TopicClient(_serviceBusConnectionString, _topicName);
            await SendMessage(data);
            await _topicClient.CloseAsync();

        }
        private static async Task SendMessage(string data)
        {
            var busMessage = new Message(Encoding.UTF8.GetBytes(data));
            await _topicClient.SendAsync(busMessage);
        }


    }
}

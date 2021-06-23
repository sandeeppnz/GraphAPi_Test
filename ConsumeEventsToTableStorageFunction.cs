using ConsumeEventsToTableStorageFunction.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ConsumeEventsToTableStorageFunction
{
    public static class ConsumeEventsToTableStorageFunction
    {
        [FunctionName("ConsumeEventsToTableStorageFunction")]
        public static void Run([ServiceBusTrigger("ssl-events-queue", Connection = "connectionString")]string myQueueItem, string messageId, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            RunInsert(myQueueItem, messageId).Wait();

        }

        private static async Task RunInsert(string message, string messageId)
        {
            var table = await Common.CreateTableAsync("Events");

            dynamic data = JsonConvert.DeserializeObject(message);

            await InsertOrMergeEntityAsync(table,
                new EventEntity
                {
                    PartitionKey = $"{data.custCode}-{data.instanceName}",
                    RowKey = messageId,
                    EventJson = message
                });
        }


        public static async Task<EventEntity> InsertOrMergeEntityAsync(CloudTable table, EventEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var insertedCustomer = result.Result as EventEntity;

                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

    }
}

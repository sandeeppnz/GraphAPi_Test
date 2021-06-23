using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

namespace TestGraphAPI
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                GetUsersAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        public static async Task GetUsersAsync()
        {
            string clientId = "";
            string tenantId = "";
            string clientSecret = "";
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantId)
                .WithClientSecret(clientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);
            var graphClient = new GraphServiceClient(authProvider);

            var groups = await graphClient.Groups.Request().Select(x => new { x.Id, x.DisplayName }).GetAsync();
            foreach (var group in groups)
            {
                Console.WriteLine($"{group.DisplayName}, {group.Id}");
            }
        }
    }

}




using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace TestGraphAPIEmail
{
    internal class Program
    {
        //private static void Main(string[] args)
        //{


        //    var t = SendEmailUsingGraphAPI();
        //    t.Wait();
        //}


        private static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in password)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }

        private static async Task<bool> SendEmailUsingGraphAPI()
        {

            // AUTHENTICATION

            var clientID = "";
            var tenantID = "";
            string clientSecret = "";

            var scopes = "user.read mail.send";  // DELEGATE permissions that the request will need

            var authority = $"https://login.microsoftonline.com/{tenantID}";
            var scopesArr = new string[] { scopes };

            try
            {
                var app = PublicClientApplicationBuilder
                        .Create(clientID)
                        .WithAuthority(authority)
                        .Build();

                var accounts = await app.GetAccountsAsync();

                AuthenticationResult result = null;
                if (accounts.Any())
                {
                    result = await app.AcquireTokenSilent(scopesArr, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                else
                {
                    // you could acquire a token by username/password authentication if you aren't federated.
                    result = await app
                        //.AcquireTokenByIntegratedWindowsAuth(scopesArr)
                        .AcquireTokenByUsernamePassword(scopesArr,"", ConvertToSecureString(""))
                        .ExecuteAsync(CancellationToken.None);
                }

                Console.WriteLine(result.Account.Username);
                var message = "{'message': {'subject': 'Hello from Microsoft Graph API', 'body': {'contentType': 'Text', 'content': 'Hello, World!'}, 'toRecipients': [{'emailAddress': {'address': '" + result.Account.Username + "'} } ]}}";

                var restClient = new RestClient("https://graph.microsoft.com/v1.0/users/" + result.Account.Username + "/sendMail");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");

                request.AddHeader("Authorization", "Bearer " + result.AccessToken);
                request.AddParameter("", message, ParameterType.RequestBody);
                var response = restClient.Execute(request);
                Console.WriteLine(response.Content);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            return true;
        }


        static async Task Main(string[] args)
        {
            var client_Id = "";
            var tenantId = "";
            string client_Secret = "";


            var scopes = new[] { "https://graph.microsoft.com/.default" };

            // Configure app builder
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var app = ConfidentialClientApplicationBuilder
                .Create(client_Id)
                .WithClientSecret(client_Secret)
                .WithAuthority(new Uri(authority))
                .Build();

            // Acquire tokens for Graph API
            var authenticationResult = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            // Create GraphClient and attach auth header to all request (acquired on previous step)
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(requestMessage =>
                {
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("bearer", authenticationResult.AccessToken);

                    return Task.FromResult(0);
                }));

            // Call Graph API
            var user = await graphClient.Users[""].Messages.Request().GetAsync();


            var message = new Message
            {
                Subject = "hello from graph explorer",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = "Welcome to graph."
                },
                ToRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = ""
                        }
                    }
                }
            };

            await graphClient.Users[""]
              .SendMail(message, null)
              .Request()
              .PostAsync();


        }


        public static async Task<GraphServiceClient> SendEmail(string clientId, string tenantID, string clientSecret)
        {

            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithTenantId(tenantID)
                .WithClientSecret(clientSecret)
                .Build();

            var authProvider = new ClientCredentialProvider(confidentialClientApplication);

            var graphClient = new GraphServiceClient(authProvider);

            var message = new Message
            {
                Subject = "hello from graph explorer",
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = "Welcome to graph."
                },
                ToRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = ""
                        }
                    }
                }
            };

            await graphClient.Me
              .SendMail(message, null)
              .Request()
              .PostAsync();

            return graphClient;

        }
    }
}

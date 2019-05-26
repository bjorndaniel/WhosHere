using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WhosHere.Common
{
    public static class GraphConnector
    {

        public static async Task<string> AquireTokenAsync(ConfigValues values, Func<DeviceCodeResult, Task> deviceCodeResultCallback)
        {
            AuthenticationResult result = null;
            var app = GetGraphClientApp(values.ClientID, values.Authority);
            var accounts = await app.GetAccountsAsync();
            // All AcquireToken* methods store the tokens in the cache, so check the cache firsty
            try
            {
                result = await app.AcquireTokenSilent(values.AppScopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilent. 
                // This indicates you need to call AcquireTokenInteractive to acquire a token
                System.Diagnostics.Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");
            }
            try
            {
                result = await app.AcquireTokenWithDeviceCode(values.AppScopes, deviceCodeResultCallback).ExecuteAsync();
                Console.WriteLine(result.Account.Username);
                return result.AccessToken;
            }
            catch (MsalServiceException)
            {
                // Kind of errors you could have (in ex.Message)

                // AADSTS50059: No tenant-identifying information found in either the request or implied by any provided credentials.
                // Mitigation: as explained in the message from Azure AD, the authoriy needs to be tenanted. you have probably created
                // your public client application with the following authorities:
                // https://login.microsoftonline.com/common or https://login.microsoftonline.com/organizations

                // AADSTS90133: Device Code flow is not supported under /common or /consumers endpoint.
                // Mitigation: as explained in the message from Azure AD, the authority needs to be tenanted

                // AADSTS90002: Tenant <tenantId or domain you used in the authority> not found. This may happen if there are 
                // no active subscriptions for the tenant. Check with your subscription administrator.
                // Mitigation: if you have an active subscription for the tenant this might be that you have a typo in the 
                // tenantId (GUID) or tenant domain name.
            }
            catch (OperationCanceledException)
            {
                // If you use a CancellationToken, and call the Cancel() method on it, then this may be triggered
                // to indicate that the operation was cancelled. 
                // See https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads 
                // for more detailed information on how C# supports cancellation in managed threads.
            }
            catch (MsalClientException)
            {
                // Verification code expired before contacting the server
                // This exception will occur if the user does not manage to sign-in before a time out (15 mins) and the
                // call to `AcquireTokenWithDeviceCode` is not cancelled in between
            }
            return null;
        }

        public static async Task<byte[]> GetUserImageAsync(string id, string graphToken)
        {
            try
            {
                byte[] bytes = null;
                using (var stream = await GetGSC(graphToken).Users[id].Photo.Content.Request().GetAsync())
                {
                    if (stream?.Length > 0)
                    {
                        bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                    }
                }
                return bytes;
            }
            catch (ServiceException)
            {
                return null;
            }

        }

        public static IPublicClientApplication GetGraphClientApp(string clientID, string authority) =>
            PublicClientApplicationBuilder
                 .Create(clientID)
                 .WithAuthority(authority)
                 .Build();

        public static async Task<IEnumerable<IGraphServiceUsersCollectionPage>> GetUsersAsync(string graphToken)
        {
            var users = await GetGSC(graphToken).Users.Request().GetAsync();
            var retVal = new List<IGraphServiceUsersCollectionPage>
            {
                users
            };
            while (users.NextPageRequest != null)
            {
                users = await users.NextPageRequest.GetAsync();
                retVal.Add(users);
            }
            return retVal;
        }

        private static GraphServiceClient GetGSC(string graphToken) =>
            new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", graphToken);
                return Task.FromResult(0);
            }));

    }
}


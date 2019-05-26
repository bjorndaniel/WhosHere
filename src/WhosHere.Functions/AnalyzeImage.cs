using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhosHere.Common;

namespace WhosHere.Functions
{
    public static class AnalyzeImage
    {
        public static IConfigurationRoot Configuration { get; private set; }

        [FunctionName("AnalyzeImage")]
        [
            return: SignalR(ConnectionStringSetting = "AzureSignalRConnectionString", HubName = "analyzis")
        ]
        public static async Task<SignalRMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            log.LogInformation("C# HTTP trigger function processed a request to analyze image.");
            var ms = new MemoryStream();
            await req.Body.CopyToAsync(ms);
            var imageBytes = ms.ToArray();
            var identified = await FaceConnector.AnalyzeImageAsync(imageBytes, GetConfig(log, context));
            identified.ToList().ForEach(_ =>
            {
                log.LogInformation(_.UserData);
            });
            dynamic data = new ExpandoObject();
            var message = new SignalRMessage
            {
                Target = "ImageAnalyzed"
            };
            var content = new AnalyzeResult
            {
                NrFound = identified.Count(),
                Users = identified.Select(_ => _.UserData).ToList()
            };
            data.result = content;
            message.Arguments = new object[] { data };
            return message;
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [SignalRConnectionInfo(HubName = "analyzis")] SignalRConnectionInfo connectionInfo) =>
            connectionInfo;

        [FunctionName("Employees")]
        public static IEnumerable<string> GetEmployees([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log, ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request for employees.");
            return StorageConnector.GetEmployees(GetConfig(log, context));
        }
        private static ConfigValues GetConfig(ILogger log, ExecutionContext context)
        {
            if (Configuration == null)
            {
                log.LogInformation("Reading config and secrets");
                Configuration = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddUserSecrets<Temp>()
                    .Build();
            }
            log.LogInformation("Returning values");
            return Configuration.GetSection(nameof(ConfigValues)).Get<ConfigValues>();
        }
    }

    public class Temp { }
}


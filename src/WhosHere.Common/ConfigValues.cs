using System.Collections.Generic;
namespace WhosHere.Common
{
    public class ConfigValues
    {
        public string ClientID { get; set; }
        public string FaceApiKey { get; set; }
        public string ReturnUri { get; set; }
        public string AppSecret { get; set; }
        public IEnumerable<string> AppScopes { get; set; }
        public string Tenant { get; set; }
        public string GraphEndpoint { get; set; }
        public string StorageAccountKey { get; set; }
        public string StorageAccountUrl { get; set; }
        public string StorageAccountName { get; set; }
        public string Authority => $"https://login.microsoftonline.com/{Tenant}";
        public string AzureSignalRConnectionString { get; set; }
        public string SignalRUrl { get; set; }
        public string StorageConnectionString { get; set; }
        public string ContainerName { get; set; }
    }
}
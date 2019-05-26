using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WhosHere.Common
{
    public static class StorageConnector
    {
        public static async Task<bool> AddUserToStorageAsync(WHUser user, ConfigValues values)
        {
            var credentials = new StorageCredentials(values.StorageAccountName, values.StorageAccountKey);
            var blob = new CloudBlockBlob(new Uri($"{values.StorageAccountUrl}{user.Mail}"), credentials);
            await blob.UploadFromByteArrayAsync(user.Image, 0, user.Image.Length);
            return true;
        }

        public static IEnumerable<string> GetEmployees(ConfigValues values)
        {
            var storageAccount = CloudStorageAccount.Parse(values.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(values.ContainerName);
            foreach (var c in container.ListBlobs(null, false))
            {
                if (c.GetType() == typeof(CloudBlockBlob))
                {
                    var blob = (CloudBlockBlob)c;
                    yield return blob.Name;
                }
            }
        }
    }
}

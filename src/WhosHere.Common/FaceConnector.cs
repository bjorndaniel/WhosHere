using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WhosHere.Common
{
    public static class FaceConnector
    {
        private const int CallLimitPerSecond = 10;
        private static Queue<DateTime> TimeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        private const string PersonGroupId = "employeegroup";
        private const string FaceUrl = "https://westeurope.api.cognitive.microsoft.com";

        public static async Task<bool> AddUserToFaceApiAsync(WHUser user, ConfigValues values)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(values.FaceApiKey), new DelegatingHandler[] { })
            {
                Endpoint = FaceUrl
            };
            await WaitCallLimitPerSecondAsync();
            using (var stream = new MemoryStream(user.Image))
            {
                try
                {
                    var existingList = await faceClient.PersonGroup.GetAsync(PersonGroupId);
                }
                catch (APIErrorException)
                {
                    try
                    {
                        await faceClient.PersonGroup.CreateAsync(PersonGroupId, "WhosHereFaces", "Faces from graph", "recognition_02");
                    }
                    catch (Exception) { }
                }
                try
                {
                    var person = await faceClient.PersonGroupPerson.CreateAsync(PersonGroupId, user.Mail, user.Mail);
                    await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(PersonGroupId, person.PersonId, stream, user.Mail);
                }
                catch (APIErrorException)
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> TrainModelAsync(ConfigValues values)
        {
            try
            {
                var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(values.FaceApiKey), new DelegatingHandler[] { })
                {
                    Endpoint = FaceUrl
                };
                await faceClient.PersonGroup.TrainAsync(PersonGroupId);
                return true;
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public static async Task<IEnumerable<Person>> AnalyzeImageAsync(byte[] image, ConfigValues values)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(values.FaceApiKey), new DelegatingHandler[] { })
            {
                Endpoint = FaceUrl
            };
            var idResult = new List<Guid>();
            var retVal = new List<Person>();
            using (var stream = new MemoryStream(image))
            {
                var result = await faceClient.Face.DetectWithStreamAsync(stream, true, false, null, "recognition_02", true);
                var foundIds = result.Select(_ => _.FaceId.Value);

                foreach (var chunk in foundIds.Chunk(10))
                {
                    try
                    {
                        var identified = await faceClient.Face.IdentifyAsync(chunk.ToList(), PersonGroupId);
                        idResult.AddRange(identified.Where(i => i.Candidates.Any()).Select(_ => _.Candidates.First().PersonId));
                    }
                    catch (APIErrorException e)
                    {
                        Console.WriteLine(e.Body.Error.Message);
                        Console.WriteLine(e.ToString());
                    }

                }
            }
            foreach (var id in idResult)
            {
                var person = await faceClient.PersonGroupPerson.GetAsync(PersonGroupId, id);
                if (person != null)
                {
                    retVal.Add(person);
                }
            }
            return retVal;
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        private static async Task WaitCallLimitPerSecondAsync()
        {
            Monitor.Enter(TimeStampQueue);
            try
            {
                if (TimeStampQueue.Count >= CallLimitPerSecond)
                {
                    var timeInterval = DateTime.UtcNow - TimeStampQueue.Peek();
                    if (timeInterval < TimeSpan.FromSeconds(1))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1) - timeInterval);
                    }
                    TimeStampQueue.Dequeue();
                }
                TimeStampQueue.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                Monitor.Exit(TimeStampQueue);
            }
        }
    }
}

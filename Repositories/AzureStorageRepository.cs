using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Bot.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamsAIssistant.Services;

public class AzureStorageRepository : IStorage
{
    private readonly BlobContainerClient _containerClient;

    private readonly JsonSerializer _jsonSerializer;

    public AzureStorageRepository(string connectionString, string containerName)
    {
        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists();

        _jsonSerializer = new JsonSerializer
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Error,
            MaxDepth = null
        };
    }

    public async Task<IDictionary<string, object>> ReadAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        var items = new Dictionary<string, object>();
        foreach (var key in keys)
        {
            var blobClient = _containerClient.GetBlobClient(key);

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                var response = await blobClient.DownloadAsync(cancellationToken);
                await using var stream = response.Value.Content;
                using var streamReader = new StreamReader(stream);
                await using var jsonReader = new JsonTextReader(streamReader);
                var jObject = await JObject.LoadAsync(jsonReader, cancellationToken);
                var result = jObject.ToObject<object>(_jsonSerializer);

                if (result != null)
                {
                    items[key] = result;
                }
            }
        }

        return items;
    }


/*
    public async Task<IDictionary<string, object>> ReadAsync2(string[] keys, CancellationToken cancellationToken = default)
    {
        var items = new Dictionary<string, object>();
        foreach (var key in keys)
        {
            var blobClient = _containerClient.GetBlobClient(key);

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                var response = await blobClient.DownloadAsync(cancellationToken);

                using var streamReader = new StreamReader(response.Value.Content);
                using var jsonReader = new JsonTextReader(streamReader);
                var jObject = await JObject.LoadAsync(jsonReader, cancellationToken);
                var result = jObject.ToObject<object>(_jsonSerializer);

                if (result != null)
                {
                    items[key] = result;
                }
            }
        }

        return items;
    }*/

    public async Task WriteAsync(IDictionary<string, object> changes, CancellationToken cancellationToken = default)
    {
        foreach (var change in changes)
        {
            if (change.Value == null) continue;

            var blobClient = _containerClient.GetBlobClient(change.Key);
            var jObject = JObject.FromObject(change.Value, _jsonSerializer);

            await using var stream = new MemoryStream();
            using var streamWriter = new StreamWriter(stream);
            await using var jsonWriter = new JsonTextWriter(streamWriter);
            await jObject.WriteToAsync(jsonWriter, cancellationToken);
            await jsonWriter.FlushAsync(cancellationToken);
            stream.Position = 0;
            await blobClient.UploadAsync(stream, overwrite: true, cancellationToken: cancellationToken);
        }
    }

    public async Task DeleteAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        foreach (var key in keys)
        {
            var blobClient = _containerClient.GetBlobClient(key);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
    }
}

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Multicloud.Blob.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Multicloud.Blob.Azure
{
    [BlobProvider(Provider = Providers.AzureBlobStorage)]
    internal class AzureBlobStorageAdapter : IBlobClient
    {
        private const string ConnectionStringKey = "ConnectionString";

        private readonly BlobServiceClient _blobServiceClient;

        private readonly ILogger<AzureBlobStorageAdapter> _logger = new NullLogger<AzureBlobStorageAdapter>();

        public AzureBlobStorageAdapter(IReadOnlyDictionary<string, string> options, StorageBlobOptions storageBlobOptions, ILoggerFactory loggerFactory = null)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (storageBlobOptions == null)
            {
                throw new ArgumentNullException(nameof(storageBlobOptions));
            }

            if (!options.TryGetValue(ConnectionStringKey, out var connectionString))
            {
                throw new ArgumentException($"{ConnectionStringKey} is required.");
            }

            _blobServiceClient = new BlobServiceClient(connectionString);

            if (storageBlobOptions.EnableLogging && loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<AzureBlobStorageAdapter>();
            }
        }

        public async Task<Stream> DownloadAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            var blobClient = GetBlobClient(containerId, blobId);
            var stream = new MemoryStream();
            var result = await blobClient.DownloadToAsync(stream, cancellationToken).ConfigureAwait(false);

            return stream;
        }

        public async Task UploadAsync(string containerId, string blobId, Stream content, string contentType = null, CancellationToken cancellationToken = default)
        {
            var blobClient = GetBlobClient(containerId, blobId);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            var result = await blobClient.UploadAsync(content, new BlobUploadOptions { HttpHeaders = blobHttpHeaders }, cancellationToken).ConfigureAwait(false);
        }

        public Task<IAsyncEnumerable<string>> ListAsync(string containerId, CancellationToken cancellationToken = default)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerId);
            var pages = blobContainerClient.GetBlobsAsync(cancellationToken: cancellationToken);

            return Task.FromResult(ListAsync(pages, cancellationToken));
        }

        public async Task DeleteAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            var blobClient = GetBlobClient(containerId, blobId);
            var result = await blobClient.DeleteAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private BlobClient GetBlobClient(string containerId, string blobId)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerId);

            return blobContainerClient.GetBlobClient(blobId);
        }

        private async IAsyncEnumerable<string> ListAsync(AsyncPageable<BlobItem> pages, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var item in pages.ConfigureAwait(false).WithCancellation(cancellationToken))
            {
                yield return item.Name;
            }
        }
    }
}

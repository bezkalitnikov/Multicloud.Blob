using Multicloud.Blob.Abstractions;
using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace Multicloud.Blob.Google
{
    [BlobProvider(Provider = Providers.GoogleCloudStorage)]
    internal class GoogleCloudStorageAdapter : IBlobClient
    {
        private readonly ILogger<GoogleCloudStorageAdapter> _logger = new NullLogger<GoogleCloudStorageAdapter>();

        public GoogleCloudStorageAdapter(StorageBlobOptions storageBlobOptions, ILoggerFactory loggerFactory = null)
        {
            if (storageBlobOptions == null)
            {
                throw new ArgumentNullException(nameof(storageBlobOptions));
            }

            if (storageBlobOptions.EnableLogging && loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<GoogleCloudStorageAdapter>();
            }
        }

        public async Task<Stream> DownloadAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            var storageClient = await GetStorageClientAsync().ConfigureAwait(false);
            var stream = new MemoryStream();
            await storageClient.DownloadObjectAsync(containerId, blobId, stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            return stream;
        }

        public async Task UploadAsync(string containerId, string blobId, Stream content, string contentType = null, CancellationToken cancellationToken = default)
        {
            var storageClient = await GetStorageClientAsync().ConfigureAwait(false);
            var result = await storageClient.UploadObjectAsync(containerId, blobId, contentType ?? "application/octet-stream", content, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task<IAsyncEnumerable<string>> ListAsync(string containerId, CancellationToken cancellationToken = default)
        {
            const string delimiter = "/";
            var storageClient = await GetStorageClientAsync().ConfigureAwait(false);
            var pages = storageClient.ListObjectsAsync(containerId,
                options: new ListObjectsOptions { Delimiter = delimiter });

            return ListAsync(pages, cancellationToken);
        }

        public async Task DeleteAsync(string containerId, string blobId, CancellationToken cancellationToken = default)
        {
            var storageClient = await GetStorageClientAsync().ConfigureAwait(false);
            await storageClient.DeleteObjectAsync(containerId, blobId, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<StorageClient> GetStorageClientAsync()
        {
            return await StorageClient.CreateAsync().ConfigureAwait(false);
        }

        private async IAsyncEnumerable<string> ListAsync(PagedAsyncEnumerable<Objects, Object> pages, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var @object in pages.ConfigureAwait(false).WithCancellation(cancellationToken))
            {
                yield return @object.Name;
            }
        }
    }
}

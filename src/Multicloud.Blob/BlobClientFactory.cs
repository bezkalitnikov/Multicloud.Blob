using Multicloud.Blob.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Multicloud.Blob
{
    public class BlobClientFactory : IBlobClientFactory
    {
        private readonly Func<BlobProviderOptions, IBlobClient> _blobClientFactory;

        private readonly ILogger<BlobClientFactory> _logger = new NullLogger<BlobClientFactory>();

        public BlobClientFactory(Func<BlobProviderOptions, IBlobClient> blobClientFactory, StorageBlobOptions storageBlobOptions, ILoggerFactory loggerFactory)
        {
            _blobClientFactory = blobClientFactory ?? throw new ArgumentNullException(nameof(blobClientFactory));

            if (storageBlobOptions == null)
            {
                throw new ArgumentNullException(nameof(storageBlobOptions));
            }

            if (storageBlobOptions.EnableLogging && loggerFactory != null)
            {
                _logger = loggerFactory.CreateLogger<BlobClientFactory>();
            }
        }

        public IBlobClient Create(BlobProviderOptions options)
        {
            return _blobClientFactory(options);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Multicloud.Blob.Abstractions
{
    public interface IBlobClient
    {
        Task<Stream> DownloadAsync(string containerId, string blobId, CancellationToken cancellationToken = default);

        Task UploadAsync(string containerId, string blobId, Stream content, string contentType = default, CancellationToken cancellationToken = default);

        Task<IAsyncEnumerable<string>> ListAsync(string containerId, CancellationToken cancellationToken = default);

        Task DeleteAsync(string containerId, string blobId, CancellationToken cancellationToken = default);
    }
}

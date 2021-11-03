using System.Collections.Generic;

namespace Multicloud.Blob.Abstractions
{
    public class BlobProviderOptions
    {
        public string Provider { get; set; }

        public IReadOnlyDictionary<string, string> Options { get; set; }
    }
}

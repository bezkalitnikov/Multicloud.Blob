using System;

namespace Multicloud.Blob
{
    [AttributeUsage(AttributeTargets.Class)]
    internal class BlobProviderAttribute : Attribute
    {
        public string Provider { get; set; }
    }
}

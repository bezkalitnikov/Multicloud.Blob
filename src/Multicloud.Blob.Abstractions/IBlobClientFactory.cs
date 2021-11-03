namespace Multicloud.Blob.Abstractions
{
    public interface IBlobClientFactory
    {
        IBlobClient Create(BlobProviderOptions options);
    }
}

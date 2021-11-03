using Multicloud.Blob.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Multicloud.Blob
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCloudStorageBlob(this IServiceCollection serviceCollection, Action<StorageBlobOptions> configure = null)
        {
            return serviceCollection
                .AddSingleton(sp =>
                {
                    var opt = new StorageBlobOptions();

                    try
                    {
                        configure?.Invoke(opt);
                    }
                    catch
                    {
                        // Just swallow, options can't be initialized
                        // use default settings
                        // due to the caller's handler throws exception
                    }

                    return opt;
                })
                .AddSingleton<IBlobClientFactory, BlobClientFactory>()
                .AddSingleton(sp =>
                {
                    var blobClientTypes = Assembly
                        .GetExecutingAssembly()
                        .GetTypes()
                        .Where(x => x.IsClass
                                    && !x.IsAbstract
                                    && typeof(IBlobClient).IsAssignableFrom(x)
                                    && x.GetCustomAttributes<BlobProviderAttribute>().Any())
                        .ToDictionary(
                            x => x.GetCustomAttributes<BlobProviderAttribute>().First().Provider,
                            x => x);

                    return new Func<BlobProviderOptions, IBlobClient>(blobProviderOptions =>
                    {
                        if (blobProviderOptions == null)
                        {
                            throw new ArgumentNullException(nameof(blobProviderOptions));
                        }

                        if (!blobClientTypes.TryGetValue(blobProviderOptions.Provider, out var tableClientType))
                        {
                            throw new ArgumentException(
                                $"There is no blob client type connected to provider: {blobProviderOptions.Provider}. Check provider name.");
                        }

                        if (blobProviderOptions.Options != null)
                        {
                            return (IBlobClient)ActivatorUtilities.CreateInstance(sp, tableClientType,
                                blobProviderOptions.Options);
                        }

                        return (IBlobClient)ActivatorUtilities.CreateInstance(sp, tableClientType);
                    });
                });
        }
    }
}

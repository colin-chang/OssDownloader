using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ColinChang.OssDownloader;

public static class OssDownloaderExtensions
{
    public static IServiceCollection AddOssDownloader(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));


        services.AddOptions<OssDownloaderOptions>()
            .Configure(configuration.Bind)
            .ValidateDataAnnotations();
        services.AddSingleton<IOptionsChangeTokenSource<OssDownloaderOptions>>(
            new ConfigurationChangeTokenSource<OssDownloaderOptions>(configuration));
        services.AddSingleton<IDownloader, Downloader>();
        return services;
    }
}
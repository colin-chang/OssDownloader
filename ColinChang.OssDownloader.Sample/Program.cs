using ColinChang.OssDownloader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build()
    .GetSection(nameof(OssDownloaderOptions));
var downloader = new ServiceCollection()
    .AddOssDownloader(configuration)
    .BuildServiceProvider()
    .GetRequiredService<IDownloader>();

// downloader.Download("src.txt");
downloader.DownloadBucket("dev/apps/", skipPrefixObject: true);

Console.ReadKey();
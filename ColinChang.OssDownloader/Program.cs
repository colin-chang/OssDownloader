using System.Collections.Concurrent;
using ColinChang.OssHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


//1. 初始化下载队列
if (!File.Exists("src.txt"))
{
    Console.WriteLine("src.txt does not exist");
    return;
}

var lines = File.ReadLines("src.txt");
if (!lines.Any())
{
    Console.WriteLine("there is no file to download");
    return;
}

var queue = new ConcurrentQueue<string>(lines);


//2. 初始化OSS
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build()
    .GetSection(nameof(OssHelperOptions));
var option = configuration.Get<OssHelperOptions>().PolicyOptions;
if (string.IsNullOrWhiteSpace(option.AccessKeyId)
    || string.IsNullOrWhiteSpace(option.AccessKeySecret)
    || string.IsNullOrWhiteSpace(option.EndPoint)
    || string.IsNullOrWhiteSpace(option.BucketName))
{
    Console.WriteLine("invalid oss configuration...");
    return;
}

var oss = new ServiceCollection()
    .AddOssHelper(configuration)
    .BuildServiceProvider()
    .GetRequiredService<IOssHelper>();


//3. 设置 并发线程数/下载保存路径
if (!int.TryParse(Environment.GetEnvironmentVariable("MAX_TASK"), out var maxTask) || maxTask <= 0)
    maxTask = 5;
var savePath = Environment.GetEnvironmentVariable("SAVE_PATH") ??
               Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "download");
if (!Directory.Exists(savePath))
    Directory.CreateDirectory(savePath);


//4. 启动并行下载
Console.WriteLine("download started ...");
var locker = new object();
var downloaded = 0;
for (var i = 0; i < maxTask; i++)
{
    new Thread(() =>
    {
        while (!queue.IsEmpty)
        {
            if (!queue.TryDequeue(out var file))
                continue;

            var filename = Path.GetFileName(file);
            Console.WriteLine($"{filename} downloading");
            var dest = Path.Combine(savePath, filename);
            if (!File.Exists(dest))
                oss.DownloadAsync(file, dest).Wait();

            Console.WriteLine($"{filename} downloaded");
            lock (locker)
            {
                if (++downloaded >= lines.Count())
                    Console.WriteLine("download finished");
            }
        }
    }).Start();
}
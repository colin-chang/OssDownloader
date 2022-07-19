using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ColinChang.OssHelper;
using Microsoft.Extensions.Options;

namespace ColinChang.OssDownloader;

public class Downloader : IDownloader
{
    private readonly OssDownloaderOptions _options;

    /// <summary>
    /// 下载队列
    /// </summary>
    private ConcurrentQueue<string> _queue;


    /// <summary>
    /// OssHelper
    /// </summary>
    private readonly IOssHelper _oss;

    public event EventHandler Started;
    public event EventHandler<DownloadEventArgs> Downloading;
    public event EventHandler<DownloadEventArgs> Downloaded;
    public event EventHandler<DownloadFailedArgs> Failed;
    public event EventHandler Stopped;

    public Downloader(IOptionsMonitor<OssDownloaderOptions> options) :
        this(options.CurrentValue)
    {
    }

    public Downloader(OssDownloaderOptions options)
    {
        _options = options;
        _oss ??= new OssHelper.OssHelper(new OssHelperOptions { PolicyOptions = _options.Policy });

        //设置 下载保存路径
        if (!Directory.Exists(_options.SavePath))
            Directory.CreateDirectory(_options.SavePath);
    }

    public void Download(IEnumerable<string> objects)
    {
        _queue = new ConcurrentQueue<string>(objects);
        Download();
    }

    public void Download(string sourceFile)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException();

        var lines = File.ReadLines(sourceFile);
        if (!lines.Any())
            return;

        _queue = new ConcurrentQueue<string>(lines);
        Download();
    }


    public void DownloadBucket(string prefix = null,
        string marker = null,
        string delimiter = null, bool skipPrefixObject = false)
    {
        var locker = new object();
        var done = false;
        var ts = new List<Thread>();
        for (var i = 0; i < _options.MaxTask; i++)
        {
            var thread = new Thread(() =>
            {
                while (!done)
                {
                    string key;
                    lock (locker)
                    {
                        if (done)
                            break;

                        var obj = _oss.ListObjectsAsync(prefix, marker, 1).Result;
                        if (!obj.IsTruncated)
                            done = true;

                        if (!obj.ObjectSummaries.Any())
                            continue;

                        marker = obj.NextMarker;
                        key = obj.ObjectSummaries.FirstOrDefault().Key;
                    }

                    if (skipPrefixObject && string.Equals(key, prefix))
                        continue;

                    var filename = Path.GetFileName(key);
                    Console.WriteLine($"{filename} downloading");
                    var dest = Path.Combine(_options.SavePath, filename);
                    if (!File.Exists(dest))
                    {
                        try
                        {
                            _oss.DownloadAsync(key, dest).Wait();
                            Console.WriteLine($"{filename} downloaded");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"failed to download {filename}. {e.Message}");
                        }
                    }
                    else
                        Console.WriteLine($"{filename} downloaded");
                }
            }) { IsBackground = true };
            ts.Add(thread);
            thread.Start();
        }

        foreach (var t in ts)
            t.Join();
        Stopped?.Invoke(this, EventArgs.Empty);
        Console.WriteLine("all done");
    }

    private void Download()
    {
        Started?.Invoke(this, EventArgs.Empty);
        Console.WriteLine("download started ...");
        var locker = new object();
        var targets = _queue.Count;

        var downloaded = 0;
        for (var i = 0; i < _options.MaxTask; i++)
        {
            new Thread(() =>
            {
                while (!_queue.IsEmpty)
                {
                    if (!_queue.TryDequeue(out var file))
                        continue;

                    var filename = Path.GetFileName(file);
                    Downloading?.Invoke(this, new DownloadEventArgs(file));
                    Console.WriteLine($"{filename} downloading");

                    var dest = Path.Combine(_options.SavePath, filename);
                    if (!File.Exists(dest))
                    {
                        if (_oss.ListObjectsAsync(file).Result.ObjectSummaries.Any())
                        {
                            try
                            {
                                _oss.DownloadAsync(file, dest).Wait();
                                Downloaded?.Invoke(this, new DownloadEventArgs(file));
                                Console.WriteLine($"{filename} downloaded");
                            }
                            catch (Exception e)
                            {
                                var msg = $"failed to download {filename}. {e.Message}";
                                Failed?.Invoke(this, new DownloadFailedArgs(file, msg, e));
                                Console.WriteLine(msg);
                            }
                        }
                        else
                        {
                            var msg = $"{filename} does not exist";
                            Failed?.Invoke(this,
                                new DownloadFailedArgs(file, msg, new FileNotFoundException(msg, file)));
                            Console.WriteLine(msg);
                        }
                    }
                    else
                    {
                        Downloaded?.Invoke(this, new DownloadEventArgs(file));
                        Console.WriteLine($"{filename} downloaded");
                    }


                    lock (locker)
                    {
                        if (++downloaded < targets)
                            continue;
                        Console.WriteLine("download finished");
                        Stopped?.Invoke(this, EventArgs.Empty);
                    }
                }
            }).Start();
        }
    }
}
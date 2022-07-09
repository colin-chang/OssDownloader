using System;
using System.Collections.Generic;

namespace ColinChang.OssDownloader;

public interface IDownloader
{
    void Download(IEnumerable<string> objects);

    void Download(string sourceFile);


    event EventHandler Started;
    event EventHandler<DownloadEventArgs> Downloading;
    event EventHandler<DownloadEventArgs> Downloaded;
    event EventHandler<DownloadFailedArgs> Failed;
    event EventHandler Stopped;


    // void DownloadBucket(string prefix = null,
    //     string marker = null,
    //     int? maxKeys = null,
    //     string delimiter = null);
}

public class DownloadEventArgs : EventArgs
{
    public string ObjectName { get; }

    public DownloadEventArgs(string objectName) => ObjectName = objectName;
}

public class DownloadFailedArgs : DownloadEventArgs
{
    public string Message { get; }
    public Exception Exception { get; }

    public DownloadFailedArgs(string objectName, string message, Exception exception) : base(objectName)
    {
        Message = message;
        Exception = exception;
    }
}
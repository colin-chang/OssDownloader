using System;
using System.Collections.Generic;

namespace ColinChang.OssDownloader;

public interface IDownloader
{
    void Download(IEnumerable<string> objects);

    void Download(string sourceFile);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="prefix">限定返回文件的Key必须以prefix作为前缀。如果把prefix设为某个文件夹名，则列举以此prefix开头的文件，即该文件夹下递归的所有文件和子文件夹</param>
    /// <param name="marker">指定List操作需要从此文件开始</param>
    /// <param name="delimiter">对Object名字进行分组的字符。所有Object名字包含指定的前缀，第一次出现delimiter字符之间的Object作为一组元素</param>
    /// <param name="skipPrefixObject">跳过下载 Prefix 同名 Object，当 prefix 为目录时可以跳过目录对象下载</param>
    void DownloadBucket(string prefix = null,
        string marker = null,
        string delimiter = null,
        bool skipPrefixObject = false);


    event EventHandler Started;
    event EventHandler<DownloadEventArgs> Downloading;
    event EventHandler<DownloadEventArgs> Downloaded;
    event EventHandler<DownloadFailedArgs> Failed;
    event EventHandler Stopped;
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
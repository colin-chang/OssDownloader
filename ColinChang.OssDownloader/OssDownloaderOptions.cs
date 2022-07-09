using ColinChang.OssHelper;
using System.ComponentModel.DataAnnotations;

namespace ColinChang.OssDownloader;

public class OssDownloaderOptions
{
    public PolicyOptions Policy { get; set; }

    [Range(1, 10, ErrorMessage = "max task must be between 0 and 15")]
    public int MaxTask { get; set; }

    [Required(ErrorMessage = "save path string is required")]
    public string SavePath { get; set; }
}
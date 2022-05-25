using FFMpegCore;
using FFMpegCore.Enums;

namespace FFvideoConverter
{
    /// <summary>
    /// 复制源文件参数的类型
    /// </summary>
    public enum FFCopyType { Audio, Video, Both, None }

    /// <summary>
    /// 转码配置文件
    /// </summary>
    public struct FFConvertConfig
    {
        public string InputFile;
        public string OutputPath;
        public string OutType;
        public FFCopyType CopyType;
    }

    public class FFmpegHelper
    {
        #region 成员变量
        /// <summary>
        /// 转码进度
        /// </summary>
        public double ConvertProgress { get; set; }

        /// <summary>
        /// 转码进度响应事件
        /// </summary>
        public event Action<double> OnConvertProgress;
        #endregion

        /// <summary>
        /// 实例化一个FFmpeg转码器
        /// </summary>
        public FFmpegHelper()
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = "./FFmpeg", TemporaryFilesFolder = "/tmp" });
        }

        /// <summary>
        /// 开始转码
        /// </summary>
        /// <param name="config">转码所需配置</param>
        /// <returns>成功返回true</returns>
        public bool Convert(FFConvertConfig config)
        {
            if (config.InputFile == string.Empty || config.OutputPath == string.Empty) return false;
            string outFilename = $"{Path.GetFileNameWithoutExtension(config.InputFile)}.{config.OutType}";
            string outputFile = Path.Combine(config.OutputPath, outFilename);
            IMediaAnalysis mediaInfo = FFProbe.Analyse(config.InputFile);
            return FFMpegArguments
            .FromFileInput(config.InputFile)
            .OutputToFile(outputFile, true
            , options => {
                if(config.CopyType != FFCopyType.None) options.CopyChannel((Channel)config.CopyType);
            })
            .NotifyOnProgress((double progress) => {
                ConvertProgress = progress;
                OnConvertProgress?.Invoke(progress);
            }, mediaInfo.Duration)
            .ProcessSynchronously();
        }

        /// <summary>
        /// 异步转码
        /// </summary>
        /// <param name="config">转码所需配置</param>
        public void ConvertAsync(FFConvertConfig config) => Task.Run(() => Convert(config));
    }
}

/*************************************************
 * 参考文档：https://ffmpeg.xabe.net/docs.html
 * ***********************************************/
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Xabe.FFmpeg;

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
        public FFVideoConfig VideoConfig;
        public FFAudioConfig AudioConfig;
    }

    public struct FFVideoConfig
    {
        public string CodecName;    // 编码
        public double FrameRate;    // 帧率

    }

    public struct FFAudioConfig
    {
        public string CodecName;    // 编码
        public int BitRate;         // 比特率
        public int SampleRateHz;    // 采样率
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
        private CancellationTokenSource CancelToken;
        #endregion

        /// <summary>
        /// 实例化一个FFmpeg转码器
        /// </summary>
        public FFmpegHelper()
        {
            FFmpeg.SetExecutablesPath("./FFmpeg");
        }

        /// <summary>
        /// 获取媒体信息
        /// </summary>
        /// <param name="file">媒体路径</param>
        /// <returns>[0]是视频信息，[1]是音频信息</returns>
        public string[] GetMediaInfo(string file)
        {
            string videoInfo = Probe.New().Start($"-hide_banner -pretty -show_streams -select_streams v {file}").Result;
            string audioInfo = Probe.New().Start($"-hide_banner -pretty -show_streams -select_streams a {file}").Result;
            string subtitleInfo = Probe.New().Start($"-hide_banner -pretty -show_streams -select_streams s {file}").Result;
            videoInfo = videoInfo.Replace("[STREAM]\r\n", "").Replace("[/STREAM]", "");
            audioInfo = audioInfo.Replace("[STREAM]\r\n", "").Replace("[/STREAM]", "");
            subtitleInfo = subtitleInfo.Replace("[STREAM]\r\n", "").Replace("[/STREAM]", "");
            return new string[] { videoInfo, audioInfo, subtitleInfo };
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
            try
            {
                IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(config.InputFile).Result;
                IVideoStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
                ISubtitleStream subtitleStream = mediaInfo.SubtitleStreams.FirstOrDefault();
                if (config.CopyType == FFCopyType.Both)
                {
                    videoStream = videoStream.CopyStream();
                    audioStream = audioStream.CopyStream();
                }
                else
                {                   
                    if (config.CopyType == FFCopyType.Video || config.CopyType == FFCopyType.None)
                    {
                        if (config.CopyType != FFCopyType.None) videoStream = videoStream.CopyStream();
                        audioStream.SetCodec(AudioCodec.aac);
                        //audioStream.SetBitrate(12000);
                        //audioStream.SetChannels(1);
                        //audioStream.SetSeek(TimeSpan.FromSeconds(5));
                    }
                    if (config.CopyType == FFCopyType.Audio || config.CopyType == FFCopyType.None)
                    {
                        if (config.CopyType != FFCopyType.None) audioStream = audioStream.CopyStream();
                        videoStream.SetCodec(VideoCodec.h264);
                        //videoStream.SetBitrate(12000);
                        //videoStream.SetSize(VideoSize.Hd480);
                    }
                }
                IConversion conversion = FFmpeg.Conversions.New();
                conversion.AddStream(audioStream, (IStream)videoStream);               
                conversion.SetOutput(outputFile);
                conversion.SetOverwriteOutput(true); // 覆盖输出文件
                //conversion.UseMultiThread(16);  // 多线程，默认应该是cpu的核心数
                conversion.AddParameter("-hide_banner");    // 禁止显示版权声明，构建选项和库版本等信息
                conversion.OnProgress += (sender, args) =>
                {
                    ConvertProgress = args.Percent;
                    OnConvertProgress?.Invoke(ConvertProgress);
                };
                conversion.OnDataReceived += (sender, args) =>
                {
                    // 输出转码信息
                    Debug.WriteLine(args.Data);
                };
                CancelToken = new CancellationTokenSource();
                IConversionResult ret = conversion.Start(CancelToken.Token).Result;
                Debug.WriteLine($"转码完成，使用参数：{ret.Arguments}");
                return true;
                //bool conversionResult = FFmpeg.Conversions.New().Start("args").IsFaulted; // 直接使用自定义参数
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 异步转码
        /// </summary>
        /// <param name="config">转码所需配置</param>
        public void ConvertAsync(FFConvertConfig config) => Task.Run(() => Convert(config));

        /// <summary>
        /// 取消转码
        /// </summary>
        public void CancelConvert()
        {
            CancelToken?.Cancel();
            CancelToken?.Dispose();
            CancelToken = null;
        }
    }
}

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
        /// <returns>媒体信息</returns>
        public FFConvertConfig GetMediaInfo(string file)
        {
            IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(file).Result;
            IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();

            FFVideoConfig videoInfo = new FFVideoConfig
            {
                CodecName = videoStream.Codec.ToUpper(),
            };
            FFAudioConfig audioInfo = new FFAudioConfig
            {
                CodecName = audioStream.Codec.ToUpper(),
            };
            return new FFConvertConfig
            {
                VideoConfig = videoInfo,
                AudioConfig = audioInfo,
            };
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
                if(config.CopyType != FFCopyType.Both)
                {
                    if(config.CopyType == FFCopyType.Video)
                    {
                        audioStream.SetCodec(AudioCodec.aac);
                    }
                    if (config.CopyType == FFCopyType.Audio)
                    {
                        videoStream.SetCodec(VideoCodec.h264);
                    }
                }
                else
                {
                    videoStream = videoStream.CopyStream();
                    audioStream = audioStream.CopyStream();
                }
                IConversion conversion = FFmpeg.Conversions.New();
                conversion.AddStream(audioStream, (IStream)videoStream);               
                conversion.SetOutput(outputFile);
                conversion.SetOverwriteOutput(true);
                //conversion.UseMultiThread(16);  // 多线程，默认应该是cpu的核心数
                conversion.OnProgress += (sender, args) =>
                {
                    ConvertProgress = args.Percent;
                    OnConvertProgress?.Invoke(ConvertProgress);
                };
                IConversionResult ret = conversion.Start().Result;
                return true;
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
    }
}

/*************************************************
 * 参考文档：https://ffmpeg.xabe.net/docs.html
 * ***********************************************/
using FFvideoConverter.Model;
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
            string videoInfo = Probe.New().Start($"-hide_banner -pretty -of json -show_streams -select_streams v {file}").Result;
            string audioInfo = Probe.New().Start($"-hide_banner -pretty -of json -show_streams -select_streams a {file}").Result;
            string subtitleInfo = Probe.New().Start($"-hide_banner -pretty -of json -show_streams -select_streams s {file}").Result;
            var vjson = (JObject)JObject.Parse(videoInfo)["streams"][0];
            var ajson = (JArray)JObject.Parse(audioInfo)["streams"];
            var sjson = (JArray)JObject.Parse(subtitleInfo)["streams"];
            var videoInfoStorage = vjson.ToObject<MediaInfoStorage.VideoInfoStorage>();
            var audioInfoStorages = ajson.ToObject<MediaInfoStorage.AudioInfoStorage[]>();
            var subtitleInfoStorages = sjson.ToObject<MediaInfoStorage.SubtitleInfoStorage[]>();
            // 视频标签信息补充
            if (videoInfoStorage.tags != null)
            {
                if (videoInfoStorage.duration == DateTime.MinValue)
                {
                    var durationNode = videoInfoStorage.tags.GetValueLike("DURATION");
                    if (durationNode != null) videoInfoStorage.duration = durationNode.ToObject<DateTime>();
                }
                if (videoInfoStorage.bit_rate == null)
                {
                    var bit_rate = videoInfoStorage.tags.GetValueLike("BPS");
                    if (bit_rate != null) videoInfoStorage.bit_rate = $"{int.Parse(bit_rate.ToString()) / 1000} kbps";
                }
            }
            // 音频标签信息补充
            foreach (var audioInfoStorage in audioInfoStorages)
            {
                if (audioInfoStorage.tags != null)
                {
                    if (audioInfoStorage.duration == DateTime.MinValue)
                    {
                        var durationNode = audioInfoStorage.tags.GetValueLike("DURATION");
                        if (durationNode != null) audioInfoStorage.duration = durationNode.ToObject<DateTime>();
                    }
                    if (audioInfoStorage.bit_rate == null)
                    {
                        var bit_rate = audioInfoStorage.tags.GetValueLike("BPS");
                        if (bit_rate != null) audioInfoStorage.bit_rate = $"{int.Parse(bit_rate.ToString()) / 1000} kbps";
                    }
                }
            }

            return MediaInfoObjectTostring(new MediaInfoStorage 
            { 
                VideoInfo = videoInfoStorage,
                AudioInfo = audioInfoStorages,
                SubtitleInfo = subtitleInfoStorages,
            });
        }

        /// <summary>
        /// 媒体信息对象转字符串描述
        /// </summary>
        /// <param name="mediaInfo"媒体信息对象></param>
        /// <returns>字符串描述数组</returns>
        public string[] MediaInfoObjectTostring(MediaInfoStorage mediaInfo)
        {
            string[] frame_rate = mediaInfo.VideoInfo.r_frame_rate.Split('/');
            string videoInfo =
                $"标题：{mediaInfo.VideoInfo.tags?["title"]}\n" +
                $"编码：{mediaInfo.VideoInfo.codec_name}\n" +
                $"编码详情：{mediaInfo.VideoInfo.codec_long_name}\n" +
                $"视频质量：{mediaInfo.VideoInfo.profile}\n" +
                $"分辨率：{mediaInfo.VideoInfo.coded_width} × {mediaInfo.VideoInfo.coded_height}\n" + 
                $"色彩格式：{mediaInfo.VideoInfo.pix_fmt}\n" +
                $"颜色空间：{mediaInfo.VideoInfo.color_space}\n" +
                $"视频时长：{mediaInfo.VideoInfo.duration:T}\n" +
                $"比特率：{mediaInfo.VideoInfo.bit_rate}\n" +
                $"帧率：{(double.Parse(frame_rate[0]) / double.Parse(frame_rate[1])):f2} fps\n" +
                $"\n";

            string audioInfo = "";
            foreach(var audio in mediaInfo.AudioInfo)
            {
                audioInfo += $"音频序号：{audio.index}\n";
                audioInfo += $"标题：{audio.tags?["title"]}\n";
                audioInfo += $"编码：{audio.codec_name}\n";
                audioInfo += $"编码详情：{audio.codec_long_name}\n";
                audioInfo += $"质量：{audio.profile}\n";
                audioInfo += $"采样率：{audio.sample_rate}\n";
                audioInfo += $"通道数：{audio.channels}\n";
                audioInfo += $"通道布局：{audio.channel_layout}\n";
                audioInfo += $"音频时长：{audio.duration:T}\n";
                audioInfo += $"比特率：{audio.bit_rate}\n";
                audioInfo += $"\n";
            }
            string subtitleInfo = "";
            foreach(var subtitle in mediaInfo.SubtitleInfo)
            {
                subtitleInfo += $"字幕序号：{subtitle.index}\n";
                subtitleInfo += $"编码：{subtitle.codec_name}\n";
                subtitleInfo += $"编码详情：{subtitle.codec_long_name}\n";
                subtitleInfo += $"语言：{subtitle.tags?["language"]}\n";
                subtitleInfo += $"标题：{subtitle.tags?["title"]}\n";
                subtitleInfo += $"文件大小：{subtitle.tags?.GetValueLike("NUMBER_OF_BYTES")} byte\n";
                subtitleInfo += $"\n";
            }

            return new[] {videoInfo, audioInfo, subtitleInfo };
        }

        /// <summary>
        /// 开始转码
        /// </summary>
        /// <param name="config">转码所需配置</param>
        /// <returns>成功返回true</returns>
        public bool Convert(FFConvertConfig config)
        {
            if (config.InputFile == string.Empty || config.OutputPath == string.Empty) return false;
            string outFilename = $"{Path.GetFileNameWithoutExtension(config.InputFile)}.{config.OutType.ToLower()}";
            string outputFile = Path.Combine(config.OutputPath, outFilename);
            try
            {
                IMediaInfo mediaInfo = FFmpeg.GetMediaInfo(config.InputFile).Result;
                IVideoStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
                ISubtitleStream subtitleStream = mediaInfo.SubtitleStreams.FirstOrDefault();
                IConversion conversion = FFmpeg.Conversions.New();

                if (config.CopyType == FFCopyType.Both)
                {
                    videoStream = videoStream.CopyStream();
                    audioStream = audioStream.CopyStream();
                }
                else
                {
                    if (config.CopyType == FFCopyType.Audio || config.CopyType == FFCopyType.None)
                    {
                        if (config.CopyType != FFCopyType.None) audioStream = audioStream.CopyStream();
                        // 设置视频编码
                        switch(config.VideoConfig.CodecName)
                        {
                            case "H.265":
                                videoStream.SetCodec(VideoCodec.hevc);
                                break;
                            case "H.264":
                                videoStream.SetCodec(VideoCodec.h264);
                                break;
                            case "MPGE-1":
                                videoStream.SetCodec(VideoCodec.mpeg1video);
                                break;
                            case "MPGE-2":
                                videoStream.SetCodec(VideoCodec.mpeg2video);
                                break;
                        }
                        // 设置帧率（fps）
                        //videoStream.SetFramerate(60);
                        //// 设置比特率
                        //videoStream.SetBitrate(12000);
                        //// 设置视频质量
                        //videoStream.SetSize(VideoSize.Hd480); //videoStream.SetSize(1920,1080);
                        //// 设置倍速
                        //videoStream.ChangeSpeed(1.25);

                        // 设置输出帧数
                        //videoStream.SetOutputFramesCount(60);
                        // 添加字幕
                        //videoStream.AddSubtitles("path");
                        // 视频截取
                        //videoStream.Split(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
                    }
                    if (config.CopyType == FFCopyType.Video || config.CopyType == FFCopyType.None)
                    {
                        if (config.CopyType != FFCopyType.None) videoStream = videoStream.CopyStream();
                        // 设置音频编码
                        //audioStream.SetCodec(AudioCodec.aac);
                        // 设置比特率
                        //audioStream.SetBitrate(12000);
                        // 设置采样率(Hz)
                        //audioStream.SetSampleRate(25);
                        // 设置音频通道
                        //audioStream.SetChannels(1);
                        // 设置倍速(0.5 - 2.0)
                        //audioStream.ChangeSpeed(1.25);
                        // 设置音量
                        //conversion.AddParameter("-filter:a \"volume=10dB\"");


                        // 音频截取
                        //audioStream.Split(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
                    }
                }
                
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
                string args = conversion.Build();
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

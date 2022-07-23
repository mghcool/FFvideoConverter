using Newtonsoft.Json.Linq;

namespace FFvideoConverter.Model
{
    /// <summary>
    /// 媒体信息储存
    /// </summary>
    public class MediaInfoStorage
    {
        /// <summary>
        /// 视频信息
        /// </summary>
        public VideoInfoStorage VideoInfo { get; set; }

        /// <summary>
        /// 音频信息
        /// </summary>
        public AudioInfoStorage[] AudioInfo { get; set; }

        /// <summary>
        /// 字幕信息
        /// </summary>
        public SubtitleInfoStorage[] SubtitleInfo { get; set; }

        /// <summary>
        /// 视频信息储存类
        /// </summary>
        public class VideoInfoStorage
        {
            /// <summary>
            /// 视频编码
            /// </summary>
            public string codec_name { get; set; }

            /// <summary>
            /// 视频编码详情
            /// </summary>
            public string codec_long_name { get; set; }

            /// <summary>
            /// 编码标签
            /// </summary>
            public string codec_tag_string { get; set; }

            /// <summary>
            /// 视频质量
            /// </summary>
            public string profile { get; set; }

            /// <summary>
            /// 分辨率宽度
            /// </summary>
            public int coded_width { get; set; }

            /// <summary>
            /// 分辨率高度
            /// </summary>
            public int coded_height { get; set; }

            /// <summary>
            /// 色彩格式
            /// </summary>
            public string pix_fmt { get; set; }

            /// <summary>
            /// 颜色空间
            /// </summary>
            public string color_space { get; set; }

            /// <summary>
            /// 视频时长
            /// </summary>
            public DateTime duration { get; set; }

            /// <summary>
            /// 比特率
            /// </summary>
            public string bit_rate { get; set; }

            /// <summary>
            /// 实际帧率
            /// </summary>
            public string r_frame_rate { get; set; }

            /// <summary>
            /// 平均帧率
            /// </summary>
            public string avg_frame_rate { get; set; }

            /// <summary>
            /// 标签
            /// </summary>
            public Tags? tags { get; set; }
        }

        /// <summary>
        /// 音频信息储存类
        /// </summary>
        public class AudioInfoStorage
        {
            public int index { get; set; }

            public string codec_name { get; set; }

            public string codec_long_name { get; set; }

            /// <summary>
            /// 编码标签
            /// </summary>
            public string codec_tag_string { get; set; }

            /// <summary>
            /// 音频质量
            /// </summary>
            public string profile { get; set; }

            /// <summary>
            /// 采样率
            /// </summary>
            public string sample_rate { get; set; }

            /// <summary>
            /// 通道数
            /// </summary>
            public int channels { get; set; }

            /// <summary>
            /// 通道布局(例如：立体声)
            /// </summary>
            public string channel_layout { get; set; }

            /// <summary>
            /// 音频时长
            /// </summary>
            public DateTime duration { get; set; }

            /// <summary>
            /// 比特率
            /// </summary>
            public string bit_rate { get; set; }

            /// <summary>
            /// 标签
            /// </summary>
            public Tags tags { get; set; }
        }

        /// <summary>
        /// 字幕信息储存类
        /// </summary>
        public class SubtitleInfoStorage
        {
            public int index { get; set; }

            public string codec_name { get; set; }

            public string codec_long_name { get; set; }

            /// <summary>
            /// 标签
            /// </summary>
            public Tags tags { get; set; }
        }

        /// <summary>
        /// 媒体流标签类
        /// </summary>
        public class Tags : Dictionary<string, JToken?>
        {
            /// <summary>
            /// 获取或设置指定键的值
            /// </summary>
            /// <param name="key">键</param>
            /// <returns></returns>
            public new JToken? this[string key]
            {
                get
                {
                    _ = this.TryGetValue(key, out JToken? jToken);
                    return jToken;
                }
                set
                {
                    this[key] = value;
                }
            }

            /// <summary>
            /// 根据模糊键查询值
            /// </summary>
            /// <param name="keyContains">键包含的字符串</param>
            /// <returns>找到返回对应值，未找到返回null</returns>
            public JToken? GetValueLike(string keyContains)
            {
                return this.Where(node => { return node.Key.Contains(keyContains); }).FirstOrDefault().Value;
            }
        }
    }
}

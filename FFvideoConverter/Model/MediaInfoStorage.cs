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
        }

        /// <summary>
        /// 字母信息储存类
        /// </summary>
        public class SubtitleInfoStorage
        {
            public int index { get; set; }

            public string codec_name { get; set; }

            public string codec_long_name { get; set; }

            public Tags tags { get; set; }

            public class Tags
            {
                public string language { get; set; }

                public string title { get; set; }
                public string NUMBER_OF_BYTES { get; set; }
            }
        }
    }
}

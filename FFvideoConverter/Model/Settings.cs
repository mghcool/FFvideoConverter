using NetDimension.NanUI.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter.Model
{
    /// <summary>
    /// 输出媒体类型
    /// </summary>
    public static class Settings
    {
        public static JavaScriptArray OutputTypes = new () { "MP4", "AVI", "MPEG", "MOV", "MKV", "3GP", "WMV", "FLV", "MPG", "AV1", "OGV", "MP3" };
    }
}

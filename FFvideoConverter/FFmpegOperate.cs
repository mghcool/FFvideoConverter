using FFvideoConverter.Model;
using NetDimension.NanUI;
using NetDimension.NanUI.JavaScript;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    public class FFmpegOperate
    {
        private Formium _formium;
        private FFmpegHelper _ffmpeg = new ();

        /// <summary>
        /// 创建一个ffmpeg前端操作类
        /// </summary>
        /// <param name="formium">窗体对象</param>
        public FFmpegOperate(Formium formium)
        {
            _formium = formium;
        }

        #region 注册的变量
        /// <summary>
        /// 文件输出类型
        /// </summary>
        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public JavaScriptArray OutputTypes => Settings.OutputTypes;

        /// <summary>
        /// 转码进度
        /// </summary>
        [JsObjectType(JsObjectType.PropertieType.ReadWrite)]
        public double Progress
        {
            get { return _ffmpeg.ConvertProgress; }
            set { _ffmpeg.ConvertProgress = value; }
        }
        #endregion

        #region 注册的方法
        /// <summary>
        /// 打开调试工具
        /// </summary>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public void ShowDevTools()
        {
            _formium.ShowDevTools();
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="WindowHWND">主窗口句柄</param>
        /// <returns>文件路径</returns>
        [JsObjectType(JsObjectType.MethodType.Sync, true)]
        public string OpenFile(IWin32Window WindowHWND)
        {
            var file = new OpenFileDialog();
            file.Filter = "video|*.mp4;*.mkv;*.ts;*.mov;*.avi;*.mpeg;*.wmv;*.rmvb";
            if (file.ShowDialog(WindowHWND) == DialogResult.OK)
                return file.FileName;
            else
                return null;
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="WindowHWND">主窗口句柄</param>
        /// <returns>文件夹路径</returns>
        [JsObjectType(JsObjectType.MethodType.Sync, true)]
        public string FolderBrowser(IWin32Window WindowHWND)
        {
            var path = new FolderBrowserDialog();
            if (path.ShowDialog(WindowHWND) == DialogResult.OK)
                return path.SelectedPath;
            else
                return null;
        }

        /// <summary>
        /// 获取媒体信息
        /// </summary>
        /// <param name="fileName">媒体文件路径</param>
        /// <returns>媒体信息数组</returns>
        /// <exception cref="Exception"></exception>
        [JsObjectType(JsObjectType.MethodType.Async)]
        public JavaScriptArray? GetMediaInfo(string fileName)
        {
            if(!File.Exists(fileName)) throw new Exception("视频文件未找到！");
            string[] mediaInfo = _ffmpeg.GetMediaInfo(fileName);
            return new JavaScriptArray()
            {
                mediaInfo[0],
                mediaInfo[1],
                mediaInfo[2]
            };
        }

        /// <summary>
        /// 开始转码
        /// </summary>
        /// <param name="json">转码配置json字符串</param>
        /// <returns>是否成功</returns>
        [JsObjectType(JsObjectType.MethodType.Async)]
        public bool StartConvert(string json)
        {
            FFConvertConfig config = JObject.Parse(json).ToObject<FFConvertConfig>();
            _ffmpeg.ConvertProgress = 0;
            return _ffmpeg.Convert(config);
        }

        /// <summary>
        /// 停止转码
        /// </summary>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public void StopConvert()
        {
            _ffmpeg.CancelConvert();
            _ffmpeg.ConvertProgress = 0;
        }
        #endregion
    }
}

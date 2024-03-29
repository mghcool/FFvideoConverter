﻿using FFvideoConverter.Model;
using NetDimension.NanUI;
using NetDimension.NanUI.Browser;
using NetDimension.NanUI.JavaScript;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    public class FFmpegOperate: JsSharpModel
    {
        private FFmpegHelper _ffmpeg = new ();

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

        /// <summary>
        /// 桌面文件路径
        /// </summary>
        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// 拖放的文件路径
        /// </summary>
        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public string DropFilePath { get; private set; }
        #endregion

        #region 注册的方法
        /// <summary>
        /// 打开调试工具
        /// </summary>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public void ShowDevTool()
        {
            ShowDevTools();
        }

        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="WindowHWND">主窗口句柄</param>
        /// <returns>文件路径</returns>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public string? OpenFile()
        {
            string ret = null;
            InvokeIfRequired(() =>
            {
                var file = new OpenFileDialog();
                string filterStr = "video|";
                foreach(string str in Settings.InputTypes)
                {
                    filterStr += $"*.{str.ToLower()};";
                }
                file.Filter = filterStr;
                if (file.ShowDialog(WindowHWND) == DialogResult.OK)
                    ret =  file.FileName;
            });
            return ret;
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="WindowHWND">主窗口句柄</param>
        /// <returns>文件夹路径</returns>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public string FolderBrowser()
        {
            string ret = null;
            InvokeIfRequired(() =>
            {
                var path = new FolderBrowserDialog();
                if (path.ShowDialog(WindowHWND) == DialogResult.OK)
                    ret = path.SelectedPath;
            });
            return ret;
        }

        /// <summary>
        /// 获取媒体信息
        /// </summary>
        /// <param name="fileName">媒体文件路径</param>
        /// <returns>媒体信息数组</returns>
        /// <exception cref="Exception"></exception>
        [JsObjectType(JsObjectType.MethodType.Async)]
        public JavaScriptJsonValue? GetMediaInfo(string fileName)
        {
            if(!File.Exists(fileName)) throw new Exception("视频文件未找到！");
            string[] mediaInfo = _ffmpeg.GetMediaInfo(fileName);
            return new JavaScriptJsonValue(new
            {
                VideoInfo = mediaInfo[0],
                AudioInfo = mediaInfo[1],
                SubtitleInfo = mediaInfo[2]
            });
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

        // 测试类型
        class TestObj
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public DateTime Time { get; set; }
        }

        /// <summary>
        /// 测试js和C#对象互传
        /// </summary>
        /// <param name="jsVal">js对象</param>
        /// <returns>转换成C#对象后再传出</returns>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public JavaScriptJsonValue TestJsObject(JavaScriptObject jsVal)
        {
            var testobj = jsVal.ToObject<TestObj>();
            return new JavaScriptJsonValue(testobj);
        }

        /// <summary>
        /// 测试执行js命令
        /// </summary>
        /// <param name="msg"></param>
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public void TestJavaScript(string msg)
        {
            ExecuteJavaScript($"window.vue.MessageShow('js执行测试', '{msg}')");
        }
        #endregion

        /// <summary>
        /// 文件拖放时触发
        /// </summary>
        /// <param name="e">拖放参数</param>
        protected override void DragEvent(DragEnterEventArgs e)
        {
            e.Handled = true;
            if (!e.DragData.IsFile) return;

            string[] draggedFiles = e.DragData.GetFileNames();
            if ((draggedFiles?.Length ?? 0) > 0)
            {
                e.Handled = false;
                string filePath = draggedFiles[0];
                Debug.WriteLine(filePath);
                if (File.Exists(filePath))
                {
                    string extensionName = Path.GetExtension(filePath).Replace(".", "");
                    if (Settings.InputTypes.Contains(extensionName.ToUpper()))
                        DropFilePath = filePath;
                    else
                        DropFilePath = "";
                }
                else
                {
                    DropFilePath = "";
                }
            }
        }
    }
}

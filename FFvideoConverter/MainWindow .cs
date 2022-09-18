using FFvideoConverter.Model;
using NetDimension.NanUI;
using NetDimension.NanUI.HostWindow;
using NetDimension.NanUI.JavaScript;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;

namespace FFvideoConverter
{
    internal class MainWindow : Formium
    {
        readonly FFmpegHelper ffmpegHelper = new FFmpegHelper();

        // 设置窗体样式类型
        public override HostWindowType WindowType => HostWindowType.Borderless;
        // 指定启动 Url
#if DEBUG
        public override string StartUrl => "http://localhost:8080/";
#else
        public override string StartUrl => "http://my.page/index.html";
#endif

        public MainWindow()
        {
            // 在此处设置窗口样式
            this.Sizable = false;
            this.Size = new Size(960, 630);
            this.Title = "FF视频转换器";
            this.Subtitle = "视频转换";
            this.AllowSystemMenu = false;
            this.Icon = (Icon)Resource.ResourceManager.GetObject("icon");
            // 设置启动位置
            this.StartPosition = FormStartPosition.CenterScreen;
            // 设置边框效果
            BorderlessWindowStyle style = UseExtendedStyles<BorderlessWindowStyle>();
            style.ShadowEffect = ShadowEffect.Glow;
            style.ShadowColor = Color.DimGray;
            // 设置启动页面
            this.CustomizeMaskPanel();
        }

        

        // 创建启动页面
        private void CustomizeMaskPanel()
        {
            // 设置标签
            var label = new Label
            {
                Text = "正在启动...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Light", 24.0f, GraphicsUnit.Point),
                Width = this.Width,
                Height = this.Height,
            };

            // 添加标签
            SplashScreen.Content.Add(label);
            // 添加背景图
            SplashScreen.BackColor = ColorTranslator.FromHtml("#545C64");
        }

        // 在此处进行浏览器相关操作
        protected override void OnReady()
        {
            // 注册js对象
            RegisterJavaScriptObject();
            JsObjTest jsObjTest = new JsObjTest();
            jsObjTest.OutputTypes = Settings.OutputTypes;
            JsObjectHelper jsObjectHelper = new JsObjectHelper(this);
            jsObjectHelper.Register("TestObject", jsObjTest);
        }

        private void RegisterJavaScriptObject()
        {
            JavaScriptObject jsObj = new JavaScriptObject();

            // 注册打开调试工具方法
            jsObj.Add("showdevtools", args =>
            {
                ShowDevTools();
                return new JavaScriptValue("ok");
            });

            //注册值
            jsObj.Add("version", "v123");

            // 注册输出文件类型
            jsObj.DefineProperty("OutputTypes", () => Settings.OutputTypes);

            // 注册打开文件方法
            jsObj.Add("OpenFile", (args, promise) =>
            {
                bool isCancel = false;
                string fileName = string.Empty;
                InvokeIfRequired(() =>
                {
                    var file = new OpenFileDialog();
                    file.Filter = "video|*.mp4;*.mkv;*.ts;*.mov;*.avi;*.mpeg;*.wmv;*.rmvb";
                    if (file.ShowDialog(WindowHWND) == DialogResult.OK)
                    {
                        fileName = file.FileName;
                    }
                    else
                    {
                        promise.Reject("取消操作");
                        isCancel = true;
                    }
                });
                if (isCancel) return;
                string[] mediaInfo = ffmpegHelper.GetMediaInfo(fileName);
                var retArray = new JavaScriptArray()
                {
                    fileName,
                    mediaInfo[0],
                    mediaInfo[1],
                    mediaInfo[2]
                };
                promise.Resovle(retArray);
            });

            // 注册打开文件夹方法
            jsObj.Add("FolderBrowser", (args, promise) =>
            {
                InvokeIfRequired(() =>
                {
                    var path = new FolderBrowserDialog();
                    if (path.ShowDialog(this.WindowHWND) == DialogResult.OK)
                    {
                        promise.Resovle(new JavaScriptValue(path.SelectedPath));
                    }
                    else
                    {
                        promise.Reject("取消操作");
                    }
                });
            });

            // 注册开始转码方法
            jsObj.Add("StartConvert", async (args, promise) =>
            {
                FFConvertConfig config = JObject.Parse(args[0].GetString()).ToObject<FFConvertConfig>();
                await Task.Run(() => {
                    ffmpegHelper.ConvertProgress = 0;
                    bool isOk = ffmpegHelper.Convert(config);
                    if (isOk) promise.Resovle(new JavaScriptValue("转码成功"));
                    else promise.Reject("转码失败");
                });
            });

            // 注册停止转码方法
            jsObj.Add("StopConvert", async (args, promise) =>
            {
                await Task.Run(() => {
                    ffmpegHelper.CancelConvert();
                    ffmpegHelper.ConvertProgress = 0;
                    promise.Resovle(new JavaScriptValue("取消转换"));
                });
            });

            // 注册转码进度条
            jsObj.DefineProperty("Progress",
                () => new JavaScriptValue(ffmpegHelper.ConvertProgress),
                (val) => { InvokeIfRequired(() => ffmpegHelper.ConvertProgress = val); });

            RegisterJavaScriptObject("SharpObject", jsObj);
        }
    }
}

using NetDimension.NanUI;
using NetDimension.NanUI.HostWindow;
using NetDimension.NanUI.JavaScript;
using Newtonsoft.Json.Linq;

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

            // 注册打开文件方法
            jsObj.Add("OpenFile", (args, promise) =>
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    OpenFileDialog file = new OpenFileDialog();
                    file.Filter = "video|*.mp4;*.mkv;*.ts";
                    if (file.ShowDialog() == DialogResult.OK)
                    {
                        promise.Resovle(new JavaScriptValue(file.FileName));
                    }
                    else
                    {
                        promise.Reject("取消操作");
                    }
                }));
                thread.SetApartmentState(ApartmentState.STA); //创建并进入一个线程单元
                thread.Start();
            });

            // 注册打开文件夹方法
            jsObj.Add("FolderBrowser", (args, promise) =>
            {
                Thread thread = new Thread(new ThreadStart(() =>
                {
                    FolderBrowserDialog path = new FolderBrowserDialog();
                    if (path.ShowDialog() == DialogResult.OK)
                    {
                        promise.Resovle(new JavaScriptValue(path.SelectedPath));
                    }
                    else
                    {
                        promise.Reject("取消操作");
                    }
                }));
                thread.SetApartmentState(ApartmentState.STA); //创建并进入一个线程单元
                thread.Start();
            });

            // 注册转码方法
            jsObj.Add("VideoConvert", async (args, promise) =>
            {
                FFConvertConfig config = JObject.Parse(args[0].GetString()).ToObject<FFConvertConfig>();
                await Task.Run(() => {
                    ffmpegHelper.ConvertProgress = 0;
                    bool isOk = ffmpegHelper.Convert(config);
                    if (isOk) promise.Resovle(new JavaScriptValue("转换成功"));
                    else promise.Reject("转码失败");
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

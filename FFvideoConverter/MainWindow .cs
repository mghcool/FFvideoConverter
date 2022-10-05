using NetDimension.NanUI;
using NetDimension.NanUI.HostWindow;
using NetDimension.NanUI.Browser;
using System.Diagnostics;
using System.Xml.Linq;
using FFvideoConverter.Model;
using System.Linq;

namespace FFvideoConverter
{
    internal class MainWindow : Formium
    {
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
                Text = "正在启动FF视频转换器...",
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
            var jsObjectHelper = new JsObjectHelper(this);
            jsObjectHelper.Register("SharpObject", new FFmpegOperate());
        }
    }
}

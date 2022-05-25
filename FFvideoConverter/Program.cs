using NetDimension.NanUI;

namespace FFvideoConverter
{
    internal static class Program
    {
        static void Main()
        {
            WinFormium.CreateRuntimeBuilder(env => {

                env.CustomCefSettings(settings =>
                {
                    // 在此处设置 CEF 的相关参数
                    settings.CachePath = "./cache";
                });

                env.CustomCefCommandLineArguments(args =>
                {
                    // 在此处指定 CEF 命令行参数
                    // 关闭同源策略(解决跨域问题)
                    args.AppendSwitch("disable-web-security");
                    // 禁用拼写检查
                    args.AppendSwitch("disable-spell-checking");
                    // 其他参数
                    args.AppendSwitch("ignore-certificate-errors");
                    args.AppendSwitch("enable-media-stream");
                    args.AppendSwitch("enable-print-preview");
                    args.AppendSwitch("enable-gpu");
                });

            }, app =>
            {
#if DEBUG
                // 指定是否开启调试模式。
                app.UseDebuggingMode();
#endif
                // 指定启动窗体
                app.UseMainWindow(context => new MainWindow());

                // 设置web资源
                app.UseEmbeddedFileResource("http", "my.page", "www");
                //app.UseLocalFileResource("http", "my.page", "www");
                //app.UseDataServiceResource("http", "my.service");
            })
            .Build()
            .Run();
        }
    }
}
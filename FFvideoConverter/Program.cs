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
                    // �ڴ˴����� CEF ����ز���
                    settings.CachePath = "./cache";
                });

                env.CustomCefCommandLineArguments(args =>
                {
                    // �ڴ˴�ָ�� CEF �����в���
                    // �ر�ͬԴ����(�����������)
                    args.AppendSwitch("disable-web-security");
                    // ����ƴд���
                    args.AppendSwitch("disable-spell-checking");
                    // ��������
                    args.AppendSwitch("ignore-certificate-errors");
                    args.AppendSwitch("enable-media-stream");
                    args.AppendSwitch("enable-print-preview");
                    args.AppendSwitch("enable-gpu");
                });

            }, app =>
            {
#if DEBUG
                // ָ���Ƿ�������ģʽ��
                app.UseDebuggingMode();
#endif
                // ָ����������
                app.UseMainWindow(context => new MainWindow());

                // ����web��Դ
                app.UseEmbeddedFileResource("http", "my.page", "www");
                //app.UseLocalFileResource("http", "my.page", "www");
                //app.UseDataServiceResource("http", "my.service");
            })
            .Build()
            .Run();
        }
    }
}
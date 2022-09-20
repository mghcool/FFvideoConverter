using NetDimension.NanUI;
using NetDimension.NanUI.JavaScript;
using Xilium.CefGlue;

namespace FFvideoConverter.Model
{
    public class JsSharpModel
    {
        private Formium _formium;

        /// <summary>
        /// 设置主窗口对象
        /// </summary>
        /// <param name="formium">Formium对象</param>
        public void SetMainWindow(Formium formium)
        {
            _formium = formium;
        }

        /// <summary>
        /// 窗口句柄
        /// </summary>
        public IWin32Window WindowHWND => _formium.WindowHWND;

        /// <summary>
        /// 显示浏览器调试工具
        /// </summary>
        public void ShowDevTools() => _formium.ShowDevTools();

        /// <summary>
        /// 在主窗口的线程上执行方法
        /// </summary>
        /// <param name="a">方法</param>
        public void InvokeIfRequired(Action a) => _formium.InvokeIfRequired(a);

        /// <summary>
        /// 同步执行js方法
        /// </summary>
        /// <param name="code"></param>
        public void ExecuteJavaScript(string code) => _formium.ExecuteJavaScript(code);

        /// <summary>
        /// 异步执行js方法
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<JavaScriptExecutionResult> EvaluateJavaScriptAsync(string code) => await _formium.EvaluateJavaScriptAsync(code);

        /// <summary>
        /// 异步执行js方法
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<JavaScriptExecutionResult> EvaluateJavaScriptAsync(CefFrame frame, string code) => await _formium.EvaluateJavaScriptAsync(frame, code);
    }
}

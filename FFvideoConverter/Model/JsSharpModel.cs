/*****************************************************************************
 * 使用说明：
 * C#方法接收Js对象参数时，需要使用 JavaScriptObject 类型
 * C#方法返回Js对象时，返回值应是 JavaScriptJsonValue 类型
 * **************************************************************************/
using NetDimension.NanUI;
using NetDimension.NanUI.Browser;
using NetDimension.NanUI.JavaScript;
using Xilium.CefGlue;

namespace FFvideoConverter.Model
{
    /// <summary>
    /// Js和C#互相操作的基类
    /// </summary>
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
            _formium.DragEnter += (object? sender, DragEnterEventArgs e) => { DragEvent(e); };
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

        /// <summary>
        /// 文件拖放时触发
        /// </summary>
        /// <param name="e">拖放参数</param>
        protected virtual void DragEvent(DragEnterEventArgs e) { }
    }
}

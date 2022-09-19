using FFvideoConverter.Model;
using NetDimension.NanUI.JavaScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    public class JsObjTest
    {
        #region 属性
        // 常量
        [JsObjectType(JsObjectType.PropertieType.Const)]
        public string Version = "1.2.30";

        // 只读属性
        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public DateTime Time => DateTime.Now;

        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public JavaScriptArray OutputTypes => Settings.OutputTypes;

        // 可读写属性
        [JsObjectType(JsObjectType.PropertieType.ReadWrite)]
        public short Age { get; set; }
        #endregion

        #region 方法
        [JsObjectType(JsObjectType.MethodType.Sync)]
        public string GetName() { return "mgh"; }

        [JsObjectType(JsObjectType.MethodType.Sync)]
        public void SetAge(short age) { Age = age; }

        [JsObjectType(JsObjectType.MethodType.Sync, true)]
        public string OpenFile(IWin32Window hwnd)
        {
            MessageBox.Show(hwnd, "hhh", "Message from JS", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return "ok";
        }

        [JsObjectType(JsObjectType.MethodType.Async)]
        public string TestAsync()
        {
            Thread.Sleep(2000);
            return "测试通过";
        }

        [JsObjectType(JsObjectType.MethodType.Async)]
        public void TestAsync2(string txt)
        {
            Thread.Sleep(2000);
            Debug.WriteLine(txt);
        }
        #endregion
    }
}

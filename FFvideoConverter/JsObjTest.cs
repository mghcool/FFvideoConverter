using NetDimension.NanUI.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    public class JsObjTest
    {
        [JsObjectType(JsObjectType.PropertieType.ReadWrite)]
        public DateTime Age { get; set; }

        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public string Name { get; set; } = "mgh";

        

        private string Id;

        [JsObjectType(JsObjectType.MethodType.Sync)]
        public string GetName() { return Name; }

        //public int GetAge(string name) { return Age; }

        [JsObjectType(JsObjectType.PropertieType.ReadOnly)]
        public JavaScriptArray OutputTypes { get; set; }

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
    }
}

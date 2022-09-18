using FFvideoConverter.Model;
using NetDimension.NanUI;
using NetDimension.NanUI.JavaScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    /// <summary>
    /// js注册对象帮助类
    /// </summary>
    public class JsObjectHelper
    {
        /// <summary>
        /// 窗口对象
        /// </summary>
        public Formium _formium;

        /// <summary>
        /// 创建一个js注册帮助对象
        /// </summary>
        /// <param name="formium">窗口对象</param>
        public JsObjectHelper(Formium formium)
        {
            _formium = formium;
        }

        /// <summary>
        /// 注册对象
        /// </summary>
        /// <param name="name">注册的名称</param>
        /// <param name="registerObj">要注册的对象</param>
        public void Register(string name, object registerObj)
        {
            var jsObj = new JavaScriptObject();
            Type jsObjType = registerObj.GetType();
            //获取所有属性
            PropertyInfo[] properties = jsObjType.GetProperties();
            // 遍历属性
            foreach (PropertyInfo prop in properties)
            {
                JsObjectType? objType = (JsObjectType?)prop.GetCustomAttribute(typeof(JsObjectType));
                if (objType != null)
                {
                    JavaScriptValue jsVal = new();
                    string propertyType = prop.PropertyType.Name.ToLower();
                    switch (propertyType)
                    {
                        case "string":
                            jsVal = new JavaScriptValue((string)prop.GetValue(registerObj)); break;
                        case "int16":
                            jsVal = new JavaScriptValue((short)prop.GetValue(registerObj)); break;
                        case "int32":
                            jsVal = new JavaScriptValue((int)prop.GetValue(registerObj)); break;
                        case "int64":
                            jsVal = new JavaScriptValue((long)prop.GetValue(registerObj)); break;
                        case "single":
                            jsVal = new JavaScriptValue((float)prop.GetValue(registerObj)); break;
                        case "double":
                            jsVal = new JavaScriptValue((double)prop.GetValue(registerObj)); break;
                        case "datetime":
                            jsVal = new JavaScriptValue((DateTime)prop.GetValue(registerObj)); break;
                        case "javascriptarray":
                            jsVal = (JavaScriptArray)prop.GetValue(registerObj); break;
                    }
                    if (objType.Propertie == JsObjectType.PropertieType.ReadOnly)
                    {
                        jsObj.DefineProperty(prop.Name, () => jsVal);
                    }
                    else if (objType.Propertie == JsObjectType.PropertieType.ReadWrite)
                    {
                        jsObj.DefineProperty(prop.Name, () => jsVal, (val) => { val.ToObject(); });
                    }
                    else if (objType.Propertie == JsObjectType.PropertieType.Const)
                    {
                        jsObj.Add(prop.Name, jsVal);
                    }
                }
            }
            //获取所有方法。
            MethodInfo[] methods = jsObjType.GetMethods();
            //遍历方法打印到控制台。
            foreach (MethodInfo method in methods)
            {
                var aa = method.Attributes;
                if (!aa.HasFlag(MethodAttributes.SpecialName))
                {
                    JsObjectType? objType = (JsObjectType?)method.GetCustomAttribute(typeof(JsObjectType));
                    Console.WriteLine(method.Name);
                    if (objType != null)
                    {
                        if(objType.Method == JsObjectType.MethodType.Sync)
                        {
                            jsObj.Add(method.Name, args =>
                            {
                                object? ret = null;
                                if (objType.UsedWindowHwnd)
                                {
                                    _formium.InvokeIfRequired(() =>
                                    {
                                        ret = method.Invoke(registerObj, ParamesToObjectArry(args, true));
                                    });
                                }
                                else
                                {
                                    ret = method.Invoke(registerObj, ParamesToObjectArry(args));
                                }
                                if(ret == null) return new JavaScriptValue();
                                string retType = ret.GetType().Name.ToLower();
                                switch (retType)
                                {
                                    case "string":
                                        return new JavaScriptValue((string)ret);
                                    case "int16":
                                        return new JavaScriptValue((short)ret);
                                    case "int32":
                                        return new JavaScriptValue((int)ret);
                                    case "int64":
                                        return new JavaScriptValue((long)ret);
                                    case "single":
                                        return new JavaScriptValue((float)ret);
                                    case "double":
                                        return new JavaScriptValue((double)ret);
                                    case "datetime":
                                        return new JavaScriptValue((DateTime)ret);
                                    default:
                                        return new JavaScriptValue();
                                }
                                
                            });

                        }
                    }
                }
            }
            // 注册
            _formium.RegisterJavaScriptObject(name, jsObj);
        }

        /// <summary>
        /// 参数转换
        /// </summary>
        /// <param name="args">前端传过来的参数</param>
        /// <param name="usedWindowHwnd">是否使用窗口句柄</param>
        /// <returns>参数对象</returns>
        private object[] ParamesToObjectArry(JavaScriptArray args, bool usedWindowHwnd = false)
        {
            List<object> result = new ();
            if (usedWindowHwnd) result.Add(_formium.WindowHWND);
            foreach (JavaScriptValue arg in args)
            {
                if (arg.IsBool) result.Add(arg.GetBool());
                else if (arg.IsString) result.Add(arg.GetString());
                //else if (arg.IsNumber) result.Add(arg.GetInt());
                else if (arg.IsNumber) result.Add(arg.GetDouble());
                else if (arg.IsDateTime) result.Add(arg.GetDateTime());
            }
            return result.ToArray();
        }
    }
}

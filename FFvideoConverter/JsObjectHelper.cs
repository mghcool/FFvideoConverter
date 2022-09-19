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
                    var jsVal = new JavaScriptValue("未识别的数据类型");
                    string propertyType = prop.PropertyType.Name.ToLower();
                    switch (propertyType)
                    {
                        case "string":
                            jsVal = new JavaScriptValue((string)prop.GetValue(registerObj)); break;
                        case "bool":
                            jsVal = new JavaScriptValue((bool)prop.GetValue(registerObj)); break;
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
                    if (objType.Propertie == JsObjectType.PropertieType.Const)
                    {
                        // 注册常量
                        jsObj.Add(prop.Name, jsVal);
                    }
                    else if (objType.Propertie == JsObjectType.PropertieType.ReadOnly)
                    {
                        // 注册只读属性
                        jsObj.DefineProperty(prop.Name, () => jsVal);
                    }
                    else if (objType.Propertie == JsObjectType.PropertieType.ReadWrite)
                    {
                        // 注册可读写属性
                        jsObj.DefineProperty(prop.Name, () => jsVal, (val) => 
                        {
                            switch (propertyType)
                            {
                                case "string":
                                    prop.SetValue(registerObj, val.GetString());
                                    break;
                                case "bool":
                                    prop.SetValue(registerObj, val.GetBool());
                                    break;
                                case "int16":
                                    prop.SetValue(registerObj, val.GetInt());
                                    break;
                                case "int32":
                                    prop.SetValue(registerObj, val.GetInt());
                                    break;
                                case "int64":
                                    prop.SetValue(registerObj, val.GetInt());
                                    break;
                                case "single":
                                    prop.SetValue(registerObj, val.GetDouble());
                                    break;
                                case "double":
                                    prop.SetValue(registerObj, val.GetDouble());
                                    break;
                                case "datetime":
                                    prop.SetValue(registerObj, val.GetDateTime());
                                    break;
                                case "javascriptarray":
                                    prop.SetValue(registerObj, val.ToArray());
                                    break;
                                default:
                                    throw new Exception("找不到要写入数据的类型");
                            }
                        });
                    }
                }
            }

            //获取所有方法。
            MethodInfo[] methods = jsObjType.GetMethods();
            //遍历方法
            foreach (MethodInfo method in methods)
            {
                JsObjectType? objType = (JsObjectType?)method.GetCustomAttribute(typeof(JsObjectType));
                if (objType != null)
                {
                    if(objType.Method == JsObjectType.MethodType.Sync)
                    {
                        // 注册同步方法
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
                            return retType switch
                            {
                                "string" => new JavaScriptValue((string)ret),
                                "bool" => new JavaScriptValue((bool)ret),
                                "int16" => new JavaScriptValue((short)ret),
                                "int32" => new JavaScriptValue((int)ret),
                                "int64" => new JavaScriptValue((long)ret),
                                "single" => new JavaScriptValue((float)ret),
                                "double" => new JavaScriptValue((double)ret),
                                "datetime" => new JavaScriptValue((DateTime)ret),
                                _ => new JavaScriptValue(),
                            };
                        });
                    }
                    else if(objType.Method == JsObjectType.MethodType.Async)
                    {
                        // 注册异步方法
                        jsObj.Add(method.Name, async (args, promise) =>
                        {
                            var ret = await Task.Run(() => 
                            {
                                return method.Invoke(registerObj, ParamesToObjectArry(args));
                            });
                            string retType = method.ReturnType.Name.ToLower();
                            if (ret != null)
                            {
                                switch (retType)
                                {
                                    case "string":
                                        promise.Resovle(new JavaScriptValue((string)ret)); break;
                                    case "bool":
                                        promise.Resovle(new JavaScriptValue((bool)ret)); break;
                                    case "int16":
                                        promise.Resovle(new JavaScriptValue((short)ret)); break;
                                    case "int32":
                                        promise.Resovle(new JavaScriptValue((int)ret)); break;
                                    case "int64":
                                        promise.Resovle(new JavaScriptValue((long)ret)); break;
                                    case "single":
                                        promise.Resovle(new JavaScriptValue((float)ret)); break;
                                    case "double":
                                        promise.Resovle(new JavaScriptValue((double)ret)); break;
                                    case "datetime":
                                        promise.Resovle(new JavaScriptValue((DateTime)ret)); break;
                                    default:
                                        promise.Reject("方法失败");
                                        break;
                                }
                                
                            }
                        });
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

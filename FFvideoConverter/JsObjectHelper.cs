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
        public void Register<T>(string name, T registerObj)
        {
            var jsObj = new JavaScriptObject();
            Type jsObjType = typeof(T);
            MemberInfo[] members = jsObjType.GetMembers();
            foreach(var member in members)
            {
                // 获取成员特性
                var memberAttr = (JsObjectType?)member.GetCustomAttribute(typeof(JsObjectType));
                // 忽略没有标记特性的成员
                if(memberAttr == null) continue;
                // 成员名称
                var memberName = member.Name;
                // 注册属性和字段
                if (member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field)
                {
                    // 获取属性、字段和数据类型
                    PropertyInfo prop = null;   // 属性对象
                    FieldInfo field = null;     // 字段对象
                    Type memberType = null;     // 成员类型
                    if (member.MemberType == MemberTypes.Property)
                    {
                        prop = (PropertyInfo)member;
                        memberType = prop.PropertyType;
                    }
                    else
                    {
                        field = (FieldInfo)member;
                        memberType = field.FieldType;
                    }
                    // 开始注册属性和字段
                    if (memberAttr.Propertie == JsObjectType.PropertieType.Const)
                    {
                        // 注册常量
                        if(member.MemberType == MemberTypes.Property)
                            jsObj.Add(memberName, ObjectToJsVal(memberType, prop.GetValue(registerObj)));
                        else
                            jsObj.Add(memberName, ObjectToJsVal(memberType, field.GetValue(registerObj)));
                    }
                    else if (memberAttr.Propertie == JsObjectType.PropertieType.ReadOnly)
                    {
                        // 注册只读属性
                        if (member.MemberType == MemberTypes.Property)
                            jsObj.DefineProperty(memberName, () => ObjectToJsVal(memberType, prop.GetValue(registerObj)));
                        else
                            jsObj.DefineProperty(memberName, () => ObjectToJsVal(memberType, field.GetValue(registerObj)));
                    }
                    else if (memberAttr.Propertie == JsObjectType.PropertieType.ReadWrite)
                    {
                        // 注册可读写属性
                        if (member.MemberType == MemberTypes.Property)
                            jsObj.DefineProperty(memberName,
                            () => ObjectToJsVal(memberType, prop.GetValue(registerObj)),
                            (val) => prop.SetValue(registerObj, JsValToObject(memberType, val)));
                        else
                            jsObj.DefineProperty(memberName,
                            () => ObjectToJsVal(memberType, field.GetValue(registerObj)),
                            (val) => field.SetValue(registerObj, JsValToObject(memberType, val)));
                    }
                }
                // 注册方法
                else if (member.MemberType == MemberTypes.Method)
                {
                    MethodInfo method = (MethodInfo)member;
                    Type returnType = method.ReturnType;
                    if (memberAttr.Method == JsObjectType.MethodType.Sync)
                    {
                        // 注册同步方法
                        jsObj.Add(memberName, args =>
                        {
                            object? ret = null;
                            // 转换参数
                            object[] parameters = ParamesToObjectArry(method.GetParameters(), args);
                            if (parameters == null) return new JavaScriptValue("参数错误");

                            if (memberAttr.UsedWindowHwnd)  // 使用窗口句柄
                            {
                                _formium.InvokeIfRequired(() =>
                                {
                                    ret = method.Invoke(registerObj, parameters);
                                });
                            }
                            else
                            {
                                ret = method.Invoke(registerObj, parameters);
                            }

                            var val = ObjectToJsVal(returnType, ret);
                            if (val != null) return val;
                            else return new JavaScriptValue();
                        });
                    }
                    else if (memberAttr.Method == JsObjectType.MethodType.Async)
                    {
                        // 注册异步方法
                        jsObj.Add(memberName, async (args, promise) =>
                        {
                            object[] parameters = ParamesToObjectArry(method.GetParameters(), args);
                            if (parameters == null)
                            {
                                promise.Reject("参数错误");
                                return;
                            }
                            var ret = await Task.Run(() =>
                            {
                                return method.Invoke(registerObj, ParamesToObjectArry(method.GetParameters(), args));
                            });
                            if(returnType.Name.ToLower() == "void")
                            {
                                promise.Resovle();
                                return;
                            }
                                
                            var val = ObjectToJsVal(returnType, ret);
                            if (val != null) 
                                promise.Resovle(val);
                            else 
                                promise.Reject("方法失败");
                        });
                    }
                }
            }
            // 注册
            _formium.RegisterJavaScriptObject(name, jsObj);
        }

        /// <summary>
        /// Object转JavaScriptValue
        /// </summary>
        /// <param name="type">属性或方法返回值的类型</param>
        /// <param name="val">要转换的值</param>
        /// <returns>结果对象，转换失败返回null</returns>
        private JavaScriptValue? ObjectToJsVal(Type type, object? val)
        {
            if(val == null) return null;
            string dateType = type.Name.ToLower();
            return dateType switch
            {
                "string" => new JavaScriptValue((string)val),
                "bool" => new JavaScriptValue((bool)val),
                "int16" => new JavaScriptValue((short)val),
                "int32" => new JavaScriptValue((int)val),
                "int64" => new JavaScriptValue((long)val),
                "single" => new JavaScriptValue((float)val),
                "double" => new JavaScriptValue((double)val),
                "datetime" => new JavaScriptValue((DateTime)val),
                "javascriptarray" => (JavaScriptArray)val,
                _ => null,
            };
        }

        /// <summary>
        /// JavaScriptValue转Object
        /// </summary>
        /// <param name="type">属性或方法返回值的类型</param>
        /// <param name="val">要转换的值</param>
        /// <returns>结果对象，转换失败返回null</returns>
        private object? JsValToObject(Type type, JavaScriptValue val)
        {
            if (val == null) return null;
            string dateType = type.Name.ToLower();
            return dateType switch
            {
                "string" => val.GetString(),
                "bool" => val.GetBool(),
                "int16" => Convert.ToInt16(val.GetInt()),
                "int32" => val.GetInt(),
                "int64" => Convert.ToInt64(val.GetInt()),
                "single" => Convert.ToSingle(val.GetDouble()),
                "double" => val.GetDouble(),
                "datetime" => val.GetDateTime(),
                "javascriptarray" => (JavaScriptArray)val,
                _ => new JavaScriptValue("获取值失败"),
            };
        }

        /// <summary>
        /// 方法参数转换
        /// </summary>
        /// <param name="argsInfo">原始方法的参数信息</param>
        /// <param name="args">前端传过来的参数</param>
        /// <param name="usedWindowHwnd">是否使用窗口句柄</param>
        /// <returns>参数对象，如果参数不符合，返回null</returns>
        private object[]? ParamesToObjectArry(ParameterInfo[] argsInfo, JavaScriptArray args)
        {
            List<object> result = new();
            for (int i = 0; i < argsInfo.Length; i++)
            {
                string argType = argsInfo[i].ParameterType.Name.ToLower();
                // 验证参数
                switch (argType)
                {
                    case "string":
                        if(!args[i].IsString) return null; break;
                    case "bool":
                        if (!args[i].IsBool) return null; break;
                    case "int16":
                    case "int32":
                    case "int64":
                    case "single":
                    case "double":
                        if (!args[i].IsNumber) return null; break;
                    case "datetime":
                        if (!args[i].IsDateTime) return null; break;
                    case "javascriptarray":
                        if (!args[i].IsArray) return null; break;
                }
                // 转换参数
                switch (argType)
                {
                    case "string":
                        result.Add(args[i].GetString());
                        break;
                    case "bool":
                        result.Add(args[i].GetBool());
                        break;
                    case "int16":
                        result.Add(Convert.ToInt16(args[i].GetInt()));
                        break;
                    case "int32":
                        result.Add(args[i].GetInt());
                        break;
                    case "int64":
                        result.Add(Convert.ToInt64(args[i].GetInt()));
                        break;
                    case "single":
                        result.Add(Convert.ToSingle(args[i].GetDouble()));
                        break;
                    case "double":
                        result.Add(args[i].GetDouble());
                        break;
                    case "datetime":
                        result.Add(args[i].GetDateTime());
                        break;
                    case "javascriptarray":
                        result.Add(args[i].ToArray());
                        break;
                    case "iwin32window":
                        result.Add(_formium.WindowHWND);
                        break;
                    default:
                        return null;
                }
            }
            return result.ToArray();
        }
    }
}

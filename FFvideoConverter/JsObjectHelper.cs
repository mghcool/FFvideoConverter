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
                            if (parameters == null) throw new Exception("参数错误");
                            //if (parameters == null) return new JavaScriptValue("参数错误");

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
            var dateType = ToolHelper.TypeToEnum<JsDataType>(type);
            return dateType switch
            {
                JsDataType.String => new JavaScriptValue((string)val),
                JsDataType.Bool => new JavaScriptValue((bool)val),
                JsDataType.Int16 => new JavaScriptValue((short)val),
                JsDataType.Int32 => new JavaScriptValue((int)val),
                JsDataType.Int64 => new JavaScriptValue((long)val),
                JsDataType.Single => new JavaScriptValue((float)val),
                JsDataType.Double => new JavaScriptValue((double)val),
                JsDataType.DateTime => new JavaScriptValue((DateTime)val),
                JsDataType.JavaScriptArray => (JavaScriptArray)val,
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
            var dateType = ToolHelper.TypeToEnum<JsDataType>(type);
            return dateType switch
            {
                JsDataType.String => val.GetString(),
                JsDataType.Bool => val.GetBool(),
                JsDataType.Int16 => Convert.ToInt16(val.GetInt()),
                JsDataType.Int32 => val.GetInt(),
                JsDataType.Int64 => Convert.ToInt64(val.GetInt()),
                JsDataType.Single => Convert.ToSingle(val.GetDouble()),
                JsDataType.Double => val.GetDouble(),
                JsDataType.DateTime => val.GetDateTime(),
                JsDataType.JavaScriptArray => (JavaScriptArray)val,
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
                //string argType = argsInfo[i].ParameterType.Name.ToLower();
                var argType = ToolHelper.TypeToEnum<JsDataType>(argsInfo[i].ParameterType);
                // 验证参数
                switch (argType)
                {
                    case JsDataType.String:
                        if(!args[i].IsString) return null; break;
                    case JsDataType.Bool:
                        if (!args[i].IsBool) return null; break;
                    case JsDataType.Int16:
                    case JsDataType.Int32:
                    case JsDataType.Int64:
                    case JsDataType.Single:
                    case JsDataType.Double:
                        if (!args[i].IsNumber) return null; break;
                    case JsDataType.DateTime:
                        if (!args[i].IsDateTime) return null; break;
                    case JsDataType.JavaScriptArray:
                        if (!args[i].IsArray) return null; break;
                }
                // 转换参数
                switch (argType)
                {
                    case JsDataType.String:
                        result.Add(args[i].GetString());
                        break;
                    case JsDataType.Bool:
                        result.Add(args[i].GetBool());
                        break;
                    case JsDataType.Int16:
                        result.Add(Convert.ToInt16(args[i].GetInt()));
                        break;
                    case JsDataType.Int32:
                        result.Add(args[i].GetInt());
                        break;
                    case JsDataType.Int64:
                        result.Add(Convert.ToInt64(args[i].GetInt()));
                        break;
                    case JsDataType.Single:
                        result.Add(Convert.ToSingle(args[i].GetDouble()));
                        break;
                    case JsDataType.Double:
                        result.Add(args[i].GetDouble());
                        break;
                    case JsDataType.DateTime:
                        result.Add(args[i].GetDateTime());
                        break;
                    case JsDataType.JavaScriptArray:
                        result.Add(args[i].ToArray());
                        break;
                    case JsDataType.IWin32Window:
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

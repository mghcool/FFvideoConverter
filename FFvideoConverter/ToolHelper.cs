using FFvideoConverter.Model;
using NetDimension.NanUI.JavaScript;
using Newtonsoft.Json.Linq;
namespace FFvideoConverter
{
    public static class ToolHelper
    {
        /// <summary>
        /// Type类型转枚举
        /// </summary>
        /// <param name="type">数据类型</param>
        /// <returns></returns>
        public static T TypeToEnum<T>(Type type)
        {
            return (T)Enum.Parse(typeof(T), type.Name);
        }

        /// <summary>
        /// Js对象转为C#对象
        /// </summary>
        /// <returns></returns>
        public static T ToObject<T>(this JavaScriptObject jsObj)
        {
            JObject jobj = new();
            foreach(string key in jsObj.PropertyNames)
            {
                JavaScriptValue value = jsObj[key];
                if(value.IsString) jobj.Add(key, value.GetString());
                else if (value.IsBool) jobj.Add(key, value.GetBool());
                else if (value.IsNumber) jobj.Add(key, value.GetDouble());
                else if (value.IsDateTime) jobj.Add(key, value.GetDateTime());
            }
            return jobj.ToObject<T>();
            //T ret = Activator.CreateInstance<T>();
            //Type jsObjType = typeof(T);
            //PropertyInfo[] properties = jsObjType.GetProperties();
            //foreach (PropertyInfo property in properties)
            //{
            //    string key = property.Name;
            //    string type = property.PropertyType.Name;
            //    JavaScriptValue value = jsObj[key];
            //    switch (type)
            //    {
            //        case "String": ret.SetPropertyValue(key, value.GetString()); break;
            //        case "Boolean": ret.SetPropertyValue(key, value.GetBool()); break;
            //        case "Int16": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "UInt16": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "Int32": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "UInt32": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "Int64": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "UInt64": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "Single": ret.SetPropertyValue(key, value.GetInt()); break;
            //        case "Double": ret.SetPropertyValue(key, value.GetInt()); break;
            //    }
            //}
            //return (T)ret;
        }
    }
}

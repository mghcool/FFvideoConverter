namespace FFvideoConverter
{
    /// <summary>
    /// js注册对象属性和方法类型特性类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public class JsObjectType : Attribute
    {
        /// <summary>
        /// 方法类型
        /// </summary>
        public enum MethodType 
        {
            /// <summary>
            /// 同步方法
            /// </summary>
            Sync,
            /// <summary>
            /// 异步方法
            /// </summary>
            Async 
        }

        /// <summary>
        /// 属性类型
        /// </summary>
        public enum PropertieType 
        {
            /// <summary>
            /// 只读属性
            /// </summary>
            ReadOnly,
            /// <summary>
            /// 可读写属性
            /// </summary>
            ReadWrite,
            /// <summary>
            /// 常量(不可变)
            /// </summary>
            Const
        }

        /// <summary>
        /// js方法类型
        /// </summary>
        public MethodType? Method { get; set; }

        /// <summary>
        /// js属性类型
        /// </summary>
        public PropertieType? Propertie { get; set; }

        /// <summary>
        /// 使用窗口句柄
        /// </summary>
        public bool UsedWindowHwnd { get; set; }

        /// <summary>
        /// 标记js方法类型特性
        /// </summary>
        /// <param name="methodType">js方法类型</param>
        /// <param name="usedWindowHwnd">是否使用窗口句柄</param>
        public JsObjectType(MethodType methodType, bool usedWindowHwnd = false)
        {
            Method = methodType;
            UsedWindowHwnd = usedWindowHwnd;
        }

        /// <summary>
        /// 标记js属性类型特性
        /// </summary>
        /// <param name="propertieType">js属性类型</param>
        /// <param name="usedWindowHwnd">是否使用窗口句柄</param>
        public JsObjectType(PropertieType propertieType, bool usedWindowHwnd = false)
        {
            Propertie = propertieType;
            UsedWindowHwnd = usedWindowHwnd;
        }
    }
}

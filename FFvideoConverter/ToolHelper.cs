using FFvideoConverter.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFvideoConverter
{
    public static class ToolHelper
    {
        public static T TypeToEnum<T>(Type type)
        {
            return (T)Enum.Parse(typeof(T), type.Name);
        }

    }
}

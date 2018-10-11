using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundAudioShare
{
    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}

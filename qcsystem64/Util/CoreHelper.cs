using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace qcsystem64
{
    public class CoreHelper
    {
        public static JsonSerializerOptions _JsonSetting;
        public static JsonSerializerOptions JsonSetting
        {
            get
            {
                if (_JsonSetting != null) return _JsonSetting;
                _JsonSetting = new JsonSerializerOptions();
                setJsonSerializerOptions(_JsonSetting);

                return _JsonSetting;
            }
        }
        public static void setJsonSerializerOptions(JsonSerializerOptions jset) {
            jset.Converters.Add(new DateTimeJsonConverter("yyyy/MM/dd HH:mm:ss"));
            jset.Converters.Add(new BoolJsonConverter());
            jset.PropertyNamingPolicy = null;
            jset.DictionaryKeyPolicy = null;
            jset.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        }
        public static T ToObj<T>(string str) where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(str, JsonSetting);

        }
        /// <summary>
        /// 将字符串转化为对象
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDic(string str)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(str, JsonSetting);
        }
        public static string ToJson(object obj)
        {
            return JsonSerializer.Serialize(obj, JsonSetting);
        }
    }
}

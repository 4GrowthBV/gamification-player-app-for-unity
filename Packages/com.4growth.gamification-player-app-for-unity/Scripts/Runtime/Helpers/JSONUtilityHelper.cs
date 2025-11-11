using Newtonsoft.Json;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JSONUtilityHelper
    {
        public static string ToJson(this object toJSON, bool pretty = false)
        {
            var json = JsonConvert.SerializeObject(toJSON);

            return json;
        }

        public static TType FromJson<TType>(this string json, bool changeEmptyArrayToObject = true)
        {
            var obj = JsonConvert.DeserializeObject<TType>(json);

            return obj;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JSONUtilityHelper
    {
        public static string ToJson(this object toJSON, bool pretty = false)
        {
            var json = JsonConvert.SerializeObject(toJSON);

            var newJSON = json.Replace("\"\"", "null");

            return newJSON;
        }

        public static TType FromJson<TType>(this string json)
        {
            var newJSON = json.Replace("null", "\"\"");

            newJSON = newJSON.Replace("[]", "{}");

            var obj = JsonConvert.DeserializeObject<TType>(newJSON);

            return obj;
        }
    }
}

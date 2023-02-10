using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JSONUtilityHelper
    {
        public static string ToJson(this object toJSON, bool pretty = false)
        {
            var json = JsonUtility.ToJson(toJSON, pretty);

            var newJSON = json.Replace("\"\"", "null");

            return newJSON;
        }

        public static TType FromJson<TType>(this string json)
        {
            var newJSON = json.Replace("null", "\"\"");

            var obj = JsonUtility.FromJson<TType>(newJSON);

            return obj;
        }
    }
}

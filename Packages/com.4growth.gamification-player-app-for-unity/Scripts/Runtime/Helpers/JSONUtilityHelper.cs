using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JSONUtilityHelper
    {
        public static string ToJson(this object toJSON)
        {
            var json = JsonUtility.ToJson(toJSON);

            var newJSON = json.Replace("\"\"", "null");

            return newJSON;
        }

        public static TType FromJson<TType>(this string json)
        {
            json.Replace("null", "\"\"");

            var obj = JsonUtility.FromJson<TType>(json);

            return obj;
        }
    }
}

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
            var newJSON = json.Replace("\"user_is_demo\":null,", "\"user_is_demo\":false,");

            newJSON = newJSON.Replace("\"organisation_allow_upgrade_to_registered_user\":null,", "\"organisation_allow_upgrade_to_registered_user\":false,");
            
            newJSON = newJSON.Replace("null", "\"\"");

            newJSON = newJSON.Replace("[]", "{}");

            var obj = JsonConvert.DeserializeObject<TType>(newJSON);

            return obj;
        }
    }
}

using Newtonsoft.Json;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JSONUtilityHelper
    {
        public static string ToJson(this object toJSON, bool pretty = false)
        {
            var json = JsonConvert.SerializeObject(toJSON);

            //var newJSON = json.Replace("\"\"", "null");

            return json;
        }

        public static TType FromJson<TType>(this string json, bool changeEmptyArrayToObject = true)
        {
            /*            
            newJSON = newJSON.Replace("null", "\"\"");

            if(changeEmptyArrayToObject)
            {
                newJSON = newJSON.Replace("[]", "{}");
            }*/

            //Debug.Log($"Deserializing JSON: {json}");

            var obj = JsonConvert.DeserializeObject<TType>(json);

            return obj;
        }
    }
}

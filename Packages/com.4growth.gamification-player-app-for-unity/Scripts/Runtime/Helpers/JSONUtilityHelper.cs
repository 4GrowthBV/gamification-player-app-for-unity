using Newtonsoft.Json;

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
            var newJSON = json.Replace("\"user_is_demo\":null,", "\"user_is_demo\":false,");

            newJSON = newJSON.Replace("\"organisation_allow_upgrade_to_registered_user\":null,", "\"organisation_allow_upgrade_to_registered_user\":false,");
            
            newJSON = newJSON.Replace("\"micro_game_id\":null", "\"micro_game_id\":\"\"");

            newJSON = newJSON.Replace("null", "\"\"");

            if(changeEmptyArrayToObject)
            {
                newJSON = newJSON.Replace("[]", "{}");
            }*/

            var obj = JsonConvert.DeserializeObject<TType>(json);

            return obj;
        }
    }
}

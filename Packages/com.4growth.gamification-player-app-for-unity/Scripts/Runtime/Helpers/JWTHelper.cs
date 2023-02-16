using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using UnityEngine;

namespace GamificationPlayer
{
    public static class JWTHelper
    {
        public static string GetJSONWebTokenPayload(string token, string secret)
        {
            string json;
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(serializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            var algorithm = new HMACSHA256Algorithm();
            var decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);

            if(string.IsNullOrEmpty(secret))
            {
                Debug.LogWarning("JWT Secret is not set! This is a security vulnerability, please set it before going live!");
                json = decoder.Decode(token, false);
                return json;
            }
            
            json = decoder.Decode(token, secret, true);
            return json;
        }
    }
}

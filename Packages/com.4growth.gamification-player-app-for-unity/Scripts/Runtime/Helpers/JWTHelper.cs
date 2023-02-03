using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace GamificationPlayer
{
    public static class JWTHelper
    {
        public static string GetJSONWebTokenPayload(string token, string secret)
        {
            var serializer = new JsonNetSerializer();
            var provider = new UtcDateTimeProvider();
            var validator = new JwtValidator(serializer, provider);
            var urlEncoder = new JwtBase64UrlEncoder();
            var algorithm = new HMACSHA256Algorithm();
            var decoder = new JwtDecoder(serializer, validator, urlEncoder, algorithm);
            
            var json = decoder.Decode(token, secret, true);

            return json;
        }
    }
}

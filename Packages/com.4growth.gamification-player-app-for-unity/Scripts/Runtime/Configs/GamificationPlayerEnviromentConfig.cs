using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "GamificationPlayerEnviromentConfig", menuName = "GamificationPlayer/GamificationPlayerEnviromentConfig", order = 1)]
    public class GamificationPlayerEnviromentConfig : ScriptableObject
    {
        public string APIKey
        {
            get
            {
                return apiKey;
            }
        }

        public string API_URL
        {
            get
            {
                return apiURL;
            }
        }

        public string Webpage
        {
            get
            {
                return webpage;
            }
        }

        public string JSONWebTokenSecret
        {
            get
            {
                return jsonWebTokenSecret;
            }
        }

        public bool IsMockServer
        {
            get
            {
                return isMockServer;
            }
        }

        public bool TurnOnLogging
        {
            get
            {
                return turnOnLogging;
            }
        }

        [SerializeField]
        private string apiKey = "123";

        [SerializeField]
        private string apiURL = "https://stoplight.io/mocks/vdhicts/gamification-player-api-spec/55579676";

        [SerializeField]
        private string webpage = "gamificationplayer.it/";

        [SerializeField]
        private string jsonWebTokenSecret = "";

        [SerializeField]
        private bool isMockServer;

        [SerializeField]
        private bool turnOnLogging;
    }
}
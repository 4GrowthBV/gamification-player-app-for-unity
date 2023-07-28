using System.Linq;
using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "GamificationPlayer/EnvironmentConfig", order = 1)]
    public class EnvironmentConfig : ScriptableObject
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

        public TextAsset[] MockDTOs
        {
            get
            {
                return mockDTOs;
            }
        }

        public bool TryGetMockDTO<TType>(out TType dto)
        {
            dto = default;
            
            if(mockDTOs == null)
            {
                return false;
            }

            var name = typeof(TType);
            var json = mockDTOs.FirstOrDefault(t => t.name == typeof(TType).ToString());

            if(json != null)
            {
                dto = json.text.FromJson<TType>();
                return true;
            }
            
            return false;
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

        [SerializeField]
        private TextAsset[] mockDTOs;
    }
}
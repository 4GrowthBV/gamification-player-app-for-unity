using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "GamificationPlayerConfig", menuName = "GamificationPlayer/GamificationPlayerConfig", order = 1)]
    public class GamificationPlayerConfig : ScriptableObject
    {
        public static GamificationPlayerEnviromentConfig EnviromentConfig
        {
            get
            {
#if PROD_BUILD
                return instance.productionConfig;
#elif STAG_BUILD
                return instance.stagingConfig;
#else
                return instance.devConfig;
#endif
            }
        }

        private static GamificationPlayerConfig s_Instance;

        private static GamificationPlayerConfig instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<GamificationPlayerConfig>("GamificationPlayerConfig");
                }

                return s_Instance;
            }
        }

        [SerializeField]
        private GamificationPlayerEnviromentConfig devConfig;

        [SerializeField]
        private GamificationPlayerEnviromentConfig stagingConfig;

        [SerializeField]
        private GamificationPlayerEnviromentConfig productionConfig;
    }
}
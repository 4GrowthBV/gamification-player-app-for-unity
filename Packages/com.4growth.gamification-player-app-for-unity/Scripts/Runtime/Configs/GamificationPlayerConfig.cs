using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "GamificationPlayerConfig", menuName = "GamificationPlayer/GamificationPlayerConfig", order = 1)]
    public class GamificationPlayerConfig : ScriptableObject
    {
        public static bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            if (instance.defaultEnvironmentDomainGroup != null && instance.defaultEnvironmentDomainGroup.TryGetEnvironmentConfig(environmentDomain, out EnvironmentConfig config))
            {
                gamificationPlayerEnvironmentConfig = config;

                return true;
            }

            foreach (var gamificationPlayerConfig in instance.allEnvironmentDomainGroups)
            {
                if(gamificationPlayerConfig != null && gamificationPlayerConfig.TryGetEnvironmentConfig(environmentDomain, out config))
                {
                    gamificationPlayerEnvironmentConfig = config;
                    return true;
                }
            }

            gamificationPlayerEnvironmentConfig = default;
            
            return false;
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
        private EnvironmentDomainGroup defaultEnvironmentDomainGroup;

        [SerializeField]
        private EnvironmentDomainGroup[] allEnvironmentDomainGroups;
    }
}
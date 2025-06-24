using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "GamificationPlayerConfig", menuName = "GamificationPlayer/GamificationPlayerConfig", order = 1)]
    public class GamificationPlayerConfig : ScriptableObject
    {
        public static bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            Debug.Log($"GamificationPlayerConfig::TryGetEnvironmentConfig::environmentDomain {environmentDomain}");

            if (instance.defaultEnvironmentDomainGroup != null && instance.defaultEnvironmentDomainGroup.TryGetEnvironmentConfig(environmentDomain, out EnvironmentConfig config))
            {
                gamificationPlayerEnvironmentConfig = config;

                Debug.Log($"GamificationPlayerConfig::TryGetEnvironmentConfig::defaultEnvironmentDomainGroup found config for {environmentDomain}");

                return true;
            }

            foreach (var gamificationPlayerConfig in instance.allEnvironmentDomainGroups)
            {
                if (gamificationPlayerConfig != null && gamificationPlayerConfig.TryGetEnvironmentConfig(environmentDomain, out config))
                {
                    Debug.Log($"GamificationPlayerConfig::TryGetEnvironmentConfig::found config for {environmentDomain} in {gamificationPlayerConfig.name}");
                    gamificationPlayerEnvironmentConfig = config;
                    return true;
                }

                Debug.Log($"GamificationPlayerConfig::TryGetEnvironmentConfig::no config found for {environmentDomain} in {gamificationPlayerConfig.name}");
            }

            Debug.Log($"GamificationPlayerConfig::TryGetEnvironmentConfig::no config found for {environmentDomain}");
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
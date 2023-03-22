using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "GamificationPlayerConfig", menuName = "GamificationPlayer/GamificationPlayerConfig", order = 1)]
    public class GamificationPlayerConfig : ScriptableObject
    {
        public static bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            Debug.Log(instance);
            Debug.Log(instance.defaultEnvironmentDomainGroup);

            EnvironmentConfig config = default;
            if(instance.defaultEnvironmentDomainGroup ?? instance.defaultEnvironmentDomainGroup.TryGetEnvironmentConfig(environmentDomain, out config))
            {
                gamificationPlayerEnvironmentConfig = config;

                return true;
            }

            foreach(var gamificationPlayerConfig in instance.allEnvironmentDomainGroups)
            {
                if(gamificationPlayerConfig ?? gamificationPlayerConfig.TryGetEnvironmentConfig(environmentDomain, out config))
                {
                    gamificationPlayerEnvironmentConfig = config;
                    return true;
                }
            }

            gamificationPlayerEnvironmentConfig = default;
            
            return false;
        }

        public static string GetStartingSubdomain()
        {
            return instance.startingSubdomain;
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
        [Header("Keep empty to make use of the generic login.")]
        private string startingSubdomain = "";

        [SerializeField]
        private EnvironmentDomainGroup defaultEnvironmentDomainGroup;

        [SerializeField]
        private EnvironmentDomainGroup[] allEnvironmentDomainGroups;
    }
}
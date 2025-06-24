using System.Linq;
using UnityEngine;

namespace GamificationPlayer
{
    [CreateAssetMenu(fileName = "EnvironmentDomainGroup", menuName = "GamificationPlayer/EnvironmentDomainGroup", order = 1)]
    public class EnvironmentDomainGroup : ScriptableObject
    {
        public bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            foreach (var config in configs)
            {
                if (environmentDomain.ToLower().Contains(config.Webpage.ToLower()) ||
                    config.Webpage.ToLower().Contains(environmentDomain.ToLower()))
                {
                    Debug.Log($"EnvironmentDomainGroup::TryGetEnvironmentConfig::found config for {environmentDomain} in {config.name}");
                    gamificationPlayerEnvironmentConfig = config;

                    return true;
                }
                
                Debug.Log($"EnvironmentDomainGroup::TryGetEnvironmentConfig::no config found for {environmentDomain} in {config.name}");
            }

            Debug.Log($"EnvironmentDomainGroup::TryGetEnvironmentConfig::no config found for {environmentDomain}");

            gamificationPlayerEnvironmentConfig = default;

            return false;
        }

        [SerializeField]
        private EnvironmentConfig[] configs;
    }
}
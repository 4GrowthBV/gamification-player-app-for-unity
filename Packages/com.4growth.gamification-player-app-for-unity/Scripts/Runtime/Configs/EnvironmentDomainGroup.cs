using System.Linq;
using UnityEngine;

namespace GamificationPlayer
{

    [CreateAssetMenu(fileName = "EnvironmentDomainGroup", menuName = "GamificationPlayer/EnvironmentDomainGroup", order = 1)]
    public class EnvironmentDomainGroup : ScriptableObject
    {
        public bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            foreach(var config in configs)
            {
                if(environmentDomain.ToLower().Contains(config.Webpage.ToLower()) || 
                    config.Webpage.ToLower().Contains(environmentDomain.ToLower()))
                {
                    gamificationPlayerEnvironmentConfig = config;

                    return true; 
                }
            }

            gamificationPlayerEnvironmentConfig = default;

            return false;
        }

        [SerializeField]
        private EnvironmentConfig[] configs;
    }
}
using System.Linq;
using UnityEngine;

namespace GamificationPlayer
{

    [CreateAssetMenu(fileName = "EnvironmentDomainGroup", menuName = "GamificationPlayer/EnvironmentDomainGroup", order = 1)]
    public class EnvironmentDomainGroup : ScriptableObject
    {
        public bool TryGetEnvironmentConfig(string environmentDomain, out EnvironmentConfig gamificationPlayerEnvironmentConfig)
        {
            Debug.Log("SEARCH: " + environmentDomain);
            foreach(var config in configs)
            {
                Debug.Log("CHECK: " + config.Webpage.ToLower());

                if(environmentDomain.ToLower().Contains(config.Webpage.ToLower()) || 
                    config.Webpage.ToLower().Contains(environmentDomain.ToLower()))
                {
                    Debug.Log("FOUND!!: " + config.name);

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
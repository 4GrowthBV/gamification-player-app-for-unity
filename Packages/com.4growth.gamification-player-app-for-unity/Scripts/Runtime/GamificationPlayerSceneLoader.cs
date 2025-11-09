using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GamificationPlayer
{
    public class GamificationPlayerSceneLoader : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod]
        public static void OnRuntimeMethodLoad()
        {
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath("GamificationPlayer");

            if(sceneIndex != -1)
            {
                if(SceneManager.GetAllScenes().Any(s => s.name != "GamificationPlayer"))
                {
                    //SceneManager.LoadSceneAsync("GamificationPlayer", LoadSceneMode.Additive);
                }
            }
        }
    }
}
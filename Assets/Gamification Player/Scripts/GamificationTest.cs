using GamificationPlayer;
using UnityEngine;
//using Vuplex.WebView;

public class GamificationTest : MonoBehaviour
{
    // Prefab for a Vuplex WebView object
    [SerializeField]
    //private BaseWebViewPrefab baseWebViewPrefab;

    async void Start()
    {
        // Wait until the Vuplex WebView object is fully initialized
        //await baseWebViewPrefab.WaitUntilInitialized();

        // Register a handler for messages emitted by the Vuplex JavaScript API
        //baseWebViewPrefab.WebView.MessageEmitted += OnMessage;

        // Uncomment this for testing with a mock server instead of the production server
        // GamificationPlayerManager.UseMockServer();

        // Register a handler for events emitted by the Gamification Player
        GamificationPlayerManager.OnEvent += (message) =>
        {
            Debug.Log(message);
        };

        // Register a handler for when a module (e.g. an exergame) starts
        GamificationPlayerManager.OnModuleStart += (moduleId) =>
        {
            Debug.Log("Module id: " + moduleId);

            // Try to get the ID of the currently active module
            GamificationPlayerManager.TryGetLatestModuleId(out var id);
            Debug.Log("Module id via TryGetActiveModuleId: " + id);

            // Check if a module session is currently active
            Debug.Log("Module is active: " + GamificationPlayerManager.IsMicroGameActive());

            // End the current module session with a score of 777 and report the score to the Gamification Player
            GamificationPlayerManager.StopMicroGame(777, true);
        };

        // Register a handler for when a new page is fully loaded in the WebView
        GamificationPlayerManager.OnPageView += () =>
        {
            // Check if a user is currently active
            if (GamificationPlayerManager.IsUserActive())
            {
                // Try to get the ID of the currently active user
                GamificationPlayerManager.TryGetActiveUserId(out var id);
                Debug.Log("New page is fully loaded, user on this page: " + id);
            }
            else
            {
                Debug.Log("New page is fully loaded, no user is active on this page");
            }
        };
    }

    /*
    // Handler for messages emitted by the Vuplex JavaScript API
    private void OnMessage(object sender, EventArgs<string> eventArgs)
    {
        // Process the message by passing it to the Gamification Player
        GamificationPlayerManager.ProcessExternalMessage(eventArgs.Value);
    }*/
}

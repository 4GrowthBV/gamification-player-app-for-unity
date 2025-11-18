using System;
using UnityEngine;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using GamificationPlayer.DTO.ExternalEvents;
using Vuplex.WebView;

namespace GamificationPlayer.TestBed
{
    /// <summary>
    /// ChatManager Vuplex Test Bed - Bridges ChatManager with HTML/JavaScript frontend via Vuplex WebView
    /// This test bed exposes ChatManager functionality to web-based UIs through JavaScript events
    /// </summary>
    public class ChatManagerVuplexTestBed : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] 
        private bool enableLogging = true;

        [SerializeField] 
        private EnvironmentConfig environmentConfig;

        [SerializeField]
        private VuplexBridge vuplexBridge;
        
        private string htmlPageUrl = "http://localhost:8000/VuplexBridge-Example.html";
        
        [Header("Vuplex WebView")]
        [SerializeField] 
        private CanvasWebViewPrefab webViewPrefab;

        private ChatManager chatManager;
        private GamificationPlayerEndpoints endpoints;
        private ISessionLogData sessionData;
        private IWebView webView;

        private IChatAIService aiService;

        async void Awake()
        {
            await webViewPrefab.WaitUntilInitialized();

            webView = webViewPrefab.WebView;
            
            Debug.Log($"Loading HTML page: {htmlPageUrl}");

            webView.LoadUrl(htmlPageUrl);
            
            await webView.WaitForNextPageLoadToFinish();            
        }

        async void Start()
        {
            // Initialize ChatManager components
            InitializeTestBed();

            vuplexBridge.StartChatInitialization(chatManager, 
                aiService,
                new ChatManager.ResumeConversationMetadata(),
                new ChatManager.InitialMetadata("AI Chat Buddy", "Frank", DateTime.Now)
            );
        }

        private void InitializeTestBed()
        {
            // Setup services
            SetupServiceOptions();
            
            // Initialize core components
            InitializeCoreComponents();
            
            Debug.Log("Test Bed initialized successfully");      
        }

        private void SetupServiceOptions()
        {
            aiService = new ChatAIService("", isLoggingEnabled: enableLogging);
        }

        private void InitializeCoreComponents()
        {
            // Create mock session data
            sessionData = new SessionLogData();

            sessionData.AddToLog(new MicroGamePayload
            {
                player = new MicroGamePayload.Player
                {
                    user_id = "5b411dd2-20c1-49dd-90a5-555dbaead5f8",
                },
                organisation = new MicroGamePayload.Organisation
                {
                    id = "edb5e165-1c74-44f8-8d57-c24b82f2f5f2",
                },
                micro_game = new MicroGamePayload.MicroGame
                {
                    id = "99d75cfb-ce23-4939-a755-013d04a435c8",
                }
            });
            
            // Create Gamification Player endpoints
            if (environmentConfig != null)
            {
                endpoints = new GamificationPlayerEndpoints(environmentConfig, sessionData);
            }
            
            // Create ChatManager
            chatManager = gameObject.AddComponent<ChatManager>();
            if (endpoints != null && sessionData != null)
            {
                chatManager.IsLogging = enableLogging;
                chatManager.Initialize(endpoints, sessionData);
            }
        }
    }
}
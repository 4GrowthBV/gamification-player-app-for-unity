using System.Collections;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace GamificationPlayer.Tests
{
    public class ChatManagerTest
    {
        private ChatManager chatManager;
        private GamificationPlayerEndpoints mockEndpoints;
        private ISessionLogData mockSessionData;
        private EnvironmentConfig environmentConfig;

        private IChatRouterService chatRouterService;
        private IChatAIService chatAIService;

        [SetUp]
        public void TestSetup()
        {
            LoadEnvironmentConfig();

            // Create mock dependencies directly without using GamificationPlayerManager
            mockSessionData = new SessionLogDataMock();

            mockEndpoints = new GamificationPlayerEndpoints(environmentConfig, mockSessionData);

            // Create a fresh ChatManager instance for testing
            GameObject go = new GameObject("ChatManager");
            chatManager = go.AddComponent<ChatManager>();

            // Set mock services
            chatRouterService = go.AddComponent<N8nRouterMockService>();
            chatAIService = go.AddComponent<OpenAIChatMockService>();

            // Initialize ChatManager with mock dependencies
            chatManager.Initialize(mockEndpoints, mockSessionData);
        }
        
        private void LoadEnvironmentConfig()
        {
            // Try to find existing EnvironmentConfig asset
            var configs = AssetDatabase.FindAssets("t:EnvironmentConfig GamificationPlayerEnviromentConfigMock");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                environmentConfig = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(path);
            }
        }

        [TearDown]
        public void TestTearDown()
        {
            // Clean up after each test
            if (chatManager != null && chatManager.gameObject != null)
            {
                // Stop any running coroutines to prevent them from accessing destroyed objects
                chatManager.StopAllCoroutines();
                UnityEngine.Object.DestroyImmediate(chatManager.gameObject);
            }
        }

        #region ChatManager Tests

        [UnityTest]
        public IEnumerator ChatManager_Initialize_ShouldSetupCorrectly()
        {
            // Act & Assert
            Assert.IsTrue(chatManager.IsInitialized(), "ChatManager should be initialized after setup");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ChatManager_InitializeChat_ShouldCompleteSuccessfully()
        {
            // Arrange
            bool chatInitialized = false;
            bool errorOccurred = false;
            string errorMessage = "";

            System.Action onInitialized = () => chatInitialized = true;
            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            ChatManager.OnChatInitialized += onInitialized;
            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act
                chatManager.InitializeChat(chatAIService);

                // Wait for initialization to complete (max 10 seconds)
                float timeout = 10f;
                float elapsed = 0f;
                while (!chatInitialized && !errorOccurred && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Assert
                Assert.IsFalse(errorOccurred, $"Chat initialization should not fail. Error: {errorMessage}");
                Assert.IsTrue(chatInitialized, "Chat should be initialized successfully");
                Assert.Less(elapsed, timeout, "Chat initialization should complete within timeout");
            }
            finally
            {
                // Cleanup
                ChatManager.OnChatInitialized -= onInitialized;
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_HandleUserMessage_ShouldGenerateAIResponse()
        {
            // Arrange
            bool aiResponseReceived = false;
            bool errorOccurred = false;
            string aiResponse = "";
            string errorMessage = "";

            System.Action<string> onAIResponse = (response) => { aiResponseReceived = true; aiResponse = response; };
            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnAIMessageReceived += onAIResponse;
            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "Hello, how are you?");

                // Wait for AI response (max 15 seconds)
                float timeout = 15f;
                float elapsed = 0f;
                while (!aiResponseReceived && !errorOccurred && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Assert
                Assert.IsFalse(errorOccurred, $"User message handling should not fail. Error: {errorMessage}");
                Assert.IsTrue(aiResponseReceived, "AI response should be received");
                Assert.IsNotEmpty(aiResponse, "AI response should not be empty");
                Assert.Less(elapsed, timeout, "AI response should be generated within timeout");
            }
            finally
            {
                // Cleanup
                ChatManager.OnAIMessageReceived -= onAIResponse;
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_HandleButtonClick_ShouldLoadPredefinedMessage()
        {
            // Arrange
            bool messageReceived = false;
            bool errorOccurred = false;
            string receivedMessage = "";
            string[] receivedButtons = null;
            string errorMessage = "";

            System.Action<string, string[]> onMessage = (msg, buttons) => { 
                messageReceived = true; 
                receivedMessage = msg; 
                receivedButtons = buttons; 
            };
            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnMessageReceived += onMessage;
            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act
                chatManager.HandleButtonClick(chatAIService, "test_button");

                // Wait for predefined message (max 10 seconds)
                float timeout = 10f;
                float elapsed = 0f;
                while (!messageReceived && !errorOccurred && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Assert
                Assert.IsFalse(errorOccurred, $"Button click handling should not fail. Error: {errorMessage}");
                Assert.IsTrue(messageReceived, "Predefined message should be received");
                Assert.IsNotEmpty(receivedMessage, "Received message should not be empty");
                Assert.Less(elapsed, timeout, "Predefined message should load within timeout");
            }
            finally
            {
                // Cleanup
                ChatManager.OnMessageReceived -= onMessage;
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_HandleEmptyMessage_ShouldReturnError()
        {
            // Arrange
            bool errorOccurred = false;
            string errorMessage = "";

            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "");

                // Wait a moment for error handling
                yield return new WaitForSeconds(0.1f);

                // Assert
                Assert.IsTrue(errorOccurred, "Empty message should trigger error");
                Assert.IsTrue(errorMessage.Contains("empty"), "Error message should mention empty message");
            }
            finally
            {
                // Cleanup
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_HandleNullMessage_ShouldReturnError()
        {
            // Arrange
            bool errorOccurred = false;
            string errorMessage = "";

            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act
                chatManager.HandleUserMessage(chatAIService, chatRouterService, null);

                // Wait a moment for error handling
                yield return new WaitForSeconds(0.1f);

                // Assert
                Assert.IsTrue(errorOccurred, "Null message should trigger error");
                Assert.IsTrue(errorMessage.Contains("empty"), "Error message should mention empty message");
            }
            finally
            {
                // Cleanup
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_MultipleUserMessages_ShouldHandleSequentially()
        {
            // Arrange
            int aiResponseCount = 0;
            bool errorOccurred = false;
            string errorMessage = "";

            System.Action<string> onAIResponse = (response) => aiResponseCount++;
            System.Action<string> onError = (msg) => { errorOccurred = true; errorMessage = msg; };

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnAIMessageReceived += onAIResponse;
            ChatManager.OnErrorOccurred += onError;

            try
            {
                // Act - Send multiple messages
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "First message");
                yield return new WaitForSeconds(0.5f);
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "Second message");
                yield return new WaitForSeconds(0.5f);
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "Third message");

                // Wait for all responses (max 20 seconds)
                float timeout = 20f;
                float elapsed = 0f;
                while (aiResponseCount < 3 && !errorOccurred && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Assert
                Assert.IsFalse(errorOccurred, $"Multiple message handling should not fail. Error: {errorMessage}");
                Assert.AreEqual(3, aiResponseCount, "Should receive 3 AI responses");
                Assert.Less(elapsed, timeout, "All responses should be generated within timeout");
            }
            finally
            {
                // Cleanup
                ChatManager.OnAIMessageReceived -= onAIResponse;
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_InitializationFlow_ShouldLoadDayOneMessage()
        {
            // Arrange
            bool messageReceived = false;
            bool chatInitialized = false;
            string receivedMessage = "";

            System.Action<string, string[]> onMessage = (msg, buttons) => { 
                messageReceived = true; 
                receivedMessage = msg; 
            };
            System.Action onInitialized = () => chatInitialized = true;

            ChatManager.OnMessageReceived += onMessage;
            ChatManager.OnChatInitialized += onInitialized;

            try
            {
                // Act
                chatManager.InitializeChat(chatAIService);

                // Wait for initialization and day_one message (max 15 seconds)
                float timeout = 15f;
                float elapsed = 0f;
                while (!chatInitialized && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // Assert
                Assert.IsTrue(chatInitialized, "Chat should be initialized");
                Assert.IsTrue(messageReceived, "Day one message should be received during initialization");
                Assert.IsNotEmpty(receivedMessage, "Day one message should not be empty");
            }
            finally
            {
                // Cleanup
                ChatManager.OnMessageReceived -= onMessage;
                ChatManager.OnChatInitialized -= onInitialized;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_Performance_InitializationShouldBeReasonablyFast()
        {
            // Arrange
            bool chatInitialized = false;
            float startTime = Time.realtimeSinceStartup;

            System.Action onInitialized = () => chatInitialized = true;
            ChatManager.OnChatInitialized += onInitialized;

            try
            {
                // Act
                chatManager.InitializeChat(chatAIService);

                // Wait for initialization
                while (!chatInitialized)
                {
                    yield return null;
                }

                float elapsed = Time.realtimeSinceStartup - startTime;

                // Assert - Initialization should be reasonably fast (under 5 seconds)
                Assert.Less(elapsed, 5f, $"Chat initialization should complete within 5 seconds. Actual: {elapsed:F2}s");
                
                // Log performance for monitoring
                Debug.Log($"Initialization performance: {elapsed:F2}s (Target: <5s)");
            }
            finally
            {
                // Cleanup
                ChatManager.OnChatInitialized -= onInitialized;
            }
        }

        [UnityTest]
        public IEnumerator ChatManager_Performance_UserMessageShouldBeReasonablyFast()
        {
            // Arrange
            bool aiResponseReceived = false;
            float startTime;

            System.Action<string> onAIResponse = (response) => aiResponseReceived = true;

            // Initialize chat first
            yield return InitializeChatAndWait();

            ChatManager.OnAIMessageReceived += onAIResponse;

            try
            {
                // Act
                startTime = Time.realtimeSinceStartup;
                chatManager.HandleUserMessage(chatAIService, chatRouterService, "Test performance message");

                // Wait for AI response
                while (!aiResponseReceived)
                {
                    yield return null;
                }

                float elapsed = Time.realtimeSinceStartup - startTime;

                // Assert - User message handling should be reasonably fast (under 5 seconds with mocks and backend calls)
                Assert.Less(elapsed, 5f, $"User message handling should complete within 5 seconds. Actual: {elapsed:F2}s");
                
                // Log performance for monitoring
                Debug.Log($"User message performance: {elapsed:F2}s (Target: <5s)");
            }
            finally
            {
                // Cleanup
                ChatManager.OnAIMessageReceived -= onAIResponse;
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to initialize chat and wait for completion
        /// </summary>
        private IEnumerator InitializeChatAndWait()
        {
            bool chatInitialized = false;
            bool errorOccurred = false;

            System.Action onInitialized = () => chatInitialized = true;
            System.Action<string> onError = (msg) => errorOccurred = true;

            ChatManager.OnChatInitialized += onInitialized;
            ChatManager.OnErrorOccurred += onError;

            try
            {
                chatManager.InitializeChat(chatAIService);

                float timeout = 10f;
                float elapsed = 0f;
                while (!chatInitialized && !errorOccurred && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                if (errorOccurred)
                {
                    Assert.Fail("Chat initialization failed during helper method");
                }

                if (!chatInitialized)
                {
                    Assert.Fail("Chat initialization timed out during helper method");
                }
            }
            finally
            {
                // Cleanup event handlers
                ChatManager.OnChatInitialized -= onInitialized;
                ChatManager.OnErrorOccurred -= onError;
            }
        }

        #endregion
    }
}

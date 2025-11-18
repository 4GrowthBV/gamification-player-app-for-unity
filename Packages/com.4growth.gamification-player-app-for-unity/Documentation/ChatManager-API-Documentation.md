# 📱 ChatManager API Documentation
### Frontend Developer's Guide to Chat Integration

This document provides complete API documentation for integrating the **ChatManager** into your Unity UI. The ChatManager is the client-side orchestrator for the Gamification Player chat system, handling conversation flows, AI interactions, and backend synchronization.

---

## 🏗️ Architecture Overview

### What ChatManager Does
- **State Management**: Manages conversation, profile, history, and flow types
- **API Orchestration**: Connects UI events to backend API calls
- **Flow Control**: Handles predefined (button-based) and AI (free-text) message flows
- **Event Broadcasting**: Emits events for UI layer consumption
- **Performance Optimization**: Caches messages and instructions for fast access

### What ChatManager Does NOT Do
- **UI Rendering**: Zero UI code - purely event-driven
- **Content Storage**: All messages come from backend APIs
- **AI Logic**: Delegates to external AI and RAG services
- **Authentication**: Handled by the endpoints layer

---

## 🎯 Quick Start Integration

### 1. Basic Setup

```csharp
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private ChatManager chatManager;
    [SerializeField] private IChatAIService aiService;
    [SerializeField] private IRAGService ragService;
    
    void Start()
    {
        // Initialize ChatManager with dependencies
        chatManager.Initialize(gamificationPlayerEndpoints, sessionData);
        
        // Subscribe to events BEFORE initializing chat
        SubscribeToEvents();
        
        // Initialize chat system
        chatManager.InitializeChat(aiService, ragService);
    }
}
```

### 2. Event Subscription

```csharp
private void SubscribeToEvents()
{
    ChatManager.OnChatInitialized += HandleChatInitialized;
    ChatManager.OnMessageReceived += HandleBotMessage;
    ChatManager.OnAIMessageReceived += HandleAIResponse;
    ChatManager.OnAIMessageChunkReceived += HandleStreamingChunk;
    ChatManager.OnErrorOccurred += HandleError;
}

private void OnDestroy()
{
    // Always unsubscribe to prevent memory leaks
    ChatManager.OnChatInitialized -= HandleChatInitialized;
    ChatManager.OnMessageReceived -= HandleBotMessage;
    ChatManager.OnAIMessageReceived -= HandleAIResponse;
    ChatManager.OnAIMessageChunkReceived -= HandleStreamingChunk;
    ChatManager.OnErrorOccurred -= HandleError;
}
```

---

## 📡 Events API

### Core Events

#### `OnChatInitialized`
Triggered when the chat system is fully initialized and ready.

```csharp
public static event Action OnChatInitialized;

private void HandleChatInitialized()
{
    // Enable chat input UI
    // Load conversation history if resuming
    // Show chat interface
    Debug.Log("Chat system ready!");
}
```

**When to Use**: Enable chat input fields, show conversation history, activate UI elements.

---

#### `OnMessageReceived`
Triggered when a predefined bot message is received (with optional buttons).

```csharp
public static event Action<ChatMessage> OnMessageReceived;

private void HandleBotMessage(ChatManager.ChatMessage message)
{
    // Display bot message
    AddMessageToUI(message.message, isFromBot: true);
    
    // Show buttons if available
    if (message.buttons != null && message.buttons.Length > 0)
    {
        ShowButtons(message.buttons);
    }
    else
    {
        HideButtons();
    }
}
```

**When to Use**: Display structured bot messages, show/hide button interfaces, update conversation UI.

---

#### `OnAIMessageReceived`
Triggered when an AI-generated response is received (free-text flow).

```csharp
public static event Action<ChatMessage> OnAIMessageReceived;

private void HandleAIResponse(ChatManager.ChatMessage message)
{
    // Display AI response (no buttons)
    AddMessageToUI(message.message, isFromBot: true);
    
    // Hide button interface since this is free-text mode
    HideButtons();
    
    // Show typing indicator off
    SetTypingIndicator(false);
}
```

**When to Use**: Display AI responses, hide button interfaces, manage typing indicators.

---

#### `OnAIMessageChunkReceived` (Optional)
Triggered during streaming AI responses for real-time updates.

```csharp
public static event Action<string> OnAIMessageChunkReceived;

private void HandleStreamingChunk(string chunk)
{
    // Update streaming message display
    AppendToStreamingMessage(chunk);
    
    // Show typing indicator
    SetTypingIndicator(true);
}
```

**When to Use**: Real-time typing effects, streaming message displays, enhanced UX.

---

#### `OnErrorOccurred`
Triggered when any error occurs during chat operations.

```csharp
public static event Action<string> OnErrorOccurred;

private void HandleError(string errorMessage)
{
    // Show error notification to user
    ShowErrorNotification(errorMessage);
    
    // Log for debugging
    Debug.LogError($"Chat Error: {errorMessage}");
    
    // Optionally retry or reset UI state
}
```

**When to Use**: Error notifications, debugging, fallback UI states.

---

## 🔧 Public Methods API

### Initialization

#### `Initialize(GamificationPlayerEndpoints, ISessionLogData)`
Sets up ChatManager with required dependencies.

```csharp
public void Initialize(GamificationPlayerEndpoints gamificationPlayerEndpoints, 
                      ISessionLogData sessionLogData)

// Example usage
chatManager.Initialize(endpoints, sessionData);

// Check if properly initialized
if (chatManager.IsInitialized())
{
    // Proceed with chat initialization
}
```

**Parameters**:
- `gamificationPlayerEndpoints`: Backend API endpoints instance
- `sessionLogData`: Session data for user context

**Must Call Before**: Any other ChatManager methods

---

#### `InitializeChat(IChatAIService, IRAGService, ...)`
Initializes the chat system with services and optional metadata.

```csharp
public void InitializeChat(IChatAIService aiService,
                          IRAGService ragService,
                          ResumeConversationMetadata resumeMetadata = null,
                          InitialMetadata initialMetadata = null,
                          bool forceNewConversation = false)

// Basic initialization
chatManager.InitializeChat(aiService, ragService);

// With initial user context
var initialMeta = new ChatManager.InitialMetadata(
    "Health Buddy",           // AI character name
    "John Doe",              // User name  
    DateTime.Now             // Start date
);
chatManager.InitializeChat(aiService, ragService, initialMetadata: initialMeta);

// Resume existing conversation with context
var resumeMeta = new ChatManager.ResumeConversationMetadata(
    "User returned after completing a workout session"
);
chatManager.InitializeChat(aiService, ragService, resumeMetadata: resumeMeta);

// Force new conversation
chatManager.InitializeChat(aiService, ragService, forceNewConversation: true);
```

**Parameters**:
- `aiService`: AI service for response generation and agent selection
- `ragService`: RAG service for document retrieval and context
- `resumeMetadata`: Optional context for resuming conversations
- `initialMetadata`: Optional initial context (AI character, user info)
- `forceNewConversation`: Forces new conversation/profile creation

**Triggers**: `OnChatInitialized` when complete

---

### Message Handling

#### `HandleUserMessage(IChatAIService, IRAGService, string)`
Processes user text input and generates AI responses.

```csharp
public void HandleUserMessage(IChatAIService aiService,
                             IRAGService ragService,
                             string userMessage)

// Example usage
private void OnSendButtonClicked()
{
    string userInput = messageInputField.text;
    if (!string.IsNullOrEmpty(userInput))
    {
        // Add user message to UI
        AddMessageToUI(userInput, isFromBot: false);
        
        // Clear input field
        messageInputField.text = "";
        
        // Process message through ChatManager
        chatManager.HandleUserMessage(aiService, ragService, userInput);
        
        // Show typing indicator
        SetTypingIndicator(true);
    }
}
```

**Parameters**:
- `aiService`: AI service for response generation
- `ragService`: RAG service for context retrieval
- `userMessage`: User's text input

**Triggers**: `OnAIMessageReceived` with AI response
**Flow**: Automatically switches to AI mode when user types

---

#### `HandleButtonClick(IChatAIService, string)`
Processes predefined flow button clicks.

```csharp
public void HandleButtonClick(IChatAIService aiService, string buttonIdentifier)

// Example button click handler
private void OnButtonClicked(Button clickedButton)
{
    string buttonId = clickedButton.name; // or store in button data
    
    // Disable buttons during processing
    DisableAllButtons();
    
    // Process button click
    chatManager.HandleButtonClick(aiService, buttonId);
}

// Example button creation from ChatMessage
private void ShowButtons(ChatManager.Button[] buttons)
{
    ClearButtons();
    
    foreach (var buttonData in buttons)
    {
        GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
        Button unityButton = buttonObj.GetComponent<Button>();
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        
        // Set button text (display text resolved from backend)
        buttonText.text = buttonData.text;
        
        // Set click handler with identifier
        string identifier = buttonData.identifier;
        unityButton.onClick.AddListener(() => OnButtonClicked(identifier));
    }
}
```

**Parameters**:
- `aiService`: AI service for profile updates
- `buttonIdentifier`: Button ID from the ChatMessage.Button.identifier

**Triggers**: `OnMessageReceived` with next predefined message
**Flow**: Stays in predefined mode after button clicks

---

#### `HandleUserActivity(IChatAIService, IRAGService, Dictionary<string, string>)`
Processes user activities from other app modules.

```csharp
public void HandleUserActivity(IChatAI service aiService,
                              IRAGService ragService,
                              Dictionary<string, string> userActivityMetadata)

// Example: Video completion in another module
public class VideoModule : MonoBehaviour
{
    [SerializeField] private ChatManager chatManager;
    
    private void OnVideoCompleted()
    {
        var activityData = new Dictionary<string, string>
        {
            { "type", "video" },
            { "name", "Breathing Exercise Tutorial" },
            { "duration", "300" }, // 5 minutes
            { "completion_rate", "100" },
            { "context", "User completed breathing exercise video" }
        };
        
        // Notify chat system of activity completion
        chatManager.HandleUserActivity(aiService, ragService, activityData);
    }
}
```

**Parameters**:
- `aiService`: AI service for response generation
- `ragService`: RAG service for context retrieval  
- `userActivityMetadata`: Activity details and context

**Triggers**: `OnAIMessageReceived` with contextual AI response
**Use Case**: Cross-module activity integration, contextual AI responses

---

### Conversation Management

#### `GetConversationHistory()`
Retrieves the complete conversation history.

```csharp
public List<ChatMessage> GetConversationHistory()

// Example: Loading conversation history into UI
private void LoadConversationHistory()
{
    var history = chatManager.GetConversationHistory();
    
    foreach (var message in history)
    {
        bool isFromBot = message.role != "user";
        AddMessageToUI(message.message, isFromBot, message.timestamp);
        
        // Show buttons for the last predefined message
        if (isFromBot && message.buttons != null && message == history.Last())
        {
            ShowButtons(message.buttons);
        }
    }
}
```

**Returns**: `List<ChatMessage>` ordered by timestamp
**Use Case**: UI initialization, conversation restoration, history display

---

#### `ForceNewConversation(IChatAIService, IRAGService, InitialMetadata)`
Forces creation of a new conversation and profile.

```csharp
public void ForceNewConversation(IChatAIService aiService,
                                IRAGService ragService,
                                InitialMetadata initialMetadata = null)

// Example: Reset conversation button
private void OnResetConversationClicked()
{
    // Clear UI
    ClearConversationUI();
    
    // Create new conversation with fresh context
    var newUserMeta = new ChatManager.InitialMetadata(
        "Wellness Coach",        // New AI character
        GetCurrentUserName(),    // Current user
        DateTime.Now            // Fresh start date
    );
    
    chatManager.ForceNewConversation(aiService, ragService, newUserMeta);
}
```

**Parameters**:
- `aiService`: AI service for new profile creation
- `ragService`: RAG service for context management
- `initialMetadata`: Optional fresh context for new conversation

**Triggers**: `OnChatInitialized` when new conversation is ready
**Use Case**: Conversation reset, fresh starts, testing

---

## 📋 Data Structures

### ChatMessage
The core message structure used throughout the system.

```csharp
[Serializable]
public class ChatMessage
{
    public string role;           // Message sender: "user", agent name, "pre_defined", "user_activity"
    public string message;        // The actual message content
    public Button[] buttons;      // Array of buttons (null for AI/activity messages)
    public DateTime timestamp;    // Message creation time
    public string buttonName;     // Display text for predefined messages
    public Dictionary<string, string> userActivityMetadata; // Activity context data
}

// Example usage in UI
private void DisplayMessage(ChatMessage msg)
{
    // Determine message type and styling
    bool isFromUser = msg.role == "user";
    bool isFromBot = msg.role.Contains("pre_defined") || IsAIAgent(msg.role);
    bool isActivity = msg.role == "user_activity";
    
    // Apply appropriate styling
    if (isFromUser)
    {
        AddUserMessage(msg.message, msg.timestamp);
    }
    else if (isFromBot)
    {
        AddBotMessage(msg.message, msg.timestamp);
        
        // Show buttons if available
        if (msg.buttons != null)
        {
            ShowButtons(msg.buttons);
        }
    }
    // Activity messages are usually internal - don't display directly
}
```

### Button
Button structure for predefined message interactions.

```csharp
[Serializable]
public class Button
{
    public string identifier;    // Unique backend identifier for routing
    public string text;         // Display text resolved from backend
}

// Example button handling
private void CreateButtonUI(ChatManager.Button buttonData)
{
    GameObject buttonObj = CreateButtonGameObject();
    Button unityButton = buttonObj.GetComponent<Button>();
    Text buttonText = buttonObj.GetComponentInChildren<Text>();
    
    // Use resolved display text
    buttonText.text = buttonData.text;
    
    // Handle click with identifier
    unityButton.onClick.AddListener(() => {
        chatManager.HandleButtonClick(aiService, buttonData.identifier);
    });
}
```

### InitialMetadata
Context data for new conversation initialization.

```csharp
[Serializable]
public class InitialMetadata
{
    public string AICharacterName;     // Name of the AI character
    public string userName;            // User's name
    public DateTime startDate;         // Conversation start date
    public Dictionary<string, string> additionalMetadata; // Extra context
}

// Example usage
var initialContext = new ChatManager.InitialMetadata(
    "Fitness Buddy",                   // AI character
    userProfile.displayName,           // User name
    DateTime.Now,                      // Start time
    new Dictionary<string, string>     // Additional context
    {
        { "user_level", "beginner" },
        { "preferred_language", "en" },
        { "timezone", "PST" }
    }
);
```

### ResumeConversationMetadata  
Context data for resuming existing conversations.

```csharp
[Serializable]
public class ResumeConversationMetadata
{
    public string context;             // Resume context message
    public Dictionary<string, string> additionalMetadata; // Extra resume data
}

// Example usage scenarios
// 1. Basic resume
var basicResume = new ChatManager.ResumeConversationMetadata();

// 2. Time-aware resume  
var timeResume = new ChatManager.ResumeConversationMetadata(
    "User returned to chat after being away for 2 hours",
    new Dictionary<string, string> { { "time_away", "2_hours" } }
);

// 3. Activity-based resume
var activityResume = new ChatManager.ResumeConversationMetadata(
    "User completed a workout and returned to chat",
    new Dictionary<string, string> 
    { 
        { "last_activity", "workout" },
        { "activity_duration", "45_minutes" }
    }
);
```

---

## 🎭 Flow Types & Behavior

### Predefined Flow (Structured Messages)
- **Trigger**: Button clicks or daily continuation
- **Characteristics**: Messages with buttons, structured progression
- **UI Behavior**: Show buttons, handle clicks, maintain structured flow
- **Backend Source**: Predefined messages from database

```csharp
// Example predefined flow handling
private void HandleBotMessage(ChatManager.ChatMessage message)
{
    // Display structured message
    AddBotMessageToUI(message.message);
    
    // Show interaction buttons
    if (message.buttons != null && message.buttons.Length > 0)
    {
        ShowButtons(message.buttons);
        HideTextInput(); // Optional: hide text input in pure predefined mode
    }
    else
    {
        HideButtons();
        ShowTextInput(); // Always show text input (can switch to AI mode)
    }
}
```

### AI Flow (Free-text Conversations)
- **Trigger**: User types any text message
- **Characteristics**: Natural language AI responses, no buttons
- **UI Behavior**: Hide buttons, focus on text input, show streaming
- **Backend Source**: AI service with RAG-enhanced context

```csharp
// Example AI flow handling  
private void HandleAIResponse(ChatManager.ChatMessage message)
{
    // Display natural AI response
    AddBotMessageToUI(message.message);
    
    // Hide structured interaction elements
    HideButtons();
    
    // Keep text input available for continued conversation
    ShowTextInput();
    EnableTextInput();
}
```

### Flow Switching Rules
- **Text Input Always Visible**: Users can always switch to AI mode by typing
- **Button Click = Predefined Mode**: Clicking buttons stays in structured flow
- **Mode Detection**: Determined by whether last bot message has buttons
- **Seamless Transitions**: No mode switching UI needed - system handles automatically

---

## 🚀 Complete Integration Example

Here's a complete example of ChatManager integration in a Unity UI:

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GamificationPlayer.Chat;
using GamificationPlayer.Chat.Services;

public class ChatInterfaceController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private ScrollRect chatScrollView;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject userMessagePrefab;
    [SerializeField] private GameObject botMessagePrefab;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject typingIndicator;
    [SerializeField] private GameObject errorNotification;
    
    [Header("Chat System")]
    [SerializeField] private ChatManager chatManager;
    [SerializeField] private IChatAIService aiService;
    [SerializeField] private IRAGService ragService;
    
    private void Start()
    {
        InitializeChatSystem();
        SetupUIHandlers();
    }
    
    private void InitializeChatSystem()
    {
        // Subscribe to ChatManager events
        ChatManager.OnChatInitialized += HandleChatInitialized;
        ChatManager.OnMessageReceived += HandleBotMessage;
        ChatManager.OnAIMessageReceived += HandleAIResponse;
        ChatManager.OnAIMessageChunkReceived += HandleStreamingChunk;
        ChatManager.OnErrorOccurred += HandleError;
        
        // Initialize chat with user context
        var initialMeta = new ChatManager.InitialMetadata(
            "Wellness Buddy",
            PlayerPrefs.GetString("username", "User"),
            System.DateTime.Now
        );
        
        chatManager.InitializeChat(aiService, ragService, initialMetadata: initialMeta);
    }
    
    private void SetupUIHandlers()
    {
        sendButton.onClick.AddListener(SendMessage);
        messageInput.onEndEdit.AddListener(OnInputEndEdit);
        
        // Disable input until chat is ready
        SetInputEnabled(false);
    }
    
    private void SendMessage()
    {
        string message = messageInput.text.Trim();
        if (!string.IsNullOrEmpty(message))
        {
            // Add user message to UI
            CreateUserMessage(message);
            
            // Clear input
            messageInput.text = "";
            
            // Send to ChatManager
            chatManager.HandleUserMessage(aiService, ragService, message);
            
            // Show typing indicator
            ShowTypingIndicator(true);
        }
    }
    
    private void OnInputEndEdit(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            SendMessage();
        }
    }
    
    #region Event Handlers
    
    private void HandleChatInitialized()
    {
        Debug.Log("Chat system initialized!");
        
        // Enable chat input
        SetInputEnabled(true);
        
        // Load conversation history
        LoadConversationHistory();
    }
    
    private void HandleBotMessage(ChatManager.ChatMessage message)
    {
        // Create bot message UI
        CreateBotMessage(message.message);
        
        // Handle buttons
        if (message.buttons != null && message.buttons.Length > 0)
        {
            CreateButtons(message.buttons);
        }
        else
        {
            ClearButtons();
        }
        
        // Hide typing indicator
        ShowTypingIndicator(false);
        
        // Auto-scroll to bottom
        ScrollToBottom();
    }
    
    private void HandleAIResponse(ChatManager.ChatMessage message)
    {
        // Create AI response UI
        CreateBotMessage(message.message);
        
        // Hide buttons (AI mode)
        ClearButtons();
        
        // Hide typing indicator
        ShowTypingIndicator(false);
        
        // Auto-scroll to bottom
        ScrollToBottom();
    }
    
    private void HandleStreamingChunk(string chunk)
    {
        // Update streaming message (if implementing real-time typing)
        UpdateStreamingMessage(chunk);
    }
    
    private void HandleError(string errorMessage)
    {
        Debug.LogError($"Chat Error: {errorMessage}");
        
        // Show error notification
        ShowErrorNotification(errorMessage);
        
        // Hide typing indicator
        ShowTypingIndicator(false);
        
        // Re-enable input
        SetInputEnabled(true);
    }
    
    #endregion
    
    #region UI Management
    
    private void CreateUserMessage(string message)
    {
        GameObject msgObj = Instantiate(userMessagePrefab, messageContainer);
        Text msgText = msgObj.GetComponentInChildren<Text>();
        msgText.text = message;
        
        ScrollToBottom();
    }
    
    private void CreateBotMessage(string message)
    {
        GameObject msgObj = Instantiate(botMessagePrefab, messageContainer);
        Text msgText = msgObj.GetComponentInChildren<Text>();
        msgText.text = message;
        
        ScrollToBottom();
    }
    
    private void CreateButtons(ChatManager.Button[] buttons)
    {
        ClearButtons();
        
        foreach (var buttonData in buttons)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button unityBtn = btnObj.GetComponent<Button>();
            Text btnText = btnObj.GetComponentInChildren<Text>();
            
            // Set button text
            btnText.text = buttonData.text;
            
            // Set click handler
            string identifier = buttonData.identifier;
            unityBtn.onClick.AddListener(() => OnButtonClicked(identifier));
        }
    }
    
    private void OnButtonClicked(string buttonIdentifier)
    {
        // Disable buttons during processing
        SetButtonsEnabled(false);
        
        // Process through ChatManager
        chatManager.HandleButtonClick(aiService, buttonIdentifier);
        
        // Show processing indicator
        ShowTypingIndicator(true);
    }
    
    private void ClearButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
    }
    
    private void LoadConversationHistory()
    {
        var history = chatManager.GetConversationHistory();
        
        foreach (var message in history)
        {
            if (message.role == "user")
            {
                CreateUserMessage(message.message);
            }
            else if (message.role != "user_activity") // Skip internal activity messages
            {
                CreateBotMessage(message.message);
                
                // Show buttons for the last predefined message
                if (message.buttons != null && message == history[history.Count - 1])
                {
                    CreateButtons(message.buttons);
                }
            }
        }
        
        ScrollToBottom();
    }
    
    private void SetInputEnabled(bool enabled)
    {
        messageInput.interactable = enabled;
        sendButton.interactable = enabled;
    }
    
    private void SetButtonsEnabled(bool enabled)
    {
        Button[] buttons = buttonContainer.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            btn.interactable = enabled;
        }
    }
    
    private void ShowTypingIndicator(bool show)
    {
        typingIndicator.SetActive(show);
        if (show)
        {
            ScrollToBottom();
        }
    }
    
    private void ShowErrorNotification(string error)
    {
        // Implement error notification UI
        errorNotification.SetActive(true);
        Text errorText = errorNotification.GetComponentInChildren<Text>();
        errorText.text = error;
        
        // Auto-hide after 3 seconds
        Invoke(nameof(HideErrorNotification), 3f);
    }
    
    private void HideErrorNotification()
    {
        errorNotification.SetActive(false);
    }
    
    private void UpdateStreamingMessage(string chunk)
    {
        // Implement streaming message updates if needed
        // This could update the last bot message with new chunks
    }
    
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        chatScrollView.verticalNormalizedPosition = 0f;
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        ChatManager.OnChatInitialized -= HandleChatInitialized;
        ChatManager.OnMessageReceived -= HandleBotMessage;
        ChatManager.OnAIMessageReceived -= HandleAIResponse;
        ChatManager.OnAIMessageChunkReceived -= HandleStreamingChunk;
        ChatManager.OnErrorOccurred -= HandleError;
    }
}
```

---

## ⚠️ Best Practices & Tips

### Event Handling
- **Always Subscribe Before InitializeChat()**: Events can fire immediately
- **Always Unsubscribe in OnDestroy()**: Prevents memory leaks and null references
- **Handle All Events**: Even if you don't use streaming, subscribe to prevent errors

### UI State Management
- **Text Input Always Visible**: Users can switch to AI mode anytime by typing
- **Button State Management**: Disable buttons during processing to prevent double-clicks
- **Auto-scrolling**: Scroll to bottom when new messages arrive
- **Typing Indicators**: Show during AI processing for better UX

### Error Handling
- **Subscribe to OnErrorOccurred**: Always show user-friendly error messages
- **Graceful Degradation**: Continue working even if some operations fail
- **Retry Logic**: Implement retry for network-related errors

### Performance
- **Message Recycling**: Use object pooling for message UI elements in long conversations
- **History Loading**: Load conversation history after chat initialization
- **Streaming Support**: Implement streaming for better perceived performance

### Flow Management
- **No Manual Mode Switching**: Let ChatManager handle predefined vs AI flow
- **Button vs Text Logic**: Buttons = predefined flow, text input = AI flow
- **State Persistence**: ChatManager handles conversation state automatically

---

## 🔍 Debugging & Troubleshooting

### Common Issues

1. **Events Not Firing**
   ```csharp
   // Wrong: Subscribing after initialization
   chatManager.InitializeChat(aiService, ragService);
   ChatManager.OnChatInitialized += HandleChatInitialized; // Too late!
   
   // Correct: Subscribe before initialization
   ChatManager.OnChatInitialized += HandleChatInitialized;
   chatManager.InitializeChat(aiService, ragService);
   ```

2. **Memory Leaks from Events**
   ```csharp
   private void OnDestroy()
   {
       // Always unsubscribe ALL events
       ChatManager.OnChatInitialized -= HandleChatInitialized;
       ChatManager.OnMessageReceived -= HandleBotMessage;
       // ... etc for all subscribed events
   }
   ```

3. **Button Clicks Not Working**
   ```csharp
   // Ensure you use the button.identifier, not button.text
   private void OnButtonClicked(string buttonIdentifier)
   {
       chatManager.HandleButtonClick(aiService, buttonIdentifier); // Use identifier
   }
   ```

### Debug Logging
Enable ChatManager logging for troubleshooting:

```csharp
chatManager.IsLogging = true; // Enable detailed logs
```

### Testing Checklist
- [ ] Events subscribed before initialization
- [ ] All events have handlers (even unused ones)
- [ ] Events unsubscribed in OnDestroy
- [ ] Button identifiers used correctly
- [ ] Error handling implemented  
- [ ] UI states managed properly
- [ ] Memory leaks checked

---

## 📚 Additional Resources

- **Flow Documentation**: See `ChatManagerFlowSummary.md` for detailed behavioral rules
- **System Architecture**: See `SystemFlowSummary.md` for complete system overview
- **Service Interfaces**: Check `IChatAIService` and `IRAGService` documentation
- **Backend APIs**: Refer to `GamificationPlayerEndpoints` documentation

---

**Happy Coding! 🚀**

The ChatManager provides a complete, event-driven chat system that handles all the complexity of conversation flows, AI interactions, and backend synchronization. Focus on creating great UI experiences while ChatManager handles all the heavy lifting behind the scenes.
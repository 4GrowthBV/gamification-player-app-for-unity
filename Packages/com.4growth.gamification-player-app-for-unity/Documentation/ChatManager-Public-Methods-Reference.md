# 🔧 ChatManager Public Methods Reference
### Quick Reference for Frontend Developers

This document provides a concise reference of all public methods available in the ChatManager class, what they do, and how to use them.

---

## 📋 Method Overview

| Method | Purpose | When to Use |
|--------|---------|-------------|
| `Initialize()` | Setup ChatManager with dependencies | Before any other method calls |
| `IsInitialized()` | Check if properly set up | To verify setup before operations |
| `InitializeChat()` | Start the chat system | To begin chat functionality |
| `HandleUserMessage()` | Process user text input | When user sends a message |
| `HandleButtonClick()` | Process button interactions | When user clicks a chat button |
| `HandleUserActivity()` | Process cross-module activities | When user completes activities elsewhere |
| `GetConversationHistory()` | Retrieve chat history | To display previous messages |
| `ForceNewConversation()` | Reset to fresh conversation | To start over or for testing |

---

## 🔧 Method Details

### `Initialize(GamificationPlayerEndpoints, ISessionLogData)`

**Purpose**: Sets up ChatManager with required backend dependencies.

**Parameters**:
- `gamificationPlayerEndpoints` - Backend API connection
- `sessionLogData` - User session information

**Usage**:
```csharp
// Must be called first, before any other ChatManager operations
chatManager.Initialize(endpoints, sessionData);

// Always check if initialization succeeded
if (chatManager.IsInitialized())
{
    // Safe to proceed with other operations
}
```

**When to Call**: Once at startup, before using any other ChatManager functionality.

---

### `IsInitialized()`

**Purpose**: Checks if ChatManager has been properly initialized with dependencies.

**Returns**: `bool` - `true` if ready to use, `false` if still needs initialization

**Usage**:
```csharp
if (chatManager.IsInitialized())
{
    // Safe to call other methods
    chatManager.InitializeChat(aiService, ragService);
}
else
{
    // Need to call Initialize() first
    Debug.LogError("ChatManager not initialized!");
}
```

**When to Call**: Before calling other methods to ensure system is ready.

---

### `InitializeChat(IChatAIService, IRAGService, ...)`

**Purpose**: Starts the chat system, loads conversation history, and prepares for user interaction.

**Parameters**:
- `aiService` - AI service for generating responses
- `ragService` - RAG service for context and knowledge
- `resumeMetadata` (optional) - Context for resuming conversations
- `initialMetadata` (optional) - Initial user/character context
- `forceNewConversation` (optional) - Force fresh conversation

**Usage**:
```csharp
// Basic chat initialization
chatManager.InitializeChat(aiService, ragService);

// With initial user context
var initialMeta = new ChatManager.InitialMetadata(
    "Health Buddy",     // AI character name
    "John Doe",         // User name
    DateTime.Now        // Start date
);
chatManager.InitializeChat(aiService, ragService, initialMetadata: initialMeta);

// Resume with context
var resumeMeta = new ChatManager.ResumeConversationMetadata(
    "User returned after completing workout"
);
chatManager.InitializeChat(aiService, ragService, resumeMetadata: resumeMeta);

// Force new conversation
chatManager.InitializeChat(aiService, ragService, forceNewConversation: true);
```

**Triggers**: `OnChatInitialized` event when complete

**When to Call**: After `Initialize()` to start chat functionality.

---

### `HandleUserMessage(IChatAIService, IRAGService, string)`

**Purpose**: Processes user text input and generates AI responses.

**Parameters**:
- `aiService` - AI service for response generation
- `ragService` - RAG service for context enhancement
- `userMessage` - The user's text input

**Usage**:
```csharp
// Called when user sends a text message
private void OnSendButtonClick()
{
    string userInput = messageInputField.text;
    if (!string.IsNullOrEmpty(userInput))
    {
        // Add to UI immediately
        DisplayUserMessage(userInput);
        
        // Process through ChatManager
        chatManager.HandleUserMessage(aiService, ragService, userInput);
        
        // Clear input and show loading
        messageInputField.text = "";
        ShowTypingIndicator(true);
    }
}
```

**Triggers**: `OnAIMessageReceived` event with AI response

**When to Call**: When user types and sends a text message.

**Flow Effect**: Switches chat to AI conversation mode.

---

### `HandleButtonClick(IChatAIService, string)`

**Purpose**: Processes predefined conversation button clicks.

**Parameters**:
- `aiService` - AI service for profile updates
- `buttonIdentifier` - The button's unique identifier (from Button.identifier)

**Usage**:
```csharp
// Called when user clicks a conversation button
private void OnChatButtonClick(string buttonId)
{
    // Disable buttons during processing
    SetButtonsEnabled(false);
    
    // Process button click
    chatManager.HandleButtonClick(aiService, buttonId);
    
    // Show processing indicator
    ShowTypingIndicator(true);
}

// Example button creation with proper identifier handling
private void CreateButton(ChatManager.Button buttonData)
{
    Button uiButton = Instantiate(buttonPrefab);
    uiButton.GetComponentInChildren<Text>().text = buttonData.text;
    
    // Important: Use buttonData.identifier, not buttonData.text
    uiButton.onClick.AddListener(() => OnChatButtonClick(buttonData.identifier));
}
```

**Triggers**: `OnMessageReceived` event with next predefined message

**When to Call**: When user clicks buttons in structured conversations.

**Flow Effect**: Continues predefined conversation flow.

---

### `HandleUserActivity(IChatAIService, IRAGService, Dictionary<string, string>)`

**Purpose**: Processes user activities from other app modules (videos, surveys, exercises, etc.).

**Parameters**:
- `aiService` - AI service for contextual responses
- `ragService` - RAG service for knowledge enhancement
- `userActivityMetadata` - Activity details and context

**Usage**:
```csharp
// Called from other modules when user completes activities
public class VideoModule : MonoBehaviour
{
    public void OnVideoCompleted(string videoName, int durationSeconds)
    {
        var activityData = new Dictionary<string, string>
        {
            { "type", "video" },
            { "name", videoName },
            { "duration", durationSeconds.ToString() },
            { "completion_rate", "100" },
            { "context", "User completed educational video" }
        };
        
        // Notify chat system
        chatManager.HandleUserActivity(aiService, ragService, activityData);
    }
}

// Survey completion example
public void OnSurveyCompleted(string surveyId, Dictionary<string, string> responses)
{
    var activityData = new Dictionary<string, string>
    {
        { "type", "survey" },
        { "survey_id", surveyId },
        { "responses", JsonUtility.ToJson(responses) },
        { "context", "User completed wellness survey" }
    };
    
    chatManager.HandleUserActivity(aiService, ragService, activityData);
}
```

**Triggers**: `OnAIMessageReceived` event with contextual AI response

**When to Call**: When user completes significant activities in other parts of your app.

**Benefits**: Creates contextually aware chat that acknowledges user's journey across the entire application.

---

### `GetConversationHistory()`

**Purpose**: Retrieves the complete conversation history as a list of messages.

**Returns**: `List<ChatMessage>` - All messages ordered by timestamp

**Usage**:
```csharp
// Load conversation history into UI
private void LoadChatHistory()
{
    var history = chatManager.GetConversationHistory();
    
    foreach (var message in history)
    {
        if (message.role == "user")
        {
            DisplayUserMessage(message.message, message.timestamp);
        }
        else if (message.role != "user_activity") // Skip internal activity messages
        {
            DisplayBotMessage(message.message, message.timestamp);
            
            // Show buttons for the last predefined message
            if (message.buttons != null && message == history.Last())
            {
                ShowButtons(message.buttons);
            }
        }
    }
}

// Count messages for UI purposes
private void UpdateMessageCounter()
{
    var messageCount = chatManager.GetConversationHistory().Count;
    messageCounterText.text = $"{messageCount} messages";
}
```

**When to Call**: 
- After `OnChatInitialized` to display conversation history
- Anytime you need to access previous messages
- For implementing message search or export features

---

### `ForceNewConversation(IChatAIService, IRAGService, InitialMetadata)`

**Purpose**: Forces creation of a completely new conversation and profile, discarding current state.

**Parameters**:
- `aiService` - AI service for new profile creation
- `ragService` - RAG service for context management
- `initialMetadata` (optional) - Fresh context for new conversation

**Usage**:
```csharp
// Reset conversation (e.g., "Start Over" button)
private void OnResetButtonClick()
{
    // Clear current UI
    ClearAllMessages();
    ClearButtons();
    
    // Create fresh conversation
    var newContext = new ChatManager.InitialMetadata(
        "Fresh Assistant",      // New AI character
        GetCurrentUserName(),   // Current user
        DateTime.Now           // Fresh start time
    );
    
    chatManager.ForceNewConversation(aiService, ragService, newContext);
}

// Testing with different contexts
private void StartTestConversation()
{
    var testContext = new ChatManager.InitialMetadata(
        "Test Bot",
        "Test User", 
        DateTime.Now,
        new Dictionary<string, string> { { "test_mode", "true" } }
    );
    
    chatManager.ForceNewConversation(aiService, ragService, testContext);
}
```

**Triggers**: `OnChatInitialized` event when new conversation is ready

**When to Call**: 
- When user wants to start over completely
- For testing different conversation scenarios
- When switching between different users or contexts

**Warning**: This permanently discards the current conversation history.

---

## 🔄 Method Call Flow

### Typical Initialization Sequence
1. `Initialize(endpoints, sessionData)` - Setup dependencies
2. `IsInitialized()` - Verify setup succeeded  
3. `InitializeChat(aiService, ragService)` - Start chat system
4. Wait for `OnChatInitialized` event
5. `GetConversationHistory()` - Load previous messages

### Typical User Interaction Sequence
1. User types message → `HandleUserMessage()`
2. Wait for `OnAIMessageReceived` event
3. Display AI response
4. User clicks button → `HandleButtonClick()`  
5. Wait for `OnMessageReceived` event
6. Display predefined message with new buttons

### Cross-Module Integration Sequence
1. User completes activity in another module
2. Module calls `HandleUserActivity()` with activity data
3. Wait for `OnAIMessageReceived` event
4. Display contextual AI acknowledgment

---

## ⚠️ Important Notes

### Method Dependencies
- **Must call `Initialize()` first** before any other methods
- **Check `IsInitialized()`** before calling other methods
- **Subscribe to events** before calling `InitializeChat()`

### Parameter Requirements
- **Button clicks**: Always use `Button.identifier`, never `Button.text`
- **Services**: Same `aiService` and `ragService` instances should be used consistently
- **Activity metadata**: Include meaningful context for better AI responses

### Error Handling
- All methods are safe to call - they emit `OnErrorOccurred` events rather than throwing exceptions
- Always subscribe to `OnErrorOccurred` for proper error handling
- Methods validate input and system state automatically

### Performance Tips
- `GetConversationHistory()` returns a new list each time - cache if calling frequently
- Activity notifications are processed asynchronously - don't block other operations
- Button clicks disable automatically during processing - no need for manual state management

---

**Quick Reference Complete! 🚀**

These 8 public methods provide complete control over the ChatManager system. Focus on the initialization sequence first, then add user interaction methods as needed for your specific UI requirements.
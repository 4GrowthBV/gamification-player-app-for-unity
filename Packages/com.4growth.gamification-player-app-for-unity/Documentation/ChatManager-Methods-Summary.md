# 🔧 ChatManager Public Methods Summary
### Simple Reference Guide

This document provides a concise overview of all public methods available in the ChatManager class and their purposes.

---

## 📋 Public Methods Overview

### **Initialize(GamificationPlayerEndpoints, ISessionLogData)**
Sets up ChatManager with required backend dependencies and session data. Must be called before using any other ChatManager functionality.

### **IsInitialized()**
Checks if ChatManager has been properly initialized with all required dependencies. Returns true if ready to use, false if still needs setup.

### **InitializeChat(IChatAIService, IRAGService, ResumeConversationMetadata, InitialMetadata, bool)**
Starts the chat system by loading conversation history, setting up AI services, and preparing for user interaction. Can resume existing conversations or create new ones.

### **HandleUserMessage(IChatAIService, IRAGService, string)**
Processes user text input by routing it through AI services to generate intelligent, context-aware responses. Automatically switches chat to conversational mode.

### **HandleButtonClick(IChatAIService, string)**
Processes user button clicks in structured conversations by advancing to the next predefined message based on the button identifier.

### **HandleUserActivity(IChatAIService, IRAGService, Dictionary<string, string>)**
Processes notifications from other app modules when users complete activities (videos, surveys, exercises) and generates contextual AI responses.

### **GetConversationHistory()**
Retrieves the complete conversation history as a chronologically ordered list of all messages exchanged in the current conversation.

### **ForceNewConversation(IChatAIService, IRAGService, InitialMetadata)**
Creates a completely new conversation and user profile, discarding all current conversation state and starting fresh.

---

## 🎯 Method Categories

### **Setup Methods**
- **Initialize** - System setup with dependencies
- **IsInitialized** - Verification of setup status
- **InitializeChat** - Chat system activation

### **User Interaction Methods**
- **HandleUserMessage** - Free-text user input processing
- **HandleButtonClick** - Structured button interaction processing
- **HandleUserActivity** - Cross-module activity integration

### **Data Access Methods**
- **GetConversationHistory** - Conversation history retrieval

### **State Management Methods**
- **ForceNewConversation** - Complete conversation reset

---

## 📊 Method Usage Flow

### **Required Sequence**
1. **Initialize** - Must be called first
2. **IsInitialized** - Verify setup succeeded
3. **InitializeChat** - Start chat functionality

### **User Interaction**
- **HandleUserMessage** - When users type messages
- **HandleButtonClick** - When users click conversation buttons
- **HandleUserActivity** - When users complete activities elsewhere

### **Data Management**
- **GetConversationHistory** - Access previous messages
- **ForceNewConversation** - Reset when needed

---

---

## 📡 Public Events Overview

### **OnChatInitialized**
Fires when the chat system is fully initialized and ready for user interaction. Indicates that conversation history has been loaded and all services are connected.

### **OnMessageReceived(ChatMessage)**
Fires when a predefined bot message is received, usually containing interactive buttons for structured conversation flow.

### **OnAIMessageReceived(ChatMessage)**
Fires when an AI-generated response is received in free-text conversation mode. These messages do not contain buttons.

### **OnAIMessageChunkReceived(string)**
Fires during streaming AI responses, providing real-time chunks of the AI response as it's being generated for typing indicator effects.

### **OnErrorOccurred(string)**
Fires when any error occurs during chat operations, providing a user-friendly error message that should be displayed to the user.

---

## 🎯 Event Categories

### **System Status Events**
- **OnChatInitialized** - System ready notification
- **OnErrorOccurred** - Error notifications

### **Message Events**
- **OnMessageReceived** - Predefined bot messages with buttons
- **OnAIMessageReceived** - AI-generated responses
- **OnAIMessageChunkReceived** - Real-time streaming updates

---

## 🔄 Event Flow

### **Initialization Flow**
1. Call **InitializeChat** method
2. System loads conversation history from backend
3. **OnChatInitialized** event fires when ready
4. Call **GetConversationHistory** to display previous messages
5. **OnErrorOccurred** fires if initialization fails

### **User Message Flow**
1. Call **HandleUserMessage** method
2. **OnAIMessageReceived** event fires with AI response
3. **OnAIMessageChunkReceived** may fire during streaming
4. **OnErrorOccurred** fires if processing fails

### **Button Interaction Flow**
1. Call **HandleButtonClick** method
2. **OnMessageReceived** event fires with next predefined message
3. **OnErrorOccurred** fires if button processing fails

### **Activity Integration Flow**
1. Call **HandleUserActivity** method
2. **OnAIMessageReceived** event fires with contextual response
3. **OnErrorOccurred** fires if activity processing fails

### **History Access Flow**
1. Call **GetConversationHistory** anytime after initialization
2. Returns complete chronological message list
3. Use for displaying conversation, message counts, or search features
4. No events fired - immediate synchronous access

---

**Complete Reference Guide! 🚀**

These 8 public methods and 5 events provide complete control and monitoring of the ChatManager system for any frontend implementation.
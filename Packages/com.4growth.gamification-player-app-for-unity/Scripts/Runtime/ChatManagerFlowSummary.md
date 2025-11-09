# 💡 Chat Manager Flow Summary
### What the Chat Manager Should Do

This document defines **exactly what the Chat Manager is responsible for** and what it explicitly **does not handle**.  
It provides the behavioral and architectural blueprint for rewriting the ChatManager C# class.

---

## 1. Core Responsibility

The Chat Manager is the **client-side orchestrator** that:
- Manages chat state (conversation, profile, history, and flow type).
- Connects Unity UI events with backend API calls.
- Synchronizes local state with the Gamification Player APIs.
- Handles predefined and AI message flows.
- Emits high-level events that the UI layer subscribes to.

It does **not** contain message content, AI logic, or UI rendering.

---

## 2. Responsibilities by Flow Stage

| **Stage** | **✅ Chat Manager Does** | **🚫 Chat Manager Does Not** | **Notes** |
|------------|-------------------------|------------------------------|-----------|
| **Initialization** | • Verify initialization and dependencies.<br>• Check for existing conversation via API.<br>• Create new conversation if none exists.<br>• Create or reuse chat profile.<br>• Load conversation history from backend.<br>• Load and cache all predefined messages for performance.<br>• Load and cache AI instructions.<br>• If it's a new day, request next week-based predefined message (e.g., `week1_day1`, `week1_day2`, `week2_day1`).<br>• If first time user, start with `week1_day0` (Welcome message).<br>• If resuming existing conversation, optionally handle ResumeConversationMetadata for contextual re-engagement.<br>• Trigger `OnChatInitialized`. | • Determine weekly progression logic — backend provides message structure.<br>• Store or compute content — only fetches from API.<br>• Manage authentication or user organization scope.<br>• Track user absence duration or activity. | Entry point for chat lifecycle setup. Enhanced with performance caching. Uses week-based progression system. Supports contextual conversation resumption. |
| **User Activity Flow** | • Handle user activity notifications from other modules/pop-ups.<br>• Log activities as special `user_activity` messages with rich metadata.<br>• Store activity metadata as JSON in conversation history.<br>• Route activity context to N8n for agent selection.<br>• Generate contextual AI responses based on activity completion.<br>• Update profile after activity-triggered conversations.<br>• Trigger `OnAIMessageReceived` with contextual response. | • Track or monitor user activities in other modules.<br>• Generate activity completion notifications.<br>• Define what activities should trigger responses. | Seamlessly integrates module activities into conversation context. Activities become part of conversation history for enhanced AI awareness. |
| **Predefined Flow** | • Handle button click events with structured Button objects.<br>• Add button choice to local and remote history.<br>• Use cached predefined messages for instant responses.<br>• Track flow state based on message buttons presence.<br>• Update profile after each message using AI service.<br>• Trigger `OnMessageReceived` event with full ChatMessage object. | • Decide button→message mapping (backend provides).<br>• Define button layout or style.<br>• Contain message text. | Executes structured predefined conversation branch with enhanced Button objects containing identifier and display text. |
| **AI Flow** | • Detect transition when user sends text.<br>• Add user message to history and backend.<br>• Determine flow based on last message having buttons or not.<br>• Call Router API with full ChatMessage[] conversation context.<br>• Update profile using AI service with structured conversation history.<br>• Request instructions for that agent from cached database.<br>• Generate response using AI service with rich context.<br>• Save both user and AI messages.<br>• Trigger `OnAIMessageReceived` with ChatMessage object. | • Implement AI or LLM logic.<br>• Store or define instructions.<br>• Manage long-term knowledge bases. | Orchestrates agent selection with enhanced context passing. Uses structured ChatMessage objects for richer data exchange. |
| **Daily Continuation** | • Detect new day with enhanced timestamp logic.<br>• Use week-based identifier progression (week1_day0 → week1_day1 → week1_day2, etc.).<br>• Request next sequential week-day message after history load.<br>• Stop daily progression when no more sequential messages exist.<br>• Reuse conversation and profile. | • Maintain calendar schedule or campaign logic.<br>• Define when daily message sequence ends. | Extends ongoing conversation with week-based day progression until sequence completion. |
| **Persistence & Sync** | • Store conversationHistory as structured ChatMessage objects.<br>• Push every message to backend immediately.<br>• Keep conversation/profile IDs in sync.<br>• Provide getter for conversation history as ChatMessage list.<br>• Support force new conversation functionality. | • Handle offline caching or retry queues.<br>• Manage non-chat session data. | Guarantees state consistency with structured data objects. Enhanced with conversation reset capability. |
| **Performance & Caching** | • Cache all predefined messages on initialization.<br>• Cache AI instructions for fast access.<br>• Use parallel execution for performance optimization.<br>• Implement smart day detection and progression. | • Handle long-term persistent storage.<br>• Manage cache invalidation strategies. | Advanced performance optimizations with intelligent caching systems. |
| **Events** | • Emit:<br>  - `OnMessageReceived(ChatMessage)` - full message object with buttons<br>  - `OnAIMessageReceived(ChatMessage)` - AI response as structured object<br>  - `OnAIMessageChunkReceived(string)` - streaming support<br>  - `OnErrorOccurred(string)` - error notifications<br>  - `OnChatInitialized()` - system ready | • Render UI.<br>• Decide visual behavior. | Enhanced event-based communication with structured data objects and streaming support. |

---

## 3. Behavioral Rules

- **Text input field is always visible.**
  - Typing any message → switches to AI flow.
  - Clicking a button → stays in predefined flow.
  - Flow type is determined by whether the last message has buttons or not.
  - Messages without buttons provide only text input option (no mode change).
- **Profile updates** occur after every message (both user and bot) using AI service with structured conversation context.
- **All message content and instructions** come from the backend — none hardcoded, with intelligent caching for performance.
- **Button handling** filters out "none" values during initial loading and uses button_name for display text.
- **Week-based progression** follows sequential pattern: week1_day0 → week1_day1 → week1_day2, etc.
- **Missing day handling** - if no predefined message exists for a specific week/day, no predefined message is shown (only conversation history).
- **Daily sequence completion** stops automatic daily messages when no more sequential messages exist.
- **Chat state** (conversation ID, profile ID, structured history) is persistent between sessions.
- **Events** are the only way the UI interacts with ChatManager, providing rich ChatMessage objects.
- **Performance optimization** through parallel execution and smart caching of predefined messages and instructions.
- **Enhanced data structures** with ChatMessage objects containing role, message, buttons, timestamps, button_name, and user activity metadata.
- **User activity integration** - activities from other modules (videos, surveys, exercises) are logged as conversation messages with rich metadata for AI context awareness.
- **Contextual conversation resumption** - ResumeConversationMetadata enables intelligent re-engagement when users return to existing conversations.

---

## 4. Enhanced Data Structures

### **ChatMessage Structure**
Rich message objects containing:
- **Role**: Identifies message sender (user, predefined system, user activity, or AI agent name)
- **Content**: The actual message text
- **Buttons**: Array of structured button objects for predefined messages (null for AI/activity messages)
- **Timestamp**: When the message was created for proper chronological ordering
- **Button Name**: Display text resolution for buttons from backend data
- **Activity Metadata**: Rich contextual data for user activity messages

### **Button Structure**
Structured button objects containing:
- **Identifier**: Backend identifier for button actions and routing
- **Display Text**: User-facing text resolved from backend button_name fields

### **Event Architecture**
- **Message Events**: Predefined messages with structured buttons and full context
- **AI Response Events**: AI-generated responses as structured message objects  
- **Streaming Events**: Real-time AI response chunks for enhanced user experience
- **Error Events**: System error notifications for proper error handling
- **Initialization Events**: System ready notifications for UI coordination

### **Resume Conversation Structure**
Contextual resumption data containing:
- **Context Message**: Explanation of resumption scenario for AI processing
- **Additional Metadata**: Flexible key-value context data (time away, previous activities, etc.)
- **Conversion Support**: Seamless integration with existing activity processing systems

### **Initial Metadata Structure**  
First-time user context containing:
- **User Information**: Name and other identifying information for personalization
- **Start Date**: Chat initiation timestamp for proper progression tracking
- **Additional Context**: Flexible contextual information for enhanced AI understanding
- **Conversion Support**: Integration with activity-based message processing

### **Public API Interface**
- **Initialize Chat**: Setup with optional resume context and initial user metadata
- **Handle User Messages**: Process free-text user input through AI flow
- **Handle Button Clicks**: Process structured predefined flow interactions
- **Handle User Activities**: Process cross-module activity notifications seamlessly
- **Get Conversation History**: Retrieve complete conversation as structured message list
- **Force New Conversation**: Reset conversation and profile with optional initial context

---

## 5. Integration Points

| Component | Interaction |
|------------|-------------|
| **GamificationPlayerEndpoints** | Used for all CRUD operations on conversations, profiles, messages, predefined messages, and instructions. Enhanced with bulk loading for caching. |
| **RouterService (n8n or custom)** | Determines which agent handles user messages using full ChatMessage[] conversation context for better routing decisions. |
| **AI Service** | Handles both AI response generation and profile updates with structured ChatMessage arrays. Single service for all AI operations with rich context. |
| **UI Layer** | Subscribes to ChatManager events to receive rich ChatMessage objects with buttons, timestamps, and streaming support. |

---

## 6. Non-Responsibilities

The Chat Manager **must not**:
- Contain message text, predefined logic, or instructions.
- Contain LLM or AI generation logic.
- Render or control UI layouts.
- Handle authentication or organization-specific logic.
- Manage non-chat app data or global session state.

---

### Essence

> The Chat Manager is a **stateful coordinator**, not a content engine.  
> It ensures smooth message flow, state management, and backend synchronization — allowing all conversation logic to remain fully server-driven.

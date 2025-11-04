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
| **Initialization** | • Verify initialization and dependencies.<br>• Check for existing conversation via API.<br>• Create new conversation if none exists.<br>• Create or reuse chat profile.<br>• Load conversation history from backend.<br>• If it’s a new day, request next `day_X` predefined message.<br>• Trigger `OnChatInitialized`. | • Determine what “day_X” means — backend decides.<br>• Store or compute content — only fetches from API.<br>• Manage authentication or user organization scope. | Entry point for chat lifecycle setup. |
| **Predefined Flow** | • Handle button click events.<br>• Add button choice to local and remote history.<br>• Fetch next predefined message by identifier.<br>• Maintain `isInPredefinedFlow = true`.<br>• Update profile after each message.<br>• Trigger `OnMessageReceived` event for UI. | • Decide button→message mapping (backend provides).<br>• Define button layout or style.<br>• Contain message text. | Executes structured predefined conversation branch. |
| **AI Flow** | • Detect transition when user sends text.<br>• Add user message to history and backend.<br>• Set `isInPredefinedFlow = false`.<br>• Call Router API → get agent.<br>• Call Memory API → update profile.<br>• Request instructions for that agent from database.<br>• Generate response (temporary simulation; future LLM call).<br>• Save both user and AI messages.<br>• Trigger `OnAIMessageReceived`. | • Implement AI or LLM logic.<br>• Store or define instructions.<br>• Manage long-term knowledge bases. | Orchestrates agent selection and message persistence. |
| **Daily Continuation** | • Detect new day (based on stored timestamp).<br>• Request next `day_X` message after history load.<br>• Reuse conversation and profile. | • Maintain calendar schedule or campaign logic. | Extends ongoing conversation with daily context. |
| **Persistence & Sync** | • Store conversationHistory list locally.<br>• Push every message to backend immediately.<br>• Keep conversation/profile IDs in sync.<br>• Provide getters for conversation state.<br>• Reset chat when requested. | • Handle offline caching or retry queues.<br>• Manage non-chat session data. | Guarantees state consistency. |
| **Events** | • Emit:<br>  - `OnMessageReceived` (bot + buttons)<br>  - `OnAIMessageReceived` (AI response)<br>  - `OnErrorOccurred`<br>  - `OnChatInitialized` | • Render UI.<br>• Decide visual behavior. | Event-based communication between logic and UI. |

---

## 3. Behavioral Rules

- **Text input field is always visible.**
  - Typing any message → switches to AI flow.
  - Clicking a button → stays in predefined flow.
- **Profile updates** occur after every message (both user and bot).
- **All message content and instructions** come from the backend — none hardcoded.
- **Chat state** (conversation ID, profile ID, history, flow mode) is persistent between sessions.
- **Events** are the only way the UI interacts with ChatManager.

---

## 4. Integration Points

| Component | Interaction |
|------------|-------------|
| **GamificationPlayerEndpoints** | Used for all CRUD operations on conversations, profiles, messages, predefined messages, and instructions. |
| **RouterService (n8n or custom)** | Determines which agent handles user messages. |
| **MemoryService** | Updates and retrieves chat profile context. |
| **UI Layer** | Subscribes to ChatManager events to render messages and input state. |
| **AI Service (future)** | Will replace current simulation in AI response generation. |

---

## 5. Non-Responsibilities

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

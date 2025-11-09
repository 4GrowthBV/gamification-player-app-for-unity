# 🧭 Final System Flow Summary
### How the Chat System Works

This document describes **how the full chat system behaves** — the complete functional flow that the Chat Manager is part of.

---

## 1. Overview

The chat system is designed to guide users through a hybrid conversation model:
- It starts with **predefined messages** (structured, button-based).
- It transitions into **AI-driven conversation** (free text).
- It continues **daily**, maintaining profile and conversation history over time.

All message logic, daily scheduling, and instruction data come from the **backend database**, never from hardcoded content in the app.

The **Chat Manager** is the client-side orchestrator — it executes these flows using API calls, manages local state, and updates the UI via events.

---

## 2. Core Flow

| **Stage** | **Trigger** | **System Actions** | **User Experience** |
|------------|--------------|--------------------|---------------------|
| **1. Initialization** | User opens the chat (e.g., "Wellbe Buddy") | • Check backend for an existing conversation.<br>• If a conversation exists → load full conversation history as structured ChatMessage objects.<br>• Load and cache all predefined messages and AI instructions for performance.<br>• If it's a new day → after loading history, fetch the next week-based predefined message (`week1_day1`, `week1_day2`, etc.).<br>• If no conversation exists → create new conversation + profile, then load welcome message (`week1_day0`).<br>• If resuming existing conversation → optionally provide contextual resume message via ResumeConversationMetadata for smooth re-engagement.<br>• Filter out "none" values from button arrays during loading. | Chat opens with either:<br>– the user's previous conversation with original timestamps, or<br>– the daily starting message if new, or<br>– a contextual welcome-back message if resuming.<br><br>Text input field is **always visible**, even in predefined flow. Performance is enhanced with intelligent caching and proper button filtering. |
| **1a. User Activity Integration** | User completes activity in other module (video, survey, exercise) and returns to chat | • Other module calls `HandleUserActivity()` with rich metadata.<br>• Create special `user_activity` message with activity details.<br>• Store activity metadata as JSON in conversation history.<br>• Route activity context through N8n for agent selection.<br>• Generate contextual AI response acknowledging the activity.<br>• Update user profile with activity completion context.<br>• Emit `OnAIMessageReceived` with contextual response. | User sees intelligent, contextual acknowledgment of their recent activity. Chat feels aware of their journey across the app, creating seamless integration between modules. |
| **2. Predefined Flow** | User clicks a **structured button** with identifier and text | • Add user action to conversation history as ChatMessage.<br>• Save the message to backend.<br>• Load next predefined message from cache via identifier.<br>• Update the profile after each message using AI service with full conversation context.<br>• Emit OnMessageReceived event with full ChatMessage object including Button[] array.<br>• If next message has no buttons, the flow remains visually unchanged. | Chat continues as a structured, guided dialogue. Buttons show both identifier and display text. Rich ChatMessage objects provide enhanced UI capabilities. The text field remains available at all times. |
| **3. AI Flow (Triggered by Free Text)** | User types and sends text in the input field | • Add user message to local history + backend as ChatMessage.<br>• Switch to AI flow (determined by message button presence).<br>• Call **Router service** with full ChatMessage[] conversation context.<br>• Use **AI service** to update the user profile with structured conversation history.<br>• Fetch **instructions** for that agent from cached database.<br>• Generate AI response via AI service with rich context and streaming support.<br>• Add AI message to history + backend as ChatMessage.<br>• Emit OnAIMessageReceived with full ChatMessage object.<br>• Update the profile again after AI response using AI service. | The user sees an intelligent, context-aware reply with potential streaming. The conversation becomes natural and continuous with enhanced context awareness. |
| **4. Daily Continuation** | New day detected with enhanced logic | • Reuse existing conversation and profile.<br>• Load history as structured ChatMessage objects.<br>• Use week-based sequential progression (week1_day0 → week1_day1 → week1_day2, etc.).<br>• Then load the next sequential week-day predefined message.<br>• Stop daily progression when no more sequential messages exist.<br>• Continue normally in predefined or AI flow. | The chat feels like an ongoing daily buddy interaction with week-based progression, remembering prior context with original timestamps. When daily sequence completes, only text input remains available. |
| **5. Persistence & Synchronization** | Continuous background operation with performance optimization | • Every message (user or bot) is stored locally as ChatMessage objects and remotely.<br>• Profile updated after each message with structured conversation context.<br>• Instructions and predefined messages cached for performance.<br>• Conversation and profile IDs saved in local state for quick restoration.<br>• Support for force new conversation functionality. | The chat always remembers previous discussions with original timestamps, enhanced performance, and the ability to reset conversations when needed. |

---

## 3. Behavioral Rules

- **Text input is always visible.**  
  - If user types → switches or stays in **AI flow**.  
  - If user clicks button → stays in **predefined flow**.
  - Flow type is determined by whether the last message has buttons or not.
- **Profile updates** occur after every message (both user and bot) using AI service with structured ChatMessage context.
- **All messages, profiles, and instructions** are loaded and saved via backend APIs with intelligent caching for performance.
- **Daily messages** follow week-based sequential progression (week1_day0, week1_day1, week1_day2, etc.) with enhanced context awareness.
- **Missing day handling** - if no predefined message exists for a specific week/day, no predefined message is shown (only conversation history).
- **Chat history** is persistent as structured ChatMessage objects — users never lose prior conversations with original timestamps.
- **Enhanced data structures** provide richer UI capabilities with Button objects containing identifiers and display text from button_name fields.
- **Button filtering** automatically removes "none" values during message loading for clean UI presentation.
- **Sequential completion** stops daily progression when sequence ends (text input remains available).
- **Performance optimization** through parallel execution, caching, and streaming support for AI responses.
- **Cross-module activity awareness** - activities from videos, surveys, exercises become part of conversation context for intelligent AI responses.
- **Contextual resumption** - when users return to existing conversations, ResumeConversationMetadata provides intelligent re-engagement context to AI agents.

---

## 4. User Activity Integration Flow

The chat system seamlessly integrates with other app modules to create a contextually aware experience:

### **Activity-as-Message Architecture**
When users complete activities in other modules (videos, surveys, exercises, etc.), these activities are logged as special conversation messages with rich metadata including activity type, completion status, duration, and other contextual information. Other modules notify the ChatManager when activities are completed, seamlessly integrating user actions across the app into the conversation flow.

### **Three-Phase Activity Processing**
1. **Phase 1 (Parallel)**: Save activity message + Route to appropriate AI agent
2. **Phase 2 (Sequential)**: Generate contextual AI response using agent-specific instructions  
3. **Phase 3 (Parallel)**: Save AI response + Update user profile with activity context

### **Benefits of Activity Integration**
- **Contextual Awareness**: AI knows what user just did and can respond appropriately
- **Seamless Experience**: No jarring transitions between modules and chat
- **Rich Conversation History**: Activities become part of the ongoing conversation narrative
- **Intelligent Responses**: AI can congratulate, encourage, or provide follow-up based on activity completion
- **Profile Enhancement**: User activities continuously improve AI's understanding of the user

---

## 4.1. Resume Conversation Flow

The chat system provides intelligent conversation resumption when users return to existing chats:

### **ResumeConversationMetadata Architecture**
When users return to a chat after being away, the system can provide contextual information about the resumption:

- **Basic Resumption**: Default context message indicating the user has returned and wants to resume conversation smoothly
- **Time-Aware Resumption**: Context that includes information about how long the user was away (e.g., "3 hours")  
- **Activity-Based Resumption**: Context that acknowledges what the user did before returning (e.g., completed a workout, watched a video)
- **Custom Context**: Flexible additional metadata for any specific resumption scenario

The ChatManager uses this context to generate appropriate welcome-back responses through the AI system.

### **Resume Processing Flow**
1. **Detection**: ChatManager detects existing conversation (not new day, not first time)
2. **Context Evaluation**: If ResumeConversationMetadata provided, convert to activity format
3. **AI Routing**: Route resume context through HandleUserActivityCoroutine for agent selection
4. **Contextual Response**: Generate welcome-back message based on conversation history and resume context
5. **Smooth Transition**: Initialize chat normally after contextual re-engagement

### **Benefits of Resume Conversation**
- **Natural Re-engagement**: Eliminates awkward "where were we?" moments
- **Context-Aware Welcome**: AI acknowledges time away and smoothly transitions back
- **Flexible Context**: Support for time-based, activity-based, or custom resume contexts
- **Agent Intelligence**: Different resume contexts can route to specialized agents (e.g., welcome-back specialists)
- **Enhanced UX**: Creates more natural, human-like conversation flow resumption

---

## 5. Key System Components

| Component | Responsibility |
|------------|----------------|
| **Chat Manager (client)** | Controls state, flow, and API orchestration with enhanced caching and parallel execution. Handles user activity integration from other modules. |
| **Gamification Player APIs** | Provides conversations, messages, predefined flows, profiles, and instructions with bulk loading support. |
| **Router Service** | Determines which agent/topic should handle a user's message using full ChatMessage[] conversation context, including activity messages. |
| **AI Service** | Generates AI responses and handles profile updates from structured conversation context with streaming support. Responds contextually to user activities. |
| **UI Layer** | Displays rich ChatMessage objects with structured buttons, timestamps, and reacts to enhanced Chat Manager events. |
| **Other Modules (Videos, Surveys, etc.)** | Call `HandleUserActivity()` when users complete activities, seamlessly integrating with chat conversation flow. |

---

### Essence

> The chat system seamlessly blends scripted messages and AI-driven conversation.  
> The backend owns all logic and content; the Chat Manager ensures correct sequencing, persistence, and synchronization.

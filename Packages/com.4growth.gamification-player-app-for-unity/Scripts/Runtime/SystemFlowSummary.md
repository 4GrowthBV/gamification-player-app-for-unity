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
| **1. Initialization** | User opens the chat (e.g., “Wellbe Buddy”) | • Check backend for an existing conversation.<br>• If a conversation exists → load full conversation history.<br>• If it’s a new day → after loading history, fetch the next predefined message (`day_two`, `day_three`, etc.).<br>• If no conversation exists → create new conversation + profile, then load first message (`day_one`). | Chat opens with either:<br>– the user’s previous conversation, or<br>– the daily starting message if new.<br><br>Text input field is **always visible**, even in predefined flow. |
| **2. Predefined Flow** | User clicks a **predefined button** | • Add user action to conversation history.<br>• Save the message to backend.<br>• Load next predefined message via identifier.<br>• Update the profile after each message (based on last message + history).<br>• If next message has no buttons, the flow remains visually unchanged — the user can type manually at any point to switch to AI. | Chat continues as a structured, guided dialogue. Buttons show response options. The text field remains available at all times. |
| **3. AI Flow (Triggered by Free Text)** | User types and sends text in the input field | • Add user message to local history + backend.<br>• Switch to AI flow (`isInPredefinedFlow = false`).<br>• Call **Router service** to determine the correct agent/topic.<br>• Call **Memory service** to update the user profile based on last message and history.<br>• Fetch **instructions** for that agent from the database (never hardcoded).<br>• Generate AI response (future via OpenAI).<br>• Add AI message to history + backend.<br>• Update the profile again after AI response. | The user sees an intelligent, context-aware reply. The conversation becomes natural and continuous. |
| **4. Daily Continuation** | New day detected (system date or API) | • Reuse existing conversation and profile.<br>• Load history.<br>• Then load the next daily predefined message (e.g., `day_two`, `day_three`).<br>• Continue normally in predefined or AI flow. | The chat feels like an ongoing daily buddy interaction, remembering prior context. |
| **5. Persistence & Synchronization** | Continuous background operation | • Every message (user or bot) is stored locally and remotely.<br>• Profile updated after each message.<br>• Instructions always fetched from database.<br>• Conversation and profile IDs saved in local state for quick restoration. | The chat always remembers previous discussions, even after closing and reopening the app. |

---

## 3. Behavioral Rules

- **Text input is always visible.**  
  - If user types → switches or stays in **AI flow**.  
  - If user clicks button → stays in **predefined flow**.
- **Profile updates** occur after every message (both user and bot).
- **All messages, profiles, and instructions** are loaded and saved via backend APIs.
- **Daily messages** (day_one, day_two, etc.) are served by backend logic, not local timers.
- **Chat history** is persistent — users never lose prior conversations.

---

## 4. Key System Components

| Component | Responsibility |
|------------|----------------|
| **Chat Manager (client)** | Controls state, flow, and API orchestration. |
| **Gamification Player APIs** | Provides conversations, messages, predefined flows, profiles, and instructions. |
| **Router Service** | Determines which agent/topic should handle a user’s message. |
| **Memory Service** | Updates and retrieves user profile context. |
| **AI Service (future)** | Generates AI responses from combined context. |
| **UI Layer** | Displays chat messages, buttons, and text input; reacts to Chat Manager events. |

---

### Essence

> The chat system seamlessly blends scripted messages and AI-driven conversation.  
> The backend owns all logic and content; the Chat Manager ensures correct sequencing, persistence, and synchronization.

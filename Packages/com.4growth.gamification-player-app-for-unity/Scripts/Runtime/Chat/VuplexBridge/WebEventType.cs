namespace GamificationPlayer.Chat
{
    /// <summary>
    /// Event types for communication between web-based chat interfaces and Unity
    /// </summary>
    public enum ReceivedFromWebEventType
    {
        send_message,
        click_button,
        user_activity,
        force_new_conversation,
        get_conversation_history
    }

    /// <summary>
    /// Event types for communication between Unity and web-based chat interfaces
    /// </summary>
    public enum SentToWebEventType
    {
        // Initialization events
        chat_initialized,
        
        // Message events
        message_received,
        ai_message_final,
        ai_streaming_started,
        ai_message_chunk,
        ai_streaming_complete,
        
        // Error events
        error_occurred,
        
        // History and conversation events
        conversation_history,
        
        // RAG system events
        rag_status
    }
}
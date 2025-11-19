/**
 * VuplexBridge JavaScript API
 * Provides a clean interface between Unity ChatManager and web-based chat frontends
 * 
 * Communication Flow:
 * ChatManager (C#) <=> VuplexBridge (C#) <=> VuplexBridge (JS) <=> Frontend
 */

class VuplexBridge {
    constructor(options = {}) {
        this.debug = options.debug || false;
        this.eventHandlers = new Map();
        this.isInitialized = false;
        this.isConnected = false;
        
        // Initialize message handling
        this._initializeMessageHandling();
        
        this._log('VuplexBridge initialized', { debug: this.debug });
    }

    // ========================================
    // PUBLIC API - Event Registration
    // ========================================

    /**
     * Register event handler for Unity events
     * @param {string} eventType - Event type from Unity (see UNITY_EVENTS)
     * @param {Function} handler - Event handler function
     * @example
     * bridge.on('chat_initialized', (data) => {
     *   console.log('Chat ready!', data.expectNewMessage);
     * });
     */
    on(eventType, handler) {
        if (!this.eventHandlers.has(eventType)) {
            this.eventHandlers.set(eventType, []);
        }
        this.eventHandlers.get(eventType).push(handler);
        this._log(`Registered handler for: ${eventType}`);
    }

    /**
     * Unregister event handler
     * @param {string} eventType - Event type
     * @param {Function} handler - Handler to remove
     */
    off(eventType, handler) {
        const handlers = this.eventHandlers.get(eventType);
        if (handlers) {
            const index = handlers.indexOf(handler);
            if (index > -1) {
                handlers.splice(index, 1);
                this._log(`Unregistered handler for: ${eventType}`);
            }
        }
    }

    // ========================================
    // PUBLIC API - Send Actions to Unity
    // ========================================

    /**
     * Send a text message to the chat
     * @param {string} message - The message text
     * @example
     * bridge.sendMessage("Hello, how can you help me today?");
     */
    sendMessage(message) {
        if (!message || typeof message !== 'string') {
            throw new Error('Message must be a non-empty string');
        }
        this._sendToUnity(UNITY_ACTIONS.SEND_MESSAGE, { message: message.trim() });
    }

    /**
     * Click a button from a bot message
     * @param {string} buttonId - The button identifier
     * @example
     * bridge.clickButton('option_1');
     */
    clickButton(buttonId) {
        if (!buttonId || typeof buttonId !== 'string') {
            throw new Error('ButtonId must be a non-empty string');
        }
        this._sendToUnity(UNITY_ACTIONS.CLICK_BUTTON, { buttonId });
    }

    /**
     * Send user activity data
     * @param {Object} activityData - Activity metadata
     * @param {string} activityData.type - Activity type
     * @param {string} activityData.name - Activity name  
     * @param {string} [activityData.context] - Additional context
     * @param {string} [activityData.timestamp] - ISO timestamp
     * @example
     * bridge.sendUserActivity({
     *   type: 'page_view',
     *   name: 'Settings Page',
     *   context: 'User navigated to settings',
     *   timestamp: new Date().toISOString()
     * });
     */
    sendUserActivity(activityData) {
        if (!activityData || typeof activityData !== 'object') {
            throw new Error('ActivityData must be an object');
        }
        if (!activityData.type || !activityData.name) {
            throw new Error('ActivityData must have type and name properties');
        }
        
        const data = {
            type: activityData.type,
            name: activityData.name,
            context: activityData.context || '',
            timestamp: activityData.timestamp || new Date().toISOString(),
            ...activityData // Allow additional properties
        };
        
        this._sendToUnity(UNITY_ACTIONS.USER_ACTIVITY, { 
            activityData: JSON.stringify(data) 
        });
    }

    /**
     * Start a new conversation (clears current chat)
     * @example
     * bridge.startNewConversation();
     */
    startNewConversation() {
        this._sendToUnity(UNITY_ACTIONS.FORCE_NEW_CONVERSATION, {});
    }

    /**
     * Request conversation history
     * @example
     * bridge.requestConversationHistory();
     */
    requestConversationHistory() {
        this._sendToUnity(UNITY_ACTIONS.GET_CONVERSATION_HISTORY, {});
    }

    // ========================================
    // PUBLIC API - State Getters
    // ========================================

    /**
     * Check if bridge is connected to Unity
     * @returns {boolean}
     */
    getIsConnected() {
        return this.isConnected;
    }

    /**
     * Check if chat is initialized
     * @returns {boolean}  
     */
    getIsInitialized() {
        return this.isInitialized;
    }

    /**
     * Enable/disable debug logging
     * @param {boolean} enabled
     */
    setDebug(enabled) {
        this.debug = enabled;
        this._log(`Debug logging ${enabled ? 'enabled' : 'disabled'}`);
    }

    // ========================================
    // INTERNAL METHODS
    // ========================================

    _initializeMessageHandling() {
        // Listen for messages from Unity
        window.addEventListener('message', (event) => {
            if (event.data && event.data.eventType) {
                this._handleUnityMessage(event.data);
            }
        });
        
        // Initialize message queue if not exists
        if (!window._pullMessages) {
            window._pullMessages = [];
        }
    }

    _sendToUnity(action, data = {}) {
        const message = {
            action: action,
            ...data
        };
        
        this._log(`➤ Sending to Unity: ${action}`, data);
        
        const json = JSON.stringify(message);
        window._pullMessages.push(json);
    }

    _handleUnityMessage(eventData) {
        this._log(`➤ Received from Unity: ${eventData.eventType}`, eventData.data);
        
        // Update internal state
        this._updateInternalState(eventData);
        
        // Validate event data
        const validationResult = this._validateEventData(eventData);
        if (!validationResult.valid) {
            console.warn(`Invalid event data for ${eventData.eventType}:`, validationResult.errors);
        }
        
        // Emit to registered handlers
        const handlers = this.eventHandlers.get(eventData.eventType);
        if (handlers && handlers.length > 0) {
            handlers.forEach(handler => {
                try {
                    handler(eventData.data, eventData);
                } catch (error) {
                    console.error(`Error in handler for ${eventData.eventType}:`, error);
                }
            });
        } else {
            this._log(`No handlers registered for: ${eventData.eventType}`);
        }
    }

    _updateInternalState(eventData) {
        switch (eventData.eventType) {
            case 'chat_initialized':
                this.isInitialized = true;
                this.isConnected = true;
                break;
            case 'error_occurred':
                // Could update error state here
                break;
        }
    }

    _validateEventData(eventData) {
        const eventType = eventData.eventType;
        const data = eventData.data;
        const errors = [];

        // Get expected schema for this event type
        const schema = EVENT_SCHEMAS[eventType];
        if (!schema) {
            return { valid: true }; // Unknown events are valid (forward compatibility)
        }

        // Validate required fields
        if (schema.required) {
            schema.required.forEach(field => {
                if (data[field] === undefined || data[field] === null) {
                    errors.push(`Missing required field: ${field}`);
                }
            });
        }

        return { valid: errors.length === 0, errors };
    }

    _log(message, data = null) {
        if (this.debug) {
            const timestamp = new Date().toLocaleTimeString();
            console.log(`[VuplexBridge ${timestamp}] ${message}`, data || '');
        }
    }
}

// ========================================
// EVENT TYPE DEFINITIONS
// ========================================

/**
 * Events sent FROM Unity TO JavaScript
 */
const UNITY_EVENTS = {
    // Initialization
    CHAT_INITIALIZED: 'chat_initialized',
    
    // Messages
    MESSAGE_RECEIVED: 'message_received',
    AI_MESSAGE_FINAL: 'ai_message_final',
    
    // Streaming
    AI_STREAMING_STARTED: 'ai_streaming_started',
    AI_MESSAGE_CHUNK: 'ai_message_chunk',
    AI_STREAMING_COMPLETE: 'ai_streaming_complete',
    
    // Errors
    ERROR_OCCURRED: 'error_occurred',
    
    // History & Conversation
    CONVERSATION_HISTORY: 'conversation_history',
    
    // RAG System
    RAG_STATUS: 'rag_status'
};

/**
 * Actions sent FROM JavaScript TO Unity
 */
const UNITY_ACTIONS = {
    SEND_MESSAGE: 'send_message',
    CLICK_BUTTON: 'click_button',
    USER_ACTIVITY: 'user_activity',
    FORCE_NEW_CONVERSATION: 'force_new_conversation',
    GET_CONVERSATION_HISTORY: 'get_conversation_history'
};

// ========================================
// DATA TYPE SCHEMAS
// ========================================

/**
 * Expected data schemas for events from Unity
 * Used for validation and documentation
 */
const EVENT_SCHEMAS = {
    'chat_initialized': {
        required: ['status', 'connectionStatus', 'expectNewMessage'],
        optional: ['conversationHistory', 'timestamp'],
        description: 'Fired when chat system is initialized and ready',
        example: {
            status: 'initialized',
            connectionStatus: 'Connected', 
            expectNewMessage: false,
            conversationHistory: [],
            timestamp: '2024-01-01 12:00:00.000'
        }
    },
    
    'message_received': {
        required: ['role', 'message', 'timestamp'],
        optional: ['buttons', 'buttonName', 'userActivityMetadata'],
        description: 'Predefined bot message with optional buttons',
        example: {
            role: 'pre_defined-week1_day0',
            message: 'Welcome! How are you feeling today?',
            timestamp: '2024-01-01 12:00:00',
            buttons: [
                { identifier: 'feeling_good', text: 'Good' },
                { identifier: 'feeling_bad', text: 'Not so good' }
            ]
        }
    },
    
    'ai_message_final': {
        required: ['role', 'message', 'timestamp'],
        optional: ['buttonName', 'userActivityMetadata'],
        description: 'Final AI message replacing streaming content',
        example: {
            role: 'wellness_coach', 
            message: 'Complete response text after streaming',
            timestamp: '2024-01-01 12:00:00'
        }
    },
    
    'ai_streaming_started': {
        required: ['timestamp'],
        description: 'AI response streaming has begun',
        example: {
            timestamp: '2024-01-01 12:00:00.123'
        }
    },
    
    'ai_message_chunk': {
        required: ['chunk', 'timestamp', 'isStreaming'],
        description: 'Partial AI response chunk during streaming',
        example: {
            chunk: 'Hello, I can help you with...',
            timestamp: '2024-01-01 12:00:00.123',
            isStreaming: true
        }
    },
    
    'ai_streaming_complete': {
        required: ['timestamp'],
        description: 'AI response streaming has finished',
        example: {
            timestamp: '2024-01-01 12:00:00.456'
        }
    },
    
    'error_occurred': {
        required: ['error', 'timestamp'],
        description: 'An error occurred in ChatManager',
        example: {
            error: 'Failed to connect to AI service',
            timestamp: '2024-01-01 12:00:00.000'
        }
    },
    
    'conversation_history': {
        required: ['history'],
        description: 'Complete conversation history',
        example: {
            history: [
                {
                    role: 'user',
                    message: 'Hello',
                    timestamp: '2024-01-01 11:59:00'
                },
                {
                    role: 'wellness_coach',
                    message: 'Hi there! How can I help?',
                    timestamp: '2024-01-01 12:00:00'
                }
            ]
        }
    },
    
    'rag_status': {
        required: ['status'],
        optional: ['info', 'error'],
        description: 'RAG system status update',
        example: {
            status: 'ready',
            info: 'Vector database loaded with 1500 documents'
        }
    }
};

// ========================================
// UTILITY FUNCTIONS
// ========================================

/**
 * Create a new VuplexBridge instance with optional configuration
 * @param {Object} options - Configuration options
 * @param {boolean} options.debug - Enable debug logging
 * @returns {VuplexBridge}
 * @example
 * const bridge = createVuplexBridge({ debug: true });
 */
function createVuplexBridge(options = {}) {
    return new VuplexBridge(options);
}

/**
 * Get all available event types
 * @returns {Object} Object containing all event type constants
 */
function getEventTypes() {
    return {
        UNITY_EVENTS,
        UNITY_ACTIONS
    };
}

/**
 * Get schema for a specific event type
 * @param {string} eventType - The event type
 * @returns {Object|null} Schema object or null if not found
 */
function getEventSchema(eventType) {
    return EVENT_SCHEMAS[eventType] || null;
}

// ========================================
// EXPORTS
// ========================================

// For ES6 modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        VuplexBridge,
        createVuplexBridge,
        getEventTypes,
        getEventSchema,
        UNITY_EVENTS,
        UNITY_ACTIONS,
        EVENT_SCHEMAS
    };
}

// For global usage
if (typeof window !== 'undefined') {
    window.VuplexBridge = VuplexBridge;
    window.createVuplexBridge = createVuplexBridge;
    window.getEventTypes = getEventTypes;
    window.getEventSchema = getEventSchema;
    window.UNITY_EVENTS = UNITY_EVENTS;
    window.UNITY_ACTIONS = UNITY_ACTIONS;
    window.EVENT_SCHEMAS = EVENT_SCHEMAS;
}
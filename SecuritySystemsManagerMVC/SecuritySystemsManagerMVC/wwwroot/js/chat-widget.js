$(document).ready(function () {
    let chatConnection;
    let isChatOpen = false;
    let chatHistory = [];
    
    // Get current user ID from the page
    let currentUserIdInt = 0;
    try {
        // Try to get from data attribute if available
        const userIdElement = document.querySelector('[data-user-id]');
        if (userIdElement) {
            currentUserIdInt = parseInt(userIdElement.getAttribute('data-user-id')) || 0;
        }
    } catch (e) {
        console.warn('Could not get current user ID:', e);
    }
    
    console.log('Current user ID:', currentUserIdInt);
    
    // Check if SignalR is available on page load
    if (typeof signalR === 'undefined') {
        console.warn('SignalR library not loaded. Chat real-time functionality will be limited.');
    } else {
        console.log('SignalR library loaded successfully.');
    }

    // Initialize SignalR connection
    function initializeChatConnection() {
        // Check if SignalR is available
        if (typeof signalR === 'undefined') {
            console.warn('SignalR is not available. Chat functionality will be limited.');
            return;
        }
        
        chatConnection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();

        chatConnection.on("ReceiveMessage", function (message) {
            console.log("Received message:", message);
            
            // Hide welcome message if it exists
            $('#welcomeMessage').hide();
            
            appendChatMessage(message);
            if (!isChatOpen) {
                updateNotificationBadge();
            }
        });

        chatConnection.on("MessageSent", function (message) {
            console.log("Message sent confirmation:", message);
            // Don't append here as it will be received via ReceiveMessage
        });

        chatConnection.on("Error", function (error) {
            console.error("Chat error:", error);
        });

        chatConnection.start().catch(function (err) {
            console.error("Chat connection error:", err.toString());
        });
    }

    // Toggle chat widget
    $('#chatWidgetButton').click(function () {
        console.log('Chat button clicked!');
        if (isChatOpen) {
            closeChatWidget();
        } else {
            openChatWidget();
        }
    });

    $('#closeChatWidget').click(function () {
        closeChatWidget();
    });

    function openChatWidget() {
        console.log('Opening chat widget...');
        $('#chatWidgetPanel').show();
        isChatOpen = true;
        $('#chatWidgetButton').addClass('active');
        
        // Load chat history
        loadChatHistory();
        
        // Mark messages as read and clear notification badge
        markMessagesAsRead();
    }

    function closeChatWidget() {
        $('#chatWidgetPanel').hide();
        isChatOpen = false;
        $('#chatWidgetButton').removeClass('active');
    }

    // Send message
    $('#chatWidgetSendBtn').click(function () {
        sendChatMessage();
    });

    $('#chatWidgetInput').keypress(function (e) {
        if (e.which === 13) {
            e.preventDefault();
            sendChatMessage();
        }
    });

    function sendChatMessage() {
        const message = $('#chatWidgetInput').val().trim();
        if (!message) return;

        console.log('Sending message:', message);

        // Add message to local history immediately for better UX
        const tempMessage = {
            message: message,
            senderId: currentUserIdInt,
            timestamp: new Date().toISOString(),
            isFromCurrentUser: true
        };
        
        chatHistory.push(tempMessage);
        saveChatHistory();
        
        // Display message immediately
        const messageHtml = `
            <div class="chat-message chat-message-own">
                <div class="chat-message-content">
                    ${escapeHtml(message)}
                </div>
                <div class="chat-message-time">
                    ${formatTime(tempMessage.timestamp)}
                </div>
            </div>
        `;
        $('#chatWidgetContent').append(messageHtml);
        scrollChatToBottom();

        // Clear input immediately
        $('#chatWidgetInput').val('');

        // Send message via SignalR
        if (chatConnection && typeof signalR !== 'undefined') {
            console.log('Chat connection exists, invoking SendUserMessage...');
            chatConnection.invoke("SendUserMessage", message).catch(function (err) {
                console.error("Send message error:", err.toString());
                alert('Грешка при изпращане на съобщението. Моля, опитайте отново.');
            });
        } else {
            console.warn('Chat connection not available - SignalR may not be loaded');
            // Still show the message locally for better UX
            console.log('Message saved locally only');
        }
    }

    function appendChatMessage(message) {
        // Check if SignalR is available before processing messages
        if (typeof signalR === 'undefined') {
            console.warn('SignalR not available, skipping message processing');
            return;
        }
        
        const isFromCurrentUser = message.senderId === currentUserIdInt;
        
        // Don't add messages from current user as they are already added locally
        if (isFromCurrentUser) {
            console.log('Skipping message from current user as it was already added locally');
            return;
        }
        
        const messageClass = isFromCurrentUser ? 'chat-message-own' : 'chat-message-other';
        
        // Check if message already exists in history to avoid duplicates
        const messageExists = chatHistory.some(msg => 
            msg.message === message.message && 
            msg.senderId === message.senderId && 
            Math.abs(new Date(msg.timestamp) - new Date(message.timestamp)) < 1000 // Within 1 second
        );
        
        if (messageExists) {
            console.log('Message already exists, skipping...');
            return;
        }
        
        // Add message to history
        chatHistory.push({
            message: message.message,
            senderId: message.senderId,
            timestamp: message.timestamp || new Date().toISOString(),
            isFromCurrentUser: isFromCurrentUser
        });
        
        // Save to localStorage
        saveChatHistory();
        
        const messageHtml = `
            <div class="chat-message ${messageClass}">
                <div class="chat-message-content">
                    ${escapeHtml(message.message)}
                </div>
                <div class="chat-message-time">
                    ${formatTime(message.timestamp || new Date().toISOString())}
                </div>
            </div>
        `;

        $('#chatWidgetContent').append(messageHtml);
        scrollChatToBottom();
    }

    function scrollChatToBottom() {
        const container = $('.chat-widget-body');
        container.scrollTop(container[0].scrollHeight);
    }

    function updateNotificationBadge() {
        if (!isChatOpen) {
            $.get('/Chat/GetUnreadCount', function (data) {
                if (data.count > 0) {
                    $('#chatNotificationBadge').text(data.count).show();
                } else {
                    $('#chatNotificationBadge').hide();
                }
            });
        }
    }

    function loadChatHistory() {
        const savedHistory = localStorage.getItem('chatHistory_' + currentUserIdInt);
        if (savedHistory) {
            try {
                chatHistory = JSON.parse(savedHistory);
                displayChatHistory();
            } catch (e) {
                console.error('Error loading chat history:', e);
                chatHistory = [];
            }
        }
        
        // Always load existing messages from server to ensure we have the latest
        loadExistingMessages();
    }

    function loadExistingMessages() {
        $.get('/Chat/GetRecentMessages', { count: 20 })
            .done(function(response) {
                console.log('Loaded existing messages:', response);
                if (response && Array.isArray(response) && response.length > 0) {
                    // Clear existing history and load from server
                    chatHistory = [];
                    // Sort messages by timestamp (oldest first) so newest appear at bottom
                    const sortedMessages = response.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
                    sortedMessages.forEach(function(message) {
                        chatHistory.push({
                            message: message.message,
                            senderId: message.senderId,
                            timestamp: message.timestamp,
                            isFromCurrentUser: message.senderId === currentUserIdInt
                        });
                    });
                    saveChatHistory();
                    displayChatHistory();
                } else {
                    console.log('No existing messages found or invalid response format');
                }
            })
            .fail(function(err) {
                console.error('Error loading existing messages:', err);
            });
    }

    function saveChatHistory() {
        // Keep only last 50 messages to avoid localStorage limits
        if (chatHistory.length > 50) {
            chatHistory = chatHistory.slice(-50);
        }
        localStorage.setItem('chatHistory_' + currentUserIdInt, JSON.stringify(chatHistory));
    }

    function displayChatHistory() {
        if (chatHistory.length === 0) {
            $('#welcomeMessage').show();
            return;
        }

        $('#welcomeMessage').hide();
        $('#chatWidgetContent').empty();

        chatHistory.forEach(function(msg) {
            const messageClass = msg.isFromCurrentUser ? 'chat-message-own' : 'chat-message-other';
            const messageHtml = `
                <div class="chat-message ${messageClass}">
                    <div class="chat-message-content">
                        ${escapeHtml(msg.message)}
                    </div>
                    <div class="chat-message-time">
                        ${formatTime(msg.timestamp)}
                    </div>
                </div>
            `;
            $('#chatWidgetContent').append(messageHtml);
        });

        scrollChatToBottom();
    }

    function markMessagesAsRead() {
        // Mark all messages as read when chat is opened
        $.post('/Chat/MarkAllAsRead', function(data) {
            // Clear notification badge
            $('#chatNotificationBadge').hide();
            console.log('Messages marked as read');
        }).fail(function(err) {
            console.error('Error marking messages as read:', err);
        });
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatTime(timestamp) {
        // Handle both string and Date objects
        let date;
        if (typeof timestamp === 'string') {
            // Check if it's already in a formatted string (dd/MM/yyyy HH:mm)
            if (timestamp.includes('/')) {
                // Parse Bulgarian date format
                const parts = timestamp.split(' ');
                const dateParts = parts[0].split('/');
                const timeParts = parts[1].split(':');
                date = new Date(
                    parseInt(dateParts[2]), // year
                    parseInt(dateParts[1]) - 1, // month (0-based)
                    parseInt(dateParts[0]), // day
                    parseInt(timeParts[0]), // hour
                    parseInt(timeParts[1]) // minute
                );
            } else {
                // If it's a string, parse it as local time
                date = new Date(timestamp);
            }
        } else {
            date = new Date(timestamp);
        }
        
        const now = new Date();
        const diffInHours = (now - date) / (1000 * 60 * 60);
        
        if (diffInHours < 24) {
            return date.toLocaleTimeString('bg-BG', { 
                hour: '2-digit', 
                minute: '2-digit' 
            });
        } else {
            return date.toLocaleDateString('bg-BG', { 
                day: '2-digit',
                month: '2-digit',
                hour: '2-digit', 
                minute: '2-digit' 
            });
        }
    }

    // Initialize SignalR connection on page load
    initializeChatConnection();
    
    // Update notification badge on page load
    updateNotificationBadge();

    // Update notification badge every 30 seconds
    setInterval(updateNotificationBadge, 30000);
}); 
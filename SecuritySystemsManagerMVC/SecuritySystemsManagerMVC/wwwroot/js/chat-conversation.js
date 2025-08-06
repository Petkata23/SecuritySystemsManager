document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendButton = document.getElementById('sendMessage');
    let chatHistory = [];

    // Load chat history from localStorage
    function loadChatHistory() {
        const savedHistory = localStorage.getItem('chatHistory_' + currentUserId);
        if (savedHistory) {
            try {
                chatHistory = JSON.parse(savedHistory);
            } catch (e) {
                console.error('Error loading chat history:', e);
                chatHistory = [];
            }
        }
        
        // Load existing messages from server if no local history
        if (chatHistory.length === 0) {
            loadExistingMessages();
        }
    }

    // Load existing messages from server
    function loadExistingMessages() {
        fetch('/Chat/GetChatMessages/' + otherUserId)
            .then(response => response.json())
            .then(data => {
                if (data.success && data.messages && data.messages.length > 0) {
                    data.messages.forEach(message => {
                        chatHistory.push({
                            message: message.message,
                            senderName: message.senderName,
                            timestamp: message.timestamp,
                            isFromSupport: message.isFromSupport
                        });
                    });
                    saveChatHistory();
                }
            })
            .catch(err => console.error('Error loading existing messages:', err));
    }

    // Save chat history to localStorage
    function saveChatHistory() {
        // Keep only last 100 messages to avoid localStorage limits
        if (chatHistory.length > 100) {
            chatHistory = chatHistory.slice(-100);
        }
        localStorage.setItem('chatHistory_' + currentUserId, JSON.stringify(chatHistory));
    }

    // SignalR connection setup
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // SignalR event handlers
    connection.on("ReceiveMessage", (message) => {
        if (!message || !message.message) return;

        const newMessage = {
            message: message.message,
            isFromSupport: message.isFromSupport,
            senderName: message.isFromSupport ? message.senderName : message.senderName,
            timestamp: message.timestamp || new Date().toISOString()
        };
        
        appendMessage(newMessage);
    });

    // Start connection
    connection.start()
        .then(() => {
            console.log('Connected to SignalR');
            // Load chat history after connection is established
            loadChatHistory();
        })
        .catch(err => console.error('Error connecting to SignalR:', err));

    // Scroll to bottom on load
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Event listeners
    messageInput?.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    sendButton?.addEventListener('click', sendMessage);

    // Функция за добавяне на съобщение към чата
    function appendMessage(message) {
        if (!chatMessages) return;

        // Check if message already exists to avoid duplicates
        const existingMessages = chatMessages.querySelectorAll('.message');
        const messageExists = Array.from(existingMessages).some(msg => {
            const content = msg.querySelector('.message-content')?.textContent;
            const time = msg.querySelector('.time')?.textContent;
            return content === message.message && time === formatTime(message.timestamp);
        });

        if (messageExists) {
            console.log('Message already exists, skipping...');
            return;
        }

        // Add message to history
        chatHistory.push({
            message: message.message,
            senderName: message.senderName,
            timestamp: message.timestamp || new Date().toISOString(),
            isFromSupport: message.isFromSupport
        });
        
        // Save to localStorage
        saveChatHistory();

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${message.isFromSupport ? 'sent' : 'received'}`;
        messageDiv.innerHTML = `
            <div class="message-content">${escapeHtml(message.message)}</div>
            <div class="message-info">
                <span class="sender">${escapeHtml(message.senderName)}</span>
                <span class="time">${formatTime(message.timestamp)}</span>
            </div>
        `;
        
        chatMessages.appendChild(messageDiv);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Функция за изпращане на съобщение
    async function sendMessage() {
        if (!messageInput || !messageInput.value.trim()) return;

        try {
            const message = messageInput.value.trim();
            
            // Clear input immediately
            messageInput.value = '';
            
            // Add message locally for better UX
            const newMessage = {
                message: message,
                isFromSupport: false,
                senderName: 'Вие',
                timestamp: new Date().toISOString()
            };
            
            appendMessage(newMessage);
            
            // Изпращаме съобщението чрез SignalR
            await connection.invoke("SendUserMessage", message);
            
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Грешка при изпращане на съобщението. Моля, опитайте отново.');
        }
    }

    // Utility functions
    function escapeHtml(unsafe) {
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
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
}); 
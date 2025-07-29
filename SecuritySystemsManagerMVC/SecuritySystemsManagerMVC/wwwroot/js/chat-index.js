document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendButton = document.getElementById('sendMessage');
    const conversationsList = document.getElementById('conversationsList');
    let currentConversationId = null;

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
        })
        .catch(err => console.error('Error connecting to SignalR:', err));

    // Event listeners for conversations
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.addEventListener('click', handleConversationClick);
    });

    // Event listeners for chat
    messageInput?.addEventListener('keypress', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });

    sendButton?.addEventListener('click', sendMessage);

    // Функция за обработка на клик върху разговор
    function handleConversationClick(e) {
        e.preventDefault();
        const userId = this.dataset.userId;
        if (userId) {
            // Визуална индикация за избрания разговор
            document.querySelectorAll('.conversation-item').forEach(item => {
                item.classList.remove('active');
            });
            this.classList.add('active');
            
            // Активираме чата
            currentConversationId = parseInt(userId);
            messageInput.disabled = false;
            sendButton.disabled = false;
            messageInput.focus();
            
            // Зареждаме съобщенията
            loadConversation(userId);
        }
    }

    // Функция за зареждане на разговор
    async function loadConversation(userId) {
        try {
            const response = await fetch(`/Chat/Conversation/${userId}`);
            if (response.ok) {
                const html = await response.text();
                // Тук бихме могли да парснем HTML и да покажем съобщенията
                // За сега просто показваме, че разговорът е зареден
                console.log('Conversation loaded for user:', userId);
            }
        } catch (error) {
            console.error('Error loading conversation:', error);
        }
    }

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

        // Премахваме welcome съобщението ако е първото съобщение
        const welcomeMessage = chatMessages.querySelector('.welcome-message');
        if (welcomeMessage) {
            welcomeMessage.remove();
        }

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
        if (!messageInput || !messageInput.value.trim() || !currentConversationId) return;

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

    function formatTime(date) {
        return new Date(date).toLocaleTimeString('bg-BG', { hour: '2-digit', minute: '2-digit' });
    }
}); 
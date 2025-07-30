// Utility functions
// Функция за форматиране на времето
function formatTime(timestamp) {
    const date = new Date(timestamp);
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

// Функция за escape на HTML
function escapeHtml(text) {
    if (text === null || text === undefined) {
        return '';
    }
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('adminChatMessages');
    const messageInput = document.getElementById('adminMessageInput');
    const sendButton = document.getElementById('adminSendMessage');
    const chatMain = document.querySelector('.chat-main');
    const userInfoPanel = document.getElementById('userInfoPanel');
    const closeInfoPanelBtn = document.getElementById('closeInfoPanel');
    let currentUserId = currentChatUserId;

    // Добавяме event listener за затваряне на информационния панел
    closeInfoPanelBtn?.addEventListener('click', () => {
        userInfoPanel.classList.remove('active');
        setTimeout(() => userInfoPanel.style.display = 'none', 300);
    });

    // Функция за прикачване на event listeners към чат елементите
    function attachChatItemListeners() {
        document.querySelectorAll('.chat-list-item').forEach(item => {
            item.addEventListener('click', handleChatItemClick);
        });
    }

    // Функция за обработка на клик върху чат
    function handleChatItemClick(e) {
        e.preventDefault();
        const userId = e.currentTarget.dataset.userId;
        console.log('Chat item clicked, userId:', userId);
        
        if (userId) {
            // Визуална индикация за избрания чат
            document.querySelectorAll('.chat-list-item').forEach(item => {
                item.classList.remove('active');
            });
            e.currentTarget.classList.add('active');
            
            // Зареждане на чата
            loadChat(parseInt(userId));
        }
    }

    // Подобрена функция за обновяване на списъка с чатове
    async function updateChatList() {
        try {
            const response = await fetch('/Chat/GetActiveChats');
            if (!response.ok) throw new Error('Network response was not ok');
            
            const chats = await response.json();
            const chatList = document.getElementById('chatList');
            
            // Запазваме текущия активен чат
            const activeUserId = document.querySelector('.chat-list-item.active')?.dataset.userId;
            
            chatList.innerHTML = chats.map(chat => `
                <div class="chat-list-item ${chat.userId === parseInt(activeUserId) ? 'active' : ''}"
                     data-user-id="${chat.userId}">
                    <div class="chat-list-item-content">
                        ${chat.hasUnreadMessages ? '<span class="unread-indicator"></span>' : ''}
                        <div class="chat-list-item-header">
                            <h6 class="chat-username">${escapeHtml(chat.username)}</h6>
                            <small class="chat-time">${formatTime(chat.lastMessageTime)}</small>
                        </div>
                        <p class="chat-last-message">${escapeHtml(chat.lastMessage)}</p>
                        <div class="chat-item-footer">
                            <small class="chat-user-role">${escapeHtml(chat.userRole)}</small>
                        </div>
                    </div>
                </div>
            `).join('');

            // Прикачваме event listeners към новите елементи
            attachChatItemListeners();

            // Обновяваме статистиката
            updateStatistics();
            
        } catch (error) {
            console.error('Error updating chat list:', error);
        }
    }

    // Подобрена функция за обновяване на статистиката
    function updateStatistics() {
        // Обновяваме броя непрочетени съобщения
        const unreadCount = document.querySelectorAll('.unread-indicator').length;
        document.getElementById('unreadCount').textContent = unreadCount;
        
        // Обновяваме броя активни чатове
        const activeChatsCount = document.querySelectorAll('.chat-list-item').length;
        document.getElementById('activeChatsCount').textContent = activeChatsCount;
    }

    // SignalR connection setup
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    // SignalR event handlers
    connection.on("ReceiveMessage", (message) => {
        if (!message || !message.message) return;

        console.log("Admin received message:", message);

        // Check if this message is for the current chat
        const currentChatUserId = parseInt(currentUserId);
        const isForCurrentChat = message.senderId === currentChatUserId || 
                               message.recipientId === currentChatUserId ||
                               (!message.recipientId && !currentChatUserId); // General support messages

        if (isForCurrentChat) {
            // Don't add support messages from current user as they are already added locally
            const currentUserIdInt = currentAdminUserId;
            if (message.isFromSupport && message.senderId === parseInt(currentUserIdInt)) {
                console.log('Skipping support message from current user as it was already added locally');
                return;
            }

            const newMessage = {
                message: message.message,
                isFromSupport: message.isFromSupport,
                senderName: message.isFromSupport ? message.senderName : message.senderName,
                timestamp: message.timestamp || new Date().toISOString()
            };
            
            appendMessage(newMessage);
        }

        // Update chat list to show new messages
        updateChatList();
    });

    // Start connection
    connection.start()
        .then(() => {
            console.log('Connected to SignalR');
            updateChatList();
            setInterval(updateChatList, 30000);
        })
        .catch(err => console.error('Error connecting to SignalR:', err));

    // Първоначално прикачване на event listeners
    attachChatItemListeners();

    // Refresh button functionality
    const refreshButton = document.getElementById('refreshChats');
    if (refreshButton) {
        refreshButton.addEventListener('click', async () => {
            const icon = refreshButton.querySelector('i');
            if (icon) {
                icon.classList.add('spinning');
            }
            
            try {
                await updateChatList();
            } finally {
                if (icon) {
                    setTimeout(() => {
                        icon.classList.remove('spinning');
                    }, 500);
                }
            }
        });
    }

    // Scroll to bottom on load
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // Функция за зареждане на чат
    async function loadChat(userId) {
        console.log('Loading chat for userId:', userId);
        if (!userId) {
            console.warn('No userId provided');
            return;
        }

        const chatMain = document.querySelector('.chat-main');
        if (!chatMain) {
            console.error('Chat main container not found');
            return;
        }

        // Първо обновяваме структурата на чата ако е нужно
        if (!chatMain.querySelector('.chat-messages')) {
            chatMain.innerHTML = `
                <div class="chat-header"></div>
                <div class="chat-messages" id="adminChatMessages"></div>
                <div class="chat-input">
                    <div class="input-group">
                        <input type="text" class="form-control" id="adminMessageInput" placeholder="Въведете съобщение...">
                        <button class="btn btn-primary" id="adminSendMessage">
                            <i class="bi bi-send"></i>
                        </button>
                    </div>
                </div>
            `;

            // Прикачваме event listeners за изпращане на съобщения
            const newMessageInput = document.getElementById('adminMessageInput');
            const newSendButton = document.getElementById('adminSendMessage');

            newMessageInput?.addEventListener('keypress', (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    sendMessage();
                }
            });

            newSendButton?.addEventListener('click', sendMessage);
        }

        const chatMessages = document.getElementById('adminChatMessages');
        const messageInput = document.getElementById('adminMessageInput');
        const sendButton = document.getElementById('adminSendMessage');

        if (!chatMessages || !messageInput || !sendButton) {
            console.error('Required chat elements not found');
            return;
        }

        // Показваме loading състояние
        chatMain.classList.add('loading');
        messageInput.disabled = true;
        sendButton.disabled = true;

        try {
            const response = await fetch(`/Chat/GetChatMessages/${userId}`);
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.error || 'Грешка при зареждане на чата');
            }
            
            const data = await response.json();
            
            // Обновяваме UI за чата
            const chatHeader = chatMain.querySelector('.chat-header');
            if (chatHeader && data.user) {
                chatHeader.innerHTML = `
                    <div class="current-chat-info">
                        <div>
                            <h5>${escapeHtml(data.user.username)}</h5>
                            <small class="text-muted">${escapeHtml(data.user.role)}</small>
                        </div>
                        <div class="chat-actions">
                            <button class="btn btn-icon" id="markAsRead" title="Маркирай като прочетено">
                                <i class="bi bi-check2-all"></i>
                            </button>
                            <button class="btn btn-icon" id="showUserInfo" title="Информация">
                                <i class="bi bi-info-circle"></i>
                            </button>
                        </div>
                    </div>
                `;
            }

            // Обновяваме съобщенията
            chatMessages.innerHTML = data.messages.map(message => `
                <div class="message ${message.isFromSupport ? 'sent' : 'received'}">
                    <div class="message-content">${escapeHtml(message.message)}</div>
                    <div class="message-info">
                        <span class="sender">${escapeHtml(message.senderName)}</span>
                        <span class="time">${formatTime(message.timestamp)}</span>
                    </div>
                </div>
            `).join('');

            // Скролваме до последното съобщение
            chatMessages.scrollTop = chatMessages.scrollHeight;
            
            // Обновяваме currentUserId
            currentUserId = userId;
            
            // Активираме input полето
            messageInput.disabled = false;
            sendButton.disabled = false;
            messageInput.focus();

            // Прикачваме event listeners за новите бутони
            document.getElementById('markAsRead')?.addEventListener('click', handleMarkAsRead);
            document.getElementById('showUserInfo')?.addEventListener('click', handleShowUserInfo);
            
        } catch (error) {
            console.error('Error loading chat:', error);
            if (chatMessages) {
                chatMessages.innerHTML = `
                    <div class="error-message">
                        <i class="bi bi-exclamation-circle"></i>
                        <p>${error.message}</p>
                        <button class="btn btn-sm btn-primary" onclick="loadChat(${userId})">
                            <i class="bi bi-arrow-clockwise"></i> Опитай отново
                        </button>
                    </div>
                `;
            }
        } finally {
            chatMain.classList.remove('loading');
        }
    }

    // Функция за добавяне на съобщение към чата
    function appendMessage(message) {
        const chatMessages = document.getElementById('adminChatMessages');
        if (!chatMessages) {
            console.error('Chat messages container not found');
            return;
        }

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
        
        // Обновяваме последното съобщение в списъка с чатове
        const chatItem = document.querySelector(`.chat-list-item[data-user-id="${currentUserId}"]`);
        if (chatItem) {
            const lastMessageElement = chatItem.querySelector('.chat-last-message');
            if (lastMessageElement) {
                lastMessageElement.textContent = message.message;
            }
            
            const timeElement = chatItem.querySelector('.chat-time');
            if (timeElement) {
                timeElement.textContent = formatTime(message.timestamp);
            }
        }
    }

    // Функция за изпращане на съобщение
    async function sendMessage() {
        const messageInput = document.getElementById('adminMessageInput');
        const sendButton = document.getElementById('adminSendMessage');
        const chatMessages = document.getElementById('adminChatMessages');

        if (!messageInput || !sendButton || !currentUserId || !messageInput.value.trim() || !chatMessages) {
            console.error('Required elements not found or invalid input');
            return;
        }

        try {
            const message = messageInput.value.trim();
            
            // Clear input field immediately
            messageInput.value = '';
            
            // Add message locally for better UX
            const newMessage = {
                message: message,
                isFromSupport: true,
                senderName: 'Поддръжка',
                timestamp: new Date().toISOString()
            };
            
            appendMessage(newMessage);
            
            // Send via SignalR
            await connection.invoke("SendSupportMessage", currentUserId, message);
            
        } catch (error) {
            console.error('Error sending message:', error);
            alert('Грешка при изпращане на съобщението. Моля, опитайте отново.');
        }
    }

    // Функция за маркиране като прочетено
    async function handleMarkAsRead() {
        if (!currentUserId) return;

        try {
            const response = await fetch(`/Chat/MarkAllAsRead/${currentUserId}`, { method: 'POST' });
            if (!response.ok) throw new Error('Network response was not ok');
            
            // Обновяваме UI
            document.querySelectorAll('.unread-indicator').forEach(indicator => {
                indicator.remove();
            });
            
            // Обновяваме статистиката
            updateStatistics();
        } catch (err) {
            console.error('Error marking messages as read:', err);
        }
    }

    // Функция за показване на информация за потребителя
    async function handleShowUserInfo() {
        if (!currentUserId) return;
        
        const userInfoPanel = document.getElementById('userInfoPanel');
        userInfoPanel.style.display = 'block';
        setTimeout(() => userInfoPanel.classList.add('active'), 10);
        loadUserInfo(currentUserId);
    }

    function loadUserInfo(userId) {
        if (!userId) return;

        fetch(`/Chat/GetUserInfo/${userId}`)
            .then(response => response.json())
            .then(data => {
                document.getElementById('firstMessageDate').textContent = 
                    data.firstMessageDate ? new Date(data.firstMessageDate).toLocaleString('bg-BG') : '-';
                document.getElementById('totalMessages').textContent = data.totalMessages;
            })
            .catch(err => console.error('Error loading user info:', err));
    }
}); 
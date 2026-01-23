# Миграция към React Frontend

## Обобщение на промените

Проектът е преработен да поддържа React frontend, като backend-ът остава като API сървър.

## Backend промени

### 1. JWT Authentication
- Добавена е JWT authentication в `Program.cs`
- Конфигурирана е в `appsettings.json` под секцията `JwtSettings`
- Поддържа се и cookie authentication за обратна съвместимост с MVC views

### 2. CORS Configuration
- Добавена е CORS политика `AllowReactApp` за React приложението
- Разрешава заявки от `http://localhost:3000` и `http://localhost:5173`

### 3. API Controllers
Създадени са нови API контролери в `Controllers/Api/`:
- `AuthApiController.cs` - Authentication endpoints (login, register, logout, me)
- `BaseApiController.cs` - Базов контролер за CRUD операции
- `LocationApiController.cs` - Location API endpoints

### API Endpoints

#### Authentication
- `POST /api/auth/login` - Вход
- `POST /api/auth/register` - Регистрация  
- `POST /api/auth/logout` - Изход
- `GET /api/auth/me` - Текущ потребител

#### Locations
- `GET /api/location` - Списък с локации (с pagination)
- `GET /api/location/{id}` - Детайли за локация
- `GET /api/location/all` - Всички локации
- `POST /api/location/create` - Създаване на локация (Admin/Manager)
- `PUT /api/location/{id}` - Обновяване на локация
- `DELETE /api/location/{id}` - Изтриване на локация

## React Frontend

### Структура
Създадена е нова директория `client/` с React приложение използващо Vite.

### Основни компоненти
- **AuthContext** - Управление на authentication state
- **PrivateRoute** - Защитени routes
- **Login/Register pages** - Страници за вход и регистрация
- **Home page** - Начална страница след вход

### Services
- `authService.js` - Authentication API calls
- `locationService.js` - Location API calls

### Конфигурация
- `vite.config.js` - Proxy конфигурация за API заявки
- `src/config/api.js` - Axios конфигурация с interceptors

## Как да стартирате

### Backend
```bash
cd SecuritySystemsManagerMVC
dotnet run
```
Backend-ът ще работи на `https://localhost:7004`

### Frontend
```bash
cd client
npm install
npm run dev
```
Frontend-ът ще работи на `http://localhost:3000`

## Следващи стъпки

1. **Добавяне на останалите API контролери**
   - UserApiController
   - OrderApiController
   - DeviceApiController
   - InvoiceApiController
   - и т.н.

2. **Създаване на React компоненти**
   - Location management компоненти
   - Order management компоненти
   - User management компоненти
   - и т.н.

3. **Интеграция на SignalR**
   - За real-time chat функционалност

4. **UI/UX подобрения**
   - Добавяне на UI библиотека (Material-UI, Ant Design, etc.)
   - Подобряване на дизайна

## Важни бележки

- Старите MVC контролери и Views все още съществуват и могат да се използват
- JWT token се съхранява в localStorage
- CORS е конфигуриран за development сървъри
- За production, трябва да се обнови CORS политиката с правилните origins

## Проблеми и решения

### SSL Certificate Errors
Ако имате проблеми с SSL сертификатите при development, Vite proxy-то е конфигурирано с `secure: false`.

### CORS Errors
Уверете се, че CORS политиката в `Program.cs` включва правилния origin за вашия React app.

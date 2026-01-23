# Security Systems Manager - React Frontend

Това е React frontend приложението за Security Systems Manager.

## Инсталация

```bash
npm install
```

## Стартиране на development сървъра

```bash
npm run dev
```

Приложението ще се стартира на `http://localhost:3000`

## Конфигурация

API URL-то е конфигурирано в `vite.config.js` като proxy. По подразбиране сочи към `https://localhost:7004/api`.

Ако искате да промените API URL-то, създайте `.env` файл в root директорията на client проекта:

```
VITE_API_BASE_URL=https://localhost:7004/api
```

## Структура на проекта

- `src/config/` - Конфигурация на API клиента
- `src/contexts/` - React contexts (AuthContext)
- `src/services/` - API service слой
- `src/pages/` - Страници (Login, Register, Home, etc.)
- `src/components/` - React компоненти

## API Endpoints

### Authentication
- `POST /api/auth/login` - Вход
- `POST /api/auth/register` - Регистрация
- `POST /api/auth/logout` - Изход
- `GET /api/auth/me` - Текущ потребител

### Locations
- `GET /api/location` - Списък с локации (с pagination)
- `GET /api/location/{id}` - Детайли за локация
- `GET /api/location/all` - Всички локации
- `POST /api/location/create` - Създаване на локация
- `PUT /api/location/{id}` - Обновяване на локация
- `DELETE /api/location/{id}` - Изтриване на локация

## Build за production

```bash
npm run build
```

Build-натите файлове ще се намират в `dist/` директорията.

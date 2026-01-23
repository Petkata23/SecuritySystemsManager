# Инструкции за стартиране на приложението

## Стъпка 1: Стартиране на Backend (ASP.NET Core)

### Отворете терминал в root директорията на проекта:

```powershell
cd "c:\Users\GRIGS\Documents\SecuritySystemsManager\SecuritySystemsManager\SecuritySystemsManagerMVC\SecuritySystemsManagerMVC"
```

### Стартирайте backend сървъра:

```powershell
dotnet run
```

Или ако искате да използвате конкретен профил:

```powershell
dotnet run --launch-profile https
```

Backend-ът ще се стартира на:
- **HTTPS**: `https://localhost:7004`
- **HTTP**: `http://localhost:5290`

## Стъпка 2: Стартиране на Frontend (React)

### Отворете НОВ терминал (оставете backend-а да работи) и отидете в client директорията:

```powershell
cd "c:\Users\GRIGS\Documents\SecuritySystemsManager\SecuritySystemsManager\SecuritySystemsManagerMVC\client"
```

### Инсталирайте зависимостите (ако още не сте го направили):

```powershell
npm install
```

### Стартирайте React development сървъра:

```powershell
npm run dev
```

Frontend-ът ще се стартира на:
- **http://localhost:3000**

## Стъпка 3: Отворете приложението

Отворете браузър и отидете на:
```
http://localhost:3000
```

## Важни бележки:

1. **Два терминала**: Трябва да имате два терминала - един за backend и един за frontend
2. **Портове**: Уверете се, че портовете 7004 и 3000 не са заети от други приложения
3. **SSL сертификати**: При първо стартиране на HTTPS може да ви се покаже предупреждение за сертификат - това е нормално за development
4. **Database**: Уверете се, че базата данни е настроена и миграциите са приложени

## Бърз старт (PowerShell скрипт)

Можете да създадете файл `start.ps1` в root директорията:

```powershell
# Start Backend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'c:\Users\GRIGS\Documents\SecuritySystemsManager\SecuritySystemsManager\SecuritySystemsManagerMVC\SecuritySystemsManagerMVC'; dotnet run"

# Wait a bit for backend to start
Start-Sleep -Seconds 5

# Start Frontend
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'c:\Users\GRIGS\Documents\SecuritySystemsManager\SecuritySystemsManager\SecuritySystemsManagerMVC\client'; npm run dev"
```

След това просто изпълнете:
```powershell
.\start.ps1
```

## Проверка дали всичко работи:

1. Backend работи: Отворете `https://localhost:7004` в браузър
2. Frontend работи: Отворете `http://localhost:3000` в браузър
3. API работи: Отворете `https://localhost:7004/api/authapi/login` (трябва да върне 400 Bad Request, което е нормално без credentials)

## Често срещани проблеми:

### Порт вече е зает:
- Променете порта в `launchSettings.json` (backend) или `vite.config.js` (frontend)

### CORS грешки:
- Уверете се, че CORS политиката в `Program.cs` включва правилния origin

### Database connection:
- Проверете connection string в `appsettings.json`

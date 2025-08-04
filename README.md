# 🔒 Security Systems Manager

> **Enterprise-Grade Security Systems Management Platform**

A sophisticated, production-ready security systems management application built with ASP.NET Core 8, featuring advanced real-time communication, cloud storage integration, and comprehensive business process automation.

## 🏆 **Project Highlights**

### **🏗️ Architectural Excellence**
- **Clean Architecture** with perfect separation of concerns across 4 distinct layers
- **Repository Pattern** with generic base implementations for maximum code reuse
- **Service Layer** abstraction following SOLID principles
- **Dependency Injection** with automatic service registration using custom attributes
- **Layered Security** with multiple authentication mechanisms

### **⚡ Real-Time Communication System**
- **SignalR Integration** for live chat functionality
- **Admin Chat Panel** with user management and support features
- **Message Persistence** with read/unread status tracking
- **Typing Indicators** and online status monitoring
- **Role-based Chat Access** with admin support capabilities

### **☁️ Advanced Cloud Integration**
- **Dropbox API Integration** with automatic token refresh
- **Secure File Storage** with permanent link generation
- **Image Proxy System** with intelligent caching (24-hour cache)
- **Database Token Storage** with automatic renewal and persistence
- **OAuth 2.0 Integration** for secure app authorization
- **CORS Configuration** for secure cross-origin requests

### **🔐 Enterprise Security Features**
- **ASP.NET Core Identity** with custom User/Role entities
- **Two-Factor Authentication (2FA)** with QR code generation
- **Google OAuth Integration** for external authentication
- **JWT Token Support** for API authentication
- **Custom Password Hashing** using PBKDF2 with salt
- **Account Lockout Policies** with configurable thresholds

## 🚀 **Core Features**

### **📋 Order Management System**
- **Security System Orders** with comprehensive tracking
- **Order Status Management** with workflow automation
- **Location-based Organization** for geographic management
- **Client Relationship Management** with order history

### **🏠 Location & Device Management**
- **Customer Location Tracking** with detailed information
- **Interactive Maps** with Leaflet.js integration
- **Geographic Coordinates** with latitude/longitude support
- **Installed Device Management** with type categorization
- **Installation History** and device lifecycle tracking
- **Maintenance Scheduling** with automated notifications

### **💰 Financial Management**
- **Invoice Generation** and tracking system
- **PDF Export** with professional invoice templates
- **Payment Status Monitoring** with automated alerts
- **Financial Reporting** capabilities
- **Customer Billing** integration

### **🔧 Maintenance & Support**
- **Maintenance Logging** with detailed records
- **Device Maintenance History** tracking
- **Maintenance Device Management** with status tracking
- **Support Ticket System** via real-time chat
- **Automated Notifications** for maintenance schedules
- **Technician Assignment** and role-based access

### **📱 User Experience**
- **Responsive Design** using Bootstrap 5
- **Real-time Notifications** with SignalR
- **Admin Dashboard** with comprehensive overview
- **Interactive Maps** for location visualization
- **PDF Export** for invoices and reports
- **Mobile-friendly Interface** for field operations
- **Dark Theme** with modern UI design
- **Chat Widget** for instant support
- **Profile Management** with 2FA setup

## 🛠 **Technology Stack**

### **Backend Framework**
- **ASP.NET Core 8** - Latest .NET framework
- **Entity Framework Core 9** - Advanced ORM with lazy loading
- **SQL Server** - Enterprise database with full-text search

### **Real-Time Communication**
- **SignalR** - Real-time bidirectional communication
- **WebSocket Support** for live updates
- **Connection Management** with user tracking

### **Authentication & Security**
- **ASP.NET Core Identity** - Comprehensive identity management
- **JWT Bearer Tokens** - Stateless authentication
- **Google OAuth 2.0** - External authentication provider
- **Two-Factor Authentication** - Enhanced security

### **Cloud Services**
- **Dropbox API v2** - Cloud file storage
- **Database Token Storage** - Secure token persistence
- **Automatic Token Refresh** - Seamless cloud integration
- **Permanent Link Generation** - Direct file access
- **OAuth Integration** - Secure app authorization

### **Frontend Technologies**
- **Bootstrap 5** - Modern responsive framework
- **jQuery** - Enhanced JavaScript functionality
- **Leaflet.js** - Interactive maps for location management
- **jsPDF** - Client-side PDF generation
- **QR Code Generation** - 2FA setup support
- **SignalR Client** - Real-time communication
- **Bootstrap Icons** - Modern icon library
- **Google Fonts** - Typography optimization

### **Development Tools**
- **AutoMapper** - Object-to-object mapping
- **Dependency Injection** - IoC container
- **Entity Framework Core** - Advanced ORM
- **ASP.NET Core Identity** - Authentication framework
- **SignalR** - Real-time communication
- **Unit Testing** - Comprehensive test coverage

## 🏗 **Architecture Overview**

### **Layered Architecture Design**

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Controllers   │  │      Views      │  │ SignalR Hubs │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Business Logic Layer                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │  Base Services  │  │ Entity Services │  │ Token Mgmt   │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │ Base Repository │  │ Entity Repos    │  │ DbContext    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Shared Contracts                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │     DTOs        │  │   Interfaces    │  │   Enums      │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### **Project Structure**

```
SecuritySystemsManager/
├── SecuritySystemsManager.Data/                    # Data Access Layer
│   ├── Entities/                                   # Domain entities
│   │   ├── BaseEntity.cs                          # Base entity with audit fields
│   │   ├── User.cs                                # Custom Identity user
│   │   ├── Role.cs                                # Custom Identity role
│   │   ├── Location.cs                            # Customer locations
│   │   ├── SecuritySystemOrder.cs                 # Security orders
│   │   ├── InstalledDevice.cs                     # Installed devices
│   │   ├── MaintenanceLog.cs                      # Maintenance records
│   │   ├── Invoice.cs                             # Financial records
│   │   ├── Notification.cs                        # System notifications
│   │   ├── ChatMessage.cs                         # Real-time messages
│   │   └── DropboxToken.cs                        # Cloud storage tokens
│   ├── Repos/                                     # Repository implementations
│   │   ├── BaseRepository.cs                      # Generic repository base
│   │   ├── UserRepository.cs                      # User data access
│   │   ├── ChatMessageRepository.cs               # Chat data persistence
│   │   └── [Entity]Repository.cs                  # Entity-specific repos
│   ├── SecuritySystemsManagerDbContext.cs         # EF Core context
│   └── Migrations/                                # Database migrations
├── SecuritySystemsManager.Services/               # Business Logic Layer
│   ├── BaseCrudService.cs                         # Generic service base
│   ├── ChatMessageService.cs                      # Chat business logic
│   ├── DropboxStorageService.cs                   # Cloud storage service
│   ├── DropboxTokenManager.cs                     # Token management
│   ├── DropboxTokenRefreshService.cs              # Background token refresh
│   └── [Entity]Service.cs                         # Entity-specific services
├── SecuritySystemsManager.Shared/                 # Shared Components
│   ├── Dtos/                                      # Data Transfer Objects
│   │   ├── BaseDto.cs                             # Base DTO with audit fields
│   │   ├── UserDto.cs                             # User data transfer
│   │   ├── ChatMessageDto.cs                      # Chat message DTO
│   │   └── [Entity]Dto.cs                         # Entity-specific DTOs
│   ├── Enums/                                     # Enumerations
│   │   ├── DeviceType.cs                          # Device type categories
│   │   ├── OrderStatus.cs                         # Order status workflow
│   │   └── RoleType.cs                            # User role definitions
│   ├── Security/                                  # Security utilities
│   │   └── PasswordHasher.cs                      # Custom password hashing
│   ├── Attributes/                                # Custom attributes
│   │   └── AutoBindAttribute.cs                   # DI auto-registration
│   ├── Extensions/                                # Extension methods
│   │   └── ServiceCollectionExtensions.cs         # DI configuration
│   └── Services/Contracts/                        # Service interfaces
│       ├── IBaseCrudService.cs                    # Generic service contract
│       ├── IChatMessageService.cs                 # Chat service contract
│       ├── IFileStorageService.cs                 # File storage contract
│       └── [Entity]Service.cs                     # Entity service contracts
├── SecuritySystemsManagerMVC/                     # Presentation Layer
│   ├── Controllers/                               # MVC Controllers
│   │   ├── BaseCrudController.cs                  # Generic CRUD controller
│   │   ├── ChatController.cs                      # Chat management
│   │   ├── DropboxAuthController.cs               # Cloud auth management
│   │   ├── ImageProxyController.cs                # Image caching proxy
│   │   └── [Entity]Controller.cs                  # Entity-specific controllers
│   ├── Hubs/                                      # SignalR Hubs
│   │   └── ChatHub.cs                             # Real-time chat hub
│   ├── Views/                                     # Razor Views
│   │   ├── Shared/                                # Shared layouts
│   │   ├── Chat/                                  # Chat interface views
│   │   └── [Entity]/                              # Entity-specific views
│   ├── ViewModels/                                # View Models
│   │   ├── BaseVm.cs                              # Base view model
│   │   ├── Chat/                                  # Chat view models
│   │   └── [Entity]/                              # Entity view models
│   ├── wwwroot/                                   # Static files
│   │   ├── css/                                   # Stylesheets
│   │   ├── js/                                    # JavaScript files
│   │   └── lib/                                   # Client libraries
│   └── Program.cs                                 # Application entry point
└── SecuritySystemsManager.Tests/                  # Test Suite
    ├── Repos/                                     # Repository tests
    ├── Services/                                  # Service tests
    └── [Entity]Tests.cs                           # Entity-specific tests
```

## 🔧 **Advanced Features Deep Dive**

### **1. Real-Time Chat System**

The chat system is built with SignalR and features:

```csharp
// ChatHub.cs - Real-time communication
public class ChatHub : Hub
{
    private static readonly Dictionary<string, string> _userConnections = new();
    
    public async Task SendUserMessage(string message)
    {
        var userId = GetUserId();
        var user = await _userService.GetByIdIfExistsAsync(userId);
        
        // Store message in database
        var chatMessage = new ChatMessageDto
        {
            Message = message,
            SenderId = userId,
            SenderName = user.Username,
            Timestamp = DateTime.Now,
            IsFromSupport = false
        };
        
        await _chatService.CreateAsync(chatMessage);
        
        // Notify support staff in real-time
        var supportUsers = await _chatService.GetSupportUserIdsAsync();
        foreach (var supportUserId in supportUsers)
        {
            await Clients.User(supportUserId.ToString())
                .SendAsync("ReceiveMessage", chatMessage);
        }
    }
}
```

**Key Features:**
- **Real-time messaging** with SignalR WebSocket connections
- **Message persistence** in SQL Server database
- **Read/unread status** tracking
- **Typing indicators** for enhanced UX
- **Admin support panel** with user management
- **Connection management** with user tracking

### **2. Dropbox Integration with Database Token Storage**

Advanced cloud storage integration with secure database token management:

```csharp
// DropboxTokenManager.cs - Database-based token management
public class DropboxTokenManager
{
    private readonly object _lockObject = new object();
    private bool _isRefreshing = false;
    
    public async Task<string> GetAccessTokenAsync()
    {
        // Check memory cache first
        if (!string.IsNullOrEmpty(_accessToken) && 
            DateTime.UtcNow.AddMinutes(5) < _accessTokenExpiry)
        {
            return _accessToken;
        }
        
        // Load from database (primary storage)
        using (var scope = _serviceProvider.CreateScope())
        {
            var tokenRepository = scope.ServiceProvider
                .GetRequiredService<IDropboxTokenRepository>();
            var (accessToken, refreshToken, expiryTime) = 
                await tokenRepository.GetLatestTokenAsync();
            
            if (!string.IsNullOrEmpty(accessToken) && 
                DateTime.UtcNow.AddMinutes(5) < expiryTime)
            {
                // Update memory cache
                _accessToken = accessToken;
                _refreshToken = refreshToken;
                _accessTokenExpiry = expiryTime;
                return accessToken;
            }
        }
        
        // Refresh token if needed
        await RefreshAccessTokenAsync();
        return _accessToken;
    }
}

// DropboxTokenRepository.cs - Database token persistence
public class DropboxTokenRepository : IDropboxTokenRepository
{
    public async Task SaveTokenAsync(string accessToken, string refreshToken, DateTime expiryTime)
    {
        var existingToken = await _dbContext.DropboxTokens
            .OrderByDescending(t => t.UpdatedAt)
            .FirstOrDefaultAsync();

        if (existingToken != null)
        {
            // Update existing token
            existingToken.AccessToken = accessToken;
            existingToken.RefreshToken = refreshToken;
            existingToken.ExpiryTime = expiryTime;
            existingToken.UpdatedAt = DateTime.Now;
        }
        else
        {
            // Create new token record
            var newToken = new DropboxToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiryTime = expiryTime,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _dbContext.DropboxTokens.Add(newToken);
        }
        
        await _dbContext.SaveChangesAsync();
    }
}
```

**Key Features:**
- **Database as primary storage** for all tokens
- **Automatic token refresh** with background service
- **Memory caching** for performance optimization
- **Thread-safe operations** with lock mechanisms
- **Secure token persistence** with audit trails
- **Error handling** with retry logic

### **3. Interactive Maps with Leaflet.js**

Advanced location management with interactive maps:

```csharp
// Location entity with geographic coordinates
public class Location : BaseEntity
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    public string Latitude { get; set; }
    
    [Required]
    public string Longitude { get; set; }
    
    public string? Description { get; set; }
    
    public virtual ICollection<SecuritySystemOrder> Orders { get; set; }
}
```

**Key Features:**
- **Leaflet.js integration** for interactive maps
- **Geographic coordinates** storage and display
- **Location-based order management**
- **Real-time map updates** with location data
- **Mobile-responsive** map interface

### **4. PDF Export with jsPDF**

Professional invoice PDF generation:

```javascript
// invoice-pdf-export.js - Client-side PDF generation
function exportInvoiceToPdf() {
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF('p', 'mm', 'a4');
    
    // Capture invoice content as image
    html2canvas(document.querySelector('.invoice-container')).then(canvas => {
        const imgData = canvas.toDataURL('image/png');
        
        // Add company logo and invoice content
        pdf.addImage(imgData, 'PNG', xOffset, yOffset, imgWidth, imgHeight);
        
        // Generate filename with invoice details
        const filename = `Invoice_${invoiceNumber}_${invoiceDate}.pdf`;
        pdf.save(filename);
    });
}
```

**Key Features:**
- **Client-side PDF generation** using jsPDF
- **Professional invoice templates** with company branding
- **Automatic filename generation** with invoice details
- **High-quality rendering** with proper formatting
- **Print-friendly** design optimization

### **5. Image Proxy with Intelligent Caching**

```csharp
// ImageProxyController.cs - Intelligent image caching
public class ImageProxyController : Controller
{
    private static readonly Dictionary<string, (byte[] Data, string ContentType, DateTime Expiry)> 
        _imageCache = new Dictionary<string, (byte[] Data, string ContentType, DateTime Expiry)>();
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
    
    [HttpGet("imageproxy")]
    public async Task<IActionResult> GetImage([FromQuery] string url)
    {
        var decodedUrl = Uri.UnescapeDataString(url);
        
        // Check cache first
        if (_imageCache.TryGetValue(decodedUrl, out var cachedImage))
        {
            if (cachedImage.Expiry > DateTime.UtcNow)
            {
                return File(cachedImage.Data, cachedImage.ContentType);
            }
            _imageCache.Remove(decodedUrl);
        }
        
        // Fetch and cache new image
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(decodedUrl);
        
        if (response.IsSuccessStatusCode)
        {
            var imageData = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            
            // Cache the image
            _imageCache[decodedUrl] = (imageData, contentType, DateTime.UtcNow.Add(_cacheDuration));
            
            // Set browser cache headers
            Response.Headers.Add("Cache-Control", $"public, max-age={_cacheDuration.TotalSeconds}");
            return File(imageData, contentType);
        }
        
        return StatusCode((int)response.StatusCode, "Failed to load image.");
    }
}
```

**Key Features:**
- **24-hour image caching** for performance optimization
- **Automatic cache expiration** management
- **Browser cache headers** for client-side caching
- **Error handling** with graceful fallbacks
- **Memory-efficient** storage with byte arrays

### **6. Custom Password Hashing**

Enterprise-grade password security:

```csharp
// PasswordHasher.cs - PBKDF2 with salt
public static class PasswordHasher
{
    public static string HashPassword(string password)
    {
        byte[] salt = GetSalt();
        var hash = KeyDerivation.Pbkdf2(
            password, salt, 
            KeyDerivationPrf.HMACSHA256, 
            10000, 32);
        
        byte[] hashPassword = new byte[48];
        Buffer.BlockCopy(salt, 0, hashPassword, 0, 16);
        Buffer.BlockCopy(hash, 0, hashPassword, 16, 32);
        
        return Convert.ToBase64String(hashPassword);
    }
    
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword)) return false;
        
        byte[] hashedPasswordInBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[16];
        Buffer.BlockCopy(hashedPasswordInBytes, 0, salt, 0, 16);
        
        var hash = KeyDerivation.Pbkdf2(
            password, salt, 
            KeyDerivationPrf.HMACSHA256, 
            10000, 32);
        
        var hashBytes = new byte[32];
        Buffer.BlockCopy(hashedPasswordInBytes, 16, hashBytes, 0, 32);
        
        return hash.SequenceEqual(hashBytes);
    }
}
```

**Key Features:**
- **PBKDF2 algorithm** with 10,000 iterations
- **Cryptographic salt** generation
- **Secure verification** process
- **Industry-standard** security practices

### **7. Notification System**

Comprehensive notification management with real-time updates:

```csharp
// NotificationController.cs - User notification management
public class NotificationController : BaseCrudController<NotificationDto, INotificationRepository, INotificationService, NotificationEditVm, NotificationDetailsVm>
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            await _notificationService.MarkAsReadAsync(id, userId);
            return Json(new { success = true });
        }
        return Json(new { success = false });
    }

    public async Task<bool> SendOrderStatusChangeNotification(int orderId, int clientId, OrderStatus oldStatus, OrderStatus newStatus)
    {
        return await _notificationService.SendOrderStatusChangeNotificationAsync(orderId, clientId, oldStatus, newStatus);
    }
}
```

**Key Features:**
- **Real-time notifications** with SignalR integration
- **Order status change alerts** for clients
- **Read/unread status** tracking
- **Bulk operations** (mark all as read)
- **User-specific notifications** with filtering

### **8. Account Management & 2FA**

Advanced user account management with security features:

```csharp
// AccountController.cs - User profile and 2FA management
public class AccountController : Controller
{
    public async Task<IActionResult> EnableAuthenticator()
    {
        var user = await _userManager.GetUserAsync(User);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var model = new TwoFactorAuthenticationViewModel
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
        };

        return View(model);
    }
}
```

**Key Features:**
- **Two-factor authentication** with QR code setup
- **Profile management** with image upload
- **Password change** functionality
- **Recovery codes** generation
- **Authenticator app** integration

### **9. Error Handling & Custom Pages**

Professional error handling with custom error pages:

```csharp
// ErrorController.cs - Custom error handling
public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult Error404()
    {
        Response.StatusCode = 404;
        return View();
    }

    [Route("Error/500")]
    public IActionResult Error500()
    {
        Response.StatusCode = 500;
        return View();
    }

    [Route("Error/{statusCode}")]
    public IActionResult Error(int statusCode)
    {
        Response.StatusCode = statusCode;
        return statusCode switch
        {
            404 => View("Error404"),
            500 => View("Error500"),
            _ => View("Error500")
        };
    }
}
```

**Key Features:**
- **Custom error pages** for 404 and 500 errors
- **User-friendly error messages**
- **Proper HTTP status codes**
- **Consistent error handling** across application

### **10. Automatic Service Registration**

Smart dependency injection with custom attributes:

```csharp
// AutoBindAttribute.cs - Custom DI attribute
public class AutoBindAttribute : Attribute { }

// ServiceCollectionExtensions.cs - Automatic registration
public static class ServiceCollectionExtensions
{
    public static void AutoBind(this IServiceCollection source, params Assembly[] assemblies)
    {
        source.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.WithAttribute<AutoBindAttribute>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());
    }
}

// Program.cs - Usage
builder.Services.AutoBind(typeof(LocationService).Assembly);
builder.Services.AutoBind(typeof(LocationRepository).Assembly);
```

**Key Features:**
- **Automatic service discovery** using reflection
- **Interface-based registration** for loose coupling
- **Scoped lifetime** management
- **Reduced boilerplate** code

## 🚀 **Getting Started**

### **Prerequisites**
- **.NET 8 SDK** (Latest version)
- **SQL Server** (LocalDB or full instance)
- **Visual Studio 2022** or **VS Code**
- **Dropbox Developer Account** (for cloud storage)

### **Installation Steps**

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Petkata23/SecuritySystemsManager.git
   cd SecuritySystemsManager
   ```

2. **Configure Database Connection**
   ```json
   // appsettings.json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SecuritySystemsManager;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Configure Dropbox Integration**
   ```json
   // appsettings.json
   {
     "Dropbox": {
       "AppKey": "your-dropbox-app-key",
       "AppSecret": "your-dropbox-app-secret",
       "RootFolder": "SecuritySystemsManager"
     }
   }
   ```
   
   **Note**: Tokens are stored securely in the database, not in configuration files.
   
   **To set up your own Dropbox app:**
   1. Go to [Dropbox Developer Console](https://www.dropbox.com/developers/apps)
   2. Create a new app with "Full Dropbox" access
   3. Set the redirect URI to: `https://localhost:7004/DropboxAuth/Callback`
   4. Copy the App Key and App Secret to your configuration
   5. Navigate to `/DropboxAuth` in your application to authorize

4. **Run Database Migrations**
   ```bash
   cd SecuritySystemsManagerMVC
   dotnet ef database update
   ```

5. **Start the Application**
   ```bash
   dotnet run
   ```

6. **Access the Application**
   - Navigate to `https://localhost:7004`
   - Default admin credentials will be created during first run

### **Initial Setup**

1. **Create Admin User**
   - Register a new user account
   - Assign admin role through database or admin panel

2. **Configure Dropbox Authentication**
   - Navigate to `/DropboxAuth` in your application
   - Follow OAuth flow to authorize your Dropbox app
   - Tokens will be automatically managed and stored in database
   - **Admin role required** to access DropboxAuth controller

3. **Set Up Two-Factor Authentication**
   - Enable 2FA for admin accounts
   - Use QR code scanner for mobile app setup

## 📊 **Database Schema**

### **Core Entities**

```sql
-- Users and Authentication
Users (Id, Username, Email, PasswordHash, RoleId, CreatedAt, UpdatedAt)
Roles (Id, Name, Description, CreatedAt, UpdatedAt)
UserClaims, UserRoles, UserLogins, UserTokens

-- Business Entities
Locations (Id, Name, Address, Latitude, Longitude, Description, CreatedAt, UpdatedAt)
SecuritySystemOrders (Id, ClientId, LocationId, Status, OrderDate, Description, CreatedAt, UpdatedAt)
InstalledDevices (Id, OrderId, DeviceType, SerialNumber, InstallationDate, CreatedAt, UpdatedAt)
MaintenanceLogs (Id, DeviceId, MaintenanceDate, Description, TechnicianId, CreatedAt, UpdatedAt)
MaintenanceDevices (Id, MaintenanceLogId, DeviceId, IssueDescription, IsFixed, CreatedAt, UpdatedAt)
Invoices (Id, OrderId, Amount, Status, DueDate, IssuedOn, CreatedAt, UpdatedAt)

-- Communication and Storage
ChatMessages (Id, SenderId, RecipientId, Message, Timestamp, IsRead, IsFromSupport, CreatedAt, UpdatedAt)
Notifications (Id, UserId, Title, Message, IsRead, CreatedAt, UpdatedAt)
DropboxTokens (Id, AccessToken, RefreshToken, ExpiryTime, CreatedAt, UpdatedAt)
```

### **Key Relationships**
- **Users** → **Roles** (Many-to-One)
- **Users** → **Orders** (One-to-Many as Client)
- **Users** → **Notifications** (One-to-Many)
- **Users** → **ChatMessages** (One-to-Many as Sender/Recipient)
- **Locations** → **Orders** (One-to-Many)
- **Orders** → **InstalledDevices** (One-to-Many)
- **Orders** → **Invoices** (One-to-Many)
- **InstalledDevices** → **MaintenanceLogs** (One-to-Many)
- **MaintenanceLogs** → **MaintenanceDevices** (One-to-Many)
- **MaintenanceDevices** → **InstalledDevices** (Many-to-One)

## 🧪 **Testing Strategy**

### **Test Coverage**
- **Unit Tests** for all services and repositories
- **Integration Tests** for database operations
- **Controller Tests** for API endpoints
- **SignalR Tests** for real-time functionality

### **Running Tests**
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test SecuritySystemsManager.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 🔒 **Security Features**

### **Authentication & Authorization**
- **Multi-factor authentication** with TOTP
- **Role-based access control** (Admin, Manager, User)
- **Session management** with sliding expiration
- **Account lockout** after failed attempts

### **Data Protection**
- **Password hashing** with PBKDF2
- **HTTPS enforcement** in production
- **CORS configuration** for secure cross-origin requests
- **Input validation** at multiple layers

### **API Security**
- **JWT token authentication** for API access
- **Rate limiting** for API endpoints
- **Request validation** with model binding
- **Error handling** without information disclosure

## 📈 **Performance Optimizations**

### **Database Optimization**
- **Lazy loading** for related entities
- **Eager loading** for frequently accessed data
- **Pagination** for large datasets
- **Indexing** on frequently queried columns

### **Caching Strategy**
- **Image caching** with 24-hour expiration
- **Token caching** in memory and database
- **Browser caching** for static resources
- **SignalR connection** pooling

### **Memory Management**
- **Disposable pattern** implementation
- **Connection pooling** for database access
- **Stream management** for file operations
- **Garbage collection** optimization

## 🚀 **Deployment**

### **Production Deployment**
1. **Database Migration**
   ```bash
   dotnet ef database update --connection "ProductionConnectionString"
   ```

2. **Environment Configuration**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Production
   export ASPNETCORE_URLS=https://+:443
   ```

3. **SSL Certificate**
   ```bash
   dotnet dev-certs https --trust
   ```

### **Docker Deployment**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SecuritySystemsManagerMVC/SecuritySystemsManagerMVC.csproj", "SecuritySystemsManagerMVC/"]
RUN dotnet restore "SecuritySystemsManagerMVC/SecuritySystemsManagerMVC.csproj"
COPY . .
WORKDIR "/src/SecuritySystemsManagerMVC"
RUN dotnet build "SecuritySystemsManagerMVC.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SecuritySystemsManagerMVC.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecuritySystemsManagerMVC.dll"]
```


## 📝 **License**

This project is licensed under a **Custom License** - see the [LICENSE](LICENSE) file for details.

**Note**: This software is provided for educational and demonstration purposes only. All rights reserved.

## 📞 **Support & Contact**

- **GitHub Issues**: [Create an issue](https://github.com/Petkata23/SecuritySystemsManager/issues)
- **Documentation**: [Wiki](https://github.com/Petkata23/SecuritySystemsManager/wiki)
- **Email**: nedhristov36@gmail.com

## 🔄 **Version History**

- **v1.2.0** - Enhanced security features and Dropbox integration
- **v1.1.0** - Added real-time chat system with SignalR
- **v1.0.0** - Initial release with core functionality

---

## 🏆 **Why This Project Stands Out**

### **Enterprise-Grade Architecture**
- **Clean separation of concerns** across multiple layers
- **Scalable design** that can grow with business needs
- **Maintainable codebase** with consistent patterns
- **Testable architecture** with dependency injection
- **Generic base classes** for maximum code reuse

### **Advanced Real-Time Features**
- **SignalR integration** for live communication
- **Sophisticated chat system** with admin panel
- **Connection management** with user tracking
- **Message persistence** with read status
- **Typing indicators** and online status

### **Cloud Integration Excellence**
- **Database token storage** with secure persistence
- **Automatic token management** with refresh logic
- **Intelligent caching** for performance optimization
- **Secure file storage** with permanent links
- **OAuth 2.0 authorization** for app security
- **Image proxy system** with browser caching

### **User Experience Excellence**
- **Dark theme design** with modern UI/UX
- **Responsive mobile interface** for field operations
- **Real-time chat widget** for instant support
- **Interactive notifications** with dropdown menu
- **Profile management** with image upload
- **Professional error pages** with user-friendly messages
- **Interactive maps** with Leaflet.js integration

### **Security Best Practices**
- **Multi-factor authentication** implementation
- **Custom password hashing** with PBKDF2
- **Role-based access control** throughout
- **Comprehensive audit trails** with timestamps
- **Account lockout policies** with configurable thresholds

### **Production-Ready Features**
- **Comprehensive error handling** at all layers
- **Performance optimizations** with caching
- **Database migrations** for version control
- **Configuration management** for different environments
- **Unit testing** with comprehensive coverage
- **Professional documentation** and code comments

**This project demonstrates professional software engineering practices and is ready for production deployment in enterprise environments.**

---

**Built by Petkata using ASP.NET Core 8 and modern development practices**
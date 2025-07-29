using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Entities;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Repos.Contracts;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManager.Shared;
using SecuritySystemsManager.Shared.Attributes;
using SecuritySystemsManager.Shared.Extensions;
using SecuritySystemsManager.Shared.Services.Contracts;
using SecuritySystemsManagerMVC;
using SecuritySystemsManagerMVC.Hubs;
using System;
using System.Text.Encodings.Web;
using Microsoft.VisualBasic;
using Constants = SecuritySystemsManager.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR
builder.Services.AddSignalR();

// Add HttpClient for image proxy
builder.Services.AddHttpClient();

// Configure AutoMapper
builder.Services.AddAutoMapper(m => m.AddProfile(new AutoMapperConfiguration()));

// Add CORS policy for Dropbox
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDropbox", policy =>
    {
        policy.WithOrigins("https://dl.dropboxusercontent.com", "https://www.dropbox.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Dropbox services
builder.Services.AddSingleton<DropboxTokenManager>();
builder.Services.AddScoped<IFileStorageService, DropboxStorageService>();
builder.Services.AddHostedService<DropboxTokenRefreshService>();

// Register UrlEncoder for QR code generation
builder.Services.AddSingleton<UrlEncoder>(UrlEncoder.Default);

// Automatically bind services and repositories by convention
builder.Services.AutoBind(typeof(LocationService).Assembly);
builder.Services.AutoBind(typeof(LocationRepository).Assembly);
builder.Services.AutoBind(typeof(ChatMessageRepository).Assembly);

// Explicit registration for chat services
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();

// Configure DbContext with connection string
builder.Services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Конфигуриране на Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    // Настройки на паролата
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // Настройки за заключване на акаунт
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Настройки за потребител
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = Constants.AllowedUserNameCharacters;

    // Настройки за потвърждение на имейл
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<SecuritySystemsManagerDbContext>()
    .AddDefaultTokenProviders();

// Конфигуриране на cookie настройки
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// Add Identity UI
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error/500");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowDropbox");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");

app.Run();

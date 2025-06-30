using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SecuritySystemsManager.Data;
using SecuritySystemsManager.Data.Repos;
using SecuritySystemsManager.Services;
using SecuritySystemsManager.Shared.Extensions;
using SecuritySystemsManagerMVC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure AutoMapper
builder.Services.AddAutoMapper(m => m.AddProfile(new AutoMapperConfiguration()));


// Automatically bind services and repositories by convention
builder.Services.AutoBind(typeof(LocationService).Assembly);
builder.Services.AutoBind(typeof(LocationRepository).Assembly);

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// Configure DbContext with connection string
builder.Services.AddDbContext<SecuritySystemsManagerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SecuritySystemsManagerDbContext>();
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

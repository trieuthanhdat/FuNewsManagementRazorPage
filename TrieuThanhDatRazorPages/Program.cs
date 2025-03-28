﻿using FUNewsManagement.App.Services;
using FUNewsManagement.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using FUNewsManagement.Infrastructure.UnitOfWorks;
using FUNewsManagement.API.Hubs;
using FUNewsManagement.Infrastructure.Repositories;
using FUNewsManagement.App.Ultilities;
using FUNewsManagement.App.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using FUNewsManagement.Domain;
using FUNewsManagement.Domain.Entities;
using TrieuThanhDatRazorPages.Ultilities;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication & Session Configuration
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Dependency Injection (Repositories & Services)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
// Register Services
// Generic Repository for NewsArticles
builder.Services.AddScoped<IGenericRepository<NewsArticle>, GenericRepository<NewsArticle>>();
builder.Services.AddScoped<IGenericRepository<Tag>, GenericRepository<Tag>>();
builder.Services.AddScoped<IGenericRepository<Category>, GenericRepository<Category>>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<INewsService, NewsService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GlobalHelper>(); 

// Razor Pages & SignalR
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Seed the Default Admin Account
await SeedDefaultAdminAsync(app);

// Map SignalR Hub FIRST
app.MapHub<AccountHub>("/accountHub");
app.MapHub<NewsHub>("/newsHub");

// Middleware Pipeline
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Default Page Redirection (Ensuring `/Auth/Login` is Redirected Properly)
var defaultRedirectPage = builder.Configuration["DefaultRedirectPage"] ?? "/Auth/Login";
app.Use(async (context, next) =>
{
    // Prevent redirecting AJAX API calls
    if (context.Request.Path == "/" && !context.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            context.Response.Redirect("/Auth/Login", false);
        }
        else if (context.User.IsInRole("Admin"))
        {
            context.Response.Redirect("/Admin/Index", false);
        }
        else if (context.User.IsInRole("Staff"))
        {
            context.Response.Redirect("/Staff/StaffDashboard", false);
        }
        else if (context.User.IsInRole("Lecturer"))
        {
            context.Response.Redirect("/Lecturer/ManageNews", false);
        }
        else
        {
            context.Response.Redirect("/Auth/Login", false);
        }
        return;
    }
    await next();
});

app.MapRazorPages();
app.Run();

// Asynchronous Admin Seeding
async Task SeedDefaultAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<FunewsManagementContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    var defaultAdminEmail = config["DefaultAdmin:Email"];
    var defaultAdminPassword = config["DefaultAdmin:Password"];
    var defaultAdminRole = int.Parse(config["DefaultAdmin:Role"]);

    // Check if Admin already exists, also checking ROLE to avoid overrides
    var existingAdmin = await context.SystemAccounts
        .Where(u => u.AccountEmail == defaultAdminEmail && u.AccountRole == defaultAdminRole)
        .FirstOrDefaultAsync();

    if (existingAdmin == null)
    {  var hashedPassword = passwordHasher.HashPassword(defaultAdminPassword);
        var adminUser = new SystemAccount
        {
            AccountName = "Default Admin",
            AccountEmail = defaultAdminEmail,
            AccountPassword = hashedPassword,
            AccountRole = defaultAdminRole
        };

        await context.SystemAccounts.AddAsync(adminUser); 
        await context.SaveChangesAsync();
    }
}



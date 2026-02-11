using MtgDeckWebsite.Components;
using Microsoft.EntityFrameworkCore;
using MtgDeckWebsite.DataLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using MtgDeckWebsite.DataLayer.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MtgDeckWebsite.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add authentication & authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";  // redirect here if user not logged in
        options.LogoutPath = "/logout"; // optional
    });

builder.Services.AddAuthorization();

// Needed for login pages to access HttpContext
builder.Services.AddHttpContextAccessor();

// registers HttpClient for DI with BaseAddress
builder.Services.AddHttpClient("ServerAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BaseUrl"] ?? "https://localhost:7202/"); // fallback to localhost if BaseUrl not set (aka when running locally without env vars)
});
// register HttpClient for DI without BaseAddress, for use in services that need to call external APIs
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

// Add the auth state provider to handle authentication state in Blazor Server
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore(); // required for <AuthorizeView> in Blazor Server

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Add routing for authentication
app.UseRouting();

// Enable authentication + authorization
app.UseAuthentication();
app.UseAuthorization();

// Anti-forgery
app.UseAntiforgery();

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

#region API endpoints 
// todo - move these to a separate controller or minimal API file, and add more endpoints for registration, logout, etc.

// Login endpoint for the simple loginsystem
app.MapPost("/api/login", async (LoginRequest login, ApplicationDbContext db, IHttpContextAccessor httpContextAccessor) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
    if (user == null) return Results.Unauthorized();

    var hasher = new PasswordHasher<AppUser>();
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, login.Password);

    if (result != PasswordVerificationResult.Success) return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Email),
        new Claim("UserId", user.Id.ToString())
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.Ok();
});
#endregion

// Run the app
app.Run();
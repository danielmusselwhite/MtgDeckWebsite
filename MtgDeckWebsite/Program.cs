
using MtgDeckWebsite.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MtgDeckWebsite.DataLayer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

#region Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// todo - delete me
// Add Identity Razoor Pages
//builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();
#endregion

// Build the app
var app = builder.Build();

#region Blazor and Identity Endpoints
// todo - delete me
//app.MapRazorPages();           // Enable Razor Pages, including Identity pages (Login, Register, Logout)
//app.MapBlazorHub();            // Enable the SignalR hub for Blazor Server
//app.MapFallbackToPage("/_Host"); // Fallback for unmatched URLs: serves _Host.cshtml so Blazor routing can handle the request
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

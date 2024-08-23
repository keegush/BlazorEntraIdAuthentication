using BlazorEntraIdAuthentication.Components; // Reference to your Blazor components.
using Microsoft.AspNetCore.Authentication.OpenIdConnect; // Requires installation of Microsoft.AspNetCore.Authentication.OpenIdConnect package.
using Microsoft.Identity.Web; // Requires installation of Microsoft.Identity.Web package.

// Retrieve the client secret from an environment variable. 
// This is used for securely storing sensitive information like the client secret.
var environmentName = Environment.GetEnvironmentVariable("TestBlazorAuth", EnvironmentVariableTarget.Process);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure services for Razor components with interactive server components for Blazor Server.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Information;
});


// Configure authentication with Azure AD using OpenID Connect.
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        // Set Azure AD instance, tenant ID, client ID, and client secret.
        options.Instance = builder.Configuration["AzureAd:Instance"];
        options.TenantId = builder.Configuration["AzureAd:TenantId"];
        options.ClientId = builder.Configuration["AzureAd:ClientId"];
        options.ClientSecret = environmentName; // Client secret from environment variable.
        options.CallbackPath = builder.Configuration["AzureAd:CallbackPath"]; // Callback URL for authentication.
    });

// Add authorization services to handle access control in the application.
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Use exception handler in production to handle errors gracefully.
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Enforce the use of HTTPS by applying HSTS (HTTP Strict Transport Security).
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Serve static files from the wwwroot folder.
app.UseStaticFiles();

// Use Anti-forgery token protection, important for preventing CSRF attacks in forms.
app.UseAntiforgery();

// Apply authentication and authorization to the request pipeline.
app.UseAuthentication();
app.UseAuthorization();

// Map the Blazor application, enforce interactive server rendering and ensure that all users must be authenticated.
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .RequireAuthorization(); // Enforce that all users must be authenticated.

app.Run(); // Run the application.
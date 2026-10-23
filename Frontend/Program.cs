using Microsoft.AspNetCore.DataProtection;
using proyecto3.Frontend.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddRazorPages();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
string? url = builder.Configuration["Api:Url"];
if (string.IsNullOrWhiteSpace(url))
{
    url = "http://localhost:5303/";
}
ClienteApi cliente = new ClienteApi(url);
builder.Services.AddSingleton(typeof(ClienteApi), cliente);
WebApplication app = builder.Build();
app.Lifetime.ApplicationStopped.Register(cliente.Dispose);
app.UseExceptionHandler("/Error");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

using System.Globalization;
using Admin.Frontend;
using Admin.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-AR");
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Registra el HttpClient para apuntar a la URL del API Gateway en Docker
builder.Services.AddTransient<AuthHttpHandler>();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("http://localhost:8090");
}).AddHttpMessageHandler<AuthHttpHandler>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api"));

await builder.Build().RunAsync();

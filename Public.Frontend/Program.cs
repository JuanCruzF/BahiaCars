using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Public.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configura el HttpClient para que apunte a tu Gateway YARP
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:8090") });

await builder.Build().RunAsync();
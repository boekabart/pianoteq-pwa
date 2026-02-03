using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Pianoteq.Client;
using Pianoteq.Pwa;
using Pianoteq.Pwa.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add MudBlazor services
builder.Services.AddMudServices();

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Pianoteq client (configure your server URL here)
builder.Services.AddScoped(sp => new PianoteqClient("http://192.168.86.30:8081"));

// Add Favorites service
builder.Services.AddScoped<FavoritesService>();

await builder.Build().RunAsync();

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ChronoBastion.Web;
using ChronoBastion.Application;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<GameSession>();
builder.Services.AddScoped<ChronoBastion.Web.Services.BrowserHighScoreStore>();

await builder.Build().RunAsync();

using CoinGecko.Api;
using CoinGecko.Api.Samples.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCoinGeckoApi(opts =>
{
    opts.Plan = CoinGeckoPlan.Demo;
    // API key would be configured via environment / user input at runtime in a real app.
});

await builder.Build().RunAsync();

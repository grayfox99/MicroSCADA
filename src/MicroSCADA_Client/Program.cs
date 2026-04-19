using ApexCharts;
using MicroSCADA_Client.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddApexCharts();
builder.Services.AddSingleton<IOpcUaService, OpcUaService>();
builder.Services.AddSingleton<ITagHistoryService, TagHistoryService>();

var app = builder.Build();

// Eagerly construct TagHistoryService so it subscribes to IOpcUaService.DataChanged
// before any user hits Connect — otherwise the first few samples post-connect are lost.
_ = app.Services.GetRequiredService<ITagHistoryService>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MicroSCADA_Client.App>()
    .AddInteractiveServerRenderMode();

app.Run();

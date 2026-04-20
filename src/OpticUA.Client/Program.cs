using ApexCharts;
using OpticUA.Client.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddApexCharts();
builder.Services.AddSingleton<IOpcUaService, OpcUaService>();
builder.Services.AddSingleton<ITagHistoryService, TagHistoryService>();
builder.Services.AddSingleton<IDiagnosticsService, DiagnosticsService>();

var app = builder.Build();

// Eagerly construct so the services attach to IOpcUaService events before any
// user hits Connect — otherwise early samples/events land before the subscriber
// exists and are lost.
_ = app.Services.GetRequiredService<ITagHistoryService>();
_ = app.Services.GetRequiredService<IDiagnosticsService>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<OpticUA.Client.App>()
    .AddInteractiveServerRenderMode();

app.Run();

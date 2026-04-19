using ApexCharts;
using MicroSCADA_Client.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddApexCharts();
builder.Services.AddSingleton<IOpcUaService, OpcUaService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MicroSCADA_Client.App>()
    .AddInteractiveServerRenderMode();

app.Run();

using HaloUI.DemoHost;
using HaloUI.DemoHost.Services;
using HaloUI.DependencyInjection;
using HaloUI.Abstractions;
using HaloUI.Iconography.Packs.Material;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHaloUICore();
builder.Services.AddHaloUIMaterialIconPack(HaloMaterialIconStyle.Outlined);

builder.Services.AddHaloUIHttpThemePreferenceStore();
builder.Services.AddHaloUIDiagnostics(builder.Configuration);
builder.Services.AddScoped<IDialogContextProvider, StaticDialogContextProvider>();
builder.Services.AddScoped<DemoDialogState>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

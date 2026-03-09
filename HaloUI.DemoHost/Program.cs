// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.DemoHost;
using HaloUI.DemoHost.Services;
using HaloUI.DependencyInjection;
using HaloUI.Abstractions;
using HaloUI.Iconography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHaloUICore();

var materialPackPath = Path.Combine(builder.Environment.ContentRootPath, "icon-packs", "material-icons-regular.json");
if (File.Exists(materialPackPath))
{
    await using var materialPackStream = File.OpenRead(materialPackPath);
    var manifest = await HaloIconPackManifest.ParseAsync(materialPackStream);
    builder.Services.AddHaloUIIconPack(manifest, new PassthroughHaloIconResolver("material-icons"));
}
else
{
    builder.Services.AddHaloUIPassthroughLigatureIcons("material-icons");
}

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

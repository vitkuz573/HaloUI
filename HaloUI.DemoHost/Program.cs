// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.DemoHost;
using HaloUI.DemoHost.Services;
using HaloUI.DependencyInjection;
using HaloUI.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHaloUICore();
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

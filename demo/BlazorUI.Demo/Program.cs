using BlazorUI.Demo;
using BlazorUI.Demo.Services;
using BlazorUI.Primitives.Extensions;
using BlazorUI.Components.Toast;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add BlazorUI.Primitives services
builder.Services.AddBlazorUIPrimitives();

// Add theme service for dark mode management (scoped because it depends on IJSRuntime)
builder.Services.AddScoped<ThemeService>();

// Add collapsible state service for menu state persistence
builder.Services.AddScoped<CollapsibleStateService>();

// Add mock data service for generating demo data
builder.Services.AddSingleton<MockDataService>();

// Add toast notification service
builder.Services.AddScoped<ToastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

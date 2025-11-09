var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.Logger.LogInformation("Running in Development mode");
}

if (app.Environment.IsDevelopment() && !IsRunningInDocker())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// 👇 Quan trọng: bật HealthChecksUI middleware
app.UseHealthChecksUI(options =>
{
    options.UIPath = "/healthchecks-ui";
    options.ApiPath = "/healthchecks-api";
});

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecksUI();
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Logger.LogInformation("WebHealthStatus started on: {urls}", builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5010");

app.Run();

static bool IsRunningInDocker()
{
    return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}

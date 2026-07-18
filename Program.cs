using dream_team.Data;
using dream_team.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "dream_team";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";
var dbSslMode = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "disable";
var dbChannelBinding = Environment.GetEnvironmentVariable("DB_CHANNEL_BINDING") ?? "disable";

var connectionString =
    $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword};"
    + $"SSL Mode={dbSslMode};Trust Server Certificate=true;Channel Binding={dbChannelBinding}";

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<DbService>();
builder.Services.AddScoped<UserService>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie("External")
    .AddGoogle(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? "";
        options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? "";
        options.SignInScheme = "External";
        options.Events = new OAuthEvents
        {
            OnRedirectToAuthorizationEndpoint = context =>
            {
                Console.WriteLine($"Google authorize URL: {context.RedirectUri}");
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
        };
    })
    .AddDiscord(options =>
    {
        options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? "";
        options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET") ?? "";
        options.SignInScheme = "External";
        options.Events.OnRemoteFailure = context =>
        {
            var detail =
                context.Failure?.InnerException?.Message ?? context.Failure?.Message ?? "unknown";
            Console.WriteLine($"OAuth remote failure: {detail}");
            context.Response.Redirect("/Auth/Login?error=external");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

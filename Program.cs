using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Home");
    options.Conventions.AuthorizePage("/ChangePassword");
    options.Conventions.AllowAnonymousToPage("/SessionConfirm");
});

builder.Services.AddDbContext<AuthDbContext>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.AllowedForNewUsers = true;
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<AuthDbContext>()
  .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Error?statusCode=403";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
    options.SlidingExpiration = true;
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Login", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            context.Response.Redirect("/Error?statusCode=401&message=You%20must%20log%20in%20to%20access%20this%20page.");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddDataProtection();
builder.Services.AddHttpClient();
builder.Services.AddScoped<RecaptchaService>();
builder.Services.AddSingleton<UserDataProtectionService>();
builder.Services.AddScoped<EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);
        var sessionClaim = context.User.FindFirst("session_id")?.Value;

        if (user != null && !string.IsNullOrWhiteSpace(user.CurrentSessionId) && sessionClaim != user.CurrentSessionId)
        {
            if (!context.Request.Path.StartsWithSegments("/Login", StringComparison.OrdinalIgnoreCase) &&
                !context.Request.Path.StartsWithSegments("/Logout", StringComparison.OrdinalIgnoreCase) &&
                !context.Request.Path.StartsWithSegments("/SessionConfirm", StringComparison.OrdinalIgnoreCase))
            {
                await context.SignOutAsync();
                context.Response.Redirect("/Error?statusCode=401&message=You%20were%20signed%20out%20because%20another%20session%20logged%20in.");
                return;
            }
        }
    }

    await next();
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();

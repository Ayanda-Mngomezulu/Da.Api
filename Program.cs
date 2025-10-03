using System.Text;
using Daf.Api.Data;
using Daf.Api.Models;
using Daf.Api.Services;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// -- Configuration / Connection string
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ??
           builder.Configuration["ConnectionStrings:DefaultConnection"];

// DbContext
IServiceCollection serviceCollection = builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(conn));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not set");
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Hangfire (uses same SQL Server storage)
IServiceCollection serviceCollection1 = builder.Services.AddHangfire(h => h.UseSqlServerStorage(conn));
builder.Services.AddHangfireServer();

// register RecurringService so Hangfire can resolve it
builder.Services.AddScoped<RecurringService>();

// Add MVC (controllers + views) and API controllers
builder.Services.AddControllersWithViews(); // includes AddControllers
builder.Services.AddEndpointsApiExplorer();
object value = builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var app = builder.Build();

// Development vs Production behaviors
if (!app.Environment.IsDevelopment())
{
    // Production: use exception handler and HSTS
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Development: enable Swagger UI for API testing
    object value1 = app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication & Authorization (must be between UseRouting and endpoint mapping)
app.UseAuthentication();
app.UseAuthorization();

// Map controllers (API controllers)
app.MapControllers();

// Map MVC default route for Razor controllers/views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Hangfire dashboard (dev helper). Replace BasicAuth usage with secure solution for prod.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] {
        new BasicAuthAuthorizationFilter(
            new Hangfire.Dashboard.BasicAuthAuthorizationFilterOptions
            {
                SslRedirect = false,
                RequireSsl = false,
                LoginCaseSensitive = true,
                Users = new [] { new Dashboard.BasicAuthAuthorizationUser { Login = "admin", PasswordClear = "admin" } }
            })
    }
});

// ===== Recurring job schedule (runs daily, change to Minutely for quick tests) =====
RecurringJob.AddOrUpdate<RecurringService>(
    "recurring-donations",
    s => s.ProcessRecurringDonations(),
    Cron.Daily); // Cron.Minutely() for dev testing

// Seed roles & admin user at startup (await allowed in top-level statements)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}

app.Run();

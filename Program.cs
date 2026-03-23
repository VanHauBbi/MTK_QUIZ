using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Patterns.Creational;
using DALTWNC_QUIZ.Patterns.Decorator.Service;
using DALTWNC_QUIZ.Patterns.State;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var appKey = Guid.NewGuid().ToString();

builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=LapTopCuaGbao\\HAGIABAO;Database=QuizSystem;Trusted_Connection=True;TrustServerCertificate=True;"));


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = $"QuizAuth_{appKey}";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromHours(1); 
        options.SlidingExpiration = false;
        options.Cookie.IsEssential = true;

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

    });

builder.Services.AddSingleton<AppConfigurationManager>();

// === ??NG KÝ DECORATOR PATTERN CHO SUBMISSION SERVICE ===
builder.Services.AddScoped<ISubmissionService>(provider =>
{
    // 1. L?y ApplicationDbContext t? h? th?ng
    var context = provider.GetRequiredService<ApplicationDbContext>();

    // 2. L?p Lõi (Core): Ch? tính toán ?i?m s? d?a trên câu tr? l?i
    var basicService = new BasicSubmissionService(context);

    // 3. L?p B?c 1: L?u k?t qu? vào SQL Server (?i?m, s? câu ?úng...)
    var dbSaveDecorator = new DatabaseSavingDecorator(basicService, context);

    // 4. L?p B?c 2 (Ngoài cùng): Ki?m tra ?i?m >= 8 ?? in dòng chúc m?ng
    // Chúng ta tr? v? highCardDecorator luôn, không b?c thêm Logging n?a
    var highCardDecorator = new HighScoreAlertDecorator(dbSaveDecorator);

    return highCardDecorator;
});
// =========================================================




var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();

app.MapRazorPages();

app.MapFallbackToPage("/_Host");

app.Run();

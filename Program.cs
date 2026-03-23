using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Patterns.Behavioral;
using DALTWNC_QUIZ.Patterns.Creational;
using DALTWNC_QUIZ.Patterns.Structural;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var appKey = Guid.NewGuid().ToString();

builder.Services.AddRazorPages();

builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=LAPTOP-JINS5QEB;Database=QuizSystem;Trusted_Connection=True;TrustServerCertificate=True;"));


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- ĐĂNG KÝ BUILDER PATTERN (THÊM VÀO ĐÂY) ---
// Đăng ký IQuizBuilder để có thể sử dụng Dependency Injection
builder.Services.AddScoped<IQuizBuilder, QuizBuilder>();
// Đăng ký QuestionBuilder (thường dùng trực tiếp hoặc qua DI)
builder.Services.AddScoped<QuestionBuilder>();
// Đăng ký Director nếu bạn muốn dùng các kịch bản dựng sẵn
builder.Services.AddScoped<QuizDirector>();

// --- ĐĂNG KÝ STRATEGY PATTERN ---
// Mỗi khi ứng dụng cần IScoringStrategy, hãy cấp cho nó StandardScoringStrategy
builder.Services.AddScoped<IScoringStrategy, StandardScoringStrategy>();

// --- ĐĂNG KÝ FACADE PATTERN ---
builder.Services.AddScoped<IQuizFacade, QuizSubmissionFacade>();

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

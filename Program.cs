using DALTWNC_QUIZ.Data;
using DALTWNC_QUIZ.Patterns.Adapter;
using DALTWNC_QUIZ.Patterns.Behavioral;
using DALTWNC_QUIZ.Patterns.Creational;
using DALTWNC_QUIZ.Patterns.Decorator.Service;
using DALTWNC_QUIZ.Patterns.State;
using DALTWNC_QUIZ.Patterns.Structural;
using DALTWNC_QUIZ.Patterns.TemplateMethod;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var appKey = Guid.NewGuid().ToString();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=HAUBBI\\SQLEXPRESS;Database=QuizSystem;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<AppConfigurationManager>();

builder.Services.AddScoped<IQuizBuilder, QuizBuilder>();
builder.Services.AddScoped<QuestionBuilder>();
builder.Services.AddScoped<QuizDirector>();

builder.Services.AddScoped<IScoringStrategy, StandardScoringStrategy>();

builder.Services.AddScoped<IQuizFacade, QuizSubmissionFacade>();

builder.Services.AddScoped<ISubmissionService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    var basicService = new BasicSubmissionService(context);
    var dbSaveDecorator = new DatabaseSavingDecorator(basicService, context);
    var highCardDecorator = new HighScoreAlertDecorator(dbSaveDecorator);
    return highCardDecorator;
});

builder.Services.AddScoped<BasicQuizProcessor>();

builder.Services.AddScoped<IQuestionAdapter, ExternalQuestionAdapter>();


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
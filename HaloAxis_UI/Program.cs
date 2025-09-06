using HaloAxis_UI;
using HaloAxis_UI.Services;
using HaloAxis_UI.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// MVC + guard
builder.Services.AddControllersWithViews(o => o.Filters.Add(new RequireCompanyAttribute()));
builder.Services.AddRazorPages();

// Cookie auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
    });

// Session
builder.Services.AddSession(o =>
{
    o.Cookie.Name = ".HaloAxis.Session";
    o.IdleTimeout = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

// ---- Existing typed clients you already had ----
builder.Services.AddHttpClient<ICompanyApi, CompanyApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var erp = cfg["Api:ErpBaseUrl"] ?? cfg["Api:BaseUrl"];
    http.BaseAddress = new Uri(erp!);
});

builder.Services.AddHttpClient<IDepartmentApi, DepartmentApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:ErpBaseUrl"] ?? cfg["Api:BaseUrl"]!);
});

builder.Services.AddHttpClient<IDesignationApi, DesignationApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:ErpBaseUrl"] ?? cfg["Api:BaseUrl"]!);
});

// Auth API (5231)
builder.Services.AddHttpClient<IAuthApi, AuthApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:AuthBaseUrl"]!);
});

// ERP API (for other endpoints)
builder.Services.AddHttpClient<IUserCompanyApi, UserCompanyApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:ErpBaseUrl"]!);
});

// ---- Common, named ERP client everyone can use ----
builder.Services.AddHttpClient("ErpApi", (sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Api:ErpBaseUrl"] ?? throw new InvalidOperationException("Api:ErpBaseUrl missing.");
    http.BaseAddress = new Uri(baseUrl);
    http.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
});

// REQUIRED for ApiClient session access
builder.Services.AddHttpContextAccessor();

// One reusable API client (no per-resource interfaces needed)
builder.Services.AddScoped<ApiClient>();

var app = builder.Build();

// Routes
app.MapControllerRoute(
    name: "company",
    pattern: "Company/{action=Index}/{id?}",
    defaults: new { controller = "Company" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.Run();

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
// ERP base, e.g. http://18.219.50.5:5228
builder.Services.AddHttpClient<ICompanyApi, CompanyApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var erp = cfg["Api:ErpBaseUrl"] ?? cfg["Api:BaseUrl"]; // fallback if you only have BaseUrl
    http.BaseAddress = new Uri(erp!);
});
// after builder.Services.AddSession();

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

// ---- TWO TYPED HTTP CLIENTS ----
// Auth API (5231)
builder.Services.AddHttpClient<IAuthApi, AuthApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:AuthBaseUrl"]!);
});

// ERP API (5228) – UserCompany etc.
builder.Services.AddHttpClient<IUserCompanyApi, UserCompanyApi>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    http.BaseAddress = new Uri(cfg["Api:ErpBaseUrl"]!);
});

var app = builder.Build();
// add this BEFORE the default route
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

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

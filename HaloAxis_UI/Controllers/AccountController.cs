using HaloAxis_UI.Models;
using HaloAxis_UI.Services.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly IAuthApi _api;
    private readonly IUserCompanyApi _userCompanyApi;

    public AccountController(IAuthApi api, IUserCompanyApi userCompanyApi)
    {
        _api = api;
        _userCompanyApi = userCompanyApi;
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous, ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = await _api.RegisterAsync(new RegisterRequest(vm.FirstName, vm.LastName, vm.Email, vm.Password, vm.Gender));
        if (user is null) { ModelState.AddModelError("", "Registration failed."); return View(vm); }
        TempData["Flash"] = "Account created. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous, ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _api.LoginAsync(new LoginRequest(vm.Email, vm.Password));
        if (user is null || string.IsNullOrWhiteSpace(user.AccessToken))
        { ModelState.AddModelError("", "Invalid email or password."); return View(vm); }

        HttpContext.Session.SetString("AccessToken", user.AccessToken!);
        HttpContext.Session.SetString("RefreshToken", user.RefreshToken ?? "");

        var claims = new List<Claim>{
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Email, user.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = vm.RememberMe });

        var companies = await _userCompanyApi.GetByUserAsync(user.Id, user.AccessToken!);

        if (companies.Count == 0) return RedirectToAction("Create", "Company");
        if (companies.Count == 1)
        {
            HttpContext.Session.SetString("CompanyId", companies[0].CompanyId);
            return RedirectToAction("Index", "Dashboard");
        }
        return RedirectToAction("Select", "Company");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
}
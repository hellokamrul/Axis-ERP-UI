using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

public sealed class RequireCompanyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        var http = ctx.HttpContext;

        // Only enforce after auth
        if (http.User?.Identity?.IsAuthenticated != true) return;

        var path = http.Request.Path.Value ?? string.Empty;

        // ✅ ADD these two lines
        if (path.Equals("/company", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/company/index", StringComparison.OrdinalIgnoreCase) ||

            // existing allow-list
            path.StartsWith("/account", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/company/select", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/company/create", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/images", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrEmpty(http.Session.GetString("CompanyId")))
        {
            ctx.Result = new RedirectToActionResult("Select", "Company", null);
        }
    }
}

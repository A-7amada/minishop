using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace MiniShop.Web.Controllers;

// Switches the UI language by setting the standard culture cookie.
public class CultureController : Controller
{
    [HttpGet]
    public IActionResult Set(string culture, string? returnUrl)
    {
        if (culture is not ("en" or "ar")) culture = "en";
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
        return LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
}

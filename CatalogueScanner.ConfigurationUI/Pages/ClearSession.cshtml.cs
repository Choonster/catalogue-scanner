using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace CatalogueScanner.ConfigurationUI.Pages
{
    public class ClearSessionModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Uri? ReturnUrl { get; set; }

        public void OnGet()
        {
            Response.Cookies.Delete("AppServiceAuthSession");

            var hostUri = new UriBuilder(Request.GetEncodedUrl())
            {
                Path = string.Empty,
                Query = string.Empty,
                Fragment = string.Empty,
            }.Uri;

            var redirectUrl = hostUri.ToString();
            if (ReturnUrl is not null)
            {
                redirectUrl = ReturnUrl.IsAbsoluteUri && (ReturnUrl.Scheme != hostUri.Scheme || ReturnUrl.Host != hostUri.Host)
                    ? hostUri.AbsoluteUri
                    : new Uri(hostUri, ReturnUrl).AbsoluteUri;
            }

            Response.Redirect(redirectUrl);
        }
    }
}

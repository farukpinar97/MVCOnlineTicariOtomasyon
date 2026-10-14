using MVCOnlineTicariOtomasyon.Filters;
using System;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Hata sayfaları
    public class ErrorController : Controller
    {
        // /Error/NotFound
        public ActionResult NotFound()
        {
            // Tarayıcıya ve arama motorlarına gerçekten "bulunamadı" denir
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;

            ViewBag.RequestedPath = RequestedPath("404;");
            SetHomeLink();
            return View();
        }

        // /Error/Forbidden
        public ActionResult Forbidden()
        {
            Response.StatusCode = 403;
            Response.TrySkipIisCustomErrors = true;

            string authority = AdminAuthorizeAttribute.AuthorityOf(User);
            ViewBag.UserName = authority != null ? User.Identity.Name : null;
            ViewBag.AuthorityName = authority != null ? AdminAuthorizeAttribute.AuthorityName(authority) : null;
            ViewBag.RequestedPath = RequestedPath("403;");
            SetHomeLink();
            return View();
        }

        // İstenen adres
        private string RequestedPath(string prefix)
        {
            string query = Request.Url.Query.TrimStart('?');
            if (query.StartsWith(prefix, StringComparison.Ordinal))
            {
                Uri original;
                if (Uri.TryCreate(Uri.UnescapeDataString(query.Substring(prefix.Length)), UriKind.Absolute, out original))
                {
                    return original.PathAndQuery;
                }
            }

            string aspx = Request.QueryString["aspxerrorpath"];
            if (!string.IsNullOrEmpty(aspx))
            {
                return aspx;
            }

            // Sayfa doğrudan açıldıysa adres yazılmaz
            string path = Request.Url.AbsolutePath;
            return path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase) ? null : path;
        }

        // "Ana sayfaya dön" butonu kim giriş yaptıysa onun ana sayfasına gider
        private void SetHomeLink()
        {
            var identity = User == null ? null : User.Identity as FormsIdentity;
            bool isCurrent = identity != null && identity.IsAuthenticated && identity.Ticket.UserData == CurrentAuthorizeAttribute.UserType;

            if (AdminAuthorizeAttribute.AuthorityOf(User) != null)
            {
                ViewBag.HomeUrl = Url.Action("Index", "Statistic");
                ViewBag.HomeText = "Yönetim paneline dön";
            }
            else if (isCurrent)
            {
                ViewBag.HomeUrl = Url.Action("Index", "CurrentPanel");
                ViewBag.HomeText = "Cari paneline dön";
            }
            else
            {
                ViewBag.HomeUrl = Url.Action("Index", "Login");
                ViewBag.HomeText = "Giriş sayfasına git";
            }
        }
    }
}

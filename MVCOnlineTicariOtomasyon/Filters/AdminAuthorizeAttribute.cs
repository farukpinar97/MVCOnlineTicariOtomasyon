using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCOnlineTicariOtomasyon.Filters
{
    // Sadece YÖNETİCİ olarak giriş yapmış kullanıcıya izin verir
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public const string UserType = "Admin";
        public const string FullAccess = "A";
        public const string Limited = "B";

        // Bu sayfa için gereken rol
        public string Authority { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            string authority = AuthorityOf(httpContext.User);
            if (authority == null)
            {
                return false;   // yönetici değil
            }

            // Tam yetkili her yere girer
            return string.IsNullOrEmpty(Authority) || authority == FullAccess || authority == Authority;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (AuthorityOf(filterContext.HttpContext.User) != null)
            {
                // Giriş yapmış ama yetkisi yetmeyen yönetici
                filterContext.Result = new HttpStatusCodeResult(403);
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);   // 401
        }

        // Yardımcılar

        // Çereze yazılacak değer
        public static string UserDataFor(string authority)
        {
            return UserType + "|" + Normalize(authority);
        }

        // Giriş yapan yöneticinin rolü
        public static string AuthorityOf(IPrincipal user)
        {
            var identity = user == null ? null : user.Identity as FormsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return null;
            }

            string data = identity.Ticket.UserData ?? "";
            if (data == UserType)
            {
                return Limited;   // rol eklenmeden önce açılmış eski oturum
            }

            return data.StartsWith(UserType + "|") ? Normalize(data.Substring(UserType.Length + 1)) : null;
        }

        public static bool IsFullAccess(IPrincipal user)
        {
            return AuthorityOf(user) == FullAccess;
        }

        // Rolün ekranda görünen adı
        public static string AuthorityName(string authority)
        {
            return authority == FullAccess ? "Tam yetkili yönetici" : "Sınırlı yetkili yönetici";
        }

        private static string Normalize(string authority)
        {
            return (authority ?? "").Trim().ToUpperInvariant() == FullAccess ? FullAccess : Limited;
        }
    }
}

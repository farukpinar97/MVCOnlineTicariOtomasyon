using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCOnlineTicariOtomasyon.Filters
{
    // Sadece CARİ olarak giriş yapmış kullanıcıya izin verir
    public class CurrentAuthorizeAttribute : AuthorizeAttribute
    {
        public const string UserType = "Current";

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var identity = httpContext.User == null ? null : httpContext.User.Identity as FormsIdentity;

            return identity != null
                && identity.IsAuthenticated
                && identity.Ticket.UserData == UserType;
        }
    }
}

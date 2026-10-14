using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    public class LoginController : Controller
    {
        Context c = new Context();

        // Giriş sayfası
        public ActionResult Index()
        {
            return View();
        }

        // Cari girişi

        // Giriş formu
        [HttpGet]
        public PartialViewResult CurrentLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_CurrentLogin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CurrentLogin(string mail, string password, string returnUrl)
        {
            mail = Clean(mail);

            Current current = null;
            if (mail != null && !string.IsNullOrEmpty(password))
            {
                // Eski kayıtlarda aynı e-posta birden fazla caride olabilir
                current = c.Currents
                    .Where(x => x.CurrentMail == mail && x.Status == true)
                    .ToList()
                    .FirstOrDefault(x => x.CurrentPassword == password);
            }

            if (current == null)
            {
                // Hangisinin yanlış olduğu bilerek söylenmez
                const string error = "E-posta ya da şifre hatalı.";

                if (Request.IsAjaxRequest())
                {
                    ViewBag.Error = error;
                    ViewBag.Mail = mail;
                    ViewBag.ReturnUrl = returnUrl;
                    return PartialView("_CurrentLogin");
                }

                TempData["Error"] = error;
                return RedirectToAction("Index", new { returnUrl });
            }

            // Çereze cari NUMARASI yazılır
            SignIn(current.CurrentId.ToString(), CurrentAuthorizeAttribute.UserType);

            // Cari paneline giderken giriş istendiyse oraya, değilse panelin ana sayfasına dön
            string target = Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/CurrentPanel", StringComparison.OrdinalIgnoreCase)
                ? returnUrl
                : Url.Action("Index", "CurrentPanel");

            return GoTo(target);
        }

        // Yönetici girişi

        // Giriş formu
        [HttpGet]
        public PartialViewResult AdminLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_AdminLogin");
        }

        // Admins tablosundaki kullanıcı adı ve şifreyle giriş
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdminLogin(string userName, string password, string returnUrl)
        {
            userName = Clean(userName);

            Admin admin = null;
            if (userName != null && !string.IsNullOrEmpty(password))
            {
                // Şifre C# tarafında karşılaştırılır
                admin = c.Admins
                    .Where(x => x.AdminUsername == userName)
                    .ToList()
                    .FirstOrDefault(x => x.AdminPassword == password);
            }

            if (admin == null)
            {
                const string error = "Kullanıcı adı ya da şifre hatalı.";

                if (Request.IsAjaxRequest())
                {
                    ViewBag.Error = error;
                    ViewBag.UserName = userName;
                    ViewBag.ReturnUrl = returnUrl;
                    return PartialView("_AdminLogin");
                }

                TempData["Error"] = error;
                return RedirectToAction("Index", new { returnUrl });
            }

            // Yönetim panelinin üst çubuğunda User.Identity.Name olarak bu ad görünür
            SignIn(admin.AdminUsername, AdminAuthorizeAttribute.UserDataFor(Convert.ToString(admin.AdminAuthority)));

            bool adminPage = Url.IsLocalUrl(returnUrl)
                && !returnUrl.StartsWith("/CurrentPanel", StringComparison.OrdinalIgnoreCase)
                && !returnUrl.StartsWith("/Login", StringComparison.OrdinalIgnoreCase);

            return GoTo(adminPage ? returnUrl : Url.Action("Index", "Statistic"));
        }

        // Çıkış

        // Çıkış
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();

            TempData["Success"] = "Çıkış yaptın.";
            return RedirectToAction("Index");
        }

        // Cari kaydı

        // Kayıt formu
        [HttpGet]
        public PartialViewResult CurrentRegister()
        {
            return PartialView("_CurrentRegister");
        }

        // Kayıt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CurrentRegister(Current p, string passwordRepeat)
        {
            ValidateRegister(p, passwordRepeat);

            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    return PartialView("_CurrentRegister", p);
                }

                // Tarayıcıda JavaScript kapalıysa
                TempData["Error"] = ModelState.Values.SelectMany(x => x.Errors).First().ErrorMessage;
                return RedirectToAction("Index");
            }

            p.Status = true;   // kayıt olan cari aktif başlar
            c.Currents.Add(p);
            c.SaveChanges();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_CurrentRegisterDone", p);
            }

            TempData["Success"] = "Kaydın oluşturuldu.";
            return RedirectToAction("Index");
        }

        // Yardımcılar

        // Giriş başarılı
        private ActionResult GoTo(string target)
        {
            if (Request.IsAjaxRequest())
            {
                return Content("<div data-redirect=\"" + HttpUtility.HtmlAttributeEncode(target) + "\"></div>", "text/html");
            }

            return Redirect(target);
        }

        // Giriş çerezini yazar
        private void SignIn(string userName, string userType)
        {
            var ticket = new FormsAuthenticationTicket(
                1,   // sürüm
                userName,   // User.Identity.Name olacak değer
                DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout),   // süre Web.config'teki timeout'tan
                false,   // tarayıcı kapanınca çıkış yapılsın
                userType);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,   // JavaScript çereze erişemesin
                Path = FormsAuthentication.FormsCookiePath,
                Secure = FormsAuthentication.RequireSSL
            };

            Response.Cookies.Add(cookie);
        }

        // Kayıt formunun kontrolleri
        private void ValidateRegister(Current p, string passwordRepeat)
        {
            p.CurrentName = Clean(p.CurrentName);
            p.CurrentSurname = Clean(p.CurrentSurname);
            p.CurrentCity = Clean(p.CurrentCity);
            p.CurrentMail = Clean(p.CurrentMail);

            if (p.CurrentName == null)
            {
                ModelState.AddModelError("CurrentName", "Adını yaz.");
            }

            // E-posta girişte kullanıcı adı olduğu için zorunlu ve tek olmalı
            string mail = p.CurrentMail;
            if (mail == null)
            {
                ModelState.AddModelError("CurrentMail", "E-posta adresini yaz.");
            }
            else if (!new EmailAddressAttribute().IsValid(mail))
            {
                ModelState.AddModelError("CurrentMail", "Geçerli bir e-posta adresi yaz.");
            }
            else if (ModelState.IsValidField("CurrentMail") && c.Currents.Any(x => x.CurrentMail == mail))
            {
                ModelState.AddModelError("CurrentMail", "Bu e-posta adresiyle kayıtlı bir cari var.");
            }

            // Şifrenin başındaki ve sonundaki boşluklar da şifreye dahildir, kırpılmaz
            if (string.IsNullOrEmpty(p.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Bir şifre belirle.");
            }
            else if (p.CurrentPassword.Length < 6)
            {
                ModelState.AddModelError("CurrentPassword", "Şifre en az 6 karakter olmalı.");
            }
            else if (p.CurrentPassword != passwordRepeat)
            {
                ModelState.AddModelError("PasswordRepeat", "Şifreler aynı değil.");
            }
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        // İstek bitince veritabanı bağlantısını serbest bırak
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                c.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

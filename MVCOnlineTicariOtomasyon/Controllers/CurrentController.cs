using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Sadece tam yetkili yönetici
    [AdminAuthorize(Authority = AdminAuthorizeAttribute.FullAccess)]
    public class CurrentController : Controller
    {
        Context c = new Context();

        // Aktif cariler
        public ActionResult Index()
        {
            var values = c.Currents.Where(x => x.Status == true).ToList();

            // Her carinin satış sayısı
            ViewBag.SalesCounts = c.Currents
                .Where(x => x.Status == true)
                .Select(x => new { x.CurrentId, Count = x.SalesTransactions.Count() })
                .ToDictionary(x => x.CurrentId, x => x.Count);

            return View(values);
        }

        // Ekleme formu
        public ActionResult CurrentAdd()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CurrentAdd(Current cur)
        {
            ValidateCurrent(cur);
            if (!ModelState.IsValid)
            {
                return View(cur);
            }

            cur.Status = true;
            c.Currents.Add(cur);
            c.SaveChanges();

            TempData["Success"] = $"“{cur.CurrentName} {cur.CurrentSurname}” eklendi.";
            return RedirectToAction("Index");
        }

        // Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CurrentDelete(int id)
        {
            var cr = c.Currents.Find(id);
            if (cr == null)
            {
                return HttpNotFound();
            }

            cr.Status = false;
            c.SaveChanges();

            TempData["Success"] = $"“{cr.CurrentName} {cr.CurrentSurname}” silindi.";
            return RedirectToAction("Index");
        }

        // Güncelleme formu
        public ActionResult CurrentGet(int id)
        {
            var cr = c.Currents.Find(id);
            if (cr == null)
            {
                return HttpNotFound();
            }

            return View("CurrentGet", cr);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CurrentUpdate(Current cr)
        {
            var current = c.Currents.Find(cr.CurrentId);
            if (current == null)
            {
                return HttpNotFound();
            }

            ValidateCurrent(cr);
            if (!ModelState.IsValid)
            {
                // Formu, yazılanlar ve hata mesajlarıyla birlikte geri göster
                return View("CurrentGet", cr);
            }

            current.CurrentName = cr.CurrentName;
            current.CurrentSurname = cr.CurrentSurname;
            current.CurrentCity = cr.CurrentCity;
            current.CurrentMail = cr.CurrentMail;
            c.SaveChanges();

            TempData["Success"] = $"“{current.CurrentName} {current.CurrentSurname}” güncellendi.";
            return RedirectToAction("Index");
        }

        // Bir carinin satış geçmişi
        public ActionResult CurrentSales(int id)
        {
            var current = c.Currents.Find(id);
            if (current == null)
            {
                return HttpNotFound();
            }

            var values = c.SalesTransactions
                .Include(x => x.Product)
                .Include(x => x.Personnel)
                .Where(x => x.CurrentId == id)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ToList();

            ViewBag.CurrentName = current.CurrentName + " " + current.CurrentSurname;
            return View(values);
        }

        // Ekleme ve güncellemede ortak
        private void ValidateCurrent(Current cur)
        {
            cur.CurrentName = Clean(cur.CurrentName);
            cur.CurrentSurname = Clean(cur.CurrentSurname);
            cur.CurrentCity = Clean(cur.CurrentCity);
            cur.CurrentMail = Clean(cur.CurrentMail);

            if (cur.CurrentName == null)
            {
                ModelState.AddModelError("CurrentName", "Ad boş bırakılamaz.");
            }

            // E-posta isteğe bağlı
            if (cur.CurrentMail != null && !new EmailAddressAttribute().IsValid(cur.CurrentMail))
            {
                ModelState.AddModelError("CurrentMail", "Geçerli bir e-posta adresi yaz.");
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

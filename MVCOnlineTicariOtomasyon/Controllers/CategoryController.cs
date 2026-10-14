using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using PagedList;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class CategoryController : Controller
    {
        Context c = new Context();

        // Listede bir sayfada kaç kategori görünsün
        private const int PageSize = 10;

        // Liste
        public ActionResult Index(string q, int page = 1)
        {
            q = Clean(q);

            var query = c.Categories.Include(x => x.Products);
            if (q != null)
            {
                query = query.Where(x => x.CategoryName.Contains(q));
            }

            // Sayfalama için sıralama şart
            var values = query.OrderBy(x => x.CategoryId).ToPagedList(Math.Max(page, 1), PageSize);

            // Olmayan bir sayfa istendiyse son sayfaya git
            if (page > 1 && page > values.PageCount)
            {
                return RedirectToAction("Index", new { q, page = values.PageCount > 1 ? values.PageCount : (int?)null });
            }

            ViewBag.Query = q;
            return View(values);
        }

        // Ekleme formu
        public ActionResult CategoryAdd()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryAdd(Category k)
        {
            ValidateCategory(k);
            if (!ModelState.IsValid)
            {
                // Hata varsa formu, yazılanlar ve hata mesajıyla birlikte geri göster
                return View(k);
            }

            c.Categories.Add(k);
            c.SaveChanges();

            TempData["Success"] = $"“{k.CategoryName}” eklendi.";
            return RedirectToAction("Index");
        }

        // Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        // page ve q, silme butonunun adresinden gelir
        public ActionResult CategoryDelete(int id, int page = 1, string q = null)
        {
            var category = c.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // İçinde ürün olan kategori silinmez
            int productCount = c.Categories.Where(x => x.CategoryId == id).SelectMany(x => x.Products).Count();
            if (productCount > 0)
            {
                TempData["Error"] = $"“{category.CategoryName}” silinemedi: içinde {productCount} ürün var. Önce ürünleri başka bir kategoriye taşı ya da sil.";
                return BackToList(page, q);
            }

            c.Categories.Remove(category);
            c.SaveChanges();

            TempData["Success"] = $"“{category.CategoryName}” silindi.";
            return BackToList(page, q);
        }

        // Güncelleme formu
        public ActionResult CategoryGet(int id)
        {
            var category = c.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            return View("CategoryGet", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CategoryUpdate(Category k)
        {
            var category = c.Categories.Find(k.CategoryId);
            if (category == null)
            {
                return HttpNotFound();
            }

            ValidateCategory(k);
            if (!ModelState.IsValid)
            {
                return View("CategoryGet", k);
            }

            category.CategoryName = k.CategoryName;
            c.SaveChanges();

            TempData["Success"] = $"“{category.CategoryName}” güncellendi.";
            return RedirectToAction("Index");
        }

        // Ekleme ve güncellemede ortak kontroller
        private void ValidateCategory(Category k)
        {
            string name = (k.CategoryName ?? "").Trim();
            int id = k.CategoryId;
            k.CategoryName = name;

            if (name == "")
            {
                ModelState.AddModelError("CategoryName", "Kategori adı boş bırakılamaz.");
            }
            else if (c.Categories.Any(x => x.CategoryName == name && x.CategoryId != id))
            {
                ModelState.AddModelError("CategoryName", $"“{name}” adında bir kategori zaten var.");
            }
        }

        // Listeye, silmeden önceki sayfa ve aramayla dön
        private ActionResult BackToList(int page, string q)
        {
            return RedirectToAction("Index", new { q = Clean(q), page = page > 1 ? page : (int?)null });
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

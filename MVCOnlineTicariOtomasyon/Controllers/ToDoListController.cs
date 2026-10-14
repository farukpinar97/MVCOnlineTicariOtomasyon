using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class ToDoListController : Controller
    {
        Context c = new Context();

        // Üstte dört rakam, altta yapılacaklar listesi
        public ActionResult Index()
        {
            // Rakamlarda sadece aktif kayıtlar sayılır
            ViewBag.CurrentCount = c.Currents.Count(x => x.Status == true);
            ViewBag.ProductCount = c.Products.Count(x => x.Status == true);
            ViewBag.CategoryCount = c.Categories.Count();

            // DÜZELTME
            ViewBag.CityCount = c.Currents
                .Where(x => x.Status == true && x.CurrentCity != null && x.CurrentCity != "")
                .Select(x => x.CurrentCity)
                .Distinct()
                .Count();

            // Önce yapılmamış işler, sonra tamamlananlar
            var values = c.ToDoLists
                .OrderBy(x => x.Status)
                .ThenBy(x => x.ToDoListId)
                .ToList();

            return View(values);
        }

        // Yeni iş ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToDoAdd(string header)
        {
            header = (header ?? "").Trim();

            if (header == "")
            {
                TempData["Error"] = "Eklemek için bir iş yaz.";
                return RedirectToAction("Index");
            }

            c.ToDoLists.Add(new ToDoList { ToDoListHeader = header, Status = false });
            c.SaveChanges();

            return RedirectToAction("Index");
        }

        // İşi tamamlandı yap ya da tamamlandıdan geri al
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToDoToggle(int id)
        {
            var item = c.ToDoLists.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            item.Status = !item.Status;
            c.SaveChanges();

            return RedirectToAction("Index");
        }

        // İşi sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToDoDelete(int id)
        {
            var item = c.ToDoLists.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }

            c.ToDoLists.Remove(item);
            c.SaveChanges();

            TempData["Success"] = $"“{item.ToDoListHeader}” silindi.";
            return RedirectToAction("Index");
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

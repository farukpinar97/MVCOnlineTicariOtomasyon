using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Sadece tam yetkili yönetici
    [AdminAuthorize(Authority = AdminAuthorizeAttribute.FullAccess)]
    public class DepartmentController : Controller
    {
        Context c = new Context();

        // Aktif departmanlar
        public ActionResult Index()
        {
            var values = c.Departments.Include(x => x.Personnels).Where(x => x.Status == true).ToList();
            return View(values);
        }

        // Ekleme formu
        public ActionResult DepartmentAdd()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DepartmentAdd(Department d)
        {
            ValidateDepartment(d);
            if (!ModelState.IsValid)
            {
                return View(d);
            }

            d.Status = true;   // yeni departman aktif başlar
            c.Departments.Add(d);
            c.SaveChanges();

            TempData["Success"] = $"“{d.DepartmentName}” eklendi.";
            return RedirectToAction("Index");
        }

        // Silme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DepartmentDelete(int id)
        {
            var dep = c.Departments.Find(id);
            if (dep == null)
            {
                return HttpNotFound();
            }

            dep.Status = false;
            c.SaveChanges();

            TempData["Success"] = $"“{dep.DepartmentName}” silindi.";
            return RedirectToAction("Index");
        }

        // Güncelleme formu
        public ActionResult DepartmentGet(int id)
        {
            var dep = c.Departments.Find(id);
            if (dep == null)
            {
                return HttpNotFound();
            }

            return View("DepartmentGet", dep);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DepartmentUpdate(Department d)
        {
            var dep = c.Departments.Find(d.DepartmentId);
            if (dep == null)
            {
                return HttpNotFound();
            }

            ValidateDepartment(d);
            if (!ModelState.IsValid)
            {
                return View("DepartmentGet", d);
            }

            dep.DepartmentName = d.DepartmentName;
            c.SaveChanges();

            TempData["Success"] = $"“{dep.DepartmentName}” güncellendi.";
            return RedirectToAction("Index");
        }

        // Departmanın personelleri
        public ActionResult DepartmentDetail(int id)
        {
            var department = c.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            // DÜZELTME
            var values = c.Departments
                .Where(x => x.DepartmentId == id)
                .SelectMany(x => x.Personnels)
                .OrderBy(x => x.PersonnelName)
                .ToList();

            ViewBag.DepartmentName = department.DepartmentName;
            return View(values);
        }

        // Bir personelin yaptığı satışlar
        public ActionResult DepartmentPersonnelSales(int id)
        {
            var personnel = c.Personnels.Find(id);
            if (personnel == null)
            {
                return HttpNotFound();
            }

            var values = c.SalesTransactions
                .Include(x => x.Product)
                .Include(x => x.Current)
                .Where(x => x.PersonnelId == id)
                .OrderByDescending(x => x.SalesTransactionDate)
                .ToList();

            ViewBag.PersonnelName = personnel.PersonnelName + " " + personnel.PersonnelSurname;

            // "Departmana dön" linki için personelin departmanı
            ViewBag.DepartmentId = c.Departments
                .Where(x => x.Personnels.Any(p => p.PersonnelId == id))
                .Select(x => (int?)x.DepartmentId)
                .FirstOrDefault();

            return View(values);
        }

        // Ekleme ve güncellemede ortak kontroller
        private void ValidateDepartment(Department d)
        {
            string name = (d.DepartmentName ?? "").Trim();
            int id = d.DepartmentId;
            d.DepartmentName = name;

            if (name == "")
            {
                ModelState.AddModelError("DepartmentName", "Departman adı boş bırakılamaz.");
            }
            else if (c.Departments.Any(x => x.DepartmentName == name && x.DepartmentId != id && x.Status == true))
            {
                ModelState.AddModelError("DepartmentName", $"“{name}” adında bir departman zaten var.");
            }
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

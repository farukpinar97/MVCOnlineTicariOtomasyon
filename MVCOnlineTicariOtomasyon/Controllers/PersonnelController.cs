using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.Entities;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Sadece tam yetkili yönetici
    [AdminAuthorize(Authority = AdminAuthorizeAttribute.FullAccess)]
    public class PersonnelController : Controller
    {
        Context c = new Context();

        // Fotoğraf yükleme ayarları
        private const string ImageFolder = "~/Images/Personnel/";
        private const int MaxImageBytes = 2 * 1024 * 1024;   // 2 MB
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        // Bütün personeller, ada göre sıralı
        public ActionResult Index()
        {
            var values = c.Personnels
                .Include(x => x.Department)
                .OrderBy(x => x.PersonnelName)
                .ThenBy(x => x.PersonnelSurname)
                .ToList();

            // Her personelin satış sayısı
            ViewBag.SalesCounts = c.Personnels
                .Select(x => new { x.PersonnelId, Count = x.SalesTransactions.Count() })
                .ToDictionary(x => x.PersonnelId, x => x.Count);

            return View(values);
        }

        // Ekleme formu
        public ActionResult PersonnelAdd()
        {
            FillDepartments(0);
            return View();
        }

        // imageFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonnelAdd(Personnel p, HttpPostedFileBase imageFile)
        {
            ValidatePersonnel(p);
            ValidateImage(imageFile);
            if (!ModelState.IsValid)
            {
                // Tarayıcı güvenlik gereği seçilen dosyayı forma geri koyamaz
                ViewBag.ImageLost = HasFile(imageFile) && ModelState.IsValidField("imageFile");
                FillDepartments(0);
                return View(p);
            }

            // Dosya, kayıttaki her şey doğruysa kaydedilir
            p.PersonnelImage = HasFile(imageFile) ? SaveImage(imageFile) : null;

            c.Personnels.Add(p);
            c.SaveChanges();

            TempData["Success"] = $"“{p.PersonnelName} {p.PersonnelSurname}” eklendi.";
            return RedirectToAction("Index");
        }

        // Güncelleme formu
        public ActionResult PersonnelGet(int id)
        {
            var prs = c.Personnels.Find(id);
            if (prs == null)
            {
                return HttpNotFound();
            }

            FillDepartments(prs.DepartmentId);
            return View("PersonnelGet", prs);
        }

        // imageFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PersonnelUpdate(Personnel p, HttpPostedFileBase imageFile, bool removeImage = false)
        {
            var prsn = c.Personnels.Find(p.PersonnelId);
            if (prsn == null)
            {
                return HttpNotFound();
            }

            ValidatePersonnel(p);
            ValidateImage(imageFile);
            if (!ModelState.IsValid)
            {
                ViewBag.ImageLost = HasFile(imageFile) && ModelState.IsValidField("imageFile");
                p.PersonnelImage = prsn.PersonnelImage;   // formda şu anki fotoğraf görünsün
                FillDepartments(prsn.DepartmentId);
                return View("PersonnelGet", p);
            }

            prsn.PersonnelName = p.PersonnelName;
            prsn.PersonnelSurname = p.PersonnelSurname;
            prsn.DepartmentId = p.DepartmentId;

            // Fotoğraf
            if (HasFile(imageFile))
            {
                string oldImage = prsn.PersonnelImage;
                prsn.PersonnelImage = SaveImage(imageFile);
                DeleteImage(oldImage);
            }
            else if (removeImage)
            {
                DeleteImage(prsn.PersonnelImage);
                prsn.PersonnelImage = null;
            }

            c.SaveChanges();

            TempData["Success"] = $"“{prsn.PersonnelName} {prsn.PersonnelSurname}” güncellendi.";
            return RedirectToAction("Index");
        }

        // Formdaki departman açılır listesi
        private void FillDepartments(int currentDepartmentId)
        {
            ViewBag.Departments = c.Departments
                .Where(x => x.Status == true || x.DepartmentId == currentDepartmentId)
                .OrderBy(x => x.DepartmentName)
                .ToList()
                .Select(x => new SelectListItem
                {
                    Text = x.Status ? x.DepartmentName : x.DepartmentName + " (pasif)",
                    Value = x.DepartmentId.ToString()
                })
                .ToList();
        }

        // Ekleme ve güncellemede ortak kontroller
        private void ValidatePersonnel(Personnel p)
        {
            p.PersonnelName = Clean(p.PersonnelName);
            p.PersonnelSurname = Clean(p.PersonnelSurname);

            if (p.PersonnelName == null)
            {
                ModelState.AddModelError("PersonnelName", "Ad boş bırakılamaz.");
            }

            if (p.PersonnelSurname == null)
            {
                ModelState.AddModelError("PersonnelSurname", "Soyad boş bırakılamaz.");
            }

            // Departman seçilmediyse MVC kendi İngilizce mesajını ekler
            int departmentId = p.DepartmentId;
            if (!ModelState.IsValidField("DepartmentId"))
            {
                ModelState["DepartmentId"].Errors.Clear();
                ModelState.AddModelError("DepartmentId", "Bir departman seç.");
            }
            else if (!c.Departments.Any(x => x.DepartmentId == departmentId))
            {
                ModelState.AddModelError("DepartmentId", "Bir departman seç.");
            }
        }

        // Fotoğraf yükleme

        private static bool HasFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0;
        }

        // Seçilen dosyanın kontrolleri
        private void ValidateImage(HttpPostedFileBase file)
        {
            if (!HasFile(file))
            {
                return;   // fotoğraf isteğe bağlı
            }

            string extension = (Path.GetExtension(file.FileName) ?? "").ToLowerInvariant();
            if (!ImageExtensions.Contains(extension))
            {
                ModelState.AddModelError("imageFile", "Sadece JPG, PNG, WEBP ya da GIF yükleyebilirsin.");
            }
            else if (file.ContentLength > MaxImageBytes)
            {
                ModelState.AddModelError("imageFile", "Fotoğraf en fazla 2 MB olabilir.");
            }
            else if (!LooksLikeImage(file.InputStream))
            {
                // Uzantısı .jpg yapılmış başka bir dosya sunucuya kaydedilmesin
                ModelState.AddModelError("imageFile", "Bu dosya bir resim değil ya da bozuk.");
            }
        }

        // Dosyanın ilk baytlarına bakar
        private static bool LooksLikeImage(Stream stream)
        {
            var head = new byte[12];
            int read = stream.Read(head, 0, head.Length);
            stream.Position = 0;   // kaydederken dosya baştan okunsun

            if (read < 12)
            {
                return false;
            }

            bool jpg = head[0] == 0xFF && head[1] == 0xD8 && head[2] == 0xFF;
            bool png = head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
            bool gif = head[0] == 0x47 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x38;   // GIF8
            bool webp = head[0] == 0x52 && head[1] == 0x49 && head[2] == 0x46 && head[3] == 0x46   // RIFF
                     && head[8] == 0x57 && head[9] == 0x45 && head[10] == 0x42 && head[11] == 0x50;   // WEBP
            return jpg || png || gif || webp;
        }

        private string SaveImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath(ImageFolder);
            Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
            file.SaveAs(Path.Combine(folder, fileName));

            return Url.Content(ImageFolder + fileName);
        }

        // Eski fotoğrafı diskten siler
        private void DeleteImage(string imagePath)
        {
            string folderUrl = Url.Content(ImageFolder);
            if (string.IsNullOrEmpty(imagePath) || !imagePath.StartsWith(folderUrl, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fullPath = Path.Combine(Server.MapPath(ImageFolder), Path.GetFileName(imagePath));
            try
            {
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (IOException)
            {
                // Dosya o an kullanılıyorsa silinemeyebilir
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

using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using MVCOnlineTicariOtomasyon.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    // Hızlı bakış
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class QuickViewController : Controller
    {
        Context c = new Context();

        // Bir tabloda en fazla bu kadar satır görünür
        const int VisibleRows = 7;

        public ActionResult Index()
        {
            return View();
        }

        // [ChildActionOnly]

        [ChildActionOnly]
        public PartialViewResult CategoryTable()
        {
            var rows = c.Categories
                .Select(x => new CountRow { Name = x.CategoryName, Count = x.Products.Count(p => p.Status == true) })
                .ToList();

            return CountTable("Kategorilere göre ürün", "Kategori", "Ürün", rows, Url.Action("Index", "Category"), "Kategorilere git");
        }

        [ChildActionOnly]
        public PartialViewResult CityTable()
        {
            // Şehri boş olan cariler tek satırda toplanır
            var rows = c.Currents
                .Where(x => x.Status == true)
                .GroupBy(x => x.CurrentCity)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .ToList()
                .Select(x => new CountRow { Name = String.IsNullOrWhiteSpace(x.City) ? "Şehir girilmemiş" : x.City, Count = x.Count })
                .ToList();

            return CountTable("Şehirlere göre cari", "Şehir", "Cari", rows, Url.Action("Index", "Current"), "Carilere git");
        }

        [ChildActionOnly]
        public PartialViewResult DepartmentTable()
        {
            var rows = c.Departments
                .Where(x => x.Status == true)
                .Select(x => new CountRow { Name = x.DepartmentName, Count = x.Personnels.Count() })
                .ToList();

            return CountTable("Departmanlara göre personel", "Departman", "Personel", rows, Url.Action("Index", "Department"), "Departmanlara git");
        }

        [ChildActionOnly]
        public PartialViewResult BrandTable()
        {
            // Markası boş olan ürünler tek satırda toplanır
            var rows = c.Products
                .Where(x => x.Status == true)
                .GroupBy(x => x.Brand)
                .Select(g => new { Brand = g.Key, Count = g.Count() })
                .ToList()
                .Select(x => new CountRow { Name = String.IsNullOrWhiteSpace(x.Brand) ? "Marka girilmemiş" : x.Brand, Count = x.Count })
                .ToList();

            return CountTable("Markalara göre ürün", "Marka", "Ürün", rows, Url.Action("Index", "Product"), "Ürünlere git");
        }

        [ChildActionOnly]
        public PartialViewResult CurrentTable()
        {
            var currents = c.Currents
                .Where(x => x.Status == true)
                .OrderBy(x => x.CurrentName)
                .ThenBy(x => x.CurrentSurname)
                .ToList();

            return PartialView("_CurrentTable", currents);
        }

        // Dört sayı tablosunun ortak hazırlığı
        private PartialViewResult CountTable(string title, string nameHeader, string countHeader,
                                             List<CountRow> rows, string linkUrl, string linkText)
        {
            var sorted = rows
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Name)
                .ToList();

            // Tek bir satır için "Diğer" açmaya değmez
            if (sorted.Count > VisibleRows + 1)
            {
                var rest = sorted.Skip(VisibleRows).ToList();
                string noun = nameHeader.ToLower(new CultureInfo("tr-TR"));

                sorted = sorted.Take(VisibleRows).ToList();
                sorted.Add(new CountRow
                {
                    Name = "Diğer " + rest.Count + " " + noun,
                    Count = rest.Sum(x => x.Count),
                    IsOther = true
                });
            }

            var model = new CountTableViewModel
            {
                Title = title,
                NameHeader = nameHeader,
                CountHeader = countHeader,
                LinkUrl = linkUrl,
                LinkText = linkText,
                Total = rows.Sum(x => x.Count),
                Rows = sorted
            };

            return PartialView("_CountTable", model);
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

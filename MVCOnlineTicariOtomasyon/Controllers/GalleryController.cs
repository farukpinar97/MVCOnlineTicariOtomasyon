using MVCOnlineTicariOtomasyon.Filters;
using MVCOnlineTicariOtomasyon.Models.Context;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace MVCOnlineTicariOtomasyon.Controllers
{
    [AdminAuthorize]   // sadece yönetici girişi yapanlar
    public class GalleryController : Controller
    {
        Context c = new Context();

        // Görseli olan aktif ürünler, ada göre sıralı
        public ActionResult Index()
        {
            var values = c.Products
                .Include(x => x.Category)
                .Where(x => x.Status == true && x.ProductImage != null && x.ProductImage != "")
                .OrderBy(x => x.ProductName)
                .ToList();

            // Galeride yer almayan aktif ürünlerin sayısı
            ViewBag.WithoutImage = c.Products.Count(x => x.Status == true && (x.ProductImage == null || x.ProductImage == ""));

            return View(values);
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

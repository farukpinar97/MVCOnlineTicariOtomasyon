using MVCOnlineTicariOtomasyon.Models.Entities;
using System.Collections.Generic;

namespace MVCOnlineTicariOtomasyon.Models.ViewModels
{
    // Ürün detay sayfası birden fazla tablodan veri gösterir
    public class ProductDetailViewModel
    {
        public IEnumerable<Product> Products { get; set; }   // kurstaki Deger1
        public IEnumerable<Detail> Details { get; set; }   // kurstaki Deger2
        public IEnumerable<SalesTransaction> RecentSales { get; set; }   // ürünün son 10 satışı

        public int TotalSold { get; set; }   // bugüne kadar satılan toplam adet
        public decimal TotalRevenue { get; set; }   // bu üründen gelen toplam ciro
    }
}

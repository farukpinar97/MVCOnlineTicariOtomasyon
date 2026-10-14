using System;
using System.Collections.Generic;

namespace MVCOnlineTicariOtomasyon.Models.ViewModels
{
    // İstatistik sayfasının ihtiyaç duyduğu bütün rakamlar tek sınıfta
    public class StatisticViewModel
    {
        public DateTime Today { get; set; }

        // Satış
        public decimal TodayRevenue { get; set; }   // bugünkü kasa
        public int TodaySalesCount { get; set; }   // bugünkü satış sayısı
        public decimal TotalRevenue { get; set; }   // kasadaki toplam tutar
        public int TotalSalesCount { get; set; }
        public string BestSellerName { get; set; }   // en çok satan ürün
        public int BestSellerQuantity { get; set; }
        public List<DailySales> Last7Days { get; set; }   // grafik

        // Stok ve ürünler
        public int ProductCount { get; set; }
        public int TotalStock { get; set; }
        public int LowStockLimit { get; set; }   // "azalan" sayılma sınırı
        public int LowStockCount { get; set; }
        public int BrandCount { get; set; }
        public string TopBrandName { get; set; }   // en çok ürünü olan marka
        public int TopBrandProductCount { get; set; }
        public string MostExpensiveName { get; set; }
        public decimal MostExpensivePrice { get; set; }
        public string CheapestName { get; set; }
        public decimal CheapestPrice { get; set; }
        public List<CategoryProductCount> CategoryRows { get; set; }   // grafik

        // Genel
        public int CurrentCount { get; set; }   // aktif cari
        public int PersonnelCount { get; set; }
        public int CategoryCount { get; set; }
        public int DepartmentCount { get; set; }   // aktif departman
    }

    // Bir günün satışları
    public class DailySales
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Count { get; set; }
    }

    // Bir kategorideki aktif ürün sayısı
    public class CategoryProductCount
    {
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }
}

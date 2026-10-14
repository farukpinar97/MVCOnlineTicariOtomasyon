using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Display(Name = "Ürün adı")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Ürün adı en fazla 30 karakter olabilir.")]
        public string ProductName { get; set; }

        [Display(Name = "Marka")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Marka en fazla 30 karakter olabilir.")]
        public string Brand { get; set; }

        [Display(Name = "Stok")]
        public short Stock { get; set; }

        [Display(Name = "Alış fiyatı")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Satış fiyatı")]
        public decimal SalePrice { get; set; }

        [Display(Name = "Aktif")]
        public bool Status { get; set; }

        [Display(Name = "Görsel adresi")]
        [Column(TypeName = "varchar")]
        [StringLength(250, ErrorMessage = "Görsel adresi en fazla 250 karakter olabilir.")]
        public string ProductImage { get; set; }

        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; }

        public ICollection<SalesTransaction> SalesTransactions { get; set; }
    }
}

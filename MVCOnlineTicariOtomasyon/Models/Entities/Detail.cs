using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Ürünün uzun açıklaması
    public class Detail
    {
        [Key]
        public int DetailId { get; set; }

        [Display(Name = "Ürün adı")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Ürün adı en fazla 30 karakter olabilir.")]
        public string ProductName { get; set; }

        // 30 karakter bir açıklama için çok kısaydı
        [Display(Name = "Açıklama")]
        [Column(TypeName = "varchar")]
        [StringLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
        public string ProductDescription { get; set; }
    }
}

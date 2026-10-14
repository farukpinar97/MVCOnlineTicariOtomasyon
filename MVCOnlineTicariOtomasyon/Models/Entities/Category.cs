using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        // Display
        [Display(Name = "Kategori adı")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Kategori adı en fazla 30 karakter olabilir.")]
        public string CategoryName { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    // Display ve hata mesajları veritabanı şemasını değiştirmez
    public class Personnel
    {
        [Key]
        public int PersonnelId { get; set; }

        [Display(Name = "Ad")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Ad en fazla 30 karakter olabilir.")]
        public string PersonnelName { get; set; }

        [Display(Name = "Soyad")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Soyad en fazla 30 karakter olabilir.")]
        public string PersonnelSurname { get; set; }

        [Display(Name = "Fotoğraf adresi")]
        [Column(TypeName = "varchar")]
        [StringLength(250, ErrorMessage = "Fotoğraf adresi en fazla 250 karakter olabilir.")]
        public string PersonnelImage { get; set; }

        public ICollection<SalesTransaction> SalesTransactions { get; set; }

        [Display(Name = "Departman")]
        public int DepartmentId { get; set; }
        public virtual Department Department { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        // Display
        [Display(Name = "Departman adı")]
        [Column(TypeName = "varchar")]
        [StringLength(30, ErrorMessage = "Departman adı en fazla 30 karakter olabilir.")]
        public string DepartmentName { get; set; }

        public bool Status { get; set; }

        public ICollection<Personnel> Personnels { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    public class ToDoList
    {
        [Key]
        public int ToDoListId { get; set; }

        // Uzunluk sınırı yok
        [Display(Name = "Başlık")]
        public string ToDoListHeader { get; set; }

        [Display(Name = "Durum")]
        [Column(TypeName = "bit")]
        public bool Status { get; set; }
    }
}

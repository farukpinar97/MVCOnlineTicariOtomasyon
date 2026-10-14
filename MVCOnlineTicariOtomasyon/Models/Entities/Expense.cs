using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCOnlineTicariOtomasyon.Models.Entities
{
    public class Expense
    {
        [Key]
        public int ExpenseId { get; set; }
        [Column(TypeName = "varchar")]
        [StringLength(100)]
        public string ExpenseDescription { get; set; }
        public DateTime ExpenseDate { get; set; }
        public decimal ExpenseTotal { get; set; }
    }
}
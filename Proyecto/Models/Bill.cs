using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Bill
    {
        [Key]
        public int IdBill { get; set; }

        public int IdEnrollment { get; set; }

        //public Enrollment Enrollments { get; set; }

        [Display(Name = "Descuento")]
        public decimal Discount { get; set; }

        public decimal Total { get; set; }
    }

    public class BillViewModel
    {
        [Required]
        public string Student { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int IdBill { get; set; }

        [Required]
        public List<BillDetails> Details { get; set; }

        [Required]
        public decimal Discount { get; set; }

        [Required]
        public decimal Total { get; set; }
    }

    public class BillDetails
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}
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

        public double Total { get; set; }
    }
}
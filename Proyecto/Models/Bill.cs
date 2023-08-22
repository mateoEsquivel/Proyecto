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

        public string StudentId { get; set; }
        
        [Display(Name ="Fecha")]
        public DateTime Date { get; set; }

        public double Subtotal { get; set; }

        [Display(Name = "Descuento")]
        public double Discount { get; set; }

        public double Total { get; set; }
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
        public double SubTotal { get; set; }

        [Required]
        public double Discount { get; set; }

        [Required]
        public double Total { get; set; }
    }

    public class BillDetails
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Credits { get; set; }

        [Required]
        public double Price { get; set; }
    }
}
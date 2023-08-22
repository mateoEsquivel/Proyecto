using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class BillDetail
    {
        [Key]
        [Column(Order = 0)]
        public int IdBill { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdDetail { get; set; }

        public int IdSchedule { get; set; }

        [Display(Name = "Curso")]
        public string Name { get; set; }

        [Display(Name = "Créditos")]
        public int Credits { get; set; }

        [Display(Name = "Precio")]
        public double Price { get; set; }
    }
}
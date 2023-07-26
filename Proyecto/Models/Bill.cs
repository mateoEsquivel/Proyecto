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
        [Display(Name = "CédulaJurídica")]
        public int LegalID { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
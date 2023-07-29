using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Enrollment
    {
        [Key]
        public int IdEnrollment { get; set; }
        [Display(Name = "IdEstudiante")]
        public int StudentId { get; set; }
        public AspNetUser AspNetUsers { get; set; }
        [Display(Name = "Precio")]
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public ICollection<EnrollmentDetail> EnrollmentDetails { get; set; }
        public ICollection<Bill> Bills { get; set; }
    }
}
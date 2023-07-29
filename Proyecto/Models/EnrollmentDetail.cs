using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class EnrollmentDetail
    {
        [Key]
        [Column(Order = 0)]
        public int IdEnrollment { get; set; }
        public Enrollment Enrollments { get; set; }
        [Key]
        [Column(Order = 1)]
        public int IdDetail { get; set; }
        public int IdSchedule { get; set; }
        public Schedule Schedules { get; set; }
        [Display(Name = "Precio")]
        public decimal Price { get; set; }
    }
}
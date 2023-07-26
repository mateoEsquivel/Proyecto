using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
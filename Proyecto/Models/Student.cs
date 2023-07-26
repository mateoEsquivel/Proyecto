using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }
        [Display(Name = "Usuario")]
        public string Username { get; set; }
        public User Users { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        [Display(Name = "Apellido")]
        public string LastName { get; set; }
        [Display(Name = "Teléfono")]
        public int PhoneNumber { get; set; }
        [Display(Name = "Correo")]
        public string Email { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
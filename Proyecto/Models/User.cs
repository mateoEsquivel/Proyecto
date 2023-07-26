using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; }
        [Display(Name ="Contraseña")]
        public string Password { get; set; }
        [Display(Name = "Estado")]
        public byte[] Status { get; set; }
        public ICollection<Professor> Professors { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
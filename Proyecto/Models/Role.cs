using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Proyecto.Models;

namespace Proyecto.Models
{
    public class Role
    {
        [Key]
        public int RolId { get; set; }
        [Display(Name = "Usuario")]
        public string Username { get; set; }
        //public User Users { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
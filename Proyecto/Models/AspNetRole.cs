using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Proyecto.Models
{
    public class AspNetRole
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Rol")]
        public string Name { get; set; }
        //public ICollection<AspNetUserRole> AspNetUserRoles { get; set; }
    }
}
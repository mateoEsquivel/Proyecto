using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Proyecto.Models
{
    public class UserData
    {
        [Key]
        public int idData { get; set; }
        public string idUser { get; set; }
        //public AspNetUser AspNetUsers { get; set; }
        [Display(Name = "Nombre")]
        public string Name { get; set; }
        [Display(Name = "Apellidos")]
        public string LastName { get; set; }
    }
}
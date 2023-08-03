using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class AspNetUserRole
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; }

       // public AspNetUser AspNetUsers { get; set; }

        [Key]
        [Column(Order = 1)]
        public string RoleId { get; set; }

        //public AspNetRole AspNetRoles { get; set; }
    }
}
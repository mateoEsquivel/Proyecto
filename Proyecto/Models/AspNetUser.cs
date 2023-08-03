using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class AspNetUser
    {
        [Key]
        public string Id { get; set; }

        [Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string PhoneNumber{ get; set; }

        public bool PhoneNumberConfirmed { get; set;}

        public bool TwoFactorEnabled { get; set; }

        public DateTime LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        [Display(Name = "Correo electrónico")]
        public string UserName { get; set; }

        //public ICollection<AspNetUserRole> AspNetUserRoles { get; set; }
        //public ICollection<Schedule> Schedules { get; set; }
        //public ICollection<Score> Scores { get; set; }
        //public ICollection<Enrollment> Enrollments { get; set; }
    }
}
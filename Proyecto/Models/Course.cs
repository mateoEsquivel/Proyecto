using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.EnterpriseServices.Internal;
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

        [Display(Name = "Créditos")]
        public int Credits { get; set; }

        [Display(Name = "Precio")]
        public double Price { get; set; }

        //public ICollection<Schedule> Schedules { get; set; }
    }

    public class CourseRegisterViewModel
    {
        [Required]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Créditos")]
        public int Credits { get; set;}

        [Required]
        [Display(Name = "Precio")]
        public double Price { get; set; }
    }

    public class CourseListViewModel
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required]
        [Display(Name="Créditos")]
        public int Credits { get; set;}

        [Required]
        [Display(Name = "Precio")]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Horarios")]
        public int Schedules { get; set;}
    }

    public class CourseDetailsViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public List<CourseDetailsListViewModel> List { get; set; }
    }

    public class CourseDetailsListViewModel
    {
        [Required]
        public int ScheduleId { get; set; }

        [Required]
        public string Day { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Students { get; set; }
    }
}
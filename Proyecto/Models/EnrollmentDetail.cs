﻿using System;
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

        //public Enrollment Enrollments { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdDetail { get; set; }

        public int IdSchedule { get; set; }

        //public Schedule Schedules { get; set; }

    }

    public class DetailViewModel
    {
        [Required]
        public int IdSchedule { get; set; }

        [Required]
        [Display(Name = "Curso")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Día")]
        public string Day { get; set; }

        [Required]
        [Display(Name = "Hora De Inicio")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Display(Name = "Hora De Finalización")]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Display(Name = "Créditos")]
        public int Credits { get; set; }

        [Required]
        [Display(Name = "Precio")]
        public decimal Price { get; set; }
    }
}
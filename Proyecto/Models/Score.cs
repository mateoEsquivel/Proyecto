using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Score
    {
        [Key]
        public int IdScore { get; set; }

        public string StudentId { get; set; }

        public int IdSchedule { get; set; }

        //public Schedule Schedules { get; set; }

        //public AspNetUser AspNetUsers { get; set; }

        [Display(Name = "Nota")]
        public double StudentScore { get; set; }

    }

    public class StudentViewModel
    {
        [Required]
        public string StudentId { get; set; }

        [Required]
        public int ScheduleId { get; set; }

        [Required]
        [Display(Name= "Nombre")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Correo")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Teléfono")]
        public string Phone { get; set; }

        [Required]
        public int ScoreId { get; set; }
    }

    public class ScoreViewModel
    {
        [Required]
        public int ScoreId { get; set; }

        [Required]
        [Display(Name = "Curso")]
        public string Course { get; set; }

        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [Required]
        public List<ScoreDetails> Details { get; set; }

        [Required]
        public double Score { get; set; }
    }

    public class ScoreDetails
    {
        [Required]
        public int IdDetail { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Percentaje { get; set; }

        [Required]
        public double Score { get; set; }
    }

    public class ScoreEditModel
    {
        [Required]
        public int ScoreId { get; set; }

        [Required]
        public int IdDetail { get; set; }

        [Required]
        public double Score { get; set; }
    }
}
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

        public int IdSchedule { get; set; }

        //public Schedule Schedules { get; set; }

        public string StudentId { get; set; }

        //public AspNetUser AspNetUsers { get; set; }

        [Display(Name = "Nota")]
        public decimal StudentScore { get; set; }

    }
}
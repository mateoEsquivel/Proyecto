using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Enrollment
    {
        [Key]
        public int IdBill { get; set; }
        [Display(Name = "IdHorario")]
        public int IdSchedule { get; set; }
        public Schedule Schedules { get; set; }
        [Display(Name = "IdEstudiante")]
        public int StudentId { get; set; }
        public Student Students { get; set; }
        [Display(Name = "Créditos")]
        public int Credits { get; set; }
        [Display(Name = "Precio")]
        public double Amount { get; set; }
        [Display(Name = "Fecha/Hora")]
        public DateTime Date { get; set; }
        [Display(Name = "Nota")]
        public int Score { get; set; }
        
    }
}
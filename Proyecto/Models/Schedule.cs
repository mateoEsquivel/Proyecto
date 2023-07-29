using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Schedule
    {
        [Key]
        public int IdSchedule { get; set; }
        [Display(Name = "IdCurso")]
        public int CourseId { get; set; }
        public Course Courses { get; set; }
        [Display(Name = "IdProfesor")]
        public int ProfessorId { get; set; }
        public AspNetUser AspNetUsers { get; set; }
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; }
        [Display(Name = "Hora De Inicio")]
        public TimeSpan StartTime { get; set; }
        [Display(Name = "Hora De Finalización")]
        public TimeSpan EndTime { get; set; }
        [Display(Name = "Cupo")]
        public int Students { get; set; }
        public ICollection<Score> Scores { get; set; }
        public ICollection<EnrollmentDetail> EnrollmentDetails { get; set; }
    }
}
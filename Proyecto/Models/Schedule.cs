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
        public Professor Professors { get; set; }
        [Display(Name = "Fecha")]
        public DateTime Date { get; set; }
        [Display(Name = "HoraInicio")]
        public TimeSpan StartTime { get; set; }
        [Display(Name = "HoraFinalización")]
        public TimeSpan EndTime { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
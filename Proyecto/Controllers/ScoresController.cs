using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Proyecto.CustomFilters;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Proyecto.Controllers
{
    [Authorize]
    public class ScoresController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

        public ScoresController()
        {

        }

        public ScoresController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Scores
        [Authorize(Roles = "PROFESSOR")]
        public ActionResult Index()
        {
            string query = "Select * from Schedules where ProfessorId='" + User.Identity.GetUserId().ToString() + "'";
            var schedules=db.Database.SqlQuery<Schedule>(query);
            var list = new List<ScheduleViewModel>();
            foreach(var schedule in schedules.ToList())
            {
                var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                var custom = new ScheduleViewModel
                {
                    IdSchedule=schedule.IdSchedule,
                    Name=course.Name,
                    Day=schedule.Day,
                    StartTime=schedule.StartTime,
                    EndTime=schedule.EndTime,
                    Students=schedule.Students
                };
                list.Add(custom);
            }
            return View(list);
        }

        [Authorize(Roles = "PROFESSOR, ADMIN")]
        public ActionResult Students(int? id)
        {
            string message;
            var enrollments = db.Enrrollment.ToList();
            var list = new List<StudentViewModel>();
            if (id==null)
            {
                message = DateTime.Now + " El id del horario estaba vació, Acción: Scores/Students";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var exist = db.Schedule.SingleOrDefault(e => e.IdSchedule == id);
            if(exist==null)
            {
                message = DateTime.Now + " El id del horario no se encontró, Acción: Scores/Students";
                log.LogError(message);
                return HttpNotFound();
            }
            foreach (var enrollment in enrollments) 
            {
                string query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                foreach(var detail in details.ToList())
                {
                    if(detail.IdSchedule==id)
                    {
                        var user = db.AspUser.SingleOrDefault(e => e.Id == enrollment.StudentId);
                        var userData = db.UserData.SingleOrDefault(e => e.idUser == enrollment.StudentId);
                        var score = db.Score.SingleOrDefault(e => e.StudentId == enrollment.StudentId && e.IdSchedule == id);
                        var custom = new StudentViewModel
                        {
                            StudentId=user.Id,
                            ScheduleId=detail.IdSchedule,
                            Name=userData.Name+" "+userData.LastName,
                            Email=user.Email,
                            Phone=user.PhoneNumber,
                            ScoreId=score.IdScore
                        };
                        list.Add(custom);
                    }
                }
            }
            return View(list);
        }

        public ActionResult Details(int? id) 
        {
            string message;
            if(id==null)
            {
                message = DateTime.Now + " El id de la nota estaba vació, Acción: Scores/Details";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var score = db.Score.SingleOrDefault(e => e.IdScore == id);
            if(score==null)
            {
                message = DateTime.Now + " El id de la nota no se encontró, Acción: Scores/Details";
                log.LogError(message);
                return HttpNotFound();
            }
            var user = db.UserData.SingleOrDefault(e => e.idUser == score.StudentId);
            var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == score.IdSchedule);
            var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.IdSchedule);
            var custom = new ScoreViewModel
            {
                ScoreId = score.IdScore,
                Course = course.Name + "(" + schedule.Day + " " + schedule.StartTime + "-" + schedule.EndTime + ")",
                Name = user.Name + " " + user.LastName,
                Details = new List<ScoreDetails>(),
                Score = score.StudentScore
            };
            var query = "Select * from ScoreDetails where IdScore=" + score.IdScore;
            var details=db.Database.SqlQuery<ScoreDetail>(query);
            foreach(var detail in details.ToList())
            {
                var d = new ScoreDetails
                {
                    IdDetail = detail.IdDetail,
                    Name = detail.Name,
                    Score = detail.Score,
                    Percentaje = detail.Percentage
                };
                custom.Details.Add(d);
            }
            return View(custom);
        }

        [Authorize(Roles = "PROFESSOR")]
        public ActionResult Edit(int? idScore, int? idDetail)
        {
            var score = db.Score.SingleOrDefault(e => e.IdScore == idScore);
            var detail = db.ScoreDetail.SingleOrDefault(e => e.IdScore == idScore && e.IdDetail == idDetail);
            var model = new ScoreEditModel
            {
                ScoreId = score.IdScore,
                IdDetail = detail.IdDetail,
                Score=detail.Score
            };
            return View(model);
        }

        [Authorize(Roles = "PROFESSOR")]
        [HttpPost]
        public ActionResult Edit(ScoreEditModel model)
        {
            string message;
            if (!ModelState.IsValid)
            {
                message = DateTime.Now + " Error en los parametros, Acción: Scores/Edit";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var detail = db.ScoreDetail.SingleOrDefault(e => e.IdScore == model.ScoreId && e.IdDetail == model.IdDetail);
            detail.Score = model.Score;
            var s = db.Score.SingleOrDefault(e => e.IdScore == model.ScoreId);
            var user = db.AspUser.SingleOrDefault(e => e.Id == s.StudentId);
            try
            {
                db.Entry(detail).State = EntityState.Modified;
                db.SaveChanges();
                message = DateTime.Now + " La nota del usuario: " + user.Email + " fue modificada";
                log.LogAlert(message);
            }
            catch (Exception ex) 
            {
                message = DateTime.Now + " Error al editar nota: " + ex;
                log.LogError(message);
                throw ex;
            }
            string query = "Select * from ScoreDetails where IdScore=" + model.ScoreId;
            var details=db.Database.SqlQuery<ScoreDetail>(query);
            double suma = 0;
            foreach(var d in details.ToList())
            {
                suma += d.Score * d.Percentage;
            }
            if((suma/100)>100)
            {
                suma = 100;
                s.StudentScore = suma;
            }
            else
            {
                s.StudentScore = suma / 100;
            }
            db.Entry(s).State = EntityState.Modified;
            db.SaveChanges();
            return Redirect("https://localhost:44302/Scores/Details/" + model.ScoreId);
        }

        [Authorize(Roles = "ADMIN")]
        public ActionResult Delete(int id)
        {
            var score = db.Score.SingleOrDefault(e => e.IdScore == id);
            var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == score.IdSchedule);
            schedule.Students--;
            db.Entry(schedule).State = EntityState.Modified;
            db.SaveChanges();
            string query = "Select * from Enrollments where StudentId='" + score.StudentId + "'";
            var enrollments = db.Database.SqlQuery<Enrollment>(query);
            foreach (var enrollment in enrollments.ToList())
            {
                query = "Delete from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment + " and IdSchedule=" + score.IdSchedule;
                db.Database.ExecuteSqlCommand(query);
                db.SaveChanges();
                query= "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                if(details.ToList().Count()==0)
                {
                    query = "Delete from Enrollments where IdEnrollment=" + enrollment.IdEnrollment;
                    db.Database.ExecuteSqlCommand(query);
                    db.SaveChanges();
                }
            }
            query = "Delete from ScoreDetails where IdScore=" + id;
            db.Database.ExecuteSqlCommand(query);
            db.SaveChanges();
            query = "Delete from Scores where IdScore=" + id;
            db.Database.ExecuteSqlCommand(query);
            db.SaveChanges();
            return RedirectToAction("Index","Home");
        }
    }
}
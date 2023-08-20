using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto.Controllers
{
    [Authorize]
    public class EnrollmentDetailsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();

        public EnrollmentDetailsController()
        {

        }

        public EnrollmentDetailsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        [Authorize(Roles = "STUDENT")]
        // GET: EnrollmentDetails
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId().ToString();
            var aux = 0;
            var query = "Select * from Enrollments where StudentId='" + id + "'";
            var enrollments = db.Database.SqlQuery<Enrollment>(query);
            var view = new List<DetailView>();
            foreach (var enrollment in enrollments.ToList())
            {
                query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                var details=db.Database.SqlQuery<EnrollmentDetail>(query);
                foreach (var detail in details.ToList())
                {
                    var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                    var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                    var teacher = db.UserData.SingleOrDefault(e => e.idUser == schedule.ProfessorId);
                    var score = db.Score.SingleOrDefault(e => e.IdSchedule == schedule.IdSchedule && e.StudentId == id);
                    if(score!=null)
                    {
                        aux = score.IdScore;
                    }
                    var custom = new DetailView
                    {
                        IdScore = aux,
                        IdEnrollment=enrollment.IdEnrollment,
                        Course=course.Name,
                        Credits=course.Credits,
                        Teacher=teacher.Name+" "+teacher.LastName,
                        Schedule=schedule.Day+"("+schedule.StartTime+"-"+schedule.EndTime+")",
                        Date=enrollment.Date
                    };
                    view.Add(custom);
                }
            }
            return View(view);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public ActionResult Details(string email)
        {
            var user=db.AspUser.SingleOrDefault(e=>e.Email==email);
            if(user!=null)
            {
                if(!user.LockoutEnabled)
                {
                    string query = "Select * from Enrollments where StudentId='" + user.Id + "'";
                    var enrollments=db.Database.SqlQuery<Enrollment>(query);
                    if(enrollments.ToList().Count!=0)
                    {
                        var view = new List<DetailView>();
                        foreach(var enrollment in enrollments.ToList())
                        {
                            query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                            var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                            foreach(var detail in details.ToList())
                            {
                                var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                                var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                                var teacher = db.UserData.SingleOrDefault(e => e.idUser == schedule.ProfessorId);
                                var custom = new DetailView
                                {
                                    IdEnrollment = enrollment.IdEnrollment,
                                    Course = course.Name,
                                    Credits = course.Credits,
                                    Teacher = teacher.Name + " " + teacher.LastName,
                                    Schedule = schedule.Day + "(" + schedule.StartTime + "-" + schedule.EndTime + ")",
                                    Date = enrollment.Date
                                };
                                view.Add(custom);
                            }
                        }
                        Session["detail"] = true;
                        return View(view);
                    }
                }
            }
            return HttpNotFound();
        }
    }
}
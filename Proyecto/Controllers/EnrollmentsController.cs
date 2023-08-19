using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Runtime.InteropServices;
using System.Data.Entity;

namespace Proyecto.Controllers
{
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();

        public static List<Schedule> enrollments = new List<Schedule>();

        public EnrollmentsController()
        {

        }

        public EnrollmentsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        //
        // GET: /Enrollments/Index
        [Authorize(Roles = "STUDENT")]
        public ActionResult Index()
        {
            var id=User.Identity.GetUserId().ToString();
            var c = db.Course.ToList();
            var aux = false;
            var courses = new List<CourseListViewModel>();
            foreach (var course in c)
            {
                string query = "Select * from Schedules where CourseId = " + course.CourseId;
                var schedule = db.Database.SqlQuery<Schedule>(query);
                if (schedule.ToList().Count() != 0)
                {
                    query = "Select * from Enrollments where StudentId='" + id + "'";
                    var enrollments = db.Database.SqlQuery<Enrollment>(query);
                    foreach (var e in enrollments.ToList())
                    {
                        query = "Select * from EnrollmentDetails where IdEnrollment=" + e.IdEnrollment;
                        var details= db.Database.SqlQuery<EnrollmentDetail>(query);
                        foreach (var d in details.ToList())
                        {
                            foreach(var s in schedule.ToList())
                            {
                                if(s.IdSchedule==d.IdSchedule)
                                {
                                    aux=true;
                                }    
                            }
                        }
                    }
                    if(!aux)
                    {
                        var enroll = new CourseListViewModel
                        {
                            CourseId = course.CourseId,
                            Name = course.Name,
                            Credits = course.Credits,
                            Price = course.Price,
                            Schedules = schedule.ToList().Count()
                        };
                        courses.Add(enroll);
                    }
                }
            }

            return View(courses);
        }

        //
        // GET: /Enrollments/ScheduleList
        [Authorize(Roles = "STUDENT")]
        public ActionResult ScheduleList(int? id) 
        {
            if(id==null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "select * from Schedules where CourseId = " + id;
            var schedules = db.Database.SqlQuery<Schedule>(query);

            if(schedules.ToList().Count()==0)
            {
                return HttpNotFound();
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId == id);

            Session["Course"] = course.Name;

            return View(schedules.ToList());
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult Enroll(int id)
        {
            var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == id);
            foreach (var s in enrollments)
            {
                if(schedule.CourseId==s.CourseId)
                {
                    return HttpNotFound();
                }
            }
            enrollments.Add(schedule);
            Session["List"] = enrollments;
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult List()
        {
            var details = new List<DetailViewModel>();

            foreach (var schedule in enrollments)
            {
                var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                var detail = new DetailViewModel
                {
                    IdSchedule = schedule.IdSchedule,
                    Name = course.Name,
                    Day = schedule.Day,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Credits = course.Credits,
                    Price = course.Price
                };
                details.Add(detail);
            }
            return View(details);
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult Cancel() 
        {
            enrollments= new List<Schedule>();
            Session["List"] = null;
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult Remove(int id)
        {
            var schedule = new Schedule();
            foreach(var s in enrollments)
            {
                if(s.IdSchedule==id)
                {
                    schedule = s;
                }
            }
            if(schedule!=null)
            {
                enrollments.Remove(schedule);
            }
            else
            {
                return HttpNotFound();
            }
            if(enrollments.Count()==0)
            {
                enrollments = new List<Schedule>();
                Session["List"] = null;
                return RedirectToAction("Index");
            }
            Session["List"] = enrollments;
            return RedirectToAction("List");
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult Create()
        {
            decimal total = 0;
            string id = User.Identity.GetUserId().ToString();
            int i = 1;
            if (enrollments.Count!=0)
            {
                foreach(var s in enrollments)
                {
                    var course = db.Course.SingleOrDefault(e => e.CourseId == s.CourseId);
                    total = total + course.Price;
                }
                var enrollment = new Enrollment
                {
                    StudentId=id,
                    Date= DateTime.Now,
                    Total=total
                };
                try
                {
                    db.Enrrollment.Add(enrollment);
                    db.SaveChanges();
                }
                catch
                {
                    throw;
                }
                enrollment = db.Enrrollment.SingleOrDefault(e => e.StudentId == id);
                foreach(var s2 in enrollments)
                {
                    var detail = new EnrollmentDetail
                    {
                        IdEnrollment = enrollment.IdEnrollment,
                        IdDetail = i,
                        IdSchedule = s2.IdSchedule
                    };
                    db.EnrrollmentDetail.Add(detail);
                    db.SaveChanges();
                    i++;
                }
                decimal discount = 0;
                if(enrollments.Count() == 2)
                {
                    discount = total * (20 / 100);
                }
                else if(enrollments.Count() == 3 || enrollments.Count() == 4)
                {
                    discount = total * (25 / 100);
                }
                else if(enrollments.Count() >= 5)
                {
                    discount = total * (30 / 100);
                }

                var bill = new Bill
                {
                    IdEnrollment = enrollment.IdEnrollment,
                    Discount = discount,
                    Total = total-discount
                };
                db.Bill.Add(bill);
                db.SaveChanges();
                enrollments = new List<Schedule>();
                Session["List"] = null;
                return RedirectToAction("Index", "Home");
            }
            return HttpNotFound();
        }
    }
}
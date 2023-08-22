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
using Proyecto.CustomFilters;
using System.Web.Services.Description;
using System.Web.UI.WebControls;

namespace Proyecto.Controllers
{
    [Authorize]
    public class EnrollmentsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

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
            var courses = db.Course.ToList();
            string query = "Select * from Enrollments where StudentId='" + id + "'";
            var enrollments = db.Database.SqlQuery<Enrollment>(query);
            var coursesView = new List<CourseListViewModel>();
            bool aux ;
            foreach (var course in courses)
            {
                aux = false;
                query = "Select * from Schedules where CourseId = " + course.CourseId;
                var schedules = db.Database.SqlQuery<Schedule>(query);
                if (schedules.ToList().Count() != 0)
                {
                    foreach (var enrollment in enrollments.ToList())
                    {
                        query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                        var details= db.Database.SqlQuery<EnrollmentDetail>(query);
                        foreach (var detail in details.ToList())
                        {
                            var schedule = db.Schedule.SingleOrDefault( e=> e.IdSchedule == detail.IdSchedule);
                            if(course.CourseId==schedule.CourseId)
                            {
                                aux = true;
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
                            Schedules = schedules.ToList().Count()
                        };
                        coursesView.Add(enroll);
                    }
                }
            }

            return View(coursesView);
        }

        //
        // GET: /Enrollments/ScheduleList
        [Authorize(Roles = "STUDENT")]
        public ActionResult ScheduleList(int? id) 
        {
            string message;
            if(id==null)
            {
                message = DateTime.Now + " El id del curso estaba vació, Accion: Enrollments/ScheduleList";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string query = "select * from Schedules where CourseId = " + id;
            var schedules = db.Database.SqlQuery<Schedule>(query);

            if(schedules.ToList().Count()==0)
            {
                message = DateTime.Now + " El curso no existe o no tiene horarios, Accion: Enrollments/ScheduleList";
                log.LogError(message);
                return HttpNotFound();
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId == id);

            ViewData["Course"] = course.Name;

            return View(schedules.ToList());
        }

        [Authorize(Roles = "STUDENT")]
        public ActionResult Enroll(int? id)
        {
            string message;
            Session["Time"] = null;
            if (id==null)
            {
                message = DateTime.Now + " El id del horario estaba vació, Accion: Enrollments/Enroll";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == id);
            if(schedule==null)
            {
                message = DateTime.Now + " No se encontro el horario, Accion: Enrollments/Enroll";
                log.LogError(message);
                return HttpNotFound();
            }
            foreach (var s in enrollments)
            {
                if(schedule.CourseId==s.CourseId)
                {
                    message = DateTime.Now + " El curso ya estaba en la lista de Matricula";
                    log.LogError(message);
                    return HttpNotFound();
                }
                if(schedule.Day==s.Day)
                {
                    int start = s.StartTime.Hours;
                    int end=s.EndTime.Hours;
                    if(schedule.StartTime.Hours>=start && schedule.StartTime.Hours<end)
                    {
                        Session["Time"] = "El horario choca con el de otro curso en la lista de matrícula";
                        return Redirect("ScheduleList/" + schedule.CourseId);
                    }
                    else if(schedule.EndTime.Hours>start && schedule.EndTime.Hours<=end)
                    {
                        Session["Time"] = "El horario choca con el de otro curso en la lista de matrícula";
                        return Redirect("https://localhost:44302/Enrollments/ScheduleList/"+schedule.CourseId);
                    }
                }
            }
            string query = "Select * from Enrollments where StudentId='" + User.Identity.GetUserId().ToString() + "'";
            var list=db.Database.SqlQuery<Enrollment>(query);
            if(list.ToList().Count()!=0)
            {
                foreach(var en in list.ToList())
                {
                    query = "Select * from EnrollmentDetails where IdEnrollment=" + en.IdEnrollment;
                    var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                    foreach(var detail in details.ToList())
                    {
                        var s = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                        if (schedule.Day == s.Day)
                        {
                            int start = s.StartTime.Hours;
                            int end = s.EndTime.Hours;
                            if (schedule.StartTime.Hours >= start && schedule.StartTime.Hours < end)
                            {
                                Session["Time"] = "El horario choca con el de otro curso matrículado";
                                return Redirect("ScheduleList/" + schedule.CourseId);
                            }
                            else if (schedule.EndTime.Hours > start && schedule.EndTime.Hours <= end)
                            {
                                Session["Time"] = "El horario choca con el de otro curso matrículado";
                                return Redirect("https://localhost:44302/Enrollments/ScheduleList/" + schedule.CourseId);
                            }
                        }

                    }
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
                    Price = course.Price,
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
        public ActionResult Remove(int? id)
        {
            string message;
            if(id==null)
            {
                message = DateTime.Now + " El id del horario estaba vació, Accion: Enrollments/Remove";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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
                message = DateTime.Now + " No se encontró el horario, Accion: Enrollments/Remove";
                log.LogError(message);
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
            string message;
            Session["Time"] = null;
            string id = User.Identity.GetUserId().ToString();
            int i = 1;
            if (enrollments.Count() == 0)
            {
                message = DateTime.Now + " La lista de matrícula esta vacía";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var enrollment = new Enrollment
            {
                StudentId = id,
                Date = DateTime.Now
            };
            string query = "Select * from Enrollments where StudentId='" + id + "'";
            var enrollmentList = db.Database.SqlQuery<Enrollment>(query);
            if(enrollmentList.ToList().Count()!=0)
            {
                foreach(var s in enrollments)
                {
                    foreach(var en in enrollmentList.ToList())
                    {
                        query = "Select * from EnrollmentDetails where IdEnrollment=" + en.IdEnrollment;
                        var detailsList = db.Database.SqlQuery<EnrollmentDetail>(query);
                        foreach(var d in detailsList.ToList())
                        {
                            var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == d.IdSchedule);
                            if(schedule.CourseId==s.CourseId)
                            {
                                Session["Time"] = "El curso ya se encuentra matrículado";
                                return Redirect("https://localhost:44302/Enrollments/ScheduleList/" + s.CourseId);
                            }
                        }
                    }
                }
            }
            try
            {
                db.Enrrollment.Add(enrollment);
                db.SaveChanges();
                message = DateTime.Now + " El estudiante: " + User.Identity.GetUserName() + " registró una nueva matrícula";
                log.LogAlert(message);
            }
            catch (Exception ex)
            {
                message = DateTime.Now + " Error al ingresar matrícula, usuario: " + User.Identity.GetUserName();
                log.LogError(message);
                throw ex;
            }
            query = "Select * from Enrollments where StudentId='" + id + "' and Date='" + enrollment.Date + "'";
            enrollment= db.Database.SqlQuery<Enrollment>(query).Last();
            foreach(var s in enrollments)
            {
                var detail = new EnrollmentDetail
                {
                    IdEnrollment = enrollment.IdEnrollment,
                    IdDetail = i,
                    IdSchedule= s.IdSchedule
                };
                db.EnrrollmentDetail.Add(detail);
                db.SaveChanges();
                ScoreCreate(s.IdSchedule);
                i++;
            }
            var idBill = BillCreate();
            enrollments = new List<Schedule>();
            Session["List"] = null;
            return Redirect("https://localhost:44302/Bills/Details/"+idBill);
        }

        [Authorize(Roles = "STUDENT")]
        // GET: EnrollmentDetails
        public ActionResult Details()
        {
            var id = User.Identity.GetUserId().ToString();
            var query = "Select * from Enrollments where StudentId='" + id + "'";
            var enrollments = db.Database.SqlQuery<Enrollment>(query);
            var view = new List<DetailView>();
            foreach (var enrollment in enrollments.ToList())
            {
                query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                foreach (var detail in details.ToList())
                {
                    var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                    var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                    var teacher = db.UserData.SingleOrDefault(e => e.idUser == schedule.ProfessorId);
                    var score = db.Score.SingleOrDefault(e => e.IdSchedule == schedule.IdSchedule && e.StudentId == id);
                    var custom = new DetailView
                    {
                        IdScore = score.IdScore,
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
            return View(view);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public ActionResult DetailsAdmin(string email)
        {
            var user = db.AspUser.SingleOrDefault(e => e.Email == email);
            if (user != null)
            {
                if (!user.LockoutEnabled)
                {
                    string query = "Select * from Enrollments where StudentId='" + user.Id + "'";
                    var enrollments = db.Database.SqlQuery<Enrollment>(query);
                    if (enrollments.ToList().Count != 0)
                    {
                        var view = new List<DetailView>();
                        foreach (var enrollment in enrollments.ToList())
                        {
                            query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                            var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                            foreach (var detail in details.ToList())
                            {
                                var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                                var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                                var teacher = db.UserData.SingleOrDefault(e => e.idUser == schedule.ProfessorId);
                                var score = db.Score.SingleOrDefault(e => e.IdSchedule == schedule.IdSchedule && e.StudentId == user.Id);
                                var custom = new DetailView
                                {
                                    IdScore=score.IdScore,
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

        private int BillCreate()
        {
            double subtotal = 0;
            double discount = 0;
            string message;
            int i = 1;
            var id = User.Identity.GetUserId().ToString();
            var list = enrollments;
            foreach (var s in list.ToList())
            {
                var course = db.Course.SingleOrDefault(e => e.CourseId == s.CourseId);
                subtotal += course.Price;
            }
            if (list.Count() == 2)
            {
                discount = subtotal * 10;
                discount /= 100;
            }
            else if (list.Count() == 3 || list.Count() == 4)
            {
                discount = subtotal * 20;
                discount /= 100;
            }
            else if (list.Count() >= 5)
            {
                discount = subtotal * 30;
                discount /= 100;
            }
            var bill = new Bill
            {
                StudentId = id,
                Date = DateTime.Now,
                Subtotal = subtotal,
                Discount = discount,
                Total = subtotal-discount
            };
            try
            {
                db.Bill.Add(bill);
                db.SaveChanges();
                message = DateTime.Now + " El usuario: " + User.Identity.GetUserName().ToString() + " genero una factura";
                log.LogAlert(message);
            }
            catch (Exception ex)
            {
                message = DateTime.Now + " Error al generar factura: " + ex;
                log.LogError(message);
                throw ex;
            }
            string query = "Select * from Bills where StudentId='" + id + "' and Date='" + bill.Date + "'";
            bill = db.Database.SqlQuery<Bill>(query).Last();
            foreach (var schedule in list.ToList())
            {
                var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                var detail = new BillDetail
                {
                    IdBill = bill.IdBill,
                    IdDetail = i,
                    IdSchedule = schedule.IdSchedule,
                    Name = course.Name + "(" + schedule.Day + " " + schedule.StartTime + "-" + schedule.EndTime + ")",
                    Credits = course.Credits,
                    Price = course.Price
                };
                db.BillDetail.Add(detail);
                db.SaveChanges();
                i++;
            }

            return bill.IdBill;
        }

        private void ScoreCreate(int id)
        {
            string message;
            string useriD = User.Identity.GetUserId().ToString();
            var score = new Score
            {
                StudentId = useriD,
                IdSchedule = id,
                StudentScore = 0
            };
            try
            {
                db.Score.Add(score);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                message = DateTime.Now + " Error al crear Nota: " + ex;
                throw ex;
            }
            score = db.Score.SingleOrDefault(e => e.StudentId == useriD && e.IdSchedule == id);
            string rubro="";
            int percentage=0;
            for (int i=1;i<=4;i++)
            {
                switch(i)
                {
                    case 1:
                        rubro = "Prácticas programadas";
                        percentage = 16;
                        break;
                    case 2:
                        rubro = "Foros";
                        percentage = 4;
                        break;
                    case 3:
                        rubro = "Casos prácticos";
                        percentage=30;
                        break;
                    case 4:
                        rubro = "Proyecto";
                        percentage=60;
                        break;
                }

                var detail = new ScoreDetail
                {
                    IdScore = score.IdScore,
                    IdDetail = i,
                    Name = rubro,
                    Score = 0,
                    Percentage = percentage,
                };
                db.ScoreDetail.Add(detail);
                db.SaveChanges();
            }
        }
    }
}
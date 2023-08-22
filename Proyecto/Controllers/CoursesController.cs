using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using Microsoft.SqlServer.Server;
using System.Web.Configuration;
using Proyecto.CustomFilters;
using System.Data;
using System.Web.Services.Description;

namespace Proyecto.Controllers
{
    public class CoursesController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

        public CoursesController()
        {

        }

        public CoursesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Courses/Index
        [AllowAnonymous]
        public ActionResult Index()
        {
            var c=db.Course.ToList();
            var courses = new List<CourseListViewModel>();
            foreach(var course in c)
            {
                string query = "select * from Schedules where CourseId = " + course.CourseId;
                var schedule = db.Database.SqlQuery<Schedule>(query);
                var courseView = new CourseListViewModel
                {
                    CourseId = course.CourseId,
                    Name = course.Name,
                    Credits = course.Credits,
                    Price = course.Price,
                    Schedules=schedule.ToList().Count()
                };
                courses.Add(courseView);
            }
            return View(courses);
        }


        //
        // GET: /Courses/Create
        [Authorize(Roles = "ADMIN")]
        public ActionResult Create()
        {
             return View();
        }

        //
        // POST: /Courses/Create
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult Create(CourseRegisterViewModel model)
        {
            bool registered = false;
            string message = null;
            string m;
            if (ModelState.IsValid)
            {
                var exist = db.Course.SingleOrDefault(e => e.Name == model.Name);
                if (exist != null)
                {
                    message = "Curso ya esta registrado";
                }
                else
                {
                    var course = new Course
                    {
                        Name = model.Name,
                        Credits = model.Credits,
                        Price = model.Price
                    };
                    try
                    {
                        db.Course.Add(course);
                        db.SaveChanges();
                        message = "Curso registrado";
                        m = DateTime.Now + " Nuevo curso: " + course.Name + " registrado";
                        log.LogAlert(m);
                        registered = true;
                    }
                    catch (Exception ex)
                    {
                        message = DateTime.Now + " Error al registrar curso: " + ex;
                        log.LogError(message);
                        throw ex;
                    } 
                }
            }
            ViewData["Message"] = message;
            if (registered)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(model);
            }
        }

        //
        // GET: /Courses/Edit
        [Authorize(Roles = "ADMIN")]
        public ActionResult Edit(int? id)
        {
            string message;
            if (id == null)
            {
                message=DateTime.Now + " El id del curso estaba vació, Acción: Courses/Edit";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId == id);

            if (course == null)
            {
                message = DateTime.Now + " No se encontró el curso, Acción: Courses/Edit";
                log.LogError(message);
                return HttpNotFound();
            }

            return View(course);
        }

        //
        // POST: /Courses/Edit
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public ActionResult Edit(Course model)
        {
            bool edited = false;
            string message = null;
            string m;
            if (ModelState.IsValid)
            {
                var exist = db.Course.SingleOrDefault(e => e.Name == model.Name && e.CourseId != model.CourseId);
                if (exist != null)
                {
                    message = "Nombre pertenece a otro curso";
                }
                else
                {
                    if (model.Credits<=0 || model.Credits>5)
                    {
                        message = "Los creditos deben estar entre 1 y 5";
                    }
                    else if(model.Price < 20000 || model.Price > 300000)
                    {
                        message = "El precio del curso debe estar entre 20000 y 300000";
                    }
                    else
                    {
                        var course = db.Course.SingleOrDefault(e => e.CourseId == model.CourseId);
                        course.Name = model.Name;
                        course.Credits = model.Credits;
                        course.Price = model.Price;
                        try
                        {
                            db.Entry(course).State = EntityState.Modified;
                            db.SaveChanges();
                            message = "Curso editado";
                            m = DateTime.Now + " El curso: " + course.Name + " fue editado";
                            log.LogAlert(m);
                            edited = true;
                        }
                        catch (Exception ex)
                        {
                            message = DateTime.Now + " Error al editar curso: " + ex;
                            log.LogError(message);
                            throw ex;
                        }
                        
                    }
                }
                ViewData["Message"] = message;
                if (edited)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(model);
                }
            }
            return View(model);
        }

        //
        // GET: /Courses/Delete
        [Authorize(Roles = "ADMIN")]
        public ActionResult Delete(int? id)
        {
            string message;
            if (id == null)
            {
                message = DateTime.Now + " El id del curso estaba vació, Acción: Courses/Delete";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId==id);

            if(course==null)
            {
                message = DateTime.Now + " No se encontró el curso, Acción: Courses/Delete";
                log.LogError(message);
                return HttpNotFound();
            }

            string query = "Select * from Schedules where CourseId=" + id;
            var schedules=db.Database.SqlQuery<Schedule>(query);
            if(schedules.ToList().Count()!=0)
            {
                var enrollments = db.Enrrollment.ToList();
                foreach(var schedule in schedules.ToList())
                {
                    query = "Delete from EnrollmentDetails where IdSchedule=" + schedule.IdSchedule;
                    db.Database.ExecuteSqlCommand(query);
                    db.SaveChanges();
                    query = "Delete from Scores where IdSchedule=" + schedule.IdSchedule;
                    db.Database.ExecuteSqlCommand(query);
                    db.SaveChanges();
                    query = "Delete from Schedules where IdSchedule=" + schedule.IdSchedule;
                    db.Database.ExecuteSqlCommand(query);
                    db.SaveChanges();
                }
                foreach(var enrollment in enrollments)
                {
                    query = "Select * from EnrollmentDetails where IdEnrollment=" + enrollment.IdEnrollment;
                    var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                    if(details.ToList().Count()==0)
                    {
                        try
                        {
                            db.Enrrollment.Remove(enrollment);
                            db.SaveChanges();
                        }
                        catch(Exception ex) 
                        {
                            message = DateTime.Now + " Error al eliminar la matrícula: " + ex;
                            log.LogError(message);
                            throw ex;
                        }
                    }  
                }

                try
                {
                    db.Course.Remove(course);
                    db.SaveChanges();
                    message = DateTime.Now + " Se elimino el curso: " + course.Name;
                    log.LogAlert(message);
                }
                catch (Exception ex)
                {
                    message = DateTime.Now + " Error al eliminar el curso: " + ex;
                    log.LogError(message);
                    throw ex;
                }

            }
            else
            {
                try
                {
                    db.Course.Remove(course);
                    db.SaveChanges();
                    message = DateTime.Now + " Se elimino el curso: " + course.Name;
                    log.LogAlert(message);
                }
                catch (Exception ex)
                {
                    message = DateTime.Now + " Error al eliminar el curso: " + ex;
                    log.LogError(message);
                    throw ex;
                }
            }

            return RedirectToAction("Index");
        }
    }
}
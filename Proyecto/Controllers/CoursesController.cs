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

namespace Proyecto.Controllers
{
    public class CoursesController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();

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

        //List of User Roles
        private List<string> List(string id)
        {
            var user = UserManager.FindById(id);
            var UserRoles = user.Roles.ToList();
            var List = new List<string>();
            var Rol = new AspNetRole();
            foreach (var role in UserRoles)
            {
                Rol = db.AspRole.SingleOrDefault(e => e.Id == role.RoleId);
                if (Rol != null)
                {
                    List.Add(Rol.Name);
                }
            }
            return List;
        }

        //
        // GET: /Courses/Index
        [AllowAnonymous]
        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                var Roles = List(User.Identity.GetUserId());
                if (Roles.Contains("ADMIN"))
                {
                    Session["Rol"] = "ADMIN";
                }
                else
                {
                    if (Roles.Contains("PROFESSOR"))
                    {
                        Session["Rol"] = "PROFESSOR";
                    }
                    Session["Rol"] = "STUDENT";
                }
            }
            else
            {
                Session["Rol"] = "STUDENT";
            }
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
                        registered = true;
                    }
                    catch
                    {
                        message = "Error al Registrar curso";
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId == id);

            if (course == null)
            {
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
                            edited = true;
                        }
                        catch (Exception ex)
                        {
                            message = "Error al editar el curso";
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var course = db.Course.SingleOrDefault(e => e.CourseId==id);

            if(course==null)
            {
                return HttpNotFound();
            }

            try
            {
                db.Course.Remove(course);
                db.SaveChanges();
            }
            catch
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }
    }
}
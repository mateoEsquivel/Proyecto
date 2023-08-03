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

namespace Proyecto.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "ADMIN")]
        // GET: Courses
        public ActionResult Index()
        {
            var courses=db.Course.ToList();
            return View(courses);
        }

        [Authorize(Roles = "ADMIN")]
        // GET: Create
        public ActionResult Create()
        {
             return View();
        }

        // POST: Create
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
                        Credits= model.Credits
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

        // GET: Edit
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

        // GET: POST
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
                    if (model.Credits<=0)
                    {
                        message = "Los creditos deben ser mayor a 0";
                    }
                    else
                    {
                        var course = db.Course.SingleOrDefault(e => e.CourseId == model.CourseId);
                        course.Name = model.Name;
                        course.Credits = model.Credits;
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
    }
}
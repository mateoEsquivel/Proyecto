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

        // GET: Courses
        public ActionResult Index()
        {
            var courses=db.Course.ToList();
            return View(courses);
        }

        // GET: Create
        public ActionResult Create()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
            var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
            if(Rol.Name=="ADMIN")
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        // POST: Create
        [HttpPost]
        public ActionResult Create(CourseRegisterViewModel model)
        {
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
                    db.Course.Add(course);
                    db.SaveChanges();
                    message = "Curso registrado";
                }
            }
            ViewData["Message"] = message;
            return View(model);
        }

        // GET: Edit
        public ActionResult Edit(int? id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
            var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
            if (Rol.Name == "ADMIN")
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

               var course = db.Course.SingleOrDefault(e=>e.CourseId == id);

                if (course == null)
                {
                    return HttpNotFound();
                }

                return View(course);
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: POST
        [HttpPost]
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
                        db.Entry(course).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Curso editado";
                        edited = true;
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
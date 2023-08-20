using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Proyecto.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();

        public HomeController()
        {

        }

        public HomeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        private List<string> List()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
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
        // GET: /Home/Index
        public ActionResult Index()
        {
            Session["Aux"] = null;
            Session["Rol"] = null;
            Session["detail"] = null;
            if (Request.IsAuthenticated)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user.LockoutEnabled)
                {
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                }
               else
               {
                    var Roles=List();
                    if (Roles.Contains("ADMIN"))
                    {
                        Session["Rol"] = "ADMIN";
                    }
                    else
                    {
                        if(Roles.Contains("PROFESSOR"))
                        {
                            Session["Rol"] = "PROFESSOR";
                        }
                        Session["Rol"] = "STUDENT";
                    }
                    
                    Session["Aux"] = true;
               }
            }
            return View();
        }

        //
        // GET: /Home/About
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        //
        // GET: /Home/Contact
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }
}
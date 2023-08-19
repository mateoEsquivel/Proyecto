using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Proyecto.Models;

namespace Proyecto.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
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
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var user= UserManager.FindByEmail(model.Email);
                    if (user.LockoutEnabled)
                    {
                        ViewData["Message"] = "Usuario deshabilitado";
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return View(model);
                    }
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ViewData["Message"] = "Usuario no encontrado";
                    return View(model);
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
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            Session["Rol"] = null;
            if (User.Identity.IsAuthenticated)
            {
                var Roles = List(User.Identity.GetUserId());
                if (Roles.Contains("ADMIN"))
                {
                    Session["Rol"] = "ADMIN";
                    return View();
                }
                else
                {
                    if (Roles.Contains("PROFESSOR"))
                    {
                        Session["Rol"] = "PROFESSOR";
                    }
                    Session["Rol"] = "STUDENT";
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            bool registered = false;
            string message = null;
            if (ModelState.IsValid)
            {
                var exist = UserManager.FindByEmail(model.Email);
                if (exist != null)
                {
                    message = "Usuario ya esta registrado";
                }
                else
                {
                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PhoneNumber=model.Phone, LockoutEndDateUtc= DateTime.Now};
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        if (model.RoleId == null)
                        {
                            var rol = db.AspRole.SingleOrDefault(e => e.Name == "STUDENT");
                            model.RoleId = rol.Id;
                        }
                        else
                        {
                            var rol = db.AspRole.SingleOrDefault(e => e.Name == model.RoleId);
                            model.RoleId = rol.Id;
                        }
                        var userData = new UserData
                        {
                            idUser = user.Id,
                            Name = model.Name,
                            LastName = model.LastName
                        };
                        var userRol = new AspNetUserRole
                        {
                            UserId = user.Id,
                            RoleId = model.RoleId
                        };
                        try
                        {
                            db.UserData.Add(userData);
                            db.AspUserRole.Add(userRol);
                            db.SaveChanges();
                            message = "Usuario registrado";
                            registered = true;
                        }
                        catch(Exception ex)
                        {
                            message = "Error al Registrar :"+ex;
                            throw;
                        }
                        if(!Request.IsAuthenticated)
                        {
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        }
                    }
                    AddErrors(result);
                }
                ViewData["Message"] = message;
                if (registered)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(model);
                }
            }
            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/Lockout
        [Authorize]
        public ActionResult Lockout()
        {
            var id = User.Identity.GetUserId();
            var user = db.AspUser.SingleOrDefault(e => e.Id == id);
            var Roles = List(user.Id);
            if (!Roles.Contains("ADMIN"))
            {
                user.LockoutEnabled = true;
                try
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch(Exception ex)
                {
                    throw;
                }
                
            }
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/LogOff
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
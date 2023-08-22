using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Proyecto.CustomFilters;
using Proyecto.Models;

namespace Proyecto.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();


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
            string message;
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
                        message = DateTime.Now + " El usuario: " + user.UserName + " trato de ingresar en una cuenta deshabilitada";
                        log.LogAlert(message);
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        return View(model);
                    }
                    message = DateTime.Now + " El usuario: " + user.UserName + " inició sesión";
                    log.LogAlert(message);
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    message = DateTime.Now + " Inicio de sesión fallido";
                    log.LogAlert(message);
                    ViewData["Message"] = "Usuario no encontrado";
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Session["Rol"]!=null)
            {
                if (Session["Rol"].ToString()=="ADMIN")
                {
                    return View();
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
                            message = DateTime.Now+" Se registró un nuevo usuario: " + user.Email;
                            log.LogAlert(message);
                            registered = true;
                        }
                        catch(Exception ex)
                        {
                            message = DateTime.Now+" Error al registrar : " + ex;
                            log.LogError(message);
                            throw ex;
                        }
                        if(!Request.IsAuthenticated)
                        {
                            message = DateTime.Now + " El usuario: " + user.UserName + " inició sesión";
                            log.LogAlert(message);
                            await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        }
                    }
                    else
                    {
                        message = DateTime.Now+" Error al registrar usuario";
                        log.LogError(message);
                        AddErrors(result);
                    }
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
           
            return View(model);
        }

        //
        // GET: /Account/Lockout
        [Authorize(Roles = "STUDENT,PROFESSOR")]
        public ActionResult Lockout()
        {
            string message;
            var id = User.Identity.GetUserId();
            var user = db.AspUser.SingleOrDefault(e => e.Id == id);
            if (Session["Rol"].ToString() == "PROFESSOR")
            {
                var query = "Select * from Schedules where ProfessorId='" + id + "'";
                var schedules=db.Database.SqlQuery<Schedule>(query);
                if(schedules.ToList().Count()!=0)
                {
                    message = DateTime.Now + " Se intento deshabilitar un profesor con cursos";
                    log.LogError(message);
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                user.LockoutEnabled = true;
                try
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    message = DateTime.Now + " Se deshabilito el usuario: " + user.Email;
                    log.LogAlert(message);
                }
                catch (Exception ex)
                {
                    message = DateTime.Now + "Error al deshabilitar: " + ex;
                    log.LogError(message);
                    throw ex;
                }
            }
            else
            {
                user.LockoutEnabled = true;
                try
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    message = DateTime.Now + " Se deshabilito el usuario: " + user.Email;
                    log.LogAlert(message);
                }
                catch (Exception ex)
                {
                    message = DateTime.Now + "Error al deshabilitar: " + ex;
                    log.LogError(message);
                    throw ex;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/LogOff
        public ActionResult LogOff()
        {
            string message =DateTime.Now+" El usuario: "+User.Identity.GetUserName()+" cerro la sesión";
            log.LogAlert(message);
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
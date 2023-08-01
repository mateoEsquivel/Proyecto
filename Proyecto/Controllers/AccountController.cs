using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            Session["Rol"] = null;
            if (User.Identity.IsAuthenticated)
            {
                var user = UserManager.FindById(User.Identity.GetUserId());
                if (user.LockoutEnabled)
                {
                    return View();
                }
                else
                {
                    if (user != null)
                    {
                        var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
                        var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
                        if (Rol.Name == "ADMIN")
                        {
                            Session["Rol"] = Rol.Name;
                            return View();
                        }
                    }
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
            var aspUser = new AspNetUser();
            if (Request.IsAuthenticated)
            {
                var id=User.Identity.GetUserId();
                aspUser = new AspNetUser();
                aspUser = db.AspUser.SingleOrDefault(e => e.Id == id);
            }
            if (ModelState.IsValid)
            {
                var exist = UserManager.FindByEmail(model.Email);
                if (exist != null)
                {
                    registered = false;
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
                        db.UserData.Add(userData);
                        db.AspUserRole.Add(userRol);
                        db.SaveChanges();
                        message = "Usuario registrado";
                        registered = true;
                        if (aspUser.Id != null)
                        {
                            if (aspUser.LockoutEnabled)
                            {
                                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            }
                        }
                        else
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
            if (!user.LockoutEnabled)
            {
                var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
                var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
                if (Rol.Name != "ADMIN")
                {
                    user.LockoutEnabled = true;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditAdmin(string email)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
            var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
            if (Rol.Name == "ADMIN")
            {
                if (email != null)
                {
                    var exist = UserManager.FindByEmail(email);
                    if (email == user.Email)
                    {
                        return RedirectToAction("Edit", "Manage");
                    }
                    if (exist == null)
                    {
                        return HttpNotFound();
                    }
                    else
                    {
                        var userData = db.UserData.SingleOrDefault(e => e.idUser == exist.Id);
                        var aspUserRol2 = db.AspUserRole.SingleOrDefault(e => e.UserId == exist.Id);
                        var Rol2 = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol2.RoleId);
                        EditViewModel model = new EditViewModel
                        {
                            UserId=exist.Id,
                            Name = userData.Name,
                            LastName= userData.LastName,
                            Phone=exist.PhoneNumber,
                            Email=exist.Email,
                            RoleId=Rol2.Name,
                            Lockout=exist.LockoutEnabled
                        };
                        return View(model);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditResult(EditViewModel model)
        {
            bool edited = false;
            string message = null;
            var user = UserManager.FindById(User.Identity.GetUserId());
            var aspUserRol = db.AspUserRole.SingleOrDefault(e => e.UserId == user.Id);
            var Rol = db.AspRole.SingleOrDefault(e => e.Id == aspUserRol.RoleId);
            if (Rol.Name == "ADMIN")
            {
                if (ModelState.IsValid)
                {
                    var user2 = db.AspUser.SingleOrDefault(e => e.Email == model.Email && e.Id != model.UserId);
                    if (user2 != null)
                    {
                        message = "Correo pertenece a otro usuario";
                    }
                    else
                    {
                        var userData = db.UserData.SingleOrDefault(e => e.idUser == model.UserId);
                        userData.Name = model.Name;
                        userData.LastName = model.LastName;
                        var user3 = db.AspUser.SingleOrDefault(e => e.Id == model.UserId);
                        user3.PhoneNumber = model.Phone;
                        user3.Email = model.Email;
                        user3.UserName = model.Email;
                        user3.LockoutEnabled = model.Lockout;
                        db.Entry(user3).State = EntityState.Modified;
                        db.Entry(userData).State = EntityState.Modified;
                        db.SaveChanges();
                        message = "Usuario editado";
                        edited = true;
                    }
                    ViewData["Message"] = message;
                    if (edited)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return View(model);
                    }
                }
            }
            return RedirectToAction("EditAdmin", "Account");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
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
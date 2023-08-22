using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Proyecto.CustomFilters;
using Proyecto.Models;

namespace Proyecto.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Manage/Edit
        public ActionResult Edit()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var userData = db.UserData.SingleOrDefault(e => e.idUser == user.Id);
            var model = new EditViewModel
            {
                UserId = user.Id,
                Name = userData.Name,
                LastName = userData.LastName,
                Phone = user.PhoneNumber,
                Email = user.Email
            };
            return View(model);
        }

        //
        // POST: /Manage/Edit
        [HttpPost]
        public ActionResult Edit(EditViewModel model)
        {
            bool edited = false;
            string message = null;
            if (ModelState.IsValid)
            {
                var exist = db.AspUser.SingleOrDefault(e => e.Email == model.Email && e.Id != model.UserId);
                if (exist != null)
                {
                    message = "Correo pertenece a otro usuario";
                }
                else
                {
                    var userData = db.UserData.SingleOrDefault(e => e.idUser == model.UserId);
                    userData.Name = model.Name;
                    userData.LastName = model.LastName;
                    var user = db.AspUser.SingleOrDefault(e => e.Id == model.UserId);
                    user.PhoneNumber = model.Phone;
                    user.Email=model.Email;
                    user.UserName = model.Email;
                    try
                    {
                        db.Entry(user).State = EntityState.Modified;
                        db.Entry(userData).State = EntityState.Modified;
                        db.SaveChanges();
                        message = DateTime.Now + "El usuario: "+user.Email+" fue editado";
                        log.LogAlert(message);
                        edited = true;
                    }
                    catch (Exception ex)
                    {
                        message = DateTime.Now+" Error al editar: "+ex;
                        log.LogError(message);
                        throw ex;
                    }
                    if (User.Identity.GetUserName() != model.Email)
                    {
                        message = DateTime.Now + " El usuario: " + User.Identity.GetUserName() + " cerro la sesión";
                        log.LogAlert(message);
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    }
                }
                ViewData["Message"] = message;
                if (edited)
                {
                    return RedirectToAction("Index","Home");
                }
                else
                {
                    return View(model);
                }
            }
            return View(model);
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            string message;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    message = DateTime.Now + " El usuario: " + User.Identity.GetUserName() + " cerro la sesión";
                    log.LogAlert(message);
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                }
                message = DateTime.Now + " El usuario: " + user.Email + " cambio su contraseña";
                log.LogAlert(message);
                return RedirectToAction("Index","Home");
            }
            message = DateTime.Now + "Error al cambiar contraseña";
            log.LogError(message);
            AddErrors(result);
            return View(model);
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
        // POST: /Manage/EditUser
        [Authorize(Roles ="ADMIN")]
        [HttpPost]
        public ActionResult EditUser(string email)
        {
            if (ModelState.IsValid)
            {
                Session["email"] = email;
                return RedirectToAction("EditAdmin");
            }
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Manage/EditAdmin
        [Authorize(Roles = "ADMIN")]
        public ActionResult EditAdmin()
        {
            string message;
            Session["schedules"] = null;
            string email = Session["email"].ToString();
            Session["email"] = null;
            if (email == User.Identity.GetUserName())
            {
                return RedirectToAction("Edit");
            }
            var user = UserManager.FindByEmail(email);
            if (user == null)
            {
                message = DateTime.Now + " Correo: " + email + " no encontrado";
                log.LogAlert(message);
                return HttpNotFound();
            }
            else
            {
                var userData = db.UserData.SingleOrDefault(e => e.idUser == user.Id);
                var Roles = List(user.Id);
                string role = "";
                if (Roles.Contains("ADMIN"))
                {
                    message = DateTime.Now + " Se trató de editar otro admin";
                    log.LogAlert(message);
                    return HttpNotFound();
                }
                else
                {
                    if (Roles.Contains("PROFESSOR"))
                    {
                        role = "PROFESSOR";
                    }
                    else
                    {
                        role = "STUDENT";
                    }
                }
                string query = "Select * from Schedules where ProfessorId='" + user.Id.ToString() + "'";
                var schedules = db.Database.SqlQuery<Schedule>(query);
                if (schedules.ToList().Count() != 0)
                {
                    Session["schedules"] = schedules.Count();
                }
                EditViewModel model = new EditViewModel
                {
                    UserId = user.Id,
                    Name = userData.Name,
                    LastName = userData.LastName,
                    Phone = user.PhoneNumber,
                    Email = user.Email,
                    RoleId = role,
                    Lockout = user.LockoutEnabled
                };
                return View(model);
            }
        }

        //
        // POST: /Manage/EditAdmin
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public ActionResult EditAdmin(EditViewModel model)
        {
            bool edited = false;
            string message = null;
            if (ModelState.IsValid)
            {
                var exist = db.AspUser.SingleOrDefault(e => e.Email == model.Email && e.Id != model.UserId);
                if (exist != null)
                {
                    message = "Correo pertenece a otro usuario";
                }
                else
                {
                    var userData = db.UserData.SingleOrDefault(e => e.idUser == model.UserId);
                    var user = db.AspUser.SingleOrDefault(e => e.Id == model.UserId);
                    var Roles = List(user.Id);
                    if(!Roles.Contains(model.RoleId))
                    {
                        var rol = db.AspRole.SingleOrDefault(e => e.Name == model.RoleId);
                        var userRol = new AspNetUserRole
                        {
                            UserId = model.UserId,
                            RoleId=rol.Id
                        };
                        try
                        {
                            string query = "Delete from AspNetUserRoles where UserId='" + userRol.UserId.ToString()+"'";
                            db.Database.ExecuteSqlCommand(query);
                            db.AspUserRole.Add(userRol);
                            db.SaveChanges();
                        }
                        catch(Exception ex)
                        {
                            message = DateTime.Now + " Error al agregar el rol: " + ex;
                            log.LogError(message);
                            throw ex;
                        }
                    }
                    userData.Name = model.Name;
                    userData.LastName = model.LastName;
                    user.PhoneNumber = model.Phone;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.LockoutEnabled = model.Lockout;
                    db.Entry(user).State = EntityState.Modified;
                    db.Entry(userData).State = EntityState.Modified;
                    db.SaveChanges();
                    message = DateTime.Now + " Usuario: " + user.Email + " editado";
                    log.LogAlert(message);
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
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
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

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Proyecto.CustomFilters;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Xml.Linq;

namespace Proyecto.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

        public BillsController()
        {

        }

        public BillsController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        // GET: Bills
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int? id) 
        {
            string message;
            if(id==null)
            {
                message = DateTime.Now + " El id de la factura estaba vació, Accion: Bills/Details";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var bill = db.Bill.SingleOrDefault(e => e.IdBill == id);
            if(bill==null)
            {
                message = DateTime.Now + " No se encontró la factura, Accion: Bills/Details";
                log.LogError(message);
                return HttpNotFound();
            }
            if (Session["Rol"].ToString()=="STUDENT")
            {
                if(User.Identity.GetUserId().ToString()!=bill.StudentId)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            var user = db.AspUser.SingleOrDefault(e => e.Id == bill.StudentId);
            var userData = db.UserData.SingleOrDefault(e => e.idUser == bill.StudentId);
            string query = "Select * from BillDetails where IdBill=" + bill.IdBill;
            var details=db.Database.SqlQuery<BillDetail>(query);
            var billView = new BillViewModel
            {
                Student = userData.Name + " " + userData.LastName,
                Email = user.Email,
                Date = bill.Date,
                IdBill = bill.IdBill,
                Details = new List<BillDetails>(),
                SubTotal = bill.Subtotal,
                Discount = bill.Discount,
                Total = bill.Total
            };
            foreach (var detail in details.ToList()) 
            {
                var billDetail = new BillDetails
                {
                    Name = detail.Name,
                    Credits=detail.Credits,
                    Price = detail.Price
                };
                billView.Details.Add(billDetail);
            }
            return View(billView);
        }
    }
}
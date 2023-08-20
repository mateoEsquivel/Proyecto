using Microsoft.AspNet.Identity;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto.Controllers
{
    [Authorize]
    public class BillsController : Controller
    {
        private Context db = new Context();

        // GET: Bills
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id) 
        {
            var enrollment = db.Enrrollment.SingleOrDefault(e => e.IdEnrollment == id);
            if(enrollment!=null)
            {
                var u = db.AspUser.SingleOrDefault(e => e.Id == enrollment.StudentId);
                var user = db.UserData.SingleOrDefault(e => e.idUser == enrollment.StudentId);
                var bill = db.Bill.SingleOrDefault(e => e.IdEnrollment == id);
                string query = "Select * from EnrollmentDetails where IdEnrollment =" + id;
                var details = db.Database.SqlQuery<EnrollmentDetail>(query);
                var list = new List<BillDetails>();
                foreach (var detail in details.ToList())
                {
                    var schedule = db.Schedule.SingleOrDefault(e => e.IdSchedule == detail.IdSchedule);
                    var course = db.Course.SingleOrDefault(e => e.CourseId == schedule.CourseId);
                    string name = course.Name + "(" + schedule.Day + " " + schedule.StartTime + "-" + schedule.EndTime + ")";
                    var billDetail = new BillDetails
                    {
                        Name = name,
                        Price = course.Price
                    };
                    list.Add(billDetail);
                }
                var billView = new BillViewModel
                {
                    Student = user.Name + " " + user.LastName,
                    Email = u.Email,
                    Date = enrollment.Date,
                    IdBill = bill.IdBill,
                    Details = list,
                    Discount = bill.Discount,
                    Total = bill.Total
                };
                return View(billView);
            }

            return HttpNotFound();
        }
    }
}
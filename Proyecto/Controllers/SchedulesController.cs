using Proyecto.CustomFilters;
using Proyecto.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Proyecto.Controllers
{
    public class SchedulesController : Controller
    {
        private Context db = new Context();
        private LogAlerts log = new LogAlerts();

        // GET: Schedules
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            string message;
            if (id == null)
            {
                message = DateTime.Now + " El id del curso estaba vació, Acción: Schedules/Details";
                log.LogError(message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var course = db.Course.SingleOrDefault(e => e.CourseId == id);
            if (course == null)
            {
                message = DateTime.Now + " No se encontró el curso, Acción: Schedules/Details";
                log.LogError(message);
                return HttpNotFound();
            }
            string query = "Select * from Schedules where CourseId=" + id;
            var schedules = db.Database.SqlQuery<Schedule>(query);
            if (schedules.ToList().Count() == 0)
            {
                message = DateTime.Now + " Se busco un curso sin horarios, Acción: Schedules/Details";
                log.LogError(message);
                return HttpNotFound();
            }
            var detail = new CourseDetailsViewModel
            {
                Name = course.Name,
                List = new List<CourseDetailsListViewModel>()
            };
            foreach (var schedule in schedules.ToList())
            {
                var teacher = db.UserData.SingleOrDefault(e => e.idUser == schedule.ProfessorId);
                var custom = new CourseDetailsListViewModel
                {
                    ScheduleId = schedule.IdSchedule,
                    Day = schedule.Day,
                    StartTime = schedule.StartTime,
                    EndTime = schedule.EndTime,
                    Name = teacher.Name + " " + teacher.LastName,
                    Students=schedule.Students
                };
                detail.List.Add(custom);
            }

            return View(detail);
        }
    }
}
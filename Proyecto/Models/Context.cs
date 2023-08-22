using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Context : DbContext
    {
        public Context() : base("DefaultConnection")
        {

        }
        public DbSet<AspNetRole> AspRole { get; set; }
        public DbSet<AspNetUser> AspUser { get; set; }
        public DbSet<AspNetUserRole> AspUserRole { get; set; }
        public DbSet<Bill> Bill { get; set; }
        public DbSet<BillDetail> BillDetail { get; set; }
        public DbSet<Course> Course { get; set; }
        public DbSet<Enrollment> Enrrollment { get; set; }
        public DbSet<EnrollmentDetail> EnrrollmentDetail { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Score> Score { get; set; }
        public DbSet<ScoreDetail> ScoreDetail { get; set; }
        public DbSet<UserData> UserData { get; set; }
    }
}
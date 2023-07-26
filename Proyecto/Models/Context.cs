using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class Context : DbContext
    {
        public Context() : base("Context")
        {

        }
        public DbSet<Course> Course { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Bill> Bill { get; set; }
        public DbSet<Enrollment> Enrrollment { get; set; }
        public DbSet<Role> Rol { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Professor> Professor { get; set; }
        public DbSet<Schedule> Schedule { get; set; }

    }
}
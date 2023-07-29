namespace Proyecto.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class actualizacionTablas : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Enrollments", "Course_CourseId", "dbo.Courses");
            DropForeignKey("dbo.Enrollments", "IdSchedule", "dbo.Schedules");
            DropForeignKey("dbo.Courses", "Professor_ProfessorId", "dbo.Professors");
            DropForeignKey("dbo.Professors", "Username", "dbo.Users");
            DropForeignKey("dbo.Enrollments", "StudentId", "dbo.Students");
            DropForeignKey("dbo.Students", "Username", "dbo.Users");
            DropForeignKey("dbo.Schedules", "ProfessorId", "dbo.Professors");
            DropForeignKey("dbo.Users", "Role_RolId", "dbo.Roles");
            DropForeignKey("dbo.Enrollments", "Bill_IdBill", "dbo.Bills");
            DropIndex("dbo.Enrollments", new[] { "IdSchedule" });
            DropIndex("dbo.Enrollments", new[] { "StudentId" });
            DropIndex("dbo.Enrollments", new[] { "Course_CourseId" });
            DropIndex("dbo.Enrollments", new[] { "Bill_IdBill" });
            DropIndex("dbo.Schedules", new[] { "ProfessorId" });
            DropIndex("dbo.Courses", new[] { "Professor_ProfessorId" });
            DropIndex("dbo.Professors", new[] { "Username" });
            DropIndex("dbo.Users", new[] { "Role_RolId" });
            DropIndex("dbo.Students", new[] { "Username" });
            RenameColumn(table: "dbo.Bills", name: "Bill_IdBill", newName: "IdEnrollment");
            DropPrimaryKey("dbo.Enrollments");
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                        AspNetRoles_Id = c.String(maxLength: 128),
                        AspNetUsers_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.AspNetRoles_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AspNetUsers_Id)
                .Index(t => t.AspNetRoles_Id)
                .Index(t => t.AspNetUsers_Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EnrollmentDetails",
                c => new
                    {
                        IdEnrollment = c.Int(nullable: false),
                        IdDetail = c.Int(nullable: false),
                        IdSchedule = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                    })
                .PrimaryKey(t => new { t.IdEnrollment, t.IdDetail })
                .ForeignKey("dbo.Enrollments", t => t.IdEnrollment, cascadeDelete: true)
                .ForeignKey("dbo.Schedules", t => t.IdSchedule, cascadeDelete: true)
                .Index(t => t.IdEnrollment)
                .Index(t => t.IdSchedule);
            
            CreateTable(
                "dbo.Scores",
                c => new
                    {
                        IdScore = c.Int(nullable: false, identity: true),
                        IdSchedule = c.Int(nullable: false),
                        StudentId = c.String(),
                        StudentScore = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AspNetUsers_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.IdScore)
                .ForeignKey("dbo.AspNetUsers", t => t.AspNetUsers_Id)
                .ForeignKey("dbo.Schedules", t => t.IdSchedule, cascadeDelete: true)
                .Index(t => t.IdSchedule)
                .Index(t => t.AspNetUsers_Id);
            
            CreateTable(
                "dbo.UserDatas",
                c => new
                    {
                        idData = c.Int(nullable: false, identity: true),
                        idUser = c.String(),
                        Name = c.String(),
                        LastName = c.String(),
                        AspNetUsers_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.idData)
                .ForeignKey("dbo.AspNetUsers", t => t.AspNetUsers_Id)
                .Index(t => t.AspNetUsers_Id);
            
            AddColumn("dbo.Bills", "Discount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Bills", "Total", c => c.Double(nullable: false));
            AddColumn("dbo.Enrollments", "IdEnrollment", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Enrollments", "Total", c => c.Double(nullable: false));
            AddColumn("dbo.Enrollments", "AspNetUsers_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Schedules", "Students", c => c.Int(nullable: false));
            AddColumn("dbo.Schedules", "AspNetUsers_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Courses", "Credits", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Enrollments", "IdEnrollment");
            CreateIndex("dbo.Enrollments", "AspNetUsers_Id");
            CreateIndex("dbo.Bills", "IdEnrollment");
            CreateIndex("dbo.Schedules", "AspNetUsers_Id");
            AddForeignKey("dbo.Enrollments", "AspNetUsers_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Schedules", "AspNetUsers_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Bills", "IdEnrollment", "dbo.Enrollments", "IdEnrollment", cascadeDelete: true);
            DropColumn("dbo.Bills", "LegalID");
            DropColumn("dbo.Enrollments", "IdBill");
            DropColumn("dbo.Enrollments", "IdSchedule");
            DropColumn("dbo.Enrollments", "Credits");
            DropColumn("dbo.Enrollments", "Amount");
            DropColumn("dbo.Enrollments", "Score");
            DropColumn("dbo.Enrollments", "Course_CourseId");
            DropColumn("dbo.Enrollments", "Bill_IdBill");
            DropColumn("dbo.Courses", "Professor_ProfessorId");
            DropTable("dbo.Professors");
            DropTable("dbo.Users");
            DropTable("dbo.Students");
            DropTable("dbo.Roles");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RolId = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.RolId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        StudentId = c.Int(nullable: false, identity: true),
                        Username = c.String(maxLength: 128),
                        Name = c.String(),
                        LastName = c.String(),
                        PhoneNumber = c.Int(nullable: false),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.StudentId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        Password = c.String(),
                        Status = c.Binary(),
                        Role_RolId = c.Int(),
                    })
                .PrimaryKey(t => t.Username);
            
            CreateTable(
                "dbo.Professors",
                c => new
                    {
                        ProfessorId = c.Int(nullable: false, identity: true),
                        Username = c.String(maxLength: 128),
                        Name = c.String(),
                        LastName = c.String(),
                        PhoneNumber = c.Int(nullable: false),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.ProfessorId);
            
            AddColumn("dbo.Courses", "Professor_ProfessorId", c => c.Int());
            AddColumn("dbo.Enrollments", "Bill_IdBill", c => c.Int());
            AddColumn("dbo.Enrollments", "Course_CourseId", c => c.Int());
            AddColumn("dbo.Enrollments", "Score", c => c.Int(nullable: false));
            AddColumn("dbo.Enrollments", "Amount", c => c.Double(nullable: false));
            AddColumn("dbo.Enrollments", "Credits", c => c.Int(nullable: false));
            AddColumn("dbo.Enrollments", "IdSchedule", c => c.Int(nullable: false));
            AddColumn("dbo.Enrollments", "IdBill", c => c.Int(nullable: false, identity: true));
            AddColumn("dbo.Bills", "LegalID", c => c.Int(nullable: false));
            DropForeignKey("dbo.Bills", "IdEnrollment", "dbo.Enrollments");
            DropForeignKey("dbo.UserDatas", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.Scores", "IdSchedule", "dbo.Schedules");
            DropForeignKey("dbo.Scores", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EnrollmentDetails", "IdSchedule", "dbo.Schedules");
            DropForeignKey("dbo.Schedules", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EnrollmentDetails", "IdEnrollment", "dbo.Enrollments");
            DropForeignKey("dbo.Enrollments", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "AspNetUsers_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "AspNetRoles_Id", "dbo.AspNetRoles");
            DropIndex("dbo.UserDatas", new[] { "AspNetUsers_Id" });
            DropIndex("dbo.Scores", new[] { "AspNetUsers_Id" });
            DropIndex("dbo.Scores", new[] { "IdSchedule" });
            DropIndex("dbo.Schedules", new[] { "AspNetUsers_Id" });
            DropIndex("dbo.EnrollmentDetails", new[] { "IdSchedule" });
            DropIndex("dbo.EnrollmentDetails", new[] { "IdEnrollment" });
            DropIndex("dbo.Bills", new[] { "IdEnrollment" });
            DropIndex("dbo.Enrollments", new[] { "AspNetUsers_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "AspNetUsers_Id" });
            DropIndex("dbo.AspNetUserRoles", new[] { "AspNetRoles_Id" });
            DropPrimaryKey("dbo.Enrollments");
            DropColumn("dbo.Courses", "Credits");
            DropColumn("dbo.Schedules", "AspNetUsers_Id");
            DropColumn("dbo.Schedules", "Students");
            DropColumn("dbo.Enrollments", "AspNetUsers_Id");
            DropColumn("dbo.Enrollments", "Total");
            DropColumn("dbo.Enrollments", "IdEnrollment");
            DropColumn("dbo.Bills", "Total");
            DropColumn("dbo.Bills", "Discount");
            DropTable("dbo.UserDatas");
            DropTable("dbo.Scores");
            DropTable("dbo.EnrollmentDetails");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            AddPrimaryKey("dbo.Enrollments", "IdBill");
            RenameColumn(table: "dbo.Bills", name: "IdEnrollment", newName: "Bill_IdBill");
            CreateIndex("dbo.Students", "Username");
            CreateIndex("dbo.Users", "Role_RolId");
            CreateIndex("dbo.Professors", "Username");
            CreateIndex("dbo.Courses", "Professor_ProfessorId");
            CreateIndex("dbo.Schedules", "ProfessorId");
            CreateIndex("dbo.Enrollments", "Bill_IdBill");
            CreateIndex("dbo.Enrollments", "Course_CourseId");
            CreateIndex("dbo.Enrollments", "StudentId");
            CreateIndex("dbo.Enrollments", "IdSchedule");
            AddForeignKey("dbo.Enrollments", "Bill_IdBill", "dbo.Bills", "IdBill");
            AddForeignKey("dbo.Users", "Role_RolId", "dbo.Roles", "RolId");
            AddForeignKey("dbo.Schedules", "ProfessorId", "dbo.Professors", "ProfessorId", cascadeDelete: true);
            AddForeignKey("dbo.Students", "Username", "dbo.Users", "Username");
            AddForeignKey("dbo.Enrollments", "StudentId", "dbo.Students", "StudentId", cascadeDelete: true);
            AddForeignKey("dbo.Professors", "Username", "dbo.Users", "Username");
            AddForeignKey("dbo.Courses", "Professor_ProfessorId", "dbo.Professors", "ProfessorId");
            AddForeignKey("dbo.Enrollments", "IdSchedule", "dbo.Schedules", "IdSchedule", cascadeDelete: true);
            AddForeignKey("dbo.Enrollments", "Course_CourseId", "dbo.Courses", "CourseId");
        }
    }
}

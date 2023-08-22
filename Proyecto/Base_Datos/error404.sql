use error404;

create Table UserDatas
(idData int identity(1,1) not null,
idUser nvarchar(128) not null,
Name nvarchar(256) not null,
LastName nvarchar(256) not null,
primary key(idData))

alter table UserDatas add foreign key (idUser) references AspNetUsers(Id);

create Table Courses
(CourseId int identity(1,1) not null,
Name nvarchar(256),
Credits int not null,
Price float not null,
primary key(CourseId))

create Table Schedules
(IdSchedule int identity(1,1) not null,
CourseId int not null,
ProfessorId nvarchar(128) not null,
Day nvarchar(256) not null,
StartTime time not null,
EndTime time not null,
Students int not null,
primary key(IdSchedule))

alter table Schedules add foreign key (CourseId) references Courses(CourseId);
alter table Schedules add foreign key (ProfessorId) references AspNetUsers(Id);

create Table Scores
(IdScore int identity(1,1) not null,
IdSchedule int not null,
StudentId nvarchar(128) not null,
StudentScore float not null,
primary key(IdScore))

alter table Scores add foreign key (IdSchedule) references Schedules(IdSchedule);
alter table Scores add foreign key (StudentId) references AspNetUsers(Id);

create Table ScoreDetails
(
IdScore int not null,
IdDetail int not null,
Name nvarchar(256) not null,
Score float not null,
Percentage int not null,
primary key(IdScore,IdDetail));

alter Table ScoreDetails add foreign key (IdScore) references Scores(IdScore);

create Table Enrollments
(IdEnrollment int identity(1,1) not null,
StudentId nvarchar(128) not null,
Date date not null,
primary key(IdEnrollment))

alter table Enrollments add foreign key (StudentId) references AspNetUsers(Id);

create Table EnrollmentDetails
(IdEnrollment int not null,
IdDetail int not null,
IdSchedule int not null,
primary key(IdEnrollment,IdDetail))

alter table EnrollmentDetails add foreign key (IdEnrollment) references Enrollments(IdEnrollment);
alter table EnrollmentDetails add foreign key (IdSchedule) references Schedules(IdSchedule);

create Table Bills
(IdBill int identity(1,1) not null,
StudentId nvarchar(128) not null,
Date date not null,
Subtotal float not null,
Discount float not null,
Total float not null,
primary key(IdBill))

alter table Bills add foreign key (StudentId) references AspNetUsers(Id);

create Table BillDetails
(IdBill int not null,
IdDetail int not null,
IdSchedule int not null,
Name nvarchar(256) not null,
Credits int not null,
Price float not null,
primary key(IdBill,IdDetail))

alter table BillDetails add foreign key (IdBill) references Bills(IdBill);

insert into AspNetRoles(Id,Name)
values(NEWID(),'ADMIN'),
(NEWID(),'PROFESSOR'),
(NEWID(),'STUDENT')

Create trigger UserLockout
on AspNetUsers
after insert
as
Begin
	DECLARE
	@UserID nvarchar(128)
	set nocount on;
	select @UserID=inserted.[Id] FROM INSERTED
	Update AspNetUsers
	set LockoutEnabled=0
	where Id=@UserID
end;

Create trigger students
on EnrollmentDetails
after insert
as
Begin
	DECLARE
	@ID int
	set nocount on;
	select @ID=inserted.[IdSchedule] FROM INSERTED
	Update Schedules
	set Students=Students+1
	where IdSchedule=@ID
end;


/*SELECTS*/
/*select * from AspNetUsers;

select Name as Rol,Email
from AspNetUserRoles A,AspNetRoles B, AspNetUsers C
where A.UserId=C.Id and A.RoleId=B.Id;

select * from UserDatas; 

select * from AspNetRoles;

select * from Courses;

select * from Schedules;

select * from Enrollments;

select * from EnrollmentDetails;

select * from Bills;

select * from BillDetails;

select * from Scores;

select * from ScoreDetails;
*/


/*CUIDADAO DELETES*/
/*delete from AspNetUserRoles
delete from AspNetRoles
delete from AspNetUsers
delete from userDatas*/



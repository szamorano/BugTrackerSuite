namespace BugTrackerSuite.Migrations
{
    using BugTrackerSuite.Models;
    using BugTrackerSuite.Models.CodeFirst;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTrackerSuite.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTrackerSuite.Models.ApplicationDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "Project Manager"))
            {
                roleManager.Create(new IdentityRole { Name = "Project Manager" });
            }
            if (!context.Roles.Any(r => r.Name == "Developer"))
            {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }
            if (!context.Roles.Any(r => r.Name == "Submitter"))
            {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));


            if (!context.Users.Any(u => u.Email == "stevenzamorano.code@gmail.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "stevenzamorano.code@gmail.com",
                    Email = "stevenzamorano.code@gmail.com",
                    FirstName = "Steven",
                    LastName = "Zamorano",
                }, "Password1!");
            }
            var userId = userManager.FindByEmail("stevenzamorano.code@gmail.com").Id;
            userManager.AddToRole(userId, "Admin");


            if (!context.Users.Any(u => u.Email == "mjaang@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "mjaang@coderfoundry.com",
                    Email = "mjaang@coderfoundry.com",
                    FirstName = "Mark",
                    LastName = "Jaang",
                }, "Password1!");
            }
            var userId_mark = userManager.FindByEmail("mjaang@coderfoundry.com").Id;
            userManager.AddToRole(userId_mark, "Project Manager");


            if (!context.Users.Any(u => u.Email == "rchapman@coderfoundry.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "rchapman@coderfoundry.com",
                    Email = "rchapman@coderfoundry.com",
                    FirstName = "Ryan",
                    LastName = "Chapman",
                }, "Password1!");
            }
            var userId_ryan = userManager.FindByEmail("rchapman@coderfoundry.com").Id;
            userManager.AddToRole(userId_ryan, "Project Manager");


            if (!context.Users.Any(u => u.Email == "admin.demo@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "admin.demo@bugtracker.com",
                    Email = "admin.demo@bugtracker.com",
                    FirstName = "Admin",
                    LastName = "Demo",
                }, "Password1!");
            }
            var userId_adminDemo = userManager.FindByEmail("admin.demo@bugtracker.com").Id;
            userManager.AddToRole(userId_adminDemo, "Admin");


            if (!context.Users.Any(u => u.Email == "projectmanager.demo@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "projectmanager.demo@bugtracker.com",
                    Email = "projectmanager.demo@bugtracker.com",
                    FirstName = "ProjectManager",
                    LastName = "Demo",
                }, "Password1!");
            }
            var userId_projectManagerDemo = userManager.FindByEmail("projectmanager.demo@bugtracker.com").Id;
            userManager.AddToRole(userId_projectManagerDemo, "Project Manager");


            if (!context.Users.Any(u => u.Email == "developer.demo@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "developer.demo@bugtracker.com",
                    Email = "developer.demo@bugtracker.com",
                    FirstName = "Developer",
                    LastName = "Demo",
                }, "Password1!");
            }
            var userId_developerDemo = userManager.FindByEmail("developer.demo@bugtracker.com").Id;
            userManager.AddToRole(userId_developerDemo, "Developer");


            if (!context.Users.Any(u => u.Email == "submitter.demo@bugtracker.com"))
            {
                userManager.Create(new ApplicationUser
                {
                    UserName = "submitter.demo@bugtracker.com",
                    Email = "submitter.demo@bugtracker.com",
                    FirstName = "Submitter",
                    LastName = "Demo",
                }, "Password1!");
            }
            var userId_submitterDemo = userManager.FindByEmail("submitter.demo@bugtracker.com").Id;
            userManager.AddToRole(userId_submitterDemo, "Submitter");


            if (!context.TicketPriorities.Any(p => p.Name == "Low"))
            {
                var priority = new TicketPriority();
                priority.Name = "Low";
                context.TicketPriorities.Add(priority);
            }

            if (!context.TicketPriorities.Any(p => p.Name == "Medium"))
            {
                var priority = new TicketPriority();
                priority.Name = "Medium";
                context.TicketPriorities.Add(priority);
            }

            if (!context.TicketPriorities.Any(p => p.Name == "High"))
            {
                var priority = new TicketPriority();
                priority.Name = "High";
                context.TicketPriorities.Add(priority);
            }

            if (!context.TicketPriorities.Any(p => p.Name == "Urgent"))
            {
                var priority = new TicketPriority();
                priority.Name = "Urgent";
                context.TicketPriorities.Add(priority);
            }

            if (!context.TicketStatuses.Any(p => p.Name == "Unassigned"))
            {
                var status = new TicketStatus();
                status.Name = "Unassigned";
                context.TicketStatuses.Add(status);
            }

            if (!context.TicketStatuses.Any(p => p.Name == "Assigned"))
            {
                var status = new TicketStatus();
                status.Name = "Assigned";
                context.TicketStatuses.Add(status);
            }

            if (!context.TicketStatuses.Any(p => p.Name == "In Progress"))
            {
                var status = new TicketStatus();
                status.Name = "In Progress";
                context.TicketStatuses.Add(status);
            }

            if (!context.TicketStatuses.Any(p => p.Name == "Complete"))
            {
                var status = new TicketStatus();
                status.Name = "Complete";
                context.TicketStatuses.Add(status);
            }

            if (!context.TicketTypes.Any(p => p.Name == "Hardware"))
            {
                var type = new TicketType();
                type.Name = "Hardware";
                context.TicketTypes.Add(type);
            }

            if (!context.TicketTypes.Any(p => p.Name == "Software"))
            {
                var type = new TicketType();
                type.Name = "Software";
                context.TicketTypes.Add(type);
            }
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}

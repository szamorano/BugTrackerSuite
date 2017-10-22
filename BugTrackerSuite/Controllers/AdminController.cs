using BugTrackerSuite.Models;
using BugTrackerSuite.Models.CodeFirst;
using BugTrackerSuite.Models.Helpers;
using BugTrackerSuite.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerSuite.Controllers
{
    [Authorize]
    public class AdminController : Universal
    {
        ApplicationDbContext db = new ApplicationDbContext();

        //private UserRoleHelper helper = new UserRoleHelper();

        // GET: Admin
        public ActionResult Index()
        {
            List<AdminUserViewModels> users = new List<AdminUserViewModels>();   // Creates a new list of "users" into an array
            UserRoleHelper helper = new UserRoleHelper();                        // Tells "helper" to use the UserRoleHelper methods, in this method

            foreach (var user in db.Users.ToList())                              // Begins the loop, going through the users in the database
            {
                var eachUser = new AdminUserViewModels();                        // Tells "eachUser" to use the model class, its this codeblock specific
                eachUser.User = user;                                            // Assigns "eachUser" the loop specific iteration
                eachUser.SelectedRoles = helper.ListUserRoles(user.Id).ToArray();  // Uses the helper.ListUserRoles to attach the roles into SelectedRoles

                users.Add(eachUser);                                             // Adds "eachUser" to the "users" list we created
            }
            return View(users.OrderBy(u => u.User.LastName).ToList());           // Orders the "users" list alphabetically into another list by last name
        }

        //GET: EditUserRoles
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(string id)                            // Grabs the Id of whatever user is selected in the Admin Index View
        {
            var user = db.Users.Find(id);                                       // Finds the Id in the database and assigns it to "var user"
            var helper = new UserRoleHelper();                                  // Allows "helper" to use the UserRoleHelper methods, in this method
            var model = new AdminUserViewModels();                              // Tells "model" to use the model class, its this codeblock specific
            model.User = user;                                                  // The Id gets assigned to model.User
            model.SelectedRoles = helper.ListUserRoles(id).ToArray();           // Uses the helper.ListUserRoles to attach the roles into SelectedRoles
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles); // Adds the multiselectlist of roles and the selected roles to model.roles

            return View(model);
        }

        //POST: EditUserRoles
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(AdminUserViewModels model)   // Uses the AdminUserViewModel with the values that were passed to it in the GET action
        {
            var user = db.Users.Find(model.User.Id);                    // The Id gets passed to var user
            UserRoleHelper helper = new UserRoleHelper();               // Allows "helper" to use the UserRoleHelper methods, in this method

            foreach (var role in db.Roles.Select(r => r.Name).ToList()) // Grabs all the roles and puts them into a list 
            {
                helper.RemoveUserFromRole(user.Id, role);               // It removes the roles that were in the list from the user; leaving it empty
            }

            if (model.SelectedRoles != null)                            // Makes sure there are Selected Roles and its not null
            {
                foreach (var role in model.SelectedRoles)               // Loops through for each role in Selected Roles
                {
                    helper.AddUserToRole(user.Id, role);                // Then adds the role/roles to the user
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }




        // GET: Project/Details/5
        //public ActionResult Details(string Id)
        //{

        //    UserProfileViewModel user = db.Users.Find(Id);
        //    if (user == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(user);
        //}





        public ActionResult ProfilePage()
        {
            ViewBag.Message = "Your Profile page.";

            return View();
        }

    }
}
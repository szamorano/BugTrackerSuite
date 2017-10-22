using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTrackerSuite.Models;
using BugTrackerSuite.Models.CodeFirst;
using BugTrackerSuite.Models.Helpers;
using Microsoft.AspNet.Identity;

namespace BugTrackerSuite.Controllers
{
    [Authorize]
    public class ProjectController : Universal
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Project
        public ActionResult Index()
        {
            //List<ProjectUserViewModel> projects = new List<ProjectUserViewModel>();
            //ProjectAssignHelper helper = new ProjectAssignHelper();

            //foreach (var project in db.Projects.ToList())

            //    var eachProject = new ProjectUserViewModel();
            //    eachProject.AssignProject = project;
            //    eachProject.AssignProjectId = project.Id;
            //    eachProject.SelectedUsers = helper.ListUserProjects(project.Id).ToArray();

            //    projects.Add(eachProject);
            //}
            //return View(projects.OrderBy(p => p.AssignProject.Title).ToList());
            ViewBag.UserTimeZone = db.Users.Find(User.Identity.GetUserId()).TimeZone;
                return View(db.Projects.ToList());       //Original  Method,  its the only line
            }


        // GET: EditProjectUsers
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProjectUsers(int id)
        {
            var project = db.Projects.Find(id);
            var user = db.Users.Find(User.Identity.GetUserId());
            var helper = new ProjectAssignHelper();
            var model = new ProjectUserViewModel();
            model.AssignProject = project;
            model.AssignProjectId = id;
            model.SelectedUsers = helper.ListUserOnProject(id).Select(u => u.Id).ToArray();
            model.Users = new MultiSelectList(db.Users, "Id", "FirstName", model.SelectedUsers);

            return View(model);
        }

        // POST: EditProjectUsers
        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditProjectUsers(ProjectUserViewModel model)
        {
            var project = db.Projects.Find(model.AssignProjectId);
            var helper = new ProjectAssignHelper();                             // ProjectAssignHelper helper = new ProjectAssignHelper();

            foreach ( var user in db.Users.Select(u => u.Id).ToList())
            {
                helper.RemoveUserFromProject(user, project.Id);
            }

            if (model.SelectedUsers != null)
            {
                foreach (var user in model.SelectedUsers)
                {
                    helper.AddUserToProject(user, project.Id);
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }


        // GET: Project/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Project/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Project/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (ModelState.IsValid)
            {
                project.Created = DateTimeOffset.UtcNow;   //Change to DateTimeOffSet.UtcNow;
                project.AuthorId = user.FullName;
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Project/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Project/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize (Roles = "Admin, ProjectManager")]
        public ActionResult ProjectUser(int id)
        {
            var project = db.Projects.Find(id);
            ProjectUserViewModel projectuserVM = new ProjectUserViewModel();
            projectuserVM.AssignProjectId = project.Id;
            projectuserVM.AssignProjectId = id;
            projectuserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();
            projectuserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "FirstName", projectuserVM.SelectedUsers);
            return View(projectuserVM);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult ProjectUser(ProjectUserViewModel model)
        {
            ProjectAssignHelper helper = new ProjectAssignHelper();
            foreach(var user  in db.Users.Select(r => r.Id).ToList())
            {
                helper.RemoveUserFromProject(user, model.AssignProjectId);
            }

            foreach(var userId in model.SelectedUsers)
            {
                helper.AddUserToProject(userId, model.AssignProjectId);
            }
                
            return View();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

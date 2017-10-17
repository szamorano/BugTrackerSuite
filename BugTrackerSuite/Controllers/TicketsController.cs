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
using Microsoft.AspNet.Identity;
using System.IO;
using BugTrackerSuite.Models.Helpers;
using BugTracker.Models.Helpers;
using System.Net.Mail;
using System.Configuration;
using System.Threading.Tasks;

namespace BugTrackerSuite.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (User.IsInRole("Admin"))
            {
                return View(db.Tickets.ToList());
            }
            else if (User.IsInRole("Project Manager"))
            {
                return View(db.Tickets.Where( t => t.Project.Users.Any(u => u.Id == user.Id)).ToList());
            }    
            else if (User.IsInRole("Developer"))
            {
                return View(db.Tickets.Where(t => t.AssignToUserId == user.Id).ToList());
            }
            else if (User.IsInRole("Submitter"))
            {
                return View(db.Tickets.Where(t => t.OwnerUserId == user.Id).ToList());
            }

            return View();
        }


        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            // Role Checking Security
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            ViewBag.ProjectId = new SelectList(db.Projects.Where(p => p.Users.Any(u => u.Id == user.Id)), "Id", "Title");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Submitter")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            TicketHistory ticketHistory = new TicketHistory();
            var user = db.Users.Find(User.Identity.GetUserId());

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.OwnerUserId = user.Id;
                ticket.TicketStatusId = 1;

                ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                ticketHistory.Created = DateTime.Now;
                ticketHistory.Property = "TICKET CREATED";
                ticketHistory.TicketId = ticket.Id;
                db.Tickets.Add(ticket);
                db.TicketHistories.Add(ticketHistory);

                


                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects.Where(p => p.Users.Any(u => u.Id == user.Id)), "Id", "Title", ticket.ProjectId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles = "Admin ,Project Manager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin ,Project Manager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,Created,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {

            TicketHistory ticketHistory = new TicketHistory();
            if (ModelState.IsValid)
            {
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                ApplicationUser oldDev = new ApplicationUser();
                ApplicationUser newDev = new ApplicationUser();
                if (oldTicket.AssignToUserId != ticket.AssignToUserId)
                {

                    if (oldTicket.AssignToUserId != null)
                    {
                        oldDev = db.Users.Find(oldTicket.AssignToUserId);
                    }
                    newDev = db.Users.Find(ticket.AssignToUserId);
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET ASSIGNED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldDev.FullName;
                    ticketHistory.NewValue = newDev.FullName;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }

                if (oldTicket.TicketPriorityId != ticket.TicketPriorityId)
                {
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET PRIORITY CHANGED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldTicket.TicketPriority.Name;
                    //ticketHistory.NewValue = ticket.TicketPriority.Name;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }

                if (oldTicket.Title != ticket.Title)
                {
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET TITLE CHANGED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldTicket.Title;
                    ticketHistory.NewValue = ticket.Title;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }

                if (oldTicket.Description != ticket.Description)
                {
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET DESCRIPTION CHANGED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldTicket.Description;
                    ticketHistory.NewValue = ticket.Description;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }

                if (oldTicket.TicketStatusId != ticket.TicketStatusId)
                {
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET STATUS CHANGED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldTicket.TicketStatus.Name;
                    //ticketHistory.NewValue = ticket.TicketStatus.Name;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();


                }

                if (oldTicket.TicketTypeId != ticket.TicketTypeId)
                {
                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET TYPE CHANGED";
                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.OldValue = oldTicket.TickerType.Name;
                    //ticketHistory.NewValue = ticket.TickerType.Name;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }


                IdentityMessage messageforNewDev = new IdentityMessage();
                messageforNewDev.Subject = "Bugtracker Notifications";
                if (oldDev != null)
                {
                    messageforNewDev.Body = $"Your ticket has been assigned to { newDev.FullName } from { oldDev.FullName }.";
                }
                else
                {
                    messageforNewDev.Body = $"Your ticket has been assigned to { newDev.FullName }.";
                }

                messageforNewDev.Destination = db.Users.Find(ticket.AssignToUserId).Email;
                EmailService email = new EmailService();
                await email.SendAsync(messageforNewDev);

                //if (oldDev != null)
                //{
                //    IdentityMessage messageforOldDev = new IdentityMessage();

                //    messageforOldDev.Body = $"Your ticket has been assigned to { newDev.FullName } from { oldDev.FullName }.";
                //    messageforOldDev.Subject = "Bugtracker Notifications";
                //    messageforOldDev.Destination = oldDev.Email;

                //    EmailService emailOld = new EmailService();
                //    await emailOld.SendAsync(messageforOldDev);
                //}


                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            UserRoleHelper helper = new UserRoleHelper();
            var developers = helper.UsersInRole("Developer");
            var devsOnTicketProject = developers.Where(d => d.Projects.Any(p => p.Id == ticket.ProjectId));

            db.TicketHistories.Add(ticketHistory);

            //if (UserRoleHelper.ListRoleUsers("Developer").Where(u => helper.UserIsOnProject(u.Id, ticket.ProjectId)).Count() > 0)
            //{
            //    vm.AssignToUserList = new SelectList(roleHelper.ListRoleUsers("Developer").Where(u => helper.UserIsOnProject(u.Id, ticket.ProjectId)), "Id", "FullName", vm.AssignToUserId);
            //}
            //else
            //{
            //    vm.AssignToUserList = null;
            //}


            ViewBag.AssignToUserId = new SelectList(devsOnTicketProject, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }































        //// GET: Ticket History Get Action
        //public ActionResult HistoryIndex()
        //{
        //    var histories = db.TicketHistories.Include(h => h.Author).Include(h => h.Ticket);
        //    return View(histories.ToList());
        //}

        //// GET: TicketHistory/Create
        //public ActionResult HistoryCreate()
        //{
        //    return View();
        //}

        //// POST: TicketHistory/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult HistoryCreate([Bind(Include = "Id,TicketId,Body")] TicketHistory ticketHistory)
        //{
        //    var user = db.Users.Find(User.Identity.GetUserId());
        //    var ticket = db.Tickets.Find(ticketHistory.TicketId);

        //    if (ModelState.IsValid) /*&& (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id)))*/
        //    {
        //        foreach(item in ticketHistory)
        //        {
        //            TicketHistory ticketHistory = new TicketHistory;
        //            ticketHistory.Created = DateTime.Now;
        //            ticketHistory.AuthorId = User.Identity.GetUserId();
        //            db.TicketHistories.Add(ticketHistory);
        //            db.SaveChanges();
        //        }
        //        return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
        //    }

        //    return RedirectToAction("Index", "Tickets");
        //}





        // GET: Comments
        public ActionResult CommentIndex()
        {
            var comments = db.TicketComments.Include(c => c.Author).Include(c => c.Ticket);
            return View(comments.ToList());
        }

        // GET: Comments/Details/5
        public ActionResult CommentDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment comment = db.TicketComments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // GET: Comments/Create
        public ActionResult CommentCreate()
        {
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.PostId = new SelectList(db.Tickets, "Id", "Title");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommentCreate([Bind(Include = "Id,TicketId,Body")] TicketComment ticketComment)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            var ticket = db.Tickets.Find(ticketComment.TicketId);


            TicketHistory ticketHistory = new TicketHistory();

            if (ModelState.IsValid && (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id)))
            {
                ticketComment.Created = DateTime.Now;
                ticketComment.AuthorId = User.Identity.GetUserId();
                db.TicketComments.Add(ticketComment);

                ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                ticketHistory.Created = DateTime.Now;
                ticketHistory.Property = "TICKET COMMENT";
                ticketHistory.NewValue = ticketComment.Body;
                ticketHistory.TicketId = ticket.Id;
                db.TicketHistories.Add(ticketHistory);



                db.SaveChanges();

                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }

            return RedirectToAction("Index", "Tickets");
        }

        

        // GET: Comments/Edit/5
        public ActionResult CommentEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment comment = db.TicketComments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", comment.AuthorId);
            ViewBag.PostId = new SelectList(db.Tickets, "Id", "Title", comment.TicketId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CommentEdit([Bind(Include = "Id,PostId,AuthorId,Body,Created,Updated,UpdateReason")] TicketComment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", comment.AuthorId);
            ViewBag.PostId = new SelectList(db.Tickets, "Id", "Title", comment.TicketId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult CommentDelete(int? id)
        {
            TicketComment comments = db.TicketComments.Find(id);
            TicketHistory ticketHistory = new TicketHistory();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (comments == null)
            {
                return HttpNotFound();
            }
            return View(comments);
        }

        // POST: Comments/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("CommentDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult CommentDeleteConfirmed(int id)
        {
            TicketComment comments = db.TicketComments.Find(id);
            TicketHistory ticketHistory = new TicketHistory();
            ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
            ticketHistory.Created = DateTime.Now;
            ticketHistory.Property = "COMMENT DELETED";
            ticketHistory.TicketId = comments.TicketId;
            db.TicketHistories.Add(ticketHistory);


            db.TicketComments.Remove(comments);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Attachment/Create
        public ActionResult AttachmentCreate()
        {
            return View();
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AttachmentCreate(IEnumerable<HttpPostedFileBase> files, int ticketId)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
             Ticket ticket = db.Tickets.Find(ticketId);
            if (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id))
            {
                foreach (var file in files)
                {
                    TicketAttachment attachment = new TicketAttachment();
                    TicketHistory ticketHistory = new TicketHistory();

                    file.SaveAs(Path.Combine(Server.MapPath("~/TicketAttachments/"), Path.GetFileName(file.FileName)));
                    attachment.FileUrl = file.FileName;

                    attachment.AuthorId = User.Identity.GetUserId();
                    attachment.TicketId = ticketId;
                    attachment.Created = DateTimeOffset.Now;

                    db.TicketAttachments.Add(attachment);

                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "TICKET ATTACHMENT";
                    ticketHistory.NewValue = attachment.FileUrl;
                    ticketHistory.TicketId = ticket.Id;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();
                }
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        [Authorize]
        public ActionResult AttachmentDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }

            return View(ticketAttachment);
    
        }

        [Authorize]
        [HttpPost, ActionName("AttachmentDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult AttachmentDeleteConfirmed(IEnumerable<HttpPostedFileBase> files, int id)
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            Ticket ticket = new Ticket();
            if (User.IsInRole("Admin") || (User.IsInRole("Project Manager") && ticket.Project.Users.Any(u => u.Id == user.Id)) || (User.IsInRole("Developer") && ticket.AssignToUserId == user.Id) || (User.IsInRole("Submitter") && ticket.OwnerUserId == user.Id))
            {

                TicketHistory ticketHistory = new TicketHistory();
                TicketAttachment attachment = db.TicketAttachments.Find(id);


                    

                    db.TicketAttachments.Remove(attachment);

                    ticketHistory.Author = db.Users.Find(User.Identity.GetUserId());
                    ticketHistory.Created = DateTime.Now;
                    ticketHistory.Property = "ATTACHMENT REMOVED";
                    ticketHistory.NewValue = attachment.FileUrl;
                    ticketHistory.TicketId = attachment.TicketId;
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();
                
            }

            return RedirectToAction("Index");
        }






        //                {

        //            var user = db.Users.Find(User.Identity.GetUserId());
        //            if (attachment != null)
        //            {
        //                var ext = Path.GetExtension(attachment.FileName).ToLower();
        //                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp" && ext != ".pdf")
        //                    ModelState.AddModelError("image", "Invalid Format");
        //            }

        //            if (ModelState.IsValid)
        //            {
        //                if (attachment != null)
        //                {

        //                    ticketAttachment.Created = DateTimeOffset.Now;
        //                    ticketAttachment.AuthorId = user.Id;
        //                    var filePath = "~/TicketAttachments/";
        //    var absPath = Server.MapPath("~" + filePath);
        //    ticketAttachment.FileUrl = filePath + attachment.FileName;
        //                    attachment.SaveAs(Path.Combine(absPath, attachment.FileName));

        //                }
        //db.TicketAttachments.Add(ticketAttachment);
        //                db.SaveChanges();
        //            }      
        //            return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
        //        }








    }
}

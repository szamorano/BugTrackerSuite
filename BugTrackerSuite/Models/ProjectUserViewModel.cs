using BugTrackerSuite.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTrackerSuite.Models
{
    public class ProjectUserViewModel
    {
        public Project AssignProject { get; set; }
        public int AssignProjectId { get; set; }
        public MultiSelectList Users { get; set; }
        public string[] SelectedUsers { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTrackerSuite.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public ApplicationUser user { get; set; }
        public AdminUserViewModels roleInfo { get; set; }
        public ProjectUserViewModel projectInfo { get; set; }
    }
}
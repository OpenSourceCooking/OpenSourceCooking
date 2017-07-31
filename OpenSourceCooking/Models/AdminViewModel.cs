using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OpenSourceCooking.Models
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "RoleName")]
        public string Name { get; set; }
    }

    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            RolesList = new List<SelectListItem>();
            GroupsList = new List<SelectListItem>();
        }
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }
        // We will still use this, so leave it here:
        public ICollection<SelectListItem> RolesList { get; }
        // Add a GroupsList Property:
        public ICollection<SelectListItem> GroupsList { get; }
    }

    public class GroupViewModel
    {
        public GroupViewModel()
        {
            UsersList = new List<SelectListItem>();
            RolesList = new List<SelectListItem>();
        }
        [Required(AllowEmptyStrings = false)]
        public string Id { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<SelectListItem> UsersList { get; }
        public ICollection<SelectListItem> RolesList { get; }
    }
}
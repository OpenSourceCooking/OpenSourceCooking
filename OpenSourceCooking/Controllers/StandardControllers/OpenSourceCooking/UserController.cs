using OpenSourceCooking.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace OpenSourceCooking.Controllers.StandardControllers
{
    public class UsersController : Controller
    {
        OpenSourceCookingEntities db = new OpenSourceCookingEntities();

        public UsersController() { }
        public UsersController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }
        ApplicationUserManager _userManager;
        ApplicationGroupManager _groupManager;
        ApplicationRoleManager _roleManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ApplicationGroupManager GroupManager
        {
            get
            {
                return _groupManager ?? new ApplicationGroupManager();
            }
            private set
            {
                _groupManager = value;
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext()
                    .Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            var user = await UserManager.FindByIdAsync(id);
            // Show the groups the user belongs to:
            var userGroups = await this.GroupManager.GetUserGroupsAsync(id);
            ViewBag.GroupNames = userGroups.Select(u => u.Name).ToList();
            return View(user);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            // Show a list of available groups:
            ViewBag.GroupsList = 
                new SelectList(this.GroupManager.Groups, "Id", "Name");
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedGroups)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = userViewModel.Email, 
                    Email = userViewModel.Email 
                };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);
                //Add User to the selected Groups 
                if (adminresult.Succeeded)                
                    if (selectedGroups != null)
                    {
                        selectedGroups = selectedGroups ?? new string[] { };
                        await GroupManager.SetUserGroupsAsync(user.Id, selectedGroups);
                    }
                    return RedirectToAction("Index");                
            }
            ViewBag.Groups = new SelectList(await RoleManager.Roles.ToListAsync(), "Id", "Name");
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)            
                return HttpNotFound();
            // Display a list of available Groups:
            var allGroups = this.GroupManager.Groups;
            var userGroups = await this.GroupManager.GetUserGroupsAsync(id);
            var model = new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email
            };
            foreach (var group in allGroups)
            {
                var listItem = new SelectListItem()
                {
                    Text = group.Name,
                    Value = group.Id,
                    Selected = userGroups.Any(g => g.Id == group.Id)
                };
                model.GroupsList.Add(listItem);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedGroups)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)                
                    return HttpNotFound();
                // Update the User:
                user.UserName = editUser.Email;
                user.Email = editUser.Email;
                await UserManager.UpdateAsync(user);
                // Update the Groups:
                selectedGroups = selectedGroups ?? new string[] { };
                await GroupManager.SetUserGroupsAsync(user.Id, selectedGroups);
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)            
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);            
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)            
                return HttpNotFound();            
            return View(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)                
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                var user = await UserManager.FindByIdAsync(id);
                if (user == null)                
                    return HttpNotFound();
                // Remove all the User Group references:
                await GroupManager.ClearUserGroupsAsync(id);
                // Then Delete the User:
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        [Authorize]
        public async Task<ActionResult> UserSettings()
        {
            string AspNetId = User.Identity.GetUserId();
            ViewBag.Chef = await db.Chefs.Where(x => x.AspNetUserId == AspNetId).Select(x=> new ChefModel {
                AspNetUserName = x.AspNetUser.UserName,
                IsEmailNotificationEnabled = x.IsEmailNotificationEnabled
            }).FirstAsync();
            return View();
        }
    }
}

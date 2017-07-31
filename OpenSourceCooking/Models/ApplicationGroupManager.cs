using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OpenSourceCooking.Models
{
    public class ApplicationGroupManager: IDisposable
    {
        ApplicationGroupStore ApplicationGroupStore;
        ApplicationDbContext ApplicationDbContext;
        ApplicationUserManager ApplicationUserManager;
        ApplicationRoleManager ApplicationRoleManager;

        public ApplicationGroupManager()
        {
            ApplicationDbContext = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
            ApplicationUserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ApplicationRoleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            ApplicationGroupStore = new ApplicationGroupStore(ApplicationDbContext);
        }
        public IQueryable<ApplicationGroup> Groups
        {
            get
            {
                return ApplicationGroupStore.Groups;
            }
        }
        public IdentityResult CreateGroup(ApplicationGroup group)
        {
            ApplicationGroupStore.Create(group);
            return IdentityResult.Success;
        }
        public IdentityResult SetGroupRoles(string groupId, params string[] roleNames)
        {
            // Clear all the roles associated with this group:
            var thisGroup = FindById(groupId);
            thisGroup.ApplicationRoles.Clear();
            ApplicationDbContext.SaveChanges();
            // Add the new roles passed in:
            var newRoles = ApplicationRoleManager.Roles.Where(r => roleNames.Any(n => n == r.Name));
            foreach (var role in newRoles)
                thisGroup.ApplicationRoles.Add(new ApplicationGroupRole { ApplicationGroupId = groupId, ApplicationRoleId = role.Id });
            ApplicationDbContext.SaveChanges();

            // Reset the roles for all affected users:
            foreach (var groupUser in thisGroup.ApplicationUsers)
                RefreshUserGroupRoles(groupUser.ApplicationUserId);
            return IdentityResult.Success;
        }
        public IdentityResult SetUserGroups(string userId, params string[] groupIds)
        {
            if (groupIds == null)
                throw new ArgumentNullException("groupIds");
            // Clear current group membership:
            var currentGroups = GetUserGroups(userId);
            foreach (var group in currentGroups)
                group.ApplicationUsers.Remove(group.ApplicationUsers.FirstOrDefault(gr => gr.ApplicationUserId == userId));
            ApplicationDbContext.SaveChanges();

            // Add the user to the new groups:
            foreach (string groupId in groupIds)
            {
                var newGroup = FindById(groupId);
                newGroup.ApplicationUsers.Add(new ApplicationUserGroup { ApplicationUserId = userId, ApplicationGroupId = groupId });
            }
            ApplicationDbContext.SaveChanges();
            RefreshUserGroupRoles(userId);
            return IdentityResult.Success;
        }
        public IdentityResult RefreshUserGroupRoles(string userId)
        {
            if (userId == null)
                throw new ArgumentNullException("userId");
            // Remove user from previous roles:
            var oldUserRoles = ApplicationUserManager.GetRoles(userId);
            if (oldUserRoles.Count > 0)
                ApplicationUserManager.RemoveFromRoles(userId, oldUserRoles.ToArray());
            // Find teh roles this user is entitled to from group membership:
            var newGroupRoles = GetUserGroupRoles(userId);
            // Get the damn role names:
            var allRoles = ApplicationRoleManager.Roles.ToList();
            var addTheseRoles = allRoles.Where(r => newGroupRoles.Any(gr => gr.ApplicationRoleId == r.Id));
            var roleNames = addTheseRoles.Select(n => n.Name).ToArray();
            // Add the user to the proper roles
            ApplicationUserManager.AddToRoles(userId, roleNames);
            return IdentityResult.Success;
        }
        public IdentityResult DeleteGroup(string groupId)
        {
            if (groupId == null)
                throw new ArgumentNullException("groupId");
            var group = FindById(groupId);    
            var currentGroupMembers = GetGroupUsers(groupId).ToList();
            // remove the roles from the group:
            group.ApplicationRoles.Clear();
            // Remove all the users:
            group.ApplicationUsers.Clear();
            // Remove the group itself:
            ApplicationDbContext.ApplicationGroups.Remove(group);
            ApplicationDbContext.SaveChanges();
            // Reset all the user roles:
            foreach (var user in currentGroupMembers)
                RefreshUserGroupRoles(user.Id);
            return IdentityResult.Success;
        }
        public IdentityResult UpdateGroup(ApplicationGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");
            ApplicationGroupStore.Update(group);
            foreach (var groupUser in group.ApplicationUsers)
                RefreshUserGroupRoles(groupUser.ApplicationUserId);
            return IdentityResult.Success;
        }
        public IdentityResult ClearUserGroups(string userId)
        {
            return SetUserGroups(userId, new string[] { });
        }
        public IEnumerable<ApplicationGroup> GetUserGroups(string userId)
        {
            var userGroups = (from g in Groups
                              where g.ApplicationUsers.Any(u => u.ApplicationUserId == userId)
                              select g).ToList();
            return userGroups;
        }
        public IEnumerable<ApplicationRole> GetGroupRoles(string groupId)
        {
            var grp = ApplicationDbContext.ApplicationGroups.FirstOrDefault(g => g.Id == groupId);
            var roles = ApplicationRoleManager.Roles.ToList();
            var groupRoles = from r in roles
                             where grp.ApplicationRoles.Any(ap => ap.ApplicationRoleId == r.Id)
                             select r;
            return groupRoles;
        }
        public IEnumerable<ApplicationUser> GetGroupUsers(string groupId)
        {
            var group = FindById(groupId);
            var users = new List<ApplicationUser>();
            foreach (var groupUser in group.ApplicationUsers)
            {
                var user = ApplicationDbContext.Users.Find(groupUser.ApplicationUserId);
                users.Add(user);
            }
            return users;
        }
        public IEnumerable<ApplicationGroupRole> GetUserGroupRoles(string userId)
        {
            var userGroups = GetUserGroups(userId);
            var userGroupRoles = new List<ApplicationGroupRole>();
            foreach (var group in userGroups)
                userGroupRoles.AddRange(group.ApplicationRoles.ToArray());
            return userGroupRoles;
        }
        public ApplicationGroup FindById(string id)
        {
            return ApplicationGroupStore.FindById(id);
        }
        public async Task<IdentityResult> CreateGroupAsync(ApplicationGroup group)
        {
            await ApplicationGroupStore.CreateAsync(group);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> SetGroupRolesAsync(string groupId, params string[] roleNames)
        {
            // Clear all the roles associated with this group:
            var thisGroup = await FindByIdAsync(groupId);
            thisGroup.ApplicationRoles.Clear();
            await ApplicationDbContext.SaveChangesAsync();
            // Add the new roles passed in:
            var newRoles = ApplicationRoleManager.Roles.Where(r => roleNames.Any(n => n == r.Name));
            foreach (var role in newRoles)
                thisGroup.ApplicationRoles.Add(new ApplicationGroupRole { ApplicationGroupId = groupId, ApplicationRoleId = role.Id });
            await ApplicationDbContext.SaveChangesAsync();
            // Reset the roles for all affected users:
            foreach (var groupUser in thisGroup.ApplicationUsers)
                await RefreshUserGroupRolesAsync(groupUser.ApplicationUserId);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> SetUserGroupsAsync(string userId, params string[] groupIds)
        {
            // Clear current group membership:
            var currentGroups = await GetUserGroupsAsync(userId);
            foreach (var group in currentGroups)
                group.ApplicationUsers.Remove(group.ApplicationUsers.FirstOrDefault(gr => gr.ApplicationUserId == userId));
            await ApplicationDbContext.SaveChangesAsync();
            // Add the user to the new groups:
            foreach (string groupId in groupIds)
            {
                var newGroup = await FindByIdAsync(groupId);
                newGroup.ApplicationUsers.Add(new ApplicationUserGroup { ApplicationUserId = userId, ApplicationGroupId = groupId });
            }
            await ApplicationDbContext.SaveChangesAsync();
            await RefreshUserGroupRolesAsync(userId);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> RefreshUserGroupRolesAsync(string userId)
        {
            var user = await ApplicationUserManager.FindByIdAsync(userId);
            if (user == null)
                throw new ArgumentNullException("User");
            // Remove user from previous roles:
            var oldUserRoles = await ApplicationUserManager.GetRolesAsync(userId);
            if (oldUserRoles.Count > 0)
                await ApplicationUserManager.RemoveFromRolesAsync(userId, oldUserRoles.ToArray());
            // Find the roles this user is entitled to from group membership:
            var newGroupRoles = await GetUserGroupRolesAsync(userId);
            // Get the damn role names:
            var allRoles = await ApplicationRoleManager.Roles.ToListAsync();
            var addTheseRoles = allRoles.Where(r => newGroupRoles.Any(gr => gr.ApplicationRoleId == r.Id));
            var roleNames = addTheseRoles.Select(n => n.Name).ToArray();
            // Add the user to the proper roles
            await ApplicationUserManager.AddToRolesAsync(userId, roleNames);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> DeleteGroupAsync(string groupId)
        {
            var group = await FindByIdAsync(groupId);
            if (group == null)
                throw new ArgumentNullException("User");
            var currentGroupMembers = (await GetGroupUsersAsync(groupId)).ToList();
            // remove the roles from the group:
            group.ApplicationRoles.Clear();
            // Remove all the users:
            group.ApplicationUsers.Clear();
            // Remove the group itself:
            ApplicationDbContext.ApplicationGroups.Remove(group);
            await ApplicationDbContext.SaveChangesAsync();
            // Reset all the user roles:
            foreach (var user in currentGroupMembers)
                await RefreshUserGroupRolesAsync(user.Id);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> UpdateGroupAsync(ApplicationGroup group)
        {
            await ApplicationGroupStore.UpdateAsync(group);
            foreach (var groupUser in group.ApplicationUsers)
                await RefreshUserGroupRolesAsync(groupUser.ApplicationUserId);
            return IdentityResult.Success;
        }
        public async Task<IdentityResult> ClearUserGroupsAsync(string userId)
        {
            return await SetUserGroupsAsync(userId, new string[] { });
        }
        public async Task<IEnumerable<ApplicationGroup>> GetUserGroupsAsync(string userId)
        {
            var result = new List<ApplicationGroup>();
            var userGroups = (from g in Groups
                              where g.ApplicationUsers.Any(u => u.ApplicationUserId == userId)
                              select g).ToListAsync();
            return await userGroups;
        }
        public async Task<IEnumerable<ApplicationRole>> GetGroupRolesAsync(string groupId)
        {
            var grp = await ApplicationDbContext.ApplicationGroups.FirstOrDefaultAsync(g => g.Id == groupId);
            var roles = await ApplicationRoleManager.Roles.ToListAsync();
            var groupRoles = (from r in roles
                              where grp.ApplicationRoles.Any(ap => ap.ApplicationRoleId == r.Id)
                              select r).ToList();
            return groupRoles;
        }
        public async Task<IEnumerable<ApplicationUser>> GetGroupUsersAsync(string groupId)
        {
            var group = await FindByIdAsync(groupId);
            var users = new List<ApplicationUser>();
            foreach (var groupUser in group.ApplicationUsers)
            {
                var user = await ApplicationDbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == groupUser.ApplicationUserId);
                users.Add(user);
            }
            return users;
        }
        public async Task<IEnumerable<ApplicationGroupRole>> GetUserGroupRolesAsync(string userId)
        {
            var userGroups = await GetUserGroupsAsync(userId);
            var userGroupRoles = new List<ApplicationGroupRole>();
            foreach (var group in userGroups)
                userGroupRoles.AddRange(group.ApplicationRoles.ToArray());
            return userGroupRoles;
        }
        public async Task<ApplicationGroup> FindByIdAsync(string id)
        {
            return await ApplicationGroupStore.FindByIdAsync(id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ApplicationGroupStore.Dispose();
                // dispose managed resources
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace OpenSourceCooking.Models
{
    // You will not likely need to customize there, but it is necessary/easier to create our own  project-specific implementations, so here they are:
    public class ApplicationUserLogOn : IdentityUserLogin<string> { }
    public class ApplicationUserClaim : IdentityUserClaim<string> { }
    public class ApplicationUserRole : IdentityUserRole<string> { }
    // Must be expressed in terms of our custom Role and other types:
    public class ApplicationUser : IdentityUser<string, ApplicationUserLogOn, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
            // Add any custom User properties/code here
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }
    // Must be expressed in terms of our custom UserRole:
    public class ApplicationRole : IdentityRole<string, ApplicationUserRole>
    {
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }
        public ApplicationRole(string name) : this()
        {
            Name = name;
        }
        // Add any custom Role properties/code here
    }
    // Must be expressed in terms of our custom types:
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string, ApplicationUserLogOn, ApplicationUserRole, ApplicationUserClaim>
    {
        public ApplicationDbContext() : base("DefaultConnection") { }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        // Add the ApplicationGroups property:
        public virtual IDbSet<ApplicationGroup> ApplicationGroups { get; set; }
        // Override OnModelsCreating:
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
                throw new ArgumentNullException("modelBuilder");
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationGroup>().HasMany((ApplicationGroup g) => g.ApplicationUsers).WithRequired().HasForeignKey((ApplicationUserGroup ag) => ag.ApplicationGroupId);
            modelBuilder.Entity<ApplicationUserGroup>().HasKey((ApplicationUserGroup r) => new
            {
                ApplicationUserId = r.ApplicationUserId,
                ApplicationGroupId = r.ApplicationGroupId
            }).ToTable("ApplicationUserGroups");
            modelBuilder.Entity<ApplicationGroup>().HasMany((ApplicationGroup g) => g.ApplicationRoles).WithRequired().HasForeignKey((ApplicationGroupRole ap) => ap.ApplicationGroupId);
            modelBuilder.Entity<ApplicationGroupRole>().HasKey((ApplicationGroupRole gr) => new
            {
                ApplicationRoleId = gr.ApplicationRoleId,
                ApplicationGroupId = gr.ApplicationGroupId
            }).ToTable("ApplicationGroupRoles");
        }
    }
    // Most likely won't need to customize these either, but they were needed because we implemented custom versions of all the other types:
    public class ApplicationUserStore : UserStore<ApplicationUser, ApplicationRole, string, ApplicationUserLogOn, ApplicationUserRole, ApplicationUserClaim>, IUserStore<ApplicationUser, string>, IDisposable
    {
        public ApplicationUserStore() : this(new IdentityDbContext())
        {
            DisposeContext = true;
        }
        public ApplicationUserStore(DbContext context) : base(context) { }
    }
    public class ApplicationRoleStore : RoleStore<ApplicationRole, string, ApplicationUserRole>, IQueryableRoleStore<ApplicationRole, string>, IRoleStore<ApplicationRole, string>, IDisposable
    {
        public ApplicationRoleStore() : base(new IdentityDbContext())
        {
            DisposeContext = true;
        }
        public ApplicationRoleStore(DbContext context) : base(context) { }
    }
    public class ApplicationGroup
    {
        public ApplicationGroup()
        {
            Id = Guid.NewGuid().ToString();
            ApplicationRoles = new List<ApplicationGroupRole>();
            ApplicationUsers = new List<ApplicationUserGroup>();
        }
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<ApplicationGroupRole> ApplicationRoles { get; }
        public virtual ICollection<ApplicationUserGroup> ApplicationUsers { get; }
    }
    public class ApplicationUserGroup
    {
        public string ApplicationUserId { get; set; }
        public string ApplicationGroupId { get; set; }
    }
    public class ApplicationGroupRole
    {
        public string ApplicationGroupId { get; set; }
        public string ApplicationRoleId { get; set; }
    }
}
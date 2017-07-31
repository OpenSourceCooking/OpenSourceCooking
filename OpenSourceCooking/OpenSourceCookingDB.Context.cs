﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OpenSourceCooking
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class OpenSourceCookingEntities : DbContext
    {
        public OpenSourceCookingEntities()
            : base("name=OpenSourceCookingEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<MeasurementType> MeasurementTypes { get; set; }
        public virtual DbSet<MeasurementUnit> MeasurementUnits { get; set; }
        public virtual DbSet<RecipeStepsCloudFile> RecipeStepsCloudFiles { get; set; }
        public virtual DbSet<RecipeViewableType> RecipeViewableTypes { get; set; }
        public virtual DbSet<RecipeStep> RecipeSteps { get; set; }
        public virtual DbSet<Flag> Flags { get; set; }
        public virtual DbSet<RecipeFlag> RecipeFlags { get; set; }
        public virtual DbSet<BugReport> BugReports { get; set; }
        public virtual DbSet<RecipeStepsIngredient> RecipeStepsIngredients { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<CommentVote> CommentVotes { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }
        public virtual DbSet<DietaryRestriction> DietaryRestrictions { get; set; }
        public virtual DbSet<Recipe> Recipes { get; set; }
        public virtual DbSet<Ingredient> Ingredients { get; set; }
        public virtual DbSet<CloudFileType> CloudFileTypes { get; set; }
        public virtual DbSet<RecipeCloudFile> RecipeCloudFiles { get; set; }
        public virtual DbSet<CloudFile> CloudFiles { get; set; }
        public virtual DbSet<CloudFilesThumbnail> CloudFilesThumbnails { get; set; }
    }
}

using System;

namespace OpenSourceCooking.Models
{
    public class RecipeFilterModel
    {
        public bool? DisplayId { get; set; }
        public bool? DisplayCompleteDateUtc { get; set; }
        public bool? DisplayCreator { get; set; }
        public bool? DisplayDescription { get; set; }
        public bool? DisplayDietaryRestrictions { get; set; }
        public bool? DisplayImage { get; set; }
        public bool? DisplaySavedByCount { get; set; }
        public bool? DisplayLastEditDateUtc { get; set; }
        public bool? DisplayName { get; set; }
        public bool? DisplayNumberOfSteps { get; set; }
        public bool? DisplayServingSize { get; set; }
        public bool? DisplayTimeToMake { get; set; }

        public bool? Drafts { get; set; }
        public bool? Follower { get; set; }
        public bool? FollowingChefs { get; set; }
        public bool? MyRecipes { get; set; }
        public bool? Public { get; set; }    
        public bool? RecipeOwner { get; set; }
        public bool? Saved { get; set; }
        public bool? Secret { get; set; }
        public bool? SortAscending { get; set; }  
        public bool? ReturnJson { get; set; }

        public int? RecipeId { get; set; }
        public int? RecipesPageIndex { get; set; }

        public string SearchText { get; set; }
        public string SortingBy { get; set; }
    }

}